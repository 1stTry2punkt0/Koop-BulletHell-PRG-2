using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using FishNet.Object.Synchronizing;
using FishNet.Object;


public class LootManager : NetworkBehaviour
{
    public static LootManager instance;

    readonly public SyncVar<int> playerEXP = new SyncVar<int>(0);

    private int neededEXP = 20;
    private int playerLevel = 1;
    readonly public SyncVar<int> collectedLevelUps = new SyncVar<int>(0);
    private int shownLevelUps = 0;

    [SerializeField] List<LevelUpSO> levelUpSOs;

    private List<GameObject> levelUpOptionGOs = new List<GameObject>();

    private List<LevelUpType> optionTypes = new List<LevelUpType>();
    private List<float> optionAmounts = new List<float>();

    private bool isInLvlUp = false;

    public PlayerActions playerActions;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerEXP.OnChange += OnEXPChanged;
        collectedLevelUps.OnChange += OnCollectedLevelUpsChanged;
    }

    

    private void OnEXPChanged(int oldValue, int newValue, bool asServer)
    {
        float expPercent = (float)newValue / neededEXP;
        UIManager.Instance.UpdateEXP(expPercent);
    }

    private void OnCollectedLevelUpsChanged(int oldValue, int newValue, bool asServer)
    {
        if (newValue > shownLevelUps)
        {
            UIManager.Instance.AddRewardItem(false);
            shownLevelUps++;
        }
    }

    public void ResetLootManager()
    {
        playerEXP.Value = 0;
        neededEXP = 20;
        playerLevel = 1;
        collectedLevelUps.Value = 0;
        shownLevelUps = 0;
        optionTypes.Clear();
        optionAmounts.Clear();
        isInLvlUp = false;
    }

    [Server]
    public void AddEXP(int exp)
    {
        playerEXP.Value += exp;
        if (playerEXP.Value >= neededEXP)
        {
            LevelUp();
        }
    }

    [Server]
    private void LevelUp()
    {
        neededEXP += Mathf.RoundToInt(neededEXP * 0.2f);
        playerEXP.Value = 0;
        collectedLevelUps.Value++;
    }

    public void StartLevelUp()
    {
        //playerActions.IsLvling = true;
        levelUpOptionGOs = UIManager.Instance.ActivateLevelUpMenu();
        StartCoroutine(LevelUpRoutine());
    }

    private void ConsumeLevelUp()
    {
        collectedLevelUps.Value--;
        for (int i = 0; i < levelUpOptionGOs.Count; i++)
        {
            SetLvlUpOptions(i);
        }
    }

    private void SetLvlUpOptions(int optionIndex)
    {
        int randomIndex = Random.Range(0, levelUpSOs.Count);
        LevelUpSO selectedLevelUp = levelUpSOs[randomIndex];
        GameObject optionGO = levelUpOptionGOs[optionIndex];
        //Get Random Rarity
        LevelUpRarity randomValue;
        int rarityRoll = Random.Range(0, 100);
        if (rarityRoll < 50)
        {
            randomValue = LevelUpRarity.Common;
        }
        else if (rarityRoll < 80)
        {
            randomValue = LevelUpRarity.Rare;
        }
        else if (rarityRoll < 95)
        {
            randomValue = LevelUpRarity.Epic;
        }
        else
        {
            randomValue = LevelUpRarity.Legendary;
        }

        Color rarityColor;
        switch (randomValue)
        {
            case LevelUpRarity.Common:
                rarityColor = Color.white;
                break;
            case LevelUpRarity.Rare:
                rarityColor = Color.green;
                break;
            case LevelUpRarity.Epic:
                rarityColor = new Color(0f, 0.7f, 1f); ;
                break;
            case LevelUpRarity.Legendary:
                rarityColor = Color.magenta;
                break;
            default:
                rarityColor = Color.white;
                break;
        }
        optionGO.GetComponent<Image>().color = rarityColor;
        optionGO.GetComponentInChildren<TextMeshProUGUI>().text = selectedLevelUp.levelUpType.ToString();

        optionAmounts.Add(selectedLevelUp.GetEffectAmount(randomValue));
        optionTypes.Add(selectedLevelUp.levelUpType);

    }

    IEnumerator LevelUpRoutine()
    {
        Debug.Log("Starting Level Up");
        Debug.Log("Collected Level Ups: " + collectedLevelUps);
        while ( collectedLevelUps.Value > 0)
        {
            isInLvlUp = true;
            ConsumeLevelUp();
            Debug.Log("Waiting for Level Up Selection");
            yield return new WaitUntil(() => !isInLvlUp);
        }
        Debug.Log("Exiting Level Up Menu");
        UIManager.Instance.DeactivateLevelUpMenu();
        isInLvlUp = false;
        optionTypes.Clear();
        optionAmounts.Clear();
        //playerActions.IsLvling = false;
    }

    public void ApplyLvlUp(int optionIndex)
    {
        playerLevel++;
        switch (optionTypes[optionIndex])
        {
            case LevelUpType.MaxHP:
                Debug.Log("Increasing Max HP by " + optionAmounts[optionIndex]);
                for (int i = 0; i < (int)optionAmounts[optionIndex]; i++)
                {
                    playerActions.IncreaseMaxHP();
                }
                break;
            case LevelUpType.DMG:
                Debug.Log("Increasing DMG by " + optionAmounts[optionIndex]);
                playerActions.IncreaseDmg(optionAmounts[optionIndex]);
                break;
            case LevelUpType.Speed:
                Debug.Log("Increasing Move Speed by " + optionAmounts[optionIndex]);
                playerActions.IncreaseMoveSpeed(optionAmounts[optionIndex]);
                break;
            case LevelUpType.AS:
                Debug.Log("Increasing Attack Speed by " + optionAmounts[optionIndex]);
                playerActions.IncreaseAttackSpeed(optionAmounts[optionIndex]);
                break;
            case LevelUpType.CritRate:
                Debug.Log("Increasing Crit Rate by " + optionAmounts[optionIndex]);
                playerActions.IncreaseCritRate(optionAmounts[optionIndex]);
                break;
            case LevelUpType.CritDMG:
                Debug.Log("Increasing Crit DMG by " + optionAmounts[optionIndex]);
                playerActions.IncreaseCritDmg(optionAmounts[optionIndex]);
                break;
            case LevelUpType.Range:
                Debug.Log("Increasing Range by " + optionAmounts[optionIndex]);
                playerActions.IncreaseRange(optionAmounts[optionIndex]);
                break;
            case LevelUpType.Heal:
                Debug.Log("Healing by " + optionAmounts[optionIndex]);
                playerActions.HealOnServer(Mathf.RoundToInt(optionAmounts[optionIndex]));
                break;
        }
        UIManager.Instance.RemoveRewardItem();
        isInLvlUp = false;
    }

    public void ChestCollected()
    {
        //lootUi.SetActive(true);
        //Time.timeScale = 0f;
    }
}

public enum LevelUpType
{
    MaxHP,
    DMG,
    Speed,
    AS,
    CritRate,
    CritDMG,
    Range,
    Heal
}

public enum  LevelUpRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}