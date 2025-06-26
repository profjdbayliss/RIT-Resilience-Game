using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

public class RGNetworkManager : NetworkManager
{
    public GameObject playerListPrefab;

    // --- Add these fields for the join queue system ---
    private readonly Queue<NetworkConnectionToClient> joinQueue = new();
    private bool isProcessingJoin = false;
    // --------------------------------------------------

    // --- Add these fields for the add player queue system ---
    private readonly Queue<NetworkConnectionToClient> addPlayerQueue = new();
    private bool isAddingPlayer = false;
    // --------------------------------------------------------

    private Coroutine waitAndAddPlayerCoroutine;

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameObject obj = Instantiate(playerListPrefab);
        playerListPrefab.transform.localScale = Vector3.one;
        NetworkServer.Spawn(obj);

        NetworkServer.RegisterHandler<AddPlayerMessage>(OnAddPlayerRequest, false);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        NetworkClient.RegisterHandler<JoinPermissionMessage>(OnJoinPermissionMessage, false);
    }

    public void OnJoinPermissionMessage(JoinPermissionMessage msg)
    {
        if (msg.canJoin)
        {
            if (!NetworkClient.ready)
            {
                NetworkClient.Ready();
                if (waitAndAddPlayerCoroutine != null)
                    StopCoroutine(waitAndAddPlayerCoroutine);
                waitAndAddPlayerCoroutine = StartCoroutine(WaitAndAddPlayer());
            }
            else if (NetworkClient.localPlayer == null)
            {
                NetworkClient.AddPlayer();
            }
        }
    }

    private IEnumerator WaitAndAddPlayer()
    {
        while (!NetworkClient.ready)
            yield return null;

        // Wait until scene is loaded
        while (SceneManager.GetActiveScene().name != onlineScene)
            yield return null;

        yield return null; // extra frame

        if (NetworkClient.localPlayer == null)
            NetworkClient.AddPlayer();

        waitAndAddPlayerCoroutine = null;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        addPlayerQueue.Enqueue(conn);
        TryProcessNextAddPlayer();
    }

    private void TryProcessNextAddPlayer()
    {
        if (isAddingPlayer || addPlayerQueue.Count == 0)
            return;

        isAddingPlayer = true;
        var conn = addPlayerQueue.Peek();

        // Call base to actually add the player
        base.OnServerAddPlayer(conn);

        Debug.Log("Player Added to Server");
        int playerID = conn.connectionId;
        RGNetworkPlayer player = (RGNetworkPlayer)conn.identity.GetComponent<RGNetworkPlayer>();
        string name = player.mPlayerName;
        var cardPlayer = player.cardPlayerInstance;
        RGNetworkPlayerList.instance.AddPlayer(playerID, name, cardPlayer, conn);

        // Sync existing players and game state to the new client
        RGNetworkPlayerList.instance.SyncPlayerListToClient(conn);

        // After player is added, dequeue and process next
        addPlayerQueue.Dequeue();
        isAddingPlayer = false;
        TryProcessNextAddPlayer();
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);
        joinQueue.Enqueue(conn);
        TryProcessNextJoin();
    }

    private void TryProcessNextJoin()
    {
        if (isProcessingJoin || joinQueue.Count == 0)
            return;

        isProcessingJoin = true;
        var conn = joinQueue.Peek();

        // Tell the client it's their turn to join
        conn.Send(new JoinPermissionMessage { canJoin = true });
    }

    private void OnAddPlayerRequest(NetworkConnectionToClient conn, AddPlayerMessage msg)
    {
        // Only allow if it's their turn
        if (joinQueue.Count > 0 && joinQueue.Peek() == conn)
        {
            base.OnServerAddPlayer(conn);
            // ...
        }
    }

    // Called by the authenticator when authentication is complete
    public void OnConnectionAuthenticated(NetworkConnectionToClient conn)
    {
        joinQueue.Enqueue(conn);
        TryProcessNextJoin();
    }

    // Called by UI element NetworkAddressInput.OnValueChanged
    public void SetHostname(string hostname)
    {
        networkAddress = hostname;
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // Remove from queue if still pending
        if (joinQueue.Contains(conn))
        {
            var newQueue = new Queue<NetworkConnectionToClient>(joinQueue.Where(c => c != conn));
            joinQueue.Clear();
            foreach (var c in newQueue)
                joinQueue.Enqueue(c);
        }
        isProcessingJoin = false;
        TryProcessNextJoin();

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

        // Remove from activePlayers and playerNames if present
        if (conn.authenticationData is string username)
        {
            RGNetworkAuthenticator.RemovePlayer(username);
        }

        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        if (waitAndAddPlayerCoroutine != null)
        {
            StopCoroutine(waitAndAddPlayerCoroutine);
            waitAndAddPlayerCoroutine = null;
        }
        // Clear queues and flags if needed
        base.OnClientDisconnect();
        SceneManager.LoadScene("MainMenu");
    }
}

public struct JoinPermissionMessage : NetworkMessage
{
    public bool canJoin;
}

public struct AddPlayerMessage : NetworkMessage
{
    // Add fields as needed for the AddPlayerMessage
}
