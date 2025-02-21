using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class DynamicContentAdjuster : MonoBehaviour
{
    private GridLayoutGroup gridLayoutGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        AdjustContentSize();
    }

    public void AdjustContentSize()
    {
        // Calculate the number of rows required based on the child count
        int childCount = transform.childCount;
        int cellsPerRow = Mathf.FloorToInt(rectTransform.rect.width / gridLayoutGroup.cellSize.x);
        int requiredRows = Mathf.CeilToInt((float)childCount / cellsPerRow);

        // Calculate the required height based on the number of rows
        float requiredHeight = requiredRows * gridLayoutGroup.cellSize.y
                               + (requiredRows - 1) * gridLayoutGroup.spacing.y
                               + gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;

        // Set the height of the Content RectTransform
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, requiredHeight);
    }
}
