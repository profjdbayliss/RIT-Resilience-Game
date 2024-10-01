using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour {
    [SerializeField] private GameObject tooltipBox;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private static Tooltip Instance;

    private void Awake() {
        Instance = this;
        HideTooltip();
    }

    public static void ShowTooltip(string message, Vector3 position) {
        Instance.tooltipText.text = message;
        Instance.tooltipBox.SetActive(true);
        Instance.tooltipBox.transform.position = position;
    }

    public static void HideTooltip() {
        Instance.tooltipBox.SetActive(false);
    }
}
