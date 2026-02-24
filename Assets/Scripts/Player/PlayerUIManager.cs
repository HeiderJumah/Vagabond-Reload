using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    public static PlayerUIManager Instance;

    private PlayerActions playerActions;
    private PlayerMovement playerMovement;
    [SerializeField] private HeartUi heartPrefab;
    [SerializeField] private Transform heartContainer;
    [SerializeField] private StaminaUI staminaPrefab;
    [SerializeField] private Transform staminaContainer;

    private List<HeartUi> hearts = new List<HeartUi>();
    private int maxHearts => Mathf.CeilToInt(playerActions.GetMaxHealth());

    private List<StaminaUI> staminas = new List<StaminaUI>();
    private int maxStamina => Mathf.CeilToInt(playerActions.GetMaxStamina());

    [Header("Status Effects")]
    [SerializeField] private GameObject burnObject;
    [SerializeField] private GameObject slowObject;
    [SerializeField] private GameObject confusedObject;
    [SerializeField] private GameObject paralizedObject;
    [SerializeField] private GameObject poisonedObject;

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
    }

    void Start()
    {
        playerActions = FindFirstObjectByType<PlayerActions>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        CreateHearts();
        UpdateHearts(playerActions.GetCurrentHealth());
        CreateStamina();
        // subscribe to players health change and stamina change events
        playerActions.OnHealthChanged += UpdateHearts;
        playerActions.OnStaminaChanged += UpdateStaminaUI;
        // subscribe to player status events 
        playerActions.OnBurnStatusChanged += SetBurnStatus;
        playerMovement.OnSlowedStatusChanged += SetSlowedStatus;
        playerMovement.OnConfusedStatusChanged += SetConfusedStatus;
        playerMovement.OnParalyzedStatusChanged += SetParalizedStatus;
        playerActions.OnPoisonedStatusChanged += SetPoisonedStatus;
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

    private void CreateStamina()
    {
        for (int i = 0; i < maxStamina; i++)
        {
            StaminaUI stamina = Instantiate(staminaPrefab, staminaContainer);
            stamina.StaminaInit();
            staminas.Add(stamina);
        }
    }

    private void UpdateStaminaUI(float stamina)
    {
        for (int i = 0; i < staminas.Count; i++)
        {
            bool isFilled = i < stamina;
            staminas[i].SetStamina(isFilled);
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
    private void SetPoisonedStatus(bool poisoned)
    {
        if(poisonedObject != null)
            poisonedObject.SetActive(poisoned);
    }

    private void OnDestroy()
    {
        // unsubcribe 
        if (playerActions != null)
        {
            playerActions.OnHealthChanged -= UpdateHearts;
            playerActions.OnStaminaChanged -= UpdateStaminaUI;
            playerActions.OnBurnStatusChanged -= SetBurnStatus;
            playerMovement.OnSlowedStatusChanged -= SetSlowedStatus;
            playerMovement.OnConfusedStatusChanged -= SetConfusedStatus;
            playerMovement.OnParalyzedStatusChanged -= SetParalizedStatus;
        }
    }

}
