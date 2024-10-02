using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FacilityEffectUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    

    public Image EffectImage;
    [SerializeField] TextMeshProUGUI effectText;
    [SerializeField] Material outlineMat;
    //[SerializeField] Sprite[] effectSprites;
    public string EffectToolTip { get; private set; }

    public void SetIconAndText((string typeString, Sprite sprite) spriteTuple, int magnitude) {
        EffectImage.sprite = spriteTuple.sprite;
        UpdateText(magnitude);

        EffectToolTip = $"{(magnitude > 0 ? "Restores" : "Reduces")} {spriteTuple.typeString} points by {Mathf.Abs(magnitude)}";
    }
    public void ToggleOutline(bool enable) {
        EffectImage.material = enable ? outlineMat : null;
    }
    public void UpdateText(int amt) {
        effectText.text = amt > 0 ? $"+{amt}" : $"{amt}";
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Vector3 tooltipPosition = Mouse.current.position.ReadValue();
        Tooltip.ShowTooltip(EffectToolTip, tooltipPosition);
    }
    public void OnPointerExit(PointerEventData eventData) {
        Tooltip.HideTooltip();
    }


}
