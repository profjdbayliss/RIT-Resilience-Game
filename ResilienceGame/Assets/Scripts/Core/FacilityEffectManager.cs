using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;

/// <summary>
/// This class belongs to a facility and manages the effects that are applied to it
/// </summary>
public class FacilityEffectManager : MonoBehaviour{
    private List<(FacilityEffect Effect, FacilityEffectUIElement UIElement)> activeEffects
    = new List<(FacilityEffect, FacilityEffectUIElement)>();
    private Facility facility;
    private bool hasNegatedEffectThisRound = false;
    [SerializeField] private Transform effectParent;
    [SerializeField] private GameObject effectPrefab;


    private void Start() {
        facility = GetComponent<Facility>();
    }


    public List<FacilityEffect> GetEffects() {
        return activeEffects.Select(effect => effect.Effect).ToList();
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
        if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        var (Effect, UIElement) = activeEffects.FirstOrDefault(e =>
               e.Effect.CreatedEffectID == effect.CreatedEffectID &&
               e.Effect.EffectType == effect.EffectType &&
               e.Effect.Target == effect.Target);

        if (Effect != null) {
            Effect.AddStack();
            UIElement.UpdateText(Effect.Stack);
        }
        else {
            var effectUI = Instantiate(effectPrefab, effectParent).GetComponent<FacilityEffectUIElement>();
            effectUI.SetEffectType(effect.Target);
            effectUI.UpdateText(effect.Stack);
            activeEffects.Add((effect, effectUI));
        }

        ApplyEffect(effect);
    }
    /// <summary>
    /// Removes an effect from the facility
    /// </summary>
    /// <param name="effect">The effect to remove</param>
    private void RemoveEffect(FacilityEffect effect) {
        if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        var existingTuple = activeEffects.FirstOrDefault(e => e.Effect.CreatedEffectID == effect.CreatedEffectID);
        if (existingTuple.Effect != null) {
            if (existingTuple.Effect.Stack > 1) {
                existingTuple.Effect.RemoveStack();
                existingTuple.UIElement.UpdateText(existingTuple.Effect.Stack);
            }
            else {
                Destroy(existingTuple.UIElement.gameObject);
                activeEffects.Remove(existingTuple);
            }
            UnapplyEffect(existingTuple.Effect);
        }
    }
    /// <summary>
    /// Forces a removal of an effect from the facility, meant to be called by Effect cards that remove effects from facility (assuming this bypasses fortification otherwise it seems kinda pointless)
    /// </summary>
    /// <param name="effect">The effect to remove from the facility</param>
    public void ForceRemoveEffect(FacilityEffect effect) {
        var existingTuple = activeEffects.FirstOrDefault(e => e.Effect.CreatedEffectID == effect.CreatedEffectID);
        if (existingTuple.Effect != null) {
            if (existingTuple.Effect.Stack > 1) {
                existingTuple.Effect.RemoveStack();
                existingTuple.UIElement.UpdateText(existingTuple.Effect.Stack);
            }
            else {
                Destroy(existingTuple.UIElement.gameObject);
                activeEffects.Remove(existingTuple);
            }
            UnapplyEffect(existingTuple.Effect);
        }
    }

    /// <summary>
    /// Applies the effect to the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void ApplyEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.Backdoor:
            case FacilityEffectType.Fortify:
                facility.UpdateEffectUI(effect);    //TODO: eventually all effects should have a ui element
                break;
            case FacilityEffectType.ModifyPoints:
            case FacilityEffectType.ModifyPointsPerTurn:
                ChangeFacilityPoints(effect);
                break;
        }
    }

    /// <summary>
    /// Removes the effect from the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void UnapplyEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.Backdoor:
            case FacilityEffectType.Fortify:    //TODO: eventually all effects should have a ui element
                facility.UpdateEffectUI(null); // Clear the effect UI
                break;
            case FacilityEffectType.ModifyPoints:
            case FacilityEffectType.ModifyPointsPerTurn:
                ChangeFacilityPoints(effect, true);
                break;
        }
    }
    /// <summary>
    /// Called when the round is ended by the game manager
    /// </summary>
    public void OnRoundEnded() {
        foreach (var (effect, uiElement) in activeEffects.ToList()) {
            if (effect.EffectType == FacilityEffectType.ModifyPointsPerTurn) {
                var newEffect = FacilityEffect.CreateEffectFromID(effect.CreatedEffectID - 6);
                AddEffect(newEffect);
            }

            if (effect.Duration > 0) {
                effect.Duration--;
                if (effect.Duration == 0) {
                    RemoveEffect(effect);
                }
            }
        }
        hasNegatedEffectThisRound = false;
    }

    public bool HasEffect(int id) {
        return activeEffects.Any(e => e.Effect.CreatedEffectID == id);
    }

    public void RemoveAllEffects() {
        foreach (var (_, uiElement) in activeEffects) {
            Destroy(uiElement.gameObject);
        }
        activeEffects.Clear();
    }
    public void ToggleAllEffectOutlines(bool enable) {
        foreach (var (_, uiElement) in activeEffects) {
            uiElement.ToggleOutline(enable);
        }
    }

    #region Effect Switches

    private void ApplyNegationChangeToFacility(FacilityEffect effect, bool negate) {
        if (negate) {
            ChangeFacilityPoints(effect, true); // Reverse effect
        }
        else {
            ChangeFacilityPoints(effect); // Reapply effect
        }
    }
    #endregion

    #region Effect Functions
    private void ChangeFacilityPoints(FacilityEffect effect, bool isRemoving = false) {
        int value = effect.Magnitude * (isRemoving ? -1 : 1);
        facility.ChangeFacilityPoints(effect.Target.ToString(), value);
    }
    public void NegateEffect(FacilityEffect effect) {
        if (facility.IsFortified && effect.EffectType == FacilityEffectType.ModifyPoints && effect.Magnitude < 0 && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        if (!effect.IsNegated) {
            effect.IsNegated = true;
            ApplyNegationChangeToFacility(effect, true); // Apply negation (reverse effect)
        }
    }

    #endregion
}
