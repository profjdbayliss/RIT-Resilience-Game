using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Examples.Chat;
using TMPro;

public class RGNetworkManager : NetworkManager
{
    public GameObject playerListPrefab;
    public CardReader cardReader;
    public CreateTextureAtlas textAtlas;
    public CreateTextureAtlas atlasMaker;

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
        textAtlas.DelayedStart();

    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        int playerID = conn.connectionId;
        if (conn.identity.isLocalPlayer && (RGNetworkPlayerList.instance.playerIDs.Contains(0) == false)) // if the player is host, join red team
        {
            RGNetworkPlayerList.instance.AddPlayer(playerID, 0);
            // Add their cards to the player

        }
        else // if the player is client, join blue team
        {
            RGNetworkPlayerList.instance.AddPlayer(playerID, 1);
            // Add their cards to the blue players

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
        //RGGameExampleUI.connNames.Remove(conn);

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
