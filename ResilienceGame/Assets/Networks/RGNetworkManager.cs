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
        RGNetworkPlayerList.instance.AddPlayer(playerID, name, cardPlayer, conn);

        // Sync existing players and game state to the new client
        RGNetworkPlayerList.instance.SyncPlayerListToClient(conn);
    }

    // Called by UI element NetworkAddressInput.OnValueChanged
    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.authenticationData != null)
        {
            string username = (string)conn.authenticationData;
            RGNetworkAuthenticator.RemovePlayer(username);
        }

        RGNetworkPlayerList.instance.RemovePlayer(conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        SceneManager.LoadScene("MainMenu");
    }
}
