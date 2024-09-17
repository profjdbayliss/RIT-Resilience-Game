using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

public enum FacilityEffectTarget {
    Physical,
    Network,
    Financial,
    None
}

public enum FacilityEffectType {
    Backdoor,
    Fortify,
    RestorePoints,
    ReducePoints,
    ReducePointsPerTurn,
    Negate,
    None
}
public enum FacilityTeam {
    Blue,
    Red
}

public class FacilityEffect {
    public FacilityEffectType EffectType { get; private set; }
    public FacilityEffectTarget Target { get; private set; }
    public int Magnitude { get; private set; } // Integer magnitude instead of enum
    public int Duration { get; set; }  // -1 for infinite
    public int Stack { get; private set; } = 1;
    public bool IsNegated { get; set; } = false;

    public static int UniqueID { get; private set; }

    public int CreatedEffectID { get; private set; }

    public List<FacilityEffect> CreatedEffects { get; private set; }

    public FacilityEffect(FacilityEffectType effectType, FacilityEffectTarget target, int createdEffectID, int magnitude, int duration = -1, int uniqueID = 0) {
        EffectType = effectType;
        Target = target;
        Magnitude = magnitude;
        Duration = duration;
        CreatedEffects = new List<FacilityEffect>();
        UniqueID = ++uniqueID;
        CreatedEffectID = createdEffectID;
    }

    public bool DecreaseDuration() {
        if (Duration > 0) {
            Duration--;
            if (Duration == 0) return true; // Effect expires
        }
        return false; // Effect still active
    }

    public void AddStack() {
        Stack++;
    }

    public void RemoveStack() {
        if (Stack > 1) {
            Stack--;
        }
    }

    public override string ToString() {
        string effectInfo = $"Effect: {EffectType}, Target: {Target}, Magnitude: {Magnitude}, Duration: {(Duration == -1 ? "Infinite" : Duration.ToString())}, " +
                            $"Stack: {Stack}, Negated: {IsNegated}";

        if (CreatedEffects.Count > 0) {
            effectInfo += "\n  Created Effects: ";
            foreach (var createdEffect in CreatedEffects) {
                effectInfo += $"\n    {createdEffect}";
            }
        }

        return effectInfo;
    }

    #region Facility Effect Creation

    public static FacilityEffect CreateEffectFromID(int id) {
        FacilityEffectType effectType = FacilityEffectType.None;
        FacilityEffectTarget target = FacilityEffectTarget.None;
        int duration = -1;
        int amount = 0;

        /* parses an int ID from the csv into a proper facility effect
         * 
         * 3 to 8 are for RestorePoints and ReducePoints with amounts 1 and 2.
         * 9 to 14 are for ReducePointsPerTurn with the same targets and amounts.
         */
        (effectType, target, amount) = id switch {
            0 => (FacilityEffectType.None, FacilityEffectTarget.None, 0),
            1 => (FacilityEffectType.Backdoor, FacilityEffectTarget.None, 0),
            2 => (FacilityEffectType.Fortify, FacilityEffectTarget.None, 0),
            // RestorePoints - Amount 1
            3 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Physical, 1),
            4 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Network, 1),
            5 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Financial, 1),
            // RestorePoints - Amount 2
            6 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Physical, 2),
            7 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Network, 2),
            8 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Financial, 2),
            // ReducePoints - Amount 1
            9 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Physical, 1),
            10 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Network, 1),
            11 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Financial, 1),
            // ReducePoints - Amount 2
            12 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Physical, 2),
            13 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Network, 2),
            14 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Financial, 2),
            // ReducePointsPerTurn - Amount 1
            15 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Physical, 1),
            16 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Network, 1),
            17 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Financial, 1),
            // ReducePointsPerTurn - Amount 2
            18 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Physical, 2),
            19 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Network, 2),
            20 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Financial, 2),
            21 => (FacilityEffectType.Negate, FacilityEffectTarget.None, 0),
            _ => (FacilityEffectType.None, FacilityEffectTarget.None, 0)
        };
        if (effectType == FacilityEffectType.Backdoor || effectType == FacilityEffectType.Fortify) {
            duration = 3;
        }
        return new FacilityEffect(effectType, target, amount, duration);
    }

    public static string GetEffectInfoFromId(int id) {
        FacilityEffectType effectType = FacilityEffectType.None;
        FacilityEffectTarget target = FacilityEffectTarget.None;

        /* parses an int ID from the csv into a proper facility effect
         * 
         * 3 to 5 are for RestorePoints with targets Physical, Network, and Financial.
            6 to 8 are for ReducePoints with the same targets.
            9 to 11 are for ReducePointsPerTurn with the same targets.
         */
        (effectType, target) = id switch {
            0 => (FacilityEffectType.None, FacilityEffectTarget.None),
            1 => (FacilityEffectType.Backdoor, FacilityEffectTarget.None),
            2 => (FacilityEffectType.Fortify, FacilityEffectTarget.None),
            3 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Physical),
            4 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Network),
            5 => (FacilityEffectType.RestorePoints, FacilityEffectTarget.Financial),
            6 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Physical),
            7 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Network),
            8 => (FacilityEffectType.ReducePoints, FacilityEffectTarget.Financial),
            9 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Physical),
            10 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Network),
            11 => (FacilityEffectType.ReducePointsPerTurn, FacilityEffectTarget.Financial),
            _ => (FacilityEffectType.None, FacilityEffectTarget.None)
        };

        return $"Effect: {effectType}, Target: {target}";
    }


    #endregion
}
