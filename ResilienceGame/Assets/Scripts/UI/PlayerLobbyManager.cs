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

    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;

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
        switch (op)
        {
            case SyncList<PlayerData>.Operation.OP_ADD:
                AddPlayerToUI(newItem);
                break;

            case SyncList<PlayerData>.Operation.OP_REMOVEAT:
                RemovePlayerFromUI(oldItem.Name);
                break;

            case SyncList<PlayerData>.Operation.OP_INSERT:
                AddPlayerToUI(newItem);
                break;

            case SyncList<PlayerData>.Operation.OP_CLEAR:
                ClearUI();
                break;

            default:
                Debug.LogWarning($"Unhandled operation: {op}");
                break;
        }
    }

    public void AddPlayer(string name, PlayerTeam team)
    {
        if (isServer)
        {
            var newPlayer = new PlayerData { Name = name, Team = team };
            players.Add(newPlayer);
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
                players[players.IndexOf(player)] = player; // Trigger a SyncList update
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
}

public enum PlayerTeam
{
    Red,
    Blue
}

[System.Serializable]
public class PlayerData
{
    public string Name;
    public PlayerTeam Team;
}
