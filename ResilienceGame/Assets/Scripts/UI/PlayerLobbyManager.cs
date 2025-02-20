using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class PlayerLobbyManager : NetworkBehaviour
{
    [SerializeField] private GameObject playerPopupPrefab;
    [SerializeField] private RectTransform blueLeftParent;
    [SerializeField] private RectTransform blueRightParent;
    [SerializeField] private RectTransform redParent;

    [SerializeField] public Color redColor;
    [SerializeField] public Color blueColor;

    public SyncList<PlayerData> players = new SyncList<PlayerData>();

    public static PlayerLobbyManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
    }

    public void AddPlayer(string name, PlayerTeam team)
    {
        if (isServer)
        {
            players.Add(new PlayerData { Name = name, Team = team });
            UpdatePlayerLobbyUI(); // Update the actual game UI after adding a player
        }
        HandlePlayerChanges(players); // Notify PlayerLobbyManager of changes
    }

    public void AddPlayerToUI(PlayerData playerData)
    {
        GameObject newPlayerPopup = Instantiate(playerPopupPrefab, GetParentForTeam(playerData.Team));
        LobbyItem lobbyItem = newPlayerPopup.GetComponent<LobbyItem>();
        lobbyItem.SetPlayerNameAndTeam(playerData.Name, playerData.Team);
    }

    private void ClearUI()
    {
        foreach (Transform child in redParent) Destroy(child.gameObject);
        foreach (Transform child in blueLeftParent) Destroy(child.gameObject);
        foreach (Transform child in blueRightParent) Destroy(child.gameObject);
    }

    private Transform GetParentForTeam(PlayerTeam team)
    {
        if (team == PlayerTeam.Red)
        {
            return redParent;
        }
        else
        {
            return GetNextAvailableSlot();
        }
    }

    private Transform GetNextAvailableSlot()
    {
        foreach (Transform child in blueLeftParent)
        {
            if (child.childCount == 0)
            {
                return child;
            }
        }

        foreach (Transform child in blueRightParent)
        {
            if (child.childCount == 0)
            {
                return child;
            }
        }

        return blueLeftParent; // Default to blueLeftParent if all slots are taken
    }

    public void UpdatePlayerLobbyUI() // called on client disconnect somewhere
    {
        HandlePlayerChanges(players);
    }

    public void HandlePlayerChanges(SyncList<PlayerData> playerDataList) // called on disconnect and when the host joins in GameManager.
    {
        // Clear the current UI
        ClearUI();

        // Add each player to the UI
        foreach (var playerData in playerDataList)
        {
            AddPlayerToUI(playerData);
        }
    }
    public void KickPlayer(string playerName)
    {
        if (isServer)
        {
            var player = players.Find(p => p.Name == playerName);
            if (player != null)
            {
                players.Remove(player);
                UpdatePlayerLobbyUI(); // Update the actual game UI after kicking a player
            }
        }
    }

    public void SwitchPlayerTeam(string playerName)
    {
        if (isServer)
        {
            var player = players.Find(p => p.Name == playerName);
            if (player != null)
            {
                player.Team = player.Team == PlayerTeam.Red ? PlayerTeam.Blue : PlayerTeam.Red;
                UpdatePlayerLobbyUI(); // Update the actual game UI after switching a player's team
            }
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string Name;
    public PlayerTeam Team;
}
