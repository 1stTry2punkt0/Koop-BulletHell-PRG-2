using Steamworks;
using Steamworks.Data;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance;

    public Lobby? CurrentLobby;
    public ulong HostSteamId;

    void Awake()
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

    // HOST: Lobby erstellen
    public async void CreateLobby()
    {
        Debug.Log("Creating Lobby...");

        var lobby = await SteamMatchmaking.CreateLobbyAsync(2);

        if (lobby == null)
        {
            Debug.LogError("Lobby creation failed");
            return;
        }

        CurrentLobby = lobby.Value;

        HostSteamId = SteamClient.SteamId.Value;

        // WICHTIG: .Value verwenden
        CurrentLobby.Value.SetData("HostID", HostSteamId.ToString());


        Debug.Log("Lobby created. HostID: " + HostSteamId);
    }

    // CLIENT: Lobby beitreten
    public async void JoinFirstAvailableLobby()
    {
        Debug.Log("Searching for lobbies...");

        var list = await SteamMatchmaking.LobbyList
            .WithSlotsAvailable(1)
            .RequestAsync();

        if (list == null || list.Length == 0)
        {
            Debug.LogWarning("No lobbies found");
            return;
        }

        var lobby = list[0];
        CurrentLobby = lobby;

        Debug.Log("Joining lobby: " + lobby.Id);

        // HostID aus Lobby lesen
        string hostIdString = lobby.GetData("HostID");

        if (!ulong.TryParse(hostIdString, out HostSteamId))
        {
            Debug.LogError($"Ungültige HostSteamId: '{hostIdString}'");
            return;
        }

        Debug.Log("HostID from lobby: " + HostSteamId);

        // Verbindung starten
        ConnectionManager.Instance.ConnectToHost(HostSteamId);
    }
}
