using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private StaminaSystem stamina;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Camera Settings")]
    [SerializeField] private float defaultSensitivity = 10f;

    private Vector3 movementInput;
    private float xRot;
    private bool isSprinting;

    // Freeze system
    private bool isFrozen = true;
    public bool IsFrozen => isFrozen;

    // Current sensitivity
    private float sensitivity;

    private void Start()
    {
        LoadSensitivity();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LoadSensitivity()
    {
        sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // Update sensitivity from Settings (in case it changed)
        sensitivity = Settings.MouseSensitivity;

        // Still allow looking around when frozen
        HandleMouseLook();

        // Block movement input if frozen
        if (isFrozen)
        {
            movementInput = Vector3.zero;
            return;
        }

        // Movement Input
        float h = Keyboard.current.aKey.isPressed ? -1 :
                  Keyboard.current.dKey.isPressed ? 1 : 0;
        float v = Keyboard.current.wKey.isPressed ? 1 :
                  Keyboard.current.sKey.isPressed ? -1 : 0;

        movementInput = new Vector3(h, 0f, v).normalized;

        // Sprint
        isSprinting = Keyboard.current.leftShiftKey.isPressed && stamina.CanSprint();
    }

    private void HandleMouseLook()
    {
        float mouseX = Mouse.current.delta.x.ReadValue() * sensitivity * Time.deltaTime;
        float mouseY = Mouse.current.delta.y.ReadValue() * sensitivity * Time.deltaTime;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void FixedUpdate()
    {
        if (!isFrozen)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.TransformDirection(movementInput) * speed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        if (isSprinting && movementInput.magnitude > 0.1f)
            stamina.ConsumeStamina();
        else
            stamina.RegenStamina();

        if (Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.25f);
    }

    public void UnfreezePlayer()
    {
        isFrozen = false;
        Debug.Log("Player unfrozen!");
    }

    public void FreezePlayer()
    {
        isFrozen = true;
        movementInput = Vector3.zero;
        Debug.Log("Player frozen!");
    }

    // Call this when settings change (optional - for immediate update)
    public void RefreshSensitivity()
    {
        sensitivity = Settings.MouseSensitivity;
    }
}