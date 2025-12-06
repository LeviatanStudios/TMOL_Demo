using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpScareTrigger : MonoBehaviour
{
    [Header("Entity Settings")]
    public GameObject scareEntity;          // The monster/ghost
    [Tooltip("If empty, pointA and pointB will be used as fallback (in that order).")]
    public List<Transform> points = new List<Transform>(); // flexible list of waypoints
    public float moveSpeed = 15f;
    public float rotationSpeed = 10f;
    public bool faceMovementDirection = true;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;         // Only trigger once?
    public bool disableAfterScare = true;   // Hide entity after reaching last point?
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

    // Backwards compatible single points (optional)
    [Header("Legacy Points (optional, only used if 'points' is empty)")]
    public Transform pointA;
    public Transform pointB;

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

        // If points list empty and legacy points set, populate fallback
        if ((points == null || points.Count == 0) && pointA != null && pointB != null)
        {
            points = new List<Transform> { pointA, pointB };
            // if there is a third legacy point, you can add it manually in inspector
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerOnce && hasTriggered) return;
        if (isMoving) return;

        Debug.Log("JumpScareTrigger: GOT TRIGGERED");
        hasTriggered = true;
        StartCoroutine(PlayJumpScare());
    }

    IEnumerator PlayJumpScare()
    {
        // Play scare sound
        if (audioSource != null && scareSound != null)
            audioSource.PlayOneShot(scareSound);

        // Freeze player if enabled (start unfreeze coroutine so movement can happen separately)
        if (freezePlayer && playerMovementScript != null)
            StartCoroutine(HandlePlayerFreeze());

        // Start light flicker
        if (flickerLight != null)
            StartCoroutine(FlickerLight());

        // Camera shake
        if (enableCameraShake && playerCamera != null)
            StartCoroutine(CameraShake());

        // Position entity at first point and show it
        if (scareEntity != null && points != null && points.Count > 0)
        {
            Transform start = points[0];
            if (start != null)
            {
                scareEntity.transform.position = start.position;
                scareEntity.transform.rotation = start.rotation;
            }
            scareEntity.SetActive(true);

            // Start walk animation
            if (entityAnimator != null && !string.IsNullOrEmpty(walkAnimTrigger))
                entityAnimator.SetTrigger(walkAnimTrigger);

            // Move through all points
            yield return StartCoroutine(MoveEntityThroughPoints());
        }

        // After movement is done, ensure idle animation and optionally disable
        if (entityAnimator != null && !string.IsNullOrEmpty(idleAnimTrigger))
            entityAnimator.SetTrigger(idleAnimTrigger);

        if (disableAfterScare)
        {
            yield return new WaitForSeconds(disableDelay);
            if (scareEntity != null)
                scareEntity.SetActive(false);
        }
    }

    IEnumerator MoveEntityThroughPoints()
    {
        if (points == null || points.Count == 0) yield break;
        if (scareEntity == null) yield break;

        isMoving = true;
        float footstepTimer = 0f;

        // iterate through points sequentially (0..n-1), target is each subsequent point
        for (int i = 1; i < points.Count; i++)
        {
            Transform target = points[i];
            if (target == null) continue;

            // Optional immediate face to next target before moving
            if (faceMovementDirection)
            {
                Vector3 dir = (target.position - scareEntity.transform.position).normalized;
                if (dir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(dir);
                    scareEntity.transform.rotation = targetRotation;
                }
            }

            while (Vector3.Distance(scareEntity.transform.position, target.position) > 0.1f)
            {
                // Move towards target
                scareEntity.transform.position = Vector3.MoveTowards(
                    scareEntity.transform.position,
                    target.position,
                    moveSpeed * Time.deltaTime
                );

                // Smoothly rotate towards movement direction
                if (faceMovementDirection)
                {
                    Vector3 direction = (target.position - scareEntity.transform.position).normalized;
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

            // small pause on each intermediate point (optional, tweak if needed)
            yield return null;
        }

        isMoving = false;
        yield break;
    }

    IEnumerator HandlePlayerFreeze()
    {
        if (playerMovementScript == null) yield break;

        playerMovementScript.enabled = false;
        yield return new WaitForSeconds(freezeDuration);
        playerMovementScript.enabled = true;
    }

    IEnumerator CameraShake()
    {
        if (playerCamera == null) yield break;

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
            // toggle
            flickerLight.enabled = !flickerLight.enabled;
            // choose a random short wait
            float wait = Random.Range(0.05f, 0.15f);
            yield return new WaitForSeconds(wait);
            elapsed += wait;
        }

        flickerLight.enabled = originalState;
    }

    // Visual helper in editor
    void OnDrawGizmos()
    {
        // Draw trigger zone
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Collider c = GetComponent<Collider>();
        if (c != null)
            Gizmos.DrawCube(transform.position, c.bounds.size);
        else
            Gizmos.DrawCube(transform.position, Vector3.one);

        // Draw points list
        if (points != null && points.Count > 0)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == null) continue;
                Gizmos.color = (i == 0) ? Color.green : (i == points.Count - 1 ? Color.red : Color.yellow);
                Gizmos.DrawWireSphere(points[i].position, 0.25f);
                if (i < points.Count - 1 && points[i + 1] != null)
                    Gizmos.DrawLine(points[i].position, points[i + 1].position);
            }
        }
        else
        {
            // fallback: draw legacy A/B points if present
            if (pointA != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(pointA.position, 0.25f);
                Gizmos.DrawLine(pointA.position, pointA.position + pointA.forward);
            }
            if (pointB != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(pointB.position, 0.25f);
            }
            if (pointA != null && pointB != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pointA.position, pointB.position);
            }
        }
    }
}
