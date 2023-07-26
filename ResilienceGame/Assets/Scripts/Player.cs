using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text;

public class Player : MonoBehaviour
{
    // Establish necessary fields
    public float funds = 1000.0f;
    public List<GameObject> Facilities;
    public GameObject seletedFacility;
    public TextMeshProUGUI fundsText;
    public FacilityV3.Type type;
    public GameManager gameManager;
    public CardReader cardReader;
    public List<int> Deck;
    public List<int> CardCountList;
    public List<GameObject> HandList;
    public List<GameObject> ActiveCardList;
    public int handSize;
    public int maxHandSize = 5;
    public GameObject cardPrefab;
    public GameObject cardDropZone;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("TEST P START: " + ((int)type));
        maxHandSize = 5;
        funds = 1000.0f;
        cardReader = GameObject.FindObjectOfType<CardReader>();
        Debug.Log("INT PARSE: " + (int)(Card.Type.Resilient));
        for(int i = 0; i < cardReader.CardIDs.Length; i++)
        {
            if (cardReader.CardTeam[i] == (int)(Card.Type.Resilient)) // Uncomment to build the deck
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
        for(int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
        foreach (GameObject fac in gameManager.allFacilities)
        {
            if (fac.GetComponent<FacilityV3>().type == type)
            {
                Facilities.Add(fac);
            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration)
            {
                Facilities.Add(fac);
            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution)
            {
                Facilities.Add(fac);

            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Water)
            {
                Facilities.Add(fac);

            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation)
            {
                Facilities.Add(fac);

            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
            {
                Facilities.Add(fac);
            }
            else
            {
                // Do nothing
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach(GameObject card in HandList)
        {
            if(card.GetComponent<Card>().state == Card.CardState.CardInPlay)
            {
                HandList.Remove(card);
                ActiveCardList.Add(card);
                card.GetComponent<Card>().duration = cardReader.CardDuration[card.GetComponent<Card>().cardID] + gameManager.turnCount;
                break;
            }
        }
        foreach (GameObject card in ActiveCardList)
        {
            if (gameManager.turnCount  >= card.GetComponent<Card>().duration)
            {
                ActiveCardList.Remove(card);
                card.SetActive(false);
                break;
            }
        }
    }

    public void DrawCard()
    {
        int rng = UnityEngine.Random.Range(0, Deck.Count);
        if(CardCountList.Count <= 0) // Check to ensure the deck is actually built before trying to draw a card
        {
            return;
        }
        //Debug.Log("ID: " + Deck[rng] + " TYPE: " + cardReader.CardTeam[Deck[rng]] + " " + CardCountList[rng]);
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
                else if(tempRaws[i].name == "Background")
                {
                    tempRaws[i].color = new Color(0.8067818f, 0.8568867f, 0.9245283f, 1.0f);
                }
            }
            TextMeshProUGUI[] tempTexts = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>();
            for (int i = 0; i < tempTexts.Length; i++)
            {
                if (tempTexts[i].name == "Title Text")
                {
                    tempTexts[i].text = Encoding.ASCII.GetString(tempCard.front.title);
                }
                else if (tempTexts[i].name == "Description Text")
                {
                    tempTexts[i].text = Encoding.ASCII.GetString(tempCard.front.description);
                }
            }
            TextMeshProUGUI[] tempInnerText = tempCardObj.GetComponent<CardFront>().innerTexts.GetComponentsInChildren<TextMeshProUGUI>();
            for (int i = 0; i < tempInnerText.Length; i++)
            {
                if (tempInnerText[i].name == "Percent Chance Text")
                {
                    tempInnerText[i].text = "Percent Chance: " + cardReader.CardPercentChance[Deck[rng]] + "%"; // Need to fix this 07/25
                }
                else if (tempInnerText[i].name == "Impact Text")
                {
                    tempInnerText[i].text = "Pop. Impacted: " + cardReader.CardImpact[Deck[rng]] + "%";
                }
                else if (tempInnerText[i].name == "Spread Text")
                {
                    tempInnerText[i].text = "Spread Chance: " + cardReader.CardSpreadChance[Deck[rng]] + "%";
                }
            }
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
                float rng = UnityEngine.Random.Range(0.0f, 1.0f);
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
        if(seletedFacility != null)
        {
            targetID = seletedFacility.GetComponent<FacilityV3>().facID;
            PlayCard(cardID, targetID);
            return true;
        }
        else
        {

            Debug.Log("Select a facility by double clicking it");
            return false;
        }


    }


    public void DiscardCard(int cardID)
    {
        // Check to see if the card has expired and if so, then discard it from play and disable the game object.

    }

    public void IncreaseOneFeedback()
    {
        // Need to determine how to select
        if (funds - 50.0f > 0.0f)
        {
            seletedFacility.GetComponent<FacilityV3>().feedback += 1;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void IncreaseAllFeedback()
    {
        if (funds - 50.0f > 0.0f)
        {
            foreach (GameObject obj in Facilities)
            {
                obj.GetComponent<FacilityV3>().feedback += 1;
            }
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void HireWorkers()
    {
        if (funds - 100.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().workers += 5.0f;
            funds -= 100.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostIT()
    {
        if (funds - 50.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().it_level += 5.0f;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostOT()
    {
        if (funds - 50.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().ot_level += 5.0f;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void ImprovePhysSec()
    {
        if (funds - 70.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().phys_security += 7.0f;
            funds -= 70.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void IncreaseFunding()
    {
        if (funds - 150.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().funding += 2.0f;
            funds -= 150.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostElectricity()
    {
        if (funds - 50.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().electricity += 5.0f;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostWater()
    {
        if (funds - 75.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().water += 7.5f;
            funds -= 75.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostFuel()
    {
        if (funds - 75.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().fuel += 7.5f;
            funds -= 75.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostCommunications()
    {
        if (funds - 90.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().communications += 9.0f;
            funds -= 90.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostHealth()
    {
        if (funds - 150.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().health += 15.0f;
            funds -= 150.0f;
        }
        else
        {
            // Show they are broke
        }
    }
}
