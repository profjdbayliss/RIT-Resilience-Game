using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text;

public class Player : CardPlayer
{
    

    // blue player only
    //public List<GameObject> seletedFacilities;
    //public bool redoCardRead = false;

    // Update is called once per frame
    //void Update()
    //{
    //    if (HandList != null)
    //    {
    //        foreach (GameObject card in HandList)
    //        {
    //            if (card.GetComponent<Card>().state == Card.CardState.CardInPlay)
    //            {
    //                HandList.Remove(card);
    //                ActiveCardList.Add(card);
    //                activeCardIDs.Add(card.GetComponent<Card>().cardID);
    //                card.GetComponent<Card>().duration = cardReader.CardDuration[card.GetComponent<Card>().cardID] + manager.turnCount;
    //                break;
    //            }
    //        }
    //    }
    //    if (ActiveCardList != null)
    //    {
    //        foreach (GameObject card in ActiveCardList)
    //        {
    //            if (manager.turnCount >= card.GetComponent<Card>().duration)
    //            {
    //                ActiveCardList.Remove(card);
    //                activeCardIDs.Remove(card.GetComponent<Card>().cardID);
    //                card.SetActive(false);
    //                break;
    //            }
    //        }
    //    }

    //}

    //public override void DrawCard()
    //{
    //    int rng = UnityEngine.Random.Range(0, Deck.Count);
    //    if (CardCountList.Count <= 0) // Check to ensure the deck is actually built before trying to draw a card
    //    {
    //        return;
    //    }
    //    if (CardCountList[rng] > 0)
    //    {
    //        CardCountList[rng]--;
    //        GameObject tempCardObj = Instantiate(cardPrefab);
    //        Card tempCard = tempCardObj.GetComponent<Card>();
    //        tempCard.cardDropZone = cardDropZone;
    //        tempCard.cardID = Deck[rng];

    //        // blue only but should also be in red?
    //        if (cardReader.CardFronts[Deck[rng]] == null && redoCardRead == false)
    //        {
    //            cardReader.CSVRead();
    //            redoCardRead = true;
    //        }
    //        ///////////////

    //        tempCard.front = cardReader.CardFronts[Deck[rng]];

    //        // blue only
    //        if (cardReader.CardSubType[Deck[rng]] == 0)
    //        {
    //            tempCard.resCardType = Card.ResCardType.Detection;
    //            foreach (DictionaryEntry entry in cardReader.blueCardTargets)
    //            {
    //                if ((int)entry.Key == Deck[rng]) // check to make sure that the key (CardID) is the same as this Card's ID
    //                {
    //                    tempCard.blueCardTargets = (int[])entry.Value; // If so, give us the right values attached (target card IDs)
    //                    break;
    //                }
    //            }

    //        }
    //        else if (cardReader.CardSubType[Deck[rng]] == 2)
    //        {
    //            tempCard.resCardType = Card.ResCardType.Prevention;
    //            foreach (DictionaryEntry entry in cardReader.blueMitMods)
    //            {
    //                if ((int)entry.Key == Deck[rng]) // check to make sure that the key (CardID) is the same as this Card's ID
    //                {
    //                    tempCard.blueTargetMits = (List<int>)entry.Value;
    //                    tempCard.blueCardTargets = new int[tempCard.blueTargetMits.Count - 1];
    //                    tempCard.potentcy = tempCard.blueTargetMits[0];
    //                    for (int i = 1; i < tempCard.blueTargetMits.Count; i++)
    //                    {
    //                        tempCard.blueCardTargets[i - 1] = tempCard.blueTargetMits[i];
    //                    }
    //                    break;
    //                }
    //            }
    //        }
    //        ////////////////
           
    //        RawImage[] tempRaws = tempCardObj.GetComponentsInChildren<RawImage>();
    //        for (int i = 0; i < tempRaws.Length; i++)
    //        {
    //            if (tempRaws[i].name == "Image")
    //            {
    //                tempRaws[i].texture = tempCard.front.img;
    //            }
    //            else if (tempRaws[i].name == "Background")
    //            {
    //                //tempRaws[i].color = new Color(0.8067818f, 0.8568867f, 0.9245283f, 1.0f);
    //            }
    //        }

    //        TextMeshProUGUI[] tempTexts = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
    //        for (int i = 0; i < tempTexts.Length; i++)
    //        {
    //            if (tempTexts[i].name == "Title Text")
    //            {
    //                tempTexts[i].text = Encoding.ASCII.GetString(tempCard.front.title);
    //            }
    //            else if (tempTexts[i].name == "Description Text")
    //            {
    //                tempTexts[i].text = Encoding.ASCII.GetString(tempCard.front.description);
    //            }
    //        }
    //        TextMeshProUGUI[] tempInnerText = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
    //        //TextMeshProUGUI[] tempInnerText = tempCardObj.GetComponent<CardFront>().innerTexts.GetComponentsInChildren<TextMeshProUGUI>();
    //        for (int i = 0; i < tempInnerText.Length; i++)
    //        {
    //            if (tempInnerText[i].name == "Percent Chance Text")
    //            {
    //                tempInnerText[i].text = "Percent Chance: " + cardReader.CardPercentChance[Deck[rng]] + "%"; // Need to fix this 07/25
    //            }
    //            else if (tempInnerText[i].name == "Impact Text")
    //            {
    //                tempInnerText[i].text = Encoding.ASCII.GetString(tempCard.front.impact);
    //            }
    //            else if (tempInnerText[i].name == "Spread Text")
    //            {
    //                tempInnerText[i].text = "Spread Chance: " + cardReader.CardSpreadChance[Deck[rng]] + "%";
    //            }
    //            else if (tempInnerText[i].name == "Cost Text")
    //            {
    //                tempInnerText[i].text = cardReader.CardCost[Deck[rng]].ToString();
    //            }
    //            else if (tempInnerText[i].name == "Target Text")
    //            {
    //                if (cardReader.CardTargetCount[Deck[rng]] == int.MaxValue)
    //                {
    //                    tempInnerText[i].text = "Target: All ";
    //                }
    //                else
    //                {
    //                    tempInnerText[i].text = "Target: " + cardReader.CardTargetCount[Deck[rng]] + " ";
    //                }
    //                switch (cardReader.CardFacilityStateReqs[Deck[rng]])
    //                {
    //                    case 0:
    //                        tempInnerText[i].text += " uninformed, and unaccessed facilities.";
    //                        tempCard.facilityStateRequirements = Card.FacilityStateRequirements.Normal;
    //                        break;

    //                    case 1:
    //                        tempInnerText[i].text += Card.FacilityStateRequirements.Informed + " facilities.";
    //                        tempCard.facilityStateRequirements = Card.FacilityStateRequirements.Informed;
    //                        break;

    //                    case 2:
    //                        tempInnerText[i].text += Card.FacilityStateRequirements.Accessed + " facilities.";
    //                        tempCard.facilityStateRequirements = Card.FacilityStateRequirements.Accessed;
    //                        break;

    //                }

    //            }
    //        }
    //        tempCard.percentSuccess = cardReader.CardPercentChance[Deck[rng]];
    //        tempCard.percentSpread = cardReader.CardSpreadChance[Deck[rng]];
    //        //tempCard.potentcy = cardReader.CardImpact[Deck[rng]];
    //        tempCard.duration = cardReader.CardDuration[Deck[rng]];
    //        tempCard.cost = cardReader.CardCost[Deck[rng]];
    //        tempCard.teamID = cardReader.CardTeam[Deck[rng]];
    //        if (cardReader.CardTargetCount[Deck[rng]] == int.MaxValue)
    //        {
    //            tempCard.targetCount = Facilities.Count;
    //        }
    //        else
    //        {
    //            tempCard.targetCount = cardReader.CardTargetCount[Deck[rng]];
    //        }
    //        tempCardObj.GetComponent<slippy>().map = tempCardObj;
    //        tempCard.state = Card.CardState.CardDrawn;
    //        Vector3 tempPos = tempCardObj.transform.position;
    //        tempCardObj.transform.position = tempPos;
    //        tempCardObj.transform.SetParent(handDropZone.transform, false);
    //        Vector3 tempPos2 = handDropZone.transform.position;
    //        handSize++;
    //        tempCardObj.transform.position = tempPos2;
    //        ////Add target count into impact description of the card
    //        //foreach (var item in tempCardObj.GetComponentsInChildren<TMP_Text>())
    //        //{
    //        //    if (item.gameObject.name.Contains("Impact Text"))
    //        //    {
    //        //        item.text = "Target Count: " + tempCard.targetCount;
    //        //    }
    //        //}
    //        HandList.Add(tempCardObj);
    //    }
    //    else
    //    {
    //        DrawCard();
    //    }
    //}

    public override void PlayCard(int cardID, int[] targetID, int targetCount = 3)
    {
        //List<int> cardsPlayed = new List<int>();
        //if (funds - cardReader.CardCost[cardID] >= 0 && CardCountList[Deck.IndexOf(cardID)] >= 0 && targetID.Length >= 0) // Check the mal actor has enough action points to play the card, there are still enough of this card to play, and that there is actually a target. Also make sure that the player hasn't already played a card against it this turn
        //{
        //    funds -= cardReader.CardCost[cardID];

        //    cardsPlayed.Add(cardID);
        //    for (int i = 0; i < targetID.Length; i++)
        //    {
        //        cardReader.CardTarget[cardID] = targetID[i];
        //        targetIDList.Add(targetID[i]); // Make sure we don't double target something
        //        cardsPlayed.Add(targetID[i]); // Make sure to track the card play to send across
        //        if (cardReader.CardSubType[cardID] == (int)Card.ResCardType.Detection)
        //        {
        //            int[] tempBlueCardTargs = new int[5];
        //            foreach (DictionaryEntry ent in cardReader.blueCardTargets)
        //            {
        //                if ((int)ent.Key == cardID)
        //                {
        //                    tempBlueCardTargs = (int[])ent.Value; // If so, give us the right values attached (target card IDs)
        //                    break;
        //                }

        //            }
        //            for (int j = 0; j < tempBlueCardTargs.Length; j++)
        //            {

        //                int indexToCheck = manager.maliciousActor.activeCardIDs.BinarySearch(tempBlueCardTargs[j]);
        //                Debug.Log(cardID + " : " + tempBlueCardTargs[j] + " : " + indexToCheck);
        //                if (indexToCheck >= 0)
        //                {
        //                    Debug.Log("CARD IND: " + tempBlueCardTargs[j] + "CARD READER TARG: " + cardReader.CardTarget[tempBlueCardTargs[j]]);

        //                    foreach (GameObject facs in Facilities)
        //                    {
        //                        if (cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] == facs.GetComponent<FacilityV3>().facID)
        //                        {
        //                            Debug.Log("Found the culprit: " + cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] + " " + facs.GetComponent<FacilityV3>().facID);
        //                            cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] = -1;
        //                            break;
        //                        }
        //                        else
        //                        {
        //                            Debug.Log(cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] + " Wrong culp: " + cardReader.CardTarget[tempBlueCardTargs[indexToCheck]] + " OR TARG: " + facs.GetComponent<FacilityV3>().facID);
        //                        }
        //                    }
        //                }

        //            }
        //        }
        //        else if (cardReader.CardSubType[cardID] == (int)Card.ResCardType.Mitigation)
        //        {
        //            // Still need to implement
        //            funds -= cardReader.CardCost[cardID];

        //        }
        //        else if (cardReader.CardSubType[cardID] == (int)Card.ResCardType.Prevention)
        //        {
        //            // Still need to implement
        //            funds -= cardReader.CardCost[cardID];


        //        }
        //        //// Check to make sure that the CardID's target type is the same as the targetID's facility type && the state of the facility is at least the same (higher number, worse state, as the attack)
        //        //if (3 >= cardReader.CardFacilityStateReqs[cardID]) //^^ cardReader.card[cardID] == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().type && cardReader.cardReq(informed,accessed, etc.) == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().state
        //        //{
        //        //    // Then store all necessary information to be calculated and transferred over the network
        //        //    cardReader.CardTarget[cardID] = targetID[i];
        //        //    targetIDList.Add(targetID[i]);
        //        //    cardsPlayed.Add(targetID[i]);

        //        //    // Store the information of CardID played and Target Facility ID to be sent over the network
        //        //}
        //        //else
        //        //{
        //        //    Debug.Log("This card can not be played on that facility. Please target a : " + targetID + " type.");// PUT THE TARGET ID Facility type in here.
        //        //}
        //    }
        //    // Reduce the size of the hand
        //    handSize--;

        //    // Pass over CardsPlayed to network


        //}
        //else
        //{
        //    Debug.Log("You do not have enough action points to play this. You have " + funds + " remaining " + cardReader.CardCount[cardID] + " " + CardCountList[Deck.IndexOf(cardID)]);
        //    Debug.Log("ID: " + cardID + " DECK ID " + Deck.IndexOf(cardID) + " CARDREADER COUNT: " + cardReader.CardCount[cardID] + " CARDCOUNTLIST: " + CardCountList[Deck.IndexOf(cardID)]);
        //}
    }

    //public bool SelectFacility(int cardID)
    //{
    //    // Intention is to have it like hearthstone where player plays a targeted card, they then select the target which is passed into playcard
    //    if (cardReader.CardTargetCount[cardID] == int.MaxValue)
    //    {
    //        if (targetIDList.Count > 0)
    //        {
    //            Debug.Log("You can't play this card, because you have already targetted a facility and this card requries all facilities");
    //            return false;
    //        }
    //        else
    //        {
    //            foreach (GameObject obj in Facilities)
    //            {
    //                Debug.Log((int)(obj.GetComponent<FacilityV3>().state) + " VS " + cardReader.CardFacilityStateReqs[cardID]);
    //                if (obj.GetComponent<FacilityV3>().state != FacilityV3.FacilityState.Down && obj.GetComponent<FacilityV3>().type == type)
    //                {
    //                    seletedFacilities.Add(obj);
    //                }
    //            }
    //        }
    //        int[] tempTargets = new int[seletedFacilities.Count];
    //        List<GameObject> removableObj = new List<GameObject>();
    //        for (int i = 0; i < seletedFacilities.Count; i++)
    //        {
    //            tempTargets[i] = seletedFacilities[i].GetComponent<FacilityV3>().facID;
    //        }
    //        Debug.Log("No overlap " + tempTargets.Length);
    //        PlayCard(cardID, tempTargets);
    //        //seletedFacilities.Clear(); // After every successful run, clear the list
    //        return true;
    //    }
    //    else if (seletedFacilities.Count > 0 && seletedFacilities.Count == cardReader.CardTargetCount[cardID]) //  && targetFacilities.Count == cardReader.targetCount[cardID]
    //    {
    //        int[] tempTargets = new int[seletedFacilities.Count];
    //        List<GameObject> removableObj = new List<GameObject>();
    //        bool tempFailed = false;
    //        for (int i = 0; i < seletedFacilities.Count; i++)
    //        {
    //            if (targetIDList.Contains(seletedFacilities[i].GetComponent<FacilityV3>().facID) == false)
    //            {
    //                tempTargets[i] = seletedFacilities[i].GetComponent<FacilityV3>().facID;
    //            }
    //            else
    //            {
    //                Debug.Log("The " + seletedFacilities[i].GetComponent<FacilityV3>().type + " you selected is already being targetted by another card this turn, so please choose another one");
    //                removableObj.Add(seletedFacilities[i]); // Add the object that we already have targetted to the list to be removed
    //                tempFailed = true; // If it failed, we want to save that it failed
    //            }
    //        }
    //        if (tempFailed)
    //        {
    //            seletedFacilities.RemoveAll(x => removableObj.Contains(x));
    //            return false;
    //        }
    //        Debug.Log("No overlap " + tempTargets.Length);
    //        PlayCard(cardID, tempTargets);
    //        //seletedFacilities.Clear(); // After every successful run, clear the list
    //        return true;

    //    }
    //    else
    //    {
    //        if (seletedFacilities.Count > cardReader.CardTargetCount[cardID])
    //        {
    //            Debug.Log("You have selected too many facilities, please deselect " + (seletedFacilities.Count - cardReader.CardTargetCount[cardID]) + " facilities.");
    //            Debug.Log("Deselect a facility by clicking it again.");
    //        }
    //        else if (seletedFacilities.Count < cardReader.CardTargetCount[cardID])
    //        {
    //            Debug.Log("You have not selected enough facilities, please select " + (cardReader.CardTargetCount[cardID] - seletedFacilities.Count) + " facilities.");
    //            Debug.Log("Select a facility by double clicking it.");
    //        }
    //        return false;
    //    }


    //}


    //public void DiscardCard(int cardID)
    //{
    //    // Check to see if the card has expired and if so, then discard it from play and disable the game object.

    //}

}
