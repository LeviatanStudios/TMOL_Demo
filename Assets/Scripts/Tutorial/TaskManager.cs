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

    private Task currentTask;

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
        // Find next incomplete task
        currentTask = tasks.Find(t => !t.isCompleted);

        if (currentTask != null)
        {
            tutorialManager?.ShowPersistent(currentTask.tutorialMessage);
            Debug.Log($"Current task: {currentTask.taskName}");
        }
        else
        {
            tutorialManager?.Clear();
            Debug.Log("All tasks completed!");
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

    public Task GetCurrentTask()
    {
        return currentTask;
    }
}