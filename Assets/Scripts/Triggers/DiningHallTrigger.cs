using UnityEngine;

public class DiningHallTrigger : MonoBehaviour
{
    [Header("Reference")]
    public TaskManager taskManager;
    private void OnTriggerEnter(Collider other)
    {
        if (taskManager != null && taskManager.IsCurrentTask("GoDiningHall"))
        {
            taskManager.CompleteTask("GoDiningHall");
        }
    }
}
