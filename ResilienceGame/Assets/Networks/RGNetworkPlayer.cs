using Mirror;
using UnityEngine;

public class RGNetworkPlayer : NetworkBehaviour
{
    // Note: any single var that's player specific should be here and sync'd
    [SyncVar] public string mPlayerName;
    [SyncVar] public int mPlayerID;

    public override void OnStartServer()
    {
        mPlayerName = (string)connectionToClient.authenticationData;
        mPlayerID = connectionToClient.connectionId;
        Debug.Log(" network player says id is " + mPlayerID);
    }

    public override void OnStartLocalPlayer()
    {      
        RGNetworkPlayerList.instance.localPlayerID = mPlayerID;
        RGNetworkPlayerList.instance.localPlayerName = mPlayerName;
        Debug.Log(" local player says id is " + mPlayerID);
    }
}
