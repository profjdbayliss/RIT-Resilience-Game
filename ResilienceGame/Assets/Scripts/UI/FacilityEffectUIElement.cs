using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FacilityEffectUIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private const int PHYSICAL = 0;
    private const int FINANCIAL = 1;
    private const int NETWORK = 2;
    private const int FINANCIAL_NETWORK = 3;
    private const int PHYSICAL_FINANCIAL = 4;
    private const int PHYSICAL_NETWORK = 5;
    private const int ALL = 6;
    private const int BACKDOOR = 0;
    private const int FORTIFY = 1;

    [SerializeField] Image effectImage;
    [SerializeField] TextMeshProUGUI effectText;
    [SerializeField] Material outlineMat;
    //[SerializeField] Sprite[] effectSprites;
    private string effectToolTip;

    public void SetEffectType(FacilityPointTarget effectTarget, int magnitude) {
        effectToolTip = "";
        if (effectTarget == FacilityPointTarget.All) {
            effectImage.sprite = FacilityEffectManager.EffectSprites[ALL];
            return;
        }

        if (effectTarget == FacilityPointTarget.None) {
            effectImage.color = Color.clear;
            return;
        }

        int combinedIndex = 0;
        if (effectTarget.HasFlag(FacilityPointTarget.Physical)) combinedIndex |= 1;
        if (effectTarget.HasFlag(FacilityPointTarget.Financial)) combinedIndex |= 2;
        if (effectTarget.HasFlag(FacilityPointTarget.Network)) combinedIndex |= 4;

        string typeString = "";

        (typeString, effectImage.sprite) = combinedIndex switch {
            1 => ("physical", FacilityEffectManager.EffectSprites[PHYSICAL]),
            2 => ("financial", FacilityEffectManager.EffectSprites[FINANCIAL]),
            4 => ("network", FacilityEffectManager.EffectSprites[NETWORK]),
            3 => ("physical and financial", FacilityEffectManager.EffectSprites[PHYSICAL_FINANCIAL]),
            5 => ("physical and network", FacilityEffectManager.EffectSprites[PHYSICAL_NETWORK]),
            6 => ("financial and network", FacilityEffectManager.EffectSprites[FINANCIAL_NETWORK]),
            _ => ("all", FacilityEffectManager.EffectSprites[ALL]),
        };
        UpdateText(magnitude);
        effectToolTip = $"{(magnitude > 0 ? "Restores": "Reduces")} {typeString} points by {Mathf.Abs(magnitude)}";
    }
    public void ToggleOutline(bool enable) {
        effectImage.material = enable ? outlineMat : null;
    }
    public void UpdateText(int amt) {
        effectText.text = amt > 0 ? $"+{amt}":$"{amt}";
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Vector3 tooltipPosition = Mouse.current.position.ReadValue();
        Tooltip.ShowTooltip(effectToolTip, tooltipPosition);
    }
    public void OnPointerExit(PointerEventData eventData) {
        Tooltip.HideTooltip();
    }


}
