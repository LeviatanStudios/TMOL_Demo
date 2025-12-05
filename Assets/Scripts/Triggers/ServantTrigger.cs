using UnityEngine;

public class ServantTrigger : MonoBehaviour
{
    [Header("Reference")]
    public TaskManager taskManager;
    private void OnTriggerEnter(Collider other)
    {
        if (taskManager != null && taskManager.IsCurrentTask("ExploreServant"))
        {
            taskManager.CompleteTask("ExploreServant");
        }
    }
}
