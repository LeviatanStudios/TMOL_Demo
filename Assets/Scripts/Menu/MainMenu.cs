using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject InGamePanel;
    [SerializeField] private GameObject JournalPanel;

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

    private void Start()
    {
        ShowMenu();

        if (fadePanel != null)
        {
            fadePanel.alpha = 1f;
            StartCoroutine(Fade(1f, 0f));
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

        // Play menu music
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
            timer += Time.deltaTime;
            menuMusic.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeDuration);
            yield return null;
        }

        menuMusic.Stop();
        menuMusic.volume = startVolume;
    }

    public void PlayGame()
    {
        Debug.Log("Button Play Game Cleck");
        if (isTransitioning) return;
        PlayButtonSound();
        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        isTransitioning = true;

        // Fade out music
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
        InGamePanel.SetActive(true);
        JournalPanel.SetActive(true);
        tutorialPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        PlayButtonSound();
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        PlayButtonSound();
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        if (isTransitioning) return;
        PlayButtonSound();
        StartCoroutine(QuitWithFade());
    }

    private IEnumerator QuitWithFade()
    {
        isTransitioning = true;

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
            timer += Time.deltaTime;
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