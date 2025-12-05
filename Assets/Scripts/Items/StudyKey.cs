using UnityEngine;

public class StudyKey : MonoBehaviour
{
    [Header("References")]
    public TaskManager taskManager;

    [Header("Settings")]
    public string taskToComplete = "GetStudyKey";

    private bool isRevealed = false;
    private GameObject visualObject;

    void Start()
    {
        // Auto-find TaskManager if not assigned
        if (taskManager == null)
            taskManager = FindFirstObjectByType<TaskManager>();

        // Start hidden (will be revealed when book is picked up)
        visualObject = gameObject;
        SetRevealed(false);
    }

    public void SetRevealed(bool revealed)
    {
        isRevealed = revealed;
        visualObject.SetActive(revealed);

        if (revealed)
            Debug.Log("Study Key has been revealed!");
    }

    public bool IsRevealed => isRevealed;
}