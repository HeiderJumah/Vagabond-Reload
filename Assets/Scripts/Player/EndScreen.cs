using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private GameObject endScreenPanel;
    [SerializeField] private GameObject GameOverPanel;
    [SerializeField] private GameObject VictoryPanel;

    public bool IsGameOverPanelActive => GameOverPanel.activeSelf;
    public bool IsVictoryPanelActive => VictoryPanel.activeSelf;


    public void OnBackToMain()
    {
        Time.timeScale = 1f;
        LevelState.ResetLevelState();
        DestoryAllPersistentObjects();
        SceneManager.LoadScene("MainMenu");

    }

    public void ActivatePanel(bool victory)
    { 
        endScreenPanel.SetActive(true);
        Time.timeScale = 0f;
        if (victory)
            ActivateVictoryPanel();
        else
            ActivateGameOverPanel();
    }

    public void ActivateGameOverPanel()
    {
        GameOverPanel.SetActive(true);
    }

    public void ActivateVictoryPanel()
    {
        VictoryPanel.SetActive(true);
    }

    public void OnClose()
    {
        if (IsGameOverPanelActive)
            GameOverPanel.SetActive(false);
        if(IsVictoryPanelActive)
            VictoryPanel.SetActive(false);
        endScreenPanel.SetActive(false);
        Time.timeScale = 1f;
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
