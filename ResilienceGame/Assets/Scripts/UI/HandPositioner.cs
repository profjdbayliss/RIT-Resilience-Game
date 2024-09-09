using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandPositioner : MonoBehaviour {
    public List<GameObject> cards = new List<GameObject>();
    public float arcRadius = 3300f;
    public float arcAngle = 20f;
    public float cardAngle = 5f;
    private float hoverHeight = 150f;
    public float hoverScale = 1f;
    public float defaultScale = .5f;
    public float cardWidth = 100f;
    public float hoverTransitionSpeed = 10f; // New field for smooth transitions

    public RectTransform hoverParent;
    private RectTransform rect;



    private const int MIN_CARDS = 3;
    private const int MAX_CARDS = 10;

    private GameObject currentHoveredCard;
    private bool isDraggingCard = false;
    private HashSet<GameObject> cardsBeingDragged = new HashSet<GameObject>();

    private void Start() {
        rect = GetComponent<RectTransform>();

    }



    public void NotifyCardDragStart(GameObject card) {
        cardsBeingDragged.Add(card);
      //  card.transform.localScale = Vector3.one * defaultScale;
        card.transform.localRotation = Quaternion.identity;
        if (currentHoveredCard == card) {
            currentHoveredCard = null;
        }
    }

    public void NotifyCardDragEnd(GameObject card) {
        cardsBeingDragged.Remove(card);
        card.transform.localScale = Vector3.one * defaultScale;
        card.transform.SetSiblingIndex(card.GetComponent<Card>().HandPosition);
        ArrangeCards(); // Rearrange cards when dragging ends
    }



    private void Update() {
        HandleNewCards();
        ArrangeCards();
        HandleHovering();
    }

    //handles adding and removing cards from this class's card tracker (cards list)
    private void HandleNewCards() {
        var newCards = transform.GetComponentsInChildren<Card>().Select(card => card.gameObject).ToList();

        var cardsToAdd = newCards.Except(cards).ToList();
        var cardsToRemove = cards.Except(newCards).ToList();

        cardsToAdd.ForEach(card => {
            card.transform.localScale = Vector3.one * defaultScale;

        });


        if (cardsToAdd.Any()) {
            Debug.Log("Found new card, adding");
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
    //private void HandleCardClick() {
    //    if (Mouse.current.leftButton.wasPressedThisFrame) {
    //        var card = GetCardUnderMouse(Mouse.current.position.ReadValue().x);
    //        if (card) {
    //            isDraggingCard = true;
    //        }
    //    }
    //    else if (Mouse.current.leftButton.wasReleasedThisFrame) {
    //        isDraggingCard = false;
    //    }
    //}

    //handles arranging the cards in the hand by fanning them out in an arc
    //spreads the cards out in the x direction to fill the hand position rect
    private void ArrangeCards() {
        int cardCount = cards.Count;
        if (cardCount == 0) return;

        float currentArcAngle = Mathf.Min(arcAngle, Mathf.Max(0, (cardCount - MIN_CARDS) * 5f));
        float angleStep = currentArcAngle / (cardCount - 1);
        if (cardCount <= MIN_CARDS) angleStep = 0;

        float startAngle = -currentArcAngle / 2f;
        float maxScreenWidth = rect.rect.width;
        float totalCardWidth = cardWidth * cardCount;

        float overlapFactor = 1f;
        if (totalCardWidth > maxScreenWidth) {
            overlapFactor = maxScreenWidth / totalCardWidth;
        }

        float horizontalSpacing = cardWidth * overlapFactor;

        for (int i = 0; i < cardCount; i++) {
            GameObject card = cards[i];

            if (cardsBeingDragged.Contains(card)) continue;

            float angle = startAngle + (i * angleStep);
            float x = (i - (cardCount - 1) / 2f) * horizontalSpacing;
            float baseY = Mathf.Cos(angle * Mathf.Deg2Rad) * arcRadius - arcRadius;

            Vector3 targetPosition = new Vector3(x, baseY, 0);
            Quaternion targetRotation = Quaternion.Euler(0, 0, -angle);

            if (card == currentHoveredCard) {
                targetPosition.y += hoverHeight;
                targetRotation = Quaternion.identity;
            }

            // Smooth transition
            card.transform.localPosition = Vector3.Lerp(card.transform.localPosition, targetPosition, Time.deltaTime * hoverTransitionSpeed);
            card.transform.localRotation = Quaternion.Slerp(card.transform.localRotation, targetRotation, Time.deltaTime * hoverTransitionSpeed);

            // Smooth scale transition
            float targetScale = card == currentHoveredCard ? hoverScale : defaultScale;
            card.transform.localScale = Vector3.Lerp(card.transform.localScale, Vector3.one * targetScale, Time.deltaTime * hoverTransitionSpeed);
        }
    }

    private void HandleHovering() {
        if (cardsBeingDragged.Count > 0) {
            currentHoveredCard = null;
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 localMousePosition = rect.InverseTransformPoint(mousePosition);

        GameObject hoveredCard = GetCardUnderMouse(localMousePosition.x);

        if (hoveredCard != currentHoveredCard) {
            currentHoveredCard = hoveredCard;

            if (currentHoveredCard != null) {
                currentHoveredCard.transform.SetAsLastSibling();
            }
        }
    }
    private bool IsMouseInsideRect() {
        // Get the mouse position in screen space
        Vector2 mousePositionScreen = Mouse.current.position.ReadValue();

        // Convert screen space to local space of the RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, mousePositionScreen, null, out Vector2 localPoint);

        // Check if the local point is inside the rect
        return rect.rect.Contains(localPoint);
    }

    private GameObject GetCardUnderMouse(float mouseX) {



        if (!IsMouseInsideRect()) return null;

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

        // You might want to add a threshold here to prevent hovering when the mouse is too far from any card
        return closestCard;
    }

    //private void HoverCard(GameObject card) {
    //    if (cardsBeingDragged.Contains(card)) return;
    //    card.transform.localPosition += Vector3.up * hoverHeight;
    //    card.transform.localScale = Vector3.one * hoverScale;
    //    card.transform.localRotation = Quaternion.identity;
    //}

    //private void ResetCardTransform(GameObject card) {
    //    if (cardsBeingDragged.Contains(card)) return;
    //    int index = cards.IndexOf(card);
    //    if (index != -1) {
    //        float angle = -arcAngle / 2f + (index * (arcAngle / (cards.Count - 1)));
    //        card.transform.localScale = Vector3.one * defaultScale;
    //        card.transform.localRotation = Quaternion.Euler(0, 0, -angle);
    //        // Restore the original y position (you might need to store this separately if it varies)
    //        Vector3 position = card.transform.localPosition;
    //        position.y -= hoverHeight;
    //        card.transform.localPosition = position;
    //        card.transform.SetSiblingIndex(card.GetComponent<Card>().HandPosition);
    //    }
    //}
}