using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Collections;

public class RGNetworkPlayer : NetworkBehaviour
{

    [SyncVar] public string playerName;
    [SyncVar] public int playerID;

    public MaliciousActor malActor;
    //[SyncVar] public GameObject centralMap;
    //[SyncVar] public GameObject cardDrop;
    //[SyncVar] public GameObject cardHandLoc;
    //[SyncVar] public List<int> rgDeck;
    //[SyncVar] public List<int> rgCardCount;
    public GameObject centralMap;
    public GameObject cardDrop;
    public GameObject cardHandLoc;
    public List<int> rgDeck;
    public List<int> rgCardCount;


    public override void OnStartServer()
    {
        playerName = (string)connectionToClient.authenticationData;
        playerID = connectionToClient.connectionId;
        //this.gameObject.AddComponent<MaliciousActor>();

    }

    public override void OnStartLocalPlayer()
    {
        RGGameExampleUI.localPlayerName = playerName;
        RGGameExampleUI.localPlayerID = playerID;
        
        //this.gameObject.AddComponent<Player>();
        if (RGNetworkPlayerList.instance.playerTeamIDs[playerID] == 0)
        {
            MaliciousActor baseMal = GameObject.FindObjectOfType<MaliciousActor>();
            baseMal.DelayedStart();
            malActor = this.gameObject.AddComponent<MaliciousActor>();
            //malActor = baseMal;
            malActor.Deck = baseMal.Deck;
            rgDeck = baseMal.Deck;
            malActor.Deck = rgDeck;
            rgCardCount = baseMal.CardCountList;
            malActor.CardCountList = baseMal.CardCountList;
            malActor.cardReader = baseMal.cardReader;
            malActor.cardDropZone = baseMal.cardDropZone;
            baseMal.cardDropZone.transform.parent = malActor.transform;
            malActor.handDropZone = baseMal.handDropZone;
            baseMal.handDropZone.transform.parent = malActor.transform;
            malActor.cardPrefab = baseMal.cardPrefab;
            malActor.HandList = baseMal.HandList;
            malActor.ActiveCardList = baseMal.ActiveCardList;
            malActor.activeCardIDs = baseMal.activeCardIDs;
            malActor.manager = baseMal.manager;
            malActor.manager.maliciousActor = malActor;
            malActor.targetFacilities = baseMal.targetFacilities;
            malActor.targetIDList = baseMal.targetIDList;
            malActor.gameExampleUI = baseMal.gameExampleUI;
            baseMal.gameObject.SetActive(false);
            centralMap = GameObject.Find("Central Map");
            this.gameObject.transform.SetParent(centralMap.transform);
            
            Debug.Log(this.gameObject.name);
            Debug.Log(malActor.Deck.Count);
            //malActor.DelayedStart();

        }
        else
        {
            this.gameObject.AddComponent<Player>();

        }
    }

}
