using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICardAction
{
    public virtual void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) { LogAction(player, opponent, facilityActedUpon, cardActedUpon, card); }
    public virtual void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) { LogAction(player, opponent, facilityActedUpon, cardActedUpon, card); }

    private void LogAction(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        

        var targetInfo = card.target switch  {
            CardTarget.Effect or
            CardTarget.Facility => $"on {facilityActedUpon.facilityName}",
            CardTarget.Card => $"on {cardActedUpon.data.name}",
            CardTarget.Hand => "",
            _ => $"in {player.playerSector.sectorName}"
        };


        Debug.Log($"Executing action {GetType()} from {player.playerName} {targetInfo}");
    }
}
