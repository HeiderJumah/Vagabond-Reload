using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip pauseClip;
    [SerializeField] private AudioClip MainMenuClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PlayMainMenuMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMainMenuMusic()
    {
        if (musicSource != null && MainMenuClip != null)
        {
            musicSource.clip = MainMenuClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayMusic()
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayBossMusic()
    {
        if (musicSource != null && bossMusic != null)
        {
            musicSource.clip = bossMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayPauseSound()
    {
        if (musicSource != null && pauseClip != null)
        {
            musicSource.clip = pauseClip;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

}
