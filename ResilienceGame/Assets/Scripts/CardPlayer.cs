using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardPlayer : MonoBehaviour
{

    // Properties needed by the new design
    public int playerTeam;
    public List<Card> cardDeck = new List<Card>();
    public List<int> cardCountList = new List<int>();
    public List<Card> handList = new List<Card>();
    public List<Facility> controlledFacilities = new List<Facility>();
    public List<Meeple> meeples = new List<Meeple>();

    // Establish necessary fields
    public Card.Type playerType = Card.Type.Resilient;
    public float funds = 100.0f;
    public GameManager manager;
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
    public List<GameObject> facilitiesActedUpon;
    public bool redoCardRead = false;

    // blue player only
    public List<GameObject> Facilities;
    public FacilityV3.Type type;

    public void DrawCards(int cardCount)
    {
        for (int i = 0; i < cardCount && cardDeck.Count > 0; i++)
        {
            handList.Add(cardDeck[0]);
            cardDeck.RemoveAt(0);
        }
    }

    public void DrawSpecificCard(string cardTitle)
    {
        var card = cardDeck.FirstOrDefault(c => c.cardTitle == cardTitle);
        if (card != null)
        {
            handList.Add(card);
            cardDeck.Remove(card);
        }
    }

    public void ShuffleCards(int cardCount)
    {
        System.Random rand = new System.Random();
        var selectedCards = handList.OrderBy(x => rand.Next()).Take(cardCount).ToList();
        foreach (var card in selectedCards)
        {
            handList.Remove(card); 
            cardDeck.Insert(rand.Next(cardDeck.Count), card); 
        }
    }

    public void DiscardCards(int cardCount)
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < cardCount && handList.Count > 0; i++)
        {
            var cardIndex = rand.Next(handList.Count);
            handList.RemoveAt(cardIndex); 
        }
    }

    public void PlayCard(Card card)
    {
        if (handList.Contains(card))
        {
            bool isPlayed = card.PlayCard(this);
            if (isPlayed)
            {
                handList.Remove(card);
            }
        }
    }


    public void InitializeCards()
    {
        // NOTE: set funds in scene var
        cardReader = GameObject.FindObjectOfType<CardReader>();
        manager = GameObject.FindObjectOfType<GameManager>();
        int count = 0;
        for (int i = 0; i < cardReader.CardIDs.Length; i++)
        {
            if (cardReader.CardTeam[i] == (int)playerType) 
            {
                Deck.Add(i);
                CardCountList.Add(cardReader.CardCount[i]);
                count++;
            }
        }
        
        if (HandList.Count < maxHandSize)
        {
            for (int i = 0; i < maxHandSize; i++)
            {
                DrawCard(true, 0);
            }
        }
    }

    public void InitializeFacilities ()
    {
        foreach (GameObject fac in manager.allFacilities)
        {
            if (fac.GetComponent<FacilityV3>().type == type)
            {
                Facilities.Add(fac);
            }
        }
    }

    public virtual void DrawCard(bool random, int cardId)
    {
        int rng;
        if (random)
        {
            rng = UnityEngine.Random.Range(0, Deck.Count);
        } else
        {
            rng = cardId;
        }
        
        if (CardCountList.Count <= 0) // Check to ensure the deck is actually built before trying to draw a card
        {
            Debug.Log("no cards drawn.");
            return;
        }
        if (CardCountList[rng] > 0)
        {
            CardCountList[rng]--;
            GameObject tempCardObj = Instantiate(cardPrefab);
            Card tempCard = tempCardObj.GetComponent<Card>();
            tempCard.cardDropZone = cardDropZone;
            tempCard.cardID = Deck[rng];

            // WORK: not sure the below case ever happens
            if (cardReader.CardFronts[Deck[rng]] == null && redoCardRead == false)
            {
                cardReader.CSVRead();
                redoCardRead = true;
            }

            tempCard.front = cardReader.CardFronts[Deck[rng]];

            if (playerType == Card.Type.Resilient) {
                if (cardReader.CardSubType[Deck[rng]] == 0)
                {
                    tempCard.resCardType = Card.ResCardType.Detection;
                    foreach (DictionaryEntry entry in cardReader.blueCardTargets)
                    {
                        if ((int)entry.Key == Deck[rng]) // check to make sure that the key (CardID) is the same as this Card's ID
                        {
                            tempCard.blueCardTargets = (int[])entry.Value; // If so, give us the right values attached (target card IDs)
                            break;
                        }
                    }

                }
                else if (cardReader.CardSubType[Deck[rng]] == 2)
                {
                    tempCard.resCardType = Card.ResCardType.Prevention;
                    foreach (DictionaryEntry entry in cardReader.blueMitMods)
                    {
                        if ((int)entry.Key == Deck[rng]) // check to make sure that the key (CardID) is the same as this Card's ID
                        {
                            tempCard.blueTargetMits = (List<int>)entry.Value;
                            tempCard.blueCardTargets = new int[tempCard.blueTargetMits.Count - 1];
                            tempCard.potentcy = tempCard.blueTargetMits[0];
                            for (int i = 1; i < tempCard.blueTargetMits.Count; i++)
                            {
                                tempCard.blueCardTargets[i - 1] = tempCard.blueTargetMits[i];
                            }
                            break;
                        }
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
                else if (tempRaws[i].name == "Background")
                {
                    //tempRaws[i].color = new Color(0.8067818f, 0.8568867f, 0.9245283f, 1.0f);
                }
            }

            TextMeshProUGUI[] tempTexts = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
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
            TextMeshProUGUI[] tempInnerText = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            //TextMeshProUGUI[] tempInnerText = tempCardObj.GetComponent<CardFront>().innerTexts.GetComponentsInChildren<TextMeshProUGUI>();
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
                else if (tempInnerText[i].name == "Spread Text")
                {
                    tempInnerText[i].text = "Spread Chance: " + cardReader.CardSpreadChance[Deck[rng]] + "%";
                }
                else if (tempInnerText[i].name == "Cost Text")
                {
                    tempInnerText[i].text = cardReader.CardCost[Deck[rng]].ToString();
                }
                else if (tempInnerText[i].name == "Target Text")
                {
                    if (cardReader.CardTargetCount[Deck[rng]] == int.MaxValue)
                    {
                        tempInnerText[i].text = "Target: All ";
                    }
                    else
                    {
                        tempInnerText[i].text = "Target: " + cardReader.CardTargetCount[Deck[rng]] + " ";
                    }
                    switch (cardReader.CardFacilityStateReqs[Deck[rng]])
                    {
                        case 0:
                            tempInnerText[i].text += " uninformed, and unaccessed facilities.";
                            tempCard.facilityStateRequirements = Card.FacilityStateRequirements.Normal;
                            break;

                        case 1:
                            tempInnerText[i].text += Card.FacilityStateRequirements.Informed + " facilities.";
                            tempCard.facilityStateRequirements = Card.FacilityStateRequirements.Informed;
                            break;

                        case 2:
                            tempInnerText[i].text += Card.FacilityStateRequirements.Accessed + " facilities.";
                            tempCard.facilityStateRequirements = Card.FacilityStateRequirements.Accessed;
                            break;

                    }

                }
            }


            if (playerType == Card.Type.Malicious)
            {
                switch (cardReader.CardSubType[Deck[rng]])
                {
                    case 3:
                        tempCard.malCardType = Card.MalCardType.Reconnaissance;
                        break;

                    case 4:
                        tempCard.malCardType = Card.MalCardType.InitialAccess;

                        break;

                    case 5:
                        tempCard.malCardType = Card.MalCardType.Impact;

                        break;

                    case 6:
                        tempCard.malCardType = Card.MalCardType.LateralMovement;
                        break;

                    case 7:
                        tempCard.malCardType = Card.MalCardType.Exfiltration;
                        break;
                }
            }

            tempCard.percentSuccess = cardReader.CardPercentChance[Deck[rng]];
            tempCard.percentSpread = cardReader.CardSpreadChance[Deck[rng]];

            // malicious player only apparently?
            if (playerType == Card.Type.Malicious)
            {
                tempCard.potentcy = cardReader.CardImpact[Deck[rng]];
            }

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

            // WORK: SEEMS SUSPICIOUS THIS IS ONLY IN MALICIOUS PLAYER!
            if (playerType == Card.Type.Malicious)
            {
                tempCardObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                tempCardObj.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            }
            ////Add target count into impact description of the card
            //foreach (var item in tempCardObj.GetComponentsInChildren<TMP_Text>())
            //{
            //    if (item.gameObject.name.Contains("Impact Text"))
            //    {
            //        item.text = "Target Count: " + tempCard.targetCount;
            //    }
            //}
            HandList.Add(tempCardObj);
        }
        else
        {
            // WORK: does this condition ever happen? Is there a card with the id of 0???
            Debug.Log("random number was less than 0");
            DrawCard(true, cardId);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (HandList != null)
        {
            foreach (GameObject card in HandList)
            {
                if (card.GetComponent<Card>().state == Card.CardState.CardInPlay)
                {
                    HandList.Remove(card);
                    ActiveCardList.Add(card);
                    activeCardIDs.Add(card.GetComponent<Card>().cardID);
                    card.GetComponent<Card>().duration = (int)(cardReader.CardDuration[card.GetComponent<Card>().cardID] + manager.turnCount);
                    break;
                }
            }
        }
        if (ActiveCardList != null)
        {
            foreach (GameObject card in ActiveCardList)
            {
                if (manager.turnCount >= card.GetComponent<Card>().duration)
                {
                    ActiveCardList.Remove(card);
                    activeCardIDs.Remove(card.GetComponent<Card>().cardID);
                    card.SetActive(false);
                    break;
                }
            }
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
                if (playerType == Card.Type.Resilient)
                {
                    foreach (GameObject obj in Facilities)
                    {
                        Debug.Log((int)(obj.GetComponent<FacilityV3>().state) + " VS " + cardReader.CardFacilityStateReqs[cardID]);
                        if (obj.GetComponent<FacilityV3>().state != FacilityV3.FacilityState.Down && obj.GetComponent<FacilityV3>().type == type)
                        {
                            facilitiesActedUpon.Add(obj);
                        }
                    }
                }
                else
                {
                    foreach (GameObject obj in manager.allFacilities)
                    {
                        if ((int)(obj.GetComponent<FacilityV3>().state) >= cardReader.CardFacilityStateReqs[cardID])
                        {
                            Debug.Log("Facility acted upon is " + obj.GetComponent<FacilityV3>().state);
                            facilitiesActedUpon.Add(obj);
                        }
                    }
                }
            }
            int[] tempTargets = new int[facilitiesActedUpon.Count];
            List<GameObject> removableObj = new List<GameObject>();
            for (int i = 0; i < facilitiesActedUpon.Count; i++)
            {
                tempTargets[i] = facilitiesActedUpon[i].GetComponent<FacilityV3>().facID;
            }
            Debug.Log("No overlap " + tempTargets.Length);
            PlayCard(cardID, tempTargets);
            //seletedFacilities.Clear(); // After every successful run, clear the list
            return true;
        }
        else if (facilitiesActedUpon.Count > 0 && facilitiesActedUpon.Count == cardReader.CardTargetCount[cardID]) //  && targetFacilities.Count == cardReader.targetCount[cardID]
        {
            int[] tempTargets = new int[facilitiesActedUpon.Count];
            List<GameObject> removableObj = new List<GameObject>();
            bool tempFailed = false;
            for (int i = 0; i < facilitiesActedUpon.Count; i++)
            {
                if (!targetIDList.Contains(facilitiesActedUpon[i].GetComponent<FacilityV3>().facID))
                {
                    tempTargets[i] = facilitiesActedUpon[i].GetComponent<FacilityV3>().facID;
                }
                else
                {
                    Debug.Log("The " + facilitiesActedUpon[i].GetComponent<FacilityV3>().type + " you selected is already being targetted by another card this turn, so please choose another one");
                    removableObj.Add(facilitiesActedUpon[i]); // Add the object that we already have targetted to the list to be removed
                    tempFailed = true; // If it failed, we want to save that it failed
                }
            }
            if (tempFailed)
            {
                facilitiesActedUpon.RemoveAll(x => removableObj.Contains(x));
                return false;
            }
            Debug.Log("No overlap " + tempTargets.Length);
            PlayCard(cardID, tempTargets);
            //seletedFacilities.Clear(); // After every successful run, clear the list
            return true;

        }
        else
        {
            if (facilitiesActedUpon.Count > cardReader.CardTargetCount[cardID])
            {
                Debug.Log("You have selected too many facilities, please deselect " + (facilitiesActedUpon.Count - cardReader.CardTargetCount[cardID]) + " facilities.");
                Debug.Log("Deselect a facility by clicking it again.");
            }
            else if (facilitiesActedUpon.Count < cardReader.CardTargetCount[cardID])
            {
                Debug.Log("You have not selected enough facilities, please select " + (cardReader.CardTargetCount[cardID] - facilitiesActedUpon.Count) + " facilities.");
                Debug.Log("Select a facility by double clicking it.");
            }
            return false;
        }
    }

    public virtual void PlayCard(int cardID, int[] targetID, int targetCount = 3)
    {
        // NOTE : THIS ISN'T ACTUALLY IN THE GAME RIGHT NOW
    }

    public void DiscardCard(int cardID)
    {
        // Check to see if the card has expired and if so, then discard it from play and disable the game object.

    }

    public void Overtime()
    {

    }
}
