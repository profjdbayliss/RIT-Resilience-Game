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

    private void Awake()
    {
        players.Callback += OnPlayersChanged;
    }

    private void OnDestroy()
    {
        players.Callback -= OnPlayersChanged;
    }

    private void OnPlayersChanged(SyncList<PlayerData>.Operation op, int index, PlayerData oldItem, PlayerData newItem)
    {
        UserInterface.Instance.BuildLobbyMenu(); // Notify UI to update
        UpdatePlayerLobbyUI(); // Update the actual game UI
    }

    public void AddPlayer(string name, PlayerTeam team)
    {
        if (isServer)
        {
            players.Add(new PlayerData { Name = name, Team = team });
        }
    }

    public void RemovePlayer(string name)
    {
        if (isServer)
        {
            var player = players.Find(p => p.Name == name);
            if (player != null)
            {
                players.Remove(player);
            }
        }
    }

    public void ChangePlayerTeam(string playerName, PlayerTeam newTeam)
    {
        if (isServer)
        {
            var player = players.Find(p => p.Name == playerName);
            if (player != null)
            {
                player.Team = newTeam;
                players[players.IndexOf(player)] = player; // SyncList updates automatically
            }
        }
    }

    private void AddPlayerToUI(PlayerData playerData)
    {
        GameObject newPlayerPopup = Instantiate(playerPopupPrefab, GetParentForTeam(playerData.Team));
        LobbyItem lobbyItem = newPlayerPopup.GetComponent<LobbyItem>();
        lobbyItem.SetPlayerNameAndTeam(playerData.Name, playerData.Team);
    }

    private void RemovePlayerFromUI(string name)
    {
        var lobbyItem = FindLobbyItemByName(name);
        if (lobbyItem != null)
        {
            Destroy(lobbyItem.gameObject);
        }
    }

    private void ClearUI()
    {
        foreach (Transform child in redParent) Destroy(child.gameObject);
        foreach (Transform child in blueLeftParent) Destroy(child.gameObject);
        foreach (Transform child in blueRightParent) Destroy(child.gameObject);
    }

    private Transform GetParentForTeam(PlayerTeam team)
    {
        return team == PlayerTeam.Red ? redParent : blueLeftParent; // Adjust as needed
    }

    private LobbyItem FindLobbyItemByName(string name)
    {
        // Check in red, blueLeft, and blueRight parents
        foreach (Transform child in redParent)
        {
            var lobbyItem = child.GetComponent<LobbyItem>();
            if (lobbyItem != null && lobbyItem.PlayerName.text == name)
                return lobbyItem;
        }

        foreach (Transform child in blueLeftParent)
        {
            var lobbyItem = child.GetComponent<LobbyItem>();
            if (lobbyItem != null && lobbyItem.PlayerName.text == name)
                return lobbyItem;
        }

        foreach (Transform child in blueRightParent)
        {
            var lobbyItem = child.GetComponent<LobbyItem>();
            if (lobbyItem != null && lobbyItem.PlayerName.text == name)
                return lobbyItem;
        }

        return null;
    }

    private void UpdatePlayerLobbyUI()
    {
        ClearUI();
        foreach (var player in players)
        {
            AddPlayerToUI(player);
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public string Name;
    public PlayerTeam Team;
}
