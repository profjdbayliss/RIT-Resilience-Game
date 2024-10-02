using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem.Utilities;
using static Facility;
using UnityEngine.PlayerLoop;
using System.Xml;
using UnityEngine.XR;

// many messages actually have no arguments
public struct RGNetworkShortMessage : NetworkMessage {
    public uint indexId;
    public uint type;
}

// for messages that need to update one or more arguments
public struct RGNetworkLongMessage : NetworkMessage {
    public uint indexId;
    public uint type;
    // number of arguments
    public uint count;
    // the parameters for the message
    // -> ArraySegment to avoid unnecessary allocations
    public ArraySegment<byte> payload;
}

public class RGNetworkPlayerList : NetworkBehaviour, IRGObserver {
    public static RGNetworkPlayerList instance;

    public int localPlayerID;
    public string localPlayerName;
    public List<int> playerIDs = new List<int>();
    private GameManager manager;

    private List<bool> playerNetworkReadyFlags = new List<bool>();
    private List<bool> playerTurnTakenFlags = new List<bool>();
    public List<PlayerTeam> playerTypes = new List<PlayerTeam>();
    public List<string> playerNames = new List<string>();

    private void Awake() {
        instance = this;
        DontDestroyOnLoad(this);
        SetupHandlers();
    }

    public void Start() {
        manager = GameObject.FindObjectOfType<GameManager>();
        Debug.Log("start run on RGNetworkPlayerList.cs");
    }
    public void AddPlayer(int id, string name) {
        if (isServer) {
            Debug.Log("adding player to server : " + id);
            playerIDs.Add(id);
            playerNetworkReadyFlags.Add(true);
            playerTurnTakenFlags.Add(false);
            playerTypes.Add(PlayerTeam.Any);
            playerNames.Add(name);

        }
    }

    public void ResetAllPlayersToNotReady() {
        for (int i = 0; i < playerIDs.Count; i++) {
            playerTypes[i] = PlayerTeam.Any;
        }
    }

    public void SetPlayerType(PlayerTeam type) {
        if (isServer) {
            playerTypes[localPlayerID] = type;
            if (CheckReadyToStart()) {
                Debug.Log("Ready to start server is last!!");
                manager.RealGameStart();
                // get the turn taking flags ready to go again
                for (int i = 0; i < playerTurnTakenFlags.Count; i++) {
                    playerTurnTakenFlags[i] = false;
                }
            }
            else {
                Debug.Log("not ready to start!");
            }
        }
    }

    public Message CreateStartGameMessage() {
        Message msg;
        List<byte> data = new List<byte>(100);
        int messageCount = playerIDs.Count;
        for (int i = 0; i < messageCount; i++) {
            // note that the player id is actually its order in this
            // message
            byte[] type = BitConverter.GetBytes((int)playerTypes[i]);
            int nameSize = playerNames[i].Length;
            byte[] nameSizeBytes = BitConverter.GetBytes(nameSize);
            byte[] name = Encoding.ASCII.GetBytes(playerNames[i]);
            data.AddRange(type);
            data.AddRange(nameSizeBytes);
            data.AddRange(name);
        }
        msg = new Message(CardMessageType.StartGame, data);
        return (msg);
    }

    public void RemovePlayer(int id) {
        if (!isServer) return;

        int index = playerIDs.FindIndex(x => x == id);
        if (index != -1) {
            playerIDs.Remove(id);
            playerNames.RemoveAt(index);
            playerTypes.RemoveAt(index);
            playerNetworkReadyFlags.RemoveAt(index);
            playerTurnTakenFlags.RemoveAt(index);
        }
    }

    public int GetIntFromByteArray(int indexStart, ArraySegment<byte> payload) {
        int returnValue = 0;
        byte first = payload.ElementAt(indexStart);
        byte second = payload.ElementAt(indexStart + 1);
        byte third = payload.ElementAt(indexStart + 2);
        byte fourth = payload.ElementAt(indexStart + 3);
        returnValue = first | (second << 8) | (third << 16) | (fourth << 24);
        return returnValue;
    }

    public void UpdateObserver(Message data) {
        // send messages here over network to appropriate place(s)
        switch (data.Type) {
            case CardMessageType.StartGame: {
                    if (isServer) {
                        // only servers start the game!
                        RGNetworkLongMessage msg = new RGNetworkLongMessage {
                            indexId = (uint)localPlayerID,
                            type = (uint)data.Type,
                            count = (uint)playerIDs.Count,
                            payload = data.byteArguments.ToArray()
                        };
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER SENT a new player name and id to clients");
                    }
                }
                break;
            case CardMessageType.EndPhase: {
                    // turn taking is handled here because the list of players on 
                    // the network happens here
                    RGNetworkShortMessage msg = new RGNetworkShortMessage {
                        indexId = (uint)localPlayerID,
                        type = (uint)data.Type
                    };
                    Debug.Log("update observer called end phase! ");
                    if (isServer) {
                        // we've played so we're no longer on the ready list
                        int playerIndex = localPlayerID;
                        playerTurnTakenFlags[playerIndex] = true;
                        // find next player to ok to play and send them a message
                        int nextPlayerId = -1;
                        for (int i = 0; i < playerTurnTakenFlags.Count; i++) {
                            if (!playerTurnTakenFlags[i]) {
                                nextPlayerId = i;
                                Debug.Log("first player not done is " + i);
                                break;
                            }
                        }

                        if (nextPlayerId == -1) {
                            Debug.Log("update observer everybody has ended phase!");
                            GamePhase nextPhase = manager.GetNextPhase();

                            // need to increment the turn and set all the players to ready again
                            for (int i = 0; i < playerTurnTakenFlags.Count; i++) {
                                playerTurnTakenFlags[i] = false;
                            }

                            // tell all the clients to go to the next phase
                            msg.type = (uint)CardMessageType.StartNextPhase;
                            NetworkServer.SendToAll(msg);

                            // server needs to start their next phase too
                            manager.StartNextPhase();

                            if (nextPhase == GamePhase.DrawRed) {
                                manager.IncrementTurn();
                                Debug.Log("Turn is done - incrementing and starting again.");
                            }
                        }
                    }
                    else {

                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT ENDED TURN AND GAVE IT BACK TO SERVER");
                    }
                }
                break;
            case CardMessageType.IncrementTurn: {
                    Debug.Log("update observer called increment turn! ");
                    if (isServer) {
                        RGNetworkShortMessage msg = new RGNetworkShortMessage {
                            indexId = (uint)localPlayerID,
                            type = (uint)data.Type
                        };
                        NetworkServer.SendToAll(msg);
                        Debug.Log("sending turn increment to all clients");
                    }
                }

                break;
            case CardMessageType.SharePlayerType: {
                    // servers only receive types in separate messages
                    if (!isServer) {
                        RGNetworkLongMessage msg = new RGNetworkLongMessage {
                            indexId = (uint)localPlayerID,
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
            case CardMessageType.ReduceCost:
            case CardMessageType.RemoveEffect:
            case CardMessageType.DiscardCard:
            case CardMessageType.DrawCard:
            case CardMessageType.MeepleShare: {
                    RGNetworkLongMessage msg = new RGNetworkLongMessage {
                        indexId = (uint)localPlayerID,
                        type = (uint)data.Type,
                        count = (uint)data.arguments.Count,
                        payload = data.arguments.SelectMany<int, byte>(BitConverter.GetBytes).ToArray()
                    };
                    // Log the message details before sending
                    string argString = string.Join(", ", data.arguments);
                    Debug.Log($"Sending {data.Type} message:" +
                        $"\nPlayer ID: {localPlayerID}" +
                        $"\nArgument Count: {data.arguments.Count}" +
                        $"\nArguments: {argString}" +
                        $"\nPayload size: {msg.payload.Count} bytes");


                    if (!isServer) {
                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT sent type: " + data.Type + " with value " + data.ToString());
                    }
                    else {
                        // share it with everybody
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER sent type: " + data.Type + " with value " + data.ToString());
                    }
                }
                break;
            case CardMessageType.EndGame: {
                    RGNetworkShortMessage msg = new RGNetworkShortMessage {
                        indexId = (uint)localPlayerID,
                        type = (uint)data.Type
                    };
                    if (isServer) {
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER SENT GAME END MESSAGE FIRST");
                    }
                    else {
                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT SENT GAME END MESSAGE FIRST");
                    }
                }
                break;
            default:
                break;
        }

    }

    public void OnClientReceiveShortMessage(RGNetworkShortMessage msg) {
        Debug.Log("CLIENT RECEIVED SHORT MESSAGE::: " + msg.indexId + " " + msg.type);
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        // NOTE: SENDTOALL ALSO SENDS THE MESSAGE TO THE SERVER AGAIN, WHICH WE DON'T NEED
        if (!isServer) {
            switch (type) {
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
                case CardMessageType.EndGame: {
                        if (!manager.HasReceivedEndGame()) {
                            manager.SetReceivedEndGame(true);
                            manager.AddMessage(new Message(CardMessageType.EndGame));
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

    public void OnServerReceiveShortMessage(NetworkConnectionToClient client, RGNetworkShortMessage msg) {
        Debug.Log("SERVER RECEIVED SHORT MESSAGE::: " + msg.indexId + " " + msg.type);
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        switch (type) {
            case CardMessageType.StartNextPhase:
                // nobody tells server to start a turn, so this shouldn't happen
                Debug.Log("server start next phase message when it shouldn't!");
                break;
            case CardMessageType.EndPhase:
                // end turn is handled here because the player list is kept
                // in this class
                Debug.Log("server received end phase message from sender: " + senderId);
                //   Debug.Log("player turn count : " + playerTurnTakenFlags.Count);
                // note this player's turn has ended      
                int playerIndex = (int)senderId;
                // Debug.Log("player index : " + playerIndex);
                playerTurnTakenFlags[playerIndex] = true;
                // Debug.Log("got here");
                // find next player to ok to play and send them a message
                int nextPlayerId = -1;
                for (int i = 0; i < playerTurnTakenFlags.Count; i++) {
                    if (!playerTurnTakenFlags[i]) {
                        nextPlayerId = playerIDs[i];
                        break;
                    }
                }
                //  Debug.Log("got here 2");
                if (nextPlayerId == -1) {
                    //   Debug.Log("got here 3");
                    GamePhase nextPhase = manager.GetNextPhase();
                    //  Debug.Log("getting next phase : " + nextPhase);

                    // need to increment the turn and set all the players to ready again
                    for (int i = 0; i < playerTurnTakenFlags.Count; i++) {
                        playerTurnTakenFlags[i] = false;
                    }
                    // 

                    // Debug.Log("sending start next phase");
                    // tell all the clients to go to the next phase
                    msg.indexId = (uint)localPlayerID;
                    msg.type = (uint)CardMessageType.StartNextPhase;
                    NetworkServer.SendToAll(msg);
                    //Debug.Log("doing the next phase");
                    // server needs to start next phase as well
                    manager.StartNextPhase();
                    //  Debug.Log("checking to make sure it's not the next turn");
                    if (nextPhase == GamePhase.DrawRed) {
                        manager.IncrementTurn();
                        Debug.Log("Turn is done - incrementing and starting again.");
                    }
                    //   Debug.Log("next phase stuff done");

                }
                break;
            case CardMessageType.IncrementTurn:
                Debug.Log("Server received increment message and did nothing.");
                break;
            case CardMessageType.EndGame: {
                    if (!manager.HasReceivedEndGame()) {
                        manager.SetReceivedEndGame(true);
                        manager.AddMessage(new Message(CardMessageType.EndGame));
                        manager.ShowEndGameCanvas();
                        Debug.Log("received end game message and will now end game on server");
                    }
                }
                break;
            default:
                break;
        }

    }

    public void OnClientReceiveLongMessage(RGNetworkLongMessage msg) {
        Debug.Log("CLIENT RECEIVED LONG MESSAGE::: " + msg.ToString());
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        if (msg.indexId != localPlayerID && !isServer) {
            // we don't send messages to ourself
            switch (type) {
                case CardMessageType.StartGame: {
                        Debug.Log("client received message to start the game");
                        uint count = msg.count;
                        int element = 0;
                        for (int i = 0; i < count; i++) {
                            // the id is the order in the list
                            int existingPlayer = playerIDs.FindIndex(x => x == i);
                            int actualInt = GetIntFromByteArray(element, msg.payload);

                            if (existingPlayer == -1) {
                                playerIDs.Add(i);
                                // then get player type

                                playerTypes.Add((PlayerTeam)actualInt);
                                // get length of player name
                                element += 4;
                                actualInt = GetIntFromByteArray(element, msg.payload);

                                // get player name
                                element += 4;
                                ArraySegment<byte> name = msg.payload.Slice(element, actualInt);
                                playerNames.Add(Encoding.ASCII.GetString(name));

                                element += actualInt;
                                Debug.Log("player being added : " + playerIDs[i] + " " + playerTypes[i] +
                                    " " + playerNames[i]);
                            }
                            else {
                                // when a game is reset we only need the player type again
                                playerTypes[existingPlayer] = (PlayerTeam)actualInt;
                                Debug.Log("player " + playerNames[existingPlayer] + " already exists! new type is: " + playerTypes[existingPlayer]);
                            }



                        }
                        // now start the next phase
                        manager.RealGameStart();
                    }
                    break;
                case CardMessageType.ShareDiscardNumber: {
                        uint count = msg.count;

                        Debug.Log("client received a player's discard amount!" + count);
                        if (count == 1) {
                            // turn the first element into an int
                            int discardCount = BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.indexId;

                            Debug.Log("setting player discard to " + discardCount);

                            // let the game manager display the new info
                            manager.DisplayGameStatus("Player " + playerNames[playerIndex] +
                                " discarded " + discardCount + " cards.");
                        }
                    }
                    break;
                case CardMessageType.DrawCard: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.DrawCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("client received draw card message from opponent containing playerID : " + uniqueId + " and card uid: " + uniqueId + " for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.CardUpdate: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update {
                            Type = CardMessageType.CardUpdate,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            FacilityType = (FacilityType)facilityType,
                        };
                        Debug.Log("client received update message from opponent containing playerID : " + uniqueId + " and card id: " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.ReduceCost: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update {
                            Type = CardMessageType.ReduceCost,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            Amount = amount,
                        };
                        Debug.Log("client received update message from opponent containing playerID : " + uniqueId + " and card id: " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.RemoveEffect: {
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
                        Update update = new Update {
                            Type = CardMessageType.RemoveEffect,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            FacilityType = (FacilityType)facilityType,
                            //EffectTarget = (FacilityEffectType)effect,
                        };
                        Debug.Log("client received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.DiscardCard: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.DiscardCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("client received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.MeepleShare: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int playerToShareWith = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int meepleColor = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.MeepleShare,
                            UniqueID = playerToShareWith,
                            CardID = meepleColor,
                            Amount = amount,
                        };
                        Debug.Log("client received update message from opponent containing player to share with : " + playerToShareWith +
                            " and meeple color " + meepleColor + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public bool CheckReadyToStart() {
        bool readyToStart = true;
        for (int i = 0; i < playerIDs.Count; i++) {
            if (playerTypes[i] == PlayerTeam.Any) {
                readyToStart = false;
                break;
            }
        }
        return readyToStart;
    }
    public void OnServerReceiveLongMessage(NetworkConnectionToClient client, RGNetworkLongMessage msg) {
        Debug.Log("SERVER RECEIVED LONG MESSAGE::: " + msg.indexId + " " + msg.type);
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        if (msg.indexId != localPlayerID) {
            switch (type) {
                case CardMessageType.SharePlayerType: {
                        uint count = msg.count;

                        Debug.Log("server received a player's type!" + count);
                        if (count == 1) {
                            // turn the first element into an int
                            PlayerTeam playerType = (PlayerTeam)BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.indexId;
                            playerTypes[playerIndex] = playerType;
                            playerTurnTakenFlags[playerIndex] = true;
                            Debug.Log("setting player type to " + playerType);

                            // check to see if we've got a player type for everybody!
                            if (CheckReadyToStart()) {
                                Debug.Log("Ready to start!");
                                manager.RealGameStart();
                                // get the turn taking flags ready to go again
                                for (int i = 0; i < playerTurnTakenFlags.Count; i++) {
                                    playerTurnTakenFlags[i] = false;
                                }
                            }
                        }
                    }
                    break;
                case CardMessageType.ShareDiscardNumber: {
                        uint count = msg.count;

                        Debug.Log("server received a player's discard amount!" + count);
                        if (count == 1) {
                            // turn the first element into an int
                            int discardCount = BitConverter.ToInt32(msg.payload);
                            int playerIndex = (int)msg.indexId;

                            Debug.Log("setting player discard to " + discardCount);

                            // share with other players
                            //NetworkServer.SendToAll(msg);

                            // let the game manager display the new info
                            manager.DisplayGameStatus("Player " + playerNames[playerIndex] +
                                " discarded " + discardCount + " cards.");
                        }
                    }
                    break;
                case CardMessageType.DrawCard: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.DrawCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("server received draw card message from opponent containing playerID : " + uniqueId + " and card uid: " + uniqueId + " for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.CardUpdate: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int facilityType = GetIntFromByteArray(element, msg.payload);

                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.CardUpdate,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            FacilityType = (FacilityType)facilityType
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.ReduceCost: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        Update update = new Update {
                            Type = CardMessageType.ReduceCost,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            Amount = amount,
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.RemoveEffect: {
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
                        Update update = new Update {
                            Type = CardMessageType.RemoveEffect,
                            UniqueID = uniqueId,
                            CardID = cardId,
                            FacilityType = (FacilityType)facilityType,
                            //EffectTarget = (FacilityEffectType)effect,
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.DiscardCard: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int uniqueId = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int cardId = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.DiscardCard,
                            UniqueID = uniqueId,
                            CardID = cardId,
                        };
                        Debug.Log("server received update message from opponent containing : " + uniqueId + " and cardid " + cardId + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                case CardMessageType.MeepleShare: {
                        int element = 0;
                        GamePhase gamePhase = (GamePhase)GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int playerToShareWith = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int meepleColor = GetIntFromByteArray(element, msg.payload);
                        element += 4;
                        int amount = GetIntFromByteArray(element, msg.payload);
                        element += 4;

                        Update update = new Update {
                            Type = CardMessageType.MeepleShare,
                            UniqueID = playerToShareWith,
                            CardID = meepleColor,
                            Amount = amount,
                        };
                        Debug.Log("server received update message from opponent containing player to share with : " + playerToShareWith +
                            " and meeple color " + meepleColor + "for game phase " + gamePhase);

                        manager.AddUpdateFromOpponent(update, gamePhase, msg.indexId);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void SetupHandlers() {
        NetworkClient.RegisterHandler<RGNetworkShortMessage>(OnClientReceiveShortMessage);
        NetworkServer.RegisterHandler<RGNetworkShortMessage>(OnServerReceiveShortMessage);
        NetworkClient.RegisterHandler<RGNetworkLongMessage>(OnClientReceiveLongMessage);
        NetworkServer.RegisterHandler<RGNetworkLongMessage>(OnServerReceiveLongMessage);
    }
}
