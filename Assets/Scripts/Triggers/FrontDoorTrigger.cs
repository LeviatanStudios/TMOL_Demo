using UnityEngine;
using System.Collections;

public class FrontDoorTrigger : MonoBehaviour
{

    [Header("Reference")]
    public TaskManager taskManager;
    public FrontDoorRight FrontDoorRight;
    public FrontDoorLeft FrontDoorLeft;
    public AudioSource AudioSource;
    public AudioClip AudioClip;
    public GameObject hukomObject;
    [SerializeField] public GameObject EmilliaDialogue;
    private void OnTriggerEnter(Collider other)
    {
        if (taskManager != null && taskManager.IsCurrentTask("TryFrontDoor"))
        {
            StartCoroutine(FrontDoorRight.ToggleDoor(true));
            StartCoroutine(FrontDoorLeft.ToggleDoor(true));
            taskManager.CompleteTask("TryFrontDoor");
            hukomObject.SetActive(false);
            AudioSource.PlayOneShot(AudioClip, 1f);

            EmilliaDialogue.SetActive(true);

            StartCoroutine(HideEmilliaDialogueAfterDelay());
        }
    }

    private IEnumerator HideEmilliaDialogueAfterDelay()
    {
        yield return new WaitForSeconds(2f); // 1 or 2 seconds
        EmilliaDialogue.SetActive(false);
    }
}
