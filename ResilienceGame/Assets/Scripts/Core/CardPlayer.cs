using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Linq;
using static Facility;
using System;
using System.Text;
#region enums
// Enum to track player type
public enum PlayerTeam {
    Red,
    Blue,
    White,
    Any,
    None
};
public enum OverTimeState {
    None,
    Overtime,
    Exhausted
}



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
    public FacilityEffectType FacilityEffectToRemoveType;
    public FacilityType AdditionalFacilitySelectedOne;
    public FacilityType AdditionalFacilitySelectedTwo;
    public FacilityType AdditionalFacilitySelectedThree;
    public SectorType sectorPlayedOn;
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
    public int NetID = -1;
    public bool IsAI = false;
    public Sector PlayerSector { get; set; }
    public string DeckName = "";

    //[Header("Game References")]
    //public GameManager manager;

    [Header("Card Collections")]
    public static Dictionary<int, Card> cards = new Dictionary<int, Card>();
    //  public List<int> FacilityIDs = new List<int>(10);
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

    [Header("Card Positioning")]
    public readonly float ORIGINAL_SCALE = 0.2f;
    private HandPositioner handPositioner;

    [Header("Drag and Drop")]
    //public GameObject hoveredDropLocation;
    private GameObject previousHoveredFacility;
    private GameObject cardDroppedOnObject;
    public Dictionary<string, GameObject> cardDropLocations = new Dictionary<string, GameObject>();
    public bool IsDraggingCard { get; private set; } = false;

    [Header("Game State")]
    public List<Card> CardsAllowedToBeDiscard;
    // public Queue<(Update, GamePhase, CardPlayer)> opponentCardPlays = new Queue<(Update, GamePhase, CardPlayer)>();
    //public bool IsAnimating { get; set; } = false;
    public PlayerReadyState ReadyState { get; set; } = PlayerReadyState.ReadyToPlay;
    public int AmountToDiscard { get; set; } = 0;
    public int AmountToSelect { get; set; } = 0;
    public int AmountToReturnToDeck { get; private set; } = 0;

    public List<Card> CardsAllowedToBeSelected;
    public Action OnCardsReturnedToDeck { get; set; }
    public Action<List<Facility>> OnFacilitiesSelected { get; set; }

    [Header("Facilities")]
    private int facilityCount = 0;
    private bool registeredFacilities = false;

    [Header("Meeple Info")]
    //public const int STARTING_MEEPLES = 2;
    public float[] maxMeeples;
    public float BlueMeeples { get; private set; }
    public float BlackMeeples { get; private set; }
    public float PurpleMeeples { get; private set; }
    public float ColorlessMeeples { get; private set; }

    //(timer, type)
    public List<(int, int)> sharedMeepleTypes = new List<(int, int)>();
    public List<(int, int)> receivedMeepleTypes = new List<(int, int)>();
    public int SharedMeepleAmount => sharedMeepleTypes.Count;
    public int ReceivedMeepleAmount => receivedMeepleTypes.Count;
    //public int sharedMeepleTimer = 0;
    public readonly int MEEPLE_SHARE_DURATION = 2;



    //public TextMeshProUGUI[] meeplesAmountText;
    // [SerializeField] private Button[] meepleButtons;
    //[SerializeField] private Image[] meepleImages;
    private Action OnMeeplesSelected;

    public int meeplesSpent = 0;
    public int numMeeplesRequired = 0;
    private int mMeeplesSpent = 0;

    [Header("Overtime")]
    public OverTimeState otState;
    public int OverTimeCounter { get; set; } = 0;
    public int overTimeCharges; // Tracks how often a sector can mandate overtime
    public readonly int MAX_OVERTIME_CHARGES = 2;
    [Header("Scoring")]
    private int mFinalScore = 0;

    // Private fields
    //private static int sUniqueIDCount = 0;
    public Queue<Update> mUpdatesThisPhase = new Queue<Update>(6);


    // Enum definition
    public enum PlayerReadyState {
        ReadyToPlay,
        ReturnCardsToDeck,
        DiscardCards,
        SelectFacilties,
        SelectMeeplesWithUI,
        SelectCardsForCostChange,
        EndedPhase
    }
    #endregion

    #region Interface Updates + Meeple Spending
    public bool CanAffordCardPlay(Card card, ref bool spendColorless) {
        float blueLeft = BlueMeeples - card.data.blueCost;
        float blackLeft = BlackMeeples - card.data.blackCost;
        float purpleLeft = PurpleMeeples - card.data.purpleCost;

        float totalDeficit = 0;
        spendColorless = false;

        // Calculate total deficit across all meeples
        if (blueLeft < 0) totalDeficit += -blueLeft;
        if (blackLeft < 0) totalDeficit += -blackLeft;
        if (purpleLeft < 0) totalDeficit += -purpleLeft;

        // Check if colorless meeples can cover the total deficit
        if (totalDeficit > ColorlessMeeples) {
            return false; // Not enough colorless meeples to cover the deficit
        }

        // If we have enough colorless meeples, mark them for spending
        spendColorless = totalDeficit > 0;
        return true;
    }

    public bool TrySpendMeeples(Card card, ref int numMeeplesSpent) {
        bool spendColorless = false;

        // Check if we can afford to play the card
        if (CanAffordCardPlay(card, ref spendColorless)) {
            // Deduct required meeples from each pool
            BlueMeeples -= card.data.blueCost;
            BlackMeeples -= card.data.blackCost;
            PurpleMeeples -= card.data.purpleCost;

            // Calculate the total number of meeples used
            numMeeplesSpent = (int)(card.data.blueCost + card.data.blackCost + card.data.purpleCost);

            // Spend colorless meeples if needed to cover deficits
            if (spendColorless) {
                float blueDeficit = Math.Max(0, -BlueMeeples);
                float blackDeficit = Math.Max(0, -BlackMeeples);
                float purpleDeficit = Math.Max(0, -PurpleMeeples);
                float totalDeficit = blueDeficit + blackDeficit + purpleDeficit;

                // Deduct from colorless pool and zero out deficits
                ColorlessMeeples -= (int)totalDeficit;
                BlueMeeples = Math.Max(0, BlueMeeples);
                BlackMeeples = Math.Max(0, BlackMeeples);
                PurpleMeeples = Math.Max(0, PurpleMeeples);

                numMeeplesSpent += (int)totalDeficit;
            }

            // Update the UI with the new meeple amounts
            UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
            return true;
        }

        return false;
    }


    public void SpendMeepleWithButton(int index) {
        switch (index) {
            case 0:
                BlackMeeples--;
                meeplesSpent++;
                if (BlackMeeples == 0) {
                    UserInterface.Instance.DisableMeepleButtonByIndex(index);
                }
                break;
            case 1:
                BlueMeeples--;
                meeplesSpent++;
                if (BlueMeeples == 0) {
                    UserInterface.Instance.DisableMeepleButtonByIndex(index);
                }
                break;
            case 2:
                PurpleMeeples--;
                meeplesSpent++;
                if (PurpleMeeples == 0) {
                    UserInterface.Instance.DisableMeepleButtonByIndex(index);
                }
                break;
        }
        if (numMeeplesRequired > 0 && meeplesSpent > 0) {
            numMeeplesRequired--;
            if (numMeeplesRequired == 0) {
                UserInterface.Instance.ResolveTextAlert();
                OnMeeplesSelected?.Invoke();
                UserInterface.Instance.DisableMeepleButtons();
            }
            else {
                UserInterface.Instance.DisplayAlertMessage($"Spend {numMeeplesRequired} {(numMeeplesRequired > 1 ? "meeples" : "meeple")} to continue", this);

            }
        }
        UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
    }
    //public void AddSubtractMeepleAmount(int index, float numMeeples) {
    //    if (index < 0 || index >= 3) return;
    //    maxMeeples[index] += numMeeples;
    //    if (maxMeeples[index] < 0) maxMeeples[index] = 0;

    //    if (blackMeeples > maxMeeples[0]) blackMeeples = maxMeeples[0];
    //    if (blueMeeples > maxMeeples[1]) blueMeeples = maxMeeples[1];
    //    if (purpleMeeples > maxMeeples[2]) purpleMeeples = maxMeeples[2];
    //    UserInterface.Instance.UpdateMeepleAmountUI(blackMeeples, blueMeeples, purpleMeeples);
    //}
    //public void MultiplyMeepleAmount(int index, float multiplier) {
    //    if (index < 0 || index >= 3) return;
    //    var reduceAmt = (int)Mathf.Floor(maxMeeples[index] * multiplier);   //don't reduce by a half value...why were meeples floats ever
    //    if (reduceAmt > 0) {
    //        AddSubtractMeepleAmount(index, reduceAmt);
    //    }
    //}
    public void ForcePlayerToChoseMeeples(int numMeeplesRequired, Action onFinish) {
        this.numMeeplesRequired = numMeeplesRequired;
        UserInterface.Instance.DisplayAlertMessage($"Spend {this.numMeeplesRequired} {(this.numMeeplesRequired > 1 ? "meeples" : "meeple")} to continue", this, onAlertFinish: onFinish);
        UserInterface.Instance.EnableMeepleButtons();
        OnMeeplesSelected = onFinish;

    }

    #endregion

    #region Initialization
    public void Start() {

        if (UserInterface.Instance.handDropZone)
            handPositioner = UserInterface.Instance.handDropZone.GetComponent<HandPositioner>();
        else {
            Debug.LogError("Hand drop zone not found");
        }

        InitDropLocations();

        overTimeCharges = MAX_OVERTIME_CHARGES;
    }
    public void InitializeCards() {
        DeckIDs.Clear();
        //manager = GameObject.FindObjectOfType<GameManager>();
        Debug.Log("card count is: " + cards.Count);
        foreach (Card card in cards.Values) {
            if (card != null && card.DeckName.Equals(DeckName)) {
                //    Debug.Log("adding card " + card.name + " with id " + card.data.cardID + " to deck " + DeckName);
                for (int j = 0; j < card.data.numberInDeck; j++) {
                    DeckIDs.Add(card.data.cardID);
                }

            }
        }
        maxMeeples = new float[] { 2, 2, 2, playerTeam == PlayerTeam.Red ? 1 : 0 };
        BlackMeeples = maxMeeples[0];
        BlueMeeples = maxMeeples[1];
        PurpleMeeples = maxMeeples[2];
        ColorlessMeeples = maxMeeples[3];


    }
    //add the facilities to the player's active facilities
    public void RegisterFacilities() {
        registeredFacilities = true;
        Debug.Log($"Player {playerName} of team {playerTeam} registering facilities");
        foreach (Facility facility in PlayerSector.facilities) {
            ActiveFacilities.Add((int)facility.facilityType, facility.gameObject);
            // FacilityIDs.Add((int)facility.facilityType);
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
            //  Debug.Log(dropZone.name);
            //cardDropColliders.Add(tag, dropZone.GetComponent<Collider2D>());
        }
        // Debug.Log("Card Drop Locations: " + cardDropLocations.Count);

    }
    #endregion

    #region Card Action Functions

    //called by card action to tell the player they need to select facilities to apply the card action to
    public void ForcePlayerSelectFacilities(int numFacilitiesToSelect, bool removeEffect, FacilityEffectType preReqEffect, Action<List<Facility>> onFacilitySelect) {
        if (numFacilitiesToSelect <= 0) {
            Debug.LogWarning("Starting facility select with no facilities to select");
            return;
        }
        ReadyState = PlayerReadyState.SelectFacilties;
        Debug.Log($"Forcing {playerName} to select {numFacilitiesToSelect} facilities before continuing");

        var numAvail = GameManager.Instance.sectorInView.EnableFacilitySelection(numFacilitiesToSelect, opponentTeam: GetOpponentTeam(), removeEffect, preReqEffect);

        if (numAvail == 0) {
            ReadyState = PlayerReadyState.ReadyToPlay;
            Debug.LogError("No facilities available to select");
            return;
        }
        UserInterface.Instance.DisplayAlertMessage($"Select {numAvail} facilities to apply the card effect", this);
        OnFacilitiesSelected = onFacilitySelect;
    }
    public void ResolveFacilitySelection() {
        ReadyState = PlayerReadyState.ReadyToPlay;
        UserInterface.Instance.ResolveTextAlert();
        var facilities = GameManager.Instance.sectorInView.GetSelectedFacilities();
        if (facilities == null) {
            Debug.LogError("selected facility list is null");
            return;
        }
        GameManager.Instance.sectorInView.DisableFacilitySelection();
        OnFacilitiesSelected?.Invoke(facilities);
    }
    public void ChooseMeeplesThenReduceCardCost(int amountOfMeeplesNeeded, CardPlayer player, Card card) {
        ChooseMeeples(amountOfMeeplesNeeded, player, card);
    }
    private void ChooseMeeples(int amountOfMeeplesNeeded, CardPlayer player, Card card) {
        ReadyState = PlayerReadyState.SelectMeeplesWithUI;
        ForcePlayerToChoseMeeples(amountOfMeeplesNeeded, () => SelectCardsInHand(player, card));
        SelectMeeplesOnCards();
    }
    private void SelectCardsInHand(CardPlayer player, Card card) {
        UserInterface.Instance.DisplayAlertMessage($"Choose {card.data.targetAmount} cards to reduce meeple cost", player);
        AddSelectEvent(card.data.targetAmount);
    }
    private void SelectMeeplesOnCards() {
        //Enable Meeple Interface

        //Store the chosen colours for each card

        //Reduce meeple costs for those cards

        //ask mike about like an animation showing it going back to the hand

    }

    //Sets the variables required to force the player to select a certain amount of cards
    public void AddSelectEvent(int amount, List<Card> cardsAllowedToBeSelected = null) {
        ReadyState = PlayerReadyState.SelectCardsForCostChange;
        AmountToSelect = amount;
        //need like a select dropzone here
        CardsAllowedToBeSelected = cardsAllowedToBeSelected;
        Debug.Log($"Enabling {playerName} to select cards");
    }

    public void StopSelect() {
        ReadyState = PlayerReadyState.ReadyToPlay;
        CardsAllowedToBeSelected?.ForEach(card => card.ToggleOutline(false));
        CardsAllowedToBeSelected = null;
        //need a select dropzone here
        UserInterface.Instance.ResolveTextAlert();
        Debug.Log($"Disabling {playerName}'s ability to select");
    }

    //Sets the variables required to force the player to discard a certain amout of cards
    public void AddDiscardEvent(int amount, List<Card> cardsAllowedToBeDiscard = null) {
        ReadyState = PlayerReadyState.DiscardCards;         //set the player state to discard cards
        AmountToDiscard = amount;                           //set the amount of cards to discard
        UserInterface.Instance.EnableDiscardDrop();                   //enable the discard drop zone
        CardsAllowedToBeDiscard = cardsAllowedToBeDiscard;  //holds the cards that are allowed to be discarded (like draw 3 discard 1 of them)
        Debug.Log($"Enabling {playerName}'s discard temporarily");
    }
    //Returns the player to ready to play state by disabling necessary ui elements and setting player state
    public void StopDiscard() {
        ReadyState = PlayerReadyState.ReadyToPlay;
        CardsAllowedToBeDiscard?.ForEach(card => card.ToggleOutline(false));    //turn off the outline on the cards that were allowed to be discarded
        CardsAllowedToBeDiscard = null;             //dispose of the list
        UserInterface.Instance.DisableDiscardDrop();           //disable the discard drop zone
        UserInterface.Instance.ResolveTextAlert();    //hide alert panel
        Debug.Log($"Disabling {playerName}'s discard");
    }
    //returns the card from the hand to the deck
    private void ReturnCardToDeck(Card card, bool updateNetwork) {
        Debug.Log($"{playerName} is returning card to deck");
        Debug.Log($"Does {playerName} have an update in queue: {mUpdatesThisPhase.Any()}");


        if (HandCards.Remove(card.UniqueID)) {
            DeckIDs.Add(card.data.cardID);//add it back to the deck
            Destroy(card.gameObject);
            Debug.Log($"Successfully returned {card.data.name} to the deck for player {playerName}");
        }
        else {
            Debug.LogError($"card with unique id {card.UniqueID} was not found in {playerName}'s hand");
        }

        if (updateNetwork) {//also means its actual player
            handPositioner.cards.Remove(card);
            EnqueueAndSendCardMessageUpdate(CardMessageType.ReturnCardToDeck, card.data.cardID, card.UniqueID);
        }

    }
    //Called by a card action to return the entire hand to the deck and draw a new hand
    public void ReturnHandToDeckAndDraw(int amount) {
        HandCards.Values.ToList().ForEach(card => {
            ReturnCardToDeck(card.GetComponent<Card>(), true);
        });
        DrawNumberOfCards(amount, updateNetwork: true);
    }
    //Called by a card action to force the player to return cards to the deck by dragging them to the play area
    public void ForcePlayerReturnCardsToDeck(int amount, Action onCardsReturned) {

        if (amount > 0) {
            AmountToReturnToDeck = amount;
            OnCardsReturnedToDeck = onCardsReturned;
            ReadyState = PlayerReadyState.ReturnCardsToDeck;
            UserInterface.Instance.DisplayAlertMessage($"Return {AmountToReturnToDeck} cards to the deck\nby dragging them to the play area", this);
        }
    }
    #endregion

    #region Meeple Sharing
    //called at the end of each action phase to update the shared meeples
    public void UpdateMeepleSharing() {
        Debug.Log($"{playerName} has {SharedMeepleAmount} lent meeples, and {receivedMeepleTypes} borrowed meeples");
        CheckReceivedMeeplesAndReturn();
        CheckSharedMeeplesAndReturn();
    }
    //increase the meeple count by 1 (max and current) of the specified color
    public void IncrementMeepleByIndex(int index) {
        if (index >= 0 && index < maxMeeples.Length) {
            maxMeeples[index]++;
            switch (index) {
                case 0:
                    BlackMeeples++;
                    break;
                case 1:
                    BlueMeeples++;
                    break;
                case 2:
                    PurpleMeeples++;
                    break;
                case 3:
                    ColorlessMeeples++;
                    break;
            }
            UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
            UserInterface.Instance.UpdateMeepleSharingMenu();
        }
    }
    //decrease the meeple count by 1 (max and current) of the specified color
    public void DecrememntMeepleByIndex(int index) {
        if (index >= 0 && index < maxMeeples.Length) {
            maxMeeples[index]--;
            switch (index) {
                case 0:
                    BlackMeeples--;
                    break;
                case 1:
                    BlueMeeples--;
                    break;
                case 2:
                    PurpleMeeples--;
                    break;
            }
            UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
            UserInterface.Instance.UpdateMeepleSharingMenu();
        }
    }
    //returns true if the player has a meeple of the specified color index
    private bool HasMeepleOfColor(int index) {
        return index switch {
            0 => BlackMeeples > 0,
            1 => BlueMeeples > 0,
            2 => PurpleMeeples > 0,
            3 => ColorlessMeeples > 0,
            _ => false
        };
    }
    //reduces the meeple count of the specified color index by 1
    //and adds it to the shared meeple tracker
    public bool ShareMeeple(int index) {
        if (index >= 0 && index < maxMeeples.Length) {
            if (maxMeeples[index] > 0) {
                if (HasMeepleOfColor(index)) {
                    DecrememntMeepleByIndex(index);
                    sharedMeepleTypes.Add((MEEPLE_SHARE_DURATION, index));
                    return true;
                }
            }
        }
        return false;
    }
    //Handles receiving a shared meeple from another player
    public void ReceiveSharedMeeple(int index) {
        Debug.Log($"{playerName} received a shared meeple of type {index}");
        if (index >= 0 && index < maxMeeples.Length) {
            receivedMeepleTypes.Add((MEEPLE_SHARE_DURATION, index));
            IncrementMeepleByIndex(index);
        }
    }
    //Checks the shared meeples list at the end of the action phase
    //to check if any shared meeples have expired and need to be returned
    public void CheckSharedMeeplesAndReturn() {
        for (int i = 0; i < sharedMeepleTypes.Count; i++) {
            if (sharedMeepleTypes[i].Item1 <= 0) {
                IncrementMeepleByIndex(sharedMeepleTypes[i].Item2);
                sharedMeepleTypes.RemoveAt(i);
            }
            else {
                sharedMeepleTypes[i] = (sharedMeepleTypes[i].Item1 - 1, sharedMeepleTypes[i].Item2);
            }
        }
    }
    //Checks the received meeples list at the end of the action phase
    //to check if any received meeples have expired and need to be returned
    public void CheckReceivedMeeplesAndReturn() {
        for (int i = 0; i < receivedMeepleTypes.Count; i++) {
            if (receivedMeepleTypes[i].Item1 <= 0) {
                IncrementMeepleByIndex(receivedMeepleTypes[i].Item2);
                receivedMeepleTypes.RemoveAt(i);
            }
            else {
                receivedMeepleTypes[i] = (receivedMeepleTypes[i].Item1 - 1, receivedMeepleTypes[i].Item2);
            }
        }
    }
    public void ShareMeepleWithPlayer(int index, CardPlayer player) {
        EnqueueAndSendCardMessageUpdate(CardMessageType.MeepleShare,
                                            UniqueID: player.NetID,
                                            CardID: index,
                                            Amount: 1);
    }
    #endregion

    #region Helpers
    public void ResetMeepleCount() {
        meeplesSpent = 0;

        BlackMeeples = maxMeeples[0];
        BlueMeeples = maxMeeples[1];
        PurpleMeeples = maxMeeples[2];
        ColorlessMeeples = maxMeeples[3];
        if (GameManager.Instance.actualPlayer == this) {
            // Debug.Log($"Resetting to MaxMeeples: {maxMeeples[0]}, {maxMeeples[1]}, {maxMeeples[2]}, {maxMeeples[3]}");
            //  Debug.Log($"Player {playerName} has {BlackMeeples} black, {BlueMeeples} blue, {PurpleMeeples} purple, and {ColorlessMeeples} colorless meeples");
            UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
        }

    }

    public void IncMaxColorlessMeeples(float value) {
        maxMeeples[3] += value;
        ColorlessMeeples = maxMeeples[3];
        UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
    }

    public void ResetMaxColorlessMeeples() {
        maxMeeples[3] = 1;
        ColorlessMeeples = maxMeeples[3];
        UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
    }

    public int GetTotalMeeples() {
        return (int)(BlueMeeples + BlackMeeples + PurpleMeeples);
    }
    public int GetMaxMeeples() {
        return (int)Mathf.Floor(maxMeeples.Aggregate((a, b) => a + b));
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
    public void ResetMeeplesSpent() {
        mMeeplesSpent = 0;
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
    private SectorType SectorPlayedOn() {
        if (cardDroppedOnObject != null) {
            return cardDroppedOnObject.GetComponentInParent<Sector>().sectorName;
        }
        return SectorType.Any;
    }
    private Facility FacilityPlayedOn() {
        Facility facility = null;
        if (cardDroppedOnObject != null) {
            facility = cardDroppedOnObject.GetComponentInParent<Facility>();
        }
        return facility;
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
                    dropZone: UserInterface.Instance.handDropZone,
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
        Debug.Log($"[{(GameManager.Instance.IsServer ? "SERVER" : "CLIENT")}]'s player {playerName} is trying to draw {GetCardNameFromID(id)} with uid {uid}");
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
            GameManager.Instance.UniqueCardIdCount++;
            //Debug.Log("setting unique id for card " + uniqueId);
        }
        else {
            // since there are multiples of each card type potentially
            // in a deck they need a unique id outside of the card's id
            //tempCard.UniqueID = GameManager.instance.UniqueCardIdCount++;
            if (GameManager.Instance.IsServer) {
                tempCard.UniqueID = RGNetworkPlayerList.instance.DrawCardForPlayer(RGNetworkPlayerList.instance.localPlayerID);
            }
            else {
                //assign temp unique id before getting a real one from network
                tempCard.UniqueID = GameManager.Instance.UniqueCardIdCount++;
            }

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
            GameManager.Instance.SendUpdatesToOpponent(GameManager.Instance.MGamePhase, this);
        }

        UserInterface.Instance.UpdateUISizeTrackers(); //update UI
        UserInterface.Instance.UpdatePlayerMenuItem(this);



        return tempCard;
    }

    #endregion

    #region Debug

    void HandleDebugInput() {
        //force add backdoor or fortify to hovered facility
        if (GameManager.Instance.actualPlayer == this) {
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
            if (GameManager.Instance.actualPlayer == this) {
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
        if (Keyboard.current.f5Key.wasPressedThisFrame) {
            maxMeeples = new float[] { 99, 99, 99, 99 };
            BlueMeeples = 99;
            BlackMeeples = 99;
            PurpleMeeples = 99;
            ColorlessMeeples = 99;
            UserInterface.Instance.UpdateMeepleAmountUI(BlackMeeples, BlueMeeples, PurpleMeeples, ColorlessMeeples);
        }
    }
    public void HandleMenuToggle() {
        if (this != GameManager.Instance.actualPlayer) {
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
            DrawCard(true, 0, -1, ref DeckIDs, UserInterface.Instance.handDropZone, true, ref HandCards);
        }
    }
    public virtual void ForceDiscardRandomCard() {
        var num = UnityEngine.Random.Range(0, HandCards.Count);
        var card = HandCards[num];
        HandCards.Remove(num);
        Discards.Add(num, card);
        card.GetComponent<Card>().SetCardState(CardState.CardNeedsToBeDiscarded);
        card.transform.SetParent(UserInterface.Instance.discardDropZone.transform, false);
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
            UpdateHoveredDropLocation(Mouse.current.position.ReadValue());
            //if (hoveredDropLocation != null)
            //    Debug.Log(hoveredDropLocation.name);
        }

        //wait and check for the proper amount of facilities selected
        if (ReadyState == PlayerReadyState.SelectFacilties) {
            if (PlayerSector.HasSelectedFacilities()) {

                ResolveFacilitySelection();
            }
        }


        if (GameManager.Instance.DEBUG_ENABLED) {
            HandleDebugInput();
        }
    }


    //updates the hoverDropLocation class field to hold the object the card is hovering over
    void UpdateHoveredDropLocation(Vector2 position, bool shouldHighlight = true, Card cardDragged = null) {
        GameObject currentHoveredFacility = null; // Reset at the beginning of each update
        bool isOverAnyDropLocation = false;

        ////only highlight when the player is ready to play cards
        //if (ReadyState != PlayerReadyState.ReadyToPlay) {
        //    return;
        //}

        //  Vector2 mousePosition = Mouse.current.position.ReadValue();

        Collider2D[] hoveredColliders = Physics2D.OverlapPointAll(position, LayerMask.GetMask("CardDrop"));
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

                var cardBeingDragged = cardDragged == null ? handPositioner.CardsBeingDragged.First() : cardDragged;
                // Debug.Log(cardDraggedTarget);
                // Check if the card being dragged is a facility card
                if (cardBeingDragged.target == CardTarget.Facility || cardBeingDragged.target == CardTarget.Effect) {
                    if (GameManager.Instance.CanHighlight() && shouldHighlight) {
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
                UserInterface.Instance.hoveredDropLocation = hoveredFacilityCollider.gameObject;
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
            UserInterface.Instance.hoveredDropLocation = null;
        }

        // Debug.Log("Hovered Drop Location: " + hoveredDropLocation);

        // Update previous hovered facility
        previousHoveredFacility = currentHoveredFacility;
    }

    #endregion

    #region Card Play Functions

    #region Play Update Loop
    //original PlayCard function that is called from the update loop
    public virtual int HandlePlayCard(GamePhase phase) {
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
                            HandleDiscardDrop(card, phase, ref playCount, ref playKey);
                            break;
                        case CardDropZoneTag.FACILITY:
                            if (card.target == CardTarget.Facility || card.target == CardTarget.Effect) {
                                HandleFacilityDrop(card, phase, ref playCount, ref playKey);
                            }
                            else {
                                HandleFreePlayDrop(card, phase, ref playCount, ref playKey);
                            }
                            break;
                        case CardDropZoneTag.FREE_PLAY:
                            Debug.Log($"Handing a sectorwide drop");
                            HandleFreePlayDrop(card, phase, ref playCount, ref playKey);
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
                    UserInterface.Instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
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
                    // Debug.Log("removed key " + playKey + " and updating tracker UI");
                    UserInterface.Instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
                }
            }
        }

        return playCount;
    }
    #endregion

    #region Dropping
    public Card AiDropCardOn(Card card, Vector2 position) {
        Debug.Log($"{playerName}'s ai is playing {card.data.name} at {position}");
        if (card == null || position == null) return null;
        UpdateHoveredDropLocation(position, false, card);
        return HandleCardDrop(card);
    }

    //This function is called when a card is dropped from that card's slippy component (happens one time at drop)
    public Card HandleCardDrop(Card card) {
        if (UserInterface.Instance.hoveredDropLocation == null) {
            Debug.Log("No drop location found");
            return null;
        }
        else {
            //clear the hover effect
            if (UserInterface.Instance.hoveredDropLocation.CompareTag("FacilityDropLocation")) {
                UserInterface.Instance.hoveredDropLocation.GetComponent<HoverActivateObject>().DeactivateHover();
            }
            if (ValidateCardPlay(card)) {
                //check if the game is waiting for the player to return cards to the deck by playing them
                if (ReadyState == PlayerReadyState.ReturnCardsToDeck) {
                    Debug.Log("Returning card to deck");
                    ReturnCardToDeck(card, true);
                    AmountToReturnToDeck--;
                    if (AmountToReturnToDeck > 0) {
                        UserInterface.Instance.DisplayAlertMessage($"Return {AmountToReturnToDeck} more cards to the deck", this); //update alert message
                    }
                    else {
                        OnCardsReturnedToDeck?.Invoke(); //Resolve the action after cards have been returned to deck
                        UserInterface.Instance.ResolveTextAlert(); //remove alert message
                        ReadyState = PlayerReadyState.ReadyToPlay; //reset player state
                    }

                }
                //check for a card play or card discard
                else if (ReadyState == PlayerReadyState.ReadyToPlay || ReadyState == PlayerReadyState.DiscardCards) {
                    Debug.Log("Card is valid to play and player is ready");
                    //set var to hold where the card was dropped
                    cardDroppedOnObject = UserInterface.Instance.hoveredDropLocation;
                    //set card state to played
                    card.SetCardState(CardState.CardDrawnDropped);
                    //remove card from hand
                    handPositioner.cards.Remove(card);
                    //set the parent to where it was played
                    card.transform.transform.SetParent(UserInterface.Instance.hoveredDropLocation.transform);
                    return card;
                }
                else if (ReadyState == PlayerReadyState.SelectCardsForCostChange) {
                    if (AmountToSelect > 0) {
                        UserInterface.Instance.DisplayCardChoiceMenu(card, AmountToSelect--);
                        handPositioner.cards.Remove(card);
                        Debug.Log(card + " selected for Training");
                    }
                    else StopSelect();
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
    private void HandleDiscardDrop(Card card, GamePhase phase, ref int playCount, ref int playKey) {
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
                        GameManager.Instance.DisablePlayerDiscard(this);
                    }
                    discard = true;
                }
                break;
        }
        if (discard) {
            Debug.Log($"Adding discard update from {playerName} who discarded {card.data.name} with uid {card.UniqueID}");
            EnqueueAndSendCardMessageUpdate(CardMessageType.DiscardCard, card.data.cardID, card.UniqueID);
            card.gameObject.SetActive(false);
            UserInterface.Instance.UpdateUISizeTrackers();
            //GameManager.Instance.AddActionLogMessage($"{playerName} discarded {card.data.name}");
        }
    }
    //Called when a Facility/Effect target card is dropped in the play area
    private void HandleFacilityDrop(Card card, GamePhase phase, ref int playCount, ref int playKey) {

        Facility facility = FacilityPlayedOn();
        Debug.Log($"Handling {card.front.title} played on {facility.facilityName}");
        switch (phase) {
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                card.SetCardState(CardState.CardInPlay);
                ActiveCards.Add(card.UniqueID, card.gameObject);
                EnqueueAndSendCardMessageUpdate(CardMessageType.CardUpdate,
                                    card.data.cardID, card.UniqueID,
                                    sectorType: facility.sectorItsAPartOf.sectorName,
                                    facilityType: facility.facilityType); //send the update to the opponent

                // card.Play(this, opponentPlayer, facility);
                playCount = 1;
                playKey = card.UniqueID;
                //log the play
                GameManager.Instance.AddActionLogMessage($"{playerName} played {card.front.title} on {facility.facilityName} in sector {facility.sectorItsAPartOf.sectorName}");

                // Start the animation

                StartCoroutine(card.AnimateCardToPosition(facility.transform.position, .6f, () => card.Play(this, null, facility)));

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
    private void HandleFreePlayDrop(Card card, GamePhase phase, ref int playCount, ref int playKey) {
        Debug.Log($"Handling non facility card - {card.front.title}");
        // Facility facility = FacilityPlayedOn(); //still need this for sector cards
        var sectorType = SectorPlayedOn();
        if (sectorType == SectorType.Any || sectorType == SectorType.All) {
            Debug.LogError("Sector type is not valid for card play");
            return;
        }

        Sector sector = GameManager.Instance.AllSectors[sectorType];
        switch (phase) {
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                card.SetCardState(CardState.CardInPlay);
                ActiveCards.Add(card.UniqueID, card.gameObject);

                EnqueueCardMessageUpdate(
                    CardMessageType.CardUpdate,
                    card.data.cardID,
                    card.UniqueID,
                    sectorType: sectorType,
                    sendUpdateImmediately: Sector.DoesUpdateCallCardPlay(card));
                playCount = 1;
                playKey = card.UniqueID;

                GameManager.Instance.AddActionLogMessage($"{playerName} played {card.front.title} on sector {sector.sectorName}");

                //start shrink animation
                StartCoroutine(card.AnimateCardToPosition(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), .6f,
                    () => card.Play(this, null, sector.facilities[0]))); //pass the first facility in the sector, we just use it to get the sector later

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
        if (IsAI) {
            Debug.LogWarning($"Can't play manually as AI player!!");
            return false;
        }
        if (AmountToReturnToDeck > 0) {
            Debug.Log($"Returning {card.front.title} to deck");
            return true;
        }

        switch (GameManager.Instance.MGamePhase) {
            case GamePhase.DrawRed:
            case GamePhase.DrawBlue:
                (response, canPlay) = CanDiscardCard(card);
                break;
            case GamePhase.BonusBlue:

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

        Debug.Log($"Playing {card.front.title} on {UserInterface.Instance.hoveredDropLocation.name} - {(canPlay ? "Allowed" : "Rejected")}\n{response}");

        return canPlay;
    }
    private (string, bool) ValidateDiscardPlay(Card card) {
        if (UserInterface.Instance.hoveredDropLocation.CompareTag(CardDropZoneTag.DISCARD)) {
            return ("Can discard during discard phase", true);
        }
        return ("Must discard on the discard drop zone", false);
    }
    private (string, bool) ValidateDiscardDuringActionPlay(Card card) {
        if (!GameManager.Instance.MIsDiscardAllowed)
            return ("Game manager says discard is not allowed", false);
        if (UserInterface.Instance.hoveredDropLocation.CompareTag(CardDropZoneTag.DISCARD)) {
            if (CardsAllowedToBeDiscard == null)    //Any card can be discarded
                return ("Discard any card allowed", true);
            if (CardsAllowedToBeDiscard.Contains(card)) //only highlighted cards can be discarded
                return ("Allowing discard of valid card", true);
            return ("Must discard one of the highlighted cards", false); //highlighted cards must be discarded
        }
        return ("Must discard cards first", false); //didn't drop on the discard drop zone
    }
    private (string, bool) ValidateActionAndReadyPlay(Card card) {
        //check prereq effects on cards for effect cards played on single facilities
        if (card.data.preReqEffectType != FacilityEffectType.None) {

            Facility facility = UserInterface.Instance.hoveredDropLocation.GetComponent<Facility>();
            if (facility == null) {
                return ("Facility not found on hovered drop location", false);
            }
            if (!facility.HasEffectOfType(card.data.preReqEffectType)) {
                return ("Facility effect does not match card prereq effect", false);
            }
        }
        //check for 'Remove' effect for sector cards
        if (card.data.effectString == "Remove") {
            Sector sector = null;
            if (UserInterface.Instance.hoveredDropLocation.TryGetComponent(out Facility facility)) {
                sector = facility.sectorItsAPartOf;
            }
            else {
                sector = UserInterface.Instance.hoveredDropLocation.GetComponentInParent<Sector>();
            }
            if (sector == null) {
                Debug.LogError($"Sector should be found here");
                return ("Sector card must be played on a sector", false);
            }
            if (!sector.HasRemovableEffectsOnFacilities(GetOpponentTeam())) {
                return ("Sector does not have removable effects", false);
            }
        }
        if (!TrySpendMeeples(card, ref mMeeplesSpent)) {
            return ("Not enough meeples to play card", false);
        }
        return ("Valid action play", true);
    }
    private (string, bool) ValidateActionPlay(Card card) {
        Debug.Log("Checking if card can be played in action phase");
        if (!GameManager.Instance.IsActualPlayersTurn())    //make sure its the player's turn
            return ($"It is not {playerTeam}'s turn", false);

        //dont allow playing on allied sectors outside of doom clock and DC effect cards
        if (UserInterface.Instance.hoveredDropLocation.TryGetComponent(out Facility facility)) {
            var sector = facility.sectorItsAPartOf;
            if (sector.sectorName != PlayerSector.sectorName) {
                if (!GameManager.Instance.IsDoomClockActive && playerTeam == PlayerTeam.Blue) {
                    return ("Cannot play on allied sectors outside of doomclock", false);
                }
                else if (GameManager.Instance.IsDoomClockActive && playerTeam == PlayerTeam.Blue) {
                    if (!card.data.hasDoomEffect) {
                        return ($"Cannot play {card.data.name} on allied sectors", false);
                    }
                }

            }

        }

        if (card.data.name == "Call Bluff" && !GameManager.Instance.IsBluffActive) {
            return ($"Cannot play {card.data.name} when the bluff isn't active", false);
        }

        if (card.data.name == "Aggression" && GameManager.Instance.IsRedAggressive) {
            return ($"Cannot play {card.data.name} as Red is already aggressive", false);
        }


        return ReadyState switch {
            PlayerReadyState.SelectFacilties => ($"Player must select facilities before playing cards", false),
            PlayerReadyState.DiscardCards => ValidateDiscardDuringActionPlay(card),
            PlayerReadyState.SelectCardsForCostChange => ("Valid card selection", true),
            PlayerReadyState.EndedPhase => ("Cannot play cards after phase has ended", false),
            PlayerReadyState.ReadyToPlay => ValidateActionAndReadyPlay(card),
            _ => ("Invalid state", false),
        };
    }

    private (string, bool) CanDiscardCard(Card card) {
        Debug.Log("Checking if card can be discarded");
        //check if it is the player's turn
        if (!GameManager.Instance.IsActualPlayersTurn())
            return ($"It is not {playerTeam}'s turn", false);
        //draw phase checks if the player is discarding a card and if they havent discard more than allowed this phase
        if (GameManager.Instance.MGamePhase == GamePhase.DrawBlue || GameManager.Instance.MGamePhase == GamePhase.DrawRed) {
            if (UserInterface.Instance.hoveredDropLocation.CompareTag("DiscardDropLocation") && GameManager.Instance.MNumberDiscarded < GameManager.Instance.MAX_DISCARDS) {
                return ("", true);
            }
            return ("Cannot discard more than " + GameManager.Instance.MAX_DISCARDS + " cards per turn", false);
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
                cardGameObject.transform.SetParent(UserInterface.Instance.discardDropZone.transform, false);
                cardGameObject.transform.localPosition = new Vector3();
                cardGameObject.SetActive(false);
                HandCards.Remove(uid);
                UserInterface.Instance.UpdateUISizeTrackers();
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
                activeCardObject.transform.SetParent(UserInterface.Instance.discardDropZone.transform, false);
                activeCardObject.transform.localPosition = new Vector3();
                activeCardObject.transform.localScale = new Vector3(1, 1, 1);

                // for the future might want to stack cards in the discard zone
                // Debug.Log("setting card to discard zone: " + card.UniqueID + " with name " + card.front.title);
                activeCardObject.SetActive(false);
                card.cardZone = UserInterface.Instance.discardDropZone;
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
        UserInterface.Instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
    }

    #endregion

    #region Network

    #region Sending
    public void UpdateNextInQueueMessage(CardMessageType cardMessageType, int CardID, int UniqueID, int Amount = -1, SectorType sectorType = SectorType.Any,
        FacilityType facilityDroppedOnType = FacilityType.None, FacilityType facilityType1 = FacilityType.None,
        FacilityType facilityType2 = FacilityType.None,
        FacilityType facilityType3 = FacilityType.None, FacilityEffectType effectTargetType = FacilityEffectType.None, bool sendUpdate = false) {

        if (mUpdatesThisPhase.Count > 0) {
            var update = mUpdatesThisPhase.Dequeue();
            update.Type = cardMessageType;
            update.CardID = CardID;
            update.UniqueID = UniqueID;
            update.Amount = Amount != -1 ? Amount : update.Amount;
            update.FacilityPlayedOnType = facilityDroppedOnType != FacilityType.None ?
                                                facilityDroppedOnType : update.FacilityPlayedOnType;

            update.AdditionalFacilitySelectedOne = facilityType1 != FacilityType.None ?
                                                facilityType1 : update.AdditionalFacilitySelectedOne;

            update.AdditionalFacilitySelectedTwo = facilityType2 != FacilityType.None ?
                                                facilityType2 : update.AdditionalFacilitySelectedTwo;

            update.AdditionalFacilitySelectedThree = facilityType3 != FacilityType.None ?
                                                facilityType3 : update.AdditionalFacilitySelectedThree;

            update.FacilityEffectToRemoveType = effectTargetType != FacilityEffectType.None ?
                                                effectTargetType : update.FacilityEffectToRemoveType;

            Debug.Log($"updated most recent update: \n" +
                $"Effect To Remove: {update.FacilityEffectToRemoveType}\n" +
                $"Facility 1: {update.AdditionalFacilitySelectedOne}\n" +
                $"Facility 2: {update.AdditionalFacilitySelectedTwo}\n" +
                $"Facility 3: {update.AdditionalFacilitySelectedThree}");

            mUpdatesThisPhase.Enqueue(update);
            if (sendUpdate) {
                GameManager.Instance.SendUpdatesToOpponent(GameManager.Instance.MGamePhase, this);
            }
        }
        else {
            Debug.LogWarning("Tried to update a network message in queue but the queue was empty!");
        }

    }
    private void EnqueueCardMessageUpdate(CardMessageType cardMessageType, int CardID, int UniqueID, int Amount = -1, SectorType sectorType = SectorType.Any,
        FacilityType facilityType = FacilityType.None, FacilityEffectType facilityEffectToRemoveType = FacilityEffectType.None, bool sendUpdateImmediately = false) {
        mUpdatesThisPhase.Enqueue(new Update {
            Type = cardMessageType,
            UniqueID = UniqueID,
            CardID = CardID,
            Amount = Amount,
            sectorPlayedOn = sectorType,
            FacilityPlayedOnType = facilityType,
            FacilityEffectToRemoveType = facilityEffectToRemoveType
        });
        if (sendUpdateImmediately) {
            GameManager.Instance.SendUpdatesToOpponent(GameManager.Instance.MGamePhase, this);
        }
    }
    private void EnqueueAndSendCardMessageUpdate(CardMessageType cardMessageType, int CardID, int UniqueID, int Amount = -1, SectorType sectorType = SectorType.Any,
        FacilityType facilityType = FacilityType.None, FacilityEffectType facilityEffectToRemoveType = FacilityEffectType.None) {
        EnqueueCardMessageUpdate(cardMessageType, CardID, UniqueID, Amount, sectorType, facilityType, facilityEffectToRemoveType, true);
    }
    #endregion

    #region Receiving 
    //called by the game manager to add an update to the player's queue from the opponent's actions
    //THIS is the first place where card updates are passed to the player
    public void AddUpdateFromPlayer(Update update, GamePhase phase, CardPlayer otherPlayer) {

        switch (update.Type) {
            case CardMessageType.DrawCard:
                Debug.Log($"{playerName} received card draw from {otherPlayer.playerName} who drew {GetCardNameFromID(update.CardID)} with uid {update.UniqueID}");

                //draw cards for opponent but dont update network which would cause an infinite loop
                otherPlayer.DrawSpecificCard(update.CardID, UserInterface.Instance.opponentDropZone, uid: update.UniqueID, updateNetwork: false);
                break;
            //  case CardMessageType.CardUpdate:
            //   case CardMessageType.CardUpdateWithExtraFacilityInfo: 
            case CardMessageType.DiscardCard:
            case CardMessageType.ReduceCost:
                //if (IsAnimating) {
                //    // Debug.Log($"Queueing card update due to ongoing animation: {update.Type}, Facility: {update.FacilityType}");
                //    opponentCardPlays.Enqueue((update, phase, opponent));
                //    return;
                //}
                // If no animation is in progress, handle the card play immediately
                ProcessCardPlay(update, phase, otherPlayer);
                break;
            case CardMessageType.ReturnCardToDeck:
                Debug.Log($"{playerName} received return card to hand message from {otherPlayer.playerName}");
                HandleReturnCardToHandUpdate(update, otherPlayer);
                break;
            case CardMessageType.RemoveEffect:
                Debug.Log($"{playerName} received remove effect from {otherPlayer.playerName}");
                HandleRemoveEffectUpdate(update, otherPlayer);
                break;
        }
        UserInterface.Instance.UpdatePlayerMenuItems();
    }
    //handles the update type meaning that the opponent player returned one of their cards to the deck
    void HandleReturnCardToHandUpdate(Update update, CardPlayer opponent) {
        if (opponent.HandCards.TryGetValue(update.UniqueID, out GameObject cardGameObject)) {
            if (cardGameObject.TryGetComponent(out Card card)) {
                opponent.ReturnCardToDeck(card, false);
                UserInterface.Instance.UpdateUISizeTrackers();//update hand size ui
            }
        }
        else {
            Debug.LogError($"Failed to find card with uid {update.UniqueID} in {opponent.playerName}'s hand - did not pass messsage");
        }
    }
    void HandleRemoveEffectUpdate(Update update, CardPlayer opponent) {
        if (update.FacilityPlayedOnType != FacilityType.None) {
            if (ActiveFacilities.TryGetValue((int)update.FacilityPlayedOnType, out GameObject facilityGo)) {
                if (facilityGo.TryGetComponent(out Facility facility)) {
                    Debug.Log($"Looking to remove effect with type {update.FacilityEffectToRemoveType} from {facility.facilityName}");
                    if (update.FacilityEffectToRemoveType != FacilityEffectType.None) {
                        if (facility.effectManager.TryRemoveEffectByType(update.FacilityEffectToRemoveType)) {
                            Debug.Log($"Successfully removed {update.FacilityEffectToRemoveType} from {facility.name}");
                        }
                    }
                }
            }
        }
    }
    //Actually process the card action, used to create the update queue and is called when the update is ready to be resolved
    void ProcessCardPlay(Update update, GamePhase phase, CardPlayer opponent) {
        Debug.Log($"Player {playerName} is processing a card update from {opponent.playerName} of type {update.Type} on sector {update.sectorPlayedOn}");


        if (update.Type == CardMessageType.DiscardCard) {
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

    #endregion
    #region Network Helpers



    public CardMessageType GetNextUpdateInMessageFormat(ref List<int> playsForMessage, GamePhase phase) {
        if (mUpdatesThisPhase.Count > 0) {
            playsForMessage.Add((int)phase);
            Update update = mUpdatesThisPhase.Dequeue();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("~~Building message~~");
            sb.AppendLine($"type:{update.Type}|card id:{update.CardID}|unique id:{update.UniqueID}");


            playsForMessage.Add(update.UniqueID);
            playsForMessage.Add(update.CardID);
            playsForMessage.Add((int)update.sectorPlayedOn);
            playsForMessage.Add((int)update.FacilityPlayedOnType);
            playsForMessage.Add((int)update.FacilityEffectToRemoveType);
            if (update.Type == CardMessageType.ReduceCost) {
                playsForMessage.Add(update.Amount);
                sb.AppendLine($"Amount:{update.Amount}");
            }
            else if (update.Type == CardMessageType.RemoveEffect) {
                playsForMessage.Add((int)update.FacilityPlayedOnType);
                sb.AppendLine($"Remove Effect facility played type: {update.FacilityPlayedOnType}");
                // playsForMessage.Add((int)update.EffectTarget); // Uncomment if needed
            }
            else if (update.Type == CardMessageType.MeepleShare) {
                // UniqueID is the player to share with
                // CardID is the meeple color
                // Amount is the number of meeples to share
                playsForMessage.Add(update.Amount);
                sb.AppendLine($"Sharing meeple amount: {update.Amount}");
            }
            else if (update.Type == CardMessageType.CardUpdateWithExtraFacilityInfo) {
                sb.AppendLine("Additional facility types selected: ");
                sb.AppendLine($"\tFacility 1: {update.AdditionalFacilitySelectedOne}");
                sb.AppendLine($"\tFacility 2: {update.AdditionalFacilitySelectedTwo}");
                sb.AppendLine($"\tFacility 3: {update.AdditionalFacilitySelectedThree}");
                playsForMessage.Add((int)update.AdditionalFacilitySelectedOne);
                playsForMessage.Add((int)update.AdditionalFacilitySelectedTwo);
                playsForMessage.Add((int)update.AdditionalFacilitySelectedThree);
            }
            // Debug.Log(sb.ToString());
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

        // FacilityIDs.Clear();
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
        if (this != GameManager.Instance.actualPlayer) return;
        if (TryGetFacilityUnderMouse(out Facility facility)) {
            facility.LogFacilityDebug();
        }
    }

    void TryShowEffectSelectionMenu() {
        if (this != GameManager.Instance.actualPlayer) return;
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

    #region Overtime
    public void StartOvertime() {
        if (otState == OverTimeState.None && overTimeCharges > 0) {
            otState = OverTimeState.Overtime;
            overTimeCharges--;

            UserInterface.Instance.ToggleOvertimeButton(false);
            UserInterface.Instance.StartOvertime();
        }
        MultiplyMaxMeeples(2);
    }
    public void EndOvertime() {
        otState = OverTimeState.Exhausted;
        OverTimeCounter = 0;
        MultiplyMaxMeeples(.25f);
        UserInterface.Instance.StartExhaustion();
    }
    public void EndExhaustion() {
        otState = OverTimeState.None;
        OverTimeCounter = 0;
        MultiplyMaxMeeples(2);
    }
    public void MultiplyMaxMeeples(float multi) {
        maxMeeples = maxMeeples.Select(x => Mathf.Max((x * multi), 1)).ToArray();
        BlackMeeples = maxMeeples[0];
        BlueMeeples = maxMeeples[1];
        PurpleMeeples = maxMeeples[2];
        ColorlessMeeples = maxMeeples[3];
        UserInterface.Instance.UpdateMeepleAmountUI(maxMeeples[0], maxMeeples[1], maxMeeples[2], ColorlessMeeples);
    }
    #endregion
}
