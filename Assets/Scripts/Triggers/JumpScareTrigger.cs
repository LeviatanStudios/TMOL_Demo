using UnityEngine;
using System.Collections;

public class JumpScareTrigger : MonoBehaviour
{
    [Header("Entity Settings")]
    public GameObject scareEntity;          // The monster/ghost
    public Transform pointA;                // Start position
    public Transform pointB;                // End position
    public float moveSpeed = 15f;
    public float rotationSpeed = 10f;
    public bool faceMovementDirection = true;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;         // Only trigger once?
    public bool disableAfterScare = true;   // Hide entity after reaching point B?
    public float disableDelay = 0.5f;       // Delay before hiding entity

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip scareSound;            // Jump scare sound
    public AudioClip footstepSound;         // Optional walking sound
    public float footstepInterval = 0.4f;

    [Header("Player Effects (Optional)")]
    public bool freezePlayer = false;       // Freeze player during scare?
    public float freezeDuration = 2f;
    public MonoBehaviour playerMovementScript;  // To disable movement

    [Header("Camera Shake (Optional)")]
    public bool enableCameraShake = false;
    public float shakeDuration = 0.3f;
    public float shakeIntensity = 0.2f;

    [Header("Lighting (Optional)")]
    public Light flickerLight;              // Light to flicker during scare
    public float flickerDuration = 1f;

    [Header("Animation (Optional)")]
    public Animator entityAnimator;
    public string walkAnimTrigger = "Walk";
    public string idleAnimTrigger = "Idle";

    private bool hasTriggered = false;
    private bool isMoving = false;
    private Camera playerCamera;

    void Start()
    {
        // Hide entity at start
        if (scareEntity != null)
            scareEntity.SetActive(false);

        // Get player camera for shake
        playerCamera = Camera.main;

        // Auto-setup audio source
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (triggerOnce && hasTriggered) return;
            if (isMoving) return;
            Debug.Log("GOT TRIGGERED");
            hasTriggered = true;
            StartCoroutine(PlayJumpScare());
        }
    }

    IEnumerator PlayJumpScare()
    {
        // Play scare sound
        if (audioSource != null && scareSound != null)
            audioSource.PlayOneShot(scareSound);

        // Freeze player if enabled
        if (freezePlayer && playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Start light flicker
        if (flickerLight != null)
            StartCoroutine(FlickerLight());

        // Camera shake
        if (enableCameraShake && playerCamera != null)
            StartCoroutine(CameraShake());

        // Position entity at point A and show it
        if (scareEntity != null && pointA != null)
        {
            scareEntity.transform.position = pointA.position;
            scareEntity.transform.rotation = pointA.rotation;
            scareEntity.SetActive(true);

            // Start walk animation
            if (entityAnimator != null)
                entityAnimator.SetTrigger(walkAnimTrigger);

            // Move entity to point B
            yield return StartCoroutine(MoveEntityToPointB());
        }

        // Unfreeze player
        if (freezePlayer && playerMovementScript != null)
        {
            yield return new WaitForSeconds(freezeDuration);
            playerMovementScript.enabled = true;
        }
    }

    IEnumerator MoveEntityToPointB()
    {
        if (pointB == null) yield break;

        isMoving = true;
        float footstepTimer = 0f;

        // Face point B
        if (faceMovementDirection)
        {
            Vector3 direction = (pointB.position - pointA.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                scareEntity.transform.rotation = targetRotation;
            }
        }

        while (Vector3.Distance(scareEntity.transform.position, pointB.position) > 0.1f)
        {
            // Move towards point B
            scareEntity.transform.position = Vector3.MoveTowards(
                scareEntity.transform.position,
                pointB.position,
                moveSpeed * Time.deltaTime
            );

            // Optional: Smoothly rotate towards movement direction
            if (faceMovementDirection)
            {
                Vector3 direction = (pointB.position - scareEntity.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    scareEntity.transform.rotation = Quaternion.Slerp(
                        scareEntity.transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
                }
            }

            // Footstep sounds
            if (footstepSound != null && audioSource != null)
            {
                footstepTimer += Time.deltaTime;
                if (footstepTimer >= footstepInterval)
                {
                    audioSource.PlayOneShot(footstepSound, 0.5f);
                    footstepTimer = 0f;
                }
            }

            yield return null;
        }

        isMoving = false;

        // Trigger idle animation
        if (entityAnimator != null)
            entityAnimator.SetTrigger(idleAnimTrigger);

        // Disable entity after delay
        if (disableAfterScare)
        {
            yield return new WaitForSeconds(disableDelay);
            scareEntity.SetActive(false);
        }
    }

    IEnumerator CameraShake()
    {
        Vector3 originalPos = playerCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;

            playerCamera.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.localPosition = originalPos;
    }

    IEnumerator FlickerLight()
    {
        if (flickerLight == null) yield break;

        float elapsed = 0f;
        bool originalState = flickerLight.enabled;

        while (elapsed < flickerDuration)
        {
            flickerLight.enabled = !flickerLight.enabled;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
            elapsed += 0.1f;
        }

        flickerLight.enabled = originalState;
    }

    // Visual helper in editor
    void OnDrawGizmos()
    {
        // Draw trigger zone
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, GetComponent<Collider>()?.bounds.size ?? Vector3.one);

        // Draw point A
        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pointA.position, 0.5f);
            Gizmos.DrawLine(pointA.position, pointA.position + pointA.forward);
        }

        // Draw point B
        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pointB.position, 0.5f);
        }

        // Draw path
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}