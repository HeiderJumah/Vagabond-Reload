using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject GameOverPanel;

    public bool IsGameOverPanelActive => GameOverPanel.activeSelf;


    public void OnBackToMain()
    {
        Time.timeScale = 1f;
        DestoryAllPersistentObjects();
        SceneManager.LoadScene("MainMenu");

    }

    public void ActivateGameOverPanel()
    {
        GameOverPanel.SetActive(true);
    }

    public void OnClose()
    {
        GameOverPanel.SetActive(false);
    }

    private void DestoryAllPersistentObjects()
    {
        var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {

            if (obj.scene.name == "DontDestroyOnLoad")
            {
                Destroy(obj);
            }
        }
    }


}
