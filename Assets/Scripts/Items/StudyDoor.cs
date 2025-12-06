using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class StudyDoor : MonoBehaviour
{
    [Header("References")]
    public TaskManager taskManager;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 8f;
    public float interactRange = 3f;

    [Header("Task Settings")]
    public string unlockTaskName = "UnlockStudy";
    public string requiredTaskID1 = "ReadFinalJournal";
    public string requiredTaskID2 = "UnlockStudy";

    [Header("Audio")]
    [SerializeField] private AudioSource doorOpenAudioSource;
    [SerializeField] private float openDelay = 0f;
    [Space(10)]
    [SerializeField] private AudioSource doorCloseAudioSource;
    [SerializeField] private float closeDelay = 0.3f;
    [Space(10)]
    [SerializeField] private AudioSource lockedAudioSource;

    private bool isUnlocked = false;
    private bool isOpen = false;
    private static bool playerHasKey = false;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;
    private Coroutine _currentCoroutine;

    void Start()
    {
        if (taskManager == null)
            taskManager = FindFirstObjectByType<TaskManager>();

        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > interactRange) return;

        // Check if current task is ReadFinalJournal OR UnlockStudy
        bool canInteract = taskManager.IsCurrentTask(requiredTaskID1) ||
                           taskManager.IsCurrentTask(requiredTaskID2);

        if (!isUnlocked && !canInteract)
        {
            PlayLockedSound();
            Debug.Log("Complete other tasks first.");
            return;
        }

        if (!isUnlocked)
        {
            if (playerHasKey)
            {
                UnlockDoor();
            }
            else
            {
                PlayLockedSound();
                Debug.Log("The door is locked. You need a key.");
            }
        }
        else
        {
            ToggleDoor();
        }
    }

    void PlayLockedSound()
    {
        if (lockedAudioSource != null)
            lockedAudioSource.Play();
    }

    void UnlockDoor()
    {
        isUnlocked = true;

        if (taskManager != null)
        {
            taskManager.CompleteTask(unlockTaskName);
        }

        Debug.Log("Door unlocked!");
        ToggleDoor();
    }

    void ToggleDoor()
    {
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine = StartCoroutine(AnimateDoor());
    }

    private IEnumerator AnimateDoor()
    {
        isOpen = !isOpen;
        Quaternion targetRotation = isOpen ? _openRotation : _closedRotation;

        // Stop any currently playing door audio
        if (doorOpenAudioSource != null) doorOpenAudioSource.Stop();
        if (doorCloseAudioSource != null) doorCloseAudioSource.Stop();

        if (isOpen)
        {
            if (doorOpenAudioSource != null)
                doorOpenAudioSource.PlayDelayed(openDelay);
        }
        else
        {
            if (doorCloseAudioSource != null)
                doorCloseAudioSource.PlayDelayed(closeDelay);
        }

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.rotation = targetRotation;
        _currentCoroutine = null;
    }

    public static void GiveKeyToPlayer()
    {
        playerHasKey = true;
        Debug.Log("Player now has the Study Key!");
    }

    public static bool PlayerHasKey => playerHasKey;
}