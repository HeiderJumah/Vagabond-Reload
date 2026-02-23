using UnityEngine;
using Vagabond.Core;
using UnityEngine.SceneManagement;

namespace Vagabond.Systems.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string LevelToLoad;
        [SerializeField] private LevelManager levelManager;

        public void OnStartGame()
        {
            //Debug.Log("[TitleScreen] Start Game");
            //GameManager.Instance.GameStateManager.ChangeState(GameState.Loading);

            Debug.Log("[TitleScreen] Start Game");
            GameManager.Instance.GameStateManager.ChangeState(GameState.InGame);
            SceneManager.LoadScene(LevelToLoad);
            LevelManager.LevelConnection = levelManager;
            MusicManager.Instance.PlayMusic();
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
