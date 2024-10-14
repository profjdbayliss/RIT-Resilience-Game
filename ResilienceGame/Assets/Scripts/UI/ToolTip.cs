using UnityEngine;
using TMPro;

public class ToolTip : MonoBehaviour {
    [SerializeField] private GameObject tooltipBox;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] RectTransform tooltipRect;
    public static ToolTip Instance;

    private float screenWidth;
    private float screenHeight;

    private void Awake() {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
        Instance = this;
        HideTooltip();
    }

    public void ShowTooltip(string message, Vector3 position) {
        Instance.tooltipText.text = message;
        Instance.tooltipBox.SetActive(true);

        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;

        //Debug.Log(tooltipWidth);

        // Clamp the tooltip's position to ensure it doesn't go off-screen
        float clampedX = Mathf.Clamp(position.x, tooltipWidth / 2, screenWidth - tooltipWidth / 2);
        float clampedY = Mathf.Clamp(position.y, tooltipHeight / 2, screenHeight - tooltipHeight / 2);

        // Set the tooltip position
        Instance.tooltipBox.transform.position = new Vector3(clampedX, clampedY, position.z);
    }


    public static void HideTooltip() {

        Instance.tooltipBox.SetActive(false);
    }
}
