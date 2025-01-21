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
    public GameObject aiPlayerPrefab; // Prefab for AI players

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

        int playerID = conn.connectionId;
        RGNetworkPlayer player = (RGNetworkPlayer)conn.identity.GetComponent<RGNetworkPlayer>();
        string name = player.mPlayerName;
        var cardPlayer = player.cardPlayerInstance;

        // Add to network player list
        RGNetworkPlayerList.instance.AddPlayer(playerID, name, cardPlayer, conn);

        // Check the number of red players and create blue AI players if needed
        int redPlayerCount = RGNetworkPlayerList.instance.GetPlayerCountByTeam(PlayerTeam.Red);
        int blueAICount = redPlayerCount * 4;
        CreateBlueAIPlayers(blueAICount);
    }

    // Method to create blue AI players
    private void CreateBlueAIPlayers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject aiPlayerObj = Instantiate(aiPlayerPrefab);
            RGNetworkPlayer aiPlayer = aiPlayerObj.GetComponent<RGNetworkPlayer>();
            aiPlayer.InitializeAIPlayer("BlueAI_" + i, PlayerTeam.Blue);
            NetworkServer.AddPlayerForConnection(null, aiPlayerObj);
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

        RGNetworkPlayerList.instance.RemovePlayer(conn.connectionId);

        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        //RGNetworkLoginUI.s_instance.gameObject.SetActive(true);
        //RGNetworkLoginUI.s_instance.playernameInput.text = "";
        //RGNetworkLoginUI.s_instance.playernameInput.ActivateInputField();
        SceneManager.LoadScene("MainMenu");
    }
}
