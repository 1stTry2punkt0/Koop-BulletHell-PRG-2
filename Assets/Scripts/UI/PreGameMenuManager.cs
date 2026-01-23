using FishNet.Object;
using UnityEngine;

public class PreGameMenuManager : NetworkBehaviour
{
    [SerializeField] private GameObject preGameMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject lobbyMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterLobby()
    {
        mainMenu.SetActive(false);
        lobbyMenu.SetActive(true);
    }

    public void ExitLobby()
    {
        lobbyMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    [ObserversRpc]
    public void EnterGame()
    {
        preGameMenu.SetActive(false);
    }
}
