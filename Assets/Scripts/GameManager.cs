using FishNet.Object;
using UnityEngine;

public class GameManager : NetworkBehaviour
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
        ObserverStartGame();
        foreach (var player in PlayerTracker.Players)
        {
            player.GetComponent<PlayerActions>().resetStats();
        }
        WaveController waveController = FindFirstObjectByType<WaveController>();
        if (waveController != null)
        {
            waveController.StartWaves();
        }

        //Get all exp objects and delete them
        var allExp = FindObjectsOfType<EXP>();

        foreach (var exp in allExp)
        {
            exp.NetworkObject.Despawn();
        }

    }
    [ObserversRpc]
    public void ObserverStartGame()
    {
        UIManager.Instance.ResetUI();
    }

}
