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
    public TaskManager taskManager;
    public PlayerMovement playerMovement; // Add this reference

    [Header("Battery Settings")]
    public int maxBatteries = 100;
    public int currentBatteries;
    public float batteryDrainRate = 1f;

    [Header("Audio")]
    public AudioSource Source;
    public AudioClip FlashLight_OnSound;
    public AudioClip FlashLight_OffSound;
    public AudioClip NoBatterySound;
    public AudioClip BatteryPickupSound;

    private bool FlashLightIsOn = false;
    private bool firstTimeFlashlightOn = false;
    private bool firstTimePickupBattery = false;
    private float drainTimer = 0f;

    void Start()
    {
        currentBatteries = 0;
        Flashlight.enabled = false;
        FlashLightIsOn = false;

        if (WarningText != null)
            WarningText.text = "";

        // Find PlayerMovement if not assigned
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        transform.position = FollowCam.transform.position;
        transform.rotation = FollowCam.transform.rotation;

        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleFlashlight();

        if (FlashLightIsOn && currentBatteries > 0)
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
                drainTimer = 0f;
                Source.PlayOneShot(FlashLight_OnSound);
            }
            else
            {
                // First time pressing F with no battery - complete tutorial task and unfreeze
                if (!firstTimeFlashlightOn)
                {
                    firstTimeFlashlightOn = true;

                    // Complete task
                    if (taskManager != null)
                        taskManager.CompleteTask("Flashlight");

                    // Unfreeze player
                    if (playerMovement != null)
                        playerMovement.UnfreezePlayer();

                    Debug.Log("Tutorial complete - Player unfrozen!");
                }

                Source.PlayOneShot(NoBatterySound);
                ShowWarning("No batteries!");
            }
        }
        else
        {
            Flashlight.enabled = false;
            FlashLightIsOn = false;
            drainTimer = 0f;
            Source.PlayOneShot(FlashLight_OffSound);
        }
    }

    void HandleBatteryDrain()
    {
        drainTimer += Time.deltaTime * batteryDrainRate;

        if (drainTimer >= 1f)
        {
            currentBatteries--;
            drainTimer = 0f;

            if (currentBatteries <= 0)
            {
                currentBatteries = 0;
                FlashLightIsOn = false;
                Flashlight.enabled = false;
                drainTimer = 0f;
                Source.PlayOneShot(NoBatterySound);
                ShowWarning("Flashlight ran out of battery!");
            }
        }
    }

    public void AddBattery(int amount)
    {
        currentBatteries += amount;
        if (currentBatteries > maxBatteries)
            currentBatteries = maxBatteries;

        Source.PlayOneShot(BatteryPickupSound);
        ShowWarning($"Battery collected! ({currentBatteries}/{maxBatteries})");

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