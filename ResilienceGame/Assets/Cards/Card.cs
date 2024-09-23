using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Collections;
using System.Linq;
using UnityEditor.Rendering;
using System;

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

// Enum to indicate what the card is being played on
public enum CardTarget {
    Hand,
    Card,
    Effect,
    Facility,
    Sector
};

public struct CardIDInfo {
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
    private float timer = 0f;
    private const float timeBetweenPositionLogs = 1f;
    private RectTransform rectTransform;

    [Header("Animation")]
    // public bool isPaused = false;
    // public bool skipAnimation = false;
    public float speed = 10f;
    public float OpponentCardPlayAnimDuration = 1f;
    public float rotationDurationPercent = 0.3f; // Rotation happens over 20% of the total duration
    public float rotationDelayPercent = 0.35f;    // Rotation starts after 40% of the duration
    public float scaleUpFactor = 1.5f;            // Increases size by 50%
    public float waitTimeAtCenter = 1.5f;           // Waits for 1.5 seconds at the center
    public float shrinkDuration = 1.5f;             // Duration of the shrink and move animation
    private bool isAnimating = false;
    private bool skipCurrentAnimation = false;

    public int HandPosition { get; set; } = 0;


    // NOTE: this is a string currently because mitigations are for 
    // cards from the other player's deck.
    //public List<string> MitigatesWhatCards = new List<string>(10);
    Vector2 mDroppedPosition;
    // GameManager mManager; 
    public List<ICardAction> ActionList = new List<ICardAction>(6);

    // Start is called before the first frame update
    void Start() {
        originalPosition = this.gameObject.transform.position;
        rectTransform = GetComponent<RectTransform>();
        //mManager = GameObject.FindObjectOfType<GameManager>();
        OutlineImage.SetActive(false);
    }

    void Update() {
        if (state == CardState.CardInPlay) {
            if (isAnimating) {
                if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame) {
                    skipCurrentAnimation = true;
                }
            }
            //if (timer > timeBetweenPositionLogs) {
            //    Debug.Log($"Unique Card Id: {UniqueID}");
            //    Debug.Log($"World position: {transform.position}");
            //    Debug.Log($"Local Position: {transform.localPosition}");
            //    Debug.Log($"Anchored Position: {rectTransform.anchoredPosition}");
            //    timer = 0f;
            //}
            //else {
            //    timer += Time.deltaTime;
            //}
        }
    }


    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("click release on card");
        if (this.state == CardState.CardDrawn) {
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

    public bool OutlineActive() {
        return OutlineImage.activeSelf;
    }

    // we save the exact position of dropping so others can look at it
    public Vector2 getDroppedPosition() {
        return mDroppedPosition;
    }


    // Play all of a cards actions
    public void Play(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon = null, Card cardActedUpon = null) {
        Debug.Log($"Executing card actions for card: {front.name}");
        foreach (ICardAction action in ActionList) {
            action.Played(player, opponent, facilityActedUpon, cardActedUpon, this);
        }
    }

    // Cancel this card
    public void Cancel(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon = null, Card cardActedUpon = null) {
        foreach (ICardAction action in ActionList) {
            action.Canceled(player, opponent, facilityActedUpon, cardActedUpon, this);
        }
    }

    public void ToggleCardVisuals(bool enable) {
        transform.GetComponentsInChildren<RectTransform>().ToList().ForEach(child => child.gameObject.SetActive(enable));
    }
    public IEnumerator AnimateCardToPosition(Vector3 targetPosition, float duration, Action onComplete = null) {
        isAnimating = true;
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
            if (skipCurrentAnimation) {
                break;
            }
            yield return null;
        }

        // Call the callback function
        onComplete?.Invoke();

        // Disable or destroy the card
        Destroy(gameObject);
    }
    /// <summary>
    /// Moves the UI element to the center with rotation and scaling, waits, then shrinks and moves it to a target position,
    /// calls a callback function, and destroys the object.
    /// </summary>
    /// <param name="rectTransform">The RectTransform of the UI element to animate.</param>
    /// <param name="facilityPosition">The target position to move to after waiting at the center.</param>
    /// <param name="onComplete">Callback function to call before destroying the object.</param>
    public IEnumerator MoveAndRotateToCenter(RectTransform rectTransform, GameObject facilityTarget = null, Action onComplete = null) {
        isAnimating = true;
        // Initial and target positions
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 centerPosition = Vector2.zero; // Center of the screen

        // Initial and target rotations
        Quaternion startRotation = rectTransform.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, 180f);

        // Initial and target scales
        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = startScale * scaleUpFactor;

        // Rotation timing
        float rotationDuration = rotationDurationPercent * OpponentCardPlayAnimDuration;
        float rotationDelay = rotationDelayPercent * OpponentCardPlayAnimDuration;

        float elapsedTime = 0f;

        // First phase: Move to center, rotate, and scale up
        while (elapsedTime < OpponentCardPlayAnimDuration) {
            float t = elapsedTime / OpponentCardPlayAnimDuration;
            float easedT = CubicEaseInOut(t);

            // Update position and scale
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, centerPosition, easedT);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedT);

            // Handle rotation
            if (elapsedTime >= rotationDelay && elapsedTime <= rotationDelay + rotationDuration) {
                float rotationElapsed = elapsedTime - rotationDelay;
                float rotationT = rotationElapsed / rotationDuration;
                float easedRotationT = CubicEaseInOut(rotationT);

                rectTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, easedRotationT);
            }
            else if (elapsedTime > rotationDelay + rotationDuration) {
                rectTransform.localRotation = targetRotation;
            }

            elapsedTime += Time.deltaTime;

            if (skipCurrentAnimation) {

                break;
            }
            yield return null;
        }
        if (skipCurrentAnimation) {
            // Call the callback function
            onComplete?.Invoke();

            // Destroy the game object
            Destroy(rectTransform.gameObject);
        }
        else {
            // Ensure final position, rotation, and scale are set
            rectTransform.anchoredPosition = centerPosition;
            rectTransform.localRotation = targetRotation;
            rectTransform.localScale = targetScale;

            // Wait at the center
            yield return new WaitForSeconds(waitTimeAtCenter);

            Vector2 endPosition = centerPosition; // Default to center if facilityTarget is null
            if (facilityTarget != null) {
                // Calculate the facility's position relative to the Canvas
                endPosition = facilityTarget.transform.position;
                // Debug.Log($"End Position (UI Local): {endPosition}");
            }

            Vector3 endScale = Vector3.zero; // Scale down to zero
            Vector3 sPos = transform.position;
            // Debug.Log($"Moving to facility target: {endPosition}");
            elapsedTime = 0f;

            while (elapsedTime < shrinkDuration) {
                float t = elapsedTime / shrinkDuration;
                float easedT = CubicEaseInOut(t);

                // Update position and scale
                rectTransform.position = Vector2.Lerp(sPos, endPosition, easedT);
                rectTransform.localScale = Vector3.Lerp(targetScale, endScale, easedT);

                elapsedTime += Time.deltaTime;

                if (skipCurrentAnimation) {
                    break;
                }
                yield return null;
            }

            // Call the callback function
            onComplete?.Invoke();

            // Destroy the game object
            Destroy(rectTransform.gameObject);
        }
    }

    /// <summary>
    /// Cubic easing in/out function.
    /// </summary>
    /// <param name="t">Normalized time (0 to 1).</param>
    /// <returns>Eased value.</returns>
    private float CubicEaseInOut(float t) {
        if (t < 0.5f)
            return 4f * t * t * t;
        else {
            float f = (2f * t) - 2f;
            return 0.5f * f * f * f + 1f;
        }
    }
    //private Vector2 GetUIPositionRelativeToCanvas(Transform targetTransform) {
    //    Vector2 position = Vector2.zero;
    //    Transform current = targetTransform;

    //    // Loop until we reach the Canvas or there are no more parents
    //    while (current != null) {
    //        RectTransform rectTransform = current.GetComponent<RectTransform>();
    //        if (rectTransform != null) {
    //            position += rectTransform.anchoredPosition;
    //        }
    //        else {
    //            // If there's no RectTransform, check for localPosition (for non-UI elements)
    //            position += new Vector2(current.localPosition.x, current.localPosition.y);
    //        }

    //        // Check if we've reached the Canvas
    //        if (current.GetComponent<Canvas>() != null) {
    //            break;
    //        }

    //        current = current.parent;
    //    }

    //    return position;
    //}


    public void OnPointerEnter(PointerEventData eventData) {
        // isPaused = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        //  isPaused = false;
    }

    void OnMouseDown() {
        // skipAnimation = true;
    }
}