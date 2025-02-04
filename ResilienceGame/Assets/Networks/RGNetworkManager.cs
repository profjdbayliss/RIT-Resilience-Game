using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.Chat;
using TMPro;
using UnityEngine.SceneManagement;

public class RGNetworkManager : NetworkManager
{
    public GameObject playerListPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameObject obj = Instantiate(playerListPrefab);
        playerListPrefab.transform.localScale = Vector3.one;
        NetworkServer.Spawn(obj);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("Player Added to Server");
        int playerID = conn.connectionId;
        RGNetworkPlayer player = (RGNetworkPlayer)conn.identity.GetComponent<RGNetworkPlayer>();
        string name = player.mPlayerName;
        var cardPlayer = player.cardPlayerInstance;

        RGNetworkPlayerList.instance.AddPlayer(playerID, name, cardPlayer, conn);
        SynchronizePlayerList();
        UpdatePlayerVisibilityForAll();
        PlayerLobbyManager.Instance.UpdatePlayerLobbyUI(); // Update the lobby screen when a player is added
    }

    public void SynchronizePlayerList()
    {
        RGNetworkPlayerList.instance.RpcUpdatePlayerList(RGNetworkPlayerList.instance.playerIDs.ToArray(), RGNetworkPlayerList.instance.playerNames.ToArray());
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.authenticationData != null)
            RGNetworkAuthenticator.playerNames.Remove((string)conn.authenticationData);

        RGNetworkPlayerList.instance.RemovePlayer(conn.connectionId);
        SynchronizePlayerList();
        UpdatePlayerVisibilityForAll();

        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        SceneManager.LoadScene("MainMenu");
    }

    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public void UpdatePlayerVisibilityForAll()
    {
        foreach (var player in FindObjectsOfType<RGNetworkPlayer>())
        {
            player.UpdatePlayerVisibility();
        }
    }
}
