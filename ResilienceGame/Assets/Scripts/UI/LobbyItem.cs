using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color missingPlayerColor;
    [Header("UI Elements")]
    public TextMeshProUGUI PlayerName;
    [SerializeField] private GameObject HostControlMenu;
    public Image backgroundImage;
    private bool isLocalPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnHover()
    {
        if (NetworkServer.active)
        {
            // Unhide the specified component
            HostControlMenu.SetActive(true);
        }
    }

    public void OffHover()
    {
        if (NetworkServer.active)
        {
            // Hide the specified component
            HostControlMenu.SetActive(false);
        }
    }

    public void SetPlayerNameAndTeam(string name, PlayerTeam team) { // Called when player disconnects or when host presses "begin"
        PlayerName.text = name;
        PlayerName.enabled = true;
        Debug.Log($"is red? {team == PlayerTeam.Red}");
        Debug.Log($"is blue? {team == PlayerTeam.Blue}");
        isLocalPlayer = name == RGNetworkPlayerList.instance.localPlayerName;
        HostControlMenu.SetActive(!isLocalPlayer);
        if (team == PlayerTeam.Red) {
            backgroundImage.color = redColor;
        }
        else if (team == PlayerTeam.Blue) {
            backgroundImage.color = blueColor;
        }
        else
        {
            backgroundImage.color = missingPlayerColor;
            //SetMissingPlayer();
        }
    }

    public void SetMissingPlayer() {
        HostControlMenu.SetActive(true);
        PlayerName.enabled = false;
        backgroundImage.color = missingPlayerColor;
    }

    public void KickPlayer()
    {
        if (!isLocalPlayer) // if player isn't the host, you're allowed to kick them.
        {
            string playerName = PlayerName.text;
            PlayerLobbyManager.Instance.RemovePlayer(playerName);
            //NetworkServer.DestroyPlayerForConnection()
        }
    }

    public void MovePlayerToTeam(string originalTeam) // If moving to red team, set to "blue", and viceversa
    {
        if (NetworkServer.active)
        {
            string playerName = PlayerName.text;

            // Try to parse the string into a PlayerTeam enum
            if (Enum.TryParse(originalTeam, true, out PlayerTeam newTeam))
            {
                PlayerLobbyManager.Instance.ChangePlayerTeam(playerName, newTeam);
            }
            else
            {
                Debug.LogWarning($"Invalid team name: {originalTeam}");
            }
        }
    }
}
