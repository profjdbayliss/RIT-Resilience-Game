using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICardAction
{
    public virtual void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) { /*LogAction(player, opponent, facilityActedUpon, cardActedUpon, card);*/ }
    public virtual void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) { /*LogAction(player, opponent, facilityActedUpon, cardActedUpon, card);*/ }

    protected void RemoveEffect(Facility facility, FacilityEffect effectToRemove, CardPlayer player) {
        if (facility.TryRemoveEffect(effectToRemove, player.NetID)) {
            Debug.Log($"Removed effect: {effectToRemove.EffectType} on {facility.facilityName}");

        }
        else {
            Debug.LogError($"Found effect to remove but then got a false value when trying to remove it");
        }
    }

}
