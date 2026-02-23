using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Vagabond.Core;
using Vagabond.Systems.UI;

namespace Vagabond.Systems.Input {
    public class InputRouter : MonoBehaviour
    {

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
                    Cursor.lockState = Cursor.lockState != CursorLockMode.None ? CursorLockMode.None : CursorLockMode.Confined;
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

