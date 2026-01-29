using UnityEngine;
using Vagabond.Core;

namespace Vagabond.Systems.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Screens")]
        [SerializeField] private GameObject mainMenuUI;
        [SerializeField] private GameObject pauseMenuUI;

        private void Start()
        {
            GameManager.Instance.GameStateManager.OnStateChanged += HandleGameStateChanged;

            // Initialen State sofort anwenden
            HandleGameStateChanged(
                GameManager.Instance.GameStateManager.CurrentState
            );
        }


        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameStateManager.OnStateChanged -= HandleGameStateChanged;
        }

        private void HandleGameStateChanged(GameState state)
        {
            HideAll();

            switch (state)
            {
                case GameState.MainMenu:
                    if (mainMenuUI != null)
                        mainMenuUI.SetActive(true);
                    break;

                case GameState.Paused:
                    if (pauseMenuUI != null)
                        pauseMenuUI.SetActive(true);
                    break;
            }
        }

        private void HideAll()
        {
            if (mainMenuUI != null)
                mainMenuUI.SetActive(false);

            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
        }
    }
}


