using UnityEngine;

public class FrontDoorTrigger : MonoBehaviour
{

    [Header("Reference")]
    public TaskManager taskManager;
    public FrontDoorRight FrontDoorRight;
    public FrontDoorLeft FrontDoorLeft;

    

    private void OnTriggerEnter(Collider other)
    {
        if (taskManager != null && taskManager.IsCurrentTask("TryFrontDoor"))
        {
            FrontDoorLeft.DoorTriggered(true);
            FrontDoorRight.DoorTriggered(true);
            taskManager.CompleteTask("TryFrontDoor");
        }
    }
}
