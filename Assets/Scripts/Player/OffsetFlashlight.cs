using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI.Table;

public class OffsetFlashlight : MonoBehaviour
{
    [Header("References")]
    public GameObject FollowCam;
    public Light Flashlight;
    public TextMeshProUGUI WarningText;
    public TaskManager taskManager;
    public PlayerMovement playerMovement;
    [SerializeField] public GameObject hukomObject;
    [SerializeField] public AudioSource playerAudioSource;

    [Header("Audio")]
    public AudioSource Source;
    public AudioClip FlashLight_OnSound;
    public AudioClip FlashLight_OffSound;
    public AudioClip NoBatterySound;
    public AudioClip BatteryPickupSound;
    public AudioClip BatteryLoadSound; // Optional: sound when loading battery

    private bool FlashLightIsOn = false;
    private bool firstTimeFlashlightOn = false;
    private bool firstTimePickupBattery = false;

    private BatteryManager batteryManager;

    void Start()
    {
        Flashlight.enabled = false;
        FlashLightIsOn = false;

        if (WarningText != null)
            WarningText.text = "";

        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        // Get BatteryManager reference
        batteryManager = BatteryManager.Instance;

        if (batteryManager != null)
        {
            batteryManager.OnBatteryDepleted.AddListener(OnBatteryDepleted);
        }
    }

    void Update()
    {
        transform.position = FollowCam.transform.position;
        transform.rotation = FollowCam.transform.rotation;

        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleFlashlight();
    }

    void ToggleFlashlight()
    {
        if (!FlashLightIsOn)
        {
            // Check if we have battery loaded
            bool hasBatteryLoaded = batteryManager != null && batteryManager.CurrentBattery > 0;

            if (hasBatteryLoaded)
            {
                // Battery is loaded, turn on flashlight
                TurnOnFlashlight();
            }
            else
            {
                // No battery loaded, check if we have spare batteries
                bool hasSpareBattery = batteryManager != null && batteryManager.HasSpareBattery();

                if (hasSpareBattery)
                {
                    // Load a spare battery and turn on
                    batteryManager.LoadBattery();

                    if (BatteryLoadSound != null)
                        Source.PlayOneShot(BatteryLoadSound);

                    ShowWarning("Battery loaded!");
                    TurnOnFlashlight();
                }
                else
                {
                    // No batteries at all
                    HandleNoBattery();
                }
            }
        }
        else
        {
            TurnOffFlashlight();
        }
    }

    void TurnOnFlashlight()
    {
        Flashlight.enabled = true;
        FlashLightIsOn = true;
        Source.PlayOneShot(FlashLight_OnSound);

        if (batteryManager != null)
            batteryManager.SetFlashlightState(true);
    }

    void TurnOffFlashlight()
    {
        Flashlight.enabled = false;
        FlashLightIsOn = false;
        Source.PlayOneShot(FlashLight_OffSound);

        if (batteryManager != null)
            batteryManager.SetFlashlightState(false);
    }

    void HandleNoBattery()
    {
        // First time pressing F with no battery - complete tutorial task and unfreeze
        if (!firstTimeFlashlightOn)
        {
            firstTimeFlashlightOn = true;

            if (taskManager != null)
                taskManager.CompleteTask("Flashlight");

            if (playerMovement != null)
            {
                playerMovement.UnfreezePlayer();
                if (hukomObject != null)
                    hukomObject.SetActive(true);
            }

            Debug.Log("Tutorial complete - Player unfrozen!");
        }

        Source.PlayOneShot(NoBatterySound);
        ShowWarning("No batteries!");
    }

    // Called by BatteryManager when battery runs out
    private void OnBatteryDepleted()
    {
        if (FlashLightIsOn)
        {
            // Try to auto-load a spare battery
            if (batteryManager != null && batteryManager.HasSpareBattery())
            {
                batteryManager.LoadBattery();
                ShowWarning("Auto-loaded spare battery!");

                if (BatteryLoadSound != null)
                    Source.PlayOneShot(BatteryLoadSound);
            }
            else
            {
                // No spare batteries, turn off flashlight
                Flashlight.enabled = false;
                FlashLightIsOn = false;
                Source.PlayOneShot(NoBatterySound);
                ShowWarning("Flashlight ran out of battery!");

                if (batteryManager != null)
                    batteryManager.SetFlashlightState(false);
            }
        }
    }

    // Called when picking up batteries
    public void AddBattery(int amount)
    {
        if (batteryManager != null)
        {
            if (batteryManager.AddBattery())
            {
                Source.PlayOneShot(BatteryPickupSound);
                ShowWarning($"Battery collected! ({batteryManager.SpareBatteries}/{batteryManager.MaxBatteries})");

                if (!firstTimePickupBattery && taskManager != null)
                {
                    taskManager.CompleteTask("CollectBattery");
                    firstTimePickupBattery = true;
                }
            }
            else
            {
                ShowWarning("Can't carry more batteries!");
            }
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

    private void OnDestroy()
    {
        if (batteryManager != null)
        {
            batteryManager.OnBatteryDepleted.RemoveListener(OnBatteryDepleted);
        }
    }
}
