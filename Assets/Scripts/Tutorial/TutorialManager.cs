using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text tutorialText; // Assign via Inspector

    private string currentText = "";

    void Awake()
    {
        if (tutorialText == null)
            Debug.LogError("TutorialManager: tutorialText not assigned!");
        else
            tutorialText.text = ""; // start empty
    }

    /// <summary>
    /// Show a persistent tutorial message until manually cleared or updated.
    /// </summary>
    public void ShowPersistent(string message)
    {
        if (tutorialText == null || string.IsNullOrEmpty(message)) return;

        // Only update if the text is different
        if (currentText == message) return;

        tutorialText.text = message;
        currentText = message;
    }

    /// <summary>
    /// Clear the tutorial text.
    /// </summary>
    public void Clear()
    {
        if (tutorialText == null) return;

        tutorialText.text = "";
        currentText = "";
    }
}
