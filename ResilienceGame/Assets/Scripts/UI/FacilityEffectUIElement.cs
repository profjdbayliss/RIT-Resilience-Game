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

    public void SetEffectType(FacilityPointTarget effectTarget) {
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

        switch (combinedIndex) {
            case 1: // Physical only
                effectImage.sprite = effectSprites[PHYSICAL];
                break;
            case 2: // Financial only
                effectImage.sprite = effectSprites[FINANCIAL];
                break;
            case 4: // Network only
                effectImage.sprite = effectSprites[NETWORK];
                break;
            case 3: // Physical and Financial
                effectImage.sprite = effectSprites[PHYSICAL_FINANCIAL];
                break;
            case 5: // Physical and Network
                effectImage.sprite = effectSprites[PHYSICAL_NETWORK];
                break;
            case 6: // Financial and Network
                effectImage.sprite = effectSprites[FINANCIAL_NETWORK];
                break;
            default:
                effectImage.sprite = effectSprites[ALL];
                break;
        }
    }
    public void ToggleOutline(bool enable) {
        effectImage.material = enable ? outlineMat : null;
    }
    public void UpdateText(string amt) {
        effectText.text = amt;
    }


}
