using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacilityEffectType {
    Blue, //positive
    Red //negative
}
public enum FacilityEffect {
    Backdoor,
    Fortify,
    NegateEffect,
    RestorePoints_Physical_One,
    RestorePoints_Network_One,
    RestorePoints_Financial_One,
    ReducePoints_Physical_One,
    ReducePoints_Network_One,
    ReducePoints_Financial_One,
    RestorePoints_Physical_Two,
    RestorePoints_Network_Two,
    RestorePoints_Financial_Two,
    ReducePoints_Physical_Two,
    ReducePoints_Network_Two,
    ReducePoints_Financial_Two,
    ReducePointsPerTurn_Physical_One,
    ReducePointsPerTurn_Network_One,
    ReducePointsPerTurn_Financial_One,
    None
}

public class FacilityEffectInstance {
    public FacilityEffect effect;
    public int duration;
    public int stack;
    public List<FacilityEffectInstance> createdEffects;
    FacilityEffectType type;
    public bool negated = false;
    public FacilityEffectInstance negatedEffect = null;  // Track the negated effect

    public FacilityEffectInstance(FacilityEffect effect, int duration, FacilityEffectType type, int stack = 1) {
        this.effect = effect;
        this.duration = duration;
        this.stack = stack;
        createdEffects = new List<FacilityEffectInstance>();
        this.type = type;
    }

    // Method to reduce duration and check if the effect should be removed
    public bool DecreaseDuration() {
        if (duration > 0) {
            duration--;
            if (duration == 0) return true; // Effect expires
        }
        return false; // Effect still active
    }

    public override string ToString() {
        string effectInfo = $"Effect: {effect}, Duration: {(duration == -1 ? "Infinite" : duration.ToString())}, " +
                            $"Stack: {stack}, Negated: {negated}";

        if (createdEffects.Count > 0) {
            effectInfo += "\n  Created Effects: ";
            foreach (var createdEffect in createdEffects) {
                effectInfo += $"\n    {createdEffect.ToString()}";
            }
        }

        return effectInfo;
    }
}

public class FacilityEffectManager {
    private List<FacilityEffectInstance> activeEffects;
    public Facility facility;
    private bool hasNegatedEffectThisRound = false;
    public FacilityEffectManager() {
        activeEffects = new List<FacilityEffectInstance>();
    }
    public List<FacilityEffectInstance> GetEffects() {
        return activeEffects;
    }

    void OnRoundEnded() {
        hasNegatedEffectThisRound = false;
        // Loop through effects, decrease duration, and remove expired ones
        for (int i = activeEffects.Count - 1; i >= 0; i--) {
            FacilityEffectInstance currentEffect = activeEffects[i];

            // Handle ReducePointsPerTurn effects by adding ReducePoints effects
            if (currentEffect.effect == FacilityEffect.ReducePointsPerTurn_Physical_One ||
                currentEffect.effect == FacilityEffect.ReducePointsPerTurn_Network_One ||
                currentEffect.effect == FacilityEffect.ReducePointsPerTurn_Financial_One) {

                // Determine the corresponding ReducePoints effect
                FacilityEffect newEffect = FacilityEffect.None;
                switch (currentEffect.effect) {
                    case FacilityEffect.ReducePointsPerTurn_Physical_One:
                        newEffect = FacilityEffect.ReducePoints_Physical_One;
                        break;
                    case FacilityEffect.ReducePointsPerTurn_Network_One:
                        newEffect = FacilityEffect.ReducePoints_Network_One;
                        break;
                    case FacilityEffect.ReducePointsPerTurn_Financial_One:
                        newEffect = FacilityEffect.ReducePoints_Financial_One;
                        break;
                }

                // Add the new ReducePoints effect
                if (newEffect != FacilityEffect.None) {
                    FacilityEffectInstance newEffectInstance = new FacilityEffectInstance(newEffect, -1, FacilityEffectType.Red); // -1 duration = infinite
                    activeEffects.Add(newEffectInstance);
                    currentEffect.createdEffects.Add(newEffectInstance); // Track the created effect
                    ApplyAddedEffect(newEffect);
                }
            }

            // Handle effect duration decrement and removal
            if (currentEffect.DecreaseDuration()) {
                // Remove any effects created by this one
                foreach (var createdEffect in currentEffect.createdEffects) {
                    activeEffects.Remove(createdEffect);
                }
                activeEffects.RemoveAt(i);
            }
        }
    }
    public bool HasEffect(FacilityEffect effect) {
        return activeEffects.Find(e => e.effect == effect) != null;
    }
    public void RemoveAllEffects() {
        foreach (var effect in activeEffects) {
            RemoveFacilityEffect(effect.effect);
        }
        activeEffects.Clear();
    }
    public void AddEffect(FacilityEffect effect, FacilityEffectType type, int duration = -1) {
        //negate the first effect red effect that is applied if the facility is fortified
        if (facility.IsFortified && type == FacilityEffectType.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }
        FacilityEffectInstance existingEffect = activeEffects.Find(e => e.effect == effect);

        if (existingEffect != null) {
            // If effect already exists, increase the stack count
            existingEffect.stack++;
        }
        else {
            // Add new effect
            activeEffects.Add(new FacilityEffectInstance(effect, duration, type));
        }
        ApplyAddedEffect(effect);
    }

    public void RemoveEffect(FacilityEffect effect) {
        FacilityEffectInstance existingEffect = activeEffects.Find(e => e.effect == effect);

        if (existingEffect != null) {
            if (existingEffect.negatedEffect != null) {
                // If the removed effect is a negate effect, restore the original effect
                FacilityEffectInstance negatedEffect = existingEffect.negatedEffect;
                negatedEffect.negated = false;  // Restore the original effect

                // Reapply the original effect to the facility
                ApplyNegationChangeToFacility(negatedEffect, false);
            }

            if (existingEffect.stack > 1) {
                existingEffect.stack--;
            }
            else {
                // Remove any effects created by this one
                foreach (var createdEffect in existingEffect.createdEffects) {
                    activeEffects.Remove(createdEffect);
                    RemoveFacilityEffect(createdEffect.effect);
                }
                activeEffects.Remove(existingEffect); // Remove the effect completely
            }
        }
        RemoveFacilityEffect(effect);
    }




    #region Effect Switches

    private void ApplyNegationChangeToFacility(FacilityEffectInstance effect, bool negate) {
        // If we are negating the effect, we want to reverse its application
        if (negate) {
            switch (effect.effect) {
                case FacilityEffect.RestorePoints_Physical_One:
                case FacilityEffect.RestorePoints_Network_One:
                case FacilityEffect.RestorePoints_Financial_One:
                case FacilityEffect.RestorePoints_Physical_Two:
                case FacilityEffect.RestorePoints_Network_Two:
                case FacilityEffect.RestorePoints_Financial_Two:
                    // When negated, remove the points it restored
                    ChangeFacilityPoints(effect.effect, true, false); // 'true' to reverse the restore effect
                    break;
                case FacilityEffect.ReducePoints_Physical_One:
                case FacilityEffect.ReducePoints_Network_One:
                case FacilityEffect.ReducePoints_Financial_One:
                case FacilityEffect.ReducePoints_Physical_Two:
                case FacilityEffect.ReducePoints_Network_Two:
                case FacilityEffect.ReducePoints_Financial_Two:
                    // When negated, reverse the point reduction
                    ChangeFacilityPoints(effect.effect, true, true); // 'true' to reverse the reduction effect
                    break;
            }
        }
        else {
            // If we're un-negating, reapply the effect
            switch (effect.effect) {
                case FacilityEffect.RestorePoints_Physical_One:
                case FacilityEffect.RestorePoints_Network_One:
                case FacilityEffect.RestorePoints_Financial_One:
                case FacilityEffect.RestorePoints_Physical_Two:
                case FacilityEffect.RestorePoints_Network_Two:
                case FacilityEffect.RestorePoints_Financial_Two:
                    // Reapply the restore effect
                    ChangeFacilityPoints(effect.effect, false, false); // Reapply the original restore
                    break;
                case FacilityEffect.ReducePoints_Physical_One:
                case FacilityEffect.ReducePoints_Network_One:
                case FacilityEffect.ReducePoints_Financial_One:
                case FacilityEffect.ReducePoints_Physical_Two:
                case FacilityEffect.ReducePoints_Network_Two:
                case FacilityEffect.ReducePoints_Financial_Two:
                    // Reapply the point reduction
                    ChangeFacilityPoints(effect.effect, false, true); // Reapply the original reduction
                    break;
            }
        }
    }
    public void ApplyAddedEffect(FacilityEffect effect) {
        switch (effect) {
            case FacilityEffect.Backdoor:
                AddRemoveBackdoor();
                break;
            case FacilityEffect.Fortify:
                AddRemoveFortify();
                break;
            case FacilityEffect.RestorePoints_Physical_One:
            case FacilityEffect.RestorePoints_Network_One:
            case FacilityEffect.RestorePoints_Financial_One:
            case FacilityEffect.RestorePoints_Physical_Two:
            case FacilityEffect.RestorePoints_Network_Two:
            case FacilityEffect.RestorePoints_Financial_Two:
                ChangeFacilityPoints(effect, false, false);
                break;
            case FacilityEffect.ReducePoints_Physical_One:
            case FacilityEffect.ReducePoints_Network_One:
            case FacilityEffect.ReducePoints_Financial_One:
            case FacilityEffect.ReducePoints_Physical_Two:
            case FacilityEffect.ReducePoints_Network_Two:
            case FacilityEffect.ReducePoints_Financial_Two:
                ChangeFacilityPoints(effect, false, true);
                break;
            case FacilityEffect.ReducePointsPerTurn_Physical_One:   //currently one card does this, adding it to do nothing when it is played, but will reduce points at the end of each round
            case FacilityEffect.ReducePointsPerTurn_Network_One:
            case FacilityEffect.ReducePointsPerTurn_Financial_One:
                break;
        }
    }
    public void RemoveFacilityEffect(FacilityEffect effect) {
        switch (effect) {
            case FacilityEffect.Backdoor:
                AddRemoveBackdoor(remove: true);
                break;
            case FacilityEffect.Fortify:
                AddRemoveBackdoor(remove: true);
                break;
            case FacilityEffect.RestorePoints_Physical_One:
            case FacilityEffect.RestorePoints_Network_One:
            case FacilityEffect.RestorePoints_Financial_One:
            case FacilityEffect.RestorePoints_Physical_Two:
            case FacilityEffect.RestorePoints_Network_Two:
            case FacilityEffect.RestorePoints_Financial_Two:
                ChangeFacilityPoints(effect, true, false);
                break;
            case FacilityEffect.ReducePoints_Physical_One:
            case FacilityEffect.ReducePoints_Network_One:
            case FacilityEffect.ReducePoints_Financial_One:
            case FacilityEffect.ReducePoints_Physical_Two:
            case FacilityEffect.ReducePoints_Network_Two:
            case FacilityEffect.ReducePoints_Financial_Two:
                ChangeFacilityPoints(effect, true, true);
                break;
            case FacilityEffect.ReducePointsPerTurn_Physical_One://currently one card does this, adding it to do nothing when it is played, but will reduce points at the end of each round
            case FacilityEffect.ReducePointsPerTurn_Network_One:
            case FacilityEffect.ReducePointsPerTurn_Financial_One:
                break;
        }
    }
    #endregion

    #region Effect Functions
    void ChangeFacilityPoints(FacilityEffect effect, bool remove = false, bool negative = false) {
        string effectString = effect.ToString();
        var info = effectString.Split('_');
        string target = info[1];
        int value = int.Parse(info[2]) * (negative ? -1 : 1) * (remove ? -1 : 1);

        facility.ChangeFacilityPoints(target, value);
    }

    void AddRemoveBackdoor(bool remove = false) {
        // effect = effectToAdd;
        facility.effectIcon.sprite = remove ? null : Sector.EffectSprites[0];
        facility.IsBackdoored = !remove;
        facility.ToggleEffectImageAlpha(); //enable image TODO: might not do this every time if you are replacing a fortify with backdoor or vice versa
    }
    void AddRemoveFortify(bool remove = false) {
        facility.effectIcon.sprite = remove ? null : Sector.EffectSprites[1];
        facility.IsFortified = !remove;
        RemoveAllEffects(); //fortify removes all other effects
        facility.ToggleEffectImageAlpha();
    }
    public void NegateEffect(FacilityEffect effect, FacilityEffectType type) {
        // TODO: does fortify block the negation? can you negate fortify?
        if (facility.IsFortified && type == FacilityEffectType.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return; //lets say no for now
        }
        FacilityEffectInstance targetEffect = activeEffects.Find(e => e.effect == effect);

        if (targetEffect != null && !targetEffect.negated) {
            // Create a new negate effect instance
            FacilityEffectInstance negateEffectInstance = new FacilityEffectInstance(FacilityEffect.None, -1, FacilityEffectType.Red);  // Define a Negate type if needed
            negateEffectInstance.negatedEffect = targetEffect;
            targetEffect.negated = true; // Mark the original effect as negated

            // Apply the negation change to the facility (temporarily remove effect)
            ApplyNegationChangeToFacility(targetEffect, true);

            // Add the negate effect to the list to track it
            activeEffects.Add(negateEffectInstance);
        }
    }



    #endregion
}
