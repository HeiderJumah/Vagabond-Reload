using UnityEngine;
using UnityEngine.SceneManagement;

public class BarrierTrigger : MonoBehaviour
{
    [SerializeField] private GameObject barrier;
    [SerializeField] private GameObject boss;

    private bool hasTriggered = false;

    private void Start()
    {
        if(LevelState.IsBossDefeated(SceneManager.GetActiveScene().name))
        {
            if(barrier != null)
                barrier.gameObject.SetActive(false);
            if(boss != null)
                boss.gameObject.SetActive(false);
            hasTriggered = true;
        }
        else
        {
            if (barrier != null)
                barrier.gameObject.SetActive(false);
            if (boss != null)
                boss.gameObject.SetActive(false);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        PlayerActions player = other.GetComponent<PlayerActions>();
        if (player != null)
        {
            hasTriggered = true;

            barrier.gameObject.SetActive(true);
            boss.gameObject.SetActive(true);

            MusicManager.Instance.PlayBossMusic();
        }
    }

    public void OnBossDeafed()
    {
        LevelState.SetBossDefeated(SceneManager.GetActiveScene().name);
        if(barrier != null)
            barrier.gameObject.SetActive(false);
    }
}
