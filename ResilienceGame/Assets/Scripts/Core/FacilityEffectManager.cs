using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This class belongs to a facility and manages the effects that are applied to it
/// </summary>
public class FacilityEffectManager {
    private List<FacilityEffect> activeEffects = new List<FacilityEffect>();
    private Facility facility;
    private bool hasNegatedEffectThisRound = false;

    public FacilityEffectManager(Facility facility) {
        this.facility = facility;
    }

    public List<FacilityEffect> GetEffects() {
        return activeEffects;
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
        // If the facility is fortified and the effect is created by the red team, negate the effect once per round
        if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }
        // Check if the effect already exists
        var existingEffect = activeEffects.FirstOrDefault(e =>
               e.CreatedEffectID == effect.CreatedEffectID &&
               e.EffectType == effect.EffectType &&
               e.Target == effect.Target);

        // If the effect already exists, add a stack to it
        if (existingEffect != null) {
            existingEffect.AddStack();
        }
        else {
            activeEffects.Add(effect);
        }
        // Apply the effect to the facility
        ApplyEffect(effect);
    }
    /// <summary>
    /// Removes an effect from the facility
    /// </summary>
    /// <param name="effect">The effect to remove</param>
    private void RemoveEffect(FacilityEffect effect) {
        // If the facility is fortified and the effect is created by the red team, negate the effect once per round
        if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        var existingEffect = activeEffects.FirstOrDefault(e => e.CreatedEffectID == effect.CreatedEffectID);
        if (existingEffect != null) {
            if (existingEffect.Stack > 1) {
                existingEffect.RemoveStack();
            }
            else {
                activeEffects.Remove(existingEffect);
            }
            UnapplyEffect(existingEffect);
        }
    }
    /// <summary>
    /// Forces a removal of an effect from the facility, meant to be called be Effect cards that remove effects from facility (assuming this bypasses fortification otherwise it seems kinda pointless)
    /// </summary>
    /// <param name="effect">The effect to remove from the facility</param>
    public void ForceRemoveEffect(FacilityEffect effect) {
        var existingEffect = activeEffects.FirstOrDefault(e => e.CreatedEffectID == effect.CreatedEffectID);
        if (existingEffect != null) {
            if (existingEffect.Stack > 1) {
                existingEffect.RemoveStack();
            }
            else {
                activeEffects.Remove(existingEffect);
            }
            UnapplyEffect(existingEffect);
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
        // Loop through all active effects
        foreach (var effect in activeEffects.ToList()) {
            //If the effect is a modify points per turn effect, create its 
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
        return activeEffects.Find(e => e.CreatedEffectID == id) != null;
    }

    public void RemoveAllEffects() {
        activeEffects.Clear();
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
