using Mirror;
using UnityEngine;

public class RGNetworkPlayer : NetworkBehaviour
{
    // Note: any single var that's player specific should be here and sync'd
    [SyncVar] public string mPlayerName;
    [SyncVar] public int mPlayerID;
    public CardPlayer cardPlayerInstance;

    // New properties for AI player and team
    [SyncVar] public bool isAI;
    [SyncVar] public PlayerTeam playerTeam;

    public override void OnStartServer()
    {
        mPlayerName = (string)connectionToClient.authenticationData;
        mPlayerID = connectionToClient.connectionId;
        cardPlayerInstance = GetComponent<CardPlayer>();
        Debug.Log(" network player says id is " + mPlayerID);
    }

    public override void OnStartLocalPlayer()
    {      
        RGNetworkPlayerList.instance.localPlayerID = mPlayerID;
        RGNetworkPlayerList.instance.localPlayerName = mPlayerName;
        cardPlayerInstance = GetComponent<CardPlayer>();
        GameManager.Instance.actualPlayer = cardPlayerInstance;
        Debug.Log(" local player says id is " + mPlayerID);
    }

    // Method to initialize AI player
    public void InitializeAIPlayer(string name, PlayerTeam team)
    {
        isAI = true;
        mPlayerName = name;
        playerTeam = team;
        cardPlayerInstance = GetComponent<AICardPlayer>();
        Debug.Log("AI player initialized: " + mPlayerName);
    }

    // Method to perform AI player's turn logic
    public void PerformAITurn()
    {
        if (isAI && cardPlayerInstance is AICardPlayer aiCardPlayer)
        {
            aiCardPlayer.PlayBlueTeamCard();
        }
    }

    // Method to assign a sector to the AI player
    public void AssignSector(Sector sector)
    {
        if (isAI && cardPlayerInstance is AICardPlayer aiCardPlayer)
        {
            aiCardPlayer.PlayerSector = sector;
        }
    }
}
