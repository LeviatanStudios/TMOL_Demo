using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupAndInspect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform inspectPosition;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject crosshairDot;
    [SerializeField] private MonoBehaviour playerRotationScript;
    [SerializeField] private MonoBehaviour cameraRotationScript;

    [Header("UI References")]
    [SerializeField] private GameObject pickupHintPanel;
    [SerializeField] private GameObject readHintPanel;

    [Header("Task & Item References")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private OffsetFlashlight flashlight;
    [SerializeField] private GameObject tiyanak;

    [Header("Final Journal Camera Animation")]
    [SerializeField] private GameObject finalJournalFocusObject;
    [SerializeField] private float cameraAnimationDuration = 2f;
    [SerializeField] private float cameraReturnDuration = 1.5f;
    [SerializeField] private AnimationCurve cameraAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float delayBeforeReturn = 0.5f;
    [SerializeField] private UnityEngine.Events.UnityEvent onFinalJournalAnimationComplete;
    [SerializeField] private UnityEngine.Events.UnityEvent onCameraReturnComplete;

    [Header("Settings")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private float throwForce = 500f;
    [SerializeField] private float moveSmoothSpeed = 8f;
    [SerializeField] private float inspectRotationSpeed = 20f;
    [SerializeField] private float autoRotateSpeed = 50f;

    private Rigidbody heldRb;
    private GameObject heldObj;
    private Collider[] heldColliders;
    private bool isInspecting = false;
    public bool IsInspecting => isInspecting;

    private bool isReading = false;
    public bool IsReading => isReading;

    // Track what type of reading we're in
    private bool isReadingJournal = false;
    private bool isReadingOccultBook = false;
    private OccultBook currentOccultBook = null;

    private bool canDrop = true;
    private int pickupLayer;
    private Collider[] playerColliders;
    private Quaternion targetRotation;
    private GameObject highlightedObj = null;

    public StudyDoor studyDoor;

    private HashSet<string> readJournals = new HashSet<string>();

    // Final journal camera animation tracking
    private bool justReadFinalJournal = false;
    private bool isCameraAnimating = false;
    public bool IsCameraAnimating => isCameraAnimating;
    private bool hasFinalSequencePlayed = false;

    // Store original camera rotation for return
    private Quaternion originalCameraRotation;
    private Animator focusObjectAnimator;

    void Start()
    {
        pickupLayer = LayerMask.NameToLayer("Pick Up");
        if (pickupLayer == -1) Debug.LogError("Layer 'Pick Up' not found!");

        playerColliders = player.GetComponentsInChildren<Collider>();

        if (flashlight == null)
            flashlight = FindFirstObjectByType<OffsetFlashlight>();

        // Cache the animator if focus object exists
        if (finalJournalFocusObject != null)
            focusObjectAnimator = finalJournalFocusObject.GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairDot != null) crosshairDot.SetActive(true);
        if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
        if (readHintPanel != null) readHintPanel.SetActive(false);
    }

    void Update()
    {
        // Block all interactions during camera animation
        if (isCameraAnimating) return;

        HandlePickupInteraction();
        HandleReadInteraction();
        HandleThrow();
        HandleUIHint();
        HandleCloseReading();
    }

    void FixedUpdate()
    {
        if (isCameraAnimating) return;
        HandleHeldObject();
    }

    #region UI Hint & Highlight
    private void HandleUIHint()
    {
        if (heldObj != null || isReading)
        {
            ClearHighlight();
            return;
        }

        RaycastHit hit;
        GameObject currentHitObj = null;
        bool isReadable = false;
        bool isPickupable = false;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
        {
            string tag = hit.transform.tag;

            if (tag == "Journal" || tag == "OccultBook")
            {
                if (tag == "OccultBook")
                {
                    OccultBook book = hit.transform.GetComponent<OccultBook>();
                    if (book != null && book.IsRevealed)
                    {
                        currentHitObj = hit.transform.gameObject;
                        isReadable = true;
                    }
                }
                else
                {
                    currentHitObj = hit.transform.gameObject;
                    isReadable = true;
                }
            }
            else if (tag == "canPickUp" || tag == "Battery" || tag == "Matches" || tag == "StudyKey")
            {
                currentHitObj = hit.transform.gameObject;
                isPickupable = true;
            }
        }

        // Update UI panels based on what we're looking at
        if (currentHitObj != null)
        {
            if (isReadable)
            {
                // Show "R to Read" hint
                if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
                if (readHintPanel != null) readHintPanel.SetActive(true);
            }
            else if (isPickupable)
            {
                // Show "E to Pick Up" hint
                if (pickupHintPanel != null) pickupHintPanel.SetActive(true);
                if (readHintPanel != null) readHintPanel.SetActive(false);
            }
        }
        else
        {
            // Hide all hints
            if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
            if (readHintPanel != null) readHintPanel.SetActive(false);
        }

        // Handle highlighting
        if (currentHitObj != highlightedObj)
        {
            ClearHighlight();
            if (currentHitObj != null) ApplyHighlight(currentHitObj);
            highlightedObj = currentHitObj;
        }
    }

    private void ApplyHighlight(GameObject obj) { /* TODO */ }
    private void RemoveHighlight(GameObject obj) { /* TODO */ }

    private void ClearHighlight()
    {
        if (highlightedObj != null) RemoveHighlight(highlightedObj);
        highlightedObj = null;
    }
    #endregion

    #region E Key - Pickup/Collect Interaction
    private void HandlePickupInteraction()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (heldObj == null && !isReading)
            {
                RaycastHit hit;
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
                {
                    string tag = hit.transform.tag;

                    switch (tag)
                    {
                        case "Battery":
                            CollectBattery(hit.transform.gameObject);
                            break;

                        case "Matches":
                            CollectMatches(hit.transform.gameObject);
                            break;

                        case "StudyKey":
                            CollectStudyKey(hit.transform.gameObject);
                            break;

                        case "canPickUp":
                            PickUpObject(hit.transform.gameObject);
                            break;
                    }
                }
            }
            else if (canDrop && !isInspecting && !isReading)
            {
                DropObject();
            }
        }
    }
    #endregion

    #region R Key - Read/Inspect Interaction
    private void HandleReadInteraction()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            // Close reading if already reading (R to close)
            if (isReading)
            {
                CloseReading();
                return;
            }

            if (heldObj != null)
            {
                ToggleInspectMode();
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
            {
                string tag = hit.transform.tag;

                if (tag == "Journal")
                {
                    ReadJournal(hit.transform.gameObject);
                }
                else if (tag == "OccultBook")
                {
                    ReadOccultBook(hit.transform.gameObject);
                }
            }
        }

        if (isInspecting && heldObj != null && Keyboard.current.qKey.isPressed)
        {
            targetRotation *= Quaternion.Euler(0, autoRotateSpeed * Time.fixedDeltaTime, 0);
        }
    }

    private void ToggleInspectMode()
    {
        isInspecting = !isInspecting;

        if (isInspecting)
        {
            if (playerRotationScript != null) playerRotationScript.enabled = false;
            if (cameraRotationScript != null) cameraRotationScript.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (crosshairDot != null) crosshairDot.SetActive(false);
            canDrop = false;

            targetRotation = heldRb.rotation;
        }
        else
        {
            if (playerRotationScript != null) playerRotationScript.enabled = true;
            if (cameraRotationScript != null) cameraRotationScript.enabled = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (crosshairDot != null) crosshairDot.SetActive(true);
            canDrop = true;
        }
    }
    #endregion

    #region Close Reading
    private void HandleCloseReading()
    {
        // Close with Escape key
        if (isReading && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseReading();
        }
    }

    private void CloseReading()
    {
        // Hide appropriate UI based on what we were reading
        if (isReadingJournal)
        {
            if (JournalUI.Instance != null)
                JournalUI.Instance.HideJournal();
        }
        else if (isReadingOccultBook)
        {
            // Close the OccultBook UI
            if (currentOccultBook != null)
            {
                currentOccultBook.CloseBook();
            }
        }

        // Reset reading states
        isReading = false;
        isReadingJournal = false;
        isReadingOccultBook = false;
        currentOccultBook = null;

        // Check if we just read the final journal/book - trigger camera animation
        if (justReadFinalJournal && !hasFinalSequencePlayed)
        {
            justReadFinalJournal = false;
            hasFinalSequencePlayed = true;
            StartCoroutine(PlayFinalJournalCameraSequence());
            return; // Don't restore controls yet - animation will handle it
        }

        // Normal close - restore controls
        RestorePlayerControls();
    }

    private void RestorePlayerControls()
    {
        if (playerRotationScript != null) playerRotationScript.enabled = true;
        if (cameraRotationScript != null) cameraRotationScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshairDot != null) crosshairDot.SetActive(true);
    }
    #endregion

    #region Final Journal Camera Animation
    private IEnumerator PlayFinalJournalCameraSequence()
    {

        if (finalJournalFocusObject == null)
        {
            Debug.LogWarning("Final Journal Focus Object not assigned! Skipping camera animation.");
            RestorePlayerControls();
            yield break;
        }

        isCameraAnimating = true;

        // Keep player controls disabled during animation
        if (playerRotationScript != null) playerRotationScript.enabled = false;
        if (cameraRotationScript != null) cameraRotationScript.enabled = false;

        // Hide cursor and crosshair during animation
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshairDot != null) crosshairDot.SetActive(false);

        // Store original camera rotation for return
        originalCameraRotation = playerCamera.transform.rotation;

        // Calculate target rotation to look at the object
        Vector3 directionToTarget = (finalJournalFocusObject.transform.position - playerCamera.transform.position).normalized;
        Quaternion targetLookRotation = Quaternion.LookRotation(directionToTarget);

        // Slam the door ONCE before camera animation starts
        if (studyDoor != null)
        {
            studyDoor.DoorSlammed();
        }
        tiyanak.SetActive(true);
        // === PHASE 1: Camera moves to look at object ===
        float elapsedTime = 0f;
        while (elapsedTime < cameraAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cameraAnimationDuration;
            float curveValue = cameraAnimationCurve.Evaluate(t);

            playerCamera.transform.rotation = Quaternion.Slerp(originalCameraRotation, targetLookRotation, curveValue);
            yield return null;
        }
        playerCamera.transform.rotation = targetLookRotation;

        Debug.Log("Camera focused on object. Starting object animation...");

        // === PHASE 2: Play object animation ===
        if (focusObjectAnimator != null)
        {
            // Trigger the animation
            focusObjectAnimator.SetTrigger("Play");

            // Wait for animation to finish
            yield return new WaitForSeconds(0.1f); // Small delay to let animation start

            // Get the current animation clip length
            AnimatorStateInfo stateInfo = focusObjectAnimator.GetCurrentAnimatorStateInfo(0);
            float animationLength = stateInfo.length;

            Debug.Log($"Playing object animation for {animationLength} seconds...");
            yield return new WaitForSeconds(animationLength);
        }
        else
        {
            Debug.LogWarning("No Animator found on focus object. Skipping animation wait.");
            yield return new WaitForSeconds(1f); // Default wait if no animator
        }

        // Fire event after object animation completes
        onFinalJournalAnimationComplete?.Invoke();

        // === PHASE 3: Disable the object ===
        finalJournalFocusObject.SetActive(false);
        Debug.Log("Focus object disabled.");

        // Small delay before camera returns
        yield return new WaitForSeconds(delayBeforeReturn);

        // === PHASE 4: Camera returns to original position ===
        Quaternion currentRotation = playerCamera.transform.rotation;
        elapsedTime = 0f;

        while (elapsedTime < cameraReturnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / cameraReturnDuration;
            float curveValue = cameraAnimationCurve.Evaluate(t);

            playerCamera.transform.rotation = Quaternion.Slerp(currentRotation, originalCameraRotation, curveValue);
            yield return null;
        }
        playerCamera.transform.rotation = originalCameraRotation;
        tiyanak.SetActive(false);
        Debug.Log("Camera returned to original position.");

        isCameraAnimating = false;

        // Fire return complete event
        onCameraReturnComplete?.Invoke();

        // Restore player controls
        RestorePlayerControls();
    }

    /// <summary>
    /// Call this to manually trigger the focus object animation (for testing)
    /// </summary>
    public void TriggerFinalJournalSequence()
    {
        if (!hasFinalSequencePlayed)
        {
            hasFinalSequencePlayed = true;
            StartCoroutine(PlayFinalJournalCameraSequence());
        }
    }

    /// <summary>
    /// Reset the final sequence so it can play again (for testing)
    /// </summary>
    public void ResetFinalSequence()
    {
        hasFinalSequencePlayed = false;
        justReadFinalJournal = false;
    }
    #endregion

    #region Reading Methods
    private void ReadJournal(GameObject journalObj)
    {
        JournalPickup journal = journalObj.GetComponent<JournalPickup>();
        if (journal == null || JournalUI.Instance == null) return;

        // Check if this is first-time read and task is required
        bool isFirstRead = !readJournals.Contains(journal.journalID);

        // Only block if it's the first read AND task can't be completed yet
        if (isFirstRead && !string.IsNullOrEmpty(journal.journalID))
        {
            if (!taskManager.CanCompleteTask(journal.journalID))
            {
                Debug.Log($"Cannot read {journal.journalTitle} yet - complete current task first!");
                return;
            }
        }

        // Check if this is the final journal - flag for camera animation
        if (journal.journalID == "ReadFinalJournal")
        {
            justReadFinalJournal = true;
        }

        // Enter reading mode
        EnterReadingMode();
        isReadingJournal = true;
        isReadingOccultBook = false;

        JournalUI.Instance.ShowJournal(
            journal.journalTitle,
            journal.journalContent,
            journal.journalImage
        );

        // Complete task only on first read
        if (isFirstRead && !string.IsNullOrEmpty(journal.journalID))
        {
            readJournals.Add(journal.journalID);
            taskManager?.CompleteTask(journal.journalID);
            Debug.Log($"Completed journal task: {journal.journalID}");
        }

        Debug.Log($"Reading journal: {journal.journalTitle}");
    }

    private void ReadOccultBook(GameObject bookObj)
    {
        OccultBook occultBook = bookObj.GetComponent<OccultBook>();
        if (occultBook == null) return;

        if (!occultBook.IsRevealed)
        {
            Debug.Log("This book's title is hidden... Try using your flashlight.");
            return;
        }

        // Only block first read if task can't be completed
        if (!occultBook.HasBeenRead && !string.IsNullOrEmpty(occultBook.bookID))
        {
            if (!taskManager.CanCompleteTask(occultBook.bookID))
            {
                Debug.Log("Cannot read this book yet - complete current task first!");
                return;
            }
        }

        // Check if this occult book should trigger final sequence
        // You can change "ReadFinalOccultBook" to whatever bookID you want
        if (occultBook.bookID == "ReadFinalJournal" || occultBook.bookID == "ReadFinalOccultBook")
        {
            justReadFinalJournal = true;
        }

        // Enter reading mode
        EnterReadingMode();
        isReadingJournal = false;
        isReadingOccultBook = true;
        currentOccultBook = occultBook;

        // Show the book content
        occultBook.TryReadBook();
        Debug.Log("Reading occult book...");
    }

    private void EnterReadingMode()
    {
        isReading = true;

        if (playerRotationScript != null) playerRotationScript.enabled = false;
        if (cameraRotationScript != null) cameraRotationScript.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (crosshairDot != null) crosshairDot.SetActive(false);
    }
    #endregion

    #region Collection Methods
    private void CollectBattery(GameObject batteryObj)
    {
        BatteryPickup batteryPickup = batteryObj.GetComponent<BatteryPickup>();

        string taskID = batteryPickup?.taskID;
        if (!string.IsNullOrEmpty(taskID) && !taskManager.CanCompleteTask(taskID))
        {
            Debug.Log("Can't collect this yet - complete current task first!");
            return;
        }

        int amount = batteryPickup != null ? batteryPickup.batteryAmount : 1;

        if (flashlight != null)
            flashlight.AddBattery(amount);

        if (!string.IsNullOrEmpty(taskID))
        {
            taskManager?.CompleteTask(taskID);
        }

        Destroy(batteryObj);
        Debug.Log($"Battery collected! (+{amount})");
    }

    private void CollectMatches(GameObject matchesObj)
    {
        MatchesPickup matchesPickup = matchesObj.GetComponent<MatchesPickup>();

        string taskID = matchesPickup?.taskID;
        if (!string.IsNullOrEmpty(taskID) && !taskManager.CanCompleteTask(taskID))
        {
            Debug.Log("Can't collect this yet - complete current task first!");
            return;
        }

        if (!string.IsNullOrEmpty(taskID))
        {
            taskManager?.CompleteTask(taskID);
        }

        Destroy(matchesObj);
        Debug.Log("Matches collected!");
    }

    private void CollectStudyKey(GameObject keyObj)
    {
        StudyKey studyKey = keyObj.GetComponent<StudyKey>();

        if (studyKey == null || !studyKey.IsRevealed)
        {
            Debug.Log("Key is not revealed yet!");
            return;
        }

        if (!taskManager.CanCompleteTask("GetStudyKey"))
        {
            Debug.Log("Can't collect this yet - complete current task first!");
            return;
        }

        StudyDoor.GiveKeyToPlayer();
        taskManager?.CompleteTask("GetStudyKey");

        Destroy(keyObj);
        Debug.Log("Picked up the Study Key!");
    }

    private void PickUpObject(GameObject pickObj)
    {
        if (!pickObj.TryGetComponent(out Rigidbody rb)) return;

        TaskItem taskItem = pickObj.GetComponent<TaskItem>();
        if (taskItem != null && !string.IsNullOrEmpty(taskItem.taskID))
        {
            if (!taskManager.CanCompleteTask(taskItem.taskID))
            {
                Debug.Log("Can't pick this up yet - complete current task first!");
                return;
            }
        }

        heldObj = pickObj;
        heldRb = rb;
        heldColliders = pickObj.GetComponentsInChildren<Collider>();

        heldRb.isKinematic = true;
        heldObj.transform.SetParent(null);
        heldObj.layer = pickupLayer;

        foreach (var pCol in playerColliders)
            foreach (var oCol in heldColliders)
                Physics.IgnoreCollision(pCol, oCol, true);

        if (taskItem != null && !string.IsNullOrEmpty(taskItem.taskID))
        {
            taskManager?.CompleteTask(taskItem.taskID);
        }
    }

    private void DropObject()
    {
        if (heldObj == null) return;

        foreach (var pCol in playerColliders)
            foreach (var oCol in heldColliders)
                Physics.IgnoreCollision(pCol, oCol, false);

        heldObj.layer = 0;
        heldRb.isKinematic = false;

        heldObj = null;
        heldRb = null;
        heldColliders = null;
    }

    private void HandleThrow()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && heldObj != null && canDrop && !isInspecting)
        {
            foreach (var pCol in playerColliders)
                foreach (var oCol in heldColliders)
                    Physics.IgnoreCollision(pCol, oCol, false);

            heldObj.layer = 0;
            heldRb.isKinematic = false;
            heldRb.AddForce(playerCamera.transform.forward * throwForce);

            heldObj = null;
            heldRb = null;
            heldColliders = null;
        }
    }
    #endregion

    #region Held Object Movement
    private void HandleHeldObject()
    {
        if (heldObj == null || heldRb == null) return;

        Vector3 targetPos = isInspecting ? inspectPosition.position : holdPosition.position;
        Quaternion targetRot = isInspecting ? targetRotation : holdPosition.rotation;

        heldRb.MovePosition(Vector3.Lerp(heldRb.position, targetPos, Time.fixedDeltaTime * moveSmoothSpeed));
        heldRb.MoveRotation(Quaternion.Slerp(heldRb.rotation, targetRot, Time.fixedDeltaTime * inspectRotationSpeed));
    }
    #endregion
}