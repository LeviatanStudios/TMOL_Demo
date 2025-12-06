using UnityEngine;

public class TaskWaypoint : MonoBehaviour
{
    [Header("Task Settings")]
    [Tooltip("Must match the task name in TaskManager")]
    public string taskID;

    [Header("Marker Offset")]
    public Vector3 markerOffset = new Vector3(0, 2f, 0);

    [Header("Optional: Custom Glow")]
    public GameObject customGlowEffect;

    private GameObject activeMarker;
    private Renderer[] renderers;
    private Material[][] originalMaterialArrays; // Store ALL materials per renderer
    private bool isGlowing = false;
    private TaskManager taskManager;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // Store ALL original materials for each renderer
        originalMaterialArrays = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                // Clone the materials array to avoid reference issues
                Material[] mats = renderers[i].materials;
                originalMaterialArrays[i] = new Material[mats.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    originalMaterialArrays[i][j] = mats[j];
                }
            }
        }

        taskManager = FindFirstObjectByType<TaskManager>();
        if (taskManager != null)
        {
            taskManager.RegisterWaypoint(this);
        }
    }

    void Update()
    {
        if (taskManager != null && (activeMarker != null || isGlowing))
        {
            if (!taskManager.IsCurrentTask(taskID))
            {
                HideMarker();
            }
        }
    }

    void OnDestroy()
    {
        if (taskManager != null)
        {
            taskManager.UnregisterWaypoint(this);
        }

        if (activeMarker != null)
        {
            Destroy(activeMarker);
        }
    }

    public void ShowMarker(GameObject markerPrefab, Material glowMaterial)
    {
        if (markerPrefab != null && activeMarker == null)
        {
            activeMarker = Instantiate(markerPrefab, transform.position + markerOffset, Quaternion.identity);
            activeMarker.transform.SetParent(transform);

            TaskMarkerAnimation anim = activeMarker.GetComponent<TaskMarkerAnimation>();
            if (anim == null)
            {
                anim = activeMarker.AddComponent<TaskMarkerAnimation>();
            }
        }

        if (glowMaterial != null && !isGlowing)
        {
            ApplyGlow(glowMaterial);
        }

        if (customGlowEffect != null)
        {
            customGlowEffect.SetActive(true);
        }

        Debug.Log($"Waypoint shown for: {taskID}");
    }

    public void HideMarker()
    {
        if (activeMarker != null)
        {
            Destroy(activeMarker);
            activeMarker = null;
        }

        if (isGlowing)
        {
            RemoveGlow();
        }

        if (customGlowEffect != null)
        {
            customGlowEffect.SetActive(false);
        }

        Debug.Log($"Waypoint hidden for: {taskID}");
    }

    public void OnObjectRead()
    {
        HideMarker();

        if (taskManager != null)
        {
            taskManager.CompleteTask(taskID);
        }
    }

    private void ApplyGlow(Material glowMaterial)
    {
        isGlowing = true;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || originalMaterialArrays[i] == null) continue;

            // Add glow material to existing materials
            Material[] currentMats = originalMaterialArrays[i];
            Material[] newMats = new Material[currentMats.Length + 1];
            currentMats.CopyTo(newMats, 0);
            newMats[newMats.Length - 1] = glowMaterial;
            renderers[i].materials = newMats;
        }

        Debug.Log($"Glow applied to: {gameObject.name}");
    }

    private void RemoveGlow()
    {
        isGlowing = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalMaterialArrays[i] != null)
            {
                // Restore ALL original materials
                renderers[i].materials = originalMaterialArrays[i];
            }
        }

        Debug.Log($"Glow removed from: {gameObject.name}");
    }

    public Vector3 GetMarkerPosition()
    {
        return transform.position + markerOffset;
    }
}