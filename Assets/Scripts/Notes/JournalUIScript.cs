using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JournalUI : MonoBehaviour
{
    public static JournalUI Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject journalPanel;
    public TMP_Text titleText;
    public TMP_Text contentText;
    public Image journalImage;

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (journalPanel != null)
            journalPanel.SetActive(false);
    }

    public void ShowJournal(string title, string content, Sprite image = null)
    {
        if (journalPanel == null) return;

        journalPanel.SetActive(true);

        if (titleText != null)
            titleText.text = title;

        if (contentText != null)
            contentText.text = content;

        if (journalImage != null)
        {
            if (image != null)
            {
                journalImage.sprite = image;
                journalImage.gameObject.SetActive(true);
            }
            else
            {
                journalImage.gameObject.SetActive(false);
            }
        }
    }

    public void HideJournal()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
    }

    public bool IsShowing()
    {
        return journalPanel != null && journalPanel.activeSelf;
    }
}