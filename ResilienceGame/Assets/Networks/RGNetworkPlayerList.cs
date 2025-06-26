using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem.Utilities;
using static Facility;
using TMPro;

#region Network Message Structs
public struct RGNetworkShortMessage : NetworkMessage
{
    public uint playerID;
    public uint type;
}

// for messages that need to update one or more arguments
public struct RGNetworkLongMessage : NetworkMessage
{
    public uint playerID;
    public uint type;
    // number of arguments
    public uint count;
    // the parameters for the message
    // -> ArraySegment to avoid unnecessary allocations
    public ArraySegment<byte> payload;
}
#endregion
// many messages actually have no arguments


public class RGNetworkPlayerList : NetworkBehaviour, IRGObserver
{
    public static RGNetworkPlayerList instance;

    int nextCardUID = 0;
    Dictionary<int, int> drawnCardUIDs = new Dictionary<int, int>();

    #region Local Player Fields
    public int localPlayerID;
    public string localPlayerName;
    private GameManager manager;

    #endregion

    #region Player Lists
    public Dictionary<int, int> playerIDs = new Dictionary<int, int>();
    private Dictionary<int, bool> playerNetworkReadyFlags = new Dictionary<int, bool>();
    public Dictionary<int, bool> playerTurnTakenFlags = new Dictionary<int, bool>();
    public Dictionary<int, PlayerTeam> playerTypes = new Dictionary<int, PlayerTeam>();
    public Dictionary<int, string> playerNames = new Dictionary<int, string>();
    private Dictionary<int, NetworkConnectionToClient> playerConnections = new Dictionary<int, NetworkConnectionToClient>();
    #endregion

    #region Start/Initialization
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        SetupHandlers();

    }

    public void Start()
    {
        manager = GameManager.Instance;

        Debug.Log("start run on RGNetworkPlayerList.cs");

    }
    public bool CheckReadyToStart()
    {
        bool readyToStart = true;
        foreach (var kvp in playerIDs) // Iterate through dictionary entries
        {
            int playerId = kvp.Key;
            if (playerTypes[playerId] == PlayerTeam.Any)
            {
                readyToStart = false;
                UserInterface.Instance.hostLobbyBeginError.GetComponentInChildren<TextMeshProUGUI>().text =
                    "Not everyone is ready yet.";
                break;
            }
        }
        return readyToStart;
    }
    public void AddWhitePlayer()
    {
        // Generate a unique ID that doesn't conflict with existing connection IDs
        int newPlayerID = playerIDs.Count > 0 ? playerIDs.Keys.Max() + 1 : 0;

        playerIDs.Add(newPlayerID, newPlayerID);
        playerNetworkReadyFlags.Add(newPlayerID, true);
        playerTurnTakenFlags.Add(newPlayerID, false);
        playerTypes.Add(newPlayerID, PlayerTeam.White);
        playerNames.Add(newPlayerID, "White_Player");
        GameManager.Instance.whitePlayer.NetID = newPlayerID;
    }

    // Assumes this doesn't exist in the list yet so please check
    // before calling
    public void AddPotentialPlayer(int id, string name, int type)
    {
        // Add or update all dictionaries
        playerIDs[id] = id; // Upsert
        playerNetworkReadyFlags[id] = true;
        playerTurnTakenFlags[id] = false;
        playerTypes[id] = (PlayerTeam)type;
        playerNames[id] = name;
    }

    public void AddPlayer(int id, string name, CardPlayer cardPlayer, NetworkConnectionToClient conn)
    {
        if (isServer)
        {
            // Use the connection ID directly
            int playerId = conn.connectionId;

            playerIDs[playerId] = playerId;
            playerNetworkReadyFlags[playerId] = true;
            playerTurnTakenFlags[playerId] = false;
            playerTypes[playerId] = PlayerTeam.Any;
            playerConnections[playerId] = conn;
            playerNames[playerId] = name;

            // Send updated player list to all clients
            Message data = CreateNewPlayerMessage(id, name, (int)PlayerTeam.Any);
            RGNetworkLongMessage msg = new RGNetworkLongMessage
            {
                playerID = data.senderID,
                type = (uint)data.Type,
                count = 1,
                payload = data.byteArguments.ToArray()
            };
            NetworkServer.SendToAll(msg);
            NotifyPlayerChanges();
        }
    }

    public void SetAiPlayerAsReadyToStartGame()
    {
        int aiPlayerIndex = playerIDs.Count - 1;
        if (aiPlayerIndex != -1)
        {
            playerNetworkReadyFlags[aiPlayerIndex] = true;
            playerTurnTakenFlags[aiPlayerIndex] = false; // reset for new game start
            Debug.Log("AI player automatically marked as ready by server.");
        }
    }
    public void SetWhitePlayerEndPhase()
    {
        int aiPlayerIndex = playerIDs.Count - 1;
        if (aiPlayerIndex != -1)
        {
            playerTurnTakenFlags[aiPlayerIndex] = true;
        }
    }

    // Helper method to replace FindIndex
    private int FindPlayerKey(int targetID)
    {
        foreach (var kvp in playerIDs)
        {
            if (kvp.Value == targetID)
            {
                return kvp.Key;
            }
        }
        return -1;
    }

    public void SetPlayerType(PlayerTeam type)
    {
        if (isServer)
        {
            playerTypes[localPlayerID] = type;
            int key = FindPlayerKey(localPlayerID);
            Message data = CreateNewPlayerMessage(key, playerNames[key], (int)playerTypes[key]);
            RGNetworkLongMessage msg = new RGNetworkLongMessage
            {
                playerID = data.senderID,
                type = (uint)data.Type,
                count = 1,
                payload = data.byteArguments.ToArray()
            };
            NetworkServer.SendToAll(msg);
            NotifyPlayerChanges(); // Notify PlayerLobbyManager of changes
        }
    }

    public void ChangePlayerTeam(int playerID, PlayerTeam newTeam)
    {
        if (isServer)
        {
            playerTypes[playerID] = newTeam;
            Message data = CreateNewPlayerMessage(playerID, playerNames[playerID], (int)newTeam);
            RGNetworkLongMessage msg = new RGNetworkLongMessage
            {
                playerID = data.senderID,
                type = (uint)data.Type,
                count = 1,
                payload = data.byteArguments.ToArray()
            };
            NetworkServer.SendToAll(msg);
            NotifyPlayerChanges(); // Notify PlayerLobbyManager of changes
        }
    }

    public Message CreateStartGameMessage()
    {
        Message msg;
        List<byte> data = new List<byte>(100);

        // Iterate through dictionary entries directly
        foreach (var kvp in playerIDs)
        {
            int playerId = kvp.Key;
            byte[] id = BitConverter.GetBytes(playerId);
            byte[] type = BitConverter.GetBytes((int)playerTypes[playerId]);
            int nameSize = playerNames[playerId].Length;
            byte[] nameSizeBytes = BitConverter.GetBytes(nameSize);
            byte[] name = Encoding.ASCII.GetBytes(playerNames[playerId]);
            data.AddRange(id);
            data.AddRange(type);
            data.AddRange(nameSizeBytes);
            data.AddRange(name);
        }

        msg = new Message(CardMessageType.StartGame, (uint)localPlayerID, data);
        return msg;
    }

    public Message CreateNewPlayerMessage(int playerID, string playerName, int type)
    {
        Message msg;
        List<byte> data = new List<byte>(100);
        int messageCount = 1;
        byte[] id = BitConverter.GetBytes(playerID);
        byte[] typeData = BitConverter.GetBytes(type);
        int nameSize = playerName.Length;
        byte[] nameSizeBytes = BitConverter.GetBytes(nameSize);
        byte[] name = Encoding.ASCII.GetBytes(playerName);
        data.AddRange(id);
        data.AddRange(typeData);
        data.AddRange(nameSizeBytes);
        data.AddRange(name);
        msg = new Message(CardMessageType.AddLobbyID, (uint)localPlayerID, data);
        return (msg);
    }

    // remove id from list
    public Message RemoveNewPlayerMessage(int playerID)
    {
        Message msg;
        List<byte> data = new List<byte>(100);
        int messageCount = 1;
        byte[] id = BitConverter.GetBytes(playerID);
        data.AddRange(id);
        msg = new Message(CardMessageType.RemLobbyID, (uint)localPlayerID, data);
        return (msg);
    }

    public void SetupHandlers()
    {
        NetworkClient.RegisterHandler<RGNetworkShortMessage>(OnClientReceiveShortMessage);
        NetworkServer.RegisterHandler<RGNetworkShortMessage>(OnServerReceiveShortMessage);
        NetworkClient.RegisterHandler<RGNetworkLongMessage>(OnClientReceiveLongMessage);
        NetworkServer.RegisterHandler<RGNetworkLongMessage>(OnServerReceiveLongMessage);
    }
    #endregion

    #region Helpers
    public void SyncPlayerListToClient(NetworkConnectionToClient conn)
    {
        if (!isServer) return;

        foreach (var kvp in playerIDs)
        {
            int id = kvp.Key;
            string name = playerNames[id];
            int type = (int)playerTypes[id];

            Message data = CreateNewPlayerMessage(id, name, type);
            RGNetworkLongMessage msg = new RGNetworkLongMessage
            {
                playerID = data.senderID,
                type = (uint)data.Type,
                count = 1,
                payload = data.byteArguments.ToArray()
            };
            conn.Send(msg);
        }

        // If the game has already started, send the StartGame message
        if (GameManager.Instance.gameStarted)
        {
            Message startMsg = CreateStartGameMessage();
            RGNetworkLongMessage startNetMsg = new RGNetworkLongMessage
            {
                playerID = startMsg.senderID,
                type = (uint)startMsg.Type,
                count = (uint)playerIDs.Count,
                payload = startMsg.byteArguments.ToArray()
            };
            conn.Send(startNetMsg);
        }
    }
    public void DebugLogPlayerLists()
    {
        Debug.Log($"Player List({playerIDs.Count}): ");
        for (int i = 0; i < playerIDs.Count; i++)
        {
            Debug.Log($"[{playerIDs[i]}]: {playerNames[i]}, team {playerTypes[i]}, has taken turn: {playerTurnTakenFlags[i]}");
        }
    }
    public List<int> GetConnectedPlayerIDs()
    {
        return playerIDs.Keys.ToList();
    }
    public int DrawCardForPlayer(int playerId)
    {
        int cardUID = nextCardUID++;
        drawnCardUIDs[playerId] = cardUID;
        return cardUID;
    }
    public void ResetAllPlayersToNotReady()
    {
        for (int i = 0; i < playerIDs.Count; i++)
        {
            playerTypes[i] = PlayerTeam.Any;
        }
    }
    public void RemovePlayer(int id)
    {
        playerIDs.Remove(id);
        playerTurnTakenFlags.Remove(id);
        playerNames.Remove(id);
        playerTypes.Remove(id);
        playerNetworkReadyFlags.Remove(id);
        playerConnections.Remove(id);

        // Instead of removing the turn flag, just mark it true to automatically pass turn
        if (playerTurnTakenFlags.ContainsKey(id))
        {
            playerTurnTakenFlags[id] = true;
            Debug.Log($"[RemovePlayer] Marking player {id} as having taken their turn.");
        }

        Message data = RemoveNewPlayerMessage(id);
        RGNetworkLongMessage msg = new RGNetworkLongMessage
        {
            playerID = data.senderID,
            type = (uint)data.Type,
            count = 1,
            payload = data.byteArguments.ToArray()
        };
        NetworkServer.SendToAll(msg);
        NotifyPlayerChanges();
        GameManager.Instance.CheckIfCanEndPhase(); // or whatever triggers phase progression
    }

    public void ResetAllPlayerTurnFlags()
    {
        List<int> keys = new List<int>(playerTurnTakenFlags.Keys);
        foreach (int key in keys)
        {
            playerTurnTakenFlags[key] = false;
        }
        Debug.Log("All playerTurnTakenFlags reset to false.");
    }


    public int GetIntFromByteArray(int indexStart, ArraySegment<byte> payload)
    {
        int returnValue = 0;
        byte first = payload.ElementAt(indexStart);
        byte second = payload.ElementAt(indexStart + 1);
        byte third = payload.ElementAt(indexStart + 2);
        byte fourth = payload.ElementAt(indexStart + 3);
        returnValue = first | (second << 8) | (third << 16) | (fourth << 24);
        return returnValue;
    }
    #endregion

    #region Direct Message Sending
    public void SendStringToClients(string stringMsg)
    {
        if (isServer)
        {
            Message msg = new Message(CardMessageType.LogAction, (uint)localPlayerID, stringMsg);
            RGNetworkLongMessage netMsg = new RGNetworkLongMessage
            {
                playerID = (uint)localPlayerID,
                type = (uint)msg.Type,
                count = (uint)msg.byteArguments.Count,
                payload = new ArraySegment<byte>(msg.byteArguments.ToArray())
            };

            // Send to all clients
            NetworkServer.SendToAll(netMsg);
        }
    }
    public void SendStringToServer(string stringMsg)
    {
        if (!isServer)
        {
            Message msg = new Message(CardMessageType.LogAction, (uint)localPlayerID, stringMsg);
            RGNetworkLongMessage netMsg = new RGNetworkLongMessage
            {
                playerID = (uint)localPlayerID,
                type = (uint)msg.Type,
                count = (uint)msg.byteArguments.Count,
                payload = new ArraySegment<byte>(msg.byteArguments.ToArray())
            };
            NetworkClient.Send(netMsg);
        }
    }

    //send message to specific client via net ID
    public void SendMessageToClient(int playerId, RGNetworkLongMessage msg)
    {
        if (playerConnections.TryGetValue(playerId, out NetworkConnectionToClient conn))
        {
            conn.Send<RGNetworkLongMessage>(msg);
        }
        else
        {
            Debug.LogError($"No connection found for player ID {playerId}");
        }
    }
    public void SendSectorDataMessage(int playerID, List<(int sectorType, bool[] sectorValues)> sectors)
    {
        if (!isServer) return;

        // Prepare a list of bytes to contain all the sector data
        List<byte> sectorData = new List<byte>();

        foreach (var sector in sectors)
        {
            // Add the sector type as an int (4 bytes)
            sectorData.AddRange(BitConverter.GetBytes(sector.sectorType));

            // Add the 3 boolean values as bytes 
            for (int i = 0; i < 3; i++)
            {
                sectorData.Add(sector.sectorValues[i] ? (byte)1 : (byte)0);
            }
        }

        // Create a new message with this data
        Message msg = new Message(CardMessageType.SendSectorData, (uint)localPlayerID, sectorData);
        RGNetworkLongMessage netMsg = new RGNetworkLongMessage
        {
            playerID = (uint)playerID,
            type = (uint)msg.Type,
            count = (uint)sectorData.Count,
            payload = new ArraySegment<byte>(sectorData.ToArray())
        };

        // Send to all clients
        NetworkServer.SendToAll(netMsg);
        Debug.Log("SERVER SENT sector data message to clients");
    }
    #endregion

    #region Update Observer
    public void UpdateObserver(Message data)
    {
        // send messages here over network to appropriate place(s)
        switch (data.Type)
        {
            case CardMessageType.StartGame:
                {
                    if (isServer)
                    {
                        // only servers start the game!
                        RGNetworkLongMessage msg = new RGNetworkLongMessage
                        {
                            playerID = data.senderID,
                            type = (uint)data.Type,
                            count = (uint)playerIDs.Count,
                            payload = data.byteArguments.ToArray()
                        };
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER SENT a new player name and id to clients");
                    }
                }
                break;
            case CardMessageType.SectorAssignment:
                {
                    if (isServer)
                    {
                        RGNetworkLongMessage sectorMsg = new RGNetworkLongMessage
                        {
                            playerID = data.senderID,
                            type = (uint)data.Type,
                            count = (uint)data.arguments.Count,
                            payload = data.arguments.SelectMany<int, byte>(BitConverter.GetBytes).ToArray()
                        };
                        NetworkServer.SendToAll(sectorMsg);
                        Debug.Log("SERVER SENT sector assignment to clients.");
                    }
                }
                break;

            case CardMessageType.EndPhase:
                {
                    // turn taking is handled here because the list of players on 
                    // the network happens here
                    RGNetworkShortMessage msg = new RGNetworkShortMessage
                    {
                        playerID = data.senderID,
                        type = (uint)data.Type
                    };
                    Debug.Log("update observer called end phase! ");
                    if (isServer)
                    {
                        // we've played so we're no longer on the ready list
                        int playerIndex = (int)msg.playerID;
                        if (playerTurnTakenFlags.ContainsKey(playerIndex))
                        {
                            playerTurnTakenFlags[playerIndex] = true;
                        }
                        else
                        {
                            Debug.LogWarning($"Player ID {playerIndex} not found in playerTurnTakenFlags during EndPhase.");
                            return;
                        }
                        // Only consider currently connected players
                        var connectedPlayerIds = RGNetworkPlayerList.instance.playerIDs.Keys;
                        bool allPlayersDone = connectedPlayerIds.All(id =>
                            playerTurnTakenFlags.TryGetValue(id, out bool taken) && taken);

                        if (allPlayersDone)
                        {
                            // Advance phase
                            Debug.Log("update observer everybody has ended phase!");
                            GamePhase nextPhase = manager.GetNextPhase();

                            // Reset turn flags for remaining players
                            foreach (int id in playerTurnTakenFlags.Keys.ToList())
                            {
                                playerTurnTakenFlags[id] = false;
                            }

                            // Notify clients
                            msg.type = (uint)CardMessageType.StartNextPhase;

                            foreach (var conn in NetworkServer.connections.Values)
                            {
                                if (conn != null && conn.isReady)
                                {
                                    conn.Send(msg);
                                    Debug.Log($"Sent StartNextPhase to connection {conn.connectionId}");
                                }
                            }

                            manager.StartNextPhase();

                            if (nextPhase == GamePhase.DrawRed)
                            {
                                manager.IncrementTurn();
                                Debug.Log("Turn is done - incrementing and starting again.");
                            }
                        }
                    }
                    else
                    {

                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT ENDED TURN AND GAVE IT BACK TO SERVER");
                    }
                }
                break;
            case CardMessageType.IncrementTurn:
                {
                    Debug.Log("update observer called increment turn! ");
                    if (isServer)
                    {
                        RGNetworkShortMessage msg = new RGNetworkShortMessage
                        {
                            playerID = data.senderID,
                            type = (uint)data.Type
                        };
                        NetworkServer.SendToAll(msg);
                        Debug.Log("sending turn increment to all clients");
                    }
                }

                break;
            case CardMessageType.SharePlayerType:
                {
                    // servers only receive types in separate messages
                    if (!isServer)
                    {
                        RGNetworkLongMessage msg = new RGNetworkLongMessage
                        {
                            playerID = data.senderID,
                            type = (uint)data.Type,
                            count = (uint)data.arguments.Count,
                            payload = data.arguments.SelectMany<int, byte>(BitConverter.GetBytes).ToArray()
                        };
                        Debug.Log("update observer called share player type ");

                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT IS SHOWING THEIR PLAYER TYPE AS " + data.ToString());
                    }
                }
                break;
            case CardMessageType.ShareDiscardNumber:
            case CardMessageType.CardUpdate:
            case CardMessageType.CardUpdateWithExtraFacilityInfo:
            case CardMessageType.ReduceCost:
            case CardMessageType.RemoveEffect:
            case CardMessageType.DiscardCard:
            case CardMessageType.SectorDieRoll:
            case CardMessageType.DrawCard:
            case CardMessageType.ReturnCardToDeck:
            case CardMessageType.ChangeCardID:
            case CardMessageType.MeepleShare:
                {
                    RGNetworkLongMessage msg = new RGNetworkLongMessage
                    {
                        playerID = data.senderID,
                        type = (uint)data.Type,
                        count = (uint)data.arguments.Count,
                        payload = data.arguments.SelectMany<int, byte>(BitConverter.GetBytes).ToArray()
                    };
                    // Log the message details before sending
                    string argString = string.Join(", ", data.arguments);
                    Debug.Log($"Sending {data.Type} message:" +
                        $"\nPlayer ID: {data.senderID}" +
                        $"\nArgument Count: {data.arguments.Count}" +
                        $"\nArguments: {argString}" +
                        $"\nPayload size: {msg.payload.Count} bytes");


                    if (!isServer)
                    {
                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT sent type: " + data.Type + " with value " + data.ToString());
                    }
                    else
                    {
                        // share it with everybody
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER sent type: " + data.Type + " with value " + data.ToString());
                    }
                }
                break;
            case CardMessageType.EndGame:
                {
                    RGNetworkShortMessage msg = new RGNetworkShortMessage
                    {
                        playerID = data.senderID,
                        type = (uint)data.Type
                    };
                    if (isServer)
                    {
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER SENT GAME END MESSAGE FIRST");
                    }
                    else
                    {
                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT SENT GAME END MESSAGE FIRST");
                    }
                }
                break;
            default:
                break;
        }

    }
    #endregion

    #region Client Receive Messages
    public void OnClientReceiveShortMessage(RGNetworkShortMessage msg)
    {
        Debug.Log("CLIENT RECEIVED SHORT MESSAGE::: " + msg.playerID + " " + msg.type);
        uint senderId = msg.playerID;
        CardMessageType type = (CardMessageType)msg.type;

        // NOTE: SENDTOALL ALSO SENDS THE MESSAGE TO THE SERVER AGAIN, WHICH WE DON'T NEED
        if (!isServer)
        {
            switch (type)
            {
                case CardMessageType.StartNextPhase:
                    Debug.Log("received start next phase message");
                    manager.StartNextPhase();
                    break;
                case CardMessageType.EndPhase:
                    // only the server should get and end turn message!
                    Debug.Log("client received end phase message and it shouldn't!");
                    break;
                case CardMessageType.IncrementTurn:
                    Debug.Log("client received increment turn message!");
                    manager.IncrementTurn();
                    break;
                case CardMessageType.EndGame:
                    {
                        if (!manager.HasReceivedEndGame())
                        {
                            manager.SetReceivedEndGame(true);
                            manager.AddMessage(new Message(CardMessageType.EndGame, (uint)localPlayerID));
                            manager.ShowEndGameCanvas();
                            Debug.Log("received end game message and will now end game on server");
                        }
                    }
                    break;
                default:
                    Debug.Log("client received unknown message!");
                    break;
            }
        }
    }
    public void OnClientReceiveLongMessage(RGNetworkLongMessage msg)
    {
        var senderName = msg.playerID.ToString();

        if (manager != null &&
            manager.playerDictionary != null &&
            manager.playerDictionary.TryGetValue((int)msg.playerID, out CardPlayer player))
        {
            senderName = player.playerName;
        }

        Debug.Log("CLIENT RECEIVED LONG MESSAGE::: From: " + senderName + " of type: " + (CardMessageType)msg.type);

        uint senderId = msg.playerID;
        CardMessageType type = (CardMessageType)msg.type;

        if (msg.playerID != localPlayerID && !isServer)
        {
            // we don't send messages to ourself
            switch (type)
            {
                case CardMessageType.AddLobbyID:
                    {
                        Debug.Log("client received message to add player to lobby");
                        int element = 0;

                        // Get the player ID
                        int id = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        // Get the player type
                        int typeData = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        // Get the length of the player name
                        int nameLength = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        // Get the player name
                        string playerName = Encoding.ASCII.GetString(msg.payload.Array, msg.payload.Offset + element, nameLength);

                        // Check if the player already exists
                        if (!playerIDs.ContainsKey(id))
                        {
                            // Player doesn't exist, add them
                            AddPotentialPlayer(id, playerName, typeData);
                            Debug.Log($"Added new player: {playerName} (ID: {id}, Type: {(PlayerTeam)typeData})");
                        }
                        else
                        {
                            // Player exists, update their type
                            playerTypes[id] = (PlayerTeam)typeData;
                            Debug.Log($"Updated player type for {playerName} (ID: {id}): {(PlayerTeam)typeData}");
                        }

                        // Notify the UI to update
                        NotifyPlayerChanges();
                    }
                    break;
                case CardMessageType.RemLobbyID:
                    {
                        Debug.Log("client received message to rem player from lobby");
                        int element = 0;
                        int id = GetIntFromByteArray(element, msg.payload);

                        // Replace FindIndex with ContainsKey check
                        if (playerIDs.ContainsKey(id))
                        {
                            Debug.Log("removing player msg received for player " + id);
                            RemovePlayer(id);
                            NotifyPlayerChanges();
                        }
                        break;
                    }
                case CardMessageType.StartGame:
                    {
                        Debug.Log("Client received message to start the game");
                        int element = 0;
                        for (int i = 0; i < msg.count; i++)
                        {
                            // Parse player ID
                            int newID = GetIntFromByteArray(element, msg.payload);
                            element += 4;

                            // Parse team type (PlayerTeam)
                            int teamType = GetIntFromByteArray(element, msg.payload);
                            element += 4;

                            // Parse name length
                            int nameLength = GetIntFromByteArray(element, msg.payload);
                            element += 4;

                            // Parse player name
                            ArraySegment<byte> nameBytes = msg.payload.Slice(element, nameLength);
                            string playerName = Encoding.ASCII.GetString(nameBytes.Array, nameBytes.Offset, nameLength);
                            element += nameLength;

                            // Add/update player data
                            if (!playerIDs.ContainsKey(newID))
                            {
                                playerIDs.Add(newID, newID);
                                playerNames.Add(newID, playerName);
                                playerTypes.Add(newID, (PlayerTeam)teamType);
                                Debug.Log($"Added new player: {playerName} (ID: {newID}, Team: {(PlayerTeam)teamType})");
                            }
                            else
                            {
                                playerTypes[newID] = (PlayerTeam)teamType;
                                Debug.Log($"Updated player {playerName} (ID: {newID}) to team: {(PlayerTeam)teamType}");
                            }
                        }
                        NotifyPlayerChanges();
                        manager.RealGameStart();
                        break;
                    }
                case CardMessageType.SendSectorData:
                    {
                        Debug.Log("Client received sector data message");
                        int element = 0;
                        while (element < msg.payload.Count)
                        {
                            // Read the sector type
                            SectorType sectorType = (SectorType)GetIntFromByteArray(element, msg.payload);
                            element += 4;

                            // Read the 3 boolean values
                            bool[] sectorValues = new bool[3];
                            for (int i = 0; i < 3; i++)
                            {
                                sectorValues[i] = msg.payload.ElementAt(element++) == 1;
                            }


                            Debug.Log($"Received SectorType: {sectorType}, Values: {string.Join(", ", sectorValues)}");
                            manager.GetSimulationStatusFromNetwork(sectorType, sectorValues);
                        }
                    }
                    break;
                case CardMessageType.LogAction:
                    {
                        string receivedString = Encoding.UTF8.GetString(msg.payload.ToArray());
                        Debug.Log("CLIENT RECEIVED string message: " + receivedString);
                        manager.AddActionLogMessage(receivedString, true); //now log the message to the action log
                        // Handle the received string here
                    }
                    break;
                case CardMessageType.SectorAssignment:
                    {
                        int element = 0;
                        int playerIndex = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int sectorIndex = GetIntFromByteArray(element, msg.payload); // Use this directly

                        // Call with sectorIndex (int), not the Sector object
                        GameManager.Instance.AssignSectorToPlayer(playerIndex, sectorIndex);
                        break;
                    }
                case CardMessageType.SectorDieRoll:
                    {
                        // Get the assigned sector index from the message payload
                        int element = 0;
                        SectorType sectorPlayedOn = (SectorType)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int roll = GetIntFromByteArray(element, msg.payload);
                        if (GameManager.Instance.AllSectors.TryGetValue(sectorPlayedOn, out Sector sector))
                        {
                            sector.UpdateDieRollFromNetwork(roll);
                        }
                        else
                        {
                            Debug.LogError($"Could not find sector {(int)sectorPlayedOn}");
                        }
                        Debug.Log("CLIENT RECEIVED sector die roll: Sector " + sectorPlayedOn + " roll " + roll);
                    }
                    break;
                case CardMessageType.ShareDiscardNumber:
                    {
                        uint count = msg.count;

                        Debug.Log("client received a player's discard amount!" + count);
                        if (count == 1)
                        {
                            // turn the first element into an int
                            int discardCount = BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.playerID;

                            Debug.Log("setting player discard to " + discardCount);

                            // let the game manager display the new info
                            UserInterface.Instance.DisplayGameStatus("Player " + playerNames[playerIndex] +
                                " discarded " + discardCount + " cards.");
                        }
                    }
                    break;
                case CardMessageType.ChangeCardID:
                    {
                        int element = 0;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int newUID = GetIntFromByteArray(element, msg.payload);

                        // Check if this message is for the current client
                        if (msg.playerID == localPlayerID)
                        {
                            // Update the card with the new UID
                            if (manager.actualPlayer.HandCards.TryGetValue(cardId, out GameObject cardGo))
                            {
                                if (cardGo.TryGetComponent(out Card drawnCard))
                                {
                                    if (drawnCard != null)
                                    {
                                        drawnCard.UniqueID = newUID;
                                        Debug.Log($"Client updated card {cardId} with new UID {newUID}");
                                        break;
                                    }
                                }
                            }
                            Debug.LogError($"Client received a card UID update for a non existent card");
                        }
                    }
                    break;
                case CardMessageType.DrawCard:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update
                        {
                            Type = CardMessageType.DrawCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("client received draw card message from opponent containing playerID : " + msg.playerID + " and card uid: " + uniqueId + " for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.ReturnCardToDeck:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.ReturnCardToDeck,
                            UniqueID = uniqueId,
                            CardID = cardId
                        };
                        Debug.Log("client received ReturnCardToHand message from opponent containing playerID : " + msg.playerID + " and card id: " + cardId + " for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.CardUpdate:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int sectorType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffectToRemoveType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        Update update = new Update
                        {
                            Type = CardMessageType.CardUpdate,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            sectorPlayedOn = (SectorType)sectorType,
                            FacilityPlayedOnType = (FacilityType)facilityType,
                            Amount = amount,
                        };
                        Debug.Log("client received update message from opponent containing playerID : " + msg.playerID + " and card id: " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.CardUpdateWithExtraFacilityInfo:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int sectorType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int effectType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffect1 = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffect2 = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffect3 = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.CardUpdateWithExtraFacilityInfo,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            sectorPlayedOn = (SectorType)sectorType,
                            FacilityPlayedOnType = (FacilityType)facilityType,
                            FacilityEffectToRemoveType = (FacilityEffectType)effectType,
                            AdditionalFacilitySelectedOne = (FacilityType)facilityEffect1,
                            AdditionalFacilitySelectedTwo = (FacilityType)facilityEffect2,
                            AdditionalFacilitySelectedThree = (FacilityType)facilityEffect3,
                        };
                        Debug.Log("client received update message from opponent containing playerID : " + uniqueId + " and card id: " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.ReduceCost:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.ReduceCost,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            Amount = amount,
                        };
                        Debug.Log("client received update message from opponent containing playerID : " + msg.playerID + " and card id: " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.RemoveEffect:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int effect = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.RemoveEffect,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            FacilityPlayedOnType = (FacilityType)facilityType,
                        };
                        Debug.Log("client received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.DiscardCard:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update
                        {
                            Type = CardMessageType.DiscardCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("client received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.MeepleShare:
                    {
                        Debug.Log("Processing meeple share message in client received");
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int playerToShareWith = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int meepleColor = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update
                        {
                            Type = CardMessageType.MeepleShare,
                            UniqueID = playerToShareWith,
                            CardID = meepleColor,
                            Amount = amount,
                        };
                        Debug.Log("client received update message from opponent containing player to share with : " + playerToShareWith +
                            " and meeple color " + meepleColor + "for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    #region Server Receive Messages
    public void OnServerReceiveShortMessage(NetworkConnectionToClient client, RGNetworkShortMessage msg)
    {
        Debug.Log("SERVER RECEIVED SHORT MESSAGE::: " + msg.playerID + " " + msg.type);
        uint senderId = msg.playerID;
        CardMessageType type = (CardMessageType)msg.type;
        // Update the connection mapping
        int playerId = (int)msg.playerID;
        if (!playerConnections.ContainsKey(playerId))
        {
            playerConnections[playerId] = client;
        }
        switch (type)
        {
            case CardMessageType.StartNextPhase:
                // nobody tells server to start a turn, so this shouldn't happen
                Debug.Log("server start next phase message when it shouldn't!");
                break;
            case CardMessageType.EndPhase:
                // end turn is handled here because the player list is kept
                // in this class
                Debug.Log("server received end phase message from sender: " + senderId);
                // note this player's turn has ended      
                int playerIndex = (int)senderId;
                playerTurnTakenFlags[playerIndex] = true;
                // find next player to ok to play and send them a message
                //int nextPlayerId = -1;

                Debug.Log("Turn flags snapshot: " + string.Join(", ",
    playerTurnTakenFlags.Select(kvp => $"P{kvp.Key}:{kvp.Value}")));

                // Only consider currently connected players
                var connectedPlayerIds = RGNetworkPlayerList.instance.playerIDs.Keys;
                bool allPlayersDone = connectedPlayerIds.All(id =>
                    playerTurnTakenFlags.TryGetValue(id, out bool taken) && taken);

                if (allPlayersDone)
                {
                    GamePhase nextPhase = manager.GetNextPhase();

                    // need to increment the turn and set all the players to ready again
                    foreach (int id in playerTurnTakenFlags.Keys.ToList())
                    {
                        playerTurnTakenFlags[id] = RGNetworkPlayerList.instance.playerIDs.ContainsKey(id) ? false : true;
                    }

                    // tell all the clients to go to the next phase
                    msg.playerID = (uint)localPlayerID;
                    msg.type = (uint)CardMessageType.StartNextPhase;
                    NetworkServer.SendToAll(msg);
                    // server needs to start next phase as well
                    manager.StartNextPhase();
                    if (nextPhase == GamePhase.DrawRed)
                    {
                        manager.IncrementTurn();
                        Debug.Log("Turn is done - incrementing and starting again.");
                    }
                }
                break;
            case CardMessageType.IncrementTurn:
                Debug.Log("Server received increment message and did nothing.");
                break;
            case CardMessageType.EndGame:
                {
                    if (!manager.HasReceivedEndGame())
                    {
                        manager.SetReceivedEndGame(true);
                        manager.AddMessage(new Message(CardMessageType.EndGame, (uint)localPlayerID));
                        manager.ShowEndGameCanvas();
                        Debug.Log("received end game message and will now end game on server");
                    }
                }
                break;
            default:
                break;
        }

    }
    public void OnServerReceiveLongMessage(NetworkConnectionToClient client, RGNetworkLongMessage msg)
    {
        var playerName = msg.playerID + "";
        if (manager != null && manager.playerDictionary != null && manager.playerDictionary.TryGetValue((int)msg.playerID, out CardPlayer player))
        {
            playerName = player.playerName;
        }

        Debug.Log("SERVER RECEIVED LONG MESSAGE::: From: " + playerName + " of type: " + (CardMessageType)msg.type); uint senderId = msg.playerID;
        CardMessageType type = (CardMessageType)msg.type;
        // Update the connection mapping
        int playerId = (int)msg.playerID;
        if (!playerConnections.ContainsKey(playerId))
        {
            playerConnections[playerId] = client;
        }
        if (msg.playerID != localPlayerID)
        {
            switch (type)
            {
                case CardMessageType.SharePlayerType:
                    {
                        uint count = msg.count;
                        Debug.Log("server received a player's type!" + count);
                        if (count == 1)
                        {
                            PlayerTeam playerType = (PlayerTeam)BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.playerID;

                            // Replace FindIndex with ContainsKey check
                            if (playerIDs.ContainsKey(playerIndex))
                            {
                                playerTypes[playerIndex] = playerType;
                                playerTurnTakenFlags[playerIndex] = true;
                                Debug.Log("setting player type to " + playerType);
                                // send info about player to everybody
                                // and update the lobby
                                Message data = CreateNewPlayerMessage(playerIndex, playerNames[playerIndex], (int)playerTypes[playerIndex]);
                                RGNetworkLongMessage msg2 = new RGNetworkLongMessage
                                {
                                    playerID = data.senderID,
                                    type = (uint)data.Type,
                                    count = 1,
                                    payload = data.byteArguments.ToArray()
                                };
                                NetworkServer.SendToAll(msg2);
                                NotifyPlayerChanges();
                            }
                        }
                    }
                    break;
                case CardMessageType.ShareDiscardNumber:
                    {
                        uint count = msg.count;

                        Debug.Log("server received a player's discard amount!" + count);
                        if (count == 1)
                        {
                            // turn the first element into an int
                            int discardCount = BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.playerID;

                            Debug.Log("setting player discard to " + discardCount);

                            // let the game manager display the new info
                            UserInterface.Instance.DisplayGameStatus("Player " + playerNames[playerIndex] +
                                " discarded " + discardCount + " cards.");
                        }
                    }
                    break;
                case CardMessageType.LogAction:
                    {
                        string receivedString = Encoding.UTF8.GetString(msg.payload.ToArray());
                        Debug.Log("SERVER RECEIVED string message: " + receivedString);
                        manager.AddActionLogMessage(receivedString, true); //log the action locally
                    }
                    break;
                case CardMessageType.SectorDieRoll:
                    {
                        // Get the assigned sector index from the message payload
                        int element = 0;
                        SectorType sectorPlayedOn = (SectorType)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int roll = -1;
                        if (GameManager.Instance.AllSectors.TryGetValue(sectorPlayedOn, out Sector sector))
                        {
                            roll = sector.SectorRollDie();
                        }
                        else
                        {
                            Debug.LogError($"Could not find sector {(int)sectorPlayedOn}");
                        }
                        Debug.Log("Server RECEIVED sector die roll request from sector " + sectorPlayedOn + " and rolled " + roll);
                    }
                    break;
                case CardMessageType.DrawCard:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        NetworkServer.SendToAll(msg); //relay draw card message to clients

                        Update update = new Update
                        {
                            Type = CardMessageType.DrawCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("server received draw card message from opponent containing playerID : " + uniqueId + " and card uid: " + uniqueId + " for game phase " + gamePhase);

                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.ReturnCardToDeck:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.ReturnCardToDeck,
                            UniqueID = uniqueId,
                            CardID = cardId
                        };
                        Debug.Log("server received ReturnCardToHand message from opponent containing playerID : " + msg.playerID + " and card id: " + cardId + " for game phase " + gamePhase);
                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.CardUpdate:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int sectorType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);

                        element += 4;

                        Update update = new Update
                        {
                            Type = CardMessageType.CardUpdate,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            sectorPlayedOn = (SectorType)sectorType,
                            FacilityPlayedOnType = (FacilityType)facilityType
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);
                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.CardUpdateWithExtraFacilityInfo:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int sectorType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int effectType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffect1 = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffect2 = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityEffect3 = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.CardUpdateWithExtraFacilityInfo,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            sectorPlayedOn = (SectorType)sectorType,
                            FacilityPlayedOnType = (FacilityType)facilityType,
                            FacilityEffectToRemoveType = (FacilityEffectType)effectType,
                            AdditionalFacilitySelectedOne = (FacilityType)facilityEffect1,
                            AdditionalFacilitySelectedTwo = (FacilityType)facilityEffect2,
                            AdditionalFacilitySelectedThree = (FacilityType)facilityEffect3,
                        };
                        Debug.Log("server received update message from opponent containing playerID : " + uniqueId + " and card id: " + cardId + "for game phase " + gamePhase);
                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.ReduceCost:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.ReduceCost,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            Amount = amount,
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);
                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.RemoveEffect:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int effect = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update
                        {
                            Type = CardMessageType.RemoveEffect,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            FacilityPlayedOnType = (FacilityType)facilityType,
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);
                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.DiscardCard:
                    {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update
                        {
                            Type = CardMessageType.DiscardCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);
                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                case CardMessageType.MeepleShare:
                    {
                        Debug.Log("Processing meeple share message in server received");
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int playerToShareWith = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int meepleColor = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update
                        {
                            Type = CardMessageType.MeepleShare,
                            UniqueID = playerToShareWith,
                            CardID = meepleColor,
                            Amount = amount,
                        };
                        Debug.Log("server received update message from opponent containing player to share with : " + playerToShareWith +
                            " and meeple color " + meepleColor + "for game phase " + gamePhase);

                        NetworkServer.SendToAll(msg); //relay to all clients
                        manager.AddUpdateFromPlayer(update, gamePhase, msg.playerID);
                    }
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    public void NotifyPlayerChanges()
    {
        PlayerLobbyManager.Instance.players.Clear();
        foreach (var kvp in playerIDs)
        {
            int id = kvp.Key;
            string name = playerNames[id];
            PlayerTeam team = playerTypes.ContainsKey(id) ? playerTypes[id] : PlayerTeam.Any;

            PlayerLobbyManager.Instance.players.Add(new PlayerData
            {
                Name = name,
                Team = team // Include "Any" (unassigned) players
            });
        }
        PlayerLobbyManager.Instance.UpdatePlayerLobbyUI();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        GameManager.Instance.HandlePlayerDisconnect(localPlayerID);
    }
}
