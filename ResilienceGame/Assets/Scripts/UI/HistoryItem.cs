using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HistoryItem : MonoBehaviour
{
    public string Tooltip { get; set; } = "This is a test. If you see this PANIC!!!!";
    
    public Image cardImage;
    [SerializeField] private Sprite missingCardImageSprite;
    private bool isInit = false;
    private HistoryMenuController controller;

    public void ShowTool() {
        if (!isInit || controller == null) return;
        controller.ShowHistoryTooltip(Tooltip);
    }
    public void HideTool() {
        if (!isInit || controller == null) return;
        controller.HideHistoryTooltip();
    }
    
    public void SetCardImage(HistoryMenuController controller = null, Sprite sprite = null) {
        if (sprite == null) {
            cardImage.sprite = missingCardImageSprite;
        }
        if (controller != null) {
            this.controller = controller;
        }
        isInit = true;

        cardImage.color = Color.white;
        cardImage.sprite = sprite;
    }
}
