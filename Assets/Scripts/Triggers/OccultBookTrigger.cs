using UnityEngine;
using TMPro;
using System.Collections;

public class OccultBook : MonoBehaviour
{
    [Header("Book Info")]
    public string bookID;
    public bool isTargetBook = false;

    [Header("Secret Panel UI")]
    [SerializeField] private GameObject secretPanel;
    [SerializeField] private TextMeshProUGUI secretTitleText;
    [SerializeField] private TextMeshProUGUI secretContentText;
    [SerializeField] private float panelDisplayTime = 5f;

    [Header("Secret Message")]
    [SerializeField] private string secretTitle = "The Patriarch's Burden";
    [SerializeField] private string secretContent = "The key is revealed.\nGo find it.";

    [Header("Visual Feedback")]
    public Material revealedMaterial;

    [Header("References")]
    public OffsetFlashlight flashlight;
    public TaskManager taskManager;

    [Header("Key Reveal")]
    public StudyKey keyToReveal;

    private bool isRevealed = false;
    private bool hasBeenRead = false;
    private Renderer bookRenderer;
    private Material originalMaterial;
    private Coroutine hidePanelCoroutine;

    public bool IsRevealed => isRevealed;
    public bool IsTargetBook => isTargetBook;
    public bool HasBeenRead => hasBeenRead;

    void Start()
    {
        bookRenderer = GetComponent<Renderer>();
        if (bookRenderer != null)
            originalMaterial = bookRenderer.material;

        if (secretPanel != null)
            secretPanel.SetActive(false);

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

        if (revealedMaterial != null && bookRenderer != null)
            bookRenderer.material = revealedMaterial;
    }

    void HideBook()
    {
        if (!isRevealed) return;

        isRevealed = false;

        if (originalMaterial != null && bookRenderer != null)
            bookRenderer.material = originalMaterial;
    }

    public bool TryReadBook()
    {
        if (!isRevealed)
        {
            Debug.Log("Cannot read - book is not illuminated!");
            return false;
        }

        // Handle first-time reading
        if (!hasBeenRead)
        {
            hasBeenRead = true;

            if (isTargetBook && taskManager != null)
            {
                taskManager.CompleteTask("SolveBookPuzzle");
                Debug.Log("Found The Patriarch's Burden!");

                // Show secret panel
                ShowSecretPanel();

                // Reveal the hidden key
                if (keyToReveal != null)
                {
                    keyToReveal.SetRevealed(true);
                }
            }
            else if (!string.IsNullOrEmpty(bookID) && taskManager != null)
            {
                taskManager.CompleteTask(bookID);
                Debug.Log($"Completed book task: {bookID}");
            }
        }

        return true;
    }

    void ShowSecretPanel()
    {
        if (secretPanel == null) return;

        if (secretTitleText != null)
            secretTitleText.text = secretTitle;

        if (secretContentText != null)
            secretContentText.text = secretContent;

        secretPanel.SetActive(true);

        if (hidePanelCoroutine != null)
            StopCoroutine(hidePanelCoroutine);

        hidePanelCoroutine = StartCoroutine(HideSecretPanelAfterDelay());
    }

    IEnumerator HideSecretPanelAfterDelay()
    {
        yield return new WaitForSeconds(panelDisplayTime);

        if (secretPanel != null)
            secretPanel.SetActive(false);

        hidePanelCoroutine = null;
    }

    public void HideSecretPanel()
    {
        if (hidePanelCoroutine != null)
        {
            StopCoroutine(hidePanelCoroutine);
            hidePanelCoroutine = null;
        }

        if (secretPanel != null)
            secretPanel.SetActive(false);
    }
}