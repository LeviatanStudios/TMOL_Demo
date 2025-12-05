using UnityEngine;

public class LibraryKeyTrigger : MonoBehaviour
{
    [Header("Reference")]
    public TaskManager taskManager;
    private void OnTriggerEnter(Collider other)
    {

        if (taskManager != null && taskManager.IsCurrentTask("EnterLibrary"))
        {
            taskManager.CompleteTask("EnterLibrary");
        }
    }
}
