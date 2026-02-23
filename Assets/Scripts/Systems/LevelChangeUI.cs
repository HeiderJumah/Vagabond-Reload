using UnityEngine;
using UnityEngine.SceneManagement;
using Vagabond.Systems.Scene;

public class LevelChangeUI : MonoBehaviour
{
    public static LevelChangeUI Instance;
    [SerializeField] private GameObject levelChangePanel;
    private SceneLoader sceneLoader;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            levelChangePanel.SetActive(false);
        }
    }

    public void ShowLevelChangePanel(SceneLoader loader)
    {
        sceneLoader = loader;
        levelChangePanel.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void OnYes()
    {
        Time.timeScale = 1f; // Resume the game
        levelChangePanel.SetActive(false);

        LevelManager.LevelConnection = sceneLoader.LevelManager;
        SceneManager.LoadScene(sceneLoader.SceneName);

    }

    public void OnNo()
    {
        Time.timeScale = 1f; // Resume the game
        levelChangePanel.SetActive(false);
        sceneLoader = null;
    }
}
