using UnityEngine;
using Mirror;
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
        //RGNetworkPlayerList.instance.AddPlayer(playerID, name);
        RGNetworkPlayerList.instance.AddPlayer(playerID, name, cardPlayer, conn);
        //SynchronizePlayerList();
        //UpdatePlayerVisibilityForAll();
        //PlayerLobbyManager.Instance.UpdatePlayerLobbyUI(); // Update the lobby screen when a player is added
    }


    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.authenticationData != null)
            RGNetworkAuthenticator.playerNames.Remove((string)conn.authenticationData);

        RGNetworkPlayerList.instance.RemovePlayer(conn.connectionId); //removes player who left
        //SynchronizePlayerList();    // THIS UPDATES THE LOBBY
        //UpdatePlayerVisibilityForAll();

        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        SceneManager.LoadScene("MainMenu");
    }
}
