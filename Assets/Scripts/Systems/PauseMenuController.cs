using UnityEngine;
using Vagabond.Core;
using UnityEngine.SceneManagement;
using Vagabond.Systems.Scene;

namespace Vagabond.Systems.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName;
        [SerializeField] private GameObject settingsPanel;

        public bool IsSettingsOpen => settingsPanel.activeSelf;

        public void OnResume()
        {
            Debug.Log("[PauseMenu] Resume");
            Time.timeScale = 1f;
            GameManager.Instance.GameStateManager.ChangeState(GameState.InGame);
            MusicManager.Instance.PlayMusic();
        }

        public void OnBackToMainMenu()
        {
            Debug.Log("[PauseMenu] Back to Main Menu");
            Time.timeScale = 1f;
            GameManager.Instance.GameStateManager.ChangeState(GameState.MainMenu);
            SceneManager.LoadScene(mainMenuSceneName);
            MusicManager.Instance.PlayMainMenuMusic();
        }

        public void OnSettings()
        {
            settingsPanel.SetActive(true);
        }

        public void OnCloseSettings()
        {
            settingsPanel.SetActive(false);
        }

    }
}

