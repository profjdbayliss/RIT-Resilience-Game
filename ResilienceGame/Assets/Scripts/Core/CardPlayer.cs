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
#region enums
// Enum to track player type
public enum PlayerTeam {
    Red,
    Blue,
    White,
    Any,
    None
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
    public FacilityType FacilityPlayedOnType;
    public int FacilityEffectUID;
    public FacilityType AdditionalFacilitySelectedOne;
    public FacilityType AdditionalFacilitySelectedTwo;
    public FacilityType AdditionalFacilitySelectedThree;
};

public enum DiscardFromWhere {
    // TODO: Needs ref to others se
    Hand,
    MyPlayZone,
    MyFacility
};
#endregion

public class CardPlayer : MonoBehaviour {
    #region Fields
    [Header("Player Information")]
    public string playerName;
    public PlayerTeam playerTeam = PlayerTeam.Any;
    public Sector PlayerSector { get; private set; }
    public string DeckName = "";

    [Header("Game References")]
    public GameManager manager;

    [Header("Card Collections")]
    public static Dictionary<int, Card> cards = new Dictionary<int, Card>();
    public List<int> FacilityIDs = new List<int>(10);
    public List<int> DeckIDs = new List<int>(52);
    public Dictionary<int, GameObject> HandCards = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> Discards = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> ActiveCards = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> ActiveFacilities = new Dictionary<int, GameObject>();

    [Header("Card Limits")]
    public int handSize;
    private const int MAX_DRAW_AMOUNT = 5;
    public const int MAX_HAND_SIZE_AFTER_ACTION = 7;

    [Header("Prefabs and UI Elements")]
    public GameObject cardPrefab;
    public GameObject discardDropZone;
    public GameObject handDropZone;
    public GameObject opponentDropZone;
    public GameObject cardStackingCanvas;

    [Header("Card Positioning")]
    public readonly float ORIGINAL_SCALE = 0.2f;
    private HandPositioner handPositioner;

    [Header("Drag and Drop")]
    public GameObject hoveredDropLocation;
    private GameObject previousHoveredFacility;
    private GameObject cardDroppedOnObject;
    public Dictionary<string, GameObject> cardDropLocations = new Dictionary<string, GameObject>();
    public bool IsDraggingCard { get; private set; } = false;

    [Header("Game State")]
    public Queue<(Update, GamePhase, CardPlayer)> opponentCardPlays = new Queue<(Update, GamePhase, CardPlayer)>();
    public bool IsAnimating { get; set; } = false;
    public PlayerReadyState ReadyState { get; set; } = PlayerReadyState.ReadyToPlay;
    public int AmountToDiscard { get; set; } = 0;
    public int AmountToReturnToDeck { get; private set; } = 0;
    public List<Card> CardsAllowedToBeDiscard;
    public Action OnCardsReturnedToDeck { get; set; }
    public Action<List<Facility>> OnFacilitiesSelected { get; set; }

    [Header("Facilities")]
    private int facilityCount = 0;
    private bool registeredFacilities = false;

    [Header("Meeples")]
    private int mMeeplesSpent = 0;

    [Header("Scoring")]
    private int mFinalScore = 0;

    // Private fields
    //private static int sUniqueIDCount = 0;
    private Queue<Update> mUpdatesThisPhase = new Queue<Update>(6);

    // Enum definition
    public enum PlayerReadyState {
        ReadyToPlay,
        ReturnCardsToDeck,
        DiscardCards,
        SelectFacilties,
        SelectMeeplesWithUI,
        SelectCardsForCostChange
    }
    #endregion

    #region Initialization
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
        foreach (Facility facility in PlayerSector.facilities) {
            ActiveFacilities.Add((int)facility.facilityType, facility.gameObject);
            FacilityIDs.Add((int)facility.facilityType);
        }
    }
    public static void AddCards(List<Card> cardList) {
        foreach (Card card in cardList) {
            cards.Add(card.data.cardID, card);
        }
    }
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
    #endregion

    #region Card Action Functions

    public void ForcePlayerSelectFacilities(int numFacilitiesToSelect, Action<List<Facility>> onFacilitySelect) {
        if (numFacilitiesToSelect <= 0) return;
        ReadyState = PlayerReadyState.SelectFacilties;
        Debug.Log($"Forcing {playerName} to select {numFacilitiesToSelect} facilities before continuing");
        PlayerSector.EnableFacilitySelection(numFacilitiesToSelect);
        OnFacilitiesSelected = onFacilitySelect;
    }
    public void ResolveFacilitySelection() {
        ReadyState = PlayerReadyState.ReadyToPlay;
        
        var facilities = PlayerSector.GetSelectedFacilities(); //also doesn't work if we move sectors to the blue players 
        if (facilities == null) {
            Debug.LogError("selected facility list is null");
            return;
        }
        PlayerSector.DisableFacilitySelection();
        OnFacilitiesSelected?.Invoke(facilities);
    }
    public void ChooseMeeplesThenReduceCardCost(int amountOfMeeplesNeeded) {
        ChooseMeeples(amountOfMeeplesNeeded);
        //TODO: move to an update loop checking ReadyState
        SelectCardsInHand();
        SelectMeeplesOnCards();
    }
    private void ChooseMeeples(int amountOfMeeplesNeeded) {
        ReadyState = PlayerReadyState.SelectMeeplesWithUI;
        PlayerSector.ForcePlayerToChoseMeeples(amountOfMeeplesNeeded, () => ReadyState = PlayerReadyState.SelectCardsForCostChange);//example
    }
    private void SelectCardsInHand() {

    }
    private void SelectMeeplesOnCards() {

    }
    //Sets the variables required to force the player to discard a certain amout of cards
    public void AddDiscardEvent(int amount, List<Card> cardsAllowedToBeDiscard = null) {
        ReadyState = PlayerReadyState.DiscardCards;         //set the player state to discard cards
        AmountToDiscard = amount;                           //set the amount of cards to discard
        discardDropZone.SetActive(true);                    //enable the discard drop zone
        CardsAllowedToBeDiscard = cardsAllowedToBeDiscard;  //holds the cards that are allowed to be discarded (like draw 3 discard 1 of them)
        Debug.Log($"Enabling {playerName}'s discard temporarily");
    }
    //Returns the player to ready to play state by disabling necessary ui elements and setting player state
    public void StopDiscard() {
        ReadyState = PlayerReadyState.ReadyToPlay;
        CardsAllowedToBeDiscard?.ForEach(card => card.ToggleOutline(false));    //turn off the outline on the cards that were allowed to be discarded
        CardsAllowedToBeDiscard = null;             //dispose of the list
        discardDropZone.SetActive(false);           //disable the discard drop zone
        GameManager.instance.mAlertPanel.ResolveTextAlert();    //hide alert panel
        Debug.Log($"Disabling {playerName}'s discard");
    }
    //returns the card from the hand to the deck
    private void ReturnCardToDeck(Card card) {
        //TODO: need to inform the opponent that a card was returned to the deck
        handPositioner.cards.Remove(card);
        DeckIDs.Add(card.data.cardID);//add it back to the deck
        HandCards.Remove(card.UniqueID);
        Destroy(card.gameObject);
    }
    //Called by a card action to return the entire hand to the deck and draw a new hand
    public void ReturnHandToDeckAndDraw(int amount) {
        //TODO: update opponent that this happened
        //the size of the deck will update correctly, but the deck IDs will not be updated (i think)
        HandCards.Values.ToList().ForEach(card => {
            ReturnCardToDeck(card.GetComponent<Card>());
        });
        DrawNumberOfCards(amount, updateNetwork: true);
    }
    //Called by a card action to force the player to return cards to the deck by dragging them to the play area
    public void ForcePlayerReturnCardsToDeck(int amount, Action onCardsReturned) {

        if (amount > 0) {
            AmountToReturnToDeck = amount;
            OnCardsReturnedToDeck = onCardsReturned;
            ReadyState = PlayerReadyState.ReturnCardsToDeck;
            GameManager.instance.DisplayAlertMessage($"Return {AmountToReturnToDeck} cards to the deck\nby dragging them to the play area", this);
        }
    }
    #endregion

    #region Helpers
    public void ResetMeepleCount() {
        if (PlayerSector != null)
            PlayerSector.ResetMeepleCount();
    }
    //TODO: update for more than 2 players
    public Sector GetActiveSector() {
        return PlayerSector == null ? GameManager.instance.opponentPlayer.PlayerSector : PlayerSector;
    }
    public void AssignSector(Sector sector) {
        PlayerSector = sector;
    }
    public PlayerTeam GetOpponentTeam() {
        return playerTeam switch {
            PlayerTeam.Red => PlayerTeam.Blue,
            PlayerTeam.Blue => PlayerTeam.Red,
            _ => PlayerTeam.Any
        };
    }
    public void InformSectorOfNewTurn() {
        if (PlayerSector != null)
            PlayerSector.InformFacilitiesOfNewTurn();
        else {
            Debug.Log($"{playerName}'s sector is null");
        }
    }

    //reset card state to in card drawn and return to the hand positioner by setting parent to hand drop zone
    public void ResetCardToInHand(Card card) {
        card.SetCardState(CardState.CardDrawn);
        handPositioner.ReturnCardToHand(card);
    }
    //returns true if the player's cards are above the max hand size at the end of the action phase to force them to discard cards
    public bool NeedsToDiscard() {
        return HandCards.Count > MAX_HAND_SIZE_AFTER_ACTION;
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
        return PlayerSector.GetTotalMeeples();
    }
    public int GetMaxMeeples() {
        return PlayerSector.GetMaxMeeples();
    }
    private Facility FacilityPlayedOn() {
        Facility facility = null;
        if (cardDroppedOnObject != null) {
            facility = cardDroppedOnObject.GetComponentInParent<Facility>();
        }
        return facility;
    }
    private void OnAnimationComplete() {
        Debug.Log("animation complete");
        IsAnimating = false;

        // Check if there are more cards in the queue
        if (opponentCardPlays.Count > 0) {
            var nextCardPlay = opponentCardPlays.Dequeue();
            Debug.Log($"Playing next card update in queue: {nextCardPlay.Item1.Type}");
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


    #endregion

    #region Card Drawing Functions
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
    //Draws a specific card from the deck and adds it to the handParent by calling the CardDraw function
    //Currently used to add a card to opponents hand when receiving a draw message from the network
    public virtual void DrawSpecificCard(int id, GameObject handParent, int uid = -1, bool updateNetwork = false) {
        Debug.Log($"[{(GameManager.instance.IsServer ? "SERVER" : "CLIENT")}]'s player {playerName} is trying to draw {GetCardNameFromID(id)} with uid {uid}");
        if (DeckIDs.Count > 0) {
            DrawCard(false, id, uid, ref DeckIDs, handParent, true, ref HandCards, updateNetwork);
        }
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
                Debug.Log("didn't find a card of this type to draw : " + cardId + $" in {(deckToDrawFrom == DeckIDs ? $"{DeckName} deck" : $"deck with size {deckToDrawFrom.Count}")}");
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
            GameManager.instance.UniqueCardIdCount++;
            //Debug.Log("setting unique id for card " + uniqueId);
        }
        else {
            // since there are multiples of each card type potentially
            // in a deck they need a unique id outside of the card's id
            tempCard.UniqueID = GameManager.instance.UniqueCardIdCount++;
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
            EnqueueCardMessageUpdate(CardMessageType.DrawCard, tempCard.data.cardID, tempCard.UniqueID);
            GameManager.instance.SendUpdatesToOpponent(GameManager.instance.MGamePhase, this);
        }

        GameManager.instance.UpdateUISizeTrackers(); //update UI




        return tempCard;
    }

    #endregion

    #region Debug
    /*
     * "modp;net;1",
"modp;phys;1",
"modp;fin;1",
"modp;all;1",
"modp;fin&net;1",
"modp;phys&net;1",
"fortify",
"backdoor",
"modp;net;-1",
"modp;phys;-1",
"modp;fin;-1",
"modp;all;-1",
"modp;phys&net;-1",
"modp;phys&fin;-1",
"modp;fin&net;-1",
     */
    void HandleDebugInput() {
        //force add backdoor or fortify to hovered facility
        if (GameManager.instance.actualPlayer == this) {
            if (Keyboard.current.digit9Key.wasPressedThisFrame) {
                if (TryGetFacilityUnderMouse(out Facility facility)) {
                    facility.AddRemoveEffectsByIdString("backdoor", true, PlayerTeam.Red);
                }

            }
            else if (Keyboard.current.digit0Key.wasPressedThisFrame) {
                if (TryGetFacilityUnderMouse(out Facility facility)) {
                    facility.AddRemoveEffectsByIdString("fortify", true, PlayerTeam.Blue);
                }
            }

        }
        //HandleDebugEffectCreation();
        if (Keyboard.current.backquoteKey.wasPressedThisFrame) {
            HandleMenuToggle();
        }
        //print info about clicked facility
        if (Mouse.current.rightButton.wasReleasedThisFrame) {
            TryLogFacilityInfo();
        }
        //show effect selection menu
        else if (Mouse.current.middleButton.wasReleasedThisFrame) {
            //TryShowEffectSelectionMenu(); //not neede
        }
        //log all of the facilities in the sector
        if (Keyboard.current.f3Key.wasPressedThisFrame) {
            if (GameManager.instance.actualPlayer == this) {
                if (PlayerSector != null) {
                    Debug.Log($"Facility info for player {playerName}");
                    foreach (Facility facility in PlayerSector.facilities) {
                        facility.LogFacilityDebug();
                    }
                }
                else {
                    Debug.Log($"Player {playerName} does not have an assigned sector");
                }
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
    void HandleDebugEffectCreation() {

        if (PlayerSector == null || PlayerSector.facilities == null || PlayerSector.facilities.Length == 0) {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;net;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;phys;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;fin;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
        }
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;fin&phys;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
        }
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;fin&net;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
        }
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;phys&net;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
        }
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) {
            PlayerSector.facilities[0].DebugAddSpecificEffect($"modp;all;{(Keyboard.current.shiftKey.isPressed ? "-1" : "1")}");
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
        card.GetComponent<Card>().SetCardState(CardState.CardNeedsToBeDiscarded);
        card.transform.SetParent(discardDropZone.transform, false);
        card.transform.localPosition = new Vector3();
    }
    #endregion

    #region Update Functions
    // Update is called once per frame
    void Update() {
        IsDraggingCard = handPositioner.IsDraggingCard;

        //init once sector is ready
        if (!registeredFacilities) {
            if (PlayerSector != null) {
                if (PlayerSector.facilities != null && PlayerSector.facilities.Length > 0)
                    RegisterFacilities();
            }
        }

        if (IsDraggingCard) {
            UpdateHoveredDropLocation();
        }

        //wait and check for the proper amount of facilities selected
        if (ReadyState == PlayerReadyState.SelectFacilties) {
            if (PlayerSector.HasSelectedFacilities()) {

                ResolveFacilitySelection();
            }
        }


        if (GameManager.instance.DEBUG_ENABLED) {
            HandleDebugInput();
        }
    }
    

    //updates the hoverDropLocation class field to hold the object the card is hovering over
    void UpdateHoveredDropLocation() {
        GameObject currentHoveredFacility = null; // Reset at the beginning of each update
        bool isOverAnyDropLocation = false;

        //only highlight when the player is ready to play cards
        if (ReadyState != PlayerReadyState.ReadyToPlay) {
            return;
        }

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

            bool highlight = false;

            // Process the hovered facility collider
            if (hoveredFacilityCollider != null) {
                var cardBeingDragged = handPositioner.CardsBeingDragged.First();
                // Debug.Log(cardDraggedTarget);
                // Check if the card being dragged is a facility card
                if (cardBeingDragged.target == CardTarget.Facility || cardBeingDragged.target == CardTarget.Effect) {
                    if (GameManager.instance.CanHighlight()) {
                        //effect card or facility with pre req effect hover
                        if (cardBeingDragged.target == CardTarget.Effect || cardBeingDragged.data.preReqEffectType != FacilityEffectType.None) {
                            if (hoveredFacilityCollider.TryGetComponent(out Facility facility)) {
                                //Debug.Log($"Hovering facility {facility.facilityName} while holding effect card");
                                //removable effects are the only ones to check for the prereq effects
                                if (facility.HasRemovableEffects(GetOpponentTeam())) {
                                    highlight = true;
                                }
                            }
                        }
                        //facility card hover
                        else {
                            highlight = true;
                        }
                    }
                }
                if (highlight) {
                    if (hoveredFacilityCollider.TryGetComponent(out HoverActivateObject hoverActivateObject)) {
                        //    Debug.Log("Hovering");
                        hoverActivateObject.ActivateHover();
                        currentHoveredFacility = hoveredFacilityCollider.gameObject; // Assign currentHoveredFacility
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

    #region Card Play Functions

    #region Play Update Loop
    //original PlayCard function that is called from the update loop
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
                    GameManager.instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
                    break;
                }
            }
        }

        if (playCount > 0) {
            if (phase == GamePhase.DrawRed || phase == GamePhase.DrawBlue || phase == GamePhase.DiscardBlue || phase == GamePhase.DiscardRed || AmountToDiscard > 0) {
                // we're not discarding a facility or sharing what we're discarding with the opponent
                DiscardAllInactiveCards(DiscardFromWhere.Hand, false, -1);
            }
            else {
                // remove the discarded card
                if (!HandCards.Remove(playKey)) {
                    Debug.Log("didn't find a key to remove! " + playKey);
                }
                else {
                    Debug.Log("removed key " + playKey + " and updating tracker UI");
                    GameManager.instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
                }
            }
        }

        return playCount;
    }
    #endregion

    #region Dropping
    //This function is called when a card is dropped from that card's slippy component (happens one time at drop)
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
                    //TODO: this is not correct??
                    //update the card update with the card that was returned
                    //if (mUpdatesThisPhase.TryPeek(out Update update)) {
                    //    if (update.Type == CardMessageType.CardUpdate) {
                    //        if (update.DiscardedOrReturnedCardUIDs == null || update.DiscardedOrReturnedCardUIDs == "") {
                    //            update.DiscardedOrReturnedCardUIDs = card.UniqueID.ToString();
                    //        }
                    //        else {
                    //            update.DiscardedOrReturnedCardUIDs += ";" + card.UniqueID;
                    //        }
                    //    }
                    //}
                    AmountToReturnToDeck--;
                    if (AmountToReturnToDeck > 0) {
                        GameManager.instance.DisplayAlertMessage($"Return {AmountToReturnToDeck} more cards to the deck", this); //update alert message
                    }
                    else {
                        OnCardsReturnedToDeck?.Invoke(); //Resolve the action after cards have been returned to deck
                        GameManager.instance.mAlertPanel.ResolveTextAlert(); //remove alert message
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
    //Called when a card is dropped onto the discard drop area
    private void HandleDiscardDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {
        bool discard = false;
        switch (phase) {
            case GamePhase.DrawBlue:
            case GamePhase.DrawRed:
            case GamePhase.DiscardBlue:
            case GamePhase.DiscardRed:
                // Debug.Log("card dropped in discard zone or needs to be discarded" + card.UniqueID);
                playKey = card.UniqueID;
                card.SetCardState(CardState.CardNeedsToBeDiscarded);
                playCount = 1;
                discard = true;
                break;
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                //discarding here will be done by a card forcing the player to discard a number of cards
                if (ReadyState == PlayerReadyState.DiscardCards) {
                    card.SetCardState(CardState.CardNeedsToBeDiscarded);
                    playKey = card.UniqueID;
                    playCount = 1;
                    //flag discard as done
                    AmountToDiscard--;
                    if (AmountToDiscard <= 0) {//check if we discard enough cards
                        GameManager.instance.DisablePlayerDiscard(this);
                    }
                    discard = true;
                }
                break;
        }
        if (discard) {
            Debug.Log($"Adding discard update from {playerName} who discarded {card.data.name} with uid {card.UniqueID}");
            EnqueueAndSendCardMessageUpdate(CardMessageType.DiscardCard, card.data.cardID, card.UniqueID);
            card.gameObject.SetActive(false);
            GameManager.instance.UpdateUISizeTrackers();

        }
    }
    //Called when a Facility/Effect target card is dropped in the play area
    private void HandleFacilityDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {

        Facility facility = FacilityPlayedOn();
        Debug.Log($"Handling {card.front.title} played on {facility.facilityName}");
        switch (phase) {
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                // StackCards(facility.gameObject, card.gameObject, playerDropZone, GamePhase.Action); TODO: throwing null ref error?
                card.SetCardState(CardState.CardInPlay);
                ActiveCards.Add(card.UniqueID, card.gameObject);
                EnqueueAndSendCardMessageUpdate(CardMessageType.CardUpdate, card.data.cardID, card.UniqueID, facilityType: facility.facilityType); //send the update to the opponent

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
    //Called when a non-Facility/Effect target card is dropped in the play area
    private void HandleFreePlayDrop(Card card, GamePhase phase, CardPlayer opponentPlayer, ref int playCount, ref int playKey) {
        Debug.Log($"Handling non facility card - {card.front.title}");
        Facility facility = FacilityPlayedOn(); //still need this for sector cards
        switch (phase) {
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                card.SetCardState(CardState.CardInPlay);
                ActiveCards.Add(card.UniqueID, card.gameObject);

                EnqueueCardMessageUpdate(CardMessageType.CardUpdate, card.data.cardID, card.UniqueID);
                playCount = 1;
                playKey = card.UniqueID;
                StartCoroutine(card.AnimateCardToPosition(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), .6f, () => card.Play(this, opponentPlayer, facility)));

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
    #endregion

    #region Card Play Validation
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
        if (ReadyState == PlayerReadyState.SelectFacilties) {
            return ($"Player must select facilities before playing cards", false);
        }
        //handle player having to discard cards during action phase
        //quitting out of the function early with true here is fine since these cards arent actually getting played/meeples spent
        else if (ReadyState == PlayerReadyState.DiscardCards && GameManager.instance.MIsDiscardAllowed) {
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
            //check prereq effects on cards for effect cards played on single facilities
            if (card.data.preReqEffectType != FacilityEffectType.None) {
                Facility facility = hoveredDropLocation.GetComponent<Facility>();
                if (!facility.HasEffectOfType(card.data.preReqEffectType)) {
                    return ("Facility effect does not match card prereq effect", false);
                }
            }
            //check for 'Remove' effect for sector cards
            if (card.data.effectString == "Remove") {
                Facility facility = hoveredDropLocation.GetComponent<Facility>();
                Sector sector = facility.sectorItsAPartOf;
                if (!sector.HasRemovableEffectsOnFacilities(playerTeam)) {
                    return ("Sector does not have removable effects", false);
                }
            }
            if (!PlayerSector.TrySpendMeeples(card, ref mMeeplesSpent)) {
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
    #endregion

    #endregion

    #region Discarding
    //attempts to discard the card with the given UID
    public bool TryDiscardFromHandByUID(int uid) {
        if (HandCards.TryGetValue(uid, out GameObject cardGameObject)) {
            if (cardGameObject.TryGetComponent(out Card card)) {
                card.SetCardState(CardState.CardNeedsToBeDiscarded);
                Discards.Add(uid, cardGameObject);
                cardGameObject.transform.SetParent(discardDropZone.transform, false);
                cardGameObject.transform.localPosition = new Vector3();
                cardGameObject.SetActive(false);
                HandCards.Remove(uid);
                GameManager.instance.UpdateUISizeTrackers();
                return true;
            }
        }
        return false;
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
                activeCardObject.transform.SetParent(discardDropZone.transform, false);
                activeCardObject.transform.localPosition = new Vector3();
                activeCardObject.transform.localScale = new Vector3(1, 1, 1);

                // for the future might want to stack cards in the discard zone
                // Debug.Log("setting card to discard zone: " + card.UniqueID + " with name " + card.front.title);
                activeCardObject.SetActive(false);
                card.cardZone = discardDropZone;
                if (addUpdate) {
                    // Debug.Log($"adding discard update from {playerName} to {GameManager.instance.opponentPlayer.playerName}");
                    EnqueueAndSendCardMessageUpdate(CardMessageType.DiscardCard, card.data.cardID, card.UniqueID);
                }
            }
        }
        foreach (int key in inactives) {
            //  Debug.Log("key being discarded is " + key);
            if (!discardFromArea.Remove(key)) {
                // Debug.Log("card not removed where it supposedly was from: " + key);
            }
        }
        GameManager.instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
    }
    #endregion

    #region Network Updates

    private void EnqueueCardMessageUpdate(CardMessageType cardMessageType, int CardID, int UniqueID, int Amount = -1, FacilityType facilityType = FacilityType.None, int facilityEffectUID = -1, bool sendUpdateImmediately = false) {
        mUpdatesThisPhase.Enqueue(new Update {
            Type = cardMessageType,
            UniqueID = UniqueID,
            CardID = CardID,
            Amount = Amount,
            FacilityPlayedOnType = facilityType,
            FacilityEffectUID = facilityEffectUID
        });
        if (sendUpdateImmediately) {
            GameManager.instance.SendUpdatesToOpponent(GameManager.instance.MGamePhase, this);
        }
    }
    private void EnqueueAndSendCardMessageUpdate(CardMessageType cardMessageType, int CardID, int UniqueID, int Amount = -1, FacilityType facilityType = FacilityType.None, int facilityEffectUID = -1) {
        EnqueueCardMessageUpdate(cardMessageType, CardID, UniqueID, Amount, facilityType, facilityEffectUID, true);
    }


    //called by the game manager to add an update to the player's queue from the opponent's actions
    public void AddUpdateFromOpponent(Update update, GamePhase phase, CardPlayer opponent) {
        /*
            CardUpdate,
            RemoveEffect,
            DiscardCard,
            MeepleShare,
            DrawCard
         */
        switch (update.Type) {
            case CardMessageType.DrawCard:
                Debug.Log($"{playerName} received card draw from {opponent.playerName} who drew {GetCardNameFromID(update.CardID)} with uid {update.UniqueID}");
                opponent.DrawSpecificCard(update.CardID, opponentDropZone, uid: update.UniqueID, updateNetwork: false); //draw cards for opponent but dont update network which would cause an infinite loop
                break;
            case CardMessageType.CardUpdate:
            case CardMessageType.DiscardCard:
            case CardMessageType.ReduceCost:
                if (IsAnimating) {
                    // Debug.Log($"Queueing card update due to ongoing animation: {update.Type}, Facility: {update.FacilityType}");
                    opponentCardPlays.Enqueue((update, phase, opponent));
                    return;
                }
                // If no animation is in progress, handle the card play immediately
                ProcessCardPlay(update, phase, opponent);
                break;
            case CardMessageType.RemoveEffect:
                Debug.Log($"{playerName} received remove effect from {opponent.playerName}");
                HandleRemoveEffectUpdate(update, opponent);
                break;
        }
    }
    void HandleRemoveEffectUpdate(Update update, CardPlayer opponent) {
        if (update.FacilityPlayedOnType != FacilityType.None) {
            if (ActiveFacilities.TryGetValue((int)update.FacilityPlayedOnType, out GameObject facilityGo)) {
                if (facilityGo.TryGetComponent(out Facility facility)) {
                    Debug.Log($"Looking to remove effect with uid {update.FacilityEffectUID} from {facility.facilityName}");

                }
            }
        }
    }
    //Actually process the card action, used to create the update queue and is called when the update is ready to be resolved
    void ProcessCardPlay(Update update, GamePhase phase, CardPlayer opponent) {
        Debug.Log($"Player {playerName} is processing a card update from {opponent.playerName} of type {update.Type}");

        if (update.Type == CardMessageType.CardUpdate) {
            IsAnimating = true;
            //handle facility card play
            if (update.FacilityPlayedOnType != FacilityType.None) {
                HandleFacilityOpponentPlay(update, phase, opponent);
            }
            //handle non facility card
            else if (update.FacilityPlayedOnType == FacilityType.None) {

                HandleFreeOpponentPlay(update, phase, opponent);
            }
            else {
                Debug.LogError($"Failed to find facility of type: {update.FacilityPlayedOnType}");
            }
        }
        else if (update.Type == CardMessageType.DiscardCard) {
            if (opponent.TryDiscardFromHandByUID(update.UniqueID)) {
                Debug.Log($"Successfully removed card with uid {update.UniqueID} from {opponent.playerName}'s hand");
            }
            else {
                Debug.LogError($"Did not find card with uid {update.UniqueID} in {opponent.playerName}'s hand!!");
            }
        }
        else {
            Debug.Log($"Unhandled update type or facility: {update.Type}, {update.FacilityPlayedOnType}");
        }
    }
    //handles when the shared logic of opponent card plays
    private void HandleOpponentCardPlay(Card card, GameObject dropZone, CardPlayer opponent, Facility facility = null, bool callPlay = true) {
        Debug.Log($"Handling {opponent.playerName}'s card play of {card.data.name}");
        if (card != null) {
            if (opponent.HandCards.TryGetValue(card.UniqueID, out GameObject cardGameObject)) {
                RectTransform cardRect = cardGameObject.GetComponent<RectTransform>();

                // Set the card's parent to nothing, in order to position it in world space
                cardRect.SetParent(null, true);
                Vector2 topMiddle = new Vector2(Screen.width / 2, Screen.height + cardRect.rect.height / 2); // top middle just off the screen
                cardRect.anchoredPosition = topMiddle;
                card.transform.localRotation = Quaternion.Euler(0, 0, 180); // flip upside down as if played by opponent
                cardRect.SetParent(GameManager.instance.gameCanvas.transform, true); // set parent to game canvas and keep world position
                cardGameObject.SetActive(true);
                //Debug.Log($"Added card to screen, starting animation");
                // Start the card animation
                StartCoroutine(card.MoveAndRotateToCenter(cardRect, dropZone, () => {
                    card.SetCardState(CardState.CardInPlay);
                    opponent.HandCards.Remove(card.UniqueID); //remove the card from the opponent's hand
                    if (callPlay)
                        card.Play(player: opponent, opponent: this, facilityActedUpon: facility);
                    GameManager.instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
                    // After the current animation is done, check if there's another card queued
                    OnAnimationComplete();
                }));
            }
            else {
                Debug.Log($"Card with uid {card.UniqueID} was not found in {opponent.playerName}'s Hand which has size {opponent.HandCards.Count}");
            }


        }
    }
    //handles when the opponent plays a non facility/effect card
    void HandleFreeOpponentPlay(Update update, GamePhase phase, CardPlayer opponent) {
        //Card card = DrawCard(random: false, cardId: update.CardID, uniqueId: -1,
        //    deckToDrawFrom: ref opponent.DeckIDs, dropZone: null,
        //    allowSlippy: false, activeDeck: ref ActiveCards);
        if (opponent.HandCards.TryGetValue(update.UniqueID, out GameObject cardObject)) {
            Card tempCard = cardObject.GetComponent<Card>();
            Debug.Log($"Found {tempCard.data.name} with uid {tempCard.UniqueID} in {opponent.playerTeam}'s hand");
            HandleOpponentCardPlay(tempCard, null, opponent, callPlay: false);//dont actually call the play function of the card once its been passed in, the draw/discard messages are already sent elsewhere
        }
        else {
            Debug.LogError($"{playerName} was looking for card with uid {update.UniqueID} but did not find it in {opponent.playerName}'s hand which has size [{opponent.HandCards.Count}]");
        }


    }
    //handles when the opponent plays a facility/effect card
    void HandleFacilityOpponentPlay(Update update, GamePhase phase, CardPlayer opponent) {
        Debug.Log($"Handling {opponent.playerName}'s facility card play with id {update.CardID} and name '{GetCardNameFromID(update.CardID)}'");
        Dictionary<int, GameObject> facilityList = ActiveFacilities.Count > 0 ? ActiveFacilities : opponent.ActiveFacilities;
        if (facilityList.TryGetValue((int)update.FacilityPlayedOnType, out GameObject facilityGo) && facilityGo.TryGetComponent(out Facility facility)) {
            Debug.Log($"{playerName} is creating card played on facility: {facility.facilityName}");
            if (opponent.HandCards.TryGetValue(update.UniqueID, out GameObject cardObject)) {
                Card tempCard = cardObject.GetComponent<Card>();
                Debug.Log($"Found {tempCard.data.name} with uid {tempCard.UniqueID} in {opponent.playerTeam}'s hand");
                HandleOpponentCardPlay(tempCard, facilityGo, opponent, facility);
            }
            else {
                Debug.LogError($"{playerName} was looking for card with uid {update.UniqueID} but did not find it in {opponent.playerName}'s hand which has size [{opponent.HandCards.Count}]");
            }
        }
    }

    #region Network Helpers

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
            Debug.Log($"type:{update.Type}|card id:{update.CardID}|unique id:{update.UniqueID}|amount:{update.Amount}|facility type:{update.FacilityPlayedOnType}");
            playsForMessage.Add(update.UniqueID);
            playsForMessage.Add(update.CardID);
            playsForMessage.Add((int)update.FacilityPlayedOnType);


            if (update.Type == CardMessageType.ReduceCost) {
                playsForMessage.Add(update.Amount);
            }
            else if (update.Type == CardMessageType.RemoveEffect) {
                playsForMessage.Add((int)update.FacilityPlayedOnType);
                //playsForMessage.Add((int)update.EffectTarget);
            }
            //else if (update.Type == CardMessageType.RestorePoints) {
            //    playsForMessage.Add(update.Amount);
            //    playsForMessage.Add((int)update.FacilityType);
            //}
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

    #endregion

    #endregion

    #region Reset
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
    #endregion

    #region Debug
    void TryLogFacilityInfo() {
        if (this != GameManager.instance.actualPlayer) return;
        if (TryGetFacilityUnderMouse(out Facility facility)) {
            facility.LogFacilityDebug();
        }
    }

    void TryShowEffectSelectionMenu() {
        if (this != GameManager.instance.actualPlayer) return;
        if (TryGetFacilityUnderMouse(out Facility facility)) {
            //facility.DebugAddNewEffect();
            // GameManager.instance.DisplayFacilityEffectChoiceMenu(facility, this);
        }
    }
    bool TryGetFacilityUnderMouse(out Facility facility) {
        var hitFacility = cardDropLocations.Values.ToList().Find(x => x.GetComponent<Collider2D>().OverlapPoint(Mouse.current.position.ReadValue()));
        if (hitFacility) {
            facility = hitFacility.GetComponentInParent<Facility>();
            return facility != null;
        }
        facility = null;
        return false;
    }

    public void LogPlayerInfo() {
        string s = $"Player info for: {playerName}\n";

        // Handle Hand Cards
        s += $"Hand size: {HandCards.Count}\n";
        foreach (var kvp in HandCards) {
            var card = kvp.Value.GetComponent<Card>();
            s += "\t[" + kvp.Key + "] - " + card.data.name + $" with uid: {card.UniqueID}\n";
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
    #endregion
}
