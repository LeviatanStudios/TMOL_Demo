using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

public class OffsetFlashlight : MonoBehaviour
{
    [Header("References")]
    public GameObject FollowCam;
    public Light Flashlight;
    public TextMeshProUGUI WarningText;
    public TaskManager taskManager; // Reference to TaskManager

    [Header("Battery Settings")]
    public int maxBatteries = 100;
    public int currentBatteries;
    public float batteryDrainRate = 1f; // units per second

    [Header("Audio")]
    public AudioSource Source;
    public AudioClip FlashLight_OnSound;
    public AudioClip FlashLight_OffSound;
    public AudioClip NoBatterySound;
    public AudioClip BatteryPickupSound;

    private bool FlashLightIsOn = false;
    private bool firstTimeFlashlightOn = false;  // Track first flashlight use
    private bool firstTimePickupBattery = false; // Track first battery pickup
    private float drainTimer = 0f;

    void Start()
    {
        currentBatteries = 0;
        Flashlight.enabled = false;
        if (WarningText != null)
            WarningText.text = "";
    }

    void Update()
    {
        // Follow camera
        transform.position = FollowCam.transform.position;
        transform.rotation = FollowCam.transform.rotation;

        // Toggle flashlight
        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleFlashlight();

        // Drain battery if flashlight is on
        if (FlashLightIsOn)
            HandleBatteryDrain();
    }

    void ToggleFlashlight()
    {
        if (!FlashLightIsOn)
        {
            if (currentBatteries > 0)
            {
                Flashlight.enabled = true;
                FlashLightIsOn = true;
                Source.PlayOneShot(FlashLight_OnSound);

                
            }
            else
            {
                // Optionally: complete first-use flashlight task
                if (!firstTimeFlashlightOn && taskManager != null)
                {
                    taskManager.CompleteTask("Flashlight");
                    firstTimeFlashlightOn = true;
                }
                Source.PlayOneShot(NoBatterySound);
                ShowWarning("Flashlight ran out of battery!");
            }
        }
        else
        {
            Flashlight.enabled = false;
            FlashLightIsOn = false;
            Source.PlayOneShot(FlashLight_OffSound);
        }
    }

    void HandleBatteryDrain()
    {
        drainTimer += Time.deltaTime;
        if (drainTimer >= 1f)
        {
            currentBatteries--;
            drainTimer = 0f;

            if (currentBatteries <= 0)
            {
                FlashLightIsOn = false;
                Flashlight.enabled = false;
                Source.PlayOneShot(NoBatterySound);
                ShowWarning("Flashlight ran out of battery!");
            }
        }
    }


   
    public void AddBattery(int amount)
    {
        int batteryCount = 0;

        batteryCount += amount;
        Debug.Log("BATTERY ADDED! Current: " + batteryCount);

        currentBatteries += amount;
        if (currentBatteries > maxBatteries)
            currentBatteries = maxBatteries;

        Source.PlayOneShot(BatteryPickupSound);
        ShowWarning($"Battery collected! ({currentBatteries}/{maxBatteries})");

        // ✅ Complete Task 3 on first battery pickup
        if (!firstTimePickupBattery && taskManager != null)
        {
            taskManager.CompleteTask("CollectBattery");
            firstTimePickupBattery = true;
        }
    }

    void ShowWarning(string message)
    {
        if (WarningText == null) return;

        WarningText.text = message;
        StopAllCoroutines();
        StartCoroutine(HideWarningAfterSeconds(3f));
    }

    IEnumerator HideWarningAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        WarningText.text = "";
    }


}
