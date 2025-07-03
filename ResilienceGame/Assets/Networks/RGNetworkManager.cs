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

        // Register all custom messages first, in the same order as client
        NetworkServer.RegisterHandler<AddPlayerMessage>(OnAddPlayerRequest, false);
        // ... any other custom messages

        if (authenticator != null)
            authenticator.OnServerAuthenticated.AddListener(OnConnectionAuthenticated);

        GameObject obj = Instantiate(playerListPrefab);
        playerListPrefab.transform.localScale = Vector3.one;
        NetworkServer.Spawn(obj);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Register all custom messages first, in the same order as server
        NetworkClient.RegisterHandler<JoinPermissionMessage>(OnJoinPermissionMessage, false);
        // Only register AddPlayerMessage if you need to handle it on the client
        // NetworkClient.RegisterHandler<AddPlayerMessage>(OnAddPlayerMessage, false);

        Mirror.NetworkMessages.LogTypes();
    }

    public void OnJoinPermissionMessage(JoinPermissionMessage msg)
    {
        if (msg.canJoin && !NetworkClient.ready)
        {
            NetworkClient.Ready();
            if (waitAndAddPlayerCoroutine != null)
                StopCoroutine(waitAndAddPlayerCoroutine);
            waitAndAddPlayerCoroutine = StartCoroutine(WaitAndAddPlayer());
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

        // Mark this player as fully ready and allow the next in the join queue to proceed
        OnPlayerFullyReady(conn);

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

        // Only send JoinPermissionMessage to the first in queue
        conn.Send(new JoinPermissionMessage { canJoin = true });
    }

    private void OnPlayerFullyReady(NetworkConnectionToClient conn)
    {
        // Remove from queue and process next
        if (joinQueue.Count > 0 && joinQueue.Peek() == conn)
            joinQueue.Dequeue();
        isProcessingJoin = false;
        TryProcessNextJoin();
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

    // If you need a client handler for AddPlayerMessage:
    private void OnAddPlayerMessage(AddPlayerMessage msg)
    {
        Debug.Log("Received AddPlayerMessage on client.");
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
