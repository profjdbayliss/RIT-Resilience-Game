using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoDisplay : MonoBehaviour {

    public TextMeshProUGUI cardInfo;
    private bool displayCardInfo = false;
    public GameObject cardInfoPanel;
    public Toggle cardInfoToggle;
    private bool panelActive = false;
    private HandPositioner handPositioner;

    // Start is called before the first frame update
    void Start() {
        if (cardInfoToggle != null) {
            cardInfoToggle.onValueChanged.AddListener(ToggleCardInfo);
        }

        // Find the HandPositioner in the scene and add the listener
        handPositioner = FindObjectOfType<HandPositioner>();
        if (handPositioner != null) {
            handPositioner.AddCardHoverListener(card => {
                if (displayCardInfo) {
                    SetCardInfo(card);
                    if (cardInfoPanel != null) {
                        cardInfoPanel.SetActive(card != null);
                    }
                }
            });
        }
        else {
            Debug.LogError("HandPositioner not found in the scene!");
        }
    }
    void SetCardInfo(Card card) {
        if (card == null) {
            cardInfo.text = "No card selected";
            return;
        }

        StringBuilder sb = new StringBuilder();

        // Card information
        sb.AppendLine($"Name: {card.front.title}");
        sb.AppendLine($"Desc: {card.front.description}");
        sb.AppendLine($"State: {card.State}");
        sb.AppendLine($"Target: {card.target}");
        sb.AppendLine($"UID: {card.UniqueID}");
        // Actions
        sb.AppendLine("Actions:");
        //Debug.Log($"Card Action List Count: {card.ActionList.Count}");
        foreach (var action in card.ActionList) {
            sb.AppendLine($"- {action}");
        }

        // CardData information
        sb.AppendLine("Card Data:");
        sb.AppendLine($"Card Type: {card.data.cardType}");
        sb.AppendLine($"Effects: ");
        foreach (var s in card.data.effectString.Split(';')) {
            sb.AppendLine($"- {s}");
        }
        sb.AppendLine($"PreReq Effect: {card.data.preReqEffectType}");
        sb.AppendLine($"Effect Count: {card.data.effectCount}");
        sb.AppendLine($"Has Doom Effect: {card.data.hasDoomEffect}");
        sb.AppendLine($"Shared card id: {card.data.cardID}");

        cardInfo.text = sb.ToString();
    }

    public void ToggleCardInfo(bool display) {
        displayCardInfo = display;
        // cardInfoPanel.SetActive(displayCardInfo);
    }
}
