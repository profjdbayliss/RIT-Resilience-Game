using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

public class RoomSyncManager : NetworkBehaviour
{
    public static RoomSyncManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    [ClientRpc]
    public void RpcUpdateRoomSlots(NetworkRoomPlayer[] updatedRoomSlots)
    {
        if (NetworkRoomManager.singleton is NetworkRoomManager roomManager)
        {
            roomManager.roomSlots = updatedRoomSlots.ToList();
        }
    }
}
