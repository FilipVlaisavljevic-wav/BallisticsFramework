using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("Reference")]
    public BallisticsManager manager;
    public FirstPersonController controller;
    public Camera cam;
    public Transform gunModel;

    [Header("Municija (1/2/3)")]
    public ProjectileData ammoA;
    public ProjectileData ammoB;
    public ProjectileData ammoC;
    private ProjectileData _current;

    [Header("ADS")]
    public float hipFov = 60f;
    public float adsFov = 18f;                 // zoom kroz scope
    public float adsSpeed = 12f;
    public Vector3 gunHipPos = new Vector3(0.25f, -0.2f, 0.5f);
    public Vector3 gunAdsPos = new Vector3(0f, -0.10f, 0.45f);

    [Header("Pucanje")]
    public float muzzleForward = 1.0f;         // metak kreće ispred kamere

    void Start()
    {
        _current = ammoA;
        if (cam) cam.fieldOfView = hipFov;
    }

    void Update()
    {
        var kb = Keyboard.current;
        var ms = Mouse.current;

        if (kb.digit1Key.wasPressedThisFrame && ammoA) _current = ammoA;
        if (kb.digit2Key.wasPressedThisFrame && ammoB) _current = ammoB;
        if (kb.digit3Key.wasPressedThisFrame && ammoC) _current = ammoC;
        if (kb.cKey.wasPressedThisFrame && manager) manager.ClearAll();

        // ADS dok je desni klik pritisnut
        bool ads = ms.rightButton.isPressed;
        if (controller) controller.isAds = ads;
        if (cam) cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, ads ? adsFov : hipFov, Time.deltaTime * adsSpeed);
        if (gunModel) gunModel.localPosition = Vector3.Lerp(gunModel.localPosition, ads ? gunAdsPos : gunHipPos, Time.deltaTime * adsSpeed);

        // pucanje
        if (ms.leftButton.wasPressedThisFrame && _current && manager && cam)
        {
            Vector3 origin = cam.transform.position + cam.transform.forward * muzzleForward;
            manager.Fire(_current, origin, cam.transform.forward);
        }
    }

    public string CurrentAmmoName => _current ? _current.name : "\u2014";
}