using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    [SerializeField] GameObject AlphaBlock;
    private Coroutine hideAlphaBlock;

    [SerializeField] TMPro.TMP_Text ScoreText;

    [SerializeField] GameObject BossUI;
    [SerializeField] GameObject BossHealthbar;

    [SerializeField] GameObject EXPBar;

    [Header("Health")]
    [SerializeField] Transform HealthContainer;
    [SerializeField] Sprite Health;
    [SerializeField] Sprite NoHealth;
    [SerializeField] GameObject HealthPrefab;
    private List<GameObject> healthIcons = new List<GameObject>();


    [Header("Rewards")]
    [SerializeField] GameObject RewardBar;
    private List<GameObject> rewardItems = new List<GameObject>();
    [SerializeField] GameObject BossRewardPrefab;
    [SerializeField] GameObject LevelRewardPrefab;

    [Header("Level Up Menu")]
    [SerializeField] GameObject LevelUpMenu;
    [SerializeField] List<GameObject> levelUpOptions = new List<GameObject>();
     
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
      /*  UpdateScore(0);
        UpdateHealth(2, 3);
        AddRewardItem(false);
        AddRewardItem(false);
        UpdateEXP(0.1f);
        ActivateBossUI("Your Father");
        UpdateBossHP(0.8f); */
    }

    public void UpdateScore(int score)
    {
        ScoreText.text = "Score: " + score.ToString();
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        // Clear existing health icons
        foreach (var icon in healthIcons)
        {
            Destroy(icon);
        }
        healthIcons.Clear();
        // Create new health icons
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject icon = Instantiate(HealthPrefab, HealthContainer);
            
            if (i < currentHealth)
            {
                icon.GetComponent<UnityEngine.UI.Image>().sprite = Health;
            }
            else
            {
                icon.GetComponent<UnityEngine.UI.Image>().sprite = NoHealth;
            }
            healthIcons.Add(icon);
        }
    }

    public void AddRewardItem(bool isBossReward)
    {
        GameObject rewardItem;
        if (isBossReward)
        {
            rewardItem = Instantiate(BossRewardPrefab, RewardBar.transform);
        }
        else
        {
            rewardItem = Instantiate(LevelRewardPrefab, RewardBar.transform);
        }
        rewardItems.Add(rewardItem);
    }

    public void RemoveRewardItem()
    {
        if (rewardItems.Count == 0) return;
        GameObject rewardItem = rewardItems[0];
        rewardItems.RemoveAt(0);
        Destroy(rewardItem);
    }

    public void UpdateEXP(float progress)
    {
        // Update EXP bar logic here
        EXPBar.GetComponent<UnityEngine.UI.Image>().fillAmount = progress;
    }

    public void ActivateBossUI(string name)
    {
        BossUI.SetActive(true);
        BossUI.GetComponentInChildren<TMPro.TMP_Text>().text = name;
    }

    public void DisableBossUI()
    {
        BossHealthbar.SetActive(false);
    }

    public void UpdateBossHP(float perc)
    {
        BossHealthbar.GetComponentInChildren<UnityEngine.UI.Image>().fillAmount = perc;
    }

    public List<GameObject> ActivateLevelUpMenu()
    {
        LevelUpMenu.SetActive(true);

        return levelUpOptions;
    }
    public void DeactivateLevelUpMenu()
    {
        LevelUpMenu.SetActive(false);
    }

    public void ShowAlphaBlock()
    {
        if (AlphaBlock == null) return;
        if (hideAlphaBlock != null)
        {
            StopCoroutine(hideAlphaBlock);
            hideAlphaBlock = null;
        }
        AlphaBlock.SetActive(true);
        hideAlphaBlock = StartCoroutine(HideAlphaBlock());
    }

    IEnumerator HideAlphaBlock()
    {
        yield return new WaitForSeconds(1.5f);
        AlphaBlock.SetActive(false);
    }
}
