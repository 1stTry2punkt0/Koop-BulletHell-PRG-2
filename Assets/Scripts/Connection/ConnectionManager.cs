using UnityEngine;
using FishNet.Managing;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    [Header("References")]
    public NetworkManager networkManager;

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

    // HOST: Server + Client starten
    public void StartHost()
    {
        Debug.Log("Starting Host (Server + Client)...");
        networkManager.ServerManager.StartConnection();
        networkManager.ClientManager.StartConnection();
    }

    // CLIENT: Verbindung zum Host
    public void ConnectToHost(ulong hostSteamId)
    {
        Debug.Log("Connecting to Host with SteamID: " + hostSteamId);

        // Für später: Steam-P2P Transport hier einbinden
        // Aktuell: Lokale Verbindung (für Tests)
        networkManager.ClientManager.StartConnection();
    }
}
