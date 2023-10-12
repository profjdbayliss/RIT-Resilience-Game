using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.Chat;

public class RGNetworkManager : NetworkManager
{
    public GameObject playerListPrefab;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameObject obj = Instantiate(playerListPrefab);
        NetworkServer.Spawn(obj);
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        int playerID = conn.connectionId;
        if (conn.identity.isLocalPlayer) // if the player is host, join red team
        {
            RGNetworkPlayerList.instance.AddPlayer(playerID, 0);
        }
        else // if the player is client, join blue team
        {
            RGNetworkPlayerList.instance.AddPlayer(playerID, 1);
        }
    }

    // Called by UI element NetworkAddressInput.OnValueChanged
    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // remove player name from the HashSet
        if (conn.authenticationData != null)
            RGNetworkAuthenticator.playerNames.Remove((string)conn.authenticationData);

        // remove connection from Dictionary of conn > names
        RGGameExampleUI.connNames.Remove(conn);

        RGNetworkPlayerList.instance.RemovePlayer(conn.connectionId);

        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        RGNetworkLoginUI.s_instance.gameObject.SetActive(true);
        RGNetworkLoginUI.s_instance.playernameInput.text = "";
        RGNetworkLoginUI.s_instance.playernameInput.ActivateInputField();
    }
}
