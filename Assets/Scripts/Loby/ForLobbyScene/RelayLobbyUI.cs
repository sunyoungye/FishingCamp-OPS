using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;

using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;

public class RelayLobbyUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nicknameInputField;
    public Button createServerButton;
    public TMP_Text roomCodeText;
    public TMP_InputField roomCodeInputField;
    public Button joinServerButton;
    public Button startGameButton;
    public TMP_Text statusText;

    [Header("Room Settings")]
    public int maxPlayers = 4;
    public string gameSceneName = "GameScene";

    private string currentJoinCode = "";
    private bool callbacksRegistered = false;

    private async void Start()
    {
        if (createServerButton != null)
        {
            createServerButton.onClick.AddListener(CreateRelayServer);
        }

        if (joinServerButton != null)
        {
            joinServerButton.onClick.AddListener(JoinRelayServer);
        }

        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
            startGameButton.interactable = false;
        }

        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: -";
        }

        RegisterNetworkCallbacks();

        await InitializeUnityServicesAsync();

        SetStatus("Ready");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && callbacksRegistered)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void RegisterNetworkCallbacks()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (callbacksRegistered)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        callbacksRegistered = true;
    }

    private async Task InitializeUnityServicesAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                SetStatus("Initializing Unity Services...");
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                SetStatus("Signing in anonymously...");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log($"Unity Services Ready. PlayerId: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            SetStatus("Unity Services Initialize Failed.");
            Debug.LogError(e);
        }
    }

    private async void CreateRelayServer()
    {
        SavePlayerName();

        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager is missing.");
            return;
        }

        try
        {
            SetButtonsInteractable(false);
            SetStatus("Creating Relay Room...");

            await InitializeUnityServicesAsync();

            int maxConnections = Mathf.Max(1, maxPlayers - 1);

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            currentJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (transport == null)
            {
                SetStatus("UnityTransport is missing.");
                SetButtonsInteractable(true);
                return;
            }

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            bool success = NetworkManager.Singleton.StartHost();

            if (!success)
            {
                SetStatus("StartHost Failed.");
                SetButtonsInteractable(true);
                return;
            }

            if (roomCodeText != null)
            {
                roomCodeText.text = $"Room Code: {currentJoinCode}";
            }

            SetStatus($"Room Created. Code: {currentJoinCode}");

            if (startGameButton != null)
            {
                startGameButton.interactable = true;
            }

            if (joinServerButton != null)
            {
                joinServerButton.interactable = false;
            }

            if (createServerButton != null)
            {
                createServerButton.interactable = false;
            }
        }
        catch (Exception e)
        {
            SetStatus("Create Relay Room Failed.");
            Debug.LogError(e);
            SetButtonsInteractable(true);
        }
    }

    private async void JoinRelayServer()
    {
        SavePlayerName();

        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager is missing.");
            return;
        }

        string joinCode = "";

        if (roomCodeInputField != null)
        {
            joinCode = roomCodeInputField.text.Trim().ToUpper();
        }

        if (string.IsNullOrEmpty(joinCode))
        {
            SetStatus("Enter Room Code first.");
            return;
        }

        try
        {
            SetButtonsInteractable(false);
            SetStatus($"Joining Room: {joinCode}...");

            await InitializeUnityServicesAsync();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (transport == null)
            {
                SetStatus("UnityTransport is missing.");
                SetButtonsInteractable(true);
                return;
            }

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            transport.SetRelayServerData(relayServerData);

            bool success = NetworkManager.Singleton.StartClient();

            if (!success)
            {
                SetStatus("StartClient Failed.");
                SetButtonsInteractable(true);
                return;
            }

            SetStatus("Join Requested...");
        }
        catch (Exception e)
        {
            SetStatus("Join Relay Room Failed.");
            Debug.LogError(e);
            SetButtonsInteractable(true);
        }
    }

    private void StartGame()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager is missing.");
            return;
        }

        if (!NetworkManager.Singleton.IsHost)
        {
            SetStatus("Only Host can start game.");
            return;
        }

        SetStatus("Loading GameScene...");

        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            SetStatus($"Client Connected. ClientId: {clientId}");
        }
        else
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SetStatus($"Joined Room. ClientId: {clientId}");
            }
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            SetStatus($"Disconnected. ClientId: {clientId}");
            SetButtonsInteractable(true);
        }
    }

    private void SavePlayerName()
    {
        string playerName = "Player";

        if (nicknameInputField != null && !string.IsNullOrWhiteSpace(nicknameInputField.text))
        {
            playerName = nicknameInputField.text.Trim();
        }

        PlayerPrefs.SetString("PLAYER_NAME", playerName);
        PlayerPrefs.Save();

        Debug.Log($"Player Name Saved: {playerName}");
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (createServerButton != null)
        {
            createServerButton.interactable = interactable;
        }

        if (joinServerButton != null)
        {
            joinServerButton.interactable = interactable;
        }

        if (startGameButton != null)
        {
            startGameButton.interactable = false;
        }
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }

        Debug.Log(message);
    }
}