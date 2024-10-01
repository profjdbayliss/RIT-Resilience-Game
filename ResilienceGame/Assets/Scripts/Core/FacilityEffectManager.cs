using System.Collections.Generic;
using System.Linq;
using Unity;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class belongs to a facility and manages the effects that are applied to it
/// </summary>
public class FacilityEffectManager : MonoBehaviour {
    private readonly List<(FacilityEffect Effect, FacilityEffectUIElement UIElement)> activeEffects = new List<(FacilityEffect, FacilityEffectUIElement)>();
    private Facility facility;
    private bool hasNegatedEffectThisRound = false;
    [SerializeField] private Transform effectParent;
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private Image effectIcon;


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

        activeEffects.Add((effect, null));//add the effect to list
        UpdateEffectUI(effect);
        ApplyEffect(effect);
    }
    public void UpdateSpecialIcon(FacilityEffect effect, bool add = true) {
        if (effect.EffectType == FacilityEffectType.Backdoor || effect.EffectType == FacilityEffectType.Fortify) {
            if (add) {
                effectIcon.sprite = Sector.EffectSprites[(int)effect.EffectType];
            }
            ToggleEffectImageAlpha();
        }
    }
    public void UpdateEffectUI(FacilityEffect effect, bool add = true) {
        if (effect.EffectType == FacilityEffectType.Backdoor || effect.EffectType == FacilityEffectType.Fortify) {
            UpdateSpecialIcon(effect, add);
            return;
        }

        int indexToUpdate = activeEffects.FindIndex(e => e.Effect.UniqueID == effect.UniqueID);

        if (add) {
            if (indexToUpdate != -1) {
                // Effect exists, update or create its UI element
                var (existingEffect, existingUI) = activeEffects[indexToUpdate];
                if (existingUI == null) {
                    // Create new UI element
                    var newEffectUI = Instantiate(effectPrefab, effectParent).GetComponent<FacilityEffectUIElement>();
                    newEffectUI.SetEffectType(effect.Target, effect.Magnitude);
                   
                    activeEffects[indexToUpdate] = (existingEffect, newEffectUI);
                }
                else {
                    // Update existing UI element
                    existingUI.SetEffectType(effect.Target, effect.Magnitude);
                    
                }
            }
            else {
                Debug.LogError("Shouldn't ever happen - probably some effect UID error");
            }
        }
        else {
            // Remove the effect element
            if (indexToUpdate != -1) {
                var (_, uiElement) = activeEffects[indexToUpdate];
                activeEffects.RemoveAt(indexToUpdate);
                if (uiElement != null) {
                    Destroy(uiElement.gameObject);
                }
            }
        }
    }
    public void ToggleEffectImageAlpha() {
        Color color = effectIcon.color;
        var newColor = color.a == 1 ? new Color(color.r, color.g, color.b, 0f) : new Color(color.r, color.g, color.b, 1);
        effectIcon.color = newColor;
    }
    /// <summary>
    /// Removes an effect from the facility
    /// </summary>
    /// <param name="effect">The effect to remove</param>
    private void RemoveEffect(FacilityEffect effect, bool bypassFortified = false) {
        if (!bypassFortified) {
            if (facility.IsFortified && effect.CreatedByTeam == FacilityTeam.Red && !hasNegatedEffectThisRound) {
                hasNegatedEffectThisRound = true;
                return;
            }
        }
        int indexToRemove = activeEffects.FindIndex(e => e.Effect.UniqueID == effect.UniqueID);
        if (indexToRemove == -1) {
            Debug.LogError("Trying to remove an effect that doesn't exist [Probably UID issue]");
            return;
        }
        var (effectToRemove, uiElement) = activeEffects[indexToRemove];

        if (effectToRemove != null) {
            Destroy(uiElement.gameObject);
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
            default:
                break;
        }
    }
    /// <summary>
    /// Called when the round is ended by the game manager
    /// </summary>
    public void OnRoundEnded() {
        foreach (var (effect, uiElement) in activeEffects.ToList()) {
            if (effect.EffectType == FacilityEffectType.ModifyPointsPerTurn) {
                var effects = FacilityEffect.CreateEffectsFromID(effect.CreatedEffectID);
                effects.ForEach(_effect => AddEffect(_effect));
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

    public bool HasEffectOfType(FacilityEffectType type) {
        return activeEffects.Any(effect => effect.Effect.EffectType == type);
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
        Debug.Log($"Changing facility points for {facility.facilityName} by {effect.Magnitude} for {effect.Target}");
        int value = effect.Magnitude * (isRemoving ? -1 : 1);

        facility.ChangeFacilityPoints(effect.Target, value);
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
