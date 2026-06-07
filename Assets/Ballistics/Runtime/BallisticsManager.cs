using UnityEngine;
using System.Collections.Generic;

public class BallisticsManager : MonoBehaviour
{
    [Header("Atmosfera (mijenjaj PRIJE pucanja za usporedbu uvjeta)")]
    public float temperatureCelsius = 15f;
    public float pressurePascal = 101325f;
    public float humidityPercent = 50f;

    [Header("Vjetar")]
    public Vector3 windMetresPerSecond = Vector3.zero;

    [Header("Simulacija")]
    public float timestep = 0.01f;
    public float maxFlightTime = 30f;
    public float killAltitudeY = -50f;

    [Header("Vizualni prikaz")]
    public float bulletRadius = 0.1f;
    public float trailWidth = 0.05f;
    public float impactMarkerRadius = 0.15f;

    private readonly List<LiveProjectile> _active = new();
    private readonly List<GameObject> _persistent = new();   
    public readonly List<ImpactInfo> Impacts = new();

    public struct ImpactInfo
    {
        public string ammo;
        public float rangeM, dropM, speedMs, energyJ, timeS;
        public Vector3 point;
        public bool hitTarget;
    }

    private class LiveProjectile
    {
        public ProjectileState State;
        public ProjectileData Data;
        public AtmosphereModel Atmosphere;  
        public float TimeAlive;
        public Vector3 StartPosition, PrevPosition;
        public GameObject Visual;
        public LineRenderer Trail;
        public List<Vector3> TrailPoints;
    }

    public void Fire(ProjectileData data, Vector3 position, Vector3 direction)
    {
        var atmo = new AtmosphereModel
        {
            TemperatureCelsius = temperatureCelsius,
            PressurePascal = pressurePascal,
            HumidityPercent = humidityPercent,
            Wind = windMetresPerSecond
        };

        Vector3 dir = direction.normalized;
        Vector3 spawn = position + dir * 0.5f;   // ispred cijevi da ne pogodi pucača

        var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = $"Bullet_{data.name}";
        visual.transform.localScale = Vector3.one * bulletRadius;
        visual.transform.position = spawn;
        Destroy(visual.GetComponent<Collider>());
        var rend = visual.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        rend.material.color = data.trailColor;

        var trailObj = new GameObject($"Trail_{data.name}");
        var trail = trailObj.AddComponent<LineRenderer>();
        trail.startWidth = trailWidth; trail.endWidth = trailWidth;
        var trailMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        trailMat.color = data.trailColor;     
        trail.material = trailMat;
        trail.numCapVertices = 2;
        trail.positionCount = 1;
        trail.SetPosition(0, spawn);
        _persistent.Add(trailObj);

        _active.Add(new LiveProjectile
        {
            Data = data,
            Atmosphere = atmo,
            State = new ProjectileState { Position = spawn, Velocity = dir * data.muzzleVelocityMs },
            TimeAlive = 0f,
            StartPosition = spawn,
            PrevPosition = spawn,
            Visual = visual,
            Trail = trail,
            TrailPoints = new List<Vector3> { spawn }
        });

        Debug.Log($"Fire: {data.name}  v0={data.muzzleVelocityMs}m/s  T={temperatureCelsius}\u00b0C  RH={humidityPercent}%");
    }

    void FixedUpdate()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var p = _active[i];
            p.PrevPosition = p.State.Position;
            p.State = BallisticsSolver.Step(p.State, timestep, p.Data, p.Atmosphere);
            p.TimeAlive += timestep;

            Vector3 newPos = p.State.Position;
            Vector3 delta = newPos - p.PrevPosition;
            float dist = delta.magnitude;

            if (dist > 0f && Physics.Raycast(p.PrevPosition, delta / dist, out RaycastHit hit, dist,
                                             ~0, QueryTriggerInteraction.Ignore))
            {
                RegisterImpact(p, hit.point, hit.collider);
                EndProjectile(p, hit.point);
                _active.RemoveAt(i);
                continue;
            }

            p.Visual.transform.position = newPos;
            p.TrailPoints.Add(newPos);
            p.Trail.positionCount = p.TrailPoints.Count;
            p.Trail.SetPosition(p.TrailPoints.Count - 1, newPos);

            if (p.TimeAlive >= maxFlightTime || newPos.y < killAltitudeY)
            {
                EndProjectile(p, newPos);
                _active.RemoveAt(i);
            }
        }
    }

    private void RegisterImpact(LiveProjectile p, Vector3 point, Collider col)
    {
        float range = Vector3.Distance(
            new Vector3(p.StartPosition.x, 0, p.StartPosition.z),
            new Vector3(point.x, 0, point.z));
        float drop = p.StartPosition.y - point.y;
        bool isTarget = col != null && col.name.StartsWith("Target");

        Impacts.Add(new ImpactInfo
        {
            ammo = p.Data.name,
            rangeM = range,
            dropM = drop,
            speedMs = p.State.Speed,
            energyJ = p.State.KineticEnergy(p.Data.massKilograms),
            timeS = p.TimeAlive,
            point = point,
            hitTarget = isTarget
        });

        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = $"Impact_{p.Data.name}";
        marker.transform.localScale = Vector3.one * impactMarkerRadius;
        marker.transform.position = point;
        if (col != null) marker.transform.SetParent(col.transform, true);
        Destroy(marker.GetComponent<Collider>());
        var mr = marker.GetComponent<Renderer>();
        var markerMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        markerMat.color = p.Data.trailColor;   /
        mr.material = markerMat;
        _persistent.Add(marker);

        Debug.Log($"POGODAK [{p.Data.name}] {(isTarget ? col.name : "tlo")} | domet {range:F1} m | pad {drop:F2} m | v {p.State.Speed:F0} m/s | E {p.State.KineticEnergy(p.Data.massKilograms):F0} J | t {p.TimeAlive:F2} s");
    }

    private void EndProjectile(LiveProjectile p, Vector3 endPoint)
    {
        if (p.Visual != null) Destroy(p.Visual);
    }

    public void ClearAll()
    {
        foreach (var p in _active) if (p.Visual != null) Destroy(p.Visual);
        _active.Clear();
        foreach (var go in _persistent) if (go != null) Destroy(go);
        _persistent.Clear();
        Impacts.Clear();
    }

    public bool TryGetLatestProjectile(out Vector3 position, out float speed, out float distance)
    {
        if (_active.Count > 0)
        {
            var p = _active[^1];
            position = p.State.Position; speed = p.State.Speed;
            distance = new Vector3(p.State.Position.x, 0, p.State.Position.z).magnitude;
            return true;
        }
        position = Vector3.zero; speed = 0; distance = 0;
        return false;
    }
}