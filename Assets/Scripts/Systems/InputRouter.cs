using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Vagabond.Core;
using Vagabond.Systems.UI;
using UnityEngine.SceneManagement;

namespace Vagabond.Systems.Input {
    public class InputRouter : MonoBehaviour
    {

        [SerializeField] private PauseMenuController pauseMenuController;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            pauseMenuController = FindFirstObjectByType<PauseMenuController>();

        }

        private void Update()
        {
            if (GameManager.Instance.GameStateManager.CurrentState == GameState.InGame 
                || GameManager.Instance.GameStateManager.CurrentState == GameState.Paused)
            {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                    return;
                if(keyboard.escapeKey.wasPressedThisFrame)
                {
                    if (LevelChangeUI.Instance != null && LevelChangeUI.Instance.IsPanelActive)
                        return; // Don't allow pausing if level change panel is active

                    if (pauseMenuController.IsSettingsOpen)
                        pauseMenuController.OnCloseSettings();
                    else
                        TogglePause();
                }
            }
        }
            private void TogglePause() {
            var gsm = GameManager.Instance.GameStateManager;
            if (gsm.CurrentState == GameState.InGame)
            {
                Time.timeScale = 0f;
                gsm.ChangeState(GameState.Paused);
                MusicManager.Instance.PlayPauseSound();
            }
            else if (gsm.CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                gsm.ChangeState(GameState.InGame);
                MusicManager.Instance.PlayMusic();

            }
        }
    }
}

