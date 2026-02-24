using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip bossMusic;
    [SerializeField] private AudioClip pauseClip;
    [SerializeField] private AudioClip MainMenuClip;

    // persitant volume settings, can be set from settings menu
    public float MusicVolume { get; private set; } = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyVolume();
            PlayMainMenuMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        musicSource.volume = MusicVolume;
    }

    private void ApplyVolume()
    {
        if (musicSource != null)
            musicSource.volume = MusicVolume;
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
        AudioClip clipToPlay = null;

        // Determine which music to play based on the current level
        if (LevelManager.LevelConnection != null && LevelManager.LevelConnection.levelMusic != null)
        {
            clipToPlay = LevelManager.LevelConnection.levelMusic;
        }
        else if (musicClip != null)
        {
            // Fallback to default music if no level-specific music is set
            clipToPlay = musicClip;
        }


        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = clipToPlay;
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
