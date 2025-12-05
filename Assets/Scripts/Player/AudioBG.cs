using UnityEngine;

public class AudioBG : MonoBehaviour
{
    [SerializeField] private AudioSource m_AudioSource;
    [SerializeField] private AudioClip m_AudioClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;

    private void Start()
    {
        if (m_AudioSource == null)
            m_AudioSource = GetComponent<AudioSource>();

        if (m_AudioSource != null && m_AudioClip != null)
        {
            m_AudioSource.clip = m_AudioClip;
            m_AudioSource.loop = true;
            m_AudioSource.volume = volume;

            if (playOnStart)
                m_AudioSource.Play();
        }
    }

    public void PlayMusic() => m_AudioSource?.Play();
    public void StopMusic() => m_AudioSource?.Stop();
    public void SetVolume(float vol) => m_AudioSource.volume = Mathf.Clamp01(vol);
}