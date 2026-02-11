using UnityEngine;

public class BarrierTrigger : MonoBehaviour
{
    [SerializeField] private GameObject barrier;

    private void OnTriggerEnter(Collider other)
    {
        PlayerActions player = other.GetComponent<PlayerActions>();
        if(player != null)
            barrier.gameObject.SetActive(true);
    }
}
