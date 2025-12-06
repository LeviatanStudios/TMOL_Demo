using System.Collections;
using UnityEngine;

public class FrontDoorLeft : MonoBehaviour
{
    private float openAngleLeft = -90f;
    public float openSpeed = 2f;

    private Quaternion _closedRotation;
    private Quaternion _openRotation;

    [Header("Triggered Front Door")]

    [Space(10)]
    [SerializeField] public AudioSource AudioSource;
    [SerializeField] public AudioClip doorSlammed;



    void Start()
    {
        _closedRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, openAngleLeft, 0));
        _openRotation = transform.rotation;
    }

    public IEnumerator ToggleDoor(bool isTriggered)
    {
        Quaternion targetRotation = isTriggered ? _closedRotation : _openRotation;

        if (isTriggered)
        {
            AudioSource.PlayOneShot(doorSlammed);
        }


        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
