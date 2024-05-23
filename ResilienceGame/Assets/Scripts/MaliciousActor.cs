using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Text;



public class MaliciousActor : CardPlayer {
    
    // specific to red
    //public List<GameObject> targetFacilities;


    //// Update is called once per frame
    //void Update()
    //{
    //    foreach (GameObject card in HandList)
    //    {
    //        if (card.GetComponent<Card>().state == Card.CardState.CardInPlay)
    //        {
    //            HandList.Remove(card);
    //            ActiveCardList.Add(card);
    //            card.GetComponent<Card>().duration = cardReader.CardDuration[card.GetComponent<Card>().cardID] + manager.turnCount;
    //            break;
    //        }
    //    }
    //    foreach (GameObject card in ActiveCardList)
    //    {
    //        if (manager.turnCount >= card.GetComponent<Card>().duration)
    //        {
    //            ActiveCardList.Remove(card);
    //            card.SetActive(false);
    //            break;
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
    //        //cardReader.CardCount[Deck[rng]]--;
    //        GameObject tempCardObj = Instantiate(cardPrefab);
    //        Card tempCard = tempCardObj.GetComponent<Card>();
    //        tempCard.cardDropZone = cardDropZone;
    //        tempCard.cardID = Deck[rng];
    //        tempCard.front = cardReader.CardFronts[Deck[rng]];
    //        RawImage[] tempRaws = tempCardObj.GetComponentsInChildren<RawImage>();
    //        for (int i = 0; i < tempRaws.Length; i++)
    //        {
    //            if (tempRaws[i].name == "Image")
    //            {
    //                tempRaws[i].texture = tempCard.front.img;
    //            }
    //            else if (tempRaws[i].name == "Background")
    //            {
    //                //tempRaws[i].color = new Color(1.0f, 0.5801887f, 0.5801887f, 1.0f);
    //                // Change this based off of what type of card it is (either recon, initial access, or impact)
    //                //switch (tempCard.malCardType)
    //                //{
    //                //    // Recon (Lightest)
                        

    //                //    // Initial Access (darker)

    //                //    // Impact (darkest)

    //                //    // What about for colors of things like exfil? Maybe green?
    //                //}
    //            }
    //        }
    //        //tempCardObj.GetComponentInChildren<TextMeshProUGUI>().text = BitConverter.ToString(tempCard.front.title);
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
    //                tempInnerText[i].text = "Pop. Impacted: " + cardReader.CardImpact[Deck[rng]] + "%";
    //            }
    //            else if (tempInnerText[i].name == "Spread Text")
    //            {
    //                tempInnerText[i].text = "Spread Chance: " + cardReader.CardSpreadChance[Deck[rng]] + "%";
    //            }
    //            else if (tempInnerText[i].name == "Cost Text")
    //            {
    //                tempInnerText[i].text = cardReader.CardCost[Deck[rng]].ToString();
    //            }
    //            else if(tempInnerText[i].name == "Target Text")
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
    //                        tempInnerText[i].text +=  " uninformed, and unaccessed facilities.";
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

    //        // malicious only
    //        switch (cardReader.CardSubType[Deck[rng]])
    //        {
    //            case 3:
    //                tempCard.malCardType = Card.MalCardType.Reconnaissance;
    //                break;

    //            case 4:
    //                tempCard.malCardType = Card.MalCardType.InitialAccess;

    //                break;

    //            case 5:
    //                tempCard.malCardType = Card.MalCardType.Impact;

    //                break;

    //            case 6:
    //                tempCard.malCardType = Card.MalCardType.LateralMovement;
    //                break;

    //            case 7:
    //                tempCard.malCardType = Card.MalCardType.Exfiltration;
    //                break;
    //        }
    //        ///////////////////
            
    //        tempCard.percentSuccess = cardReader.CardPercentChance[Deck[rng]];
    //        tempCard.percentSpread = cardReader.CardSpreadChance[Deck[rng]];

    //        // malicious only?
    //        tempCard.potentcy = cardReader.CardImpact[Deck[rng]];
    //        //

    //        tempCard.duration = cardReader.CardDuration[Deck[rng]];
    //        tempCard.cost = cardReader.CardCost[Deck[rng]];
    //        tempCard.teamID = cardReader.CardTeam[Deck[rng]];
    //        if(cardReader.CardTargetCount[Deck[rng]] == int.MaxValue)
    //        {
    //            tempCard.targetCount = manager.allFacilities.Count;

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
        
    //        // only in malicious
    //        handSize++;
    //        tempCardObj.transform.position = tempPos2;
    //        tempCardObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
    //        tempCardObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
    //        //

    //        ////Add target count into impact description of the card
    //        //foreach (var item in tempCardObj.GetComponentsInChildren<TMP_Text>())
    //        //{
    //        //    if (item.gameObject.name.Contains("Impact"))
    //        //    {
    //        //        item.text = "Target Count: " + tempCard.targetCount;
    //        //    }
    //        //}
    //        HandList.Add(tempCardObj);
    //    }
    //    else
    //    {
    //        Debug.Log("not enough");
    //        DrawCard();
    //    }
    //}


    public override void PlayCard(int cardID, int[] targetID, int targetCount = 3)
    {
        //Debug.Log("Card Play Call" + cardReader.CardCount[cardID] + CardCountList[Deck.IndexOf(cardID)]);
        //if (funds - cardReader.CardCost[cardID] >= 0 && CardCountList[Deck.IndexOf(cardID)] >= 0 && targetID.Length >= 0) // Check the mal actor has enough action points to play the card, there are still enough of this card to play, and that there is actually a target. Also make sure that the player hasn't already played a card against it this turn
        //{
        //   funds -= cardReader.CardCost[cardID];
        //    List<int> cardTargets = new List<int>(); // Format: First Index: Card being played, Every other index is the facilities being targetted by this card
        //    cardTargets.Add(cardID);
        //    activeCardIDs.Add(cardID);
        //    Debug.Log(cardID);
        //    for (int i = 0; i < targetID.Length; i++)
        //    {
        //        // Check to make sure that the CardID's target type is the same as the targetID's facility type && the state of the facility is at least the same (higher number, worse state, as the attack)
        //        if ((int)manager.allFacilities[targetID[i]].GetComponent<FacilityV3>().state >= cardReader.CardFacilityStateReqs[cardID]) //^^ cardReader.card[cardID] == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().type && cardReader.cardReq(informed,accessed, etc.) == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().state
        //        //if (((int)manager.allFacilities[targetID].GetComponent<FacilityV3>().state) >= cardReader.CardFacilityStateReqs[cardID]) //^^ cardReader.card[cardID] == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().type && cardReader.cardReq(informed,accessed, etc.) == gameManager.allFacilities[targetID].GetComponent<FacilityV3>().state
        //        {
        //            // Then store all necessary information to be calculated and transferred over the network
        //            cardReader.CardTarget[cardID] = targetID[i]; // Right now, will only store 1 target facility accurately
        //            targetIDList.Add(targetID[i]);
        //            cardTargets.Add(targetID[i]); // Ideally want to pass this list to the network somehow
        //            activeCardIDs.Add(cardID);
        //            //gameExampleUI.PlayCard(cardID); // Potentially only need to call it once
        //            //Card tempCard = new Card();
        //            //float rng = UnityEngine.Random.Range(0.0f, 1.0f);
        //            //// Determine ranges for the percent chance to allow for super success, success, failure, super failure
        //            //if (rng >= (1.0 - cardReader.CardPercentChance[cardID]))
        //            //{
        //            //    // Success
        //            //    // Get the facility based off of the target ID

        //            //    // Apply the impact, activate these things locally and then the results will be transferred through the network at the end of the turn

        //            //    // Apply the duration to be Current turn count + duration


        //            //}
        //            //else
        //            //{
        //            //    Debug.Log("Attack fizzled");
        //            //}


        //            // Regardless of success or not, we remove the card from play.
        //            //cardReader.CardCount[cardID] -= 1; // This doesn't work as we want, as it would potentially reduce card count for other players if networked, if we don't network this it is not an issue.
        //            //Debug.Log(cardReader.CardCount[cardID]);
        //            // Deck.Remove(cardID);

        //            // Store the information of CardID played and Target Facility ID to be sent over the network
        //        }
        //        else
        //        {
        //            Debug.Log(manager.allFacilities[targetID[i]].GetComponent<FacilityV3>().facID);
        //            Debug.Log("Attempted FAC STATE: " + manager.allFacilities[targetID[i]].GetComponent<FacilityV3>().state + " CARD REQ STATE: " + cardReader.CardFacilityStateReqs[cardID]);
        //            Debug.Log("This card can not be played on that facility. Please target a : " + targetID + " type.");// PUT THE TARGET ID Facility type in here.
        //        }
        //    }
        //    // Reduce the size of the hand
        //    handSize--;

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
    //    if(cardReader.CardTargetCount[cardID] == int.MaxValue)
    //    {
    //        if(targetIDList.Count > 0)
    //        {
    //            Debug.Log("You can't play this card, because you have already targetted a facility and this card requries all facilities");
    //            return false;
    //        }
    //        else
    //        {
    //            foreach(GameObject obj in manager.allFacilities)
    //            {
    //                if((int)(obj.GetComponent<FacilityV3>().state) >= cardReader.CardFacilityStateReqs[cardID])
    //                {
    //                    targetFacilities.Add(obj);
    //                }
    //            }
    //        }
    //        int[] tempTargets = new int[targetFacilities.Count];
    //        List<GameObject> removableObj = new List<GameObject>();
    //        for (int i = 0; i < targetFacilities.Count; i++)
    //        {
    //            tempTargets[i] = targetFacilities[i].GetComponent<FacilityV3>().facID;
    //        }
    //        PlayCard(cardID, tempTargets);
    //        //targetFacilities.Clear(); // After every successful run, clear the list
    //        return true;
    //    }
    //    else if(targetFacilities.Count > 0 && targetFacilities.Count == cardReader.CardTargetCount[cardID]) 
    //    {
    //        int[] tempTargets = new int[targetFacilities.Count];
    //        List<GameObject> removableObj = new List<GameObject>();
    //        bool tempFailed = false;
    //        for (int i = 0; i < targetFacilities.Count; i++)
    //        {
    //            if (targetIDList.Contains(targetFacilities[i].GetComponent<FacilityV3>().facID) == false)
    //            {
    //                tempTargets[i] = targetFacilities[i].GetComponent<FacilityV3>().facID;
    //            }
    //            else
    //            {
    //                Debug.Log("The " + targetFacilities[i].GetComponent<FacilityV3>().type + " you selected is already being targetted by another card this turn, so please choose another one");
    //                removableObj.Add(targetFacilities[i]); // Add the object that we already have targetted to the list to be removed
    //                tempFailed = true; // If it failed, we want to save that it failed
    //            }
    //        }
    //        if(tempFailed)
    //        {
    //            targetFacilities.RemoveAll(x => removableObj.Contains(x));
    //            return false;
    //        }
    //        Debug.Log("No overlap " + tempTargets.Length);
    //        PlayCard(cardID, tempTargets);
    //        //targetFacilities.Clear(); // After every successful run, clear the list
    //        return true;

    //    }
    //    else
    //    {
    //        if(targetFacilities.Count > cardReader.CardTargetCount[cardID])
    //        {
    //            Debug.Log("You have selected too many facilities, please deselect " + (targetFacilities.Count - cardReader.CardTargetCount[cardID]) + " facilities.");
    //            Debug.Log("Deselect a facility by clicking it again.");
    //        }
    //        else if(targetFacilities.Count < cardReader.CardTargetCount[cardID])
    //        {
    //            Debug.Log("You have not selected enough facilities, please select " + (cardReader.CardTargetCount[cardID] - targetFacilities.Count) + " more facilities.");
    //            Debug.Log("Select a facility by double clicking it.");
    //        }
    //        return false;
    //    }


    //}
}
