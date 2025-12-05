using System.Collections;
using UnityEngine;

public class FrontDoorRight : MonoBehaviour, Interactable
{
    private float openAngleRight = 90f;
    public float openSpeed = 8f;
    private bool isOpen = true;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    [Header("Triggered Front Door")]
    public bool isTrigger = false;

    [SerializeField] private AudioSource doorOpenAudioSource = null;
    [SerializeField] private float openDelay = 0;

    [Space(10)]
    [SerializeField] private AudioSource doorCloseAudioSource = null;
    [SerializeField] private float closeDelay = 0.3f;



    void Start()
    {
        _closedRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngleRight, 0));
        _openRotation = transform.rotation;
    }
    public void DoorTriggered(bool isTriggered)
    {
        Debug.Log(isTriggered);
        isTrigger = isTriggered;


        if (isTrigger)
        {
            doorOpenAudioSource.PlayDelayed(openDelay);
            StartCoroutine(ToggleDoor());
        }

    }

    // Update is called once per frame
    public void Interact()
    {
        StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        Quaternion targetRotation = isOpen ? _closedRotation : _openRotation;
        isOpen = !isOpen;

        if (isOpen)
        {
            doorOpenAudioSource.PlayDelayed(openDelay);
        }
        else if (!isOpen)
        {
            doorCloseAudioSource.PlayDelayed(closeDelay);
        }

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

            yield return null;
        }

        transform.rotation = targetRotation;

    }
}
