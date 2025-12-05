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

    [Header("Settings")]
    [SerializeField] private float pickupRange = 20f;
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

    private bool canDrop = true;
    private int pickupLayer;
    private Collider[] playerColliders;
    private Quaternion targetRotation;
    private GameObject highlightedObj = null;

    // Journal tracking
    private HashSet<string> readJournals = new HashSet<string>();

    void Start()
    {
        pickupLayer = LayerMask.NameToLayer("Pick Up");
        if (pickupLayer == -1) Debug.LogError("Layer 'Pick Up' not found!");

        playerColliders = player.GetComponentsInChildren<Collider>();

        if (flashlight == null)
            flashlight = FindFirstObjectByType<OffsetFlashlight>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (crosshairDot != null) crosshairDot.SetActive(true);
        if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
        if (readHintPanel != null) readHintPanel.SetActive(false);
    }

    void Update()
    {
        HandlePickupInteraction();  // E key - pickup/collect
        HandleReadInteraction();     // R key - read/inspect
        HandleThrow();
        HandleUIHint();
        HandleCloseReading();
    }

    void FixedUpdate()
    {
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

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
        {
            string tag = hit.transform.tag;

            // Check if it's a readable item (journal or occult book)
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
            // Check if it's a pickupable item
            else if (tag == "canPickUp" || tag == "Battery" || tag == "Matches" || tag == "StudyKey")
            {
                currentHitObj = hit.transform.gameObject;
                isReadable = false;
            }
        }

        // Update hints
        if (currentHitObj != null)
        {
            if (isReadable)
            {
                if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
                if (readHintPanel != null) readHintPanel.SetActive(true);
            }
            else
            {
                if (pickupHintPanel != null) pickupHintPanel.SetActive(true);
                if (readHintPanel != null) readHintPanel.SetActive(false);
            }
        }
        else
        {
            if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
            if (readHintPanel != null) readHintPanel.SetActive(false);
        }

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
        if (pickupHintPanel != null) pickupHintPanel.SetActive(false);
        if (readHintPanel != null) readHintPanel.SetActive(false);
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
            // If reading, close it
            if (isReading)
            {
                CloseReading();
                return;
            }

            // If holding object, toggle inspect mode
            if (heldObj != null)
            {
                ToggleInspectMode();
                return;
            }

            // If not holding anything, try to read
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

        // Q key for auto-rotate while inspecting
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
        if (isReading && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseReading();
        }
    }

    private void CloseReading()
    {
        isReading = false;

        if (playerRotationScript != null) playerRotationScript.enabled = true;
        if (cameraRotationScript != null) cameraRotationScript.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshairDot != null) crosshairDot.SetActive(true);

        if (JournalUI.Instance != null)
            JournalUI.Instance.HideJournal();
    }
    #endregion

    #region Reading Methods
    private void ReadJournal(GameObject journalObj)
    {
        JournalPickup journal = journalObj.GetComponent<JournalPickup>();
        if (journal == null || JournalUI.Instance == null) return;

        // Check if this journal has a required task and if it's current
        if (!string.IsNullOrEmpty(journal.journalID))
        {
            // Only complete task if it's the current one
            if (!taskManager.CanCompleteTask(journal.journalID))
            {
                Debug.Log($"Cannot read {journal.journalTitle} yet - complete current task first!");
                return; // Block reading if not the right task
            }
        }

        EnterReadingMode();

        JournalUI.Instance.ShowJournal(
            journal.journalTitle,
            journal.journalContent,
            journal.journalImage
        );

        // Complete task on first read
        if (!string.IsNullOrEmpty(journal.journalID) && !readJournals.Contains(journal.journalID))
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

        // Check if this book's task is current
        if (!string.IsNullOrEmpty(occultBook.bookID))
        {
            if (!taskManager.CanCompleteTask(occultBook.bookID))
            {
                Debug.Log($"Cannot read {occultBook.bookTitle} yet - complete current task first!");
                return;
            }
        }

        EnterReadingMode();
        occultBook.TryReadBook();

        Debug.Log($"Reading occult book: {occultBook.bookTitle}");
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

    #region Collection Methods (with task protection)
    private void CollectBattery(GameObject batteryObj)
    {
        BatteryPickup batteryPickup = batteryObj.GetComponent<BatteryPickup>();

        // Check if battery collection is the current task
        string taskID = batteryPickup?.taskID;
        if (!string.IsNullOrEmpty(taskID) && !taskManager.CanCompleteTask(taskID))
        {
            Debug.Log("Can't collect this yet - complete current task first!");
            return;
        }

        int amount = batteryPickup != null ? batteryPickup.batteryAmount : 1;

        if (flashlight != null)
            flashlight.AddBattery(amount);

        // Complete task if it's the current one
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

        // Check task protection
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

        // Check if this pickup has a required task
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

        // Complete task if assigned
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