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
    private Material[] originalMaterials;
    private bool isGlowing = false;
    private TaskManager taskManager;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalMaterials[i] = renderers[i].material;
        }

        taskManager = FindFirstObjectByType<TaskManager>();
        if (taskManager != null)
        {
            taskManager.RegisterWaypoint(this);
        }
    }

    void Update()
    {
        // Auto-hide if task is no longer current
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
        // Spawn arrow marker
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

        // Apply glow material
        if (glowMaterial != null && !isGlowing)
        {
            ApplyGlow(glowMaterial);
        }

        // Enable custom effect
        if (customGlowEffect != null)
        {
            customGlowEffect.SetActive(true);
        }

        Debug.Log($"Waypoint shown for: {taskID}");
    }

    public void HideMarker()
    {
        // Remove arrow marker
        if (activeMarker != null)
        {
            Destroy(activeMarker);
            activeMarker = null;
        }

        // Remove glow
        if (isGlowing)
        {
            RemoveGlow();
        }

        // Disable custom effect
        if (customGlowEffect != null)
        {
            customGlowEffect.SetActive(false);
        }

        Debug.Log($"Waypoint hidden for: {taskID}");
    }

    private void ApplyGlow(Material glowMaterial)
    {
        isGlowing = true;

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            // Add glow as additional material
            Material[] mats = rend.materials;
            Material[] newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[newMats.Length - 1] = glowMaterial;
            rend.materials = newMats;
        }

        Debug.Log($"Glow applied to: {gameObject.name}");
    }

    private void RemoveGlow()
    {
        isGlowing = false;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && i < originalMaterials.Length && originalMaterials[i] != null)
            {
                renderers[i].material = originalMaterials[i];
            }
        }

        Debug.Log($"Glow removed from: {gameObject.name}");
    }

    public Vector3 GetMarkerPosition()
    {
        return transform.position + markerOffset;
    }
}