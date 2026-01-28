using UnityEngine;

namespace Vagabond.Core
{
    public class Bootstrap : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager missing in scene!");
            }
        }
    }
}

