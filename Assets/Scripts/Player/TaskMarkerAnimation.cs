using UnityEngine;

public class TaskMarkerAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    public float rotateSpeed = 50f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Bob up and down
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        // Rotate
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}