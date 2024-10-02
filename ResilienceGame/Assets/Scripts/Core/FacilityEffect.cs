using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

[Flags]
public enum FacilityPointTarget {
    None = 0,
    Physical = 1 << 0,
    Financial = 1 << 1,
    Network = 1 << 2,
    All = Physical | Financial | Network
}

public enum FacilityEffectType {
    Backdoor,
    Fortify,
    ModifyPoints,
    ModifyPointsPerTurn,
    Negate,
    None
}


public class FacilityEffect {
    private const int BACKDOOR_FORT_DURATION = 3;
    public FacilityEffectType EffectType { get; private set; }
    public FacilityPointTarget Target { get; private set; }


    public PlayerTeam CreatedByTeam { get; set; } = PlayerTeam.None;
    public int Magnitude { get; private set; } // Integer magnitude instead of enum
    public int Duration { get; set; }  // -1 for infinite
                                       // public int Stack { get; private set; } = 1;
    public bool IsNegated { get; set; } = false;

    private static int _uniqueID = 0;
    public int UniqueID { get; private set; }

    public string CreatedEffectID { get; private set; }

    public List<FacilityEffect> CreatedEffects { get; private set; }
    public Sprite EffectIcon;

    public FacilityEffect(FacilityEffectType effectType, FacilityPointTarget target, string createdEffectID, int magnitude, int duration = -1, int uniqueID = -1, Sprite effectIcon = null) {
        EffectType = effectType;
        Target = target;
        Magnitude = magnitude;
        Duration = duration;
        CreatedEffects = new List<FacilityEffect>();
        if (uniqueID == -1) {
            UniqueID = _uniqueID++;
        }
        else {
            if (uniqueID < _uniqueID) {
                UniqueID = _uniqueID++;
                Debug.LogError($"Unique Facility Effect id is less than uniqueID which will cause duplicate unique ID values");
            }
            else {
                _uniqueID = uniqueID;
                UniqueID = uniqueID;
            }
        }
        CreatedEffectID = createdEffectID;
        EffectIcon = effectIcon;
    }


    public override string ToString() {
        string effectInfo = $"Effect: {EffectType}, Target: {Target}, Magnitude: {Magnitude}, Duration: {(Duration == -1 ? "Infinite" : Duration.ToString())}, " +
                            $"Negated: {IsNegated}";

        if (CreatedEffects.Count > 0) {
            effectInfo += "\n  Created Effects: ";
            foreach (var createdEffect in CreatedEffects) {
                effectInfo += $"\n    {createdEffect}";
            }
        }

        return effectInfo;
    }

    #region Facility Effect Creation

    public static List<FacilityEffect> CreateEffectsFromID(string effectString) {
        List<FacilityEffect> effects = new List<FacilityEffect>();
        string[] effectParts = effectString.Split(';');
        string effectTypeString = effectParts[0];
        if (effectParts.Length < 2) {
            FacilityEffectType effectType = ParseEffectType(effectParts[0]);
            effects.Add(new FacilityEffect(effectType, FacilityPointTarget.None, "", 0, 3));
            return effects;

        }
        string targetInfoString = effectParts[1];
        int magnitude = int.Parse(effectParts[2]);

        var effectTypes = effectTypeString.Split('&');
        foreach (var effect in effectTypes) {
            FacilityEffectType effectType = ParseEffectType(effect);
            if (effectType == FacilityEffectType.Backdoor || effectType == FacilityEffectType.Fortify) {
                effects.Add(new FacilityEffect(effectType, FacilityPointTarget.None, "", magnitude, BACKDOOR_FORT_DURATION));
            }
            else {
                string effectCreatedByEffect = effectType == FacilityEffectType.ModifyPointsPerTurn
                    ? $"modp;{targetInfoString};{magnitude}"
                    : "";

                FacilityPointTarget target = ParseTarget(targetInfoString);
                effects.Add(new FacilityEffect(effectType, target, effectCreatedByEffect, magnitude));
            }
        }
        return effects;
    }

    public static FacilityEffectType ParseEffectType(string typeString) {
        return typeString.ToLower() switch {
            "modp" => FacilityEffectType.ModifyPoints,
            "modppt" => FacilityEffectType.ModifyPointsPerTurn,
            "fortify" => FacilityEffectType.Fortify,
            "backdoor" => FacilityEffectType.Backdoor,
            "remove" => FacilityEffectType.Negate,
            _ => FacilityEffectType.None
        };
    }

    public static FacilityPointTarget ParseTarget(string targetString) {
        FacilityPointTarget target = FacilityPointTarget.None;
        foreach (string t in targetString.Split('&')) {
            target |= t.ToLower() switch {
                "phys" => FacilityPointTarget.Physical,
                "net" => FacilityPointTarget.Network,
                "fin" => FacilityPointTarget.Financial,
                "all" => FacilityPointTarget.All,
                _ => FacilityPointTarget.None
            };
        }
        return target;
    }

    #endregion
}
