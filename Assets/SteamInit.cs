using UnityEngine;
using Steamworks;

public class SteamInit : MonoBehaviour
{
    void Start()
    {
        if (!SteamAPI.Init())
        {
            Debug.LogError("Steam init failed!");
        }
        else
        {
            Debug.Log("Steam initialized. User: " + SteamFriends.GetPersonaName());
        }
    }

    void Update()
    {
        SteamAPI.RunCallbacks();
    }

    void OnDestroy()
    {
        SteamAPI.Shutdown();
    }
}
