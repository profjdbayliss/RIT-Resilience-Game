using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;

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
        int playerId = conn.connectionId;

        // Remove player data
        if (RGNetworkPlayerList.instance != null)
        {
            RGNetworkPlayerList.instance.RemovePlayer(playerId);
        }

        // Remove from GameManager observers (if present)
        if (GameManager.Instance != null)
        {
            var observers = GameManager.Instance.GetObservers();
            foreach (var observer in observers.ToList()) // ToList avoids modifying the collection during iteration
            {
                if (observer is RGNetworkPlayerList && RGNetworkPlayerList.instance.localPlayerID == playerId)
                {
                    GameManager.Instance.RemoveObserver(observer);
                    Debug.Log($"Observer for player {playerId} removed.");
                }
            }
        }

        base.OnServerDisconnect(conn);
    }


    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        SceneManager.LoadScene("MainMenu");
    }
}
