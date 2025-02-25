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
    public List<int> playerIDs = new List<int>();
    private List<bool> playerNetworkReadyFlags = new List<bool>();
    public List<bool> playerTurnTakenFlags = new List<bool>();
    public List<PlayerTeam> playerTypes = new List<PlayerTeam>();
    public List<string> playerNames = new List<string>();
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
        for (int i = 0; i < playerIDs.Count; i++)
        {
            if (playerTypes[i] == PlayerTeam.Any)
            {
                readyToStart = false;
                UserInterface.Instance.hostLobbyBeginError.GetComponentInChildren<TextMeshProUGUI>().text = "Not everyone is ready yet.";
                break;
            }
        }
        return readyToStart;
    }
    public void AddWhitePlayer()
    {
        playerIDs.Add(playerIDs.Count);
        playerNetworkReadyFlags.Add(true); // AI is always ready
        playerTurnTakenFlags.Add(false);
        playerTypes.Add(PlayerTeam.White); // Define PlayerTeam.AI in your enum if not done
        playerNames.Add("White_Player");
        GameManager.Instance.whitePlayer.NetID = playerIDs.Count - 1;
        // No need to add a NetworkConnection for the AI
    }

    // Assumes this doesn't exist in the list yet so please check
    // before calling
    public void AddPotentialPlayer(int id, string name, int type)
    {
        playerIDs.Add(id);
        playerNetworkReadyFlags.Add(true);
        playerTurnTakenFlags.Add(false);
        playerTypes.Add((PlayerTeam)type);
        playerNames.Add(name);
    }

    public void AddPlayer(int id, string name, CardPlayer cardPlayer, NetworkConnectionToClient conn)
    {
        if (isServer)
        {
            Debug.Log("adding player to server : " + id);
            playerIDs.Add(id);
            playerNetworkReadyFlags.Add(true);
            playerTurnTakenFlags.Add(false);
            playerTypes.Add(PlayerTeam.Any);
            playerConnections[id] = conn; // Store the connection
            playerNames.Add(name);
            if (id != 0)
            {
                manager.networkPlayers.Add(cardPlayer);
            }
            int count = 0;
            // every time somebody joins we need to send the whole list to them
            // and update everybody else
            foreach (int playerID in playerIDs)
            {
                Message data = CreateNewPlayerMessage(playerID, playerNames[count], (int)playerTypes[count]);
                // only servers start the game!
                RGNetworkLongMessage msg = new RGNetworkLongMessage
                {
                    playerID = data.senderID,
                    type = (uint)data.Type,
                    count = 1,
                    payload = data.byteArguments.ToArray()
                };
                NetworkServer.SendToAll(msg);

                count++;
            }
            NotifyPlayerChanges(); // Notify PlayerLobbyManager of changes
            PlayerLobbyManager.Instance.UpdatePlayerLobbyUI(); // Update the lobby screen when a player is added
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

    public void SetPlayerType(PlayerTeam type)
    {
        if (isServer)
        {
            playerTypes[localPlayerID] = type;
            int id = playerIDs.FindIndex(x => x == localPlayerID);
            // make sure to update the lobby
            Message data = CreateNewPlayerMessage(id, playerNames[id], (int)playerTypes[id]);
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
        int messageCount = playerIDs.Count;
        for (int i = 0; i < messageCount; i++)
        {
            // note that the player id is actually its order in this
            // message
            byte[] id = BitConverter.GetBytes((int)playerIDs[i]);
            byte[] type = BitConverter.GetBytes((int)playerTypes[i]);
            int nameSize = playerNames[i].Length;
            byte[] nameSizeBytes = BitConverter.GetBytes(nameSize);
            byte[] name = Encoding.ASCII.GetBytes(playerNames[i]);
            data.AddRange(id);
            data.AddRange(type);
            data.AddRange(nameSizeBytes);
            data.AddRange(name);
        }
        msg = new Message(CardMessageType.StartGame, (uint)localPlayerID, data);
        return (msg);
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
    public void DebugLogPlayerLists()
    {
        Debug.Log($"Player List({playerIDs.Count}): ");
        for (int i = 0; i < playerIDs.Count; i++)
        {
            Debug.Log($"[{playerIDs[i]}]: {playerNames[i]}, team {playerTypes[i]}, has taken turn: {playerTurnTakenFlags[i]}");
        }
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
        if (isServer)
        {
            int index = playerIDs.FindIndex(x => x == id);
            if (index != -1) // this check right here needs to be updated?
            {
                playerIDs.Remove(id);
                playerNames.RemoveAt(index);
                playerTypes.RemoveAt(index);
                playerNetworkReadyFlags.RemoveAt(index);
                playerTurnTakenFlags.RemoveAt(index);
                playerConnections.Remove(id); // Remove the connection
                // need to send a message to delete player from the clients
                Message data = RemoveNewPlayerMessage(id);
                // only servers start the game!
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
        else
        {
            int index = playerIDs.FindIndex(x => x == id);
            if (index != -1)
            {
                Debug.Log("removing player " + id);
                playerIDs.Remove(id);
                playerNames.RemoveAt(index);
                playerTypes.RemoveAt(index);
                playerNetworkReadyFlags.RemoveAt(index);
                playerTurnTakenFlags.RemoveAt(index);
            }
        }
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
                        playerTurnTakenFlags[playerIndex] = true;
                        // find next player to ok to play and send them a message
                        int nextPlayerId = -1;
                        for (int i = 0; i < playerTurnTakenFlags.Count; i++)
                        {
                            if (!playerTurnTakenFlags[i])
                            {
                                nextPlayerId = i;
                                Debug.Log("first player not done is " + i);
                                break;
                            }
                        }

                        if (nextPlayerId == -1)
                        {
                            Debug.Log("update observer everybody has ended phase!");
                            GamePhase nextPhase = manager.GetNextPhase();

                            // need to increment the turn and set all the players to ready again
                            for (int i = 0; i < playerTurnTakenFlags.Count; i++)
                            {
                                playerTurnTakenFlags[i] = false;
                            }

                            // tell all the clients to go to the next phase
                            msg.type = (uint)CardMessageType.StartNextPhase;
                            NetworkServer.SendToAll(msg);

                            // server needs to start their next phase too
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
        var playerName = msg.playerID + "";
        if (manager != null && manager.playerDictionary != null && manager.playerDictionary.TryGetValue((int)msg.playerID, out CardPlayer player))
        {
            playerName = player.playerName;
        }

        Debug.Log("CLIENT RECEIVED LONG MESSAGE::: From: " + playerName + " of type: " + (CardMessageType)msg.type);

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
                        int actualInt = 0;
                        int typeData = 0;
                        int id = 0;
                        // first, get the player id we're adding
                        id = GetIntFromByteArray(element, msg.payload);
                        // make sure the player isn't added twice
                        int existingPlayer = playerIDs.FindIndex(x => x == id);

                        if (existingPlayer == -1)
                        {
                            // get the player type
                            element += 4;
                            typeData = GetIntFromByteArray(element, msg.payload);

                            // get length of player name
                            element += 4;
                            actualInt = GetIntFromByteArray(element, msg.payload);

                            // get player name
                            element += 4;
                            ArraySegment<byte> name = msg.payload.Slice(element, actualInt);

                            // since this doesn't exist, add it
                            AddPotentialPlayer(id, Encoding.ASCII.GetString(name), typeData);
                            NotifyPlayerChanges(); // Update the lobby screen when a player is added
                        }
                        else
                        {
                            // this is only changing the player type
                            element += 4;
                            typeData = GetIntFromByteArray(element, msg.payload);
                            playerTypes[existingPlayer] = (PlayerTeam)typeData;
                            Debug.Log(id + " player type changed to: " + playerTypes[existingPlayer]);
                            NotifyPlayerChanges(); // Update the lobby screen when a player is added
                        }
                    }
                    break;
                case CardMessageType.RemLobbyID:
                    {
                        Debug.Log("client received message to rem player from lobby");
                        int element = 0;
                        int id = 0;
                        // first, get the player id
                        id = GetIntFromByteArray(element, msg.payload);
                        // does it exist
                        int existingPlayer = playerIDs.FindIndex(x => x == id);

                        if (existingPlayer != -1)
                        {
                            Debug.Log("removing player msg received for player " + id);
                            RemovePlayer(id);
                            NotifyPlayerChanges();
                        }
                    }
                    break;
                case CardMessageType.StartGame:
                    {
                        Debug.Log("client received message to start the game");
                        uint count = msg.count;
                        int element = 0;
                        for (int i = 0; i < count; i++)
                        {
                            // player id is the first arg
                            int newID = GetIntFromByteArray(element, msg.payload);
                            int existingPlayer = playerIDs.FindIndex(x => x == newID);

                            element += 4;
                            // next is type
                            int actualInt = GetIntFromByteArray(element, msg.payload);
                            // get length of player name
                            element += 4;
                            actualInt = GetIntFromByteArray(element, msg.payload);

                            // get player name
                            element += 4;
                            ArraySegment<byte> name = msg.payload.Slice(element, actualInt);
                            element += actualInt;

                            if (existingPlayer == -1)
                            {
                                playerIDs.Add(newID);
                                playerTypes.Add((PlayerTeam)actualInt);
                                playerNames.Add(Encoding.ASCII.GetString(name));
                                Debug.Log("player being added : " + playerIDs[i] + " " + playerTypes[i] +
                                    " " + playerNames[i]);
                            }
                            else
                            {
                                // when a game is reset we only need the player type again
                                playerTypes[existingPlayer] = (PlayerTeam)actualInt;
                                Debug.Log("player " + playerNames[existingPlayer] + " already exists! new type is: " + playerTypes[existingPlayer]);

                            }
                            NotifyPlayerChanges();
                        }
                        // no start game
                        manager.RealGameStart();

                    }
                    break;
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
                        // Get the assigned sector index from the message payload
                        int element = 0;
                        int playerIndex = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int sectorIndex = GetIntFromByteArray(element, msg.payload);
                        manager.AssignSectorToPlayer(playerIndex, sectorIndex);



                        Debug.Log("CLIENT RECEIVED sector assignment: Sector " + sectorIndex);
                    }
                    break;
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
                int nextPlayerId = -1;
                for (int i = 0; i < playerTurnTakenFlags.Count; i++)
                {
                    if (!playerTurnTakenFlags[i])
                    {
                        nextPlayerId = playerIDs[i];
                        break;
                    }
                }
                if (nextPlayerId == -1)
                {
                    GamePhase nextPhase = manager.GetNextPhase();

                    // need to increment the turn and set all the players to ready again
                    for (int i = 0; i < playerTurnTakenFlags.Count; i++)
                    {
                        playerTurnTakenFlags[i] = false;
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
            if (!playerIDs.Contains(playerId))
            {
                Debug.LogWarning($"Received message from removed player ID {playerId}. Ignoring message.");
                return;
            }
            switch (type)
            {
                case CardMessageType.SharePlayerType:
                    {
                        uint count = msg.count;
                        Debug.Log("server received a player's type!" + count);
                        if (count == 1)
                        {
                            // turn the first element into an int
                            PlayerTeam playerType = (PlayerTeam)BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.playerID;
                            Debug.Log("player type being set for player: " + playerIndex);
                            int index = playerIDs.FindIndex(x => x == playerIndex);
                            if (index != -1)
                            {
                                playerTypes[index] = playerType;
                                playerTurnTakenFlags[index] = true;
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
        for (int i = 0; i < playerIDs.Count; i++)
        {
            PlayerLobbyManager.Instance.players.Add(new PlayerData { Name = playerNames[i], Team = playerTypes[i] });
        }
        PlayerLobbyManager.Instance.UpdatePlayerLobbyUI();
    }

}
