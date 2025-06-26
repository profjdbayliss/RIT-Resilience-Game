using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class RGNetworkLoginUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] internal TMP_InputField playernameInput;
    [SerializeField] internal TMP_InputField ipInputField;
    [SerializeField] internal Button hostButton;
    [SerializeField] internal Button clientButton;

    public static RGNetworkLoginUI s_instance;

    // The IP address of the currently hosted game (set this when hosting starts)
    private string hostedGameIp;
    private bool isJoining = false;

    void Awake()
    {
        s_instance = this;
    }

    void Start()
    {
        ipInputField.onValueChanged.AddListener(OnIpInputChanged);
        clientButton.onClick.AddListener(OnJoinButtonClicked);

        // Listen for Mirror's static events
        NetworkClient.OnConnectedEvent += OnClientConnected;
        NetworkClient.OnDisconnectedEvent += OnClientDisconnected;

        UpdateJoinButtonState();
    }

    void OnDestroy()
    {
        ipInputField.onValueChanged.RemoveListener(OnIpInputChanged);
        clientButton.onClick.RemoveListener(OnJoinButtonClicked);

        NetworkClient.OnConnectedEvent -= OnClientConnected;
        NetworkClient.OnDisconnectedEvent -= OnClientDisconnected;
    }

    private void OnIpInputChanged(string newIp)
    {
        UpdateJoinButtonState();
    }

    private void OnJoinButtonClicked()
    {
        if (!isJoining && ipInputField.text == hostedGameIp)
        {
            isJoining = true;
            UpdateJoinButtonState();

            // Start the client connection
            NetworkManager.singleton.networkAddress = ipInputField.text;
            NetworkManager.singleton.StartClient();
        }
    }

    private void OnClientConnected()
    {
        isJoining = false;
        UpdateJoinButtonState();
    }

    private void OnClientDisconnected()
    {
        isJoining = false;
        UpdateJoinButtonState();
    }

    private void UpdateJoinButtonState()
    {
        // Disable if joining is in progress and IP matches hosted game
        if (isJoining && ipInputField.text == hostedGameIp)
        {
            clientButton.interactable = false;
        }
        else
        {
            clientButton.interactable = !string.IsNullOrWhiteSpace(playernameInput.text) && !string.IsNullOrWhiteSpace(ipInputField.text);
        }
    }

    // Called by UI element UsernameInput.OnValueChanged
    public void ToggleButtons(string username)
    {
        hostButton.interactable = !string.IsNullOrWhiteSpace(username);
        clientButton.interactable = !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(ipInputField.text);
    }

    public void LoadScene(int index)
    {
        Debug.Log("loading scene in rg login ui");
        SceneManager.LoadScene(index);    
    }
}
