using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class MaliciousActor : MonoBehaviour
{
    // Establish necessary fields
    public float funds = 750.0f;
    public GameObject targetFacility;
    public GameObject ransomwaredFacility;
    public GameManager manager;
    public float ransomwareTurn;
    public float randomEventChance;
    public CardReader cardReader;
    public List<int> Deck;
    public List<int> CardCountList;
    public List<GameObject> HandList;
    public List<GameObject> ActiveCardList;
    public int handSize;
    public int maxHandSize = 5;
    public GameObject cardPrefab;
    public GameObject cardDropZone;
    public GameObject map;


    // Start is called before the first frame update
    void Start()
    {
        funds = 750.0f;
        cardReader = GameObject.FindObjectOfType<CardReader>();
        manager = GameObject.FindObjectOfType<GameManager>();
        Debug.Log("TEST MAL START");
        for (int i = 0; i < cardReader.CardIDs.Length; i++)
        {
            if (cardReader.CardTeam[i] == (int)(Card.Type.Malicious)) // Uncomment to build the deck
            {
                Debug.Log("CARD ID: " + i + " CARD TEAM: " + cardReader.CardTeam[i]);
                Deck.Add(i);
                CardCountList.Add(cardReader.CardCount[i]);
            }

            //if (cardReader.CardTeam[i] == (int)(Card.Type.Resilient)) // Uncomment to build the deck
            //{
            //    for(int j = 0; j < cardReader.CardCount[i]; j++)
            //    {
            //        Deck.Add(i);
            //    }
            //}

            // Gets facility specific cards which we don't have yet
            //if (cardReader.CardTeam[i] == ((int)type)) // Uncomment to build the deck
            //{
            //    Deck.Add(i);
            //}
        }
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (GameObject card in HandList)
        {
            if (card.GetComponent<Card>().state == Card.CardState.CardInPlay)
            {
                HandList.Remove(card);
                ActiveCardList.Add(card);
                card.GetComponent<Card>().duration = cardReader.CardDuration[card.GetComponent<Card>().cardID] + manager.turnCount;
                break;
            }
        }
        foreach (GameObject card in ActiveCardList)
        {
            if (manager.turnCount >= card.GetComponent<Card>().duration)
            {
                ActiveCardList.Remove(card);
                card.SetActive(false);
                break;
            }
        }
    }

    public void SpawnDeck()
    {
        cardReader = GameObject.FindObjectOfType<CardReader>();
        for (int i = 0; i < cardReader.CardIDs.Length; i++)
        {
            if (cardReader.CardTeam[i] == (int)(Card.Type.Malicious)) // Uncomment to build the deck
            {
                Deck.Add(i);
                CardCountList.Add(cardReader.CardCount[i]);
            }
        }
    }
    public void DrawCard()
    {
        int rng = Random.Range(0, Deck.Count);
        Debug.Log("ID: " + Deck[rng] + " TYPE: " + cardReader.CardTeam[Deck[rng]] + " " + CardCountList[rng]);
        //if (cardReader.CardCount[Deck[rng]] > 0)
        if (CardCountList[rng] > 0)
        {
            CardCountList[rng]--;
            //cardReader.CardCount[Deck[rng]]--;
            GameObject tempCardObj = Instantiate(cardPrefab);
            Card tempCard = tempCardObj.GetComponent<Card>();
            tempCard.cardDropZone = cardDropZone;
            tempCard.cardID = Deck[rng];
            tempCard.front = cardReader.CardFronts[Deck[rng]];
            RawImage[] tempRaws = tempCardObj.GetComponentsInChildren<RawImage>();
            for (int i = 0; i < tempRaws.Length; i++)
            {
                if (tempRaws[i].name == "Image")
                {
                    tempRaws[i].texture = tempCard.front.img;
                }
                else if (tempRaws[i].name == "Background")
                {
                    tempRaws[i].color = new Color(0.8067818f, 0.8568867f, 0.9245283f, 1.0f);
                }
            }
            tempCardObj.GetComponentInChildren<TextMeshProUGUI>().text = tempCard.front.title;
            tempCard.percentSuccess = cardReader.CardPercentChance[Deck[rng]];
            tempCard.percentSpread = cardReader.CardSpreadChance[Deck[rng]];
            tempCard.potentcy = cardReader.CardImpact[Deck[rng]];
            tempCard.duration = cardReader.CardDuration[Deck[rng]];
            tempCard.cost = cardReader.CardCost[Deck[rng]];
            tempCard.teamID = cardReader.CardTeam[Deck[rng]];
            tempCardObj.GetComponent<slippy>().map = tempCardObj;
            tempCard.state = Card.CardState.CardDrawn;
            Vector3 tempPos = tempCardObj.transform.position;
            tempPos.x += handSize * 8;
            tempCardObj.transform.position = tempPos;
            tempCardObj.transform.SetParent(this.transform, false);

            handSize++;
            HandList.Add(tempCardObj);
        }
        else
        {
            DrawCard();
        }
    }


    public void PlayCard(int cardID, int targetID)
    {
        Debug.Log("Card Play Call" + cardReader.CardCount[cardID] + CardCountList[Deck.IndexOf(cardID)]);
        if (funds - cardReader.CardCost[cardID] >= 0 && CardCountList[Deck.IndexOf(cardID)] >= 0 && targetID >= 0) // cardReader.CardCount[cardID] >= 0
        {
            // Check to make sure that the CardID's target type is the same as the targetID's facility type
            if (true) //^^ cardReader.card[cardID] == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().type
            {
                Card tempCard = new Card();
                float rng = Random.Range(0.0f, 1.0f);
                // Determine ranges for the percent chance to allow for super success, success, failure, super failure
                if (rng >= (1.0 - cardReader.CardPercentChance[cardID]))
                {
                    // Success
                    // Get the facility based off of the target ID
                    cardReader.CardTarget[cardID] = targetID;
                    // Apply the impact, activate these things locally and then the results will be transferred through the network at the end of the turn

                    // Apply the duration to be Current turn count + duration


                }
                else
                {
                    Debug.Log("Attack fizzled");
                }
                // Reduce the size of the hand
                handSize--;

                // Regardless of success or not, we remove the card from play.
                //cardReader.CardCount[cardID] -= 1; // This doesn't work as we want, as it would potentially reduce card count for other players if networked, if we don't network this it is not an issue.
                //Debug.Log(cardReader.CardCount[cardID]);
                // Deck.Remove(cardID);

                // Store the information of CardID played and Target Facility ID to be sent over the network
            }
            else
            {
                Debug.Log("This card can not be played on that facility. Please target a : " + targetID + " type.");// PUT THE TARGET ID Facility type in here.
            }

        }
        else
        {
            Debug.Log("You do not have enough action points to play this. You have " + funds + " remaining " + cardReader.CardCount[cardID] + " " + CardCountList[Deck.IndexOf(cardID)]);
            Debug.Log("ID: " + cardID + " DECK ID " + Deck.IndexOf(cardID) + " CARDREADER COUNT: " + cardReader.CardCount[cardID] + " CARDCOUNTLIST: " + CardCountList[Deck.IndexOf(cardID)]);
        }
    }

    public bool SelectFacility(int cardID)
    {
        int targetID = -1;
        // Intention is to have it like hearthstone where player plays a targeted card, they then select the target which is passed into playcard


        //if (seletedFacility == null)
        //{
        //    Debug.Log("PICK A CARD");
        //}
        //else
        //{
        //    targetID = seletedFacility.GetComponent<FacilityV3>().facID;
        //    PlayCard(cardID, targetID);
        //}
        if (targetFacility != null)
        {
            targetID = targetFacility.GetComponent<FacilityV3>().facID;
            PlayCard(cardID, targetID);
            return true;
        }
        else
        {

            Debug.Log("Select a facility by double clicking it");
            return false;
        }


    }


    public void CompromiseWorkers()
    {
        // Lower associated value
        if((targetFacility != null) && (funds - 20.0f > 0.0f))
        {

            targetFacility.GetComponent<FacilityV3>().workers -= 1.0f;
            funds -= 20.0f;
        }
    }

    public void CompromiseIT()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().it_level -= 1.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseOT()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().ot_level -= 1.0f;
            funds -= 20.0f;

        }
    }

    public void CompromisePhysSec()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().phys_security -= 1.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseFunding()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().funding -= 2.0f;
            funds -= 20.0f;

        }
    }

    public void ComprpomiseElectricity()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().electricity -= 10.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseWater()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().water -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseFuel()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().fuel -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseCommunications()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().communications -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseHealth()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().health -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void DataBreach()
    {
        // Lower associated value
        if (targetFacility != null)
        {

        }
    }

    public void GasLineEvent()
    {
        // Attack this facility heavily in gas, but affect nearby facilities fuel levels as well
        if (targetFacility != null)
        {
            if(funds - 100.0f >= 0.0f)
            {
                targetFacility.GetComponent<FacilityV3>().fuel -= 15.0f;
                foreach (FacilityV3 fac in targetFacility.GetComponent<FacilityV3>().connectedFacilities)
                {
                    fac.fuel -= 5.0f;
                }
                funds -= 100.0f;
            }

        }
    }

    public void ElectricityFlowEvent()
    {
        // Potentially attack this facility heavily, but affect nearby facilities as well
        if (targetFacility != null)
        {
            if (funds - 100.0f >= 0.0f)
            {
                targetFacility.GetComponent<FacilityV3>().electricity -= 15.0f;
                foreach (FacilityV3 fac in targetFacility.GetComponent<FacilityV3>().connectedFacilities)
                {
                    fac.electricity -= 5.0f;
                }
                funds -= 100.0f;
            }
        }
    }

    public void RansomwareEvent()
    {
        // Could be a percent chance of happening based off of the preparedness of a facility??

        // Save the current turn count to the target facility

        // if they do not solve it by X turns (I am imagening 2, maybe 3?), deal X amount of damage

        // can be solved by .... (paying, cracking the ransomware, etc.)
        if(targetFacility != null)
        {
            if(funds -100.0f >= 0.0f)
            {
                ransomwaredFacility = targetFacility;
                ransomwareTurn = manager.GetComponent<GameManager>().turnCount + 2.0f;
                funds -= 100.0f;
                if ((ransomwaredFacility != null) && (manager.GetComponent<GameManager>().turnCount >= ransomwareTurn))
                {
                    ransomwaredFacility.GetComponent<FacilityV3>().output_flow /= 2.0f;
                    ransomwareTurn = float.MaxValue;
                    ransomwaredFacility = null;
                }
            }

        }
    }
}
