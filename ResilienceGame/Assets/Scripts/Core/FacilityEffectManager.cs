using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Facility;

/// <summary>
/// This class belongs to a facility and manages the effects that are applied to it
/// </summary>
public class FacilityEffectManager : MonoBehaviour {
    #region Fields
    private readonly List<FacilityEffect> activeEffects = new List<FacilityEffect>();
    private Facility facility;
    private bool hasNegatedEffectThisRound = false;
    [SerializeField] private Transform effectParent;
    [SerializeField] private GameObject effectPrefab;
    private RectTransform facilityEffectMenu;
    [SerializeField] private RectTransform effectBoxParent;
    //  public EffectPopoutState effectTargetState = EffectPopoutState.Hide;
    //  private EffectPopoutState effectPopoutState = EffectPopoutState.Hide;
    private Vector2 effectHiddenPos;
    private float effectAnimationDuration = .75f;
    private float effectPopoutDistance = 120f;
    public Coroutine effectPopoutRoutine;
    //[SerializeField] private Image effectIcon;
    //[SerializeField] private GameObject counterBackground;
    //  [SerializeField] private TextMeshProUGUI counterText;

    // [SerializeField] private GameObject uiElementPrefab;
    //  [SerializeField] private Transform uiElementParent;

    public static Sprite[] EffectSprites { get => Sector.EffectSprites; }
    private int counter = 0;

    #endregion

    #region Start
    private void Start() {
        facility = GetComponent<Facility>();
        facilityEffectMenu = effectParent.GetComponent<RectTransform>();
        effectHiddenPos = effectBoxParent.anchoredPosition;
    }
    #endregion

    

    #region Debug

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
    #endregion

    #region Helpers
    private void ChangeFacilityPoints(FacilityEffect effect, bool isRemoving = false) {
        // Debug.Log($"Changing facility points for {facility.facilityName} by {effect.Magnitude} for {effect.Target}");
        int value = effect.Magnitude * (isRemoving ? 0 : 1); //dont give points back when removing effects

        facility.ChangeFacilityPoints(effect.Target, value);
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
    //returns a list of effects that can be removed,
    //these are effects marked with the correct type that are created by the opponent team
    public List<FacilityEffect> GetRemoveableEffects(PlayerTeam playerTeam, bool removePointsPerTurnEffects = false) {
        var opponentTeam = playerTeam == PlayerTeam.Red ? PlayerTeam.Blue : PlayerTeam.Red;
        return GetEffectsCreatedByTeam(opponentTeam).Where(effect => effect.IsRemoveable).ToList();
    }
    public List<FacilityEffect> GetEffectsCreatedByTeam(PlayerTeam team) {
        return activeEffects.Where(effect => effect.CreatedByTeam == team).ToList();
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

    #endregion

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
    
    #region Remove Effects
    void RemoveNegativeEffects() {
        //remove backdoor or points per turn effects
        activeEffects.Where(
            effect => effect.HasUIElement &&
            (effect.EffectType == FacilityEffectType.ModifyPointsPerTurn || effect.EffectType == FacilityEffectType.Backdoor))
            .ToList()
            .ForEach(effect => UpdateUI(effect, false));

        activeEffects.RemoveAll(effect => effect.EffectType == FacilityEffectType.ModifyPointsPerTurn || effect.EffectType == FacilityEffectType.Backdoor);
    }
    private void RemoveAllEffects() {
        while (activeEffects.Count > 0) {
            AddRemoveEffect(activeEffects[0], false);
        }
    }
    /// <summary>
    /// Removes the effect from the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void UnapplyEffect(FacilityEffect effect) {
        switch (effect.EffectType) {
            case FacilityEffectType.ModifyPoints:
                ChangeFacilityPoints(effect, isRemoving: true);
                break;

            case FacilityEffectType.ModifyPointsPerTurn:
            case FacilityEffectType.Backdoor:
            case FacilityEffectType.Fortify:
                //UpdateSpecialIcon(effect, false);
                break;
            default:
                break;
        }
        UpdateUI(effect, false);
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
    #endregion

    #region Add Effects
    
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

        if (effect.EffectType == FacilityEffectType.Backdoor && IsBackdoored()) {
            activeEffects.Find(activeEffects => activeEffects.EffectType == FacilityEffectType.Backdoor).Duration = effect.Duration;
            return;
        }
        else if (effect.EffectType == FacilityEffectType.Fortify && IsFortified()) {
            activeEffects.Find(activeEffects => activeEffects.EffectType == FacilityEffectType.Fortify).Duration = effect.Duration;

            return;
        }


        activeEffects.Add(effect);//add the effect to list
        //UpdateEffectUI(effect);
        ApplyEffect(effect);
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
                // UpdateSpecialIcon(effect);
                break;
            case FacilityEffectType.Fortify:
                //if (IsBackdoored()) {
                //    // ToggleEffectImageAlpha();
                //}
                RemoveNegativeEffects();

                break;
            default:
                break;
        }
        UpdateUI(effect, true);

    }
    #endregion

    #region Interface Updates
    private void UpdateUI(FacilityEffect effect, bool add) {
        // Debug.Log($"Updating UI element for effect {effect.EffectType}");
        if (!effect.HasUIElement) return;
        if (effectPopoutRoutine != null) {
            StopCoroutine(effectPopoutRoutine);
        }

        if (add) {

            var facilityEffectUI = Instantiate(effectPrefab, effectParent).GetComponent<FacilityEffectUIElement>();
            effect.UIElement = facilityEffectUI;
            facilityEffectUI.Init(effect);
            effectBoxParent.anchoredPosition = effectHiddenPos - new Vector2(0, effectPopoutDistance);
            effectPopoutRoutine = StartCoroutine(MoveUI(effectBoxParent, 
                effectHiddenPos, 
                effectHiddenPos - new Vector2(0, effectPopoutDistance), 
                null));


        }
        else {
            if (!activeEffects.Any(effect => effect.HasUIElement)) {
                effectBoxParent.anchoredPosition = effectHiddenPos;
                effectPopoutRoutine = StartCoroutine(MoveUI(effectBoxParent,
                    effectHiddenPos - new Vector2(0, effectPopoutDistance),
                    effectHiddenPos,
                    effect.UIElement.gameObject));
            }
            
        }
    }
    public void ToggleEffectImageAlpha(Image effectIcon) {
        Color color = effectIcon.color;
        var newColor = color.a == 1 ? new Color(color.r, color.g, color.b, 0f) : new Color(color.r, color.g, color.b, 1);
        effectIcon.color = newColor;

        //  Debug.Log($"Toggling effect icon alpha to {newColor.a}");
    }

    #region Effect Menu
    private IEnumerator MoveUI(RectTransform rectTransform, Vector2 startPos, Vector2 endPos, GameObject objectToDestroy) {
        float elapsedTime = 0f;

        while (elapsedTime < effectAnimationDuration) {
            // Calculate the cubic eased value
            float t = elapsedTime / effectAnimationDuration;
            t = CubicEaseInOut(t);

            // Lerp position based on cubic easing
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final position is the target position
        rectTransform.anchoredPosition = endPos;
        if (endPos == effectHiddenPos) {
            Destroy(objectToDestroy);
        }
        
        Debug.Log("Finished moving effect menu");
    }
    private float CubicEaseInOut(float t) {
        if (t < 0.5f) {
            return 4f * t * t * t; // ease-in
        }
        else {
            t = (t - 1f);
            return 1f + 4f * t * t * t; // ease-out
        }
    }
    #endregion
    #endregion

    #region End Round
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
                if (effect.HasUIElement) {
                    effect.UIElement.SetCounterText(effect.Duration.ToString());
                }
                // DecrementCounter();

                if (effect.Duration == 0) {
                    ForceRemoveEffect(effect);
                }
            }
        }
        hasNegatedEffectThisRound = false;
    }
    #endregion

}
