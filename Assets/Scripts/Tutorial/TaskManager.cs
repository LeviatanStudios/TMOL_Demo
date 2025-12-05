using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public string taskName;
    public string tutorialMessage;
    public bool isCompleted = false;
}

public class TaskManager : MonoBehaviour
{
    [Header("Task Settings")]
    public List<Task> tasks = new List<Task>();

    [Header("Tutorial UI")]
    public TutorialManager tutorialManager;

    [Header("Waypoint Settings")]
    [Tooltip("Prefab for the floating marker (arrow)")]
    public GameObject waypointMarkerPrefab;

    [Tooltip("Glow material to apply to task objects")]
    public Material glowMaterial;

    [Tooltip("Show waypoint markers")]
    public bool showWaypoints = true;

    private Task currentTask;
    private List<TaskWaypoint> registeredWaypoints = new List<TaskWaypoint>();
    private TaskWaypoint activeWaypoint;

    void Start()
    {
        UpdateCurrentTask();
    }

    void Update()
    {
        if (currentTask != null && currentTask.isCompleted)
        {
            UpdateCurrentTask();
        }
    }

    private void UpdateCurrentTask()
    {
        // Hide previous waypoint
        if (activeWaypoint != null)
        {
            activeWaypoint.HideMarker();
            activeWaypoint = null;
        }

        // Find next incomplete task
        currentTask = tasks.Find(t => !t.isCompleted);

        if (currentTask != null)
        {
            tutorialManager?.ShowPersistent(currentTask.tutorialMessage);

            // Show waypoint for current task
            if (showWaypoints)
            {
                ShowWaypointForTask(currentTask.taskName);
            }
        }
        else
        {
            tutorialManager?.Clear();
        }
    }

    private void ShowWaypointForTask(string taskName)
    {
        // Find waypoint matching this task
        TaskWaypoint waypoint = registeredWaypoints.Find(w => w != null && w.taskID == taskName);

        if (waypoint != null)
        {
            waypoint.ShowMarker(waypointMarkerPrefab, glowMaterial);
            activeWaypoint = waypoint;
            Debug.Log($"Showing waypoint for task: {taskName}");
        }
    }

    public void RegisterWaypoint(TaskWaypoint waypoint)
    {
        if (!registeredWaypoints.Contains(waypoint))
        {
            registeredWaypoints.Add(waypoint);

            // If this waypoint is for current task, show it
            if (currentTask != null && waypoint.taskID == currentTask.taskName && showWaypoints)
            {
                waypoint.ShowMarker(waypointMarkerPrefab, glowMaterial);
                activeWaypoint = waypoint;
            }
        }
    }

    public void UnregisterWaypoint(TaskWaypoint waypoint)
    {
        registeredWaypoints.Remove(waypoint);

        if (activeWaypoint == waypoint)
        {
            activeWaypoint = null;
        }
    }

    public bool CanCompleteTask(string taskName)
    {
        if (string.IsNullOrEmpty(taskName)) return true;
        return currentTask != null && currentTask.taskName == taskName && !currentTask.isCompleted;
    }

    public void CompleteTask(string taskName)
    {
        if (CanCompleteTask(taskName))
        {
            currentTask.isCompleted = true;
            Debug.Log($"Task Completed: {currentTask.taskName}");
            UpdateCurrentTask();
        }
        else
        {
            Debug.Log($"Cannot complete '{taskName}' - current task is '{currentTask?.taskName ?? "none"}'");
        }
    }

    public bool IsCurrentTask(string taskName)
    {
        return CanCompleteTask(taskName);
    }

    public bool AllTasksCompleted()
    {
        return tasks.TrueForAll(t => t.isCompleted);
    }

    public string GetCurrentTaskName()
    {
        return currentTask?.taskName ?? "None";
    }

    /// <summary>
    /// Toggle waypoint visibility
    /// </summary>
    public void SetWaypointsVisible(bool visible)
    {
        showWaypoints = visible;

        if (visible && currentTask != null)
        {
            ShowWaypointForTask(currentTask.taskName);
        }
        else if (activeWaypoint != null)
        {
            activeWaypoint.HideMarker();
            activeWaypoint = null;
        }
    }
}