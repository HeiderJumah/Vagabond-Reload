using UnityEngine;
using Vagabond.Core;

namespace Vagabond.Systems.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public void OnStartGame()
        {
            //Debug.Log("[TitleScreen] Start Game");
            //GameManager.Instance.GameStateManager.ChangeState(GameState.Loading);

            Debug.Log("[TitleScreen] Start Game");
            GameManager.Instance.GameStateManager.ChangeState(GameState.InGame);
        }

        public void OnQuitGame()
        {
            Debug.Log("[TitleScreen] Quit Game");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
