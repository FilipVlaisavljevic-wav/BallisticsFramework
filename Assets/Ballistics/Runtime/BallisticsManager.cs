using UnityEngine;
using System.Collections.Generic;

public class BallisticsManager : MonoBehaviour
{
    [Header("Atmosfera")]
    public float temperatureCelsius = 15f;
    public float pressurePascal = 101325f;
    public float humidityPercent = 50f;

    [Header("Simulacija")]
    public float timestep = 0.01f;
    public float maxFlightTime = 30f;
    public float killAltitudeY = -500f;

    [Header("Vizualni prikaz")]
    [Tooltip("Prefab projektila — koristit ćemo sferu. Ako je null, kreirat će se automatski.")]
    public GameObject projectilePrefab;
    [Tooltip("Boja trag linije")]
    public Color trailColor = Color.red;

    private AtmosphereModel _atmosphere;
    private List<LiveProjectile> _active = new List<LiveProjectile>();

    private class LiveProjectile
    {
        public ProjectileState State;
        public ProjectileData Data;
        public float TimeAlive;
        public GameObject VisualObject;   // 3D kuglica
        public LineRenderer Trail;          // linija putanje
        public List<Vector3> TrailPoints;    // povijest pozicija
    }

    void Awake()
    {
        _atmosphere = new AtmosphereModel
        {
            TemperatureCelsius = temperatureCelsius,
            PressurePascal = pressurePascal,
            HumidityPercent = humidityPercent
        };
    }

    public void Fire(ProjectileData data, Vector3 position, Vector3 direction)
    {
        // 1. Kreiraj vizualnu kuglicu
        GameObject visual;
        if (projectilePrefab != null)
        {
            visual = Instantiate(projectilePrefab, position, Quaternion.identity);
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.transform.localScale = Vector3.one * 0.1f;
            visual.transform.position = position;
            Destroy(visual.GetComponent<SphereCollider>());

            var renderer = visual.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.red;
        }

        // 2. Kreiraj LineRenderer za trag
        var trailObj = new GameObject($"Trail_{data.name}");
        var trail = trailObj.AddComponent<LineRenderer>();
        trail.startWidth = 0.05f;
        trail.endWidth = 0.05f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = trailColor;
        trail.endColor = trailColor;
        trail.positionCount = 0;

        // 3. Dodaj u listu aktivnih projektila
        var p = new LiveProjectile
        {
            Data = data,
            State = new ProjectileState
            {
                Position = position,
                Velocity = direction.normalized * data.muzzleVelocityMs
            },
            TimeAlive = 0f,
            VisualObject = visual,
            Trail = trail,
            TrailPoints = new List<Vector3> { position }
        };
        _active.Add(p);

        Debug.Log($"Fire: {data.name} v0={data.muzzleVelocityMs}m/s");
    }

    void FixedUpdate()
    {
        for (int i = _active.Count - 1; i >= 0; i--)
        {
            var p = _active[i];
            p.State = BallisticsSolver.Step(p.State, timestep, p.Data, _atmosphere);
            p.TimeAlive += timestep;

            // Ažuriraj vizualnu poziciju kuglice
            p.VisualObject.transform.position = p.State.Position;

            // Ažuriraj trag svakih ~5 koraka da ne radimo previše točaka
            if (Mathf.FloorToInt(p.TimeAlive * 100f) % 5 == 0)
            {
                p.TrailPoints.Add(p.State.Position);
                p.Trail.positionCount = p.TrailPoints.Count;
                p.Trail.SetPositions(p.TrailPoints.ToArray());
            }

            // Uvjeti uklanjanja
            if (p.TimeAlive >= maxFlightTime || p.State.Position.y < killAltitudeY)
            {
                Destroy(p.VisualObject);
                Destroy(p.Trail.gameObject, 5f);  // ostavi trag još 5 sekundi za pregled
                _active.RemoveAt(i);
            }
        }
    }

    /// <summary>Vrati trenutno aktivni projektil (za UI prikaz).</summary>
    public bool TryGetLatestProjectile(out Vector3 position, out float speed, out float distance)
    {
        if (_active.Count > 0)
        {
            var p = _active[_active.Count - 1];
            position = p.State.Position;
            speed = p.State.Speed;
            distance = new Vector3(p.State.Position.x, 0, p.State.Position.z).magnitude;
            return true;
        }
        position = Vector3.zero;
        speed = 0;
        distance = 0;
        return false;
    }
}