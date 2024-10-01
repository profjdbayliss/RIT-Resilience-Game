using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FacilityEffectUIElement : MonoBehaviour
{
    private const int PHYSICAL = 0;
    private const int FINANCIAL = 1;
    private const int NETWORK = 2;
    private const int FINANCIAL_NETWORK = 3;
    private const int PHYSICAL_FINANCIAL = 4;
    private const int PHYSICAL_NETWORK = 5;
    private const int ALL = 6;

    [SerializeField] Image effectImage;
    [SerializeField] TextMeshProUGUI effectText;
    [SerializeField] Material outlineMat;
    [SerializeField] Sprite[] effectSprites;
    private string effectToolTip;

    public void SetEffectType(FacilityPointTarget effectTarget, int magnitude) {
        effectToolTip = "";
        if (effectTarget == FacilityPointTarget.All) {
            effectImage.sprite = effectSprites[ALL];
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
            1 => ("physical", effectSprites[PHYSICAL]),
            2 => ("financial", effectSprites[FINANCIAL]),
            4 => ("network", effectSprites[NETWORK]),
            3 => ("physical and financial", effectSprites[PHYSICAL_FINANCIAL]),
            5 => ("physical and network", effectSprites[PHYSICAL_NETWORK]),
            6 => ("financial and network", effectSprites[FINANCIAL_NETWORK]),
            _ => ("all", effectSprites[ALL]),
        };
        UpdateText(magnitude);
        effectToolTip = $"{(magnitude > 0 ? "Restores": "Reduces")} {typeString} points by {magnitude}";
    }
    public void ToggleOutline(bool enable) {
        effectImage.material = enable ? outlineMat : null;
    }
    public void UpdateText(int amt) {
        effectText.text = amt > 0 ? $"+{amt}":$"{amt}";
    }


}
