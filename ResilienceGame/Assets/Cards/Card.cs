using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Collections;
using System.Linq;
using TMPro;
using UnityEngine.Animations;

// Enum to track the state of the card
public enum CardState {
    NotInDeck,
    CardInDeck,
    CardDrawn,
    CardDrawnDropped,
    CardInPlay,
    CardNeedsToBeDiscarded,
    CardDiscarded,
};



public class Card : MonoBehaviour, IPointerClickHandler {
    public CardData data;
    // this card needs a unique id since multiples of the same card can be played
    public int UniqueID;

    public CardState state;
    public CardTarget dropTarget; //TODO: might not use this
    public string DeckName;
    public GameObject cardZone;
    public GameObject originalParent;
    public Vector3 originalPosition;
    public GameObject CanvasHolder;
    public bool HasCanvas = false;
    public int stackNumber = 0;
    public GameObject OutlineImage;
    public int DefenseHealth = 0;
    public List<int> ModifyingCards = new List<int>(10);
    public List<CardIDInfo> AttackingCards = new List<CardIDInfo>(10);

    
    public int HandPosition { get; set; } = 0;

   // public int serializedHandPosition = 0;

    // NOTE: this is a string currently because mitigations are for 
    // cards from the other player's deck.
    //public List<string> MitigatesWhatCards = new List<string>(10);
    Vector2 mDroppedPosition;
    // GameManager mManager; 
    //    public List<ICardAction> ActionList = new List<ICardAction>(6);
    public List<string> ActionList = new List<string>();
    // Start is called before the first frame update
    void Start() {
        originalPosition = this.gameObject.transform.position;
        //    mManager = GameObject.FindObjectOfType<GameManager>();
        OutlineImage.SetActive(false);
    }
    //void Update() {
    //    serializedHandPosition = HandPosition;
    //}

    public void InitializeFromCard(Card sourceCard, GameObject dropZone, int uniqueId, Transform parent = null) {
        this.cardZone = dropZone;
        this.data = sourceCard.data;
        this.UniqueID = uniqueId;
        Debug.Log($"Setting unique id for card {this.UniqueID}");

        // Deep copy all other properties
        this.state = sourceCard.state;
        this.dropTarget = sourceCard.dropTarget;
        this.DeckName = sourceCard.DeckName;
        this.originalParent = sourceCard.originalParent;
        this.originalPosition = sourceCard.originalPosition;
        this.CanvasHolder = sourceCard.CanvasHolder;
        this.HasCanvas = sourceCard.HasCanvas;
        this.stackNumber = sourceCard.stackNumber;
        this.DefenseHealth = sourceCard.DefenseHealth;
       

        // Deep copy lists
        this.ModifyingCards = new List<int>(sourceCard.ModifyingCards);
        this.AttackingCards = new List<CardIDInfo>(sourceCard.AttackingCards);
        this.ActionList = new List<string>(sourceCard.ActionList);

        this.originalPosition = this.transform.position;
    }

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("click happened on card");
        if (this.state == CardState.CardDrawn) {
            // note that click consumes the release of most drag and release motions
            Debug.Log("potentially card dropped.");
            state = CardState.CardDrawnDropped;
            mDroppedPosition = new Vector2(this.transform.position.x, this.transform.position.y);
        }
        // TODO: Update or remove
        /*
        else if (this.data.cardType == CardType.Station && mManager.CanStationsBeHighlighted())
        {
            // only station type cards can be highlighted and played on
            // for this game
            Debug.Log("right card type and phase for highlight");
            if (OutlineImage.activeSelf)
            {
                // turn off activation
                OutlineImage.SetActive(false);
            }
            else
            {
                OutlineImage.SetActive(true);
            }
        }*/
    }

    public bool OutlineActive() {
        return OutlineImage.activeSelf;
    }

    // we save the exact position of dropping so others can look at it
    public Vector2 GetDroppedPosition() {
        return mDroppedPosition;
    }


    // Play all of a cards actions
    public void Play(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon = null, Card cardActedUpon = null) {
        //foreach (ICardAction action in ActionList) {
        //    action.Played(player, opponent, facilityActedUpon, cardActedUpon, this);
        //}
        ActionList.ForEach(action => CardActionManager.Instance.ExecuteCardAction(action, player, opponent, facilityActedUpon, cardActedUpon, this));
    }

    // Cancel this card
    public void Cancel(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon = null, Card cardActedUpon = null) {
        //TODO: implement action canceling
        //currently it only did print statements but this might be needed later

        //foreach (ICardAction action in ActionList) {
        //    action.Canceled(player, opponent, facilityActedUpon, cardActedUpon, this);
        //}
    }

    #region Init Card Visuals
    public void SetupCardVisuals() {
        SetupRawImages();
        SetupColoredCircles();
        SetupTextElements();
    }

    private void SetupRawImages() {
        RawImage[] tempRaws = GetComponentsInChildren<RawImage>();
        foreach (var raw in tempRaws) {
            if (raw.name == "Image") {
                raw.texture = this.data.front.img;
            }
            else if (raw.name == "Background") {
                raw.color = this.data.front.color;
            }
        }
    }

    private void SetupColoredCircles() {
        Image[] tempImages = GetComponentsInChildren<Image>();
        foreach (var img in tempImages) {
            switch (img.name) {
                case "BlackCardSlot":
                    img.enabled = this.data.front.blackCircle;
                    break;
                case "BlueCardSlot":
                    img.enabled = this.data.front.blueCircle;
                    break;
                case "PurpleCardSlot":
                    img.enabled = this.data.front.purpleCircle;
                    break;
            }
        }
    }

    private void SetupTextElements() {
        TextMeshProUGUI[] tempTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in tempTexts) {
            switch (text.name) {
                case "Title Text":
                    text.text = this.data.front.title;
                    break;
                case "Description Text":
                    text.text = this.data.front.description;
                    break;
                case "Flavor Text":
                    text.text = this.data.front.flavor;
                    break;
                case "BlackCardNumber":
                    SetupCostText(text, this.data.front.blackCircle, this.data.meepleCost[MeepleType.Black]);
                    break;
                case "BlueCardNumber":
                    SetupCostText(text, this.data.front.blueCircle, this.data.meepleCost[MeepleType.Blue]);
                    break;
                case "PurpleCardNumber":
                    SetupCostText(text, this.data.front.purpleCircle, this.data.meepleCost[MeepleType.Purple]);
                    break;
            }
        }
    }

    private void SetupCostText(TextMeshProUGUI text, bool isEnabled, int cost) {
        text.enabled = isEnabled;
        if (isEnabled) {
            text.text = cost.ToString();
        }
    }

    public void ToggleCardVisuals(bool enable) {
        transform.GetComponentsInChildren<RectTransform>().ToList().ForEach(child => child.gameObject.SetActive(enable));
    }
    #endregion


}