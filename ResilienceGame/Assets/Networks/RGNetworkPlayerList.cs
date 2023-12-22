using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RGNetworkPlayerList : NetworkBehaviour
{
    public static RGNetworkPlayerList instance;

    public SyncList<int> playerIDs = new SyncList<int>();
    public SyncList<int> playerTeamIDs = new SyncList<int>();
    public SyncList<bool> playerReadyFlags = new SyncList<bool>();
    public SyncList<List<int>> playerDecks = new SyncList<List<int>>();
    public SyncList<List<int>> playerCardCounts = new SyncList<List<int>>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPlayer(int id, int teamID)
    {
        //if (!isServer) return;
        playerIDs.Add(id);
        playerTeamIDs.Add(teamID);
        playerReadyFlags.Add(true);
        //playerDecks.Add(playerDeck);
        //playerCardCounts.Add(playerCardCount);
    }

    public void RemovePlayer(int id)
    {
        if (!isServer) return;
        playerIDs.Remove(id);

        int playerIndex = playerIDs.Find(x => x == id);
        playerTeamIDs.RemoveAt(playerIndex);
        playerReadyFlags.RemoveAt(playerIndex);
    }

    public void ChangeReadyFlag(int id, bool flag)
    {
        if (!isServer) return;
        int playerIndex = playerIDs.Find(x => x == id);
        playerReadyFlags[playerIndex] = flag;
    }

    public void CleanReadyFlag()
    {
        if (!isServer) return;

        for(int i = 0; i < playerReadyFlags.Count; i++)
        {
            playerReadyFlags[i] = false;
        }
    }

    public bool IsTeamReady(int teamID)
    {
        for (int i = 0; i < playerTeamIDs.Count; i++)
        {
            // If the player is part of the specified team
            if (playerTeamIDs[i] == teamID)
            {
                // If any player in the team is not ready, return false
                if (playerReadyFlags[i] == false)
                {
                    return false;
                }
            }
        }

        // If all players in the team are ready, return true
        return true;
    }
}
