using UnityEngine;
using Vagabond.Core;
using UnityEngine.SceneManagement;
using Vagabond.Systems.Scene;
using UnityEngine.UI;

namespace Vagabond.Systems.UI
{
    public class PauseMenuController : MonoBehaviour
    {

        [SerializeField] private string mainMenuSceneName;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Slider audioSlider;

        public bool IsSettingsOpen => settingsPanel.activeSelf;

        public void OnResume()
        {
            Debug.Log("[PauseMenu] Resume");
            Time.timeScale = 1f;
            GameManager.Instance.GameStateManager.ChangeState(GameState.InGame);
            MusicManager.Instance.PlayMusicForState();
        }

        public void OnBackToMainMenu()
        {
            Debug.Log("[PauseMenu] Back to Main Menu");
            Time.timeScale = 1f;
            DestoryAllPersistentObjects();
            // let bosses spawn again when going back to main menu
            LevelState.ResetLevelState();
            //GameManager.Instance.GameStateManager.ChangeState(GameState.MainMenu);
            SceneManager.LoadScene(mainMenuSceneName);
            //MusicManager.Instance.PlayMainMenuMusic(); 
        }

        public void OnSettings()
        {
            settingsPanel.SetActive(true);
        }

        public void OnCloseSettings()
        {
            settingsPanel.SetActive(false);
        }


        public void DropwDown(int index)
        {
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            bool value = index == 1;
            playerMovement.SetCameraLock(value);
        }

        private void Start()
        {
            if(audioSlider != null)
            {
                // Set initial slider value to current music volume
                audioSlider.value = MusicManager.Instance.MusicVolume;
                // Add listener for slider value changes
                audioSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            }
        }

        private void OnMusicSliderChanged(float value)
        {
            MusicManager.Instance.SetMusicVolume(value);
        }

        private void DestoryAllPersistentObjects()
        {
            var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach(var obj in allObjects)
            {
                
                if (obj.scene.name == "DontDestroyOnLoad")
                {
                    Destroy(obj);
                }
            }
        }

    }
}

