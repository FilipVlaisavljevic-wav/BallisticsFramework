using UnityEngine;

public class ReticleHUD : MonoBehaviour
{
    [Header("Reference")]
    public WeaponController weapon;
    public FirstPersonController controller;   

    [Header("Crosshair")]
    public Color color = new Color(1f, 1f, 1f, 0.9f);
    public float length = 10f;    
    public float thickness = 2f;   
    public float gap = 6f;        
    public bool centerDot = true;
    public float dotSize = 2f;

    private Texture2D _tex;

    void Awake()
    {
        _tex = new Texture2D(1, 1);
        _tex.SetPixel(0, 0, Color.white);
        _tex.Apply();
    }

    void OnGUI()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        bool ads = controller != null && controller.isAds;


            GUI.color = color;
            DrawBox(cx - thickness * 0.5f, cy - gap - length, thickness, length); // gore
            DrawBox(cx - thickness * 0.5f, cy + gap, thickness, length); // dolje
            DrawBox(cx - gap - length, cy - thickness * 0.5f, length, thickness); // lijevo
            DrawBox(cx + gap, cy - thickness * 0.5f, length, thickness); // desno
            if (centerDot)
                DrawBox(cx - dotSize * 0.5f, cy - dotSize * 0.5f, dotSize, dotSize);
            GUI.color = Color.white;

        if (weapon != null)
            GUI.Label(new Rect(20, Screen.height - 28, 380, 25),
                $"Municija: {weapon.CurrentAmmoName}   [1/2/3 zamjena · desni klik ADS · C očisti]");
    }

    void DrawBox(float x, float y, float w, float h)
        => GUI.DrawTexture(new Rect(x, y, w, h), _tex);
}