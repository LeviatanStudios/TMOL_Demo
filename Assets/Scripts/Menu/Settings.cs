using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;

    [Header("Gameplay")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;

    [Header("Graphics")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    // Static so other scripts can access current sensitivity
    public static float MouseSensitivity { get; private set; } = 10f;

    private Resolution[] resolutions;

    // Quality mapping: 0 (Low) → 1, 1 (Medium) → 3, 2 (High) → 5
    private int[] qualityMapping = { 1, 3, 5 };

    private void Start()
    {
        SetupListeners();
        SetupResolutions();
        SetupQuality();
        LoadSettings();
    }

    private void SetupListeners()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(SetResolution);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQuality);
    }

    private void SetupResolutions()
    {
        if (resolutionDropdown == null) return;

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", currentResolutionIndex);
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupQuality()
    {
        if (qualityDropdown == null) return;

        qualityDropdown.ClearOptions();

        var qualityNames = new System.Collections.Generic.List<string>()
        {
            "Low",
            "Medium",
            "High"
        };

        qualityDropdown.AddOptions(qualityNames);

        int savedQuality = PlayerPrefs.GetInt("Quality", 1); // Default to Medium
        qualityDropdown.value = Mathf.Clamp(savedQuality, 0, qualityNames.Count - 1);
        qualityDropdown.RefreshShownValue();
    }

    private void LoadSettings()
    {
        // Load Volume
        if (volumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("Volume", 1f);
            volumeSlider.value = volume;
            SetVolume(volume);
        }

        // Load Sensitivity
        if (sensitivitySlider != null)
        {
            float sensitivity = PlayerPrefs.GetFloat("Sensitivity", 10f);
            sensitivitySlider.value = sensitivity;
            SetSensitivity(sensitivity);
        }

        // Load Fullscreen
        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        // Load Quality
        if (qualityDropdown != null)
        {
            int quality = PlayerPrefs.GetInt("Quality", 1);
            SetQuality(quality);
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);

        if (volumeText != null)
            volumeText.text = Mathf.RoundToInt(volume * 100f) + "%";
    }

    public void SetSensitivity(float sensitivity)
    {
        MouseSensitivity = sensitivity;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);

        if (sensitivityText != null)
            sensitivityText.text = sensitivity.ToString("F1");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Debug.Log($"Fullscreen: {isFullscreen}");
    }

    public void SetResolution(int resolutionIndex)
    {
        if (resolutions == null || resolutionIndex >= resolutions.Length) return;

        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
        Debug.Log($"Resolution: {resolution.width} x {resolution.height}");
    }

    public void SetQuality(int qualityIndex)
    {
        // Map custom index to Unity quality levels
        int unityQualityLevel = qualityMapping[Mathf.Clamp(qualityIndex, 0, qualityMapping.Length - 1)];

        QualitySettings.SetQualityLevel(unityQualityLevel);
        PlayerPrefs.SetInt("Quality", qualityIndex);
        Debug.Log($"Quality set to: {qualityIndex} (Unity level: {unityQualityLevel})");
    }

    public void ResetToDefaults()
    {
        // Reset Volume
        if (volumeSlider != null)
            volumeSlider.value = 1f;

        // Reset Sensitivity
        if (sensitivitySlider != null)
            sensitivitySlider.value = 10f;

        // Reset Fullscreen
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = true;

        // Reset Quality to Medium
        if (qualityDropdown != null)
            qualityDropdown.value = 1;

        // Reset Resolution to highest
        if (resolutionDropdown != null && resolutions != null)
            resolutionDropdown.value = resolutions.Length - 1;

        SaveSettings();
        Debug.Log("Settings reset to defaults");
    }

    public void SaveSettings()
    {
        PlayerPrefs.Save();
        Debug.Log("Settings saved");
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);

        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.RemoveListener(SetSensitivity);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(SetResolution);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.RemoveListener(SetQuality);
    }
}