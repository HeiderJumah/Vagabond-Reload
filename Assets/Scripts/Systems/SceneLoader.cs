using UnityEngine;
using UnityEngine.SceneManagement;

namespace Vagabond.Systems.Scene
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private string sceneName;
        [SerializeField] private Transform spawnPoint;

        public LevelManager LevelManager => levelManager;
        public string SceneName => sceneName;

        private void Start()
        {
            if (levelManager == LevelManager.LevelConnection)
            {
                FindFirstObjectByType<PlayerActions>().transform.position = spawnPoint.position;
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerActions player = other.GetComponent<PlayerActions>();
            if (player != null && player.IsAlive)
            {
               /* LevelManager.LevelConnection = levelManager;
                SceneManager.LoadScene(sceneName);*/
               LevelChangeUI.Instance.ShowLevelChangePanel(this);
            }
        }
    }
}

