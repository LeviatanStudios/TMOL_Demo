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
    [SerializeField] private float sensitivity = 10f;

    private Vector3 movementInput;
    private float xRot;
    private bool isSprinting;

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        // ===== Movement Input =====
        float h = Keyboard.current.aKey.isPressed ? -1 :
                  Keyboard.current.dKey.isPressed ? 1 : 0;
        float v = Keyboard.current.wKey.isPressed ? 1 :
                  Keyboard.current.sKey.isPressed ? -1 : 0;
        movementInput = new Vector3(h, 0f, v).normalized;

        // ===== Sprint =====
        isSprinting = Keyboard.current.leftShiftKey.isPressed && stamina.CanSprint();

        // ===== Mouse Input =====
        float mouseX = Mouse.current.delta.x.ReadValue() * sensitivity * Time.deltaTime;
        float mouseY = Mouse.current.delta.y.ReadValue() * sensitivity * Time.deltaTime;

        // Apply camera rotation
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        // Determine current speed
        float speed = isSprinting ? sprintSpeed : walkSpeed;

        // Transform local movement to world space
        Vector3 move = transform.TransformDirection(movementInput) * speed;

        // Apply movement while preserving Y velocity
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);

        // Handle stamina
        if (isSprinting && movementInput.magnitude > 0.1f)
            stamina.ConsumeStamina();
        else
            stamina.RegenStamina();

        // Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.25f);
    }
}
