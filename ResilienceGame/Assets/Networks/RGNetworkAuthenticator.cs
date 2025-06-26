using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGNetworkAuthenticator : NetworkAuthenticator
{
    // Use a static dictionary to track active players by username
    public static readonly Dictionary<string, NetworkConnection> activePlayers = new Dictionary<string, NetworkConnection>();

    readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new HashSet<NetworkConnection>();
    internal static readonly HashSet<string> playerNames = new HashSet<string>();

    [Header("Client Username")]
    public string playerName;

    #region Messages
    public struct AuthRequestMessage : NetworkMessage
    {
        public string authUsername;
    }

    public struct AuthResponseMessage : NetworkMessage
    {
        public byte code;
        public string message;
    }
    #endregion

    #region Server

    // RuntimeInitializeOnLoadMethod -> fast playmode without domain reload
    [UnityEngine.RuntimeInitializeOnLoadMethod]
    static void ResetStatics()
    {
        playerNames.Clear();
        activePlayers.Clear(); // Clear the active players dictionary on reset
    }

    /// <summary>
    /// Called on server from StartServer to initialize the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartServer()
    {
        // register a handler for the authentication request we expect from client
        NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
        // Only register JoinPermissionMessage here if it's an auth message
    }

    /// <summary>
    /// Called on server from StopServer to reset the Authenticator
    /// <para>Server message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStopServer()
    {
        // unregister the handler for the authentication request
        NetworkServer.UnregisterHandler<AuthRequestMessage>();
    }

    /// <summary>
    /// Called on server from OnServerConnectInternal when a client needs to authenticate
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    public override void OnServerAuthenticate(NetworkConnectionToClient conn)
    {
        // do nothing...wait for AuthRequestMessage from client
    }

    /// <summary>
    /// Called on server when the client's AuthRequestMessage arrives
    /// </summary>
    /// <param name="conn">Connection to client.</param>
    /// <param name="msg">The message payload</param>
    public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
    {
        Debug.Log($"Authentication Request: {msg.authUsername}");

        // Check if the username exists in activePlayers but connection is invalid
        if (activePlayers.TryGetValue(msg.authUsername, out NetworkConnection existingConn))
        {
            if (existingConn.owned == null)
            {
                // Remove stale entry
                RemovePlayer(msg.authUsername);
            }
        }

        // Reject if the game has already started
        if (GameManager.Instance != null && GameManager.Instance.gameStarted)
        {
            AuthResponseMessage gameStartedResponse = new AuthResponseMessage // Renamed variable
            {
                code = 205, // Custom code for "Game already started"
                message = "Game already started. Cannot join."
            };
            conn.Send(gameStartedResponse);
            ServerReject(conn);
            return;
        }

        if (connectionsPendingDisconnect.Contains(conn)) return;

        // Check if the username is already in use
        if (activePlayers.ContainsKey(msg.authUsername))
        {
            connectionsPendingDisconnect.Add(conn);

            AuthResponseMessage usernameDuplicateResponse = new AuthResponseMessage // Renamed variable
            {
                code = 200,
                message = "Username already in use...try again"
            };

            conn.Send(usernameDuplicateResponse);
            conn.isAuthenticated = false;

            StartCoroutine(DelayedDisconnect(conn, 1f));
            return;
        }

        // Add the player to the active players dictionary
        activePlayers[msg.authUsername] = conn;

        // Add the name to the HashSet
        playerNames.Add(msg.authUsername);

        // Store username in authenticationData
        conn.authenticationData = msg.authUsername;

        // Send success response to the client
        AuthResponseMessage authResponseMessage = new AuthResponseMessage
        {
            code = 100,
            message = "Success"
        };

        conn.Send(authResponseMessage);

        // Accept the successful authentication
        ServerAccept(conn);

        // Notify the network manager about the authenticated connection
        ((RGNetworkManager)NetworkManager.singleton).OnConnectionAuthenticated(conn);
    }

    IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        // Reject the unsuccessful authentication
        ServerReject(conn);

        yield return null;

        // Remove the connection from pending connections
        connectionsPendingDisconnect.Remove(conn);
    }

    #endregion

    #region Client
    public static void RemovePlayer(string username)
    {
        // Remove from both collections
        activePlayers.Remove(username);
        playerNames.Remove(username);
    }

    // Called by UI element UsernameInput.OnValueChanged
    public void SetPlayername(string username)
    {
        playerName = username;
    }

    /// <summary>
    /// Called on client from StartClient to initialize the Authenticator
    /// <para>Client message handlers should be registered in this method.</para>
    /// </summary>
    public override void OnStartClient()
    {
        // Register authentication response handler
        NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        // Only register JoinPermissionMessage here if it's an auth message
    }

    /// <summary>
    /// Called on client from StopClient to reset the Authenticator
    /// <para>Client message handlers should be unregistered in this method.</para>
    /// </summary>
    public override void OnStopClient()
    {
        // unregister the handler for the authentication response
        NetworkClient.UnregisterHandler<AuthResponseMessage>();
    }

    /// <summary>
    /// Called on client from OnClientConnectInternal when a client needs to authenticate
    /// </summary>
    public override void OnClientAuthenticate()
    {
        NetworkClient.Send(new AuthRequestMessage { authUsername = playerName });
    }

    /// <summary>
    /// Called on client when the server's AuthResponseMessage arrives
    /// </summary>
    /// <param name="msg">The message payload</param>
    public void OnAuthResponseMessage(AuthResponseMessage msg)
    {
        if (msg.code == 100)
        {
            Debug.Log($"Authentication Response: {msg.code} {msg.message}");

            // Authentication has been accepted
            ClientAccept();
        }
        else
        {
            Debug.LogError($"Authentication Response: {msg.code} {msg.message}");

            // Authentication has been rejected
            // StopHost works for both host client and remote clients
            NetworkManager.singleton.StopHost();

            // Do this AFTER StopHost so it doesn't get cleared / hidden by OnClientDisconnect
        }
    }

    #endregion
}