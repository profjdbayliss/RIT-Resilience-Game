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
    [SerializeField] private List<Sprite> effectSprites;
    // [SerializeField] Sprite[] effectSprites;
    [SerializeField] private GameObject counter;
    [SerializeField] private TextMeshProUGUI counterText;
    public string EffectToolTip { get; private set; }

    public void Init(FacilityEffect effect) {
        var type = effect.EffectType;
        var magnitude = effect.Magnitude;
        (EffectToolTip, EffectImage.sprite) = type switch {
            FacilityEffectType.Backdoor => ("Backdoored - allows certain red cards to be played on this facility", effectSprites[0]),
            FacilityEffectType.Fortify => ("Fortified - blocks the first red effect played on this facility each turn", effectSprites[1]),
            FacilityEffectType.ModifyPointsPerTurn => ("Reduces Physical and Network points by 1 per turn", effectSprites[2]),
            FacilityEffectType.ProtectPoints => ($"{effect.Target} points cannot be reduced for the duration", effectSprites[3]),
            FacilityEffectType.HoneyPot => ($"If a red effect targets this facility, cancel the effect and the red player discards 1 card", effectSprites[4]),
            _ => ("", null)
        };
        if (effect.Duration > 0) {
            counter.SetActive(true);
            counterText.text = effect.Duration.ToString();
        }

        

    }
    
    public void SetCounterText(string dur) {
        counterText.text = dur;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        Vector3 tooltipPosition = Mouse.current.position.ReadValue();
        ToolTip.Instance.ShowTooltip(EffectToolTip, tooltipPosition);
    }
    public void OnPointerExit(PointerEventData eventData) {
        ToolTip.HideTooltip();
    }


}
