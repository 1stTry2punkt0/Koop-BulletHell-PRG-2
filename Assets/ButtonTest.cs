using UnityEngine;
using FishNet.Managing;
using FishNet;

public class ButtonTest : MonoBehaviour
{
    public void StartHost()
    {
        var nm = InstanceFinder.NetworkManager;
        nm.ServerManager.StartConnection();
        nm.ClientManager.StartConnection();
    }

    public void StartClient()
    {
        var nm = InstanceFinder.NetworkManager;
        nm.ClientManager.StartConnection();
    }

    public void StartServer()
    {
        var nm = InstanceFinder.NetworkManager;
        nm.ServerManager.StartConnection();
    }
}
