using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPrefab;
    public string tooltipText;
    public Vector2 padding = new Vector2(10, 10); // Padding for the text
    public float hoverDelay = 0.5f;
    public float maxWidth = 300f;

    private GameObject tooltipInstance;
    private TMP_Text textComponent;
    private RectTransform tooltipRect;
    private Coroutine showRoutine;
    private Coroutine hideRoutine;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        tooltipInstance = Instantiate(tooltipPrefab, transform.parent);
        textComponent = tooltipInstance.GetComponentInChildren<TMP_Text>();
        tooltipRect = tooltipInstance.GetComponent<RectTransform>();
        canvasGroup = tooltipInstance.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = tooltipInstance.AddComponent<CanvasGroup>();
        }

        textComponent.text = tooltipText;
        tooltipInstance.SetActive(false);
        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Truncate;
    }

    private void Update()
    {
        if (tooltipInstance.activeSelf)
        {
            UpdateTooltipPosition();

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };
            pointerData.position = Mouse.current.position.ReadValue();

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            canvasGroup.blocksRaycasts = results.Count == 0 || (results.Count > 0 && results[0].gameObject == tooltipInstance);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (showRoutine == null)
        {
            showRoutine = StartCoroutine(ShowTooltip());
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        hideRoutine = StartCoroutine(HideTooltip());
    }

    private IEnumerator ShowTooltip()
    {
        yield return new WaitForSeconds(hoverDelay);
        tooltipInstance.SetActive(true);
        UpdateTooltipPosition();
        showRoutine = null;
    }

    private IEnumerator HideTooltip()
    {
        tooltipInstance.SetActive(false);
        hideRoutine = null;
        yield return null;
    }


    private void UpdateTooltipPosition()
    {
        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];
        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.x += width / 2 + 5;
        mousePosition.y += height / 2 + 5;

        tooltipInstance.transform.position = mousePosition;


        // Get the preferred size for the text
        Vector2 textSize = textComponent.GetPreferredValues(textComponent.text, maxWidth, 0);

        // Calculate the actual width of the tooltip
        float tooltipWidth = Mathf.Min(textSize.x + padding.x * 2, maxWidth);

        // Set the size and position of the TextMeshPro component
        textComponent.rectTransform.sizeDelta = new Vector2(tooltipWidth, textSize.y);
        textComponent.rectTransform.pivot = new Vector2(0.0f, 1.0f);
        textComponent.rectTransform.anchoredPosition = new Vector2(padding.x, -padding.y);

        // Set the size of the tooltip
        tooltipRect.sizeDelta = new Vector2(tooltipWidth, textSize.y) + padding * 2;


        // Check if the tooltip is out of the screen and adjust its position if necessary
        Vector3[] screenCorners = new Vector3[4];
        tooltipRect.GetWorldCorners(screenCorners);
        for (int i = 0; i < screenCorners.Length; i++)
        {
            screenCorners[i] = RectTransformUtility.WorldToScreenPoint(null, screenCorners[i]);
        }
        Rect tooltipScreenRect = new Rect(screenCorners[0].x, screenCorners[0].y, screenCorners[2].x - screenCorners[0].x, screenCorners[2].y - screenCorners[0].y);

        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);

        if (tooltipScreenRect.xMax > screenRect.xMax)
        {
            float overflow = tooltipScreenRect.xMax - screenRect.xMax;
            tooltipInstance.transform.position -= new Vector3(overflow, 0, 0);
        }
        if (tooltipScreenRect.xMin < screenRect.xMin)
        {
            float overflow = screenRect.xMin - tooltipScreenRect.xMin;
            tooltipInstance.transform.position += new Vector3(overflow, 0, 0);
        }
        if (tooltipScreenRect.yMax > screenRect.yMax)
        {
            float overflow = tooltipScreenRect.yMax - screenRect.yMax;
            tooltipInstance.transform.position -= new Vector3(0, overflow, 0);
        }
        if (tooltipScreenRect.yMin < screenRect.yMin)
        {
            float overflow = screenRect.yMin - tooltipScreenRect.yMin;
            tooltipInstance.transform.position += new Vector3(0, overflow, 0);
        }
    }
}
