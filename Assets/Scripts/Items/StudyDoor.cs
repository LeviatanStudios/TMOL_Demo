using UnityEngine;
using UnityEngine.InputSystem;

public class StudyDoor : MonoBehaviour
{
    [Header("References")]
    public TaskManager taskManager;
    public Animator doorAnimator; // Optional: for door animation
    public AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip lockedSound;

    [Header("Settings")]
    public string unlockTaskName = "UnlockStudy";
    public float interactRange = 3f;

    private bool isUnlocked = false;
    private bool isOpen = false;
    private static bool playerHasKey = false; // Static so it persists

    void Start()
    {
        if (taskManager == null)
            taskManager = FindFirstObjectByType<TaskManager>();
    }

    void Update()
    {
        // Check for interaction
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // Check if player is close enough
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > interactRange) return;

        if (!isUnlocked)
        {
            if (playerHasKey)
            {
                UnlockDoor();
            }
            else
            {
                // Door is locked and player has no key
                if (audioSource != null && lockedSound != null)
                    audioSource.PlayOneShot(lockedSound);

                Debug.Log("The door is locked. You need a key.");
            }
        }
        else
        {
            // Door is unlocked, toggle open/close
            ToggleDoor();
        }
    }

    void UnlockDoor()
    {
        isUnlocked = true;

        if (audioSource != null && unlockSound != null)
            audioSource.PlayOneShot(unlockSound);

        // Complete the task
        if (taskManager != null)
        {
            taskManager.CompleteTask(unlockTaskName);
        }

        Debug.Log("Door unlocked!");

        // Automatically open after unlocking
        ToggleDoor();
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;

        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsOpen", isOpen);
        }
        else
        {
            // Simple rotation if no animator
            transform.rotation = Quaternion.Euler(0, isOpen ? 90f : 0f, 0f);
        }
    }

    // Call this when player picks up the key
    public static void GiveKeyToPlayer()
    {
        playerHasKey = true;
        Debug.Log("Player now has the Study Key!");
    }

    public static bool PlayerHasKey => playerHasKey;
}