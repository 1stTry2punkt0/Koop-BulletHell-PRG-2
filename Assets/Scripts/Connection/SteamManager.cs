using Steamworks;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public uint appId = 480; // Test AppID

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SteamClient.Init(appId);
    }

    void Start()
    {
        Debug.Log("SteamTest Start() wurde ausgeführt");
        Debug.Log("SteamID: " + SteamClient.SteamId);
    }


    void Update()
    {
        SteamClient.RunCallbacks();
    }

    void OnDestroy()
    {
        SteamClient.Shutdown();
    }
}
