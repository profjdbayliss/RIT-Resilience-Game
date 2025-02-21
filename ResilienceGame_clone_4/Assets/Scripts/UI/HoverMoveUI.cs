using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverMoveUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public float openDuration = 0.75f; // Duration for opening animation
    public float closeDuration = 1.5f; // Duration for closing animation

    [SerializeField] private RectTransform closedTarget; // Target position for closed state
    [SerializeField] private RectTransform openTarget; // Target position for open state

    private RectTransform rectTransform;
    private bool isHovered = false;
    private bool isLockedOpen = false;
    public bool overrideMover = false;
    private Coroutine currentMoveCoroutine;

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (overrideMover) return;
        if (!isLockedOpen) {
            isHovered = true;
            StartMoveCoroutine(openTarget.anchoredPosition, openDuration);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (overrideMover) return;
        if (!isLockedOpen) {
            isHovered = false;
            StartMoveCoroutine(closedTarget.anchoredPosition, closeDuration);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (overrideMover) return;
        isLockedOpen = !isLockedOpen;

        if (isLockedOpen) {
            StartMoveCoroutine(openTarget.anchoredPosition, openDuration);
        }
        else if (!isHovered) {
            StartMoveCoroutine(closedTarget.anchoredPosition, closeDuration);
        }
    }

    public void SetLockedOpen() {
        overrideMover = true;
        StartMoveCoroutine(openTarget.anchoredPosition, openDuration);
    }

    public void DisableLockedOpen() {
        overrideMover = false;
        StartMoveCoroutine(closedTarget.anchoredPosition, closeDuration);
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
