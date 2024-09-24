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
using static Facility;
using System;

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
    public FacilityType FacilityType;
    public FacilityEffectType EffectTarget;
    public string DiscardedOrReturnedCardUIDs;
};

public enum DiscardFromWhere {
    // TODO: Needs ref to others se
    Hand,
    MyPlayZone,
    MyFacility
};


public class CardPlayer : MonoBehaviour {
    public enum PlayerReadyState {
        ReadyToPlay,
        ReturnCardsToDeck,
        DiscardCards,
        SelectCardsForCostChange
    }
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
    private const int MAX_DRAW_AMOUNT = 5;
    public const int MAX_HAND_SIZE_AFTER_ACTION = 7;
    public GameObject cardPrefab;
    public GameObject discardDropZone;
    public GameObject handDropZone;
    private HandPositioner handPositioner;
    public GameObject opponentDropZone;
    // public GameObject playerDropZone;
    public GameObject cardStackingCanvas;
    public readonly float ORIGINAL_SCALE = 0.2f;
    public string DeckName = "";
    public bool IsDraggingCard { get; private set; } = false;
    public GameObject hoveredDropLocation;
    private GameObject previousHoveredFacility;
    private GameObject cardDroppedOnObject;
    public Dictionary<string, GameObject> cardDropLocations = new Dictionary<string, GameObject>();
    //private Dictionary<string, Collider2D> cardDropColliders = new Dictionary<string, Collider2D>();
    public Queue<(Update, GamePhase, CardPlayer)> opponentCardPlays = new Queue<(Update, GamePhase, CardPlayer)>();
    public bool IsAnimating { get; set; } = false;
    public PlayerReadyState ReadyState { get; set; } = PlayerReadyState.ReadyToPlay;
    //  public bool ForceDiscard { get; set; } = false;
    public int AmountToDiscard { get; set; } = 0;

    public int AmountToReturnToDeck { get; private set; } = 0;

    public List<Card> CardsAllowedToBeDiscard;
    public Action OnCardsReturnedToDeck { get; set; }

    int facilityCount = 0;
    //Meeples
    // TODO: Move to Sector.cs if needed
    // public int blueMeepleCount = 2, blackMeepleCount = 2, purpleMeepleCount = 2;
    //int mTotalMeepleValue = 0;
    int mMeeplesSpent = 0;

    //Vector2 discardDropMin;
    //Vector2 discardDropMax;
    //Vector2 playedDropMin;
    //Vector2 playedDropMax;
    //Vector2 opponentDropMin;
    //Vector2 opponentDropMax;
    // the var is static to make sure the id's don't overlap between
    // multiple card players
    static int sUniqueIDCount = 0;
    int mFinalScore = 0;
    Queue<Update> mUpdatesThisPhase = new Queue<Update>(6);


    bool registeredFacilities = false;

    public void Start() {

        if (handDropZone)
            handPositioner = handDropZone.GetComponent<HandPositioner>();
        else {
            Debug.LogError("Hand drop zone not found");
        }

        InitDropLocations();
        //// discard rectangle information for AABB collisions
        //RectTransform discardRectTransform = discardDropZone.GetComponent<RectTransform>();
        //discardDropMin.x = discardRectTransform.position.x - (discardRectTransform.rect.width / 2);
        //discardDropMin.y = discardRectTransform.position.y - (discardRectTransform.rect.height / 2);
        //discardDropMax.x = discardRectTransform.position.x + (discardRectTransform.rect.width / 2);
        //discardDropMax.y = discardRectTransform.position.y + (discardRectTransform.rect.height / 2);

        //// played area rectangle information for AABB collisions
        //RectTransform playedRectTransform = playerDropZone.GetComponent<RectTransform>();
        //playedDropMin.x = playedRectTransform.position.x - (playedRectTransform.rect.width / 2);
        //playedDropMin.y = playedRectTransform.position.y - (playedRectTransform.rect.height / 2);
        //playedDropMax.x = playedRectTransform.position.x + (playedRectTransform.rect.width / 2);
        //playedDropMax.y = playedRectTransform.position.y + (playedRectTransform.rect.height / 2);

        //// playing on opponent area rectangle information
        //RectTransform opponentRectTransform = opponentDropZone.GetComponent<RectTransform>();
        //opponentDropMin.x = opponentRectTransform.position.x - (opponentRectTransform.rect.width / 2);
        //opponentDropMin.y = opponentRectTransform.position.y - (opponentRectTransform.rect.height / 2);
        //opponentDropMax.x = opponentRectTransform.position.x + (opponentRectTransform.rect.width / 2);
        //opponentDropMax.y = opponentRectTransform.position.y + (opponentRectTransform.rect.height / 2);

    }
    public bool NeedsToDiscard() {
        return HandCards.Count > MAX_HAND_SIZE_AFTER_ACTION;
    }
    public void AddDiscardEvent(int amount, List<Card> cardsAllowedToBeDiscard = null) {
        ReadyState = PlayerReadyState.DiscardCards;
        AmountToDiscard = amount;
        discardDropZone.SetActive(true);
        CardsAllowedToBeDiscard = cardsAllowedToBeDiscard;
        Debug.Log($"Enabling {playerName}'s discard temporarily");
    }
    public void StopDiscard() {
        ReadyState = PlayerReadyState.ReadyToPlay;
        CardsAllowedToBeDiscard?.ForEach(card => card.ToggleOutline(false));
        CardsAllowedToBeDiscard = null;
        discardDropZone.SetActive(false);
        GameManager.instance.mAlertPanel.ResolveAlert();
        Debug.Log($"Disabling {playerName}'s discard");
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
    //add the facilities to the player's active facilities
    public void RegisterFacilities() {
        registeredFacilities = true;
        Debug.Log($"Player {playerName} of team {playerTeam} registering facilities");
        foreach (Facility facility in playerSector.facilities) {
            ActiveFacilities.Add((int)facility.facilityType, facility.gameObject);
            FacilityIDs.Add((int)facility.facilityType);
        }
    }
    //draws enough cards until max hand size
    public virtual void DrawCardsToFillHand() {
        int numCards = MAX_DRAW_AMOUNT - HandCards.Count;
        if (numCards <= 0) {
            return;
        }
        DrawNumberOfCards(numCards, updateNetwork: true);
    }
    //add the number of cards from deck to player hand
    public virtual void DrawNumberOfCards(int num, List<Card> cardsDrawn = null, bool highlight = false, bool updateNetwork = false) {

        Card cardDrawn = null;
        if (DeckIDs.Count > 0) {
            for (int i = 0; i < num; i++) {
                cardDrawn = DrawCard(
                    random: true,
                    cardId: 0,
                    uniqueId: -1,
                    deckToDrawFrom: ref DeckIDs,
                    dropZone: handDropZone,
                    allowSlippy: true,
                    activeDeck: ref HandCards,
                    sendUpdate: updateNetwork);
                if (highlight) {
                    cardDrawn.ToggleOutline(true);
                }
                cardsDrawn?.Add(cardDrawn);
            }
        }
    }
    //These are for testing purposes to add/remove cards from the hand
    public virtual void DrawSpecificCard(int id, GameObject handParent, bool updateNetwork = false) {
        Debug.Log($"{playerName} is trying to draw {GetCardNameFromID(id)} with id {id}");
        if (DeckIDs.Count > 0) {
            DrawCard(false, id, -1, ref DeckIDs, handParent, true, ref HandCards, updateNetwork);
        }
    }
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
        card.GetComponent<Card>().SetCardState(CardState.CardNeedsToBeDiscarded);
        card.transform.SetParent(discardDropZone.transform, false);
        card.transform.localPosition = new Vector3();
    }
    //Creates a card and adds it to the activeDeck from the deckToDrawFrom
    protected virtual Card DrawCard(bool random, int cardId, int uniqueId, ref List<int> deckToDrawFrom,
        GameObject dropZone, bool allowSlippy,
        ref Dictionary<int, GameObject> activeDeck, bool sendUpdate = false) {
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
        tempCard.SetCardState(CardState.CardDrawn);
        //  Vector3 tempPos = tempCardObj.transform.position;
        //  tempCardObj.transform.position = tempPos;
        tempCardObj.transform.SetParent(dropZone.transform, false);
        // Vector3 tempPos2 = dropZone.transform.position;
        handSize++;
        //  tempCardObj.transform.position = tempPos2;
        tempCardObj.transform.localPosition = Vector3.zero;
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

        //send the update to the opponent about which card was drawn
        if (sendUpdate) {
            Update update = new Update() {
                UniqueID = tempCard.UniqueID,
                CardID = tempCard.data.cardID,
                Type = CardMessageType.DrawCard
            };
            mUpdatesThisPhase.Enqueue(update);
            GameManager.instance.SendUpdatesToOpponent(GameManager.instance.MGamePhase, this);
        }

        // GameManager.instance.UpdateDeckSizeText(); //update UI




        return tempCard;
    }

    #region Update Functions
    // Update is called once per frame
    void Update() {
        IsDraggingCard = handPositioner.IsDraggingCard;

        //init once sector is ready
        if (!registeredFacilities) {
            if (playerSector != null) {
                if (playerSector.facilities != null && playerSector.facilities.Length > 0)
                    RegisterFacilities();
            }
        }

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
        //  Debug.Log("Hovered Colliders: " + hoveredColliders.Length);
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
                    if (GameManager.instance.CanHighlight()) {
                        // Activate the hover effect
                        if (hoveredFacilityCollider.TryGetComponent(out HoverActivateObject hoverActivateObject)) {
                            //    Debug.Log("Hovering");
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
            //Debug.Log($"Adding {tag} to cardDropLocations");
            cardDropLocations.Add(tag, dropZone.gameObject);

            //cardDropColliders.Add(tag, dropZone.GetComponent<Collider2D>());
        }
        // Debug.Log("Card Drop Locations: " + cardDropLocations.Count);


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
                //check if the game is waiting for the player to return cards to the deck by playing them
                if (ReadyState == PlayerReadyState.ReturnCardsToDeck) {
                    Debug.Log("Returning card to deck");
                    ReturnCardToDeck(card);
                    //update the card update with the card that was returned
                    if (mUpdatesThisPhase.TryPeek(out Update update)) {
                        if (update.Type == CardMessageType.CardUpdate) {
                            if (update.DiscardedOrReturnedCardUIDs == null || update.DiscardedOrReturnedCardUIDs == "") {
                                update.DiscardedOrReturnedCardUIDs = card.UniqueID.ToString();
                            }
                            else {
                                update.DiscardedOrReturnedCardUIDs += ";" + card.UniqueID;
                            }
                        }
                    }
                    AmountToReturnToDeck--;
                    if (AmountToReturnToDeck > 0) {
                        GameManager.instance.DisplayAlertMessage($"Return {AmountToReturnToDeck} more cards to the deck", this); //update alert message
                    }
                    else {
                        OnCardsReturnedToDeck?.Invoke(); //Resolve the action after cards have been returned to deck
                        GameManager.instance.mAlertPanel.ResolveAlert(); //remove alert message
                        ReadyState = PlayerReadyState.ReadyToPlay; //reset player state
                        //update opponent now that the update has all the info it needs
                        GameManager.instance.SendUpdatesToOpponent(GameManager.instance.MGamePhase, this);
                    }

                }
                //check for a card play or card discard
                else if (ReadyState == PlayerReadyState.ReadyToPlay || ReadyState == PlayerReadyState.DiscardCards) {
                    Debug.Log("Card is valid to play and player is ready");
                    //set var to hold where the card was dropped
                    cardDroppedOnObject = hoveredDropLocation;
                    //set card state to played
                    card.SetCardState(CardState.CardDrawnDropped);
                    //remove card from hand
                    handPositioner.cards.Remove(card);
                    //set the parent to where it was played
                    card.transform.transform.SetParent(hoveredDropLocation.transform);
                    return card;
                }
                else if (ReadyState == PlayerReadyState.SelectCardsForCostChange) {

                }

            }
            else {
                //reset card positions
                handPositioner.ResetCardSiblingIndices();
            }

        }
        return null;
    }
    private void ReturnCardToDeck(Card card) {
        handPositioner.cards.Remove(card);
        DeckIDs.Add(card.data.cardID);//add it back to the deck
        HandCards.Remove(card.UniqueID);
        Destroy(card.gameObject);
    }
    public void ReturnHandToDeckAndDraw(int amount) {
        HandCards.Values.ToList().ForEach(card => {
            ReturnCardToDeck(card.GetComponent<Card>());
        });
        //for (int i = 0; i < amount; i++) {
        //    DrawCard(true, 0, -1, ref DeckIDs, handDropZone, true, ref HandCards);
        //}
        DrawNumberOfCards(amount, updateNetwork: true);
    }

    private bool ValidateCardPlay(Card card) {
        string response = "";
        bool canPlay = false;
        if (AmountToReturnToDeck > 0) {
            Debug.Log($"Returning {card.front.title} to deck");
            return true;
        }
        switch (GameManager.instance.MGamePhase) {
            case GamePhase.DrawRed:
            case GamePhase.DrawBlue:
                (response, canPlay) = CanDiscardCard(card);
                break;
            case GamePhase.BonusBlue:
            case GamePhase.BonusRed:
                (response, canPlay) = ("Cannot play cards during bonus phase", false); //turn only happens during Doomclock? where you can allocate overtime
                break;
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                (response, canPlay) = ValidateActionPlay(card);
                break;
            case GamePhase.DiscardRed:
            case GamePhase.DiscardBlue:
                (response, canPlay) = ValidateDiscardPlay(card);
                break;
        }
        Debug.Log($"Playing {card.front.title} on {hoveredDropLocation.name} - {(canPlay ? "Allowed" : "Rejected")}");
        Debug.Log(response);

        return canPlay;
    }
    private (string, bool) ValidateDiscardPlay(Card card) {
        if (hoveredDropLocation.CompareTag(CardDropZoneTag.DISCARD)) {
            return ("Can discard during discard phase", true);
        }
        return ("Must discard on the discard drop zone", false);
    }
    private (string, bool) ValidateActionPlay(Card card) {
        Debug.Log("Checking if card can be played in action phase");
        if (!GameManager.instance.IsActualPlayersTurn())
            return ($"It is not {playerTeam}'s turn", false);
        if (ReadyState == PlayerReadyState.DiscardCards && GameManager.instance.MIsDiscardAllowed) {
            if (hoveredDropLocation.CompareTag(CardDropZoneTag.DISCARD)) {
                if (CardsAllowedToBeDiscard == null)    //Any card can be discarded
                    return ("Discard any card allowed", true);
                if (CardsAllowedToBeDiscard.Contains(card)) //only highlighted cards can be discarded
                    return ("Allowing discard of valid card", true);
                return ("Must discard one of the highlighted cards", false); //highlighted cards must be discarded
            }
            return ("Must discard cards first", false); //didn't drop on the discard drop zone
        }
        else {
            //check prereq effects on cards
            if (card.data.preReqEffectId != 0) {
                Facility facility = cardDroppedOnObject.GetComponentInParent<Facility>();
                if (!facility.HasEffect(card.data.preReqEffectId)) {
                    return ("Facility effect does not match card prereq effect", false);
                }
            }
            if (!playerSector.TrySpendMeeples(card, ref mMeeplesSpent)) {
                return ("Not enough meeples to play card", false);
            }
        }
        return ("Valid action play", true);
    }

    private (string, bool) CanDiscardCard(Card card) {
        Debug.Log("Checking if card can be discarded");
        //check if it is the player's turn
        if (!GameManager.instance.IsActualPlayersTurn())
            return ($"It is not {playerTeam}'s turn", false);
        //draw phase checks if the player is discarding a card and if they havent discard more than allowed this phase
        if (GameManager.instance.MGamePhase == GamePhase.DrawBlue || GameManager.instance.MGamePhase == GamePhase.DrawRed) {
            if (hoveredDropLocation.CompareTag("DiscardDropLocation") && GameManager.instance.MNumberDiscarded < GameManager.instance.MAX_DISCARDS) {
                return ("", true);
            }
            return ("Cannot discard more than " + GameManager.instance.MAX_DISCARDS + " cards per turn", false);
        }
        //if (GameManager.instance.MIsDiscardAllowed) {
        //    //card effect caused the player to need to discard cards
        //    if (CardsAllowedToBeDiscard == null)    //Any card can be discarded
        //        return ("", true);
        //    if (CardsAllowedToBeDiscard.Contains(card))
        //        return ("", true);
        //    return ("Must discard one of the highlighted cards", false);

        //}
        return ("You do not need to discard cards currently", false);
    }
    public bool IsPlayerTurn() {
        //replace with call to game manager?
        //some code to validate turn order red goes before blue
        return true;
    }


    public void ResetMeepleCost() {
        mMeeplesSpent = 0;
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
            // DiscardFromWhere.MyFacility => ActiveFacilities,
            _ => HandCards,
        };
        foreach (GameObject activeCardObject in discardFromArea.Values) {
            //GameObject activeCardObject = ActiveCardList[i];
            Card card = activeCardObject.GetComponent<Card>();

            if (card.State == CardState.CardNeedsToBeDiscarded) {
                Discards.Add(card.UniqueID, activeCardObject);
                inactives.Add(card.UniqueID);
                card.SetCardState(CardState.CardDiscarded);

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

    public string GetCardNameFromID(int cardID) {
        if (cards.TryGetValue(cardID, out Card card)) {
            return card.data.name;
        }
        return "Card not found";
    }
    public int GetTotalMeeples() {
        return playerSector.GetTotalMeeples();
    }
    public int GetMaxMeeples() {
        return playerSector.GetMaxMeeples();
    }

    private void HandleDiscardDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {
        switch (phase) {
            case GamePhase.DrawBlue:
            case GamePhase.DrawRed:
            case GamePhase.DiscardBlue:
            case GamePhase.DiscardRed:
                // Debug.Log("card dropped in discard zone or needs to be discarded" + card.UniqueID);
                card.SetCardState(CardState.CardNeedsToBeDiscarded);
                playCount = 1;
                break;
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                //discarding here will be done by a card forcing the player to discard a number of cards
                if (ReadyState == PlayerReadyState.DiscardCards) {
                    card.SetCardState(CardState.CardNeedsToBeDiscarded);
                    playCount = 1;
                    //flag discard as done
                    AmountToDiscard--;
                    if (AmountToDiscard <= 0) {//check if we discard enough cards
                        GameManager.instance.DisablePlayerDiscard(this);
                    }
                }
                break;
        }
    }

    private void HandleFacilityDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {

        Facility facility = FacilityPlayedOn();
        Debug.Log($"Handling {card.front.title} played on {facility.facilityName}");
        switch (phase) {
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                // StackCards(facility.gameObject, card.gameObject, playerDropZone, GamePhase.Action); TODO: throwing null ref error?
                card.SetCardState(CardState.CardInPlay);
                ActiveCards.Add(card.UniqueID, card.gameObject);
                // NOTE: TO DO - need to add the correct update for the card played since some of them
                // need different info
                mUpdatesThisPhase.Enqueue(new Update {
                    Type = CardMessageType.CardUpdate,
                    UniqueID = card.UniqueID,
                    CardID = card.data.cardID,
                    FacilityType = facility.facilityType //added facility type to update
                });
                GameManager.instance.SendUpdatesToOpponent(phase, this); //immediately update opponent

                // card.Play(this, opponentPlayer, facility);
                playCount = 1;
                playKey = card.UniqueID;


                // Start the animation

                StartCoroutine(card.AnimateCardToPosition(facility.transform.position, .6f, () => card.Play(this, opponentPlayer, facility)));

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
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                card.SetCardState(CardState.CardInPlay);
                ActiveCards.Add(card.UniqueID, card.gameObject);
                // NOTE TO DO: need to add proper data and message type for the card here
                mUpdatesThisPhase.Enqueue(new Update {
                    Type = CardMessageType.CardUpdate,
                    UniqueID = card.UniqueID,
                    CardID = card.data.cardID
                });
                //GameManager.instance.SendUpdatesToOpponent(phase, this); //dont update opponent yet we need to add more info to the update (maybe)

                //card.Play(this, opponentPlayer, null, card); //TODO: idk if this is right, it passes itself as the "card to be acted on" should this just be null?
                playCount = 1;
                playKey = card.UniqueID;
                StartCoroutine(card.AnimateCardToPosition(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), .6f, () => card.Play(this, opponentPlayer)));

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
                if (card.State == CardState.CardDrawnDropped) {

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
            if (phase == GamePhase.DrawRed || phase == GamePhase.DrawBlue || phase == GamePhase.DiscardBlue || phase == GamePhase.DiscardRed) {
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
        card.SetCardState(CardState.CardDrawn);
        handPositioner.ReturnCardToHand(card);
    }
    public void ForcePlayerReturnCardsToDeck(int amount, Action onCardsReturned) {

        if (amount > 0) {
            AmountToReturnToDeck = amount;
            OnCardsReturnedToDeck = onCardsReturned;
            ReadyState = PlayerReadyState.ReturnCardsToDeck;
            GameManager.instance.DisplayAlertMessage($"Return {AmountToReturnToDeck} cards to the deck\nby dragging them to the play area", this);
        }
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
    #region Stack Cards (old)
    public void StackCards(GameObject stationObject, GameObject addedObject, GameObject dropZone, GamePhase phase) {
        Card stationCard = stationObject.GetComponent<Card>();

        // unhighlight the outline if it's turned on
        //stationCard.OutlineImage.SetActive(false);
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
    #endregion
    //returns any dropped cards to the hand
    public void ClearDropState() {
        if (HandCards.Count != 0) {
            foreach (GameObject cardGameObject in HandCards.Values) {
                Card card = cardGameObject.GetComponent<Card>();
                if (card.State == CardState.CardDrawnDropped) {
                    card.SetCardState(CardState.CardDrawn);
                }
            }
        }
    }

    //called by the game manager to add an update to the player's queue from the opponent's actions
    public void AddUpdateFromOpponent(Update update, GamePhase phase, CardPlayer opponent) {
        switch (update.Type) {
            case CardMessageType.DrawCard:
                Debug.Log($"{playerName} received card draw from {opponent.playerName} who drew {GetCardNameFromID(update.CardID)} with uid {update.UniqueID}");
                opponent.DrawSpecificCard(update.CardID, opponentDropZone, updateNetwork: false); //draw cards for opponent but dont update network which would cause an infinite loop
                break;
            default: //card update for now, maybe discard?
                Debug.Log($"Player {playerName} is adding card update: {update.Type}, FacilityType: {update.FacilityType}");

                if (IsAnimating) {
                    Debug.Log($"Queueing card update due to ongoing animation: {update.Type}, Facility: {update.FacilityType}");
                    opponentCardPlays.Enqueue((update, phase, opponent));
                    return;
                }

                // If no animation is in progress, handle the card play immediately
                ProcessCardPlay(update, phase, opponent);
                break;

        }
    }
    void ProcessCardPlay(Update update, GamePhase phase, CardPlayer opponent) {
        IsAnimating = true;
        if (update.Type == CardMessageType.CardUpdate) {
            //handle facility card play
            if (update.FacilityType != FacilityType.None) {
                HandleFacilityOpponentPlay(update, phase, opponent);
            }
            //handle non facility card
            else if (update.FacilityType == FacilityType.None) {
                HandleFreeOpponentPlay(update, phase, opponent);
            }
            else {
                Debug.LogError($"Failed to find facility of type: {update.FacilityType}");
            }
        }
        else {
            Debug.Log($"Unhandled update type or facility: {update.Type}, {update.FacilityType}");
        }
    }
    private void HandleOpponentCardPlay(Card card, GameObject dropZone, CardPlayer opponent, Facility facility = null) {
        if (card != null) {
            GameObject cardGameObject = ActiveCards[card.UniqueID];
            RectTransform cardRect = cardGameObject.GetComponent<RectTransform>();

            // Set the card's parent to nothing, in order to position it in world space
            cardRect.SetParent(null, true);
            Vector2 topMiddle = new Vector2(Screen.width / 2, Screen.height + cardRect.rect.height / 2); // top middle just off the screen
            cardRect.anchoredPosition = topMiddle;
            card.transform.localRotation = Quaternion.Euler(0, 0, 180); // flip upside down as if played by opponent
            cardRect.SetParent(GameManager.instance.gameCanvas.transform, true); // set parent to game canvas and keep world position
            cardGameObject.SetActive(true);

            // Start the card animation
            StartCoroutine(card.MoveAndRotateToCenter(cardRect, dropZone, () => {
                card.SetCardState(CardState.CardInPlay);
                card.Play(opponent, this, facility);
                // After the current animation is done, check if there's another card queued
                OnAnimationComplete();
            }));
        }
    }

    void HandleFreeOpponentPlay(Update update, GamePhase phase, CardPlayer opponent) {
        Card card = DrawCard(random: false, cardId: update.CardID, uniqueId: -1,
            deckToDrawFrom: ref DeckIDs, dropZone: null,
            allowSlippy: false, activeDeck: ref ActiveCards);

        HandleOpponentCardPlay(card, null, opponent);
    }

    void HandleFacilityOpponentPlay(Update update, GamePhase phase, CardPlayer opponent) {
        Dictionary<int, GameObject> facilityList = ActiveFacilities.Count > 0 ? ActiveFacilities : opponent.ActiveFacilities;
        if (facilityList.TryGetValue((int)update.FacilityType, out GameObject facilityGo) && facilityGo.TryGetComponent(out Facility facility)) {
            Debug.Log($"Creating card played on facility: {facility.facilityName}");
            Card card = DrawCard(random: false, cardId: update.CardID, uniqueId: -1,
                deckToDrawFrom: ref DeckIDs, dropZone: facilityGo,
                allowSlippy: false, activeDeck: ref ActiveCards);

            HandleOpponentCardPlay(card, facilityGo, opponent, facility);
        }
    }
    //called when the card animation is complete to start the next animation
    private void OnAnimationComplete() {
        IsAnimating = false;

        // Check if there are more cards in the queue
        if (opponentCardPlays.Count > 0) {
            var nextCardPlay = opponentCardPlays.Dequeue();
            ProcessCardPlay(nextCardPlay.Item1, nextCardPlay.Item2, nextCardPlay.Item3);
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
    // a. phase it happened in
    // b. unique card id for a specific player
    // c. id of the card played
    // d. other unique info for special cards
    public CardMessageType GetNextUpdateInMessageFormat(ref List<int> playsForMessage, GamePhase phase) {
        if (mUpdatesThisPhase.Count > 0) {
            playsForMessage.Add((int)phase);
            Update update = mUpdatesThisPhase.Dequeue();
            //public CardMessageType Type;int CardID;int UniqueID;int Amount;FacilityType FacilityType;FacilityEffectType Effect;
            Debug.Log($"type:{update.Type}|card id:{update.CardID}|unique id:{update.UniqueID}|amount:{update.Amount}|facility type:{update.FacilityType}|effect target:{update.EffectTarget}");
            playsForMessage.Add(update.UniqueID);
            playsForMessage.Add(update.CardID);
            playsForMessage.Add((int)update.FacilityType);


            if (update.Type == CardMessageType.ReduceCost) {
                playsForMessage.Add(update.Amount);
            }
            else if (update.Type == CardMessageType.RemoveEffect) {
                playsForMessage.Add((int)update.FacilityType);
                playsForMessage.Add((int)update.EffectTarget);
            }
            else if (update.Type == CardMessageType.RestorePoints) {
                playsForMessage.Add(update.Amount);
                playsForMessage.Add((int)update.FacilityType);
            }
            else if (update.Type == CardMessageType.MeepleShare) {
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

    public void LogPlayerInfo() {
        string s = $"Player info for: {playerName}\n";

        // Handle Hand Cards
        s += $"Hand size: {HandCards.Count}\n";
        var handCardGroups = HandCards.Values
            .Select(x => x.GetComponent<Card>().front.title)
            .GroupBy(x => x)
            .Select(g => new { Title = g.Key, Count = g.Count() })
            .OrderBy(x => x.Title);

        foreach (var cardGroup in handCardGroups) {
            s += $"\t{cardGroup.Title}{(cardGroup.Count > 1 ? $" x{cardGroup.Count}" : "")}\n";
        }

        // Handle Deck Cards
        s += $"Deck Size: {DeckIDs.Count}\n";
        var deckCardGroups = DeckIDs
            .Select(cardId => GetCardNameFromID(cardId))
            .GroupBy(x => x)
            .Select(g => new { Title = g.Key, Count = g.Count() })
            .OrderBy(x => x.Title);

        foreach (var cardGroup in deckCardGroups) {
            s += $"\t{cardGroup.Title}{(cardGroup.Count > 1 ? $" x{cardGroup.Count}" : "")}\n";
        }

        s += $"Active Facilities: {ActiveFacilities.Count}";
        Debug.Log(s);
    }
}
