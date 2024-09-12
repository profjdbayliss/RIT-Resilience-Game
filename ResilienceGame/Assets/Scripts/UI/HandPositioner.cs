using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

#region Events

// Custom UnityEvent that passes a Card object
[System.Serializable]
public class CardHoverEvent : UnityEvent<Card> { }

#endregion
/// <summary>
/// This class is responsible for arranging the cards in the player's hand.
/// </summary>
public class HandPositioner : MonoBehaviour {

    private CardHoverEvent onCardHover = new CardHoverEvent();
    public List<GameObject> cards = new List<GameObject>();
    public float arcRadius = 3300f;     //the size of the arc/curve the cards will be placed on
    public float arcAngle = 20f;
    public float cardAngle = 5f;
    private float hoverHeight = 150f;   //how far to move the card up when hovered over
    public float hoverScale = 1f;       //scale of the cards when hovered over
    public float defaultScale = .5f;    //default scale of the cards when in the hand
    public float cardWidth = 100f;      //approximate width of a card
    public float hoverTransitionSpeed = 10f; // transition speed of hover animation faster is faster

    private RectTransform rect; //the rect transform of the hand positioner

    //TODO: unknown if there is a real max cards in game rules but there should be for technical reasons if nothing else
    private const int MIN_CARDS = 3;
    private const int MAX_CARDS = 10;


    private GameObject currentHoveredCard;
    public bool IsDraggingCard { get; private set; } = false;
    private HashSet<GameObject> cardsBeingDragged = new HashSet<GameObject>();

    private void Start() {
        rect = GetComponent<RectTransform>();

    }


    /// <summary>
    /// Tells the hand positioner that a card is being dragged
    /// </summary>
    /// <param name="card">The game object of the card being dragged</param>
    public void NotifyCardDragStart(GameObject card) {
        cardsBeingDragged.Add(card);
        IsDraggingCard = true;
        card.transform.localRotation = Quaternion.identity;
        if (currentHoveredCard == card) {
            currentHoveredCard = null;
        }
    }

    /// <summary>
    /// Tells the hand positioner that the card was dropped after being dragged
    /// </summary>
    /// <param name="card">The card that was dropped</param>
    public void NotifyCardDragEnd(GameObject card) {
        cardsBeingDragged.Remove(card);
        IsDraggingCard = false;
        //card was played somewhere, so we need to do something with it
        var dropLoc = GameManager.instance.actualPlayer.hoveredDropLocation;
        if (dropLoc) {
           // Debug.Log($"card was played on: {dropLoc.name}");
            
        }
        else {
            //reset scale and reset sibling index to position it correctly in the hand
            card.transform.localScale = Vector3.one * defaultScale;
            var tCard  = card.GetComponent<Card>();
           // Debug.Log($"returning {tCard.data.front.title} to position {tCard.HandPosition}");
            card.transform.SetSiblingIndex(tCard.HandPosition);
            ArrangeCards(); // Rearrange cards when dragging ends
        }
    }

    public void DiscardCard(GameObject card) {
        cards.Remove(card);
      //  Destroy(card);
    }



    private void Update() {
        HandleNewCards();
        ArrangeCards();
        HandleHovering();
    }

    /// <summary>
    /// handles adding and removing cards from this class's card tracker (cards list)
    /// </summary>
    private void HandleNewCards() {
        //get all the child cards of the hand positioner
        var newCards = transform.GetComponentsInChildren<Card>().Select(card => card.gameObject).ToList();

        //filter the cards list and cardsToAdd to create exlusively new cards and cards to remove
        var cardsToAdd = newCards.Except(cards).ToList();
        var cardsToRemove = cards.Except(newCards).ToList();

        //set the scale of the new cards to the default scale
        cardsToAdd.ForEach(card => {
            card.transform.localScale = Vector3.one * defaultScale;

        });

        //If there are new cards or cards to remove, update the cards list
        if (cardsToAdd.Any()) {
            cards.AddRange(cardsToAdd);
            for (int x = 0; x < cards.Count; x++) {
                cards[x].GetComponent<Card>().HandPosition = x;
            }
        }

        if (cardsToRemove.Any()) {
            cards.RemoveAll(card => cardsToRemove.Contains(card));
            for (int x = 0; x < cards.Count; x++) {
                cards[x].GetComponent<Card>().HandPosition = x;
            }
        }


    }

    //handles arranging the cards in the hand by fanning them out in an arc
    //spreads the cards out in the x direction to fill the hand position rect
    private void ArrangeCards() {
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        // Calculate the angle step based on the number of cards
        float currentArcAngle = Mathf.Min(arcAngle, Mathf.Max(0, (cardCount - MIN_CARDS) * 5f));
        float angleStep = currentArcAngle / (cardCount - 1);
        if (cardCount <= MIN_CARDS) angleStep = 0;      //3 cards or less should be in a straight line

        //calculate the overlap factor using the max allowed width (hand positioner size) and the total width of the cards
        float startAngle = -currentArcAngle / 2f;
        float maxScreenWidth = rect.rect.width;
        float totalCardWidth = cardWidth * cardCount;

        float overlapFactor = 1f;
        if (totalCardWidth > maxScreenWidth) {
            overlapFactor = maxScreenWidth / totalCardWidth;
        }

        float horizontalSpacing = cardWidth * overlapFactor;

        // Loop through all the cards and position them
        for (int i = 0; i < cardCount; i++) {
            GameObject card = cards[i];
            // Skip cards that are being dragged
            if (cardsBeingDragged.Contains(card)) continue;

            //calculate the angle and x position of the card
            float angle = startAngle + (i * angleStep);
            float x = (i - (cardCount - 1) / 2f) * horizontalSpacing;
            //calculate the y position of the card
            float baseY = Mathf.Cos(angle * Mathf.Deg2Rad) * arcRadius - arcRadius;

            Vector3 targetPosition = new Vector3(x, baseY, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);

            //if the card is the current hover card, rotate it to straight and push it up a bit
            if (card == currentHoveredCard) {
                targetPosition.y += hoverHeight;
                targetRotation = Quaternion.identity;
            }

            // Smooth position and rotation transition
            card.transform.SetLocalPositionAndRotation(
                Vector3.Lerp(
                    card.transform.localPosition, 
                    targetPosition, 
                    Time.deltaTime * hoverTransitionSpeed),
                Quaternion.Slerp(
                    card.transform.localRotation, 
                    targetRotation, 
                    Time.deltaTime * hoverTransitionSpeed));

            // Smooth scale transition
            float targetScale = card == currentHoveredCard ? hoverScale : defaultScale;
            card.transform.localScale = Vector3.Lerp(card.transform.localScale, Vector3.one * targetScale, Time.deltaTime * hoverTransitionSpeed);
        }
    }

    //Allows other classes to subscribe to the onCardHover event
    public void AddCardHoverListener(UnityAction<Card> listener) {
        onCardHover.AddListener(listener);
    }
    /// <summary>
    /// Handles determining which card is being hovered over
    /// </summary>
    private void HandleHovering() {
        GameObject newHoveredCard = null;

        // If no cards are being dragged, check which card is under the mouse
        if (cardsBeingDragged.Count == 0) {
            Vector2 localMousePosition = rect.InverseTransformPoint(Mouse.current.position.ReadValue());
            newHoveredCard = GetCardUnderMouse(localMousePosition.x);
        }

        if (newHoveredCard != currentHoveredCard) {
            currentHoveredCard = newHoveredCard;

            if (currentHoveredCard != null) {
                currentHoveredCard.transform.SetAsLastSibling();
            }
            else {
                ResetCardSiblingIndices();
            }

            onCardHover.Invoke(currentHoveredCard?.GetComponent<Card>());
        }
    }

    private void ResetCardSiblingIndices() {
        foreach (var card in cards) {
            card.transform.SetSiblingIndex(card.GetComponent<Card>().HandPosition);
        }
    }

    /// <summary>
    /// Determines if the mouse is inside of the hand positioner rect
    /// </summary>
    /// <returns>True if the mouse is inside of the hand positioner</returns>
    private bool IsMouseInsideRect() {
        // Get the mouse position in screen space
        Vector2 mousePositionScreen = Mouse.current.position.ReadValue();

        // Convert screen space to local space of the RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePositionScreen, null, out Vector2 localPoint);

        // Check if the local point is inside the rect
        return rect.rect.Contains(localPoint);
    }

    /// <summary>
    /// Gets the card under the mouse, this is done by finding the card with the closest x position to the mouse to provide a seemless transition between selecting neighboring cards
    /// </summary>
    /// <param name="mouseX">The local x position of the mouse</param>
    /// <returns>A game object, the card in the hand that is closest the the x position of the mouse</returns>
    private GameObject GetCardUnderMouse(float mouseX) {

        if (!IsMouseInsideRect()) return null; //dont hover if the mouse is outside of the hand area

        float minDistance = float.MaxValue;
        GameObject closestCard = null;

        foreach (GameObject card in cards) {
            float cardX = card.transform.localPosition.x;
            float distance = Mathf.Abs(mouseX - cardX);

            if (distance < minDistance) {
                minDistance = distance;
                closestCard = card;
            }
        }

        return closestCard;
    }

}