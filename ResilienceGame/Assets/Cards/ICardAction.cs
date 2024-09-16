using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICardAction
{
    public virtual void Played(CardPlayer player, CardPlayer opponent, Facility faciltyActedUpon, Card cardActedUpon, Card card) { LogAction(); }
    public virtual void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) { LogAction(); }

    private void LogAction() {
        Debug.Log($"Executing action {GetType()}");
    }
}
