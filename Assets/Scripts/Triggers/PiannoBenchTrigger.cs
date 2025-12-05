using UnityEngine;

public class PiannoBenchTrigger : MonoBehaviour
{

    [Header("Reference")]
    public TaskManager taskManager;

    private void OnTriggerEnter(Collider other)
    {
        if (taskManager != null)
        {
            taskManager.CompleteTask("CollectBattery");
        }
    }
}
