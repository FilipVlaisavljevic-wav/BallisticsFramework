using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Kretanje")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float gravity = -9.81f;

    [Header("Pogled")]
    public Transform cameraTransform;
    public float mouseSensitivity = 0.08f;
    public float adsSensitivityMultiplier = 0.4f;

    private CharacterController _cc;
    private float _pitch;
    private Vector3 _velocity;
    [HideInInspector] public bool isAds;

    void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Look()
    {
        Vector2 d = Mouse.current.delta.ReadValue() * mouseSensitivity * (isAds ? adsSensitivityMultiplier : 1f);
        transform.Rotate(Vector3.up, d.x);                       // yaw na igraču
        _pitch = Mathf.Clamp(_pitch - d.y, -89f, 89f);
        cameraTransform.localEulerAngles = new Vector3(_pitch, 0f, 0f); // pitch na kameri
    }

    void Move()
    {
        var kb = Keyboard.current;
        float x = (kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0);
        float z = (kb.wKey.isPressed ? 1 : 0) - (kb.sKey.isPressed ? 1 : 0);
        float speed = kb.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        _cc.Move(move * speed * Time.deltaTime);

        if (_cc.isGrounded && _velocity.y < 0) _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }
}