using Unity.VisualScripting;
using UnityEngine;

public class NotesScript : MonoBehaviour
{
    [Header("Reference")]
    public TaskManager taskManager;

    private void CollectNotes(GameObject notes)
    {
        if (taskManager != null && taskManager.IsCurrentTask("PickJournal"))
        {
            taskManager.CompleteTask("PickJournal");
        }
    }
}
