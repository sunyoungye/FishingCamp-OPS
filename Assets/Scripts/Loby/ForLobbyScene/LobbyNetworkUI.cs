using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class LobbyNetworkUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nicknameInputField;
    public Button createServerButton;
    public Button joinServerButton;
    public TMP_Text statusText;

    [Header("Scene")]
    public string gameSceneName = "MainHome";

    private void Start()
    {
        if (createServerButton != null)
        {
            createServerButton.onClick.AddListener(CreateServer);
        }

        if (joinServerButton != null)
        {
            joinServerButton.onClick.AddListener(JoinServer);
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        SetStatus("....");
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void CreateServer()
    {
        SavePlayerName();

        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager is missing.");
            return;
        }

        bool success = NetworkManager.Singleton.StartHost();

        if (!success)
        {
            SetStatus("Create Server Failed.");
            return;
        }

        SetStatus("Host Started. Loading GameScene...");

        NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    private void JoinServer()
    {
        SavePlayerName();

        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager is missing.");
            return;
        }

        bool success = NetworkManager.Singleton.StartClient();

        if (!success)
        {
            SetStatus("Join Server Failed.");
            return;
        }

        SetStatus("Joining Server...");

        if (createServerButton != null)
        {
            createServerButton.interactable = false;
        }

        if (joinServerButton != null)
        {
            joinServerButton.interactable = false;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            SetStatus($"Host Connected. ClientId: {clientId}");
        }
        else
        {
            SetStatus($"Client COnnected. ClientId: {clientId}");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        if (clientId != NetworkManager.Singleton.LocalClientId)
        {
            return;
        }

        SetStatus($"Disconnected. ClientId: {clientId}");

        if (createServerButton != null)
        {
            createServerButton.interactable = true;
        }

        if (joinServerButton != null)
        {
            joinServerButton.interactable = true;
        }
    }

    private void SavePlayerName()
    {
        string playerName = "Player";

        if (nicknameInputField != null && !string.IsNullOrEmpty(nicknameInputField.text))
        {
            playerName = nicknameInputField.text.Trim();
        }

        PlayerPrefs.SetString("PLAYER_NAME", playerName);
        PlayerPrefs.Save();

        Debug.Log($"Player Name Saved: {playerName}");
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
