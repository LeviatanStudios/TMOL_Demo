using UnityEngine;
using TMPro;

public class OccultBook : MonoBehaviour
{
    [Header("Book Info")]
    public string bookTitle = "Unknown Tome";
    public string bookID; // For task completion
    public bool isTargetBook = false; // Set true for "The Patriarch's Burden"

    [Header("Book Content")]
    [TextArea(5, 15)]
    public string bookContent; // The text content of the book
    public Sprite bookImage; // Optional image for the book

    [Header("Visual Feedback")]
    public GameObject hiddenTitleUI; // World-space canvas with title
    public TextMeshProUGUI titleText;
    public Material revealedMaterial; // Optional: glowing material when revealed

    [Header("References")]
    public OffsetFlashlight flashlight;
    public TaskManager taskManager;

    [Header("Key Reveal")]
    public StudyKey keyToReveal; // Assign the Study Key object here

    private bool isRevealed = false;
    private bool hasBeenRead = false;
    private Renderer bookRenderer;
    private Material originalMaterial;

    // Public accessors
    public bool IsRevealed => isRevealed;
    public bool IsTargetBook => isTargetBook;
    public bool HasBeenRead => hasBeenRead;

    void Start()
    {
        bookRenderer = GetComponent<Renderer>();
        if (bookRenderer != null)
            originalMaterial = bookRenderer.material;

        if (hiddenTitleUI != null)
            hiddenTitleUI.SetActive(false);

        // Auto-find references if not assigned
        if (flashlight == null)
            flashlight = FindFirstObjectByType<OffsetFlashlight>();
        if (taskManager == null)
            taskManager = FindFirstObjectByType<TaskManager>();
    }

    void Update()
    {
        CheckFlashlightIllumination();
    }

    void CheckFlashlightIllumination()
    {
        if (flashlight == null || flashlight.Flashlight == null) return;

        bool flashlightOn = flashlight.Flashlight.enabled;

        if (flashlightOn)
        {
            Vector3 toBook = transform.position - flashlight.transform.position;
            float angle = Vector3.Angle(flashlight.transform.forward, toBook);
            float distance = toBook.magnitude;

            float spotAngle = flashlight.Flashlight.spotAngle / 2f;
            float range = flashlight.Flashlight.range;

            if (angle <= spotAngle && distance <= range)
            {
                if (Physics.Raycast(flashlight.transform.position, toBook.normalized, out RaycastHit hit, distance))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        RevealBook();
                        return;
                    }
                }
            }
        }

        HideBook();
    }

    void RevealBook()
    {
        if (isRevealed) return;

        isRevealed = true;

        if (hiddenTitleUI != null)
        {
            hiddenTitleUI.SetActive(true);
            if (titleText != null)
                titleText.text = bookTitle;
        }

        if (revealedMaterial != null && bookRenderer != null)
            bookRenderer.material = revealedMaterial;

        Debug.Log($"Book revealed: {bookTitle}");
    }

    void HideBook()
    {
        if (!isRevealed) return;

        isRevealed = false;

        if (hiddenTitleUI != null)
            hiddenTitleUI.SetActive(false);

        if (originalMaterial != null && bookRenderer != null)
            bookRenderer.material = originalMaterial;
    }

    /// <summary>
    /// Called when player presses E on the book while it's revealed.
    /// Shows the book content UI.
    /// </summary>
    public bool TryReadBook()
    {
        if (!isRevealed)
        {
            Debug.Log("Cannot read - book is not illuminated!");
            return false;
        }

        // Show book content in UI
        if (JournalUI.Instance != null)
        {
            JournalUI.Instance.ShowJournal(bookTitle, bookContent, bookImage);
        }

        // Handle first-time reading
        if (!hasBeenRead)
        {
            hasBeenRead = true;

            // Complete task if this is the target book
            if (isTargetBook && taskManager != null)
            {
                taskManager.CompleteTask("SolveBookPuzzle");
                Debug.Log("Found The Patriarch's Burden!");

                // Reveal the hidden key!
                if (keyToReveal != null)
                {
                    keyToReveal.SetRevealed(true);
                }
            }
            // Complete task by bookID for non-target books
            else if (!string.IsNullOrEmpty(bookID) && taskManager != null)
            {
                taskManager.CompleteTask(bookID);
                Debug.Log($"Completed book task: {bookID}");
            }
        }

        return true;
    }
}