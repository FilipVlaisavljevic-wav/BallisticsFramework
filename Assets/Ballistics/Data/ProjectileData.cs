using UnityEngine;

public enum DragModelType { G1, G7 }

[CreateAssetMenu(fileName = "NewProjectile", menuName = "Ballistics/Projectile Data")]
public class ProjectileData : ScriptableObject
{
    [Header("Physical properties")]
    public float massKilograms = 0.004f;
    public float diameterMetres = 0.00556f;
    public float muzzleVelocityMs = 940f;

    [Header("Drag model")]
    public DragModelType dragModel = DragModelType.G7;
    public float ballisticCoefficient = 0.151f;

    public float CrossSectionArea
    {
        get
        {
            float r = diameterMetres / 2f;
            return Mathf.PI * r * r;
        }
    }
}