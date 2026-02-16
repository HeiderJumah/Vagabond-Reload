using UnityEngine;

public class BarrierTrigger : MonoBehaviour
{
    [SerializeField] private GameObject barrier;
    [SerializeField] private GameObject boss;

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions player = other.GetComponent<PlayerActions>();
        if(player != null)
            barrier.gameObject.SetActive(true);
            boss.gameObject.SetActive(true);
    }
}
