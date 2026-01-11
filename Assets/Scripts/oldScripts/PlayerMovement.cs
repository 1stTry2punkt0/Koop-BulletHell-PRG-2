using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using FishNet.Object.Synchronizing.Internal;
using FishNet.Connection;
using FishNet.Managing.Timing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class PlayerMovement : NetworkBehaviour
{
    readonly public SyncVar<float> syncSpeed = new SyncVar<float>();
    public float speed = 5f;
    private Vector3 _input;

    readonly public SyncVar<string> playerName = new SyncVar<string>();
    public TextMeshPro nameTMP;

    //readonly public SyncVar<Color> playerColor = new SyncVar<Color>();

    readonly public SyncVar<int> playerHealth = new SyncVar<int>();
    public UnityEngine.UI.Image healthBar;
    private bool isOnCD = false;

    private float traveledDistance = 0f;
    [SerializeField] float spawnDistance = 20f;
    private ProjectileSpawner spawner;


    private Animator animator;

    private GameObject characterModel;
    readonly public SyncVar<Quaternion> syncRotation = new SyncVar<Quaternion>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Register this player on the server
        PlayerTracker.RegisterPlayer(this);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        // Unregister this player from the server
        PlayerTracker.UnregisterPlayer(this);
    }


    private void Start()
    {
        // Subscribe to tick event when the object is active
        TimeManager.OnTick += TimeManager_OnTick;


        syncSpeed.OnChange += OnSpeedChange;
        playerName.OnChange += OnNameChange;
        playerName.Value = "Player_" + Owner.ClientId;

        syncRotation.OnChange += OnRotationChanged;

        //playerColor.OnChange += OnColorChange;
        //playerColor.Value = new Color(Random.value, Random.value, Random.value);

        playerHealth.Value = 100;
        playerHealth.OnChange += (prev, next, asServer) =>
        {
            healthBar.fillAmount = playerHealth.Value / 100f;
        };

        // Initialize default speed on the server (if not already set)
        if (syncSpeed.Value == 0f)
            syncSpeed.Value = 5f;

        traveledDistance = 0f;
        //spawner = GetComponent<ProjectileSpawner>();

        //set spawn position
        SetInitialPositionServer(new Vector3(Random.Range(-3f, 3f), 6.5f, Random.Range(-3f, 3f)));
        //set y position to 6.5f to avoid spawning inside the ground
        transform.position = new Vector3(transform.position.x, 6.5f, transform.position.z);

        animator = GetComponentInChildren<Animator>();
        characterModel = transform.GetChild(0).gameObject;
    }

    [ServerRpc]
    public void SetInitialPositionServer(Vector3 position)
    {
        transform.position = position;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        OnNameChange(default, playerName.Value, false);
        //OnColorChange(default, playerColor.Value, false);
        OnHealthChange(default, playerHealth.Value, false);
        OnRotationChanged(default, syncRotation.Value, false);
    }

    /// <summary>
    /// Called by FishNet's TimeManager on every network tick.
    /// </summary>
    private void TimeManager_OnTick()
    {
        // Only the owning client should read input
        if (!IsOwner)
            return;

        //Set main cam above the player
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y + 20f, transform.position.z);

        // Make sure we actually have a keyboard (e.g. not on some weird platform)
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        // --- WASD movement using the new Input System ---
        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed) vertical += 1f;

        _input = new Vector3(horizontal, 0f, vertical);

        // M key toggles speed (new Input System)
        if (keyboard.mKey != null && keyboard.mKey.wasPressedThisFrame)
            ChangeSpeed();

        // Send input to the server (server-authoritative movement)
        if (_input != Vector3.zero)
        {
            MoveServer(_input);
        }
        else
        {
            GoIdle();
        }

        //if (Input.GetKey(KeyCode.C))
        //{
        //    ChangeColor();
        //}

        if (Input.GetKey(KeyCode.H) && !isOnCD)
        {
            ChangeHealth(10);
            isOnCD = true;
            Invoke(nameof(ResetCD), 1f);
        }
    }

    [ServerRpc]
    private void MoveServer(Vector3 input)
    {
        // Use TickDelta for tick-based movement instead of Time.deltaTime
        float delta = (float)TimeManager.TickDelta;

        // Calculate movement on the server only (server-authoritative)
        Vector3 movement = input.normalized * syncSpeed.Value * delta;


        // Apply movement to server-side position
        transform.position += movement;

        traveledDistance += movement.magnitude;
        if (traveledDistance >= spawnDistance)
        {
            traveledDistance = 0f;
            //spawner.SpawnProjectileServer(transform.position);
        }

        animator.Play("Run");
        // Rotate character model to face movement direction
        Quaternion targetRotation = Quaternion.LookRotation(movement);
        characterModel.transform.rotation = Quaternion.Slerp(characterModel.transform.rotation, targetRotation, 0.2f);
        syncRotation.Value = characterModel.transform.rotation;


        // Create callback message
        string callbackText = $"Moved by: {movement}";

        // Send callback only to the owning client
        MoveCallback(Owner, callbackText);
    }

    [ServerRpc]
    private void GoIdle()
    {
        animator.Play("Idle");
    }

    // First parameter MUST be NetworkConnection for a TargetRpc
    [TargetRpc]
    private void MoveCallback(NetworkConnection conn, string msg)
    {
        // Runs only on the client that owns this object
        Debug.Log($"[Callback] {msg}");
    }


    private void ResetCD()
    {
        isOnCD = false;
    }

    [ServerRpc]
    private void ChangeSpeed()
    {
        // Toggle between two speeds (server decides)
        syncSpeed.Value = syncSpeed.Value == 5f ? 10f : 5f;
    }

    [ServerRpc]
    private void ChangeColor()
    {
        //playerColor.Value = new Color(Random.value, Random.value, Random.value);
    }

    [ServerRpc]
    private void ChangeHealth(int amount)
    {
        playerHealth.Value -= amount;
        if(playerHealth.Value <= 0)
        {
            playerHealth.Value = 100;
            transform.position = Vector3.zero;
        }
    }

    public void OnSpeedChange(float prev, float next, bool asServer)
    {
        // Logs whenever the speed SyncVar changes
        Debug.Log($"Speed changed: {prev} ? {next}");
    }

    public void OnNameChange(string prev, string next, bool asServer)
    {
        name = playerName.Value;
        nameTMP.text = playerName.Value;
    }

    private void OnRotationChanged(Quaternion prev, Quaternion next, bool asServer)
    {
        if (characterModel != null)
        {
            characterModel.transform.rotation = syncRotation.Value;
        }
    }

    //public void OnColorChange(Color prev, Color next, bool asServer)
    //{
    //    GetComponent<Renderer>().material.color = next;

    //    float luminance = next.r * 0.2126f + next.g * 0.7152f + next.b * 0.0722f;

    //    if (luminance < 0.5f)
    //    {
    //        nameTMP.color = Color.white;
    //        healthBar.color = Color.white;
    //    }
    //    else
    //    {
    //        nameTMP.color = Color.black;
    //        healthBar.color = Color.black;
    //    }
            
    //}

    public void OnHealthChange(int prev, int next, bool asServer)
    {
        healthBar.fillAmount = playerHealth.Value / 100f;
    }
}