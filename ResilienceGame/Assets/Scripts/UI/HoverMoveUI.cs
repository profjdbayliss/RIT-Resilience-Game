using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverMoveUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public float moveDistance = 100f; // Distance to move in the Y direction
    public float openDuration = 0.75f; // Duration for opening animation
    public float closeDuration = 1.5f; // Duration for closing animation

    private RectTransform rectTransform;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isHovered = false;
    private bool isLockedOpen = false;
    public bool overrideMover = false;
    private Coroutine currentMoveCoroutine;

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
        closedPosition = rectTransform.anchoredPosition;
        openPosition = closedPosition + new Vector3(0, moveDistance, 0);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (overrideMover) return;
        if (!isLockedOpen) {
            isHovered = true;
            StartMoveCoroutine(openPosition, openDuration);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (overrideMover) return;
        if (!isLockedOpen) {
            isHovered = false;
            StartMoveCoroutine(closedPosition, closeDuration);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (overrideMover) return;
        isLockedOpen = !isLockedOpen;

        if (isLockedOpen) {
            StartMoveCoroutine(openPosition, openDuration);
        }
        else if (!isHovered) {
            StartMoveCoroutine(closedPosition, closeDuration);
        }
    }
    public void SetLockedOpen() {
        overrideMover = true;
        StartMoveCoroutine(openPosition, openDuration);
    }
    public void DisableLockedOpen() {
        overrideMover = false;
        StartMoveCoroutine(closedPosition, closeDuration);
    }
    private void StartMoveCoroutine(Vector3 targetPosition, float duration) {
        if (currentMoveCoroutine != null) {
            StopCoroutine(currentMoveCoroutine);
        }
        currentMoveCoroutine = StartCoroutine(MoveToPosition(targetPosition, duration));
    }
    private IEnumerator MoveToPosition(Vector3 targetPosition, float duration) {
        Vector3 startPosition = rectTransform.anchoredPosition;
        float elapsedTime = 0f;

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = CubicEaseInOut(t);
            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }

    // Cubic Ease In/Out function for smoother animation
    private float CubicEaseInOut(float t) {
        if (t < 0.5f)
            return 4f * t * t * t;
        else
            return 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
