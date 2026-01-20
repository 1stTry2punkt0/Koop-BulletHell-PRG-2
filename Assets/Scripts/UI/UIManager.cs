using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Elements")]
    [SerializeField] GameObject alphaBlock;
    private Coroutine hideAlphaBlock;

    [SerializeField] TMPro.TMP_Text scoreText;

    [SerializeField] GameObject bossUI;
    [SerializeField] GameObject bossHealthbar;

    [SerializeField] GameObject EXPBar;

    [Header("Health")]
    [SerializeField] Transform healthContainer;
    [SerializeField] Sprite health;
    [SerializeField] Sprite noHealth;
    [SerializeField] GameObject healthPrefab;
    private List<GameObject> healthIcons = new List<GameObject>();


    [Header("Rewards")]
    [SerializeField] GameObject rewardBar;
    private List<GameObject> rewardItems = new List<GameObject>();
    [SerializeField] GameObject rewardIconPrefab;
    [SerializeField] Sprite levelReward;
    [SerializeField] Sprite bossReward;

    [Header("Level Up Menu")]
    [SerializeField] GameObject levelUpMenu;
    [SerializeField] List<GameObject> levelUpOptions = new List<GameObject>();

    [Header("End Screen")]
    [SerializeField] GameObject endScreen;
    [SerializeField] TMPro.TMP_Text resultText;
    [SerializeField] Transform playerScores;
    [SerializeField] Transform Leaderboard;
    [SerializeField] GameObject playerScorePrefab;

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
        AddRewardItem(true);
        UpdateEXP(0.1f);
        ActivateBossUI("Your Father");
        UpdateBossHP(0.8f); 
        ActivateEndScreen(true);*/
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score.ToString();
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
            GameObject icon = Instantiate(healthPrefab, healthContainer);
            
            if (i < currentHealth)
            {
                icon.GetComponent<UnityEngine.UI.Image>().sprite = health;
            }
            else
            {
                icon.GetComponent<UnityEngine.UI.Image>().sprite = noHealth;
            }
            healthIcons.Add(icon);
        }
    }

    public void AddRewardItem(bool isBossReward)
    {
        GameObject rewardItem;
        if (isBossReward)
        {
            rewardItem = Instantiate(rewardIconPrefab, rewardBar.transform);
            rewardItem.GetComponent<UnityEngine.UI.Image>().sprite = bossReward;
        }
        else
        {
            rewardItem = Instantiate(rewardIconPrefab, rewardBar.transform);
            rewardItem.GetComponent<UnityEngine.UI.Image>().sprite = levelReward;
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
        bossUI.SetActive(true);
        bossUI.GetComponentInChildren<TMPro.TMP_Text>().text = name;
    }

    public void DisableBossUI()
    {
        bossHealthbar.SetActive(false);
    }

    public void UpdateBossHP(float perc)
    {
        bossHealthbar.GetComponentInChildren<UnityEngine.UI.Image>().fillAmount = perc;
    }

    public List<GameObject> ActivateLevelUpMenu()
    {
        levelUpMenu.SetActive(true);

        return levelUpOptions;
    }
    public void DeactivateLevelUpMenu()
    {
        levelUpMenu.SetActive(false);
    }

    public void ActivateEndScreen(bool isWin)
    {
        endScreen.SetActive(true);
        if (isWin)
        {
            resultText.text = "Victory";
        }
        else
        {
            resultText.text = "Defeated";
        }

        foreach (PlayerMovement player in PlayerTracker.Players)
        {
            GameObject score = Instantiate(playerScorePrefab, playerScores);
            score.GetComponent<TMPro.TMP_Text>().text = player.playerName + ": " + player.score.ToString();
        }

        //// Get top 4 players of the db
        //List<(string, int)> topPlayers = DatabaseManager.Instance.GetTopPlayers(4);
        //foreach (var (name, score) in topPlayers)
        //{
        //    GameObject scoreEntry = Instantiate(playerScorePrefab, Leaderboard);
        //    scoreEntry.GetComponent<TMPro.TMP_Text>().text = name + ": " + score.ToString();
        //}
        DatabaseManager.Instance.FetchTopScores(4, scores =>
        {
            foreach (var s in scores) {
                GameObject scoreEntry = Instantiate(playerScorePrefab, Leaderboard);
                scoreEntry.GetComponent<TMPro.TMP_Text>().text = $"{s.name}: {s.score}";
            }
        });

    }

    public void NewGame()
    {
        Debug.Log("New Game pressed");
    }

    public void BackToLobby()
    {
        Debug.Log("Back to Lobby pressed");
    }

    public void ShowAlphaBlock()
    {
        if (alphaBlock == null) return;
        if (hideAlphaBlock != null)
        {
            StopCoroutine(hideAlphaBlock);
            hideAlphaBlock = null;
        }
        alphaBlock.SetActive(true);
        hideAlphaBlock = StartCoroutine(HideAlphaBlock());
    }

    IEnumerator HideAlphaBlock()
    {
        yield return new WaitForSeconds(1.5f);
        alphaBlock.SetActive(false);
    }
}
