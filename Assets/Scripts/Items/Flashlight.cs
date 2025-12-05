using UnityEngine;
using TMPro;

public class Flashlight : MonoBehaviour
{
    [Header("References")]
    public GameObject flashlight;          // Flashlight object
    public TMP_Text tutorialText;          // TextMeshProUGUI reference

    private bool flashlightUsed = false;

    void Start()
    {
        flashlight.SetActive(false);
        tutorialText.text = "Press [F] for Flashlight";
    }

    void Update()
    {
        HandleFlashlight();
    }

    void HandleFlashlight()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            flashlight.SetActive(!flashlight.activeSelf);

            if (!flashlightUsed)
            {
                flashlightUsed = true;
                tutorialText.text = "Explore the Mansion.";
            }
        }
    }
}
