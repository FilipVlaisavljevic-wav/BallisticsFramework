using UnityEngine;

public class ProjectileHUD : MonoBehaviour
{
    public BallisticsManager manager;
    public ProjectileData currentProjectile;

    private GUIStyle _style;

    void Start()
    {
        _style = new GUIStyle();
        _style.fontSize = 18;
        _style.normal.textColor = Color.white;
    }

    void OnGUI()
    {
        // Pozadina (poluprozirni crni kvadrat)
        GUI.Box(new Rect(10, 10, 280, 130), "");

        if (manager.TryGetLatestProjectile(out Vector3 pos, out float speed, out float distance))
        {
            float energy = currentProjectile != null
                ? 0.5f * currentProjectile.massKilograms * speed * speed
                : 0f;

            GUI.Label(new Rect(20, 15, 280, 25), $"Projektil: {(currentProjectile != null ? currentProjectile.name : "—")}", _style);
            GUI.Label(new Rect(20, 40, 280, 25), $"Udaljenost: {distance:F1} m", _style);
            GUI.Label(new Rect(20, 65, 280, 25), $"Brzina: {speed:F1} m/s ({speed * 3.281f:F0} fps)", _style);
            GUI.Label(new Rect(20, 90, 280, 25), $"Visina: {pos.y:F2} m", _style);
            GUI.Label(new Rect(20, 115, 280, 25), $"Kin. energija: {energy:F0} J", _style);
        }
        else
        {
            GUI.Label(new Rect(20, 15, 280, 25), "Pritisni SPACE za pucanje", _style);
        }
    }
}