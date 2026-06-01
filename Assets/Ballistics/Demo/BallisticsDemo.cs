using UnityEngine;
using UnityEngine.InputSystem;

public class BallisticsDemo : MonoBehaviour
{
    [Tooltip("Drag tvoj ProjectileData ScriptableObject ovdje.")]
    public ProjectileData projectile;

    [Tooltip("Drag tvoj BallisticsManager iz scene ovdje.")]
    public BallisticsManager manager;

    [Tooltip("Drag GameObject 'Shooter' iz scene — odakle leti metak.")]
    public Transform shooter;

    [Tooltip("Visinski kut nagiba pucanja (stupnjevi). 0 = horizontalno.")]
    [Range(-5f, 10f)]
    public float elevationDegrees = 0f;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (projectile == null || manager == null || shooter == null)
            {
                Debug.LogError("Dodijeli projectile, manager i shooter u Inspectoru!");
                return;
            }

            // Smjer s kutem elevacije
            float radians = elevationDegrees * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(0, Mathf.Sin(radians), Mathf.Cos(radians));

            manager.Fire(projectile, shooter.position, direction);
        }
    }
}