using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject InGamePanel;
    [SerializeField] private GameObject JournalPanel;
    [SerializeField] private GameObject BatteryPanel;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialPanel;

    [Header("Cameras")]
    [SerializeField] private Camera menuCamera;
    [SerializeField] private Camera playerCamera;

    [Header("Player")]
    [SerializeField] private GameObject player;

    [Header("Fade (Optional)")]
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioSource buttonClickSound;
    [SerializeField] private AudioSource menuMusic;
    [SerializeField] private float musicFadeDuration = 1f;

    private bool isTransitioning = false;
    private bool isGameStarted = false;
    private bool isPaused = false;
    private bool settingsOpenedFromPause = false; // Track where settings was opened from

    public bool IsPaused => isPaused;

    private void Start()
    {
        ShowMenu();

        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            StartCoroutine(Fade(1f, 0f));
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        if (isGameStarted && !isTransitioning)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                // If settings is open, close it first
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    private void ShowMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);

        menuCamera.gameObject.SetActive(true);
        playerCamera.gameObject.SetActive(false);

        player.SetActive(false);

        isGameStarted = false;
        isPaused = false;
        settingsOpenedFromPause = false;
        Time.timeScale = 1f;

        PlayMenuMusic();
    }

    private void PlayMenuMusic()
    {
        if (menuMusic != null && !menuMusic.isPlaying)
        {
            menuMusic.volume = 1f;
            menuMusic.loop = true;
            menuMusic.Play();
        }
    }

    private void StopMenuMusic()
    {
        if (menuMusic != null && menuMusic.isPlaying)
        {
            StartCoroutine(FadeOutMusic());
        }
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = menuMusic.volume;
        float timer = 0f;

        while (timer < musicFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            menuMusic.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
            yield return null;
        }

        menuMusic.Stop();
        menuMusic.volume = startVolume;
    }

    public void PlayGame()
    {
        Debug.Log("Button Play Game Click");
        if (isTransitioning) return;
        PlayButtonSound();
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        isTransitioning = true;

        StopMenuMusic();

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(0f, 1f));
        }

        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);

        menuCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        player.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
        }

        isTransitioning = false;
        isGameStarted = true;

        InGamePanel.SetActive(true);
        BatteryPanel.SetActive(true);
        JournalPanel.SetActive(true);
        tutorialPanel.SetActive(true);
    }

    #region Pause Menu
    public void PauseGame()
    {
        if (!isGameStarted || isTransitioning) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        if (InGamePanel != null)
            InGamePanel.SetActive(false);
        if (BatteryPanel != null)
            BatteryPanel.SetActive(false);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        PlayButtonSound();

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (InGamePanel != null)
            InGamePanel.SetActive(true);
        if (BatteryPanel != null)
            BatteryPanel.SetActive(true);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game Resumed");
    }

    public void QuitToMainMenu()
    {
        if (isTransitioning) return;
        PlayButtonSound();
        StartCoroutine(BackToMenuFromPause());
    }

    private IEnumerator BackToMenuFromPause()
    {
        isTransitioning = true;

        Time.timeScale = 1f;

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(0f, 1f));
        }

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (InGamePanel != null)
            InGamePanel.SetActive(false);
        if (BatteryPanel != null)
            BatteryPanel.SetActive(false);
        if (JournalPanel != null)
            JournalPanel.SetActive(false);
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);

        ShowMenu();

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
        }

        isTransitioning = false;
    }
    #endregion

    #region Settings
    // Called from Main Menu Settings button
    public void OpenSettings()
    {
        PlayButtonSound();
        settingsOpenedFromPause = false; // Opened from Main Menu

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Debug.Log("Settings opened from Main Menu");
    }

    // Called from Pause Menu Settings button
    public void OpenSettingsFromPause()
    {
        PlayButtonSound();
        settingsOpenedFromPause = true; // Opened from Pause Menu

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        Debug.Log("Settings opened from Pause Menu");
    }

    // Called from Settings Back button
    public void CloseSettings()
    {
        PlayButtonSound();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsOpenedFromPause)
        {
            // Return to Pause Menu
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);
            Debug.Log("Returning to Pause Menu");
        }
        else
        {
            // Return to Main Menu
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
            Debug.Log("Returning to Main Menu");
        }
    }
    #endregion

    public void QuitGame()
    {
        if (isTransitioning) return;
        PlayButtonSound();
        StartCoroutine(QuitWithFade());
    }

    private IEnumerator QuitWithFade()
    {
        isTransitioning = true;

        Time.timeScale = 1f;

        StopMenuMusic();

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(0f, 1f));
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator Fade(float from, float to)
    {
        fadePanel.blocksRaycasts = true;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
            yield return null;
        }

        fadePanel.alpha = to;
        fadePanel.blocksRaycasts = (to == 1f);
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }
    }

    public void ReturnToMenu()
    {
        if (isTransitioning) return;
        StartCoroutine(BackToMenu());
    }

    private IEnumerator BackToMenu()
    {
        isTransitioning = true;

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(0f, 1f));
        }

        ShowMenu();

        if (fadePanel != null)
        {
            yield return StartCoroutine(Fade(1f, 0f));
        }

        isTransitioning = false;
    }
}