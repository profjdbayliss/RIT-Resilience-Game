using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditEffect : MonoBehaviour {
    [SerializeField] private GameObject targetParent;
    [SerializeField] private GameObject magParent;
    [SerializeField] private TMP_Dropdown targetDropdown;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown magDropdown;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnTypeChange() {
        try {
            var type = Enum.Parse<FacilityEffectType>(typeDropdown.options[typeDropdown.value].text);

            switch (type) {
                case FacilityEffectType.ModifyPoints:
                case FacilityEffectType.ModifyPointsPerTurn:
                    targetParent.SetActive(true);
                    magParent.SetActive(true);
                    break;
                default:
                    targetParent.SetActive(false);
                    magParent.SetActive(false);
                    break;

            }
        }
        catch (Exception e) {
            Debug.LogError(e);
        }

    }
    public void SetTypeDropdown(FacilityEffectType type) {
        switch (type) {
            case FacilityEffectType.Backdoor:
                typeDropdown.value = 0;
                break;
            case FacilityEffectType.ModifyPointsPerTurn:
                typeDropdown.value = 1;
                break;
            case FacilityEffectType.Fortify:
                typeDropdown.value = 2;
                break;
            case FacilityEffectType.HoneyPot:
                typeDropdown.value = 3;
                break;
            case FacilityEffectType.ModifyPoints:
                typeDropdown.value = 4;
                break;
            case FacilityEffectType.ProtectPoints:
                typeDropdown.value = 5;
                break;
            case FacilityEffectType.RemoveOne:
                typeDropdown.value = 6;
                break;
            case FacilityEffectType.RemoveAll:
                typeDropdown.value = 7;
                break;
        }
    }
    public void SetTargetDropdown(FacilityEffectTarget target) {
        switch (target) {
            case FacilityEffectTarget.Financial:
                targetDropdown.value = 0;
                break;
            case FacilityEffectTarget.Network:
                targetDropdown.value = 1;
                break;
            case FacilityEffectTarget.Physical:
                targetDropdown.value = 2;
                break;
            case FacilityEffectTarget.NetworkPhysical:
                targetDropdown.value = 3;
                break;
            case FacilityEffectTarget.FinancialNetwork:
                targetDropdown.value = 4;
                break;
            case FacilityEffectTarget.FinancialPhysical:
                targetDropdown.value = 5;
                break;
            case FacilityEffectTarget.All:
                targetDropdown.value = 6;
                break;
        }
    }
    public void SetMagDropdown(int mag) {
        switch (mag) {
            case -3: magDropdown.value = 0; break;
            case -2: magDropdown.value = 1; break;
            case -1: magDropdown.value = 2; break;
            case 1: magDropdown.value = 3; break;
            case 2: magDropdown.value = 4; break;
            case 3: magDropdown.value = 5; break;
        }
        
    }
    public FacilityEffectTarget GetDropdownTarget() {
        return Enum.Parse<FacilityEffectTarget>(targetDropdown.options[targetDropdown.value].text);
    }
    public FacilityEffectType GetDropdownType() {
        return Enum.Parse<FacilityEffectType>(typeDropdown.options[typeDropdown.value].text);
    }
    public int GetMagDropdown() {
        return int.Parse(magDropdown.options[magDropdown.value].text);
    }
    public void SetEffect(FacilityEffect effect) {
        SetTypeDropdown(effect.EffectType);
        SetTargetDropdown(effect.Target);
        SetMagDropdown(effect.Magnitude);
    }
    public string GetEffectStringFromFields() {
        return FacilityEffect.CreateEffectIdString(
            GetDropdownType(), 
            GetDropdownTarget(), 
            GetMagDropdown());
    }
}
