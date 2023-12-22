using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Unity.Collections;
using TMPro;

public class RGNetworkPlayer : NetworkBehaviour
{

    [SyncVar] public string playerName;
    [SyncVar] public int playerID;

    public MaliciousActor malActor;
    public Player resActor;
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
        if(playerID < RGNetworkPlayerList.instance.playerTeamIDs.Count)
        {
            if (RGNetworkPlayerList.instance.playerTeamIDs[playerID] == 0)
            {
                Player baseRes = GameObject.FindObjectOfType<Player>();
                baseRes.gameObject.SetActive(false);
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
                GameObject obj = GameObject.Find("RGTitle");
                obj.GetComponent<TextMeshProUGUI>().text = "M " + malActor.Deck.Count;
                Debug.Log(this.gameObject.name);
                Debug.Log(malActor.Deck.Count);
                //malActor.DelayedStart();

            }
            else
            {
                MaliciousActor baseMal = GameObject.FindObjectOfType<MaliciousActor>();
                baseMal.gameObject.SetActive(false);

                Player baseRes = GameObject.FindObjectOfType<Player>();
                baseRes.DelayedStart();
                resActor = this.gameObject.AddComponent<Player>();
                //malActor = baseMal;
                resActor.Deck = baseRes.Deck;
                rgDeck = baseRes.Deck;
                resActor.Deck = rgDeck;
                rgCardCount = baseRes.CardCountList;
                resActor.CardCountList = baseRes.CardCountList;
                resActor.cardReader = baseRes.cardReader;
                resActor.cardDropZone = baseRes.cardDropZone;
                baseRes.cardDropZone.transform.SetParent(resActor.transform);
                resActor.handDropZone = baseRes.handDropZone;
                baseRes.handDropZone.transform.SetParent(resActor.transform);
                resActor.cardPrefab = baseRes.cardPrefab;
                resActor.HandList = baseRes.HandList;
                resActor.ActiveCardList = baseRes.ActiveCardList;
                resActor.activeCardIDs = baseRes.activeCardIDs;
                resActor.gameManager = baseRes.gameManager;
                resActor.gameManager.allPlayers[0] = resActor.gameObject;
                //resActor.gameManager. = resActor;
                resActor.seletedFacilities = baseRes.seletedFacilities;
                resActor.targetIDList = baseRes.targetIDList;
                //resActor.gameExampleUI = baseRes.gameExampleUI;
                baseRes.gameObject.SetActive(false);
                centralMap = GameObject.Find("Central Map");
                this.gameObject.transform.SetParent(centralMap.transform);
                GameObject obj = GameObject.Find("RGTitle");
                obj.GetComponent<TextMeshProUGUI>().text = "R " + resActor.Deck.Count;
                //this.syncDirection = SyncDirection.ClientToServer;
                Debug.Log(this.gameObject.name);
                Debug.Log(resActor.Deck.Count);
            }
        }
        
    }

}
