using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TutorialPanel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 30f;
    [SerializeField] private float fadeDuration = 1f;

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;

    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        StartCoroutine(AutoHide());
    }

    private IEnumerator AutoHide()
    {
        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    // Call this to hide tutorial early (e.g., when player presses a key)
    public void HideTutorial()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        float startAlpha = canvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, timer / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}