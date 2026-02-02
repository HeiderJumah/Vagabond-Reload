using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{

    private PlayerActions playerActions;
    [SerializeField] private HeartUi heartPrefab;
    [SerializeField] private Transform heartContainer;

    private List<HeartUi> hearts = new List<HeartUi>();
    private int maxHearts => Mathf.CeilToInt(playerActions.GetMaxHealth());

    void Start()
    {
        playerActions = FindFirstObjectByType<PlayerActions>();
        CreateHearts();
        UpdateHearts(playerActions.GetCurrentHealth());

        // subscribe to players health change 
        playerActions.OnHealthChanged += UpdateHearts;
    }

    private void CreateHearts()
    {
        for (int i = 0; i < maxHearts; i++)
        {
            HeartUi heart = Instantiate(heartPrefab, heartContainer);
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

    private void OnDestroy()
    {
        // unsubcribe 
        if (playerActions != null) 
            playerActions.OnHealthChanged -= UpdateHearts;
    }

}
