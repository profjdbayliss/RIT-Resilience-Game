using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Collections;
using System.Linq;

// Enum to track the state of the card
public enum CardState
{
    NotInDeck,
    CardInDeck,
    CardDrawn,
    CardDrawnDropped,
    CardInPlay,
    CardNeedsToBeDiscarded,
    CardDiscarded,
};

// Enum to indicate what the card is being played on
public enum CardTarget
{
    Hand,
    Card,
    Effect,
    Facility,
    Sector
};

public struct CardIDInfo
{
    public int UniqueID;
    public int CardID;
};

public class Card : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    public CardData data;
    // this card needs a unique id since multiples of the same card can be played
    public int UniqueID; 
    public CardFront front;
    public CardState state;
    public CardTarget target;
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

    [Header("Animation")]
    public bool isPaused = false;
    public bool skipAnimation = false;

    public int HandPosition { get; set; } = 0;


    // NOTE: this is a string currently because mitigations are for 
    // cards from the other player's deck.
    //public List<string> MitigatesWhatCards = new List<string>(10);
    Vector2 mDroppedPosition;
   // GameManager mManager; 
    public List<ICardAction> ActionList = new List<ICardAction>(6);

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = this.gameObject.transform.position;
        //mManager = GameObject.FindObjectOfType<GameManager>();
        OutlineImage.SetActive(false);
    }
    
    void Update() {
        if (state == CardState.CardInPlay) {
            Debug.Log($"World position: {transform.position}");
            Debug.Log($"Local Position: {transform.localPosition}");
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click release on card");
        if (this.state == CardState.CardDrawn)
        {
            // note that click consumes the release of most drag and release motions
            //Debug.Log("potentially card dropped.");
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

    public bool OutlineActive()
    {
        return OutlineImage.activeSelf;
    }

    // we save the exact position of dropping so others can look at it
    public Vector2 getDroppedPosition() {
        return mDroppedPosition;
    }


    // Play all of a cards actions
    public void Play(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon = null, Card cardActedUpon = null)
    {
        Debug.Log($"Executing card actions for card: {front.name}");
        foreach(ICardAction action in ActionList)
        {
            action.Played(player, opponent, facilityActedUpon, cardActedUpon, this);
        }
    }

    // Cancel this card
    public void Cancel(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon = null, Card cardActedUpon = null)
    {
        foreach (ICardAction action in ActionList)
        {
            action.Canceled(player, opponent, facilityActedUpon, cardActedUpon, this);
        }
    }

    public void ToggleCardVisuals(bool enable) {
        transform.GetComponentsInChildren<RectTransform>().ToList().ForEach(child => child.gameObject.SetActive(enable));
    }
    public IEnumerator AnimateCardToFacility(Vector3 targetPosition, float duration) {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = targetPosition;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero; // Scale down to zero
        float elapsed = 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth step interpolation
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        // Ensure final position and scale are set
        transform.position = endPosition;
        transform.localScale = endScale;

        // Disable or destroy the card
        gameObject.SetActive(false);
    }
    public IEnumerator AnimateOpponentCard(Vector3 startPosition, Vector3 facilityPosition) {
        

        // Calculate center position
        Vector3 centerPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
        centerPosition.z = 0;

        Vector3 endScale = Vector3.one; // Normal scale
        float duration = 1.0f;
        float elapsed = 0f;

        // Debug logs
        Debug.Log($"Start Position: {startPosition}");
        Debug.Log($"Center Position: {centerPosition}");
        Debug.Log($"End Position: {facilityPosition}");

        // Animate to center and scale up
        while (elapsed < duration) {
            if (skipAnimation) break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPosition, centerPosition, t);
            transform.localScale = Vector3.Lerp(Vector3.zero, endScale, t);
            yield return null;
        }

        if (skipAnimation) {
            transform.position = centerPosition;
            transform.localScale = endScale;
        }

        // Wait for 2 seconds or until skipped
        float waitTime = 2.0f;
        float waitElapsed = 0f;
        while (waitElapsed < waitTime) {
            if (skipAnimation) break;
            if (!isPaused) waitElapsed += Time.deltaTime;
            yield return null;
        }

        // Animate to facility and scale down
        elapsed = 0f;
        while (elapsed < duration) {
            if (skipAnimation) break;
            if (!isPaused) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.SmoothStep(0f, 1f, t);
                transform.position = Vector3.Lerp(centerPosition, facilityPosition, t);
                transform.localScale = Vector3.Lerp(endScale, Vector3.zero, t);
            }
            yield return null;
        }

        transform.position = facilityPosition;
        transform.localScale = Vector3.zero;

        // Disable or destroy the card
        gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isPaused = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isPaused = false;
    }

    void OnMouseEnter() {
        
    }

    void OnMouseExit() {
        
    }

    void OnMouseDown() {
        skipAnimation = true;
    }
}