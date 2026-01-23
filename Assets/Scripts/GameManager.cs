using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


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

    public void StartGame()
    {
        // Implement game start logic here
        LootManager.instance.ResetLootManager();
        UIManager.Instance.ResetUI();
        foreach (var player in PlayerTracker.Players)
        {
            player.GetComponent<PlayerActions>().resetStats();
        }
        WaveController waveController = FindFirstObjectByType<WaveController>();
        if (waveController != null)
        {
            waveController.StartWaves();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
