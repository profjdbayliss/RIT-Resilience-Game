using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class slippy : MonoBehaviour, IDragHandler, IScrollHandler, IBeginDragHandler, IEndDragHandler {
    public GameObject gameCanvas;

   // public Camera cam;

    public GameObject DraggableObject;

    public float maxScale;

    public float minScale;

    public Vector2 originalScale;
    private Vector2 dragOffset;
    public Vector3 originalPosition;

    public InputAction resetScale;

    public InputAction resetPosition;

    public PlayerInput playerInput;

    private Vector2 mOffsetPos;

    private HandPositioner handPositioner;

    public bool IsBeingDragged { get; private set; } = false;

    public Vector2 hoverScale = new(1,1);
    public Vector2 dragScale = new(.5f, .5f); 
    private float scaleDuration = 0.1f;
    public float dragSmoothTime = 0.1f; // Smoothing time for drag movement
    private Coroutine scaleCoroutine;
    private Vector3 velocity = Vector3.zero; // Used for SmoothDamp
    private Vector2 lastMousePosition;


    // Start is called before the first frame update
    void Start() {
        originalScale = new Vector2(0.5f, 0.5f);
        originalPosition = this.gameObject.transform.position;
        
        gameCanvas = GameObject.Find("GameCanvas");
        // initial offset is always zero
        mOffsetPos = new Vector2();
        handPositioner = GetComponentInParent<HandPositioner>();
        ResetScale();

    }

    // Update is called once per frame
    void Update() {
        if (resetScale.WasPressedThisFrame()) {
            ResetScale();
        }

        //forces a cap in case anything gets too large or small accidentally 
        if (DraggableObject.transform.localScale.x > maxScale) {
            //Debug.Log("greater than max scale!");
            Vector2 tempScale = DraggableObject.transform.localScale;
            tempScale.x = maxScale;
            tempScale.y = maxScale;
            DraggableObject.transform.localScale = tempScale;
        }
        else if (DraggableObject.transform.localScale.x < minScale) {
            Vector2 tempScale = DraggableObject.transform.localScale;
            tempScale.x = minScale;
            tempScale.y = minScale;
            DraggableObject.transform.localScale = tempScale;
        }

        if (IsBeingDragged) {
            UpdateCardPosition();
        }

    }
    private void UpdateCardPosition() {
        Vector2 targetPosition = lastMousePosition;
        

        DraggableObject.transform.position = Vector3.SmoothDamp(
            DraggableObject.transform.position,
            targetPosition,
            ref velocity,
            dragSmoothTime
        );
    }

    

    public void OnScroll(PointerEventData pointer) {
        Debug.Log("onscroll is being called");
        if (pointer.scrollDelta.y > 0.0f) // Zoom in
        {
            if ((DraggableObject.transform.localScale.x + 0.05f) <= maxScale) // Only zoom in when the zoom is less than the max, we allow the zoom in
            {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x += 0.05f;
                tempScale.y += 0.05f;
                DraggableObject.transform.localScale = tempScale;
            }
            else {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x = maxScale;
                tempScale.y = maxScale;
                DraggableObject.transform.localScale = tempScale;
            }
        }
        else {
            if ((DraggableObject.transform.localScale.x - 0.05f) >= minScale) // Only zoom out when the zoom is more than the minimum.
            {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x -= 0.05f;
                tempScale.y -= 0.05f;
                DraggableObject.transform.localScale = tempScale;
            }
            else {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x = minScale;
                tempScale.y = minScale;
                DraggableObject.transform.localScale = tempScale;
            }
        }
    }
    public void OnDrag(PointerEventData eventData) {
        if (DraggableObject.activeSelf && IsBeingDragged) {
            lastMousePosition = eventData.position;
        }
    }
    //public void OnDrag(PointerEventData pointer) {
    //    if (DraggableObject.activeSelf && IsBeingDragged) {
    //        UpdatePosition();
    //        RectTransform target = DraggableObject.GetComponent<RectTransform>();
    //        Vector2 localPos = target.transform.localPosition;

    //        // check to see where we're dragging
    //        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position, pointer.pressEventCamera, out Vector2 tempNewVec)) {
    //            DraggableObject.transform.localPosition = new Vector2(localPos.x + tempNewVec.x,
    //                localPos.y + tempNewVec.y);
    //        }
    //    }
    //}
    
    public void UpdatePosition() {
        originalPosition = gameObject.transform.position;
    }
    public void UpdateScale() {
        originalPosition = this.gameObject.transform.localScale;
    }

    public void ResetScale() {
        Transform parent = this.gameObject.transform.parent;
        this.gameObject.transform.SetParent(null, true);
        this.gameObject.transform.localScale = originalScale;
        this.gameObject.transform.SetParent(parent, true);
    }

    public void ResetPosition() {
        Transform parent = this.gameObject.transform.parent;
        this.gameObject.transform.SetParent(null, true);
        this.gameObject.transform.SetPositionAndRotation(new Vector3(), gameObject.transform.rotation);
        this.gameObject.transform.SetParent(parent, true);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        IsBeingDragged = true;
        lastMousePosition = eventData.position;
        if (handPositioner != null) {
            handPositioner.NotifyCardDragStart(gameObject);
        }

        // Center the card on the mouse position
        UpdateCardPosition();

        // Start scaling
        ScaleTo(dragScale);
    }

    public void OnEndDrag(PointerEventData eventData) {
        IsBeingDragged = false;

        // Notify the HandPositioner that the card has been dropped
        if (GameObject.FindWithTag("GameController").TryGetComponent(out GameManager gameManager)) {
            if (handPositioner != null) {
                handPositioner.NotifyCardDragEnd(
                    gameObject, 
                    gameManager.actualPlayer.HandleCardDrop(GetComponent<Card>())); //get the drop zone from the card player
            }
        }
        //Get the mouse position to see where the card was dropped
        var mousePos = eventData.position;
        
    }

    //public void OnPointerEnter(PointerEventData eventData) {
    //    if (!IsBeingDragged) {
    //        ScaleTo(hoverScale);
    //    }
    //}

    //public void OnPointerExit(PointerEventData eventData) {
    //    if (!IsBeingDragged) {
    //        ScaleTo(dragScale);
    //    }
    //}

    private void ScaleTo(Vector2 targetScale) {
        if (scaleCoroutine != null) {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleCoroutine(targetScale));
        transform.localScale = targetScale;
    }

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
