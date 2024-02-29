using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RGNetworkPlayerList : NetworkBehaviour
{
    public static RGNetworkPlayerList instance;

    public int localPlayerID;
    public List<int> playerIDs = new List<int>();
    public List<int> playerTeamIDs = new List<int>();
    public List<bool> playerReadyFlags = new List<bool>();
    public List<List<int>> playerDecks = new List<List<int>>();
    public List<List<int>> playerCardCounts = new List<List<int>>();

    public bool isUpdated = false;

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

    [Command(requiresAuthority = false)]
    public void CmdUpdateInfo()
    {
        RpcUpdateInfo(playerIDs, playerTeamIDs, playerReadyFlags);
    }


    [ClientRpc]
    public void RpcUpdateInfo(List<int> playerIDs, List<int> playerTeamIDs, List<bool> playerReadyFlags)
    {
        this.playerIDs.Clear();
        this.playerTeamIDs.Clear();
        this.playerReadyFlags.Clear();
        for (int i = 0; i < playerIDs.Count; i++)
        {
            this.playerIDs.Add(playerIDs[i]);
        }
        for (int i = 0; i < playerTeamIDs.Count; i++)
        {
            this.playerTeamIDs.Add(playerTeamIDs[i]);
        }
        for (int i = 0; i < playerReadyFlags.Count; i++)
        {
            this.playerReadyFlags.Add(playerReadyFlags[i]);
        }
        isUpdated = true;
    }

    public void AddPlayer(int id, int teamID)
    {
        if (!isServer) return;
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
