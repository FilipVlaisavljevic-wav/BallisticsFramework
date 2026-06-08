using UnityEngine;
using UnityEngine.InputSystem;

public class BallisticsDemo : MonoBehaviour
{
    public BallisticsManager manager;
    public Transform shooter;
    [Range(-5f, 10f)] public float elevationDegrees = 0f;

    [Header("Projektili (tipke 1/2/3)")]
    public ProjectileData ammoA;   
    public ProjectileData ammoB;   
    public ProjectileData ammoC; 
   private ProjectileData _current;

    void Start() => _current = ammoA;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame && ammoA) _current = ammoA;
        if (kb.digit2Key.wasPressedThisFrame && ammoB) _current = ammoB;
        if (kb.digit3Key.wasPressedThisFrame && ammoC) _current = ammoC;
        if (kb.cKey.wasPressedThisFrame) manager.ClearAll();

        if (kb.spaceKey.wasPressedThisFrame && _current && manager && shooter)
        {
            float rad = elevationDegrees * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(0, Mathf.Sin(rad), Mathf.Cos(rad));
            manager.Fire(_current, shooter.position, dir);
        }
    }
}