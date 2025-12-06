using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public float interactionDistance = 3f;
    public LayerMask interactableLayer; // Set this in Inspector!
    public Transform playerCamera;

    private Interactable _currentInteractable;
    private Outline _currentOutline;

    void Update()
    {
        CheckForInteractable();

        if (_currentInteractable != null && Input.GetKeyDown(KeyCode.E))
        {
            _currentInteractable.Interact();
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
        {
            // Make sure we're not hitting ourselves
            if (hit.collider.transform.IsChildOf(transform) || hit.collider.transform == transform)
            {
                ResetOutline();
                return;
            }

            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                if (_currentInteractable != interactable)
                {
                    ResetOutline();
                    _currentInteractable = interactable;
                    _currentOutline = hit.collider.GetComponent<Outline>();

                    if (_currentOutline != null)
                    {
                        _currentOutline.enabled = true;
                    }
                }
                return;
            }
        }

        ResetOutline();
    }

    private void ResetOutline()
    {
        if (_currentOutline != null)
        {
            _currentOutline.enabled = false;
        }
        _currentInteractable = null;
        _currentOutline = null;
    }
}