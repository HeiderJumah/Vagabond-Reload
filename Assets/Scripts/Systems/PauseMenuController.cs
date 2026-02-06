using UnityEngine;
using Vagabond.Core;

namespace Vagabond.Systems.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        public void OnResume()
        {
            Debug.Log("[PauseMenu] Resume");
            Time.timeScale = 1f;
            GameManager.Instance.GameStateManager.ChangeState(GameState.InGame);
        }

        public void OnBackToMainMenu()
        {
            Debug.Log("[PauseMenu] Back to Main Menu");
            Time.timeScale = 1f;
            GameManager.Instance.GameStateManager.ChangeState(GameState.MainMenu);
        }
    }
}

