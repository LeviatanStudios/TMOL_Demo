using UnityEngine;

public class BabyCryTrigger : MonoBehaviour
{
    [SerializeField] TaskManager taskManager;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip clip;

    private bool isTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (taskManager.IsCurrentTask("UnlockStudy") && !isTrigger)
        {
           
            audioSource.PlayOneShot(clip);
            isTrigger = true;
        }
    }
    
}
