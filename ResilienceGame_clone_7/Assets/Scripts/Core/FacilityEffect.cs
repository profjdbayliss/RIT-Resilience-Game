using System;
using System.Collections.Generic;
using UnityEngine;

public enum FacilityEffectTarget {
    Fortify,
    Network,
    Physical,
    NetworkPhysical,
    All,
    Financial,
    Backdoor,
    FinancialNetwork,
    FinancialPhysical,
    None
}

public enum FacilityEffectType {
    Backdoor,
    Fortify,
    HoneyPot,
    ModifyPoints,
    ModifyPointsPerTurn,
    ProtectPoints,
    RemoveOne,
    RemoveAll,
    None
}


public class FacilityEffect {
    private const int BACKDOOR_FORT_DURATION = 3;
    public FacilityEffectType EffectType { get; private set; }
    public FacilityEffectTarget Target { get; private set; }
    public bool IsRestoreEffect => EffectType == FacilityEffectType.ModifyPoints && Magnitude > 0;

    public PlayerTeam CreatedByTeam { get; set; } = PlayerTeam.None;
    public int Magnitude { get; private set; } // Integer magnitude instead of enum
    public int Duration { get; set; }  // -1 for infinite
                                       // public int Stack { get; private set; } = 1;
    public bool IsNegated { get; set; } = false;

    public int CreatedByPlayerID { get; set; } = -1;
    public bool HasTrap { get; set; } = false;
    public bool IsRemoveable => 
        EffectType == FacilityEffectType.Backdoor || 
        EffectType == FacilityEffectType.ModifyPointsPerTurn || 
        EffectType == FacilityEffectType.Fortify ||
        EffectType == FacilityEffectType.HoneyPot ||
        EffectType == FacilityEffectType.ProtectPoints;

    public Action<int> OnEffectRemoved;
    //private static int _uniqueID = 0;
    public int UniqueID { get; private set; }
    //add more hidden effects here
    public bool IsObfuscated => EffectType == FacilityEffectType.HoneyPot;

    public string EffectCreatedOnRoundEndIdString { get; private set; } //holds a string used to create the effect that this effect will create
    public string EffectIdString { get; private set; } //holds a string used to create this effect
    public List<FacilityEffect> CreatedEffects { get; private set; }

    public bool HasUIElement { get; private set; } = false;
    public FacilityEffectUIElement UIElement { get; set; }

    public FacilityEffect(FacilityEffectType effectType, FacilityEffectTarget target, string createdEffectID, int magnitude, int duration = -1) {
        EffectType = effectType;

        //TODO: add more ui elements here
        HasUIElement = effectType == FacilityEffectType.ModifyPointsPerTurn ||
                        effectType == FacilityEffectType.Backdoor ||
                        effectType == FacilityEffectType.Fortify ||
                        effectType == FacilityEffectType.ProtectPoints ||
                        effectType == FacilityEffectType.HoneyPot;


        Target = target;
        Magnitude = magnitude;
        Duration = duration;
        CreatedEffects = new List<FacilityEffect>();
        if (GameManager.Instance != null) {
            UniqueID = GameManager.Instance.UniqueFacilityEffectIdCount++;
        }
        else {
            UniqueID = -1;
        }
        EffectCreatedOnRoundEndIdString = createdEffectID;
    }
    public static string CombineEffectStrings(string effect1, string effect2) {
        var effect1Parts = effect1.Split(';');
        var effect2Parts = effect2.Split(';');

        if (effect1Parts.Length > 1) {
            return $"{effect1Parts[0]}&{effect2Parts[0]};{effect1Parts[1]};{effect1Parts[2]}";
        }
        else if (effect2Parts.Length > 1) {
            return $"{effect2Parts[0]}&{effect1Parts[0]};{effect2Parts[1]};{effect2Parts[2]}";
        }
        else {
            return $"{effect1}&{effect2}";
        }
    }
    public override string ToString() {
        string effectInfo = $"UID: {UniqueID}, Effect: {EffectType}, Target: {Target}, Magnitude: {Magnitude}, Duration: {(Duration == -1 ? "Infinite" : Duration.ToString())}, " +
                            $"Negated: {IsNegated}, Removable: {IsRemoveable}";

        if (CreatedEffects.Count > 0) {
            effectInfo += "\n  Created Effects: ";
            foreach (var createdEffect in CreatedEffects) {
                effectInfo += $"\n    {createdEffect}";
            }
        }

        return effectInfo;
    }


    #region Facility Effect Creation

    public static string CreateEffectIdString(FacilityEffectType effectType, FacilityEffectTarget target, int magnitude) {
        if (effectType == FacilityEffectType.ModifyPointsPerTurn || effectType == FacilityEffectType.ModifyPoints) {
            return $"{GetEffectTypeString(effectType)};{GetTargetString(target)};{magnitude}";
        }
        return GetEffectTypeString(effectType);

    }

    //creates a list of facility effects from a string in the csv file
    //format currently supports multiple effects, but only 1 effect with a target
    //this works with current card design, but may need to be updated if we add more effect types
    //ie. cards can 'fortify' and '+1 physical' but you can't 'fortify' and 'backdoor' in the same card
    //csv format is "effectType&effectType2;target1&target2;magnitude" where effectType2 and target2 are optional
    public static List<FacilityEffect> CreateEffectsFromID(string effectString) {
        List<FacilityEffect> effects = new List<FacilityEffect>();
        string[] effectParts = effectString.Split(';');
        string effectTypeString = effectParts[0];
        //if there is only 1 piece of info, its backdoor or fortify so we can just add that effect
        if (effectParts.Length < 2) {
            FacilityEffectType effectType = ParseEffectType(effectParts[0]);
            effects.Add(new FacilityEffect(effectType, FacilityEffectTarget.None, "", 0, 3));
            //set the team created field
            if (effectType == FacilityEffectType.Backdoor ||
                effectType == FacilityEffectType.ModifyPointsPerTurn) {
                effects[^1].CreatedByTeam = PlayerTeam.Red;
            }
            else if (effectType == FacilityEffectType.Fortify ||
                     effectType == FacilityEffectType.ProtectPoints ||
                     effectType == FacilityEffectType.HoneyPot)
                effects[^1].CreatedByTeam = PlayerTeam.Blue;
            effects[^1].EffectIdString = effectString;
            HandleOnEffectRemovalCreation(effectType, effectString, ref effects);
            return effects;

        }
        string targetInfoString = effectParts[1];
        int magnitude = 0;
        try {
            magnitude = int.Parse(effectParts[2]);
        }
        catch { }

        var effectTypes = effectTypeString.Split('&');
        //create an effect for each effect type
        foreach (var effect in effectTypes) {
            FacilityEffectType effectType = ParseEffectType(effect);
            //if effect is backdoor or fortify, dont worry about target
            if (effectType == FacilityEffectType.Backdoor || effectType == FacilityEffectType.Fortify) {
                effects.Add(new FacilityEffect(effectType, FacilityEffectTarget.None, "", magnitude, BACKDOOR_FORT_DURATION));
                effects[^1].EffectIdString = effect.ToString().ToLower(); //only backdoor or fortify
            }
            else {
                //create a string to represent the effect that this effect will create (if its a Modify Points Per Turn type)
                string effectCreatedByEffect = effectType == FacilityEffectType.ModifyPointsPerTurn
                    ? $"modp;{targetInfoString};{magnitude}"
                    : "";

                FacilityEffectTarget target = ParseTarget(targetInfoString);
                effects.Add(new FacilityEffect(effectType, target, effectCreatedByEffect, magnitude));
                effects[^1].EffectIdString = $"{effect};{targetInfoString};{magnitude}";
            }
            if (effectType != FacilityEffectType.None) {
                //initially set a created by team
                if (magnitude < 0) {
                    effects[^1].CreatedByTeam = PlayerTeam.Red;
                }
                else {
                    effects[^1].CreatedByTeam = PlayerTeam.Blue;
                }

            }

        }
        return effects;
    }
    //add more on effect removal actions here
    //actions get passed the id of the player that caused them to occur
    private static void HandleOnEffectRemovalCreation(FacilityEffectType effectType, string effectString, ref List<FacilityEffect> effects) {

        switch (effectType) {
            case FacilityEffectType.HoneyPot:
                effects[^1].HasTrap = true;
                effects[^1].OnEffectRemoved += (id) => {
                    if (id != -1) {
                        //TODO: update if we add AI players
                        if (GameManager.Instance.actualPlayer.NetID == id) {
                            GameManager.Instance.ForcePlayerDiscardOneCard(id);
                        }
                    }
                    else if (id == -1) {
                        Debug.LogError("Honeypot effect was called with -1 id");
                    }

                };
                break;
            case FacilityEffectType.Backdoor:
                int effectCreatedBy = effects[^1].CreatedByPlayerID;
                effects[^1].OnEffectRemoved += (id) => {
                    if (id != effectCreatedBy) {
                        ScoreManager.Instance.AddBackdoorRemoval(id);
                    }
                    else {
                        ScoreManager.Instance.AddEffectExpireScore(effectCreatedBy);
                    }

                };
                break;
        }
    }
    public string ToIdString() {
        if (EffectType == FacilityEffectType.Backdoor || EffectType == FacilityEffectType.Fortify) {
            return EffectType.ToString().ToLower();
        }
        return $"{EffectType.ToString().ToLower()}&{Target.ToString().ToLower()}&{Magnitude}";
    }
    public static FacilityEffectType ParseEffectType(string typeString) {
        return typeString.ToLower() switch {
            "modp" => FacilityEffectType.ModifyPoints,
            "modppt" => FacilityEffectType.ModifyPointsPerTurn,
            "protp" => FacilityEffectType.ProtectPoints,
            "fortify" => FacilityEffectType.Fortify,
            "backdoor" => FacilityEffectType.Backdoor,
            "honeypot" => FacilityEffectType.HoneyPot,
            "removeone" => FacilityEffectType.RemoveOne,
            "removeall" => FacilityEffectType.RemoveAll,
            _ => FacilityEffectType.None
        };
    }
    public static string GetEffectTypeString(FacilityEffectType type) {
        return type switch {
            FacilityEffectType.ModifyPoints => "modp",
            FacilityEffectType.ModifyPointsPerTurn => "modppt",
            FacilityEffectType.ProtectPoints => "protp",
            FacilityEffectType.Fortify => "fortify",
            FacilityEffectType.Backdoor => "backdoor",
            FacilityEffectType.HoneyPot => "honeypot",
            FacilityEffectType.RemoveOne => "removeone",
            FacilityEffectType.RemoveAll => "removeall",
            _ => ""
        };
    }
    public static string GetTargetString(FacilityEffectTarget target) {

        return target switch {
            FacilityEffectTarget.Physical => "phys",
            FacilityEffectTarget.Financial => "fin",
            FacilityEffectTarget.Network => "net",
            FacilityEffectTarget.All => "all",
            FacilityEffectTarget.FinancialPhysical => "fin&phys",
            FacilityEffectTarget.FinancialNetwork => "fin&net",
            FacilityEffectTarget.NetworkPhysical => "net&phys",
            _ => "",
        };
    }

    public static FacilityEffectTarget ParseTarget(string targetString) {
        if (targetString.Contains("all")) {
            return FacilityEffectTarget.All;
        }
        bool isPhysical = targetString.Contains("phys");
        bool isFinancial = targetString.Contains("fin");
        bool isNetwork = targetString.Contains("net");

        if (isPhysical && isFinancial && isNetwork)
            return FacilityEffectTarget.All;
        else if (isPhysical && isFinancial)
            return FacilityEffectTarget.FinancialPhysical;
        else if (isPhysical && isNetwork)
            return FacilityEffectTarget.NetworkPhysical;
        else if (isFinancial && isNetwork)
            return FacilityEffectTarget.FinancialNetwork;
        else if (isPhysical)
            return FacilityEffectTarget.Physical;
        else if (isFinancial)
            return FacilityEffectTarget.Financial;
        else if (isNetwork)
            return FacilityEffectTarget.Network;

        return FacilityEffectTarget.None;
    }

    #endregion
}
