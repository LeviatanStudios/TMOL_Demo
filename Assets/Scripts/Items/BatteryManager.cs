using UnityEngine;
using UnityEngine.Events;

public class BatteryManager : MonoBehaviour
{
    public static BatteryManager Instance { get; private set; }

    [Header("Battery Settings")]
    [SerializeField] private int maxBatteryCapacity = 100;
    [SerializeField] private int maxBatteries = 3;
    [SerializeField] private float drainRate = 5f; // Per second when flashlight is on

    [Header("Current State")]
    [SerializeField] private float currentBattery = 0f; // Changed to float for smooth draining
    [SerializeField] private int spareBatteries = 0;

    [Header("Events")]
    public UnityEvent<int, int> OnBatteryChanged; // current, max
    public UnityEvent<int, int> OnSpareBatteriesChanged; // current, max
    public UnityEvent OnBatteryDepleted;

    public int CurrentBattery => Mathf.RoundToInt(currentBattery);
    public int MaxBatteryCapacity => maxBatteryCapacity;
    public int SpareBatteries => spareBatteries;
    public int MaxBatteries => maxBatteries;
    public float BatteryPercent => currentBattery / maxBatteryCapacity;

    private bool isFlashlightOn = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Initialize UI
        OnBatteryChanged?.Invoke(CurrentBattery, maxBatteryCapacity);
        OnSpareBatteriesChanged?.Invoke(spareBatteries, maxBatteries);
    }

    private void Update()
    {
        if (isFlashlightOn && currentBattery > 0)
        {
            DrainBattery(drainRate * Time.deltaTime);
        }
    }

    public void SetFlashlightState(bool isOn)
    {
        isFlashlightOn = isOn;
    }

    public void DrainBattery(float amount)
    {
        float previousBattery = currentBattery;
        currentBattery = Mathf.Max(0f, currentBattery - amount);

        // Only update UI when the rounded value changes (to avoid spam)
        if (Mathf.RoundToInt(previousBattery) != Mathf.RoundToInt(currentBattery))
        {
            OnBatteryChanged?.Invoke(CurrentBattery, maxBatteryCapacity);
        }

        if (currentBattery <= 0f)
        {
            currentBattery = 0f;
            OnBatteryDepleted?.Invoke();
        }
    }

    public bool TryUseSpareBattery()
    {
        if (spareBatteries > 0)
        {
            spareBatteries--;
            currentBattery = maxBatteryCapacity;

            OnBatteryChanged?.Invoke(CurrentBattery, maxBatteryCapacity);
            OnSpareBatteriesChanged?.Invoke(spareBatteries, maxBatteries);

            Debug.Log($"Used spare battery! Remaining: {spareBatteries}");
            return true;
        }

        Debug.Log("No spare batteries left!");
        return false;
    }

    // Add battery to spare inventory
    public bool AddBattery()
    {
        if (spareBatteries < maxBatteries)
        {
            spareBatteries++;
            OnSpareBatteriesChanged?.Invoke(spareBatteries, maxBatteries);
            Debug.Log($"Added spare battery! Total: {spareBatteries}/{maxBatteries}");
            return true;
        }

        Debug.Log("Battery inventory full!");
        return false;
    }

    public bool HasBattery()
    {
        return currentBattery > 0 || spareBatteries > 0;
    }

    public bool HasSpareBattery()
    {
        return spareBatteries > 0;
    }

    // Load a spare battery into flashlight
    public bool LoadBattery()
    {
        if (spareBatteries > 0 && currentBattery <= 0)
        {
            return TryUseSpareBattery();
        }
        return false;
    }
}