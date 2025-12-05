using UnityEngine;

public class JournalPickup : MonoBehaviour
{
    [Header("Journal Settings")]
    public string journalID;           // Unique ID for task completion
    public string journalTitle = "Note";

    [TextArea(5, 15)]
    public string journalContent = "This is a note...";

    [Header("Optional")]
    public Sprite journalImage;        // Optional image to display
}