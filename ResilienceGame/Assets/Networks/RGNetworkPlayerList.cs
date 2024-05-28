using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using System.Linq;

// many messages actually have no arguments
public struct RGNetworkShortMessage : NetworkMessage
{
    public uint indexId;
    public uint type;
}

// for messages that need to update one or more arguments
public struct RGNetworkLongMessage : NetworkMessage
{
    public uint indexId;
    public uint type;
    // number of arguments
    public uint count;
    // the parameters for the message
    // -> ArraySegment to avoid unnecessary allocations
    public ArraySegment<byte> payload;
}

public class RGNetworkPlayerList : NetworkBehaviour, IRGObserver
{
    public static RGNetworkPlayerList instance;

    public int localPlayerID;
    public List<int> playerIDs = new List<int>();
    private bool messageHandlersSet = false;
    private GameManager manager;

    public List<int> playerTeamIDs = new List<int>();
    private List<bool> playerNetworkReadyFlags = new List<bool>();
    private List<bool> playerTurnTakenFlags = new List<bool>();
    //public List<List<int>> playerDecks = new List<List<int>>();
    //public List<List<int>> playerCardCounts = new List<List<int>>();

    //public bool isUpdated = false;
    //public int gameState = 0; // 0 = ongoing, 1 = red win, 2 = blue win

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        SetupHandlers();
        manager = FindObjectOfType<GameManager>();
    }

    //[Command(requiresAuthority = false)]
    //public void CmdEndGame(int gameState)
    //{
    //    RpcEndGame(gameState);
    //}

    //[ClientRpc]
    //public void RpcEndGame(int gameState)
    //{
    //    this.gameState = gameState;
    //    GameManager gm = FindObjectOfType<GameManager>();
    //    gm.ShowEndGameCanvas(gameState);
    //}

    //[Command(requiresAuthority = false)]
    //public void CmdUpdateInfo()
    //{
    //    RpcUpdateInfo(playerIDs, playerTeamIDs, playerReadyFlags);
    //}


    //[ClientRpc]
    //public void RpcUpdateInfo(List<int> playerIDs, List<int> playerTeamIDs, List<bool> playerReadyFlags)
    //{
    //    this.playerIDs.Clear();
    //    this.playerTeamIDs.Clear();
    //    this.playerReadyFlags.Clear();
    //    for (int i = 0; i < playerIDs.Count; i++)
    //    {
    //        this.playerIDs.Add(playerIDs[i]);
    //    }
    //    for (int i = 0; i < playerTeamIDs.Count; i++)
    //    {
    //        this.playerTeamIDs.Add(playerTeamIDs[i]);
    //    }
    //    for (int i = 0; i < playerReadyFlags.Count; i++)
    //    {
    //        this.playerReadyFlags.Add(playerReadyFlags[i]);
    //    }
    //    isUpdated = true;
    //}

    //public void AskUpdateFacilities(List<FacilityV3Info> facilities) // Called by the client who wants to update
    //{
    //    CmdUpdateFacilities(facilities);
    //}

    //[Command(requiresAuthority = false)]
    //public void CmdUpdateFacilities(List<FacilityV3Info> facilities) // Server receives the updated info
    //{
    //    RpcUpdateFacilities(facilities);
    //}

    //[ClientRpc]
    //public void RpcUpdateFacilities(List<FacilityV3Info> facilities) // Update the info to clients
    //{
    //    if (FindObjectOfType<GameManager>() != null)
    //    {
    //        GameManager gm = FindObjectOfType<GameManager>();
    //        for(int i = 0; i < gm.allFacilities.Count; i++)
    //        {
    //            gm.allFacilities[i].GetComponent<FacilityV3>().UpdateFacilityData(facilities[i]);
    //        }

    //    }
    //}

    public void AddPlayer(int id, int teamID)
    {
        if (!isServer) return;
        playerIDs.Add(id);
        playerTeamIDs.Add(teamID);
        playerNetworkReadyFlags.Add(true);
        playerTurnTakenFlags.Add(false);
    }

    public void RemovePlayer(int id)
    {
        if (!isServer) return;
        playerIDs.Remove(id);

        int playerIndex = playerIDs.Find(x => x == id);
        playerTeamIDs.RemoveAt(playerIndex);
        playerNetworkReadyFlags.RemoveAt(playerIndex);
        playerTurnTakenFlags.RemoveAt(playerIndex);
    }

    public void UpdateObserver(Message data)
    {

        // send messages here over network to appropriate place
        switch (data.Type)
        {
            case CardMessageType.EndTurn:
                {
                    RGNetworkShortMessage msg = new RGNetworkShortMessage
                    {
                        indexId = (uint)localPlayerID,
                        type = (uint)data.Type
                    };
                    Debug.Log("update observer called end turn! ");
                    if (isServer)
                    {


                        // we've played so we're no longer on the ready list
                        int playerIndex = playerIDs.Find(x => x == localPlayerID);
                        playerTurnTakenFlags[playerIndex] = true;
                        // find next player to ok to play and send them a message
                        int nextPlayerId = -1;
                        for (int i = playerIndex + 1; i < playerTurnTakenFlags.Count; i++)
                        {
                            if (!playerTurnTakenFlags[i])
                            {
                                nextPlayerId = i;
                                break;
                            }
                        }

                        if (nextPlayerId != -1)
                        {
                            // send my turn to the next player
                            NetworkConnectionToClient connection = NetworkServer.connections[nextPlayerId];
                            //connection.Send(msg);
                            msg.type = (uint)CardMessageType.StartTurn;
                            connection.Send(msg);
                            Debug.Log("next client should receive message " + nextPlayerId + " with connection id " + connection.identity.netId);

                        }
                        else
                        {
                            // need to increment the turn and set all the players to ready again
                            for (int i = 0; i < playerTurnTakenFlags.Count; i++)
                            {
                                playerTurnTakenFlags[i] = false;
                            }
                            // set turn to the server
                            manager.StartTurn();
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
              
                Debug.Log("update observer called increment turn! ");
                if (isServer)
                {
                    RGNetworkShortMessage msg = new RGNetworkShortMessage
                    {
                        indexId = (uint)localPlayerID,
                        type = (uint)data.Type
                    };
                    NetworkServer.SendToAll(msg);
                    Debug.Log("sending turn increment to all clients");
                }
                break;
            case CardMessageType.ShowCards:
                {
                    RGNetworkLongMessage msg = new RGNetworkLongMessage
                    {
                        indexId = (uint)localPlayerID,
                        type = (uint)data.Type,
                        count = (uint)data.arguments.Count,
                        payload = data.arguments.SelectMany<int, byte>(BitConverter.GetBytes).ToArray()
                    };
                    Debug.Log("update observer called show all my cards ");
                    if (isServer)
                    {
                        NetworkServer.SendToAll(msg);
                        Debug.Log("SERVER IS SHOWING THEIR HAND");
                    }
                    else
                    {
                        NetworkClient.Send(msg);
                        Debug.Log("CLIENT IS SHOWING THEIR HAND");
                    }
                }
                break;
            case CardMessageType.EndGame:
                //RGNetworkLongMessage msg = new RGNetworkLongMessage
                //{
                //    netId = (uint)localPlayerID,
                //    type = (uint)data.Type,
                //    count = (uint)data.arguments.Count(),
                //    payload = data.arguments.SelectMany<int, byte>(BitConverter.GetBytes).ToArray()
                //};
                //if (isServer)
                //{
                //    NetworkServer.SendToAll(msg);
                //    Debug.Log("SERVER SENT GAME END MESSAGE FIRST");
                //}
                //else
                //{
                //    NetworkClient.Send(msg);
                //    Debug.Log("CLIENT SENT GAME END MESSAGE FIRST");
                //}
                break;
        default:
                break;
    }

}

    public void OnClientReceiveShortMessage(RGNetworkShortMessage msg)
    {
        Debug.Log("CLIENT RECEIVED SHORT MESSAGE::: " + msg.indexId + " " +msg.type );
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        // NOTE: SENDTOALL ALSO SENDS THE MESSAGE TO THE SERVER AGAIN, WHICH WE DON'T NEED
        if (!isServer)
        {
            switch (type)
            {
                case CardMessageType.StartTurn:
                    Debug.Log("received start turn message");
                    manager.StartTurn();
                    break;
                case CardMessageType.EndTurn:
                    // only the server should get and end turn message!
                    Debug.Log("client received end turn message!");
                    break;
                case CardMessageType.IncrementTurn:
                    // only the server should get and end turn message!
                    Debug.Log("client received increment turn message!");
                    manager.IncrementTurn();
                    break;
               
                default:
                    Debug.Log("client received unknown message!");
                    break;
            }
        }
    }

    public void OnServerReceiveShortMessage(NetworkConnectionToClient client, RGNetworkShortMessage msg)
    {
        Debug.Log("SERVER RECEIVED SHORT MESSAGE::: " + msg.indexId + " " + msg.type);
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        switch (type)
        {
            case CardMessageType.StartTurn:
                // nobody tells server to start a turn, so this shouldn't happen
                Debug.Log("server start turn message");
                break;
            case CardMessageType.EndTurn:
                Debug.Log("server received end turn message");
                // note this player's turn has ended      
                int playerIndex = playerIDs.Find(x => x == senderId);
                playerTurnTakenFlags[playerIndex] = true;
                // find next player to ok to play and send them a message
                int nextPlayerId = -1;
                for (int i = playerIndex + 1; i < playerTurnTakenFlags.Count; i++)
                {
                    if (!playerTurnTakenFlags[i])
                    {
                        nextPlayerId = playerIDs[i];
                        break;
                    }
                }

                if (nextPlayerId != -1)
                {
                    // send my turn to the next player
                    NetworkConnectionToClient connection = NetworkServer.connections[nextPlayerId];
                    msg.indexId = (uint)nextPlayerId;
                    msg.type = (uint)CardMessageType.StartTurn;
                    connection.Send(msg);
                    Debug.Log("next client should receive message " + nextPlayerId);

                }
                else
                {
                    // need to increment the turn and set all the players to ready again
                    for (int i = 0; i < playerTurnTakenFlags.Count; i++)
                    {
                        playerTurnTakenFlags[i] = false;
                    }
                    manager.IncrementTurn();
                    manager.StartTurn();
                    Debug.Log("Turn is done - incrementing and starting again.");
                }
                break;
            case CardMessageType.IncrementTurn:
                Debug.Log("Server received increment message and did nothing.");
                break;
            default:
                break;
        }

    }

    public void OnClientReceiveLongMessage(RGNetworkLongMessage msg)
    {
        Debug.Log("CLIENT RECEIVED LONG MESSAGE::: " + msg.indexId + " " + msg.type);
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;

        switch (type)
        {
            case CardMessageType.ShowCards:
                // only the server should get and end turn message!
                uint count = msg.count;
                Debug.Log("client received a list of an opponents cards! " + count);
                
                List<int> cardIds = new List<int>((int)count);
                for (int i = 0; i < count*4; i+=4)
                {
                    byte first = msg.payload.ElementAt(i);
                    byte second = msg.payload.ElementAt(i+1);
                    byte third = msg.payload.ElementAt(i+2);
                    byte fourth = msg.payload.ElementAt(i+3);
                    int actualInt = first | (second << 8) | (third << 16) | (fourth << 24);
                    cardIds.Add(actualInt);
                    Debug.Log(" :: " + actualInt + " :: ");
                }
                // how do I show them visually?????
                break;
            //case CardMessageType.EndGame:
            //    if (msg.count == 1)
            //    {
            //        int whoWins = BitConverter.ToInt32(msg.payload);
            //        manager.EndGame(whoWins, false);
            //        Debug.Log("received end game message and will now end game on client");

            //    }
            //    break;
            default:
                break;
        }
    }

    public void OnServerReceiveLongMessage(NetworkConnectionToClient client, RGNetworkLongMessage msg)
    {
        Debug.Log("SERVER RECEIVED LONG MESSAGE::: " + msg.indexId + " " + msg.type);
        uint senderId = msg.indexId;
        CardMessageType type = (CardMessageType)msg.type;
        
        switch (type)
        {
            case CardMessageType.ShowCards:
                // only the server should get and end turn message!  
                uint count = msg.count;
                Debug.Log("server received a list of an opponents cards!" + count);
                List<int> cardIds = new List<int>((int)count);
                for (int i = 0; i < count * 4; i += 4)
                {
                    byte first = msg.payload.ElementAt(i);
                    byte second = msg.payload.ElementAt(i + 1);
                    byte third = msg.payload.ElementAt(i + 2);
                    byte fourth = msg.payload.ElementAt(i + 3);
                    int actualInt = first | (second << 8) | (third << 16) | (fourth << 24);
                    cardIds.Add(actualInt);
                    Debug.Log(" :: " + actualInt + " :: ");
                }
                // how do I show them visually?????
                break;
            //case CardMessageType.EndGame:
            //    if (msg.count == 1)
            //    {
            //        int whoWins = BitConverter.ToInt32(msg.payload);
            //        manager.EndGame(whoWins, false);
            //        Debug.Log("received end game message and will now end game on client");

            //    }
            //    break;
            default:
                break;
        }
    }

    public void SetupHandlers()
    {
        NetworkClient.RegisterHandler<RGNetworkShortMessage>(OnClientReceiveShortMessage);
        NetworkServer.RegisterHandler<RGNetworkShortMessage>(OnServerReceiveShortMessage);
        NetworkClient.RegisterHandler<RGNetworkLongMessage>(OnClientReceiveLongMessage);
        NetworkServer.RegisterHandler<RGNetworkLongMessage>(OnServerReceiveLongMessage);
    }

    //public void ChangeReadyFlag(int id, bool flag)
    //{
    //    if (!isServer) return;
    //    int playerIndex = playerIDs.Find(x => x == id);
    //    playerReadyFlags[playerIndex] = flag;
    //}

    //public void CleanReadyFlag()
    //{
    //    if (!isServer) return;

    //    for(int i = 0; i < playerReadyFlags.Count; i++)
    //    {
    //        playerReadyFlags[i] = false;
    //    }
    //}

    //public bool IsTeamReady(int teamID)
    //{
    //    for (int i = 0; i < playerTeamIDs.Count; i++)
    //    {
    //        // If the player is part of the specified team
    //        if (playerTeamIDs[i] == teamID)
    //        {
    //            // If any player in the team is not ready, return false
    //            if (playerReadyFlags[i] == false)
    //            {
    //                return false;
    //            }
    //        }
    //    }

    //    // If all players in the team are ready, return true
    //    return true;
    //}
}
