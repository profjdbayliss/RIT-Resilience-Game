using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    [SerializeField] private GameObject counterBackground;
    [SerializeField] private TextMeshProUGUI counterText;

    //indicies from the sprite sheet
    private const int PHYSICAL_INDEX = 8;
    private const int FINANCIAL_INDEX = 5;
    private const int NETWORK_INDEX = 7;
    private const int FINANCIAL_NETWORK_INDEX = 1;
    private const int PHYSICAL_FINANCIAL_INDEX = 2;
    private const int PHYSICAL_NETWORK_INDEX = 3;
    private const int ALL_INDEX = 4;
    private const int BACKDOOR_INDEX = 6;
    private const int FORTIFY_INDEX = 0;



    public static Sprite[] EffectSprites { get => Sector.EffectSprites; }
    private int counter = 0;
    private void Start() {
        facility = GetComponent<Facility>();
    }


    public (string, Sprite) GetEffectSprite(FacilityEffect effect) {
        if (effect.EffectType == FacilityEffectType.Backdoor) {
            return ("backdoor", EffectSprites[BACKDOOR_INDEX]);
        }
        if (effect.EffectType == FacilityEffectType.Fortify) {
            return ("fortify", EffectSprites[FORTIFY_INDEX]);
        }
        return GetEffectSpriteByPointTarget(effect.Target);
    }
    private static (string, Sprite) GetEffectSpriteByPointTarget(FacilityPointTarget effectTarget) {
        // Default to "all" if all points are affected
        if (effectTarget == FacilityPointTarget.All) {
            return ("all", EffectSprites[ALL_INDEX]);
        }

        // Return null if no effect is applied
        if (effectTarget == FacilityPointTarget.None) {
            return ("", null);
        }

        int combinedIndex = 0;
        if (effectTarget.HasFlag(FacilityPointTarget.Physical)) combinedIndex |= 1;
        if (effectTarget.HasFlag(FacilityPointTarget.Financial)) combinedIndex |= 2;
        if (effectTarget.HasFlag(FacilityPointTarget.Network)) combinedIndex |= 4;

        return combinedIndex switch {
            1 => ("physical", EffectSprites[PHYSICAL_INDEX]),               // Physical
            2 => ("financial", EffectSprites[FINANCIAL_INDEX]),              // Financial
            4 => ("network", EffectSprites[NETWORK_INDEX]),                // Network
            3 => ("physical and financial", EffectSprites[PHYSICAL_FINANCIAL_INDEX]),     // Physical and Financial
            5 => ("physical and network", EffectSprites[PHYSICAL_NETWORK_INDEX]),       // Physical and Network
            6 => ("financial and network", EffectSprites[FINANCIAL_NETWORK_INDEX]),      // Financial and Network
            _ => ("all", EffectSprites[ALL_INDEX]),                    // Default to "all"
        };

    }

#if UNITY_EDITOR
    public void DebugAddEffect() {
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
        AddRemoveEffect(FacilityEffect.CreateEffectsFromID(allEffects[Random.Range(0, allEffects.Count)])[0], true); //add a random effect from the list
    }
#endif
    public List<FacilityEffect> GetEffects() {
        return activeEffects.Select(effect => effect.Effect).ToList();
    }
    public List<(FacilityEffect Effect, FacilityEffectUIElement UIElement)> GetEffectsCreatedByTeam(PlayerTeam team) {
        return activeEffects.Where(effect => effect.Effect.CreatedByTeam == team).ToList();
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
        if (IsFortified() && effect.CreatedByTeam == PlayerTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            return;
        }

        activeEffects.Add((effect, null));//add the effect to list
        //UpdateEffectUI(effect);
        ApplyEffect(effect);
    }
    public void UpdateSpecialIcon(FacilityEffect effect, bool add = true) {
        if (effect.EffectType == FacilityEffectType.Backdoor || effect.EffectType == FacilityEffectType.Fortify) {
            if (add) {
                //Debug.Log($"Adding special effect icon to facility");
                effectIcon.sprite = Sector.EffectSprites[(int)effect.EffectType];
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
                    newEffectUI.SetIconAndText(GetEffectSprite(effect), effect.Magnitude);

                    activeEffects[indexToUpdate] = (existingEffect, newEffectUI);
                }
                else {
                    // Update existing UI element
                    existingUI.SetIconAndText(GetEffectSprite(effect), effect.Magnitude);

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
        UpdateEffectUI(effect, true);
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
    public void UpdateForNextActionPhase() {
        // Debug.Log($"Updating for new action phase for Facility {facility.facilityName}");
        //update all effects
        foreach (var (effect, uiElement) in activeEffects.ToList()) {
            //only update effects that are created by the team whos turn it is
            //this will be called twice once in action red and once in action blue
            //we need to update the correct effects on both clients each phase
            if (!IsEffectCreatorsTurn(effect)) return;

            if (effect.EffectType == FacilityEffectType.ModifyPointsPerTurn) {
                var effects = FacilityEffect.CreateEffectsFromID(effect.CreatedEffectID);
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
        Debug.Log($"Checking if facility {facility.facilityName} has effects by {opponentTeam}");
        activeEffects.ForEach(effect => Debug.Log($"{effect.Effect.EffectType} created by {effect.Effect.CreatedByTeam}"));
        return activeEffects.Any(effect => effect.Effect.CreatedByTeam == opponentTeam);
    }
    public bool HasEffectOfType(FacilityEffectType type) {
        return activeEffects.Any(effect => effect.Effect.EffectType == type);
    }
    public bool IsFortified() {
        return activeEffects.Any(effect => effect.Effect.EffectType == FacilityEffectType.Fortify);
    }
    public void RemoveAllEffects() {
        foreach (var (_, uiElement) in activeEffects) {
            Destroy(uiElement.gameObject);
        }
        activeEffects.Clear();
    }
    public void ToggleAllEffectOutlines(bool enable, PlayerTeam opponentTeam) {
        foreach (var (effect, uiElement) in activeEffects) {
            if (effect.CreatedByTeam == opponentTeam)
                uiElement.ToggleOutline(enable);
        }
    }

    #region Effect Functions

    private void ApplyNegationChangeToFacility(FacilityEffect effect, bool negate) {
        if (negate) {
            ChangeFacilityPoints(effect, true); // Reverse effect
        }
        else {
            ChangeFacilityPoints(effect); // Reapply effect
        }
    }


    private void ChangeFacilityPoints(FacilityEffect effect, bool isRemoving = false) {
        // Debug.Log($"Changing facility points for {facility.facilityName} by {effect.Magnitude} for {effect.Target}");
        int value = effect.Magnitude * (isRemoving ? -1 : 1);

        facility.ChangeFacilityPoints(effect.Target, value);
    }
    public void NegateEffect(FacilityEffect effect) {
        if (IsFortified() && effect.EffectType == FacilityEffectType.ModifyPoints && effect.Magnitude < 0 && !hasNegatedEffectThisRound) {
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
