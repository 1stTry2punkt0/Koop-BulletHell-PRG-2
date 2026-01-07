using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine.InputSystem;
using System.Collections;
using FishNet.Demo.AdditiveScenes;
using Unity.Collections.LowLevel.Unsafe;

public class PlayerController : NetworkBehaviour
{
    private readonly SyncVar<Color> playerColor = new SyncVar<Color>();
    private readonly SyncVar<bool> isReady = new SyncVar<bool>();

    public bool IsReady => isReady.Value;

    private Renderer playerRenderer;
    public int playerIndex;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float minY = -4f;
    [SerializeField] float maxY = 4f;

    [Header("Input System")]
    [SerializeField] InputAction moveAction;
    [SerializeField] InputAction colorChangeAction;

    #region Init
    private void OnDisable()
    {
        playerColor.OnChange -= OnColorChanged;
        if (!IsOwner) return;

        moveAction?.Disable();
        colorChangeAction?.Disable();
        if (TimeManager != null)
        {
            TimeManager.OnTick -= OnTick;
        }
    }

    private void Start()
    {
        StartCoroutine(DelayedIsOwner());
    }

    private IEnumerator DelayedIsOwner()
    {
        playerColor.OnChange += OnColorChanged;
        playerIndex = OwnerId + 1;
        GetComponentInChildren<TMPro.TMP_Text>().text = $"Ich bin Player {playerIndex}";
        playerRenderer = GetComponentInChildren<Renderer>();
        playerRenderer.material = new Material(playerRenderer.material);
        playerRenderer.material.color = playerIndex == 1? Color.blue:Color.red;


        yield return null; //Wait a frame to ensure IsOwner is set correctly
        if (IsOwner)
        {
            //ChangeColor(Random.value, Random.value, Random.value);

            moveAction?.Enable();
            colorChangeAction?.Enable();
            if (TimeManager != null)
            {
                TimeManager.OnTick += OnTick;
            }
        }
        NetworkGameManager.Instance.CheckForPlayers();
    }
    #endregion

    private void OnTick()
    {
        if (!IsOwner) return;
        if (isReady.Value)
        {
            HandleInput();
        }
        else
        {
            CheckForChangeColor();
        }
    }

    #region ReadyStateHandling
    [ServerRpc]
    public void SetReadyStateServerRpc(string name)
    {
        isReady.Value = !isReady.Value;

        if(transform.position.x < 0)
        {
            if(IsReady) NetworkGameManager.Instance.Player1.Value = name + " -Ist dabei";
            if(!IsReady) NetworkGameManager.Instance.Player1.Value = name + " -Nicht dabei";
        }
        else
        {
            if(IsReady) NetworkGameManager.Instance.Player2.Value = name + " -Ist dabei";
            if(!IsReady) NetworkGameManager.Instance.Player2.Value = name + " -Nicht dabei";
        }

        NetworkGameManager.Instance.DisableNameField(Owner, isReady.Value);
        NetworkGameManager.Instance.ChackAndStartGame();
    }

    private void CmdSetReady(bool ready)
    {
        Debug.Log($"Player {playerIndex} ready: {ready}");
    }
    #endregion

    #region Movement 
    private void HandleInput()
    {
        float input= moveAction.ReadValue<float>();
        Move(input);
    }

    [ServerRpc]
    private void Move(float input)
    {
        float newY = transform.position.y + input * moveSpeed  * (float)TimeManager.TickDelta;
        newY = Mathf.Clamp(newY, minY, maxY);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
    #endregion

    #region Colorchange
    private void CheckForChangeColor()
    {
        if (!colorChangeAction.triggered) return;

        float r = Random.value;
        float g = Random.value;
        float b = Random.value;
        ChangeColor(r, g, b);
    }

    [ServerRpc]
    private void ChangeColor(float r, float g, float b)
    {
        playerColor.Value = new Color(r, g, b);
    }

    private void OnColorChanged(Color prevColor, Color newColor, bool asServer)
    {
        playerRenderer.material.color = newColor;
    }
    #endregion
}
