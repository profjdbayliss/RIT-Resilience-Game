using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// This class belongs to a facility and manages the effects that are applied to it
/// </summary>
public class FacilityEffectManager : MonoBehaviour {
    private readonly List<FacilityEffect> activeEffects = new List<FacilityEffect>();
    private Facility facility;
    private bool hasNegatedEffectThisRound = false;
    [SerializeField] private Transform effectParent;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Image effectIcon;
    [SerializeField] private GameObject counterBackground;
    [SerializeField] private TextMeshProUGUI counterText;


    public static Sprite[] EffectSprites { get => Sector.EffectSprites; }
    private int counter = 0;
    private void Start() {
        facility = GetComponent<Facility>();
    }


    public (string, Sprite) GetEffectSprite(FacilityEffect effect) {
        if (effect.EffectType == FacilityEffectType.Backdoor) {
            return ("backdoor", EffectSprites[(int)FacilityEffectTarget.Backdoor]);
        }
        if (effect.EffectType == FacilityEffectType.Fortify) {
            return ("fortify", EffectSprites[(int)FacilityEffectTarget.Fortify]);
        }
        return GetEffectSpriteByPointTarget(effect.Target);
    }
    private static (string, Sprite) GetEffectSpriteByPointTarget(FacilityEffectTarget effectTarget) {
        int index = (int)effectTarget;
        string type = effectTarget switch {
            FacilityEffectTarget.Physical => "physical",
            FacilityEffectTarget.Financial => "financial",
            FacilityEffectTarget.Network => "network",
            FacilityEffectTarget.FinancialNetwork => "financial and network",
            FacilityEffectTarget.FinancialPhysical => "financial and physical",
            FacilityEffectTarget.NetworkPhysical => "network and physical",
            FacilityEffectTarget.All => "all",
            _ => ""
        };

        if (index == -1 || effectTarget == FacilityEffectTarget.None) {
            Debug.LogError($"invalid index {index}");
            return ("", null);
        }


        Debug.Log("Creating icon for effect type: " + effectTarget.ToString() + " with index: " + index);

        return (type, EffectSprites[index]);
    }



    public void DebugAddEffect(string effectId = "") {
        var allEffects = new List<string>() {
            "modp;net;1",
            "modp;phys;1",
            "modp;fin;1",
            "modp;all;1",
            "modp;fin&net;1",
            "modp;phys&net;1",
            "fortify",
            "backdoor",
            "modp;net;-1",
            "modp;phys;-1",
            "modp;fin;-1",
            "modp;all;-1",
            "modp;phys&net;-1",
            "modp;phys&fin;-1",
            "modp;fin&net;-1",
        };
        //foreach (Sprite sprite in EffectSprites) {
        //    AddRemoveEffect(FacilityEffect.CreateEffectsFromID($"modp;net;1")[0], true);
        //}

        Debug.Log($"DEBUG: Adding facility effect with id: {effectId}");
        if (effectId == "")
            AddRemoveEffect(FacilityEffect.CreateEffectsFromID(allEffects[UnityEngine.Random.Range(0, allEffects.Count)])[0], true); //add a random effect from the list
        else
            AddRemoveEffect(FacilityEffect.CreateEffectsFromID(effectId)[0], true);
    }
    public List<FacilityEffect> GetEffects() {
        return activeEffects;
    }
    public FacilityEffect FindEffectByUID(int uid) {
        var result = activeEffects.Find(e => e.UniqueID == uid);

        if (result == null) {
            Debug.LogError($"Did not find effect on {facility.facilityName} with uid {uid}");
        }
        return result;

    }
    public bool TryRemoveEffect(FacilityEffect effect) {
        if (activeEffects.Contains(effect)) {
            ForceRemoveEffect(effect);
            return true;
        }
        return false;


    }
    public bool TryRemoveEffectByType(FacilityEffectType type) {
        var result = activeEffects.Find(e => e.EffectType == type);
        if (result != null) {
            ForceRemoveEffect(result);
            return true;
        }
        Debug.LogWarning($"Did not find effect on {facility.facilityName} with type {type} to remove!");
        return false;
    }
    public void RemoveEffectByUID(int uid) {
        var result = FindEffectByUID(uid);
        if (result != null && result.EffectType != FacilityEffectType.None) {
            ForceRemoveEffect(result); //remove the effect bypassing any possible negation
        }
        else {
            Debug.LogError($"Did not find effect on {facility.facilityName} with uid {uid} to remove!");
        }
    }
    //returns a list of effects that can be removed,
    //these are effects marked with the correct type that are created by the opponent team
    public List<FacilityEffect> GetRemoveableEffects(PlayerTeam playerTeam, bool removePointsPerTurnEffects = false) {
        var opponentTeam = playerTeam == PlayerTeam.Red ? PlayerTeam.Blue : PlayerTeam.Red;
        return GetEffectsCreatedByTeam(opponentTeam).Where(effect => effect.IsRemoveable).ToList();
    }
    public List<FacilityEffect> GetEffectsCreatedByTeam(PlayerTeam team) {
        return activeEffects.Where(effect => effect.CreatedByTeam == team).ToList();
    }
    /// <summary>
    /// Handles adding or removing an effect from the facility
    /// </summary>
    /// <param name="effect">The facility effect to add or remove from the facility</param>
    /// <param name="isAdding">True if the effect should be added, false otherwise</param>
    public void AddRemoveEffect(FacilityEffect effect, bool isAdding) {
        if (isAdding) {
            AddEffect(effect);
        }
        else {
            RemoveEffect(effect);
        }

    }
    /// <summary>
    /// Adds an effect to the facility
    /// </summary>
    /// <param name="effect">The effect to add</param>
    private void AddEffect(FacilityEffect effect) {
        //special case of a remove type from a card effect
        if (effect.EffectType == FacilityEffectType.Remove) {
            RemoveAllEffects();
            return;
        }
        if (IsFortified() && effect.CreatedByTeam == PlayerTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        activeEffects.Add(effect);//add the effect to list
        //UpdateEffectUI(effect);
        ApplyEffect(effect);
    }
    public void UpdateSpecialIcon(FacilityEffect effect, bool add = true) {
        if (effect.EffectType == FacilityEffectType.Backdoor || effect.EffectType == FacilityEffectType.Fortify) {
            if (add) {
                //Debug.Log($"Adding special effect icon to facility");
                effectIcon.sprite = GetEffectSprite(effect).Item2;
                counterBackground.SetActive(true);
                counter = effect.Duration;
                counterText.text = effect.Duration.ToString();
            }
            else {
                counterBackground.SetActive(false);
            }
            ToggleEffectImageAlpha();
        }
    }
    public void DecrementCounter() {
        counter--;
        counterText.text = counter.ToString();
        if (counter == 0) {
            counter = -1;
            counterBackground.SetActive(false);
        }
    }
    //add back ui element here
    //public void UpdateEffectUI(FacilityEffect effect, bool add = true) {
    //    if (effect.EffectType == FacilityEffectType.Backdoor || effect.EffectType == FacilityEffectType.Fortify) {
    //        UpdateSpecialIcon(effect, add);
    //        return;
    //    }

    //    int indexToUpdate = activeEffects.FindIndex(e => e.UniqueID == effect.UniqueID);

    //    if (add) {
    //        if (indexToUpdate != -1) {
    //            // Effect exists, update or create its UI element
    //            var (existingEffect, existingUI) = activeEffects[indexToUpdate];
    //            if (existingUI == null) {
    //                // Create new UI element
    //                var newEffectUI = Instantiate(effectPrefab, effectParent).GetComponent<FacilityEffectUIElement>();
    //                newEffectUI.SetIconAndText(GetEffectSprite(effect), effect.Magnitude);

    //                activeEffects[indexToUpdate] = (existingEffect, newEffectUI);
    //            }
    //            else {
    //                // Update existing UI element
    //                existingUI.SetIconAndText(GetEffectSprite(effect), effect.Magnitude);

    //            }
    //        }
    //        else {
    //            Debug.LogError("Shouldn't ever happen - probably some effect UID error");
    //        }
    //    }
    //    else {
    //        // Remove the effect element
    //        if (indexToUpdate != -1) {
    //            var (_, uiElement) = activeEffects[indexToUpdate];
    //            activeEffects.RemoveAt(indexToUpdate);
    //            if (uiElement != null) {
    //                Destroy(uiElement.gameObject);
    //            }
    //        }
    //    }
    //}
    public void ToggleEffectImageAlpha() {
        Color color = effectIcon.color;
        var newColor = color.a == 1 ? new Color(color.r, color.g, color.b, 0f) : new Color(color.r, color.g, color.b, 1);
        effectIcon.color = newColor;
        
        Debug.Log($"Toggling effect icon alpha to {newColor.a}");
    }
    /// <summary>
    /// Removes an effect from the facility
    /// </summary>
    /// <param name="effect">The effect to remove</param>
    private void RemoveEffect(FacilityEffect effect, bool bypassFortified = false) {
        if (!bypassFortified) {
            if (facility.IsFortified() && effect.CreatedByTeam == PlayerTeam.Red && !hasNegatedEffectThisRound) {
                hasNegatedEffectThisRound = true;
                return;
            }
        }
        int indexToRemove = activeEffects.FindIndex(e => e.UniqueID == effect.UniqueID);
        if (indexToRemove == -1) {
            Debug.LogError("Trying to remove an effect that doesn't exist [Probably UID issue]");
            return;
        }
        var effectToRemove = activeEffects[indexToRemove];

        if (effectToRemove != null) {
            activeEffects.RemoveAt(indexToRemove);
            UnapplyEffect(effectToRemove);
        }
    }
    /// <summary>
    /// Forces a removal of an effect from the facility, meant to be called by Effect cards that remove effects from facility (assuming this bypasses fortification otherwise it seems kinda pointless)
    /// </summary>
    /// <param name="effect">The effect to remove from the facility</param>
    public void ForceRemoveEffect(FacilityEffect effect) {
        RemoveEffect(effect, true);
    }

    /// <summary>
    /// Applies the effect to the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void ApplyEffect(FacilityEffect effect) {
        Debug.Log($"Applying effect {effect.EffectType} to {facility.facilityName}");
        switch (effect.EffectType) {
            case FacilityEffectType.ModifyPoints:
            case FacilityEffectType.ModifyPointsPerTurn:
                ChangeFacilityPoints(effect);
                break;
            case FacilityEffectType.Backdoor:
                UpdateSpecialIcon(effect);
                break;
            case FacilityEffectType.Fortify:
                if (IsBackdoored()) {
                    ToggleEffectImageAlpha();
                }
                RemoveNegativeEffects();
                UpdateSpecialIcon(effect);
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// Removes the effect from the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void UnapplyEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.ModifyPoints:
            case FacilityEffectType.ModifyPointsPerTurn:
                ChangeFacilityPoints(effect, isRemoving: true);
                break;
            case FacilityEffectType.Backdoor:
            case FacilityEffectType.Fortify:
                UpdateSpecialIcon(effect, false);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Called when the round is ended by the game manager
    /// </summary>
    public void UpdateForNextActionPhase() {
        // Debug.Log($"Updating for new action phase for Facility {facility.facilityName}");
        //update all effects
        foreach (var effect in activeEffects) {
            //only update effects that are created by the team whos turn it is
            //this will be called twice once in action red and once in action blue
            //we need to update the correct effects on both clients each phase
            if (!IsEffectCreatorsTurn(effect)) return;

            if (effect.EffectType == FacilityEffectType.ModifyPointsPerTurn) {
                var effects = FacilityEffect.CreateEffectsFromID(effect.EffectCreatedOnRoundEndIdString);
                effects.ForEach(_effect => AddEffect(_effect));
            }

            if (effect.Duration > 0) {
                // Debug.Log($"Reducing duration of {effect.EffectType} on facility {facility.facilityName}");
                effect.Duration--;
                DecrementCounter();
                if (effect.Duration == 0) {
                    ForceRemoveEffect(effect);
                }
            }
        }
        hasNegatedEffectThisRound = false;
    }

    private bool IsEffectCreatorsTurn(FacilityEffect effect) {
        //Debug.Log($"Checking if {effect.EffectType} created by the {effect.CreatedByTeam} team should be adjusted during {GameManager.instance.MGamePhase} phase");
        return effect.CreatedByTeam switch {
            PlayerTeam.Red => GameManager.instance.MGamePhase == GamePhase.ActionRed,
            PlayerTeam.Blue => GameManager.instance.MGamePhase == GamePhase.ActionBlue,
            _ => false
        };
    }
    public bool HasEffectsByOpponentTeam(PlayerTeam opponentTeam) {
        //return true if there are any effects created by the opponent team
        return activeEffects.Any(effect => effect.CreatedByTeam == opponentTeam);
    }
    public bool HasEffectOfType(FacilityEffectType type) {
        return activeEffects.Any(effect => effect.EffectType == type);
    }
    public bool IsFortified() {
        return activeEffects.Any(effect => effect.EffectType == FacilityEffectType.Fortify);
    }
    public bool IsBackdoored() {
        return activeEffects.Any(effect => effect.EffectType == FacilityEffectType.Backdoor);
    }
    void RemoveNegativeEffects() {
        //remove backdoor or points per turn effects
        activeEffects.RemoveAll(effect => effect.EffectType == FacilityEffectType.ModifyPointsPerTurn || effect.EffectType == FacilityEffectType.Backdoor);
    }
    private void RemoveAllEffects() {
        while (activeEffects.Count > 0) {
            AddRemoveEffect(activeEffects[0], false);
        }
    }

    public void DisplayEffectImageTooltip() {
        FacilityEffect activeIconEffect = activeEffects.Find(effect => effect.EffectType == FacilityEffectType.Backdoor || effect.EffectType == FacilityEffectType.Fortify);
        if (activeIconEffect == null) return;
        string tooltip = activeIconEffect.EffectType switch {
            FacilityEffectType.Fortify => $"Fortified - blocks the first red effect played on this facility each turn\n{activeIconEffect.Duration} turns remaining",
            FacilityEffectType.Backdoor => $"Backdoored - allows certain red cards to be played on this facility\n{activeIconEffect.Duration} turns remaining",
            _ => ""
        };
        if (tooltip != "") {
            ToolTip.Instance.ShowTooltip(tooltip, Mouse.current.position.ReadValue());
        }


    }
    public void HideEffectImageTooltip() {
        ToolTip.HideTooltip();
    }

    #region Effect Functions

    //private void ApplyNegationChangeToFacility(FacilityEffect effect, bool negate) {
    //    if (negate) {
    //        ChangeFacilityPoints(effect, true); // Reverse effect
    //    }
    //    else {
    //        ChangeFacilityPoints(effect); // Reapply effect
    //    }
    //}


    private void ChangeFacilityPoints(FacilityEffect effect, bool isRemoving = false) {
        // Debug.Log($"Changing facility points for {facility.facilityName} by {effect.Magnitude} for {effect.Target}");
        int value = effect.Magnitude * (isRemoving ? 0 : 1); //dont give points back when removing effects

        facility.ChangeFacilityPoints(effect.Target, value);
    }
    //public void NegateEffect(FacilityEffect effect) {
    //    if (IsFortified() && effect.EffectType == FacilityEffectType.ModifyPoints && effect.Magnitude < 0 && !hasNegatedEffectThisRound) {
    //        hasNegatedEffectThisRound = true;
    //        return;
    //    }

    //    if (!effect.IsNegated) {
    //        effect.IsNegated = true;
    //        ApplyNegationChangeToFacility(effect, true); // Apply negation (reverse effect)
    //    }
    //}

    #endregion
}
