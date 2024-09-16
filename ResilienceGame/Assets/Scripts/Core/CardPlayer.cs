using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using static UnityEngine.PlayerLoop.PreUpdate;
using Image = UnityEngine.UI.Image;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Linq;

// Enum to track player type
public enum PlayerTeam {
    Red,
    Blue,
    White,
    Any
};

public enum PlayerSector {
    // TODO: Randomly assign to blue players at the start of the game, starting with the core sectors
    Communications, //Core
    Energy, //Core
    Water, //Core
    Information, //Core
    Chemical,
    Commercial,
    Manufacturing,
    Dams,
    Defense,
    Emergency,
    Financial,
    Agriculture,
    Government,
    Healthcare,
    Nuclear,
    Transport,
    Any,
    All
};

public enum AddOrRem {
    Add,
    Remove
};

public struct Updates {
    public AddOrRem WhatToDo;
    public int UniqueFacilityID;
    public int CardID;
};

public enum DiscardFromWhere {
    // TODO: Needs ref to others se
    Hand,
    MyPlayZone,
    MyFacility
};

public class CardPlayer : MonoBehaviour {
    // Establish necessary fields
    public GameManager manager;
    public PlayerTeam playerTeam = PlayerTeam.Any;
    public Sector playerSector;
    public static Dictionary<int, Card> cards = new Dictionary<int, Card>();
    public List<int> FacilityIDs = new List<int>(10);
    public List<int> DeckIDs = new List<int>(52);
    public Dictionary<int, GameObject> HandCards = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> Discards = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> ActiveCards = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> ActiveFacilities = new Dictionary<int, GameObject>();
    public int handSize;
    public int maxHandSize = 4;
    public GameObject cardPrefab;
    public GameObject discardDropZone;
    public GameObject handDropZone;
    private HandPositioner handPositioner;
    public GameObject opponentDropZone;
    public GameObject playerDropZone;
    public GameObject cardStackingCanvas;
    public readonly float ORIGINAL_SCALE = 0.2f;
    public string DeckName = "";
    public bool IsDraggingCard { get; private set; } = false;
    public GameObject hoveredDropLocation;
    private GameObject previousHoveredFacility;
    public Dictionary<string, GameObject> cardDropLocations = new Dictionary<string, GameObject>();

    int facilityCount = 0;
    //Meeples
    // TODO: Move to Sector.cs if needed
    public int blueMeepleCount = 2, blackMeepleCount = 2, purpleMeepleCount = 2;
    int mTotalMeepleValue = 0;
    int mMeeplesSpent = 0;

    Vector2 discardDropMin;
    Vector2 discardDropMax;
    Vector2 playedDropMin;
    Vector2 playedDropMax;
    //Vector2 opponentDropMin;
    //Vector2 opponentDropMax;
    // the var is static to make sure the id's don't overlap between
    // multiple card players
    static int sUniqueIDCount = 0;
    int mFinalScore = 0;
    List<Updates> mUpdatesThisPhase = new List<Updates>(6);



    //public GameObject hoveredDropLocation;


    public void Start() {

        if (handDropZone)
            handPositioner = handDropZone.GetComponent<HandPositioner>();
        else {
            Debug.LogError("Hand drop zone not found");
        }

        InitDropLocations();
        // discard rectangle information for AABB collisions
        RectTransform discardRectTransform = discardDropZone.GetComponent<RectTransform>();
        discardDropMin.x = discardRectTransform.position.x - (discardRectTransform.rect.width / 2);
        discardDropMin.y = discardRectTransform.position.y - (discardRectTransform.rect.height / 2);
        discardDropMax.x = discardRectTransform.position.x + (discardRectTransform.rect.width / 2);
        discardDropMax.y = discardRectTransform.position.y + (discardRectTransform.rect.height / 2);

        // played area rectangle information for AABB collisions
        RectTransform playedRectTransform = playerDropZone.GetComponent<RectTransform>();
        playedDropMin.x = playedRectTransform.position.x - (playedRectTransform.rect.width / 2);
        playedDropMin.y = playedRectTransform.position.y - (playedRectTransform.rect.height / 2);
        playedDropMax.x = playedRectTransform.position.x + (playedRectTransform.rect.width / 2);
        playedDropMax.y = playedRectTransform.position.y + (playedRectTransform.rect.height / 2);

        //// playing on opponent area rectangle information
        //RectTransform opponentRectTransform = opponentDropZone.GetComponent<RectTransform>();
        //opponentDropMin.x = opponentRectTransform.position.x - (opponentRectTransform.rect.width / 2);
        //opponentDropMin.y = opponentRectTransform.position.y - (opponentRectTransform.rect.height / 2);
        //opponentDropMax.x = opponentRectTransform.position.x + (opponentRectTransform.rect.width / 2);
        //opponentDropMax.y = opponentRectTransform.position.y + (opponentRectTransform.rect.height / 2);

    }

    public static void AddCards(List<Card> cardList) {
        foreach (Card card in cardList) {
            cards.Add(card.data.cardID, card);
        }
    }

    public void InitializeCards() {
        DeckIDs.Clear();
        manager = GameObject.FindObjectOfType<GameManager>();
        Debug.Log("card count is: " + cards.Count);
        foreach (Card card in cards.Values) {
            if (card != null && card.DeckName.Equals(DeckName)) {
                Debug.Log("adding card " + card.name + " with id " + card.data.cardID + " to deck " + DeckName);
                for (int j = 0; j < card.data.numberInDeck; j++) {
                    DeckIDs.Add(card.data.cardID);
                }

            }
        }

    }

    public virtual void DrawCards() {
        if (HandCards.Count < maxHandSize) // TODO: Liar???????
        {
            int count = HandCards.Count;
            for (int i = 0; i < maxHandSize - count; i++) {
                if (DeckIDs.Count > 0) {
                    DrawCard(true, 0, -1, ref DeckIDs, handDropZone, true, ref HandCards);

                }
                else {
                    break;
                }
            }
        }
    }

    //These are for testing purposes to add/remove cards from the hand
    public virtual void ForceDrawCard() {
        if (DeckIDs.Count > 0) {
            DrawCard(true, 0, -1, ref DeckIDs, handDropZone, true, ref HandCards);
        }
    }
    public virtual void ForceDiscardRandomCard() {
        var num = UnityEngine.Random.Range(0, HandCards.Count);
        var card = HandCards[num];
        HandCards.Remove(num);
        Discards.Add(num, card);
        card.GetComponent<Card>().state = CardState.CardNeedsToBeDiscarded;
        card.transform.SetParent(discardDropZone.transform, false);
        card.transform.localPosition = new Vector3();
    }

    public virtual Card DrawCard(bool random, int cardId, int uniqueId, ref List<int> deckToDrawFrom,
        GameObject dropZone, bool allowSlippy,
        ref Dictionary<int, GameObject> activeDeck) {
        int rng = -1;
        Card actualCard;
        int indexForCard = -1;

        if (random) {
            rng = UnityEngine.Random.Range(0, deckToDrawFrom.Count);
            if (cards.TryGetValue(deckToDrawFrom[rng], out actualCard)) {
                Debug.Log("found proper card!");
            }
            indexForCard = rng;
        }
        else {
            if (!cards.TryGetValue(cardId, out actualCard)) {
                Debug.Log("Error: handed the card deck a card id that isn't in the deck! " + cardId);
                rng = 0;
                return null;

            }
            indexForCard = deckToDrawFrom.FindIndex(x => x == cardId);
            if (indexForCard == -1) {
                Debug.Log("didn't find a card of this type to draw : " + cardId + " to card deck with number " + deckToDrawFrom.Count);
                return null;
            }
        }

        if (deckToDrawFrom.Count <= 0) // Check to ensure the deck is actually built before trying to draw a card
        {
            Debug.Log("no cards drawn.");
            return null;
        }

        GameObject tempCardObj = Instantiate(cardPrefab);
        Card tempCard = tempCardObj.GetComponent<Card>();
        tempCard.cardZone = dropZone;
        tempCard.data = actualCard.data;
        tempCard.ActionList = new List<ICardAction>(actualCard.ActionList); // Copy action list
        tempCard.target = actualCard.target; // Copy the target type
        if (uniqueId != -1) {
            tempCard.UniqueID = uniqueId;
            Debug.Log("setting unique id for facility " + uniqueId);
        }
        else {
            // since there are multiples of each card type potentially
            // in a deck they need a unique id outside of the card's id
            tempCard.UniqueID = sUniqueIDCount;
            sUniqueIDCount++;
        }

        // set the info on the card front
        CardFront front = actualCard.GetComponent<CardFront>();
        tempCard.front = front;

        RawImage[] tempRaws = tempCardObj.GetComponentsInChildren<RawImage>();
        for (int i = 0; i < tempRaws.Length; i++) {
            Debug.Log(tempRaws[i]);
            if (tempRaws[i].name == "Image") {
                tempRaws[i].texture = tempCard.front.img;
            }
            else if (tempRaws[i].name == "Background") {
                tempRaws[i].color = tempCard.front.color;
                Debug.Log(tempCard.front.color);
            }
        }

        Image[] tempImage = tempCardObj.GetComponentsInChildren<Image>();
        for (int i = 0; i < tempImage.Length; i++) {
            if (tempImage[i].name.Equals("BlackCardSlot")) {
                tempImage[i].enabled = tempCard.front.blackCircle;
            }
            else if (tempImage[i].name.Equals("BlueCardSlot")) {
                tempImage[i].enabled = tempCard.front.blueCircle;
            }
            else if (tempImage[i].name.Equals("PurpleCardSlot")) {
                tempImage[i].enabled = tempCard.front.purpleCircle;
            }
        }

        TextMeshProUGUI[] tempTexts = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < tempTexts.Length; i++) {
            if (tempTexts[i].name.Equals("Title Text")) {
                tempTexts[i].text = tempCard.front.title;
            }
            else if (tempTexts[i].name.Equals("Description Text")) {
                tempTexts[i].text = tempCard.front.description;
            }
            else if (tempTexts[i].name.Equals("Flavor Text")) {
                tempTexts[i].text = tempCard.front.flavor;
            }
            else if (tempTexts[i].name.Equals("BlackCardNumber")) {
                if (tempCard.front.blackCircle) {
                    // set the text number for cost
                    tempTexts[i].enabled = true;
                    tempTexts[i].text = tempCard.data.blackCost + "";
                }
                else {
                    // turn off the text box
                    tempTexts[i].enabled = false;
                }
            }
            else if (tempTexts[i].name.Equals("BlueCardNumber")) {
                if (tempCard.front.blueCircle) {
                    tempTexts[i].enabled = true;
                    tempTexts[i].text = tempCard.data.blueCost + "";
                }
                else { tempTexts[i].enabled = false; }
            }
            else if (tempTexts[i].name.Equals("PurpleCardNumber")) {
                if (tempCard.front.purpleCircle) {
                    tempTexts[i].enabled = true;
                    tempTexts[i].text = tempCard.data.purpleCost + "";
                }
                else { tempTexts[i].enabled = false; }
            }
        }
        /*
        foreach(string mitigation in cards[tempCard.data.cardID].MitigatesWhatCards)
        {
            tempCard.MitigatesWhatCards.Add(mitigation);
        }*/


        tempCardObj.GetComponent<slippy>().DraggableObject = tempCardObj;
        if (!allowSlippy) {
            slippy tempSlippy = tempCardObj.GetComponent<slippy>();
            tempSlippy.enabled = false;
        }
        tempCard.state = CardState.CardDrawn;
        Vector3 tempPos = tempCardObj.transform.position;
        tempCardObj.transform.position = tempPos;
        tempCardObj.transform.SetParent(dropZone.transform, false);
        Vector3 tempPos2 = dropZone.transform.position;
        handSize++;
        tempCardObj.transform.position = tempPos2;
        tempCardObj.SetActive(true);


        if (!activeDeck.TryAdd(tempCard.UniqueID, tempCardObj)) {
            Debug.Log("number of cards in draw active deck are: " + activeDeck.Count);
            foreach (GameObject gameObject in activeDeck.Values) {
                Card card = gameObject.GetComponent<Card>();
                Debug.Log("active deck value: " + card.UniqueID);
            }
        }


        // remove this card so we don't draw it again
        deckToDrawFrom.RemoveAt(indexForCard);
        return tempCard;
    }

    #region Update Functions
    // Update is called once per frame
    void Update() {
        IsDraggingCard = handPositioner.IsDraggingCard;

        if (IsDraggingCard) {
            UpdateHoveredDropLocation();
        }
    }
    //updates the hoverDropLocation class field to hold the object the card is hovering over
    void UpdateHoveredDropLocation() {
        GameObject currentHoveredFacility = null;
        bool isOverAnyDropLocation = false;

        //check all drop locations to see if the mouse is over any of them
        foreach (KeyValuePair<string, GameObject> kvp in cardDropLocations) {

            if (kvp.Value.TryGetComponent(out Collider2D collider)) {                       //grab colliders
                                                                                            // Debug.Log("Checking for overlap with " + kvp.Value.name + " at " + Mouse.current.position.ReadValue());
                if (collider.OverlapPoint(Mouse.current.position.ReadValue())) {            //see if the mouse is inside the collider
                    isOverAnyDropLocation = true;
                    GameObject hoveredObject = kvp.Value;
                    // Debug.Log("Hovered over " + hoveredObject.name);

                    //check if the card being dragged is a facility card
                    var cardDraggedTarget = handPositioner.CardsBeingDragged.First().target;
                    if (cardDraggedTarget == CardTarget.Facility || cardDraggedTarget == CardTarget.Effect) {

                        // Handle fade in if we've moved over a facility
                        if (kvp.Key.Contains("FacilityDropLocation")) {
                            if (GameManager.instance.CanStationsBeHighlighted()) {
                                currentHoveredFacility = kvp.Value;
                                if (currentHoveredFacility != previousHoveredFacility) {
                                    if (currentHoveredFacility.TryGetComponent(out HoverActivateObject hoverActivateObject)) {
                                        //Debug.Log("Hightlight on");
                                        hoverActivateObject.ActivateHover();
                                    }
                                    else {
                                        Debug.LogError("Missing hover on faciltiy " + kvp.Value.name);
                                    }
                                }
                            }
                            hoveredObject = kvp.Value.transform.parent.gameObject;
                        }
                    }
                    hoveredDropLocation = hoveredObject;
                    // Debug.Log("Hovered over " + hoveredDropLocation.name);
                    break;
                }
            }
        }

        // Handle fade out if we've moved off a facility
        if (previousHoveredFacility != null && previousHoveredFacility != currentHoveredFacility) {
            if (previousHoveredFacility.TryGetComponent(out HoverActivateObject previousHoverActivateObject)) {
                //   Debug.Log("Highlight off");
                previousHoverActivateObject.DeactivateHover();
            }
            else {
                Debug.LogError("Missing hover on faciltiy " + previousHoverActivateObject.name);
            }


        }
        // If we're not over any drop location, set hoveredDropLocation to null
        if (!isOverAnyDropLocation) {
            hoveredDropLocation = null;
        }

        previousHoveredFacility = currentHoveredFacility;
    }
    #endregion
    void InitDropLocations() {

        var dropZones = FindObjectsOfType<CardDropLocation>();
        foreach (var dropZone in dropZones) {
            var tag = dropZone.tag;
            if (cardDropLocations.ContainsKey(tag)) {
                tag += ++facilityCount;
            }
            cardDropLocations.Add(tag, dropZone.gameObject);
        }


    }
    public Card HandleCardDrop(Card card) {

        //  Debug.Log("CardPlayer HandleCardDrop");

        if (hoveredDropLocation == null) {
            Debug.Log("No drop location found");
            return null;
        }
        else {
            if (ValidateCardPlay(card)) {
                //HandlePlayCard(card, hoveredDropLocation);
                //set card state to played
                card.state = CardState.CardDrawnDropped;
                handPositioner.cards.Remove(card);
                card.transform.transform.SetParent(hoveredDropLocation.transform);
                Debug.Log($"Set {card.front.name} State to CardDrawnDropped");
                return card;
            }
            else {
                //reset card positions
                handPositioner.ResetCardSiblingIndices();
            }

        }
        return null;
    }
    private bool ValidateCardPlay(Card card) {
        //much simpler card validation
        var canPlay = GameManager.instance.MGamePhase switch {
            GamePhase.Draw => CanDiscardCard(),
            GamePhase.Bonus => false, //TODO get clarification on this phase
            GamePhase.Action => playerSector.SpendMeeples(card, ref mMeeplesSpent), //returns true if the card could be afforded, false if not, will also spend the meeples on the sector
            _ => false,
        };
        //var canPlay = true;

        Debug.Log($"Playing {card.front.title} on {hoveredDropLocation.name} - {(canPlay ? "Allowed" : "Rejected")}");

        return canPlay;
    }
    private bool CanDiscardCard() {
        return hoveredDropLocation.CompareTag("DiscardDropLocation") && GameManager.instance.MNumberDiscarded < GameManager.instance.MAX_DISCARDS;
    }
    public bool IsPlayerTurn() {
        //replace with call to game manager?
        //some code to validate turn order red goes before blue
        return true;
    }


    public void ResetMeepleCost() {
        mMeeplesSpent = 0;
    }

    public void HandleAttackPhase(CardPlayer opponent) {
        List<int> facilitiesToRemove = new List<int>(8);

        // for all active facilities
        foreach (GameObject facilityGameObject in ActiveFacilities.Values) { }
        //{
        //    Facility facilityCard = facilityGameObject.GetComponent<Facility>();
        //    // for all attacking cards on those facilities
        //    foreach(CardIDInfo cardInfo in facilityCard.AttackingCards)
        //    {
        //        // TODO: Remove random


        //        // run the effects of the card, but only if we roll between 11-20 on a d20 does the attack happen
        //        // This is the same as 50-99 on a 0-100 random roll
        //        int randomNumber = UnityEngine.Random.Range(0, 100);
        //        if (randomNumber >= 50)
        //        {
        //            // get the card
        //            GameObject opponentAttackObject = opponent.GetActiveCardObject(cardInfo);

        //            // run the attack effects
        //            if (opponentAttackObject != null)
        //            {

        //                Card opponentCard = opponentAttackObject.GetComponent<Card>();
        //                Debug.Log("attacking card with value : " + opponentCard.data.facilityAmount);
        //                opponentCard.Play(this, opponent, facilityCard);
        //                mUpdatesThisPhase.Add(new Updates
        //                {
        //                    WhatToDo = AddOrRem.Remove,
        //                    UniqueFacilityID = facilityCard.UniqueID,
        //                    CardID = opponentCard.data.cardID
        //                });
        //            } else
        //            {
        //                Debug.Log("there's a problem because an opponent attack card wasn't in the opponent's active list.");
        //            }
        //        }
        //    }

        //    Debug.Log("facility worth is " + (facilityCard.data.facilityAmount + facilityCard.DefenseHealth));

        //    // now check the total worth of the facility to see if it
        //    // and do a removal of all cards that were spent in attacks
        //    if (facilityCard.data.facilityAmount+facilityCard.DefenseHealth <= 0)
        //    {
        //        Debug.Log("we need to get rid of this facility");
        //        // the facility needs to be removed along with all remaining
        //        // attack cards on it
        //        foreach(CardIDInfo cardInfo in facilityCard.AttackingCards)
        //        {
        //            GameObject cardObject = opponent.GetActiveCardObject(cardInfo);
        //            if (cardObject != null)
        //            {
        //                Card cardToDispose = cardObject.GetComponent<Card>();
        //                Debug.Log("handling all attack cards on defunct facility : this one's id is " + cardToDispose.UniqueID);
        //                cardToDispose.state = CardState.CardNeedsToBeDiscarded;

        //            } else
        //            {
        //                Debug.Log("attack card with id " + cardInfo.CardID + " wasn't found in the pile of cards on a defunct facility.");
        //            }
        //            //opponent.HandleDiscard(opponent.ActiveCards, opponent.opponentDropZone, facilityCard.UniqueID, true);
        //        }
        //        // let's discard all the cards on the facility in question
        //        opponent.DiscardAllInactiveCards(DiscardFromWhere.MyPlayZone, true, facilityCard.UniqueID);
        //        facilityCard.AttackingCards.Clear();
        //        facilityCard.state = CardState.CardNeedsToBeDiscarded;

        //        mUpdatesThisPhase.Add(new Updates
        //        {
        //            WhatToDo = AddOrRem.Remove,
        //            UniqueFacilityID = facilityCard.UniqueID,
        //            CardID = facilityCard.data.cardID
        //        });

        //    } 

        //}

        // now discard all facilities annihilated
        DiscardAllInactiveCards(DiscardFromWhere.MyFacility, false, -1);

    }

    public GameObject GetActiveCardObject(CardIDInfo cardIdInfo) {
        GameObject cardObject = null;
        if (ActiveCards.ContainsKey(cardIdInfo.UniqueID)) {
            cardObject = ActiveCards[cardIdInfo.UniqueID];
        }
        else if (HandCards.ContainsKey(cardIdInfo.UniqueID)) {
            Debug.Log("hand cards contained the card with unique id " + cardIdInfo.UniqueID);
        }

        return cardObject;
    }

    public void DiscardAllInactiveCards(DiscardFromWhere where, bool addUpdate, int uniqueFacilityID) {
        List<int> inactives = new List<int>(10);
        Dictionary<int, GameObject> discardFromArea = where switch {
            DiscardFromWhere.Hand => HandCards,
            DiscardFromWhere.MyPlayZone => ActiveCards,
            DiscardFromWhere.MyFacility => ActiveFacilities,
            _ => HandCards,
        };
        foreach (GameObject activeCardObject in discardFromArea.Values) {
            //GameObject activeCardObject = ActiveCardList[i];
            Card card = activeCardObject.GetComponent<Card>();

            if (card.state == CardState.CardNeedsToBeDiscarded) {
                Discards.Add(card.UniqueID, activeCardObject);
                inactives.Add(card.UniqueID);
                card.state = CardState.CardDiscarded;

                // change parent and rescale
                activeCardObject.GetComponentInParent<HoverScale>().previousScale = Vector2.zero;
                activeCardObject.GetComponentInParent<HoverScale>().ResetScale();
                activeCardObject.GetComponentInParent<slippy>().enabled = false;
                activeCardObject.GetComponentInParent<slippy>().ResetScale();
                activeCardObject.GetComponent<HoverScale>().enabled = false;
                activeCardObject.GetComponent<slippy>().ResetScale();
                activeCardObject.GetComponent<slippy>().enabled = false;
                activeCardObject.transform.SetParent(discardDropZone.transform, false);
                activeCardObject.transform.localPosition = new Vector3();
                activeCardObject.transform.localScale = new Vector3(1, 1, 1);

                // for the future might want to stack cards in the discard zone
                Debug.Log("setting card to discard zone: " + card.UniqueID + " with name " + card.front.title);
                activeCardObject.SetActive(false);
                card.cardZone = discardDropZone;
                if (addUpdate) {
                    Debug.Log("adding update for opponent to get");
                    mUpdatesThisPhase.Add(new Updates {
                        WhatToDo = AddOrRem.Remove,
                        UniqueFacilityID = uniqueFacilityID,
                        CardID = card.data.cardID
                    });
                }
            }
        }
        foreach (int key in inactives) {
            Debug.Log("key being discarded is " + key);
            if (!discardFromArea.Remove(key)) {
                Debug.Log("card not removed where it supposedly was from: " + key);
            }
        }
    }

    public int GetMeeplesSpent() {
        return mMeeplesSpent;
    }
    public int AddMeeplesSpent(int meeples) {
        mMeeplesSpent += meeples;
        return mMeeplesSpent;
    }

    public int GetTotalMeeples() {
        return blueMeepleCount + purpleMeepleCount + blackMeepleCount;
    }

    public virtual int HandlePlayCard(GamePhase phase, CardPlayer opponentPlayer) {
        int playCount = 0;
        int playKey = 0;

        if (HandCards.Count != 0) {
            foreach (GameObject gameObjectCard in HandCards.Values) {
                Card card = gameObjectCard.GetComponent<Card>();
                if (card.state == CardState.CardDrawnDropped) {
                    Debug.Log("card dropped in cardhandle");
                    // card has been dropped somewhere - where?
                    Vector2 cardPosition = card.getDroppedPosition();

                    // DO a AABB collision test to see if the card is on the discard drop
                    if ((cardPosition.y < discardDropMax.y &&
                       cardPosition.y > discardDropMin.y &&
                       cardPosition.x < discardDropMax.x &&
                       cardPosition.x > discardDropMin.x)) {
                        switch (phase) {
                            case GamePhase.Draw:
                                Debug.Log("card dropped in discard zone or needs to be discarded" + card.UniqueID);

                                // change parent and rescale
                                card.state = CardState.CardNeedsToBeDiscarded;
                                playCount = 1;
                                break;
                            case GamePhase.Action:
                                break;
                        }
                    }

                    // DO a AABB collision test to see if the card is on the player's drop
                    else if (cardPosition.y < playedDropMax.y &&
                       cardPosition.y > playedDropMin.y &&
                       cardPosition.x < playedDropMax.x &&
                       cardPosition.x > playedDropMin.x) {
                        Debug.Log("collision with played area");
                        switch (phase) {
                            case GamePhase.Action:
                                /*if (card.data.cardType==CardType.Defense && CheckHighlightedStations())
                                {
                                    GameObject selected = GetHighlightedStation();
                                    Card selectedCard = selected.GetComponent<Card>();
                                    StackCards(selected, gameObjectCard, playerDropZone, GamePhase.Defense);
                                    card.state = CardState.CardInPlay;
                                    ActiveCards.Add(card.UniqueID, gameObjectCard);

                                    selectedCard.ModifyingCards.Add(card.UniqueID);
                                    mUpdatesThisPhase.Add(new Updates
                                    {
                                        WhatToDo=AddOrRem.Add,
                                        UniqueFacilityID=selectedCard.UniqueID,
                                        CardID=card.data.cardID
                                    });

                                    // we should play the card's effects
                                    card.Play(this, opponentPlayer, selectedCard);
                                    playCount = 1;
                                    selectedCard.OutlineImage.SetActive(false);
                                    playKey = card.UniqueID;
                                }
                                else
                                {
                                    card.state = CardState.CardDrawn;
                                    manager.DisplayGameStatus("Please select a single facility you own and play a defense card type.");
                                }*/
                                break;
                            
                            //break;
                            default:
                                // we're not in the right phase, so
                                // reset the dropped state
                                //card.state = CardState.CardDrawn;
                                ResetCardToInHand(card);
                                break;
                        }

                    }
                    
                    else {
                        Debug.Log("card not dropped in card drop zone");
                        // If it fails, parent it back to the hand location and then set its state to be in hand and make it grabbable again
                        //  gameObjectCard.transform.SetParent(handDropZone.transform, false);
                        //  card.state = CardState.CardDrawn;
                        handPositioner.ReturnCardToHand(card);
                        gameObjectCard.GetComponentInParent<slippy>().enabled = true;
                        gameObjectCard.GetComponent<HoverScale>().Drop();
                    }
                }

                // index of where this card is in handlist
                if (playCount > 0) {
                    break;
                }
            }
        }

        if (playCount > 0) {
            if (phase == GamePhase.Draw) {
                // we're not discarding a facility or sharing what we're discarding with the opponent
                DiscardAllInactiveCards(DiscardFromWhere.Hand, false, -1);
            }
            else {
                // remove the discarded card
                if (!HandCards.Remove(playKey)) {
                    Debug.Log("didn't find a key to remove! " + playKey);
                }
            }
        }

        return playCount;
    }
    //reset card state to in card drawn and return to the hand positioner by setting parent to hand drop zone
    public void ResetCardToInHand(Card card) {
        card.state = CardState.CardDrawn;
        handPositioner.ReturnCardToHand(card);
    }

    public bool DuplicateCardPlayed(Card facilityCard, Card cardToPlay) {
        bool duplicateCardFound = false;

        foreach (CardIDInfo cardInfo in facilityCard.AttackingCards) {
            if (cardInfo.CardID == cardToPlay.data.cardID) {
                duplicateCardFound = true;
                break;
            }
        }

        return duplicateCardFound;
    }

    public void ChangeScaleAndPosition(Vector2 scale, GameObject objToScale) {
        Transform parent = objToScale.transform.parent;
        slippy parentSlippy = objToScale.GetComponentInParent<slippy>();
        slippy areaSlippy = objToScale.GetComponent<slippy>();

        if (parent != null && parentSlippy != null) {
            objToScale.transform.SetParent(null, true);

            parentSlippy.originalScale = scale;
            parentSlippy.originalPosition = new Vector3();
            parentSlippy.ResetScale();

            if (areaSlippy != null) {
                areaSlippy.originalScale = scale;
                areaSlippy.originalPosition = new Vector3();
                areaSlippy.ResetScale();
            }

            objToScale.transform.SetPositionAndRotation(new Vector3(), objToScale.transform.rotation);
        }
        else if (parent != null) {
            objToScale.transform.SetParent(null, true);

            if (areaSlippy != null) {
                areaSlippy.originalScale = scale;
                areaSlippy.originalPosition = new Vector3();
                areaSlippy.ResetScale();
            }

            objToScale.transform.localScale = scale;
            objToScale.transform.SetPositionAndRotation(new Vector3(), objToScale.transform.rotation);
        }
        else {
            if (areaSlippy != null) {
                areaSlippy.originalScale = scale;
                areaSlippy.originalPosition = new Vector3();
                areaSlippy.ResetScale();
            }

            // if there's no parent then our scale is THE scale
            objToScale.transform.localScale = new Vector3(scale.x, scale.y, 1.0f);
            objToScale.transform.SetPositionAndRotation(new Vector3(), objToScale.transform.rotation);


        }
    }

    public void StackCards(GameObject stationObject, GameObject addedObject, GameObject dropZone, GamePhase phase) {
        Card stationCard = stationObject.GetComponent<Card>();

        // unhighlight the outline if it's turned on
        stationCard.OutlineImage.SetActive(false);
        GameObject tempCanvas;

        if (stationCard.HasCanvas) {
            // at least one card is already played on this one!    
            tempCanvas = stationCard.CanvasHolder;

            ChangeScaleAndPosition(new Vector2(1.0f, 1.0f), addedObject);
            addedObject.transform.SetParent(tempCanvas.transform, false);

            // set local offset for actual stacking
            stationCard.stackNumber += 1;
            /*if (phase == GamePhase.Defense)
            {
                // added cards go at the back
                addedObject.transform.SetAsFirstSibling();
            }
            else if (phase == GamePhase.Vulnerability)
            {
                // added cards go at the front if they're vulnerabilities
                addedObject.transform.SetAsLastSibling();
            }
            else if (phase == GamePhase.Mitigate)
            {
                // added cards go at the front if they're vulnerabilities
                addedObject.transform.SetAsLastSibling();
            }*/
            addedObject.transform.SetAsLastSibling();

            addedObject.GetComponent<slippy>().enabled = false;
            addedObject.GetComponent<HoverScale>().previousScale = Vector2.one;
            addedObject.GetComponent<HoverScale>().SlippyOff = true;

        }
        else {
            // add a canvas component and change around the parents
            tempCanvas = Instantiate(cardStackingCanvas);
            // set defaults for canvas
            Transform parent = tempCanvas.transform.parent;
            if (parent != null) {
                tempCanvas.transform.SetParent(null, false);
            }
            tempCanvas.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            tempCanvas.transform.localScale = new Vector3(ORIGINAL_SCALE, ORIGINAL_SCALE, 1.0f);

            // turn slippy off - needs to be here???
            if (addedObject.GetComponentInParent<slippy>() != null) {
                addedObject.GetComponentInParent<slippy>().enabled = false;
            }
            if (stationObject.GetComponentInParent<slippy>() != null) {
                stationObject.GetComponentInParent<slippy>().enabled = false;
            }

            // now reset scale on all the cards under the canvas!
            // this is only necessary since they likely already have their own scale and we
            // want the canvas to now scale them
            ChangeScaleAndPosition(new Vector2(1.0f, 1.0f), stationObject);
            ChangeScaleAndPosition(new Vector2(1.0f, 1.0f), addedObject);

            // now add them to canvas
            addedObject.transform.SetParent(tempCanvas.transform, false);
            addedObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            stationObject.transform.SetParent(tempCanvas.transform, false);
            /*
            if (phase == GamePhase.Defense)
            {
                // added cards go at the back
                addedObject.transform.SetAsFirstSibling();
            }
            else if (phase == GamePhase.Vulnerability)
            {
                // added cards go at the front if they're vulnerabilities
                addedObject.transform.SetAsLastSibling();
            } else if (phase == GamePhase.Mitigate)
            {
                // added cards go at the front if they're vulnerabilities
                addedObject.transform.SetAsLastSibling();
            }*/

            addedObject.transform.SetAsLastSibling();
            // make sure the station knows if has a canvas with children
            stationCard.HasCanvas = true;
            stationCard.CanvasHolder = tempCanvas;
            stationCard.stackNumber += 1;

            // reset some hoverscale info
            addedObject.GetComponent<HoverScale>().previousScale = Vector2.one;
            addedObject.GetComponent<HoverScale>().SlippyOff = true;
            stationObject.GetComponent<HoverScale>().SlippyOff = true;
            stationObject.GetComponent<HoverScale>().previousScale = Vector2.one;

            // add the canvas to the played card holder
            tempCanvas.transform.SetParent(dropZone.transform, false);
            tempCanvas.SetActive(true);

            addedObject.GetComponent<slippy>().enabled = false;
        }

    }

    public void ClearDropState() {
        if (HandCards.Count != 0) {
            foreach (GameObject cardGameObject in HandCards.Values) {
                Card card = cardGameObject.GetComponent<Card>();
                if (card.state == CardState.CardDrawnDropped) {
                    card.state = CardState.CardDrawn;
                }
            }
        }
    }

    public bool CheckHighlightedStations() {
        bool singleHighlighted = false;
        int countHighlighted = 0;

        foreach (GameObject gameObject in ActiveFacilities.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.OutlineImage.activeSelf) {
                countHighlighted++;
            }
        }

        if (countHighlighted == 1)
            singleHighlighted = true;

        return singleHighlighted;
    }

    public GameObject GetHighlightedStation() {
        GameObject station = null;

        foreach (GameObject gameObject in ActiveFacilities.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.OutlineImage.activeSelf) {
                station = gameObject;
                break;
            }
        }

        return station;
    }


    public bool CheckForCardsOfType(CardType cardType, Dictionary<int, GameObject> listToCheck) {
        bool hasCardType = false;

        foreach (GameObject gameObject in listToCheck.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.data.cardType == cardType) {
                hasCardType = true;
                break;
            }
        }

        return hasCardType;
    }

    public void AddUpdate(Updates update, GameObject cardGameObject, GameObject dropZone, GamePhase phase, bool getRidOfFacility) {
        GameObject facility = null;
        Card facilityCard = null;

        // find unique facility in facilities list
        if (ActiveFacilities.TryGetValue(update.UniqueFacilityID, out facility)) {
            facilityCard = facility.GetComponent<Card>();
            // if we found the right facility
            if (cardGameObject != null && update.WhatToDo == AddOrRem.Add) {
                Debug.Log("card add called with phase " + phase);
                // create card to be displayed
                Card card = cardGameObject.GetComponent<Card>();
                if (phase == GamePhase.Action) {
                    Debug.Log("adding attack with card id : " + card.data.cardID);
                    facilityCard.AttackingCards.Add(new CardIDInfo {
                        CardID = card.data.cardID,
                        UniqueID = card.UniqueID
                    });
                    cardGameObject.SetActive(false);

                    // add card to its displayed cards
                    StackCards(facility, cardGameObject, dropZone, phase);
                    card.state = CardState.CardInPlay;
                    cardGameObject.SetActive(true);
                }

            }
            else if (update.WhatToDo == AddOrRem.Remove) {
                if (phase == GamePhase.Action) {
                    if (!getRidOfFacility) {
                        Debug.Log("removing attack  for mitigation with card id " + update.CardID);
                        int cardIndex = facilityCard.AttackingCards.FindIndex(x => x.CardID == update.CardID);

                        if (cardIndex != -1) {
                            CardIDInfo cardInfo = facilityCard.AttackingCards[cardIndex];
                            Debug.Log("facilities attacking cards contained the unique card info " + cardInfo.CardID + " with unique id " + cardInfo.UniqueID);
                            // discard it
                            GameObject cardObject = manager.actualPlayer.GetActiveCardObject(cardInfo);
                            if (cardObject != null) {
                                Card discardCard = cardObject.GetComponent<Card>();
                                discardCard.state = CardState.CardNeedsToBeDiscarded;
                                //manager.actualPlayer.HandleDiscard(manager.actualPlayer.HandCards, manager.actualPlayer.playerDropZone,
                                //    facilityCard.UniqueID, false);
                                //manager.actualPlayer.HandleDiscard(manager.actualPlayer.ActiveCards, manager.actualPlayer.playerDropZone,
                                //facilityCard.UniqueID, false);
                                manager.actualPlayer.DiscardAllInactiveCards(DiscardFromWhere.Hand, false, facilityCard.UniqueID);
                                manager.actualPlayer.DiscardAllInactiveCards(DiscardFromWhere.MyPlayZone, false, facilityCard.UniqueID);
                            }
                            else {
                                Debug.Log("an attack card couldn't be found in the hand at 1139 in CardPlayer. " + cardInfo);
                            }

                            //manager.actualPlayer.DiscardSingleActiveCard(facilityCard.UniqueID, cardInfo, false);
                            // remove the card info from the facility
                            facilityCard.AttackingCards.RemoveAt(cardIndex);
                        }
                    }
                    else {
                        // discard all the cards attacking this now dead facility
                        foreach (CardIDInfo cardInfo in facilityCard.AttackingCards) {
                            GameObject cardObject = manager.actualPlayer.GetActiveCardObject(cardInfo);
                            if (cardObject != null) {
                                Card cardToDispose = cardObject.GetComponent<Card>();
                                Debug.Log("handling all attack cards on defunct facility : this one's id is " + cardToDispose.UniqueID);
                                cardToDispose.state = CardState.CardNeedsToBeDiscarded;

                            }
                            else {
                                Debug.Log("attack card with id " + cardInfo.CardID + " wasn't found in the pile of cards on a defunct facility.");
                            }
                            //opponent.HandleDiscard(opponent.ActiveCards, opponent.opponentDropZone, facilityCard.UniqueID, true);
                        }
                        // let's discard all the cards on the facility in question
                        manager.actualPlayer.DiscardAllInactiveCards(DiscardFromWhere.MyPlayZone, true, facilityCard.UniqueID);
                        facilityCard.AttackingCards.Clear();
                        facilityCard.state = CardState.CardNeedsToBeDiscarded;

                        // now discard the facility itself
                        //HandleDiscard(ActiveFacilities, dropZone, facilityCard.UniqueID, false);
                        DiscardAllInactiveCards(DiscardFromWhere.MyFacility, false, facilityCard.UniqueID);
                    }
                }
            }
        }
        else {
            Debug.Log("a facility wasn't found for an opponent play - there's a bug somewhere OR the facility just ran out of points and got nixed.");
        }
    }

    public void AddUpdates(ref List<Updates> updates, GamePhase phase, CardPlayer opponent) {
        foreach (Updates update in updates) {
            GameObject facility;
            Facility selectedFacility = null;
            int index = -1;
            Debug.Log("number of active facilities are " + ActiveFacilities.Count);

            // find unique facility in facilities list
            if (ActiveFacilities.TryGetValue(update.UniqueFacilityID, out facility)) {
                selectedFacility = facility.GetComponent<Facility>();

                // if we found the right facility
                if (update.WhatToDo == AddOrRem.Add) {
                    // create card to be displayed
                    Card card = DrawCard(false, update.CardID, -1, ref DeckIDs, opponentDropZone, true, ref ActiveCards);
                    GameObject cardGameObject = ActiveCards[card.UniqueID];
                    cardGameObject.SetActive(false);

                    // add card to its displayed cards
                    StackCards(facility, cardGameObject, opponentDropZone, GamePhase.Action);
                    card.state = CardState.CardInPlay;
                    Debug.Log("opponent player updates added " + card.data.cardID + " to the active list of size " + ActiveCards.Count);
                    card.Play(this, opponent, selectedFacility);
                    cardGameObject.SetActive(true);
                }

            }
            else {
                Debug.Log("a facility was not found for an opponent play - there's a bug somewhere.");
            }
        }

    }

    void CalculateScore() {
        mFinalScore = 42;
    }

    public int GetScore() {
        CalculateScore();
        return mFinalScore;
    }

    public bool HasUpdates() {
        return (mUpdatesThisPhase.Count != 0);
    }

    // an update message consists of:
    // a. count of updates - 1 per card
    // b. what game phase this is happening for
    // c. the list of updates in the order of: add/remove, unique facility id, card id
    public void GetUpdatesInMessageFormat(ref List<int> playsForMessage, GamePhase phase) {
        playsForMessage.Add(mUpdatesThisPhase.Count);
        playsForMessage.Add((int)phase);

        foreach (Updates update in mUpdatesThisPhase) {
            playsForMessage.Add((int)update.WhatToDo);
            playsForMessage.Add(update.UniqueFacilityID);
            playsForMessage.Add(update.CardID);
            //Debug.Log("adding update to send to opponent: " + update.UniqueFacilityID + " and card id " + update.CardID + " for phase " + phase);
        }

        // we've given the updates away, so let's make sure to 
        // clear the list
        mUpdatesThisPhase.Clear();
    }

    // Reset the variables in this class to allow for a new
    // game to happen.
    public void ResetForNewGame() {
        Debug.Log("resetting all game objects on screen - destroying game objects.");
        foreach (GameObject gameObject in HandCards.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.CanvasHolder != null) {
                Destroy(card.CanvasHolder);
            }
            Destroy(card);
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in Discards.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.CanvasHolder != null) {
                Destroy(card.CanvasHolder);
            }
            Destroy(card);
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in ActiveCards.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.CanvasHolder != null) {
                Destroy(card.CanvasHolder);
            }
            Destroy(card);
            Destroy(gameObject);
        }

        foreach (GameObject gameObject in ActiveFacilities.Values) {
            Card card = gameObject.GetComponent<Card>();
            if (card.CanvasHolder != null) {
                Destroy(card.CanvasHolder);
            }
            Destroy(card);
            Destroy(gameObject);
        }

        FacilityIDs.Clear();
        DeckIDs.Clear();
        HandCards.Clear();
        Discards.Clear();
        ActiveCards.Clear();
        ActiveFacilities.Clear();
        handSize = 0;
        mTotalMeepleValue = 0;
        mMeeplesSpent = 0;
        mFinalScore = 0;
        mUpdatesThisPhase.Clear();
    }
}
