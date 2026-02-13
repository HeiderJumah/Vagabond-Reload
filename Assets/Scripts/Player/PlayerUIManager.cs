using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{

    private PlayerActions playerActions;
    private PlayerMovement playerMovement;
    [SerializeField] private HeartUi heartPrefab;
    [SerializeField] private Transform heartContainer;

    private List<HeartUi> hearts = new List<HeartUi>();
    private int maxHearts => Mathf.CeilToInt(playerActions.GetMaxHealth());

    [Header("Status Effects")]
    [SerializeField] private GameObject burnObject;
    [SerializeField] private GameObject slowObject;
    [SerializeField] private GameObject confusedObject;
    [SerializeField] private GameObject paralizedObject;

    void Start()
    {
        playerActions = FindFirstObjectByType<PlayerActions>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        CreateHearts();
        UpdateHearts(playerActions.GetCurrentHealth());

        // subscribe to players health change 
        playerActions.OnHealthChanged += UpdateHearts;
        // subscribe to player status events 
        playerActions.OnBurnStatusChanged += SetBurnStatus;
        playerMovement.OnSlowedStatusChanged += SetSlowedStatus;
        playerMovement.OnConfusedStatusChanged += SetConfusedStatus;
        playerMovement.OnParalyzedStatusChanged += SetParalizedStatus;
    }

    private void CreateHearts()
    {
        for (int i = 0; i < maxHearts; i++)
        {
            HeartUi heart = Instantiate(heartPrefab, heartContainer);
            heart.HeartInit();
            hearts.Add(heart);
        }
    }

    public void UpdateHearts(float health)
    {
        for (int i = 0;i < hearts.Count; i++)
        {
            float heartValue = Mathf.Clamp(health - i, 0f, 1f);
            hearts[i].SetHeart(heartValue);
        }

        int damageHeart = Mathf.FloorToInt(health);
        if (damageHeart < hearts.Count)
        {
            hearts[damageHeart].DamageFlash();
        }
    }

    private void SetBurnStatus(bool burn)
    {
        if(burnObject != null)
            burnObject.SetActive(burn);
    }

    private void SetSlowedStatus(bool slow)
    {
        if(slowObject != null)
            slowObject.SetActive(slow);
    }

    private void SetConfusedStatus(bool confused)
    { 
        if(confusedObject != null)
            confusedObject.SetActive(confused);
    }
    private void SetParalizedStatus(bool paralized)
    {
        if (paralizedObject != null)
            paralizedObject.SetActive(paralized);
    }

    private void OnDestroy()
    {
        // unsubcribe 
        if (playerActions != null)
        {
            playerActions.OnHealthChanged -= UpdateHearts;
            playerActions.OnBurnStatusChanged -= SetBurnStatus;
            playerMovement.OnSlowedStatusChanged -= SetSlowedStatus;
            playerMovement.OnConfusedStatusChanged -= SetConfusedStatus;
            playerMovement.OnParalyzedStatusChanged -= SetParalizedStatus;
        }
    }

}
