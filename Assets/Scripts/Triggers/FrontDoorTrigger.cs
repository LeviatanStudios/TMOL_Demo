using UnityEngine;

public class FrontDoorTrigger : MonoBehaviour
{

    [Header("Reference")]
    public TaskManager taskManager;
    public FrontDoorRight FrontDoorRight;
    public FrontDoorLeft FrontDoorLeft;
    public AudioSource AudioSource;
    public AudioClip AudioClip;
    

    private void OnTriggerEnter(Collider other)
    {
        if (taskManager != null && taskManager.IsCurrentTask("TryFrontDoor"))
        {
            StartCoroutine(FrontDoorRight.ToggleDoor(true));
            StartCoroutine(FrontDoorLeft.ToggleDoor(true));
            taskManager.CompleteTask("TryFrontDoor");

            AudioSource.PlayOneShot(AudioClip, 1f);

        }
    }
}
