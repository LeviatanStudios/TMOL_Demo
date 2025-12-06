using UnityEngine;

public class MenuCamera : MonoBehaviour
{
    [Header("Camera Movement (Optional)")]
    [SerializeField] private bool enableMovement = false;
    [SerializeField] private float rotateSpeed = 0.2f;
    [SerializeField] private float bobSpeed = 0.5f;
    [SerializeField] private float bobAmount = 0.1f;

    [Header("Look At Target (Optional)")]
    [SerializeField] private Transform lookAtTarget;

    private Vector3 startPosition;
    private float timer;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!enableMovement) return;

        timer += Time.deltaTime;

        // Slow rotation
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // Gentle bobbing
        Vector3 newPos = startPosition;
        newPos.y += Mathf.Sin(timer * bobSpeed) * bobAmount;
        transform.position = newPos;

        // Look at target if assigned
        if (lookAtTarget != null)
        {
            transform.LookAt(lookAtTarget);
        }
    }
}