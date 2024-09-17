using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class FacilityEffectManager {
    private List<FacilityEffect> activeEffects;
    public Facility facility;
    private bool hasNegatedEffectThisRound = false;

    public FacilityEffectManager(Facility facility) {
        activeEffects = new List<FacilityEffect>();
        this.facility = facility;
    }

    public List<FacilityEffect> GetEffects() {
        return activeEffects;
    }
    /// <summary>
    /// Called when the round is ended by the game manager
    /// </summary>
    void OnRoundEnded() {
        hasNegatedEffectThisRound = false;

        // Loop through effects, decrease duration, and remove expired ones
        for (int i = activeEffects.Count - 1; i >= 0; i--) {
            FacilityEffect currentEffect = activeEffects[i];

            // Handle ReducePointsPerTurn effects by adding ReducePoints effects
            if (currentEffect.EffectType == FacilityEffectType.ReducePointsPerTurn) {
                FacilityEffect newEffect = new FacilityEffect(FacilityEffectType.ReducePoints, currentEffect.Target, currentEffect.Magnitude, -1);
                activeEffects.Add(newEffect);
                currentEffect.CreatedEffects.Add(newEffect);
                ApplyAddedEffect(newEffect);
            }

            // Handle effect duration decrement and removal
            if (currentEffect.Duration > 0 && --currentEffect.Duration == 0) {
                // Remove any effects created by this one
                foreach (var createdEffect in currentEffect.CreatedEffects) {
                    activeEffects.Remove(createdEffect);
                }
                activeEffects.RemoveAt(i);
            }
        }
    }
    public void RemoveEffectByCreatedId(int id) {
        if (activeEffects.Find(e => e.CreatedEffectID == id) is FacilityEffect effect) {
            RemoveFacilityEffect(effect.EffectType, effect.Target, effect.Magnitude);
        }
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

    public void AddEffect(FacilityEffect effect) {
        Debug.Log("Facility effect manager recieved new effect to add: " + effect.ToString());
        
        if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {   //Null reference?
            hasNegatedEffectThisRound = true;
            return;
        }
        FacilityEffect existingEffect = activeEffects.Find(e => e.CreatedEffectID == effect.CreatedEffectID);

        if (existingEffect != null) {
            existingEffect.AddStack();
            ApplyAddedEffect(existingEffect);
        }
        else {
            //deep copy to new effect object
            FacilityEffect newEffect = new FacilityEffect(effect.EffectType, effect.Target, effect.Magnitude, effect.Duration);
            activeEffects.Add(newEffect);
            ApplyAddedEffect(newEffect);
        }
    }

    //called by the facility's add/remove method
    //public void AddEffect(FacilityEffectType effectType, FacilityEffectTarget target, int magnitude, int duration = -1) {
    //    Debug.Log("Facility effect manager recieved new effect to add: " + effectType + " " + target + " " + magnitude + " " + duration);
    //    if (facility.IsFortified && effectType == FacilityEffectType.ReducePoints && !hasNegatedEffectThisRound) {
    //        hasNegatedEffectThisRound = true;
    //        return;
    //    }

    //    FacilityEffect existingEffect = activeEffects.Find(e => e.EffectType == effectType && e.Target == target && e.Magnitude == magnitude);

    //    if (existingEffect != null) {
    //        existingEffect.AddStack();
    //        ApplyAddedEffect(existingEffect);
    //    }
    //    else {
    //        FacilityEffect newEffect = new FacilityEffect(effectType, target, magnitude, duration);
    //        activeEffects.Add(newEffect);
    //        ApplyAddedEffect(newEffect);
    //    }


    //}

    //public void RemoveEffect(FacilityEffectType effectType, FacilityEffectTarget target, int magnitude) {
    //    FacilityEffect existingEffect = activeEffects.Find(e => e.EffectType == effectType && e.Target == target && e.Magnitude == magnitude);

    //    if (existingEffect != null) {
    //        if (existingEffect.Stack > 1) {
    //            existingEffect.RemoveStack();
    //        }
    //        else {
    //            foreach (var createdEffect in existingEffect.CreatedEffects) {
    //                activeEffects.Remove(createdEffect);
    //            }
    //            activeEffects.Remove(existingEffect);
    //        }
    //    }
    //}

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

    private void ApplyAddedEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.Backdoor:
                AddRemoveBackdoor();
                break;
            case FacilityEffectType.Fortify:
                AddRemoveFortify();
                break;
            case FacilityEffectType.RestorePoints:
            case FacilityEffectType.ReducePoints:
                ChangeFacilityPoints(effect, false);
                break;
        }
    }

    public void RemoveFacilityEffect(FacilityEffectType effectType, FacilityEffectTarget target, int magnitude) {
        FacilityEffect effect = activeEffects.Find(e => e.EffectType == effectType && e.Target == target && e.Magnitude == magnitude);
        if (effect != null) {
            ApplyNegationChangeToFacility(effect, true);  // Apply reversal before removing
            activeEffects.Remove(effect);
        }
    }

    #endregion

    #region Effect Functions

    void ChangeFacilityPoints(FacilityEffect effect, bool remove = false) {
        Debug.Log("changing facility points due to effect add/remove");
        int sign = effect.EffectType == FacilityEffectType.RestorePoints ? 1 : -1;
        int value = effect.Magnitude * sign * (remove ? -1 : 1);
        facility.ChangeFacilityPoints(target.ToString(), value);
    }

    void AddRemoveBackdoor(bool remove = false) {
        facility.effectIcon.sprite = remove ? null : Sector.EffectSprites[0];
        facility.IsBackdoored = !remove;
        facility.ToggleEffectImageAlpha();
    }

    void AddRemoveFortify(bool remove = false) {
        facility.effectIcon.sprite = remove ? null : Sector.EffectSprites[1];
        facility.IsFortified = !remove;
        RemoveAllEffects();
        facility.ToggleEffectImageAlpha();
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
