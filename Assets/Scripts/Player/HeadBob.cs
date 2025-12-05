using UnityEngine;
using UnityEngine.InputSystem; // IMPORTANT

public class HeadBob : MonoBehaviour
{
    [Header("Bob Settings")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.02f;

    [Header("Smoothing")]
    public float smoothSpeed = 10f;

    private Vector3 startPos;
    private float timer = 0f;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) z += 1;
            if (Keyboard.current.sKey.isPressed) z -= 1;
            if (Keyboard.current.dKey.isPressed) x += 1;
            if (Keyboard.current.aKey.isPressed) x -= 1;
        }

        float move = new Vector3(x, 0, z).magnitude;

        if (move > 0.1f)
            ApplyHeadBob(move);
        else
            ResetHeadBob();
    }

    void ApplyHeadBob(float movement)
    {
        timer += Time.deltaTime * bobSpeed;

        float bobX = Mathf.Cos(timer) * bobAmount * 0.5f;
        float bobY = Mathf.Sin(timer * 2) * bobAmount;

        Vector3 targetPos = startPos + new Vector3(bobX, bobY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothSpeed);
    }

    void ResetHeadBob()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * smoothSpeed);
    }
}
