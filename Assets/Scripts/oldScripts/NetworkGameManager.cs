using UnityEngine;
using FishNet.Object;
using TMPro;
using UnityEngine.UI;
using FishNet.Object.Synchronizing;
using System.Linq;
using FishNet.Connection;

public class NetworkGameManager : NetworkBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] TMP_Text stateText;
    [SerializeField] TMP_Text player1NameText;
    [SerializeField] TMP_Text player2NameText;
    [SerializeField] TMP_InputField PlayerNameField;
    [SerializeField] Button ReadyButton;

    public readonly SyncVar<string> Player1 = new SyncVar<string>();
    public readonly SyncVar<string> Player2 = new SyncVar<string>();

    [Header("Score")]
    private readonly SyncVar<int> scoreP1 = new SyncVar<int>();
    private readonly SyncVar<int> scoreP2 = new SyncVar<int>();

    [Header("Game")]
    private readonly SyncVar<GameState> currentState = new SyncVar<GameState>();
    public GameState CurrentState => currentState.Value;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentState.OnChange += OnStateChanged;
        scoreP1.OnChange += (oldValue, newValue, asServer) => UpdateStateText();
        scoreP2.OnChange += (oldValue, newValue, asServer) => UpdateStateText();

        Player1.OnChange += (oldValue, newValue, asServer) =>
        {
            if (player1NameText != null)
            {
                player1NameText.text = newValue;
            }
        };
        Player2.OnChange += (oldValue, newValue, asServer) =>
        {
            if (player2NameText != null)
            {
                player2NameText.text = newValue;
            }
        };
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        currentState.Value = GameState.WaitingForPlayers; // <---------------------------- Mark Gamestatesetting
        scoreP1.Value = 0;
        scoreP2.Value = 0;
        Debug.Log("Server started GameManager");
    }

    #region StartHandling
    [Server]
    public void ChackAndStartGame()//Sucht alle spieler und startet das spiel wenn alle bereit sind
    {
        if (CurrentState != GameState.WaitingForPlayers) return;

        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        if (players.Length >= 2 && players.All(p => p.IsReady))
        {
            currentState.Value = GameState.Playing;         // <---------------------------- Mark Gamestatesetting
            StartCoroutine(BallSpawner.Instance.SpawnBall(2f));
        } else if (players.Length == 1)
        {
            stateText.text = "Such dir Freunde, du bist allein!";
        }
    }

    public void CheckForPlayers()
    {
        var players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        if (players.Length == 1)
        {
            stateText.text = "Such dir Freunde, du bist allein!";
        }
        if (players.Length >= 2)
        {
            stateText.text = "Waiting for Players...";
        }

        Debug.Log($"Players connected: {players.Length}");
    }

    public void SetPlayerReady()
    {
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            if (player.IsOwner)
            {
                if (!player.IsReady) { 
                    ReadyButton.image.color = Color.green;
                    ReadyButton.GetComponentInChildren<TMP_Text>().text = "Unready"; 

                }
                else { ReadyButton.image.color = Color.white; ReadyButton.GetComponentInChildren<TMP_Text>().text = "Ready"; }
                player.SetReadyStateServerRpc(PlayerNameField.text);
            }
        }
    }

    [TargetRpc]
    public void DisableNameField(NetworkConnection con, bool isOff)
    {
        PlayerNameField.gameObject.SetActive(!isOff);
    }

    private void OnStateChanged(GameState oldState, GameState newState, bool asServer)
    {
        UpdateStateText();
    }

    private void UpdateStateText()
    {
        if (stateText == null) return;

        switch (currentState.Value)
        {
            case GameState.WaitingForPlayers:
                stateText.text = "Waiting for Players...";
                break;
            case GameState.Playing:
                stateText.text = $"Scroe: {scoreP1.Value} | {scoreP2.Value}";
                break;
            case GameState.Finished:
                stateText.text = "Finished";
                break;

        }
    }
    #endregion

    #region Scoring
    [Server]
    public void ScorePoint(int playerIndex)
    {
        if (currentState.Value != GameState.Playing) return;
        if (playerIndex == 0)
            scoreP1.Value++;
        else if (playerIndex == 1)
            scoreP2.Value++;

        if (scoreP1.Value >= 10 || scoreP2.Value >= 10)
        {
            currentState.Value = GameState.Finished;            // <---------------------------- Mark Gamestatesetting
        }
        else
        {
            StartCoroutine(BallSpawner.Instance.SpawnBall(4f));
        }
    }
    #endregion
}

public enum GameState
{
    WaitingForPlayers,
    Playing,
    Finished
}

/*
 SyncVars:
NetworkGameManager
- Player1 : string
- Player2 : string
- scoreP1 : int
- scoreP2 : int
- currentState : GameState

PlayerController (Pro Instanz von Player)
- PlayerColor : Color
- IsReady : bool

Halten den Wert der Variablen zwischen Server und Clients synchron.
 */