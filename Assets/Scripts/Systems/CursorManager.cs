using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D customCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero; // center of the texture

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Set cursor settings immediately on Awake to ensure it's applied in all scenes
        Cursor.lockState = CursorLockMode.Confined;

        if (customCursor != null)
            Cursor.SetCursor(customCursor, cursorHotspot, CursorMode.Auto);

    }

}
