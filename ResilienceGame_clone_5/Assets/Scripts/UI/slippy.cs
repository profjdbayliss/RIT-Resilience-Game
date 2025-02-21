using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class slippy : MonoBehaviour, IDragHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler {
    public GameObject gameCanvas;
    public GameObject DraggableObject;
    public float maxScale;
    public float minScale;
    public Vector2 originalScale;
    public Vector3 originalPosition;
    public InputAction resetScale;
    public InputAction resetPosition;
    public PlayerInput playerInput;
    //public Vector2 hoverScale = new(1,1);
    public Vector2 dragScale = new(.5f, .5f);
    public float dragSmoothTime = 0.1f; // Smoothing time for drag movement

    private HandPositioner handPositioner;
    private float scaleDuration = 0.1f;
    private Coroutine scaleCoroutine;
    private Vector3 velocity = Vector3.zero; // Used for SmoothDamp
    private Vector2 lastMousePosition;

    public bool IsBeingDragged { get; private set; } = false;

    // Initialization
    void Start() {
        originalScale = new Vector2(0.5f, 0.5f);
        originalPosition = this.gameObject.transform.position;

        gameCanvas = GameObject.Find("GameCanvas");
        handPositioner = GetComponentInParent<HandPositioner>();
        ResetScale();
    }

    // Update is called once per frame
    void Update() {
        if (resetScale.WasPressedThisFrame()) {
            ResetScale();
        }

        // Enforce scale limits
        EnforceScaleLimits();

        if (IsBeingDragged) {
            UpdateCardPosition();
        }
    }

    // Enforces the minimum and maximum scale of the draggable object
    private void EnforceScaleLimits() {
        Vector2 tempScale = DraggableObject.transform.localScale;
        tempScale.x = Mathf.Clamp(tempScale.x, minScale, maxScale);
        tempScale.y = Mathf.Clamp(tempScale.y, minScale, maxScale);
        DraggableObject.transform.localScale = tempScale;
    }

    // Updates the card position with smooth damping
    private void UpdateCardPosition() {
        Vector2 targetPosition = lastMousePosition;
        DraggableObject.transform.position = Vector3.SmoothDamp(
            DraggableObject.transform.position,
            targetPosition,
            ref velocity,
            dragSmoothTime
        );
    }

    // Handles scrolling to zoom in/out
    public void OnScroll(PointerEventData pointer) {
        Vector2 tempScale = DraggableObject.transform.localScale;
        float scaleDelta = pointer.scrollDelta.y > 0 ? 0.05f : -0.05f;
        tempScale += new Vector2(scaleDelta, scaleDelta);
        DraggableObject.transform.localScale = tempScale;
        EnforceScaleLimits();
    }

    // Handles dragging of the object
    public void OnDrag(PointerEventData eventData) {
        if (DraggableObject.activeSelf && IsBeingDragged) {
            lastMousePosition = eventData.position;
        }
    }

    // Updates the original position of the object
    public void UpdatePosition() {
        originalPosition = gameObject.transform.position;
    }

    // Updates the original scale of the object
    public void UpdateScale() {
        originalPosition = this.gameObject.transform.localScale;
    }

    // Resets the scale of the object to its original scale
    public void ResetScale() {
        Transform parent = this.gameObject.transform.parent;
        this.gameObject.transform.SetParent(null, true);
        this.gameObject.transform.localScale = originalScale;
        this.gameObject.transform.SetParent(parent, true);
    }

    // Resets the position of the object to its original position
    public void ResetPosition() {
        Transform parent = this.gameObject.transform.parent;
        this.gameObject.transform.SetParent(null, true);
        this.gameObject.transform.SetPositionAndRotation(new Vector3(), gameObject.transform.rotation);
        this.gameObject.transform.SetParent(parent, true);
    }

    // Handles the start of dragging
    public void OnBeginDrag(PointerEventData eventData) {
        IsBeingDragged = true;
        lastMousePosition = eventData.position;
        if (handPositioner != null) {
            handPositioner.NotifyCardDragStart(gameObject);
        }

        UpdateCardPosition();
        ScaleTo(dragScale);
    }

    // Handles the end of dragging
    public void OnEndDrag(PointerEventData eventData) {
        IsBeingDragged = false;

        if (GameObject.FindWithTag("GameController").TryGetComponent(out GameManager gameManager)) {
            if (handPositioner != null) {
                gameManager.actualPlayer.HandleCardDrop(GetComponent<Card>());
                handPositioner.NotifyCardDragEnd(gameObject);
            }
        }
    }

    // Scales the object to a target scale
    private void ScaleTo(Vector2 targetScale) {
        if (scaleCoroutine != null) {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
        transform.localScale = targetScale;
    }

    // Coroutine for smooth scaling
    private IEnumerator ScaleCoroutine(Vector2 targetScale) {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration) {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}