using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Vagabond.Core;

namespace Vagabond.Systems.Input {
    public class InputRouter : MonoBehaviour
    {
        private void Update()
        {
            if (GameManager.Instance.GameStateManager.CurrentState == GameState.InGame)
            {
                var keyboard = Keyboard.current;
                if (keyboard == null)
                    return;
                if(keyboard.tabKey.wasPressedThisFrame)
                {
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
            }
            else if (gsm.CurrentState == GameState.Paused)
            {
                Time.timeScale = 1f;
                gsm.ChangeState(GameState.InGame);
            }
        }
    }
}

