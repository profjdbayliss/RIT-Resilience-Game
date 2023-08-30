
using Mirror;

public class RGNetworkPlayer : NetworkBehaviour
{
    [SyncVar]
    public string playerName;

    public override void OnStartServer()
    {
        playerName = (string)connectionToClient.authenticationData;
    }

    public override void OnStartLocalPlayer()
    {
        RGGameExampleUI.localPlayerName = playerName;
    }
}
