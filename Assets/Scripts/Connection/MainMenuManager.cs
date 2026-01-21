using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    public void OnHostClicked() 
    {
        LobbyManager.Instance.CreateLobby();
        ConnectionManager.Instance.StartHost(); 
    }
    public void OnJoinClicked() 
    {
        LobbyManager.Instance.JoinFirstAvailableLobby(); 
    }
}
