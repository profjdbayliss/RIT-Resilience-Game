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
    public List<GameObject> seletedFacilities;
    public TextMeshProUGUI fundsText;
    public FacilityV3.Type type;
    public GameManager gameManager;
    public CardReader cardReader;
    public List<int> Deck;
    public List<int> CardCountList;
    public List<int> targetIDList;
    public List<GameObject> HandList;
    public List<GameObject> ActiveCardList;
    public List<int> activeCardIDs;
    public int handSize;
    public int maxHandSize = 5;
    public GameObject cardPrefab;
    public GameObject cardDropZone;
    public GameObject handDropZone;
    public bool redoCardRead = false;

    // Start is called before the first frame update
    void Start()
    {
        //maxHandSize = 5;
        //funds = 1000.0f;
        //cardReader = GameObject.FindObjectOfType<CardReader>();
        //for(int i = 0; i < cardReader.CardIDs.Length; i++)
        //{
        //    if (cardReader.CardTeam[i] == (int)(Card.Type.Resilient)) // Uncomment to build the deck
        //    {
        //        Deck.Add(i);
        //        CardCountList.Add(cardReader.CardCount[i]);
        //    }
        //}
        //for(int i = 0; i < maxHandSize; i++)
        //{
        //    DrawCard();
        //}
        //foreach (GameObject fac in gameManager.allFacilities)
        //{
        //    if (fac.GetComponent<FacilityV3>().type == type)
        //    {
        //        Facilities.Add(fac);
        //    }
        //    //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration)
        //    //{
        //    //    Facilities.Add(fac);
        //    //}
        //    //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution)
        //    //{
        //    //    Facilities.Add(fac);

        //    //}
        //    //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Water)
        //    //{
        //    //    Facilities.Add(fac);

        //    //}
        //    //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation)
        //    //{
        //    //    Facilities.Add(fac);

        //    //}
        //    //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
        //    //{
        //    //    Facilities.Add(fac);
        //    //}
        //    //else
        //    //{
        //    //    // Do nothing
        //    //}
        //}
    }

    public void DelayedStart()
    {
        maxHandSize = 5;
        funds = 1000.0f;
        cardReader = GameObject.FindObjectOfType<CardReader>();
        //cardReader.CSVRead();
        for (int i = 0; i < cardReader.CardIDs.Length; i++)
        {
            if (cardReader.CardTeam[i] == (int)(Card.Type.Resilient)) // Uncomment to build the deck
            {
                Deck.Add(i);
                CardCountList.Add(cardReader.CardCount[i]);
            }
        }
        for (int i = 0; i < maxHandSize; i++)
        {
            DrawCard();
        }
        foreach (GameObject fac in gameManager.allFacilities)
        {
            if (fac.GetComponent<FacilityV3>().type == type)
            {
                Facilities.Add(fac);
            }
            //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration)
            //{
            //    Facilities.Add(fac);
            //}
            //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution)
            //{
            //    Facilities.Add(fac);

            //}
            //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Water)
            //{
            //    Facilities.Add(fac);

            //}
            //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation)
            //{
            //    Facilities.Add(fac);

            //}
            //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
            //{
            //    Facilities.Add(fac);
            //}
            //else
            //{
            //    // Do nothing
            //}
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(HandList != null)
        {
            foreach (GameObject card in HandList)
            {
                if (card.GetComponent<Card>().state == Card.CardState.CardInPlay)
                {
                    HandList.Remove(card);
                    ActiveCardList.Add(card);
                    activeCardIDs.Add(card.GetComponent<Card>().cardID);
                    card.GetComponent<Card>().duration = cardReader.CardDuration[card.GetComponent<Card>().cardID] + gameManager.turnCount;
                    break;
                }
            }
        }
        if(ActiveCardList != null)
        {
            foreach (GameObject card in ActiveCardList)
            {
                if (gameManager.turnCount >= card.GetComponent<Card>().duration)
                {
                    ActiveCardList.Remove(card);
                    activeCardIDs.Remove(card.GetComponent<Card>().cardID);
                    card.SetActive(false);
                    break;
                }
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
        if (CardCountList[rng] > 0)
        {
            CardCountList[rng]--;
            GameObject tempCardObj = Instantiate(cardPrefab);
            Card tempCard = tempCardObj.GetComponent<Card>();
            tempCard.cardDropZone = cardDropZone;
            tempCard.cardID = Deck[rng];
            if (cardReader.CardFronts[Deck[rng]] == null && redoCardRead == false)
            {
                cardReader.CSVRead();
                redoCardRead = true;
            }

            tempCard.front = cardReader.CardFronts[Deck[rng]];
            if(cardReader.CardSubType[Deck[rng]] == 0)
            {
                tempCard.resCardType = Card.ResCardType.Detection;
                foreach(DictionaryEntry entry in cardReader.blueCardTargets)
                {
                    if((int)entry.Key == Deck[rng]) // check to make sure that the key (CardID) is the same as this Card's ID
                    {
                        tempCard.blueCardTargets = (int[])entry.Value; // If so, give us the right values attached (target card IDs)
                        break;
                    }
                }

            }
            else if(cardReader.CardSubType[Deck[rng]] == 2)
            {
                tempCard.resCardType = Card.ResCardType.Prevention;
                foreach (DictionaryEntry entry in cardReader.blueMitMods)
                {
                    if ((int)entry.Key == Deck[rng]) // check to make sure that the key (CardID) is the same as this Card's ID
                    {
                        tempCard.blueTargetMits = (List<int>)entry.Value;
                        tempCard.blueCardTargets = new int[tempCard.blueTargetMits.Count-1];
                        tempCard.potentcy = tempCard.blueTargetMits[0];
                        for (int i = 1; i < tempCard.blueTargetMits.Count; i++)
                        {
                            tempCard.blueCardTargets[i-1] = tempCard.blueTargetMits[i];
                        }
                        break;
                    }
                }
            }
            RawImage[] tempRaws = tempCardObj.GetComponentsInChildren<RawImage>();
            for (int i = 0; i < tempRaws.Length; i++)
            {
                if (tempRaws[i].name == "Image")
                {
                    tempRaws[i].texture = tempCard.front.img;
                }
                else if(tempRaws[i].name == "Background")
                {
                    //tempRaws[i].color = new Color(0.8067818f, 0.8568867f, 0.9245283f, 1.0f);
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
                    tempInnerText[i].text = Encoding.ASCII.GetString(tempCard.front.impact);
                }

            }
            tempCard.percentSuccess = cardReader.CardPercentChance[Deck[rng]];
            tempCard.percentSpread = cardReader.CardSpreadChance[Deck[rng]];
            //tempCard.potentcy = cardReader.CardImpact[Deck[rng]];
            tempCard.duration = cardReader.CardDuration[Deck[rng]];
            tempCard.cost = cardReader.CardCost[Deck[rng]];
            tempCard.teamID = cardReader.CardTeam[Deck[rng]];
            if (cardReader.CardTargetCount[Deck[rng]] == int.MaxValue)
            {
                tempCard.targetCount = Facilities.Count;
            }
            else
            {
                tempCard.targetCount = cardReader.CardTargetCount[Deck[rng]];
            }
            tempCardObj.GetComponent<slippy>().map = tempCardObj;
            tempCard.state = Card.CardState.CardDrawn;
            Vector3 tempPos = tempCardObj.transform.position;
            tempCardObj.transform.position = tempPos;
            tempCardObj.transform.SetParent(handDropZone.transform, false);
            Vector3 tempPos2 = handDropZone.transform.position;
            handSize++;
            tempCardObj.transform.position = tempPos2;
            //Add target count into impact description of the card
            foreach (var item in tempCardObj.GetComponentsInChildren<TMP_Text>())
            {
                if (item.gameObject.name.Contains("Impact"))
                {
                    item.text = "Target Count: " + tempCard.targetCount;
                }
            }
            HandList.Add(tempCardObj);
        }
        else
        {
            DrawCard();
        }
    }

    public void PlayCard(int cardID, int[] targetID, int targetCount = 3)
    {
        List<int> cardsPlayed = new List<int>();
        if (funds - cardReader.CardCost[cardID] >= 0 && CardCountList[Deck.IndexOf(cardID)] >= 0 && targetID.Length >= 0) // Check the mal actor has enough action points to play the card, there are still enough of this card to play, and that there is actually a target. Also make sure that the player hasn't already played a card against it this turn
        {
            funds -= cardReader.CardCost[cardID];

            cardsPlayed.Add(cardID);
            for (int i = 0; i < targetID.Length; i++)
            {
                cardReader.CardTarget[cardID] = targetID[i];
                targetIDList.Add(targetID[i]); // Make sure we don't double target something
                cardsPlayed.Add(targetID[i]); // Make sure to track the card play to send across
                if (cardReader.CardSubType[cardID] == (int)Card.ResCardType.Detection)
                {
                    int[] tempBlueCardTargs = new int[5];
                    foreach (DictionaryEntry ent in cardReader.blueCardTargets)
                    {
                        if((int)ent.Key == cardID)
                        {
                            tempBlueCardTargs = (int[])ent.Value; // If so, give us the right values attached (target card IDs)
                            break;
                        }

                    }
                    for (int j = 0; j < tempBlueCardTargs.Length; j++)
                    {

                        int indexToCheck = gameManager.maliciousActor.activeCardIDs.BinarySearch(tempBlueCardTargs[j]);
                        Debug.Log(cardID + " : " + tempBlueCardTargs[j] + " : " +  indexToCheck);
                        if(indexToCheck >= 0)
                        {
                            Debug.Log("CARD IND: " + tempBlueCardTargs[j] + "CARD READER TARG: " + cardReader.CardTarget[tempBlueCardTargs[j]]);

                            foreach (GameObject facs in Facilities)
                            {
                                if (cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] == facs.GetComponent<FacilityV3>().facID)
                                {
                                    Debug.Log("Found the culprit: " + cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] + " " + facs.GetComponent<FacilityV3>().facID);
                                    cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] = -1;
                                    break;
                                }
                                else
                                {
                                    Debug.Log(cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] + " Wrong culp: " + cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] + " OR TARG: " + facs.GetComponent<FacilityV3>().facID);
                                }
                            }
                        }
                        
                    }
                }
                else if (cardReader.CardSubType[cardID] == (int)Card.ResCardType.Mitigation)
                {
                    // Still need to implement
                    funds -= cardReader.CardCost[cardID];

                }
                else if (cardReader.CardSubType[cardID] == (int)Card.ResCardType.Prevention)
                {
                    // Still need to implement
                    funds -= cardReader.CardCost[cardID];


                }
                //// Check to make sure that the CardID's target type is the same as the targetID's facility type && the state of the facility is at least the same (higher number, worse state, as the attack)
                //if (3 >= cardReader.CardFacilityStateReqs[cardID]) //^^ cardReader.card[cardID] == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().type && cardReader.cardReq(informed,accessed, etc.) == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().state
                //{
                //    // Then store all necessary information to be calculated and transferred over the network
                //    cardReader.CardTarget[cardID] = targetID[i];
                //    targetIDList.Add(targetID[i]);
                //    cardsPlayed.Add(targetID[i]);

                //    // Store the information of CardID played and Target Facility ID to be sent over the network
                //}
                //else
                //{
                //    Debug.Log("This card can not be played on that facility. Please target a : " + targetID + " type.");// PUT THE TARGET ID Facility type in here.
                //}
            }
            // Reduce the size of the hand
            handSize--;

            // Pass over CardsPlayed to network


        }
        else
        {
            Debug.Log("You do not have enough action points to play this. You have " + funds + " remaining " + cardReader.CardCount[cardID] + " " + CardCountList[Deck.IndexOf(cardID)]);
            Debug.Log("ID: " + cardID + " DECK ID " + Deck.IndexOf(cardID) + " CARDREADER COUNT: " + cardReader.CardCount[cardID] + " CARDCOUNTLIST: " + CardCountList[Deck.IndexOf(cardID)]);
        }
    }

    public bool SelectFacility(int cardID)
    {
        // Intention is to have it like hearthstone where player plays a targeted card, they then select the target which is passed into playcard
        if (cardReader.CardTargetCount[cardID] == int.MaxValue)
        {
            if (targetIDList.Count > 0)
            {
                Debug.Log("You can't play this card, because you have already targetted a facility and this card requries all facilities");
                return false;
            }
            else
            {
                foreach (GameObject obj in Facilities)
                {
                    Debug.Log((int)(obj.GetComponent<FacilityV3>().state) + " VS " + cardReader.CardFacilityStateReqs[cardID]);
                    if (obj.GetComponent<FacilityV3>().state != FacilityV3.FacilityState.Down && obj.GetComponent<FacilityV3>().type == type)
                    {
                        seletedFacilities.Add(obj);
                    }
                }
            }
            int[] tempTargets = new int[seletedFacilities.Count];
            List<GameObject> removableObj = new List<GameObject>();
            for (int i = 0; i < seletedFacilities.Count; i++)
            {
                tempTargets[i] = seletedFacilities[i].GetComponent<FacilityV3>().facID;
            }
            Debug.Log("No overlap " + tempTargets.Length);
            PlayCard(cardID, tempTargets);
            //seletedFacilities.Clear(); // After every successful run, clear the list
            return true;
        }
        else if (seletedFacilities.Count > 0 && seletedFacilities.Count == cardReader.CardTargetCount[cardID]) //  && targetFacilities.Count == cardReader.targetCount[cardID]
        {
            int[] tempTargets = new int[seletedFacilities.Count];
            List<GameObject> removableObj = new List<GameObject>();
            bool tempFailed = false;
            for (int i = 0; i < seletedFacilities.Count; i++)
            {
                if (targetIDList.Contains(seletedFacilities[i].GetComponent<FacilityV3>().facID) == false)
                {
                    tempTargets[i] = seletedFacilities[i].GetComponent<FacilityV3>().facID;
                }
                else
                {
                    Debug.Log("The " + seletedFacilities[i].GetComponent<FacilityV3>().type + " you selected is already being targetted by another card this turn, so please choose another one");
                    removableObj.Add(seletedFacilities[i]); // Add the object that we already have targetted to the list to be removed
                    tempFailed = true; // If it failed, we want to save that it failed
                }
            }
            if (tempFailed)
            {
                seletedFacilities.RemoveAll(x => removableObj.Contains(x));
                return false;
            }
            Debug.Log("No overlap " + tempTargets.Length);
            PlayCard(cardID, tempTargets);
            //seletedFacilities.Clear(); // After every successful run, clear the list
            return true;

        }
        else
        {
            if (seletedFacilities.Count > cardReader.CardTargetCount[cardID])
            {
                Debug.Log("You have selected too many facilities, please deselect " + (seletedFacilities.Count - cardReader.CardTargetCount[cardID]) + " facilities.");
                Debug.Log("Deselect a facility by clicking it again.");
            }
            else if (seletedFacilities.Count < cardReader.CardTargetCount[cardID])
            {
                Debug.Log("You have not selected enough facilities, please select " + (cardReader.CardTargetCount[cardID] - seletedFacilities.Count) + " facilities.");
                Debug.Log("Select a facility by double clicking it.");
            }
            return false;
        }


    }


    public void DiscardCard(int cardID)
    {
        // Check to see if the card has expired and if so, then discard it from play and disable the game object.

    }


    //public void IncreaseOneFeedback()
    //{
    //    // Need to determine how to select
    //    if (funds - 50.0f > 0.0f)
    //    {
    //        seletedFacility.GetComponent<FacilityV3>().feedback += 1;
    //        funds -= 50.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void IncreaseAllFeedback()
    //{
    //    if (funds - 50.0f > 0.0f)
    //    {
    //        foreach (GameObject obj in Facilities)
    //        {
    //            obj.GetComponent<FacilityV3>().feedback += 1;
    //        }
    //        funds -= 50.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void HireWorkers()
    //{
    //    if (funds - 100.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().workers += 5.0f;
    //        funds -= 100.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostIT()
    //{
    //    if (funds - 50.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().it_level += 5.0f;
    //        funds -= 50.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostOT()
    //{
    //    if (funds - 50.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().ot_level += 5.0f;
    //        funds -= 50.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void ImprovePhysSec()
    //{
    //    if (funds - 70.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().phys_security += 7.0f;
    //        funds -= 70.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void IncreaseFunding()
    //{
    //    if (funds - 150.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().funding += 2.0f;
    //        funds -= 150.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostElectricity()
    //{
    //    if (funds - 50.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().electricity += 5.0f;
    //        funds -= 50.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostWater()
    //{
    //    if (funds - 75.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().water += 7.5f;
    //        funds -= 75.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostFuel()
    //{
    //    if (funds - 75.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().fuel += 7.5f;
    //        funds -= 75.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostCommunications()
    //{
    //    if (funds - 90.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().communications += 9.0f;
    //        funds -= 90.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}

    //public void BoostHealth()
    //{
    //    if (funds - 150.0f > 0.0f)
    //    {
    //        // Do something
    //        seletedFacility.GetComponent<FacilityV3>().health += 15.0f;
    //        funds -= 150.0f;
    //    }
    //    else
    //    {
    //        // Show they are broke
    //    }
    //}
}
