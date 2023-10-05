
using Mirror;

public class RGNetworkPlayer : NetworkBehaviour
{

    [SyncVar] public string playerName;
    [SyncVar] public int playerID;

    public override void OnStartServer()
    {
        playerName = (string)connectionToClient.authenticationData;
        playerID = connectionToClient.connectionId;
    }

    public override void OnStartLocalPlayer()
    {
        RGGameExampleUI.localPlayerName = playerName;
        RGGameExampleUI.localPlayerID = playerID;

    }

}
