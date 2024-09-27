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
    [SerializeField] Image effectImage;
    [SerializeField] TextMeshProUGUI effectText;
    [SerializeField] Material outlineMat;
    [SerializeField] Sprite[] effectSprites;

    public void SetEffectType(FacilityEffectTarget effectTarget) {
        switch (effectTarget) {
            case FacilityEffectTarget.Physical:
                effectImage.sprite = effectSprites[PHYSICAL];
                break;
            case FacilityEffectTarget.Financial:
                effectImage.sprite = effectSprites[FINANCIAL];
                break;
            case FacilityEffectTarget.Network:
                effectImage.sprite = effectSprites[NETWORK];
                break;
        }
    }
    public void ToggleOutline(bool enable) {
        effectImage.material = enable ? outlineMat : null;
    }
    public void UpdateText(int stack) {
        effectText.text = stack > 1 ? stack.ToString() : "";
    }


}
