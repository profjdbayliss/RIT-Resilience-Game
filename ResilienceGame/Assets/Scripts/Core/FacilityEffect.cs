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
    ModifyPoints,
    ModifyPointsPerTurn,
    Negate,
    None
}
public enum FacilityTeam {
    Blue,
    Red,
    None
}

public class FacilityEffect {
    public FacilityEffectType EffectType { get; private set; }
    public FacilityEffectTarget Target { get; private set; }

    public FacilityTeam CreatedByTeam { get; set; } = FacilityTeam.None;
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

        (effectType, target, amount) = id switch {
            0 => (FacilityEffectType.None, FacilityEffectTarget.None, 0),
            1 => (FacilityEffectType.Backdoor, FacilityEffectTarget.None, 0),
            2 => (FacilityEffectType.Fortify, FacilityEffectTarget.None, 0),
            // ModifyPoints - Amount +1
            3 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, 1),
            4 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, 1),
            5 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, 1),
            // ModifyPoints - Amount +2
            6 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, 2),
            7 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, 2),
            8 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, 2),
            // ModifyPoints - Amount -1
            9 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, -1),
            10 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, -1),
            11 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, -1),
            // ModifyPoints - Amount -2
            12 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, -2),
            13 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, -2),
            14 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, -2),
            // ModifyPointsPerTurn - Amount -1
            15 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Physical, -1),
            16 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Network, -1),
            17 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Financial, -1),
            // ModifyPointsPerTurn - Amount -2
            18 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Physical, -2),
            19 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Network, -2),
            20 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Financial, -2),
            21 => (FacilityEffectType.Negate, FacilityEffectTarget.None, 0),
            _ => (FacilityEffectType.None, FacilityEffectTarget.None, 0)
        };
        if (effectType == FacilityEffectType.Backdoor || effectType == FacilityEffectType.Fortify) {
            duration = 3;
        }
        Debug.Log($"Creating effect: {effectType}, {target}, {amount}, {duration}");
        return new FacilityEffect(effectType, target, id, amount, duration);
    }

    public static string GetEffectInfoFromId(int id) {
        FacilityEffectType effectType = FacilityEffectType.None;
        FacilityEffectTarget target = FacilityEffectTarget.None;
        int magnitude = 0;

        /* parses an int ID from the csv into a proper facility effect
         * 
         * 3 to 8 are for ModifyPoints with targets Physical, Network, and Financial, and magnitudes +1 and +2.
         * 9 to 14 are for ModifyPoints with the same targets, and magnitudes -1 and -2.
         * 15 to 20 are for ModifyPointsPerTurn with the same targets and magnitudes -1 and -2.
         */
        (effectType, target, magnitude) = id switch {
            0 => (FacilityEffectType.None, FacilityEffectTarget.None, 0),
            1 => (FacilityEffectType.Backdoor, FacilityEffectTarget.None, 0),
            2 => (FacilityEffectType.Fortify, FacilityEffectTarget.None, 0),
            3 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, 1),
            4 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, 1),
            5 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, 1),
            6 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, 2),
            7 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, 2),
            8 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, 2),
            9 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, -1),
            10 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, -1),
            11 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, -1),
            12 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Physical, -2),
            13 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Network, -2),
            14 => (FacilityEffectType.ModifyPoints, FacilityEffectTarget.Financial, -2),
            15 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Physical, -1),
            16 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Network, -1),
            17 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Financial, -1),
            18 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Physical, -2),
            19 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Network, -2),
            20 => (FacilityEffectType.ModifyPointsPerTurn, FacilityEffectTarget.Financial, -2),
            21 => (FacilityEffectType.Negate, FacilityEffectTarget.None, 0),
            _ => (FacilityEffectType.None, FacilityEffectTarget.None, 0)
        };

        string effectInfo = $"Effect: {effectType}, Target: {target}";
        if (magnitude != 0) {
            effectInfo += $", Magnitude: {magnitude}";
        }
        return effectInfo;
    }


    #endregion
}
