using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    public int batteryAmount = 100;

    [Tooltip("Leave empty if battery can be collected anytime")]
    public string taskID;
}