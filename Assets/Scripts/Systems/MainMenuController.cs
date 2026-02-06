using UnityEngine;
using Vagabond.Core;

namespace Vagabond.Systems.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Cursor Settings")]
        [SerializeField] private Texture2D customCursor;
        [SerializeField] private Vector2 cursorHotspot = Vector2.zero; // center of the texture

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Confined;

            if (customCursor != null)
                Cursor.SetCursor(customCursor, cursorHotspot, CursorMode.Auto);
        }

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
