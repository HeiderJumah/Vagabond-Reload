using UnityEngine;
using UnityEngine.UI;

public class StaminaUI : MonoBehaviour
{
    [Header("Stamina Images")]
    [SerializeField] private Image emptySprite;
    [SerializeField] private Image staminaSprit;

    // fix size on runtime
    private Vector2 staminaSize = new Vector2(50, 50);

    public void StaminaInit()
    {
        emptySprite.rectTransform.sizeDelta = staminaSize;
        staminaSprit.rectTransform.sizeDelta = staminaSize;
    }

    public void SetStamina(bool isFilled)
    {
        if(staminaSprit != null)
            staminaSprit.gameObject.SetActive(isFilled);
    }
}
