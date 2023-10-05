using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RGNetworkPlayerList : NetworkBehaviour
{
    public static RGNetworkPlayerList instance;

    public SyncList<int> playerIDs = new SyncList<int>();

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

    public void AddPlayer(int id)
    {
        if (!isServer) return;
        playerIDs.Add(id);
    }

    public void RemovePlayer(int id)
    {
        if (!isServer) return;
        playerIDs.Remove(id);
    }
}
