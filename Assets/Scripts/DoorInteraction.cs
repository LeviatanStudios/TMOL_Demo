using System.Collections;
using UnityEngine;

public class DoorInteraction : MonoBehaviour, Interactable
{
    public float openAngle = 90f;
    public float openSpeed = 8f;
    private bool isOpen = false;
    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    [SerializeField] private AudioSource doorOpenAudioSource;
    [SerializeField] private float openDelay = 0;
    [Space(10)]
    [SerializeField] private AudioSource doorCloseAudioSource;
    [SerializeField] private float closeDelay = 0.3f;

    private Coroutine _currentCoroutine; // Track the running coroutine

    void Start()
    {
        _closedRotation = transform.rotation;
        _openRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngle, 0));
    }

    public void Interact()
    {
        // Stop any running coroutine before starting a new one
        if (_currentCoroutine != null)
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine = StartCoroutine(ToggleDoor());
    }

    private IEnumerator ToggleDoor()
    {
        isOpen = !isOpen;
        Quaternion targetRotation = isOpen ? _openRotation : _closedRotation;

        // Stop any currently playing door audio
        doorOpenAudioSource.Stop();
        doorCloseAudioSource.Stop();

        if (isOpen)
        {
            doorOpenAudioSource.PlayDelayed(openDelay);
        }
        else
        {
            doorCloseAudioSource.PlayDelayed(closeDelay);
        }

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }

        transform.rotation = targetRotation;
        _currentCoroutine = null;
    }
}