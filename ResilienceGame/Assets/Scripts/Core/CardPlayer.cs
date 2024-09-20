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
using UnityEngine.PlayerLoop;
using static UnityEngine.PlayerLoop.EarlyUpdate;

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

//public enum AddOrRem {
//    Add,
//    Remove
//};

public struct Update {
    public CardMessageType Type;
    public int CardID;
    public int UniqueID;
    public int Amount;
    public FacilityEffectTarget FacilityType;
    public FacilityEffectType Effect;
};

public enum DiscardFromWhere {
    // TODO: Needs ref to others se
    Hand,
    MyPlayZone,
    MyFacility
};

public class CardPlayer : MonoBehaviour {
    // Establish necessary fields
    public string playerName;
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
    private GameObject cardDroppedOnObject;
    public Dictionary<string, GameObject> cardDropLocations = new Dictionary<string, GameObject>();
    //private Dictionary<string, Collider2D> cardDropColliders = new Dictionary<string, Collider2D>();

    int facilityCount = 0;
    //Meeples
    // TODO: Move to Sector.cs if needed
    // public int blueMeepleCount = 2, blackMeepleCount = 2, purpleMeepleCount = 2;
    //int mTotalMeepleValue = 0;
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
    Queue<Update> mUpdatesThisPhase = new Queue<Update>(6);




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
            //    Debug.Log("adding card " + card.name + " with id " + card.data.cardID + " to deck " + DeckName);
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
                    DrawCard(
                        random: true,
                        cardId: 0,
                        uniqueId: -1,
                        deckToDrawFrom: ref DeckIDs,
                        dropZone: handDropZone,
                        allowSlippy: true,
                        activeDeck: ref HandCards);

                }
                else {
                    break;
                }
            }
        }
    }
    public virtual void ForceDrawSpecificCard(int id) {
        if (DeckIDs.Count > 0) {
            DrawCard(false, id, -1, ref DeckIDs, handDropZone, true, ref HandCards);
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
    //This is for testing to force draw a specific card
    public void DisplayCardSelectionMenu() {

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
              //  Debug.Log("found proper card!");
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
          //  Debug.Log(tempRaws[i]);
            if (tempRaws[i].name == "Image") {
                tempRaws[i].texture = tempCard.front.img;
            }
            else if (tempRaws[i].name == "Background") {
                tempRaws[i].color = tempCard.front.color;
             //   Debug.Log(tempCard.front.color);
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
             //   Debug.Log("active deck value: " + card.UniqueID);
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
        if (GameManager.instance.DEBUG_ENABLED) {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame) {
                HandleMenuToggle();
            }
            if (Mouse.current.rightButton.wasReleasedThisFrame) {
                TryLogFacilityInfo();
            }
        }
    }
    public void HandleMenuToggle() {
        if (this != GameManager.instance.actualPlayer) {
            return;
        }
        var cardMenu = FindObjectOfType<CardSelectionMenu>();
        if (cardMenu != null) {
            if (cardMenu.IsMenuActive) {
                Debug.Log("Hiding card selection menu");
                cardMenu.DisableMenu();
            }
            else {
                Debug.Log("Showing card selection menu");
                var cardsList = new List<Card>(cards.Values);
                cardMenu.EnableMenu(cardsList);
            }
        }
    }
    //updates the hoverDropLocation class field to hold the object the card is hovering over
    void UpdateHoveredDropLocation() {
        GameObject currentHoveredFacility = null; // Reset at the beginning of each update
        bool isOverAnyDropLocation = false;
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Collider2D[] hoveredColliders = Physics2D.OverlapPointAll(mousePosition, LayerMask.GetMask("CardDrop"));

        if (hoveredColliders != null && hoveredColliders.Length > 0) {
            isOverAnyDropLocation = true;
            Collider2D hoveredFacilityCollider = null;

            // Check for a facility collider if there are multiple
            if (hoveredColliders.Length == 2) {
                foreach (var collider in hoveredColliders) {
                    if (collider.CompareTag(CardDropZoneTag.FACILITY)) {
                        hoveredFacilityCollider = collider;
                        break;
                    }
                }
                // If no facility collider is found, process the other collider
                if (hoveredFacilityCollider == null) {
                    hoveredFacilityCollider = hoveredColliders.First();
                }
            }
            else {
                // Only one collider, process that
                hoveredFacilityCollider = hoveredColliders.First();
            }

            // Process the hovered facility collider
            if (hoveredFacilityCollider != null) {
                var cardDraggedTarget = handPositioner.CardsBeingDragged.First().target;
                // Check if the card being dragged is a facility card
                if (cardDraggedTarget == CardTarget.Facility || cardDraggedTarget == CardTarget.Effect) {
                    if (GameManager.instance.CanStationsBeHighlighted()) {
                        // Activate the hover effect
                        if (hoveredFacilityCollider.TryGetComponent(out HoverActivateObject hoverActivateObject)) {
                            hoverActivateObject.ActivateHover();
                            currentHoveredFacility = hoveredFacilityCollider.gameObject; // Assign currentHoveredFacility
                        }
                    }
                }
                hoveredDropLocation = hoveredFacilityCollider.gameObject;
            }
        }

        // Handle fade out if we've moved off a facility
        if (previousHoveredFacility != null && previousHoveredFacility != currentHoveredFacility) {
            if (previousHoveredFacility.TryGetComponent(out HoverActivateObject previousHoverActivateObject)) {
                previousHoverActivateObject.DeactivateHover();
            }
            else {
                Debug.LogError("Missing hover on facility " + previousHoveredFacility.name);
            }
        }

        // If we're not over any drop location, set hoveredDropLocation to null
        if (!isOverAnyDropLocation) {
            hoveredDropLocation = null;
        }

        // Debug.Log("Hovered Drop Location: " + hoveredDropLocation);

        // Update previous hovered facility
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
            //cardDropColliders.Add(tag, dropZone.GetComponent<Collider2D>());
        }


    }
    public Card HandleCardDrop(Card card) {
        if (hoveredDropLocation == null) {
            Debug.Log("No drop location found");
            return null;
        }
        else {
            //clear the hover effect
            if (hoveredDropLocation.CompareTag("FacilityDropLocation")) {
                hoveredDropLocation.GetComponent<HoverActivateObject>().DeactivateHover();
            }
            if (ValidateCardPlay(card)) {
                //set var to hold where the card was dropped
                cardDroppedOnObject = hoveredDropLocation;
                //set card state to played
                card.state = CardState.CardDrawnDropped;
                //remove card from hand
                handPositioner.cards.Remove(card);
                //set the parent to where it was played
                card.transform.transform.SetParent(hoveredDropLocation.transform);
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
            GamePhase.Bonus => false, //turn only happens during Doomclock? where you can allocate overtime
            GamePhase.Action => ValidateActionPlay(card),
            _ => false,
        };
        Debug.Log($"Playing {card.front.title} on {hoveredDropLocation.name} - {(canPlay ? "Allowed" : "Rejected")}");

        return canPlay;
    }
    private bool ValidateActionPlay(Card card) {
        //check prereq effects on cards
        if (card.data.preReqEffectId != 0) {
            Facility facility = cardDroppedOnObject.GetComponentInParent<Facility>();
            if (!facility.HasEffect(card.data.preReqEffectId)) {
                Debug.Log("Facility effect does not match card prereq effect");
                return false;
            }
        }
        return playerSector.TrySpendMeeples(card, ref mMeeplesSpent); //returns true if the card could be afforded, false if not, will also spend the meeples on the sector if possible
    }

    private bool CanDiscardCard() {
        //draw phase checks if the player is discarding a card and if they havent discard more than allowed this phase
        if (GameManager.instance.MGamePhase == GamePhase.Draw) {
            return hoveredDropLocation.CompareTag("DiscardDropLocation") && GameManager.instance.MNumberDiscarded < GameManager.instance.MAX_DISCARDS;
        }
        return GameManager.instance.MIsDiscardAllowed;  //if not in draw phase, discard is determined by the game manager
    }
    public bool IsPlayerTurn() {
        //replace with call to game manager?
        //some code to validate turn order red goes before blue
        return true;
    }


    public void ResetMeepleCost() {
        mMeeplesSpent = 0;
    }

    #region old
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
    #endregion
  
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
                    mUpdatesThisPhase.Enqueue(new Update {
                        Type = CardMessageType.DiscardCard,
                        UniqueID = uniqueFacilityID,
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
        return playerSector.GetTotalMeeples();
    }
    public int GetMaxMeeples() {
        return playerSector.GetMaxMeeples();
    }

    private void HandleDiscardDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {
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

    private void HandleFacilityDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {

        Facility facility = FacilityPlayedOn();
        Debug.Log($"Handling {card.front.title} played on {facility.facilityName}");
        switch (phase) {
            case GamePhase.Action:
                // StackCards(facility.gameObject, card.gameObject, playerDropZone, GamePhase.Action); TODO: throwing null ref error?
                card.state = CardState.CardInPlay;
                ActiveCards.Add(card.UniqueID, card.gameObject);
                // NOTE: TO DO - need to add the correct update for the card played since some of them
                // need different info
                mUpdatesThisPhase.Enqueue(new Update {
                    Type = CardMessageType.CardUpdate,
                    UniqueID = card.UniqueID,
                    CardID = card.data.cardID
                });

                card.Play(this, opponentPlayer, facility, card); //TODO: idk if this is right, it passes itself as the "card to be acted on" should this just be null?
                playCount = 1;
                playKey = card.UniqueID;

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
    private Facility FacilityPlayedOn() {
        Facility facility = null;
        if (cardDroppedOnObject != null) {
            facility = cardDroppedOnObject.GetComponentInParent<Facility>();
        }
        return facility;
    }
    private void HandleFreePlayDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {
        Debug.Log($"Handling non facility card - {card.front.title}");
        switch (phase) {
            case GamePhase.Action:
                card.state = CardState.CardInPlay;
                ActiveCards.Add(card.UniqueID, card.gameObject);
                // NOTE TO DO: need to add proper data and message type for the card here
                mUpdatesThisPhase.Enqueue(new Update {
                    Type = CardMessageType.CardUpdate,
                    UniqueID = card.UniqueID,
                    CardID = card.data.cardID
                });

                card.Play(this, opponentPlayer, null, card); //TODO: idk if this is right, it passes itself as the "card to be acted on" should this just be null?
                playCount = 1;
                playKey = card.UniqueID;
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

    public virtual int HandlePlayCard(GamePhase phase, CardPlayer opponentPlayer) {
        int playCount = 0;
        int playKey = 0;

        if (HandCards.Count != 0) {
            //Debug.Log(phase);
            foreach (GameObject gameObjectCard in HandCards.Values) {
                Card card = gameObjectCard.GetComponent<Card>();
                if (card.state == CardState.CardDrawnDropped) {

                    // card has been dropped somewhere - where?
                    // Vector2 cardPosition = card.getDroppedPosition();

                    if (cardDroppedOnObject == null) {
                        Debug.Log("card not dropped in card drop zone");
                        handPositioner.ReturnCardToHand(card);
                        gameObjectCard.GetComponentInParent<slippy>().enabled = true;
                        gameObjectCard.GetComponent<HoverScale>().Drop();
                        return playCount;
                        // Debug.LogError("Card was dropped on null object?");
                    }
                    Debug.Log("Valid card play made somewhere!");
                    //check where the card was dropped based on the tag
                    switch (cardDroppedOnObject.tag) {
                        case CardDropZoneTag.DISCARD:
                            HandleDiscardDrop(card, phase, opponentPlayer, ref playCount, ref playKey);
                            break;
                        case CardDropZoneTag.FACILITY:
                            if (card.target == CardTarget.Facility || card.target == CardTarget.Effect) {
                                HandleFacilityDrop(card, phase, opponentPlayer, ref playCount, ref playKey);
                            }
                            else {
                                HandleFreePlayDrop(card, phase, opponentPlayer, ref playCount, ref playKey);
                            }
                            break;
                        default:
                            Debug.Log("card not dropped in card drop zone");
                            handPositioner.ReturnCardToHand(card);
                            gameObjectCard.GetComponentInParent<slippy>().enabled = true;
                            gameObjectCard.GetComponent<HoverScale>().Drop();
                            break;
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


    // NOTE: TO DO - needs to be updated for new card effects without
    // facilities
    public void AddUpdate(Update update, GamePhase phase, CardPlayer opponent)
    {
        //GameObject facility;
        //Facility selectedFacility = null;
        //int index = -1;
        //Debug.Log("number of active facilities are " + ActiveFacilities.Count);

        //// find unique facility in facilities list
        //if (ActiveFacilities.TryGetValue(update.UniqueID, out facility))
        //{
        //    selectedFacility = facility.GetComponent<Facility>();

        //    // if we found the right facility
           
        //    // facilities
        //    if (update.Type == CardMessageType.CardUpdate)
        //    {
        //        // create card to be displayed
        //        Card card = DrawCard(false, update.CardID, -1, ref DeckIDs, opponentDropZone, true, ref ActiveCards);
        //        GameObject cardGameObject = ActiveCards[card.UniqueID];
        //        cardGameObject.SetActive(false);

        //        // add card to its displayed cards
        //        StackCards(facility, cardGameObject, opponentDropZone, GamePhase.Action);
        //        card.state = CardState.CardInPlay;
        //        Debug.Log("opponent player updates added " + card.data.cardID + " to the active list of size " + ActiveCards.Count);
        //        card.Play(this, opponent, selectedFacility);
        //        cardGameObject.SetActive(true);
        //    }

        //}
        //else
        //{
        //    Debug.Log("a facility was not found for an opponent play - there's a bug somewhere.");
        //}
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
    // a. phase it happened in
    // b. unique card id for a specific player
    // c. id of the card played
    // d. other unique info for special cards
    public CardMessageType GetNextUpdateInMessageFormat(ref List<int> playsForMessage, GamePhase phase) {
        if (mUpdatesThisPhase.Count > 0)
        {
            playsForMessage.Add((int)phase);
            Update update = mUpdatesThisPhase.Dequeue();
            playsForMessage.Add(update.UniqueID);
            playsForMessage.Add(update.CardID);

            if (update.Type == CardMessageType.ReduceCost)
            {
                playsForMessage.Add(update.Amount);
            }
            else if (update.Type == CardMessageType.RemoveEffect)
            {
                playsForMessage.Add((int)update.FacilityType);
                playsForMessage.Add((int)update.Effect);
            }
            else if (update.Type == CardMessageType.RestorePoints)
            {
                playsForMessage.Add(update.Amount);
                playsForMessage.Add((int)update.FacilityType);
            }
            else if (update.Type == CardMessageType.MeepleShare)
            {
                // unique id is the player to share with
                // card id is actually the meeple color
                // amount is the number of meeples to share
                playsForMessage.Add(update.Amount);
            }
            return update.Type;
        }
        return CardMessageType.None;
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
        //mTotalMeepleValue = 0;
        //TODO: reset meeples? clear out sector? reset player sector?
        mMeeplesSpent = 0;
        mFinalScore = 0;
        mUpdatesThisPhase.Clear();
    }
    void TryLogFacilityInfo() {
        if (this != GameManager.instance.actualPlayer) return;
        var hitFacility = cardDropLocations.Values.ToList().Find(x => x.GetComponent<Collider2D>().OverlapPoint(Mouse.current.position.ReadValue()));
        if (hitFacility) {
            var faciltiy = hitFacility.GetComponentInParent<Facility>();
            if (faciltiy) {
                faciltiy.LogFacilityDebug(); 
            }
        }
    }
}
