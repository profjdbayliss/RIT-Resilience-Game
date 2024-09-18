using System.Collections.Generic;
using System.Linq;

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
    public void AddRemoveEffect(FacilityEffect effect, bool isAdding) {
        if (isAdding) {
            AddEffect(effect);
        }
        else {
            RemoveEffect(effect);
        }
    }
    private void AddEffect(FacilityEffect effect) {
        if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        var existingEffect = activeEffects.FirstOrDefault(e =>
       e.CreatedEffectID == effect.CreatedEffectID &&
       e.EffectType == effect.EffectType &&
       e.Target == effect.Target);

        if (existingEffect != null) {
            existingEffect.AddStack();
        }
        else {
            activeEffects.Add(effect);
        }

        ApplyEffect(effect);
    }

    private void RemoveEffect(FacilityEffect effect) {
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

    private void ApplyEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.Backdoor:
            case FacilityEffectType.Fortify:
                facility.UpdateEffectUI(effect);
                break;
            case FacilityEffectType.RestorePoints:
            case FacilityEffectType.ReducePoints:
                ChangeFacilityPoints(effect, false);
                break;
        }
    }

    private void UnapplyEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.Backdoor:
            case FacilityEffectType.Fortify:
                facility.UpdateEffectUI(null); // Clear the effect UI
                break;
            case FacilityEffectType.RestorePoints:
            case FacilityEffectType.ReducePoints:
                ChangeFacilityPoints(effect, true);
                break;
        }
    }

    
    /// <summary>
    /// Called when the round is ended by the game manager
    /// </summary>
    public void OnRoundEnded() {
        foreach (var effect in activeEffects.ToList()) {
            if (effect.EffectType == FacilityEffectType.ReducePointsPerTurn) {
                var newEffect = new FacilityEffect(FacilityEffectType.ReducePoints, effect.Target, effect.Magnitude, 1);
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
    /// <summary>
    /// Checks if the facility has a specific effect
    /// </summary>
    /// <param name="effectType">The type of effect blue/red</param>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool HasEffect(FacilityEffectType effectType, FacilityEffectTarget target) {
        return activeEffects.Find(e => e.EffectType == effectType && e.Target == target) != null;
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
            switch (effect.EffectType) {
                case FacilityEffectType.RestorePoints:
                    ChangeFacilityPoints(effect, true); // Reverse restore effect
                    break;
                case FacilityEffectType.ReducePoints:
                    ChangeFacilityPoints(effect, false);  // Reverse reduce effect
                    break;
            }
        }
        else {
            switch (effect.EffectType) {
                case FacilityEffectType.RestorePoints:
                    ChangeFacilityPoints(effect, false); // Reapply restore effect
                    break;
                case FacilityEffectType.ReducePoints:
                    ChangeFacilityPoints(effect, true);  // Reapply reduce effect
                    break;
            }
        }
    }
    #endregion

    #region Effect Functions
    private void ChangeFacilityPoints(FacilityEffect effect, bool isRemoving) {
        int sign = effect.EffectType == FacilityEffectType.RestorePoints ? 1 : -1;
        int value = effect.Magnitude * sign * (isRemoving ? -1 : 1);
        facility.ChangeFacilityPoints(effect.Target.ToString(), value);
    }
    public void NegateEffect(FacilityEffect effect) {
        if (facility.IsFortified && effect.EffectType == FacilityEffectType.ReducePoints && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        if (!effect.IsNegated) {
            effect.IsNegated = true;
            ApplyNegationChangeToFacility(effect, true); // Apply negation (remove effect)
        }
    }

    #endregion
}
