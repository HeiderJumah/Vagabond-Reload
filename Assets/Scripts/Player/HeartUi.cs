using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HeartUi : MonoBehaviour
{
    [Header("HeartImages")]
    [SerializeField] private Image empty;
    [SerializeField] private Image red;
    [SerializeField] private Image damageFlash;

    [SerializeField] private float flashTime = 0.1f;

    public void SetHeart(float value)
    {
        value = Mathf.Clamp01(value);

        red.fillAmount = value;
        damageFlash.fillAmount = value;
        damageFlash.gameObject.SetActive(false);
    }

    public void DamageFlash()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        damageFlash.gameObject.SetActive(true);
        yield return new WaitForSeconds(flashTime);
        damageFlash.gameObject.SetActive(false);
    }
}
