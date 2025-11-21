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
    private List<FacilityEffectUIElement> uiElements = new List<FacilityEffectUIElement>();
    private Facility facility;
    private bool hasNegatedEffectThisRound = false;
    [SerializeField] private Transform effectParent;
    [SerializeField] private GameObject effectPrefab;
    private RectTransform facilityEffectMenu;
    [SerializeField] private RectTransform effectBoxParent;
    //  public EffectPopoutState effectTargetState = EffectPopoutState.Hide;
    //  private EffectPopoutState effectPopoutState = EffectPopoutState.Hide;
    private Vector2 effectHiddenPos;
    private readonly float effectAnimationDuration = .75f;
    private readonly float effectPopoutDistance = 120f;
    public Coroutine effectPopoutRoutine;
    public enum EffectMenuState {
        Opening,
        Closing,
        Open,
        Closed
    }
    public EffectMenuState effectMenuState = EffectMenuState.Closed;
    public EffectMenuState targetMenuState = EffectMenuState.Closed;
    public Queue<Action> QueuedUIUpdates = new Queue<Action>();


    //[SerializeField] private Image effectIcon;
    //[SerializeField] private GameObject counterBackground;
    //  [SerializeField] private TextMeshProUGUI counterText;

    // [SerializeField] private GameObject uiElementPrefab;
    //  [SerializeField] private Transform uiElementParent;

    public static Sprite[] EffectSprites { get => Sector.EffectSprites; }


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
        //var allEffects = new List<string>() {
        //    "modp;net;1",
        //    "modp;phys;1",
        //    "modp;fin;1",
        //    "modp;all;1",
        //    "modp;fin&net;1",
        //    "modp;phys&net;1",
        //    "fortify",
        //    "backdoor",
        //    "modp;net;-1",
        //    "modp;phys;-1",
        //    "modp;fin;-1",
        //    "modp;all;-1",
        //    "modp;phys&net;-1",
        //    "modp;phys&fin;-1",
        //    "modp;fin&net;-1",
        //};
        ////foreach (Sprite sprite in EffectSprites) {
        ////    AddRemoveEffect(FacilityEffect.CreateEffectsFromID($"modp;net;1")[0], true);
        ////}

        //Debug.Log($"DEBUG: Adding facility effect with id: {effectId}");
        //if (effectId == "")
        //    AddRemoveEffect(FacilityEffect.CreateEffectsFromID(allEffects[UnityEngine.Random.Range(0, allEffects.Count)])[0], true); //add a random effect from the list
        //else
        //    AddRemoveEffect(FacilityEffect.CreateEffectsFromID(effectId)[0], true);
    }
    #endregion

    #region Helpers
    public static bool IsEffectObfuscated(FacilityEffectType type) =>
       type switch {
           FacilityEffectType.HoneyPot => true,
           _ => false
       };
    public void CheckForScoring(FacilityEffect effect, bool isAdding, int createdById) {
        if (!GameManager.Instance.playerDictionary.ContainsKey(createdById)) return;

        if (isAdding) {
            if (effect.IsRestoreEffect) {
                //restore points to a core sector
                if (facility.sectorItsAPartOf.isCore) {
                    ScoreManager.Instance.AddCoreFacilitySupport(createdById);
                }
                else {
                    //bring a non-core sector back up
                    if (facility.IsDown) {
                        ScoreManager.Instance.AddFacilityRestoration(createdById);
                    }
                }
            }
            else {
                switch (effect.EffectType) {
                    case FacilityEffectType.Fortify: //fortify
                        ScoreManager.Instance.AddFortification(createdById);
                        break;
                    case FacilityEffectType.ModifyPoints:
                        if (WillEffectDownFacility(effect)) {
                            if (facility.sectorItsAPartOf.isCore) {
                                ScoreManager.Instance.AddCoreFacilityTakeDown(createdById);
                                ScoreManager.Instance.AddDoomClockActivateionPersonal(createdById);
                            }
                            ScoreManager.Instance.AddFacilityTakeDown(createdById);

                            //check if this will start Doom clock
                            if (GameManager.Instance.NumDownedSectors + 1 > GameManager.Instance.AllSectors.Count / 2) {
                                ScoreManager.Instance.AddDoomClockActivateionPersonal(createdById);
                            }
                        }
                        break;
                    case FacilityEffectType.Backdoor:
                        ScoreManager.Instance.AddBackdoorCreation(createdById);
                        break;
                }

            }
        }
    }
    public bool CheckForTrapEffects(int createdById) {
        if (!GameManager.Instance.playerDictionary.ContainsKey(createdById)) return false;

        Debug.Log($"Checking for trap effects that would trigger from {createdById}'s card play");
        List<FacilityEffect> trapEffects = activeEffects.Where(effect => effect.HasTrap).ToList();
        List<FacilityEffect> trapEffectsFromOpposingTeam = trapEffects.Where(effect => effect.CreatedByTeam != GameManager.Instance.GetPlayerTeam(createdById)).ToList();
        if (trapEffectsFromOpposingTeam.Any()) {
            trapEffectsFromOpposingTeam.ForEach(trapEffect => {
                //   trapEffect.OnEffectRemoved?.Invoke(createdById); //triggered on removal
                Debug.Log($"Triggering trap effect: {trapEffect.EffectType}");
                // Display honeypot card animation when honeypot is triggered
                if (trapEffect.EffectType == FacilityEffectType.HoneyPot) {
                    DisplayHoneypotCard(trapEffect);
                }
                RemoveEffect(trapEffect, createdById, true); //remove the trap effect
            });
            return true;
        }
        return false;
    }

    private void DisplayHoneypotCard(FacilityEffect honeypotEffect) {
        // Find the honeypot card from the static cards dictionary
        Card honeypotCardTemplate = null;
        foreach (var cardPair in CardPlayer.cards) {
            if (cardPair.Value.data.effectString != null && 
                cardPair.Value.data.effectString.ToLower().Contains("honeypot")) {
                honeypotCardTemplate = cardPair.Value;
                break;
            }
        }

        if (honeypotCardTemplate == null) {
            Debug.LogWarning("Could not find honeypot card for display");
            return;
        }

        // Get the player who created the honeypot effect
        CardPlayer honeypotCreator = null;
        if (GameManager.Instance.playerDictionary.TryGetValue(honeypotEffect.CreatedByPlayerID, out honeypotCreator)) {
            // Create a temporary card GameObject for animation
            GameObject tempCardObj = Instantiate(honeypotCreator.cardPrefab);
            Card tempCard = tempCardObj.GetComponent<Card>();
            tempCard.data = honeypotCardTemplate.data;
            tempCard.ActionList = new List<ICardAction>(honeypotCardTemplate.ActionList);
            tempCard.target = honeypotCardTemplate.target;
            tempCard.DeckName = honeypotCardTemplate.DeckName;
            tempCard.UniqueID = GameManager.Instance.UniqueCardIdCount++;

            // Set up the card front by copying from the template
            CardFront templateFront = honeypotCardTemplate.GetComponent<CardFront>();
            tempCard.front = templateFront;

            // Copy visual elements from the template card
            RawImage[] tempRaws = tempCardObj.GetComponentsInChildren<RawImage>();
            for (int i = 0; i < tempRaws.Length; i++) {
                if (tempRaws[i].name == "Image") {
                    tempRaws[i].texture = templateFront.img;
                }
                else if (tempRaws[i].name == "Background") {
                    tempRaws[i].color = templateFront.color;
                }
            }

            // Set up text elements
            TextMeshProUGUI[] tempTexts = tempCardObj.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < tempTexts.Length; i++) {
                if (tempTexts[i].name.Equals("Title Text")) {
                    tempTexts[i].text = templateFront.title;
                }
                else if (tempTexts[i].name.Equals("Description Text")) {
                    tempTexts[i].text = templateFront.description;
                }
                else if (tempTexts[i].name.Equals("Flavor Text")) {
                    tempTexts[i].text = templateFront.flavor;
                }
            }

            // Add to honeypot creator's hand temporarily so it can be displayed
            honeypotCreator.HandCards.Add(tempCard.UniqueID, tempCardObj);
            tempCardObj.SetActive(true);

            // Display the card animation through the facility's sector
            if (facility != null && facility.sectorItsAPartOf != null) {
                facility.sectorItsAPartOf.StartCoroutine(
                    DisplayHoneypotCardAnimation(tempCard, honeypotCreator, facility)
                );
            }
        }
    }

    private IEnumerator DisplayHoneypotCardAnimation(Card card, CardPlayer player, Facility facility) {
        if (!player.HandCards.TryGetValue(card.UniqueID, out GameObject cardObject)) {
            yield break;
        }

        RectTransform cardRect = cardObject.GetComponent<RectTransform>();
        
        // Set the card's parent to nothing, in order to position it in world space
        cardRect.SetParent(null, true);
        Vector2 topMiddle = new Vector2(Screen.width / 2, Screen.height + cardRect.rect.height / 2);
        cardRect.anchoredPosition = topMiddle;
        card.transform.localRotation = Quaternion.Euler(0, 0, 180); // flip upside down as if played by opponent
        cardRect.SetParent(facility.sectorItsAPartOf.SectorCanvas.transform, true);
        cardObject.SetActive(true);

        // Start the card animation
        yield return card.MoveAndRotateToCenter(
            rectTransform: cardRect,
            facilityTarget: facility.gameObject,
            onComplete: () => {
                Debug.Log($"Honeypot card animation complete for {card.data.name}");
                // Remove the card from the player's hand
                player.HandCards.Remove(card.UniqueID);
            },
            scaleUpFactor: 1.0f
        );
    }
    private bool WillEffectDownFacility(FacilityEffect effect) =>


            (effect.EffectType == FacilityEffectType.ModifyPoints) && effect.Target switch {
                FacilityEffectTarget.Physical => facility.Points[0] + effect.Magnitude <= 0,
                FacilityEffectTarget.Network => facility.Points[1] + effect.Magnitude <= 0,
                FacilityEffectTarget.Financial => facility.Points[2] + effect.Magnitude <= 0,
                FacilityEffectTarget.All => facility.Points[0] + effect.Magnitude <= 0 &&
                                           facility.Points[1] + effect.Magnitude <= 0 &&
                                           facility.Points[2] + effect.Magnitude <= 0,
                FacilityEffectTarget.FinancialPhysical => facility.Points[0] + effect.Magnitude <= 0 &&
                                                          facility.Points[2] + effect.Magnitude <= 0,
                FacilityEffectTarget.NetworkPhysical => facility.Points[0] + effect.Magnitude <= 0 &&
                                                         facility.Points[1] + effect.Magnitude <= 0,
                FacilityEffectTarget.FinancialNetwork => facility.Points[1] + effect.Magnitude <= 0 &&
                                                        facility.Points[2] + effect.Magnitude <= 0,
                _ => false
            };
    private void ChangeFacilityPoints(FacilityEffect effect, int createdById, bool isRemoving = false) {
        // Debug.Log($"Changing facility points for {facility.facilityName} by {effect.Magnitude} for {effect.Target}");
        int value = effect.Magnitude * (isRemoving ? 0 : 1); //dont give points back when removing effects
        if (effect.Magnitude < 0 && GameManager.Instance.IsRedLayingLow)
            GameManager.Instance.IsRedLayingLow = false;
        Debug.Log(effect.ToString());

        var protectPointsEffects = activeEffects.FindAll(effect => effect.EffectType == FacilityEffectType.ProtectPoints).Select(effect=>effect.Target);


        //convert possible multi point target to a list of single point targets
        List<FacilityEffectTarget> effectTargets = new List<FacilityEffectTarget>();

        switch (effect.Target) {
            case FacilityEffectTarget.Physical:
                effectTargets.Add(FacilityEffectTarget.Physical);
                break;
            case FacilityEffectTarget.Financial:
                effectTargets.Add(FacilityEffectTarget.Financial);
                break;
            case FacilityEffectTarget.Network:
                effectTargets.Add(FacilityEffectTarget.Network);
                break;
            case FacilityEffectTarget.NetworkPhysical:
                effectTargets.Add(FacilityEffectTarget.Network);
                effectTargets.Add(FacilityEffectTarget.Physical);
                break;
            case FacilityEffectTarget.FinancialPhysical:
                effectTargets.Add(FacilityEffectTarget.Financial);
                effectTargets.Add(FacilityEffectTarget.Physical);
                break;
            case FacilityEffectTarget.FinancialNetwork:
                effectTargets.Add(FacilityEffectTarget.Financial);
                effectTargets.Add(FacilityEffectTarget.Network);
                break;
            case FacilityEffectTarget.All:
                effectTargets.Add(FacilityEffectTarget.Financial);
                effectTargets.Add(FacilityEffectTarget.Physical);
                effectTargets.Add(FacilityEffectTarget.Network);
                break;
        }


        //check for any protection points effects that would prevent this effect from taking place
        foreach (var target in effectTargets) {
            bool add = true;


            foreach (var protectedPoint in protectPointsEffects) {
            
                //protected point
                if (effect.Magnitude < 0 && target == protectedPoint) {
                    add = false;
                    break;
                }
                
               
            }
            //unprotected point
            if (add)
                facility.ChangeFacilityPoints(target, createdById, value);
        }

        

    }
    
    private bool IsEffectCreatorsTurn(FacilityEffect effect) {
        //Debug.Log($"Checking if {effect.EffectType} created by the {effect.CreatedByTeam} team should be adjusted during {GameManager.instance.MGamePhase} phase");
        return effect.CreatedByTeam switch {
            PlayerTeam.Red => GameManager.Instance.MGamePhase == GamePhase.ActionRed,
            PlayerTeam.Blue => GameManager.Instance.MGamePhase == GamePhase.ActionBlue,
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

    public bool IsHoneyPotted() {
        return activeEffects.Any(effect => effect.EffectType == FacilityEffectType.HoneyPot);
    }

    //returns a list of effects that can be removed,
    //these are effects marked with the correct type that are created by the opponent team
    public List<FacilityEffect> GetEffectsRemovableByTeam(PlayerTeam playerTeam, bool removePointsPerTurnEffects = false) {
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
    public void AddRemoveEffect(List<FacilityEffect> effects, bool isAdding, int createdById) {
        Debug.Log($"Adding or removing {effects.Count} effects to {facility.facilityName} from {createdById}");
        if (CheckForTrapEffects(createdById)) {
            return;
        }

        effects.ForEach(effect => {
            CheckForScoring(effect, isAdding, createdById);
            //check for trap effects here
            if (isAdding) {
                AddEffect(effect, createdById);
            }
            else {
                RemoveEffect(effect, createdById);
            }
        });
    }

    #region Remove Effects
    void RemoveNegativeEffects(int calledById) {
        var negativeEffects = activeEffects
            .Where(effect => effect.CreatedByTeam == PlayerTeam.Red).ToList();
        Debug.Log("Removing negative effects: " + negativeEffects.Count);
        negativeEffects.ForEach(negativeEffects => Debug.Log(negativeEffects.EffectType));
        negativeEffects.ForEach(effect => RemoveEffect(effect, calledById, true));
    }
    private void RemoveAllEffects(int calledById) {

        AddRemoveEffect(activeEffects, false, calledById);

    }
    /// <summary>
    /// Removes the effect from the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void UnapplyEffect(FacilityEffect effect, int createdById) {
        switch (effect.EffectType) {
            case FacilityEffectType.ModifyPoints:
                ChangeFacilityPoints(effect, createdById, isRemoving: true);
                break;
            case FacilityEffectType.ProtectPoints:
                bool protectPhys = effect.Target == FacilityEffectTarget.Physical ||
                                   effect.Target == FacilityEffectTarget.FinancialPhysical ||
                                   effect.Target == FacilityEffectTarget.NetworkPhysical ||
                                   effect.Target == FacilityEffectTarget.All;
                bool protectNet = effect.Target == FacilityEffectTarget.Network ||
                                   effect.Target == FacilityEffectTarget.FinancialNetwork ||
                                   effect.Target == FacilityEffectTarget.NetworkPhysical ||
                                   effect.Target == FacilityEffectTarget.All;
                bool protectFin = effect.Target == FacilityEffectTarget.Financial ||
                                   effect.Target == FacilityEffectTarget.FinancialNetwork ||
                                   effect.Target == FacilityEffectTarget.FinancialPhysical ||
                                   effect.Target == FacilityEffectTarget.All;

                facility.DisableSinglePointProtection(protectPhys, protectNet, protectFin);
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
    private void RemoveEffect(FacilityEffect effect, int createdById, bool bypassFortified = false) {
        if (!bypassFortified) {
            if (facility.IsFortified() && GameManager.Instance.GetPlayerTeam(createdById) == PlayerTeam.Blue && !hasNegatedEffectThisRound) {
                hasNegatedEffectThisRound = true;
                ScoreManager.Instance.AddSuccessfulDefense(
                    activeEffects.Find(e => e.EffectType == FacilityEffectType.Fortify).CreatedByPlayerID
                    );
                return;
            }
        }
        //bypass fortify on second effect played on this facility
        if (IsFortified() && effect.CreatedByTeam == PlayerTeam.Blue && hasNegatedEffectThisRound) {
            ScoreManager.Instance.AddFortifyOvercome(createdById);
        }
        int indexToRemove = activeEffects.FindIndex(e => e.UniqueID == effect.UniqueID);
        if (indexToRemove == -1) {
            Debug.LogError("Trying to remove an effect that doesn't exist [Probably UID issue]");
            return;
        }
        var effectToRemove = activeEffects[indexToRemove];

        if (effectToRemove != null) {
            Debug.Log($"Removing effect {effectToRemove.EffectType} from {facility.facilityName}");

            effectToRemove.OnEffectRemoved?.Invoke(createdById);

            activeEffects.RemoveAt(indexToRemove);
            UnapplyEffect(effectToRemove, createdById);
        }
    }
    /// <summary>
    /// Forces a removal of an effect from the facility, meant to be called by Effect cards that remove effects from facility (assuming this bypasses fortification otherwise it seems kinda pointless)
    /// </summary>
    /// <param name="effect">The effect to remove from the facility</param>
    public void ForceRemoveEffect(FacilityEffect effect, int calledById) {
        RemoveEffect(effect, calledById, true);
    }
    public bool TryRemoveEffect(FacilityEffect effect, int calledById) {
        if (activeEffects.Contains(effect)) {
            ForceRemoveEffect(effect, calledById);
            return true;
        }
        return false;


    }
    public bool TryRemoveEffectByType(FacilityEffectType type, int calledById) {
        var result = activeEffects.Find(e => e.EffectType == type);
        if (result != null) {
            ForceRemoveEffect(result, calledById);
            return true;
        }
        Debug.LogWarning($"Did not find effect on {facility.facilityName} with type {type} to remove!");
        return false;
    }
    public void RemoveEffectByUID(int uid, int calledById) {
        var result = FindEffectByUID(uid);
        if (result != null && result.EffectType != FacilityEffectType.None) {
            ForceRemoveEffect(result, calledById); //remove the effect bypassing any possible negation
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
    private void AddEffect(FacilityEffect effect, int createdById) {

        Debug.Log($"Adding {effect.EffectType} to {facility.facilityName} from {createdById}");

        //special case of a remove type from a card effect
        if (effect.EffectType == FacilityEffectType.RemoveAll) {
            RemoveAllEffects(createdById);
            return;
        }
        else if (effect.EffectType == FacilityEffectType.RemoveOne) {
            Debug.Log($"Removing one effect from {facility.facilityName}");
            var team = effect.CreatedByTeam == PlayerTeam.Red ? PlayerTeam.Red : PlayerTeam.Blue;
            var removeable = GetEffectsRemovableByTeam(team, false);
            Debug.Log($"Found {removeable.Count} effects to remove on facility {facility.facilityName}");
            if (removeable.Count > 0) {
                RemoveEffect(removeable.First(), createdById);
                return;
            }
        }
        if (IsFortified() && effect.CreatedByTeam == PlayerTeam.Red && !hasNegatedEffectThisRound) {
            hasNegatedEffectThisRound = true;
            ScoreManager.Instance.AddSuccessfulDefense(
                activeEffects.Find(e => e.EffectType == FacilityEffectType.Fortify).CreatedByPlayerID
                );
            return;
        }
        //bypass fortify on second effect played on this facility
        if (IsFortified() && effect.CreatedByTeam == PlayerTeam.Red && hasNegatedEffectThisRound) {
            ScoreManager.Instance.AddFortifyOvercome(createdById);
        }

        //TODO: update this for all effects that have a duration
        if (effect.EffectType == FacilityEffectType.Backdoor && IsBackdoored()) {
            var activeBackdoor = activeEffects.Find(activeEffects => activeEffects.EffectType == FacilityEffectType.Backdoor);
            activeBackdoor.Duration = effect.Duration;
            activeBackdoor.UIElement.SetCounterText(activeBackdoor.Duration.ToString());
            return;
        }
        else if (effect.EffectType == FacilityEffectType.Fortify && IsFortified()) {
            var activeFortify = activeEffects.Find(activeEffects => activeEffects.EffectType == FacilityEffectType.Fortify);
            activeFortify.Duration = effect.Duration;
            activeFortify.UIElement.SetCounterText(activeFortify.Duration.ToString());
            return;
        }

        effect.CreatedByPlayerID = createdById;
        activeEffects.Add(effect);//add the effect to list
        //UpdateEffectUI(effect);
        ApplyEffect(effect, createdById);
    }
    /// <summary>
    /// Applies the effect to the facility based on the facility effect type
    /// </summary>
    /// <param name="effect">The facility effect object that holds the facility effect type</param>
    private void ApplyEffect(FacilityEffect effect, int createdById) {
        Debug.Log($"Applying effect {effect.EffectType} to {facility.facilityName}");
        switch (effect.EffectType) {
            case FacilityEffectType.ModifyPoints:

                ChangeFacilityPoints(effect, createdById);
                break;
            //case FacilityEffectType.Backdoor:
            //case FacilityEffectType.ModifyPointsPerTurn:
            //    // UpdateSpecialIcon(effect); 
            //    break;
            case FacilityEffectType.Fortify:
                //if (IsBackdoored()) {
                //    // ToggleEffectImageAlpha();
                //}
                RemoveNegativeEffects(createdById);

                break;
            case FacilityEffectType.ProtectPoints:
                bool protectPhys = effect.Target == FacilityEffectTarget.Physical ||
                                   effect.Target == FacilityEffectTarget.FinancialPhysical ||
                                   effect.Target == FacilityEffectTarget.NetworkPhysical ||
                                   effect.Target == FacilityEffectTarget.All;
                bool protectNet = effect.Target == FacilityEffectTarget.Network ||
                                   effect.Target == FacilityEffectTarget.FinancialNetwork ||
                                   effect.Target == FacilityEffectTarget.NetworkPhysical ||
                                   effect.Target == FacilityEffectTarget.All;
                bool protectFin = effect.Target == FacilityEffectTarget.Financial ||
                                   effect.Target == FacilityEffectTarget.FinancialNetwork ||
                                   effect.Target == FacilityEffectTarget.FinancialPhysical ||
                                   effect.Target == FacilityEffectTarget.All;

                facility.ActivatePointProtectionUI(protectPhys, protectNet, protectFin);
                break;
            default:
                break;
        }
        UpdateUI(effect, true, effect.IsObfuscated ? GameManager.Instance.GetPlayerTeam(createdById) : PlayerTeam.Any);

    }
    #endregion

    #region Interface Updates
    private void UpdateUI(FacilityEffect effect, bool add, PlayerTeam showForTeam = PlayerTeam.Any) {
        if (!effect.HasUIElement) return;
        Debug.Log($"showing ui for {effect.EffectType} with show for team: {showForTeam}");
        //only show the effect for the player if its the correct team
        if (showForTeam != PlayerTeam.Any) {
            if (GameManager.Instance.actualPlayer.playerTeam != showForTeam)
                return;
        }
        // Debug.Log($"Updating UI element for effect {effect.EffectType}");

        Action action;

        if (add) {
            //   Debug.Log($"Creating UI Element to Add");
            action = () => {
                //  Debug.Log("Adding effect ui element");
                var facilityEffectUI = Instantiate(effectPrefab, effectParent).GetComponent<FacilityEffectUIElement>();
                effect.UIElement = facilityEffectUI;
                facilityEffectUI.Init(effect);
            };
        }
        else {
            //   Debug.Log($"Queueing destroy action");
            action = () => {
                if (effect.HasUIElement && effect.UIElement != null)
                    Destroy(effect.UIElement.gameObject);
            };
        }
        QueuedUIUpdates.Enqueue(action);
        if (effectPopoutRoutine == null && effectMenuState == EffectMenuState.Open) {
            //  Debug.Log($"Hiding menu to change effects");
            //hide the effect box to prepare for the new effect
            effectPopoutRoutine = StartCoroutine(MoveUI(effectBoxParent,
                        effectHiddenPos - new Vector2(0, effectPopoutDistance),
                        effectHiddenPos));
            effectMenuState = EffectMenuState.Closing;
        }
        else if (effectPopoutRoutine == null && effectMenuState == EffectMenuState.Closed) {
            //  Debug.Log($"Showing effects menu");
            ProcessQueuedUIUpdates();
            if (add) {
                effectPopoutRoutine = StartCoroutine(MoveUI(effectBoxParent,
                        effectHiddenPos,
                        effectHiddenPos - new Vector2(0, effectPopoutDistance)));
                effectMenuState = EffectMenuState.Opening;
            }
            else {
                //this is ok now with some effects being hidden for some teams and not others
                //Debug.LogWarning($"Shouldn't get here, removing a UI component with menu hidden");
            }
        }
        else if (effectPopoutRoutine != null && effectMenuState == EffectMenuState.Opening) {
            // Debug.Log($"Canceling menu opening to change effects");
            //cancel menu opening and close it
            StopCoroutine(effectPopoutRoutine);
            effectPopoutRoutine = null;
            effectPopoutRoutine = StartCoroutine(MoveUI(effectBoxParent,
                        effectBoxParent.position,
                        effectHiddenPos));
        }
        else if (effectPopoutRoutine != null && effectMenuState == EffectMenuState.Closing) {
            //queued updates will be precessed when the menu closes
        }

    }
    private bool ProcessQueuedUIUpdates() {
        bool didSomething = false;
        Debug.Log($"Processing facility effect ui updates: {QueuedUIUpdates.Count} updates");
        while (QueuedUIUpdates.Count > 0) {
            QueuedUIUpdates.Dequeue()?.Invoke();
            didSomething = true;
        }
        return didSomething;

    }
    #region Effect Menu
    private IEnumerator MoveUI(RectTransform rectTransform, Vector2 startPos, Vector2 endPos) {
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
            effectMenuState = EffectMenuState.Closed;
            if (ProcessQueuedUIUpdates()) {
                if (activeEffects.Any(effect => effect.HasUIElement)) {
                    Debug.Log("Showing effects menu after processed changes");
                    effectPopoutRoutine = null;
                    effectPopoutRoutine = StartCoroutine(MoveUI(effectBoxParent,
                        effectHiddenPos,
                        effectHiddenPos - new Vector2(0, effectPopoutDistance)));
                    effectMenuState = EffectMenuState.Opening;
                }
                else {
                    effectPopoutRoutine = null;
                }
            }
            else {
                effectPopoutRoutine = null;
            }
        }
        else {
            effectMenuState = EffectMenuState.Open;
            effectPopoutRoutine = null;
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

        var currentActiveEffects = new List<FacilityEffect>(activeEffects);


        foreach (var effect in currentActiveEffects) {


            if (effect.EffectType == FacilityEffectType.ModifyPointsPerTurn) {
                var effects = FacilityEffect.CreateEffectsFromID(effect.EffectCreatedOnRoundEndIdString);
                effects.ForEach(_effect => AddEffect(_effect, effect.CreatedByPlayerID));
            }

            if (effect.Duration > 0) {
                // Debug.Log($"Reducing duration of {effect.EffectType} on facility {facility.facilityName}");
                effect.Duration--;
                if (effect.HasUIElement && effect.UIElement != null) {
                    effect.UIElement.SetCounterText(effect.Duration.ToString());
                }
                // DecrementCounter();

                if (effect.Duration == 0) {
                    ForceRemoveEffect(effect, effect.CreatedByPlayerID);
                }
            }
        }
        hasNegatedEffectThisRound = false;
    }
    #endregion



}
