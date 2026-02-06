using UnityEngine;
using Vagabond.Systems.Scene;
using Vagabond.Systems.UI;
using Vagabond.Systems.Input;

namespace Vagabond.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private InputRouter inputRouter;

        public GameStateManager GameStateManager => gameStateManager;
        public SceneLoader SceneLoader => sceneLoader;
        public UIManager UIManager => uiManager;
        public InputRouter InputRouter => inputRouter;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[GameManager] Awake");
            Initialize();
        }

        private void Initialize()
        {
            if (gameStateManager == null)
                gameStateManager = GetComponent<GameStateManager>();

            if (sceneLoader == null)
                sceneLoader = GetComponent<SceneLoader>();

            if (uiManager == null)
                uiManager = GetComponent<UIManager>();

            if (inputRouter == null)
                inputRouter = GetComponent<InputRouter>();

            gameStateManager.Initialize();

            Debug.Log("[GameManager] All managers initialized");
        }


        private void Start()
        {
            gameStateManager.ChangeState(GameState.Boot);
            gameStateManager.ChangeState(GameState.MainMenu);
        }
    }
}



