using UnityEngine;
using UnityEngine.UI; 

public class StaminaSystem : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float MaxStamina = 100f;
    [SerializeField] private float CurrentStamina;

    [Header("Consumption/Regen Rates")]
    [SerializeField] private float StaminaCostPerSecond = 10f;
    [SerializeField] private float RegenRatePerSecond = 5f;
    [SerializeField] private float LowStaminaThreshold = 5f; 
    [SerializeField] private float RegenDelayAfterSprint = 1.0f; 

    [Header("UI References (Optional)")]
    [SerializeField] private Slider StaminaBar; 

    private float lastSprintTime;

    private void Start()
    {
        CurrentStamina = MaxStamina;
        UpdateStaminaBar();
    }

    private void UpdateStaminaBar()
    {
        
        if (StaminaBar != null)
        {
            StaminaBar.maxValue = MaxStamina;
            StaminaBar.value = CurrentStamina;
        }
    }

    
    public bool CanSprint()
    {
        return CurrentStamina > LowStaminaThreshold;
    }

    public void ConsumeStamina()
    {
        
        CurrentStamina -= StaminaCostPerSecond * Time.deltaTime;
        CurrentStamina = Mathf.Max(CurrentStamina, 0f); 

        lastSprintTime = Time.time; 

        UpdateStaminaBar();
    }

    public void RegenStamina()
    {
        
        if (Time.time < lastSprintTime + RegenDelayAfterSprint)
        {
            return;
        }

       
        CurrentStamina += RegenRatePerSecond * Time.deltaTime;
        CurrentStamina = Mathf.Min(CurrentStamina, MaxStamina); 

        UpdateStaminaBar();
    }
}