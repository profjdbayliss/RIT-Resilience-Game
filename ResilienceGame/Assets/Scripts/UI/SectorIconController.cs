using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SectorIconController : MonoBehaviour {
    public enum HoverState {
        Hover,
        NotHover
    }

    private RectTransform rectTransform;
    [SerializeField] private Image backGround;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite[] bgs = new Sprite[2];
    [SerializeField] private Collider2D iconCollider; // Reference to Collider2D for precise position check
    private HoverState hoverState = HoverState.NotHover;
    private HoverState targetState = HoverState.NotHover;
    private Sector sector;
    private Vector2 targetSize;
    private Vector2 normalSize = new Vector2(64, 64);
    private Vector2 hoverSize = new Vector2(128, 256);
    float duration = 0.2f;
    private Coroutine sizeCoroutine;

    // Start is called before the first frame update
    void Start() {
        rectTransform = GetComponent<RectTransform>();
        targetSize = normalSize;
        iconCollider = GetComponent<Collider2D>();
    }
    void OnEnable() {
        iconCollider = GetComponent<Collider2D>();
        rectTransform = GetComponent<RectTransform>();
        if (backGround != null) {
            backGround.sprite = bgs[0]; // Reset to default background
        }
        targetSize = normalSize;
        hoverState = HoverState.NotHover;
        targetState = HoverState.NotHover;
    }

    public void SetSector(Sector sector) {
        this.sector = sector;
        gameObject.SetActive(true);
    }

    // This is called when the pointer enters the UI element
    public void OnPointerEnter() {
        Debug.Log("Pointer Enter");
        SetHoverState(HoverState.Hover);
    }

    // This is called when the pointer exits the UI element
    public void OnPointerExit() {
        Debug.Log("Pointer Exit");
        SetHoverState(HoverState.NotHover);
    }

    private void SetHoverState(HoverState newState) {
        if (hoverState == newState) return;

        targetState = newState;
        targetSize = (targetState == HoverState.Hover) ? hoverSize : normalSize;
        

        // Stop any existing size interpolation coroutine and start a new one
        if (sizeCoroutine != null) {
            StopCoroutine(sizeCoroutine);
        }
        if (targetState == HoverState.Hover) {
            backGround.sprite = bgs[1];
        }
        //backGround.sprite = (targetState == HoverState.Hover) ? bgs[1] : bgs[0];
        sizeCoroutine = StartCoroutine(AnimateSizeChange());
    }

    private IEnumerator AnimateSizeChange() {
        Vector2 initialSize = rectTransform.sizeDelta;
        
        float elapsed = 0f;
          
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); // Cubic smoothing for smoother transition
            rectTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, t);
            yield return null;

            
        }

        rectTransform.sizeDelta = targetSize;
        hoverState = targetState; // Set to final state 
        if (hoverState == HoverState.NotHover) {
            backGround.sprite = bgs[0];
        }
        OnTargetStateReached();
    }

    private void OnTargetStateReached() {
        // Check if the mouse is still over the icon when the animation completes
        if (hoverState == HoverState.Hover && !IsMouseOverIcon()) {
            SetHoverState(HoverState.NotHover);
        }
        else if (hoverState == HoverState.NotHover && IsMouseOverIcon()) {
            SetHoverState(HoverState.Hover);
        }

        sizeCoroutine = null;
    }

    private bool IsMouseOverIcon() {
        // Checks if the mouse is within the Collider2D bounds
        return iconCollider != null && iconCollider.OverlapPoint(Mouse.current.position.ReadValue());
    }
}
