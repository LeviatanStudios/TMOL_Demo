using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BatteryUI : MonoBehaviour
{
    [Header("Battery Bar")]
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private Gradient batteryColorGradient;

    [Header("Battery Text (Optional)")]
    [SerializeField] private TextMeshProUGUI batteryPercentText;
    [SerializeField] private TextMeshProUGUI spareBatteryText;

    [Header("Spare Battery Icons (Optional)")]
    [SerializeField] private GameObject[] spareBatteryIcons;

    [Header("Animation")]
    [SerializeField] private bool animateFill = true;
    [SerializeField] private float fillAnimationSpeed = 5f;

    private float targetFillAmount = 1f;
    private BatteryManager batteryManager;

    private void Start()
    {
        batteryManager = BatteryManager.Instance;

        if (batteryManager != null)
        {
            batteryManager.OnBatteryChanged.AddListener(UpdateBatteryUI);
            batteryManager.OnSpareBatteriesChanged.AddListener(UpdateSpareBatteryUI);

            // Initial update
            UpdateBatteryUI(batteryManager.CurrentBattery, batteryManager.MaxBatteryCapacity);
            UpdateSpareBatteryUI(batteryManager.SpareBatteries, batteryManager.MaxBatteries);
        }
    }

    private void Update()
    {
        // Smooth fill animation
        if (animateFill && batteryFillImage != null)
        {
            batteryFillImage.fillAmount = Mathf.Lerp(
                batteryFillImage.fillAmount,
                targetFillAmount,
                Time.deltaTime * fillAnimationSpeed
            );
        }
    }

    private void UpdateBatteryUI(int current, int max)
    {
        float percent = (float)current / max;
        float percentValue = percent * 100f;
        targetFillAmount = percent;

        if (!animateFill && batteryFillImage != null)
        {
            batteryFillImage.fillAmount = percent;
        }

        // Update color based on gradient
        if (batteryFillImage != null && batteryColorGradient != null)
        {
            batteryFillImage.color = batteryColorGradient.Evaluate(percent);
        }

        // Update percentage text
        if (batteryPercentText != null)
        {
            batteryPercentText.text = $"{Mathf.RoundToInt(percentValue)}%";
        }
    }

    private void UpdateSpareBatteryUI(int current, int max)
    {
        // Update text
        if (spareBatteryText != null)
        {
            spareBatteryText.text = $"{current}/{max}";
        }

        // Update icons
        if (spareBatteryIcons != null)
        {
            for (int i = 0; i < spareBatteryIcons.Length; i++)
            {
                if (spareBatteryIcons[i] != null)
                {
                    spareBatteryIcons[i].SetActive(i < current);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (batteryManager != null)
        {
            batteryManager.OnBatteryChanged.RemoveListener(UpdateBatteryUI);
            batteryManager.OnSpareBatteriesChanged.RemoveListener(UpdateSpareBatteryUI);
        }
    }
}