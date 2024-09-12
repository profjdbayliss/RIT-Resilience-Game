using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
/// <summary>
/// This class serves to handle all card actions
/// New actions should be added here
/// </summary>
public class CardActionManager : MonoBehaviour
{

    public static CardActionManager Instance { get; private set; }
    private Dictionary<string, Action<CardPlayer, CardPlayer, Facility, Card, Card>> cardActions;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeCardActions();
        }
        else {
            Destroy(gameObject);
        }
    }
    //map card action names to their respective functions
    private void InitializeCardActions() {
        cardActions = new Dictionary<string, Action<CardPlayer, CardPlayer, Facility, Card, Card>>
        {
            { "DrawAndDiscardCards", DrawAndDiscardCards },
            { "ShuffleAndDrawCards", ShuffleAndDrawCards },
            { "ReduceCardCost", ReduceCardCost },
            { "ChangeNetworkPoints", ChangeNetworkPoints },
            { "ChangeFinancialPoints", ChangeFinancialPoints },
            { "ChangePhysicalPoints", ChangePhysicalPoints },
            { "AddEffect", AddEffect },
            { "RemoveEffectByTeam", RemoveEffectByTeam },
            { "NegateEffect", NegateEffect },
            { "RemoveEffect", RemoveEffect },
            { "SpreadEffect", SpreadEffect },
            { "ChangeMeepleAmount", ChangeMeepleAmount },
            { "IncreaseOvertimeAmount", IncreaseOvertimeAmount },
            { "ShuffleCardsFromDiscard", ShuffleCardsFromDiscard }
        };
    }

    /// <summary>
    /// Executes the card action
    /// </summary>
    /// <param name="actionName">String name of the action</param>
    /// <param name="player">actor who played the action</param>
    /// <param name="opponent">opponent player</param>
    /// <param name="facilityActedUpon">the facility the card was played on or null if non facility card</param>
    /// <param name="cardActedUpon">The card the card was played on or null</param>
    /// <param name="card">The card that was played</param>
    public void ExecuteCardAction(string actionName, CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        if (cardActions.TryGetValue(actionName, out var action)) {
            action.Invoke(player, opponent, facilityActedUpon, cardActedUpon, card);
        }
        else {
            Debug.LogWarning($"Card action '{actionName}' not found.");
        }
    }


    #region Card Actions
    private void DrawAndDiscardCards(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.data.front.title + " played.");
        for (int i = 0; i < card.data.drawAmount; i++) {
            player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        }
        player.DiscardAllInactiveCards(DiscardFromWhere.Hand, false, -1);
    }

    private void ShuffleAndDrawCards(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.data.front.title + " played to mitigate a card on the selected station.");
        player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        // TODO: Implement shuffle logic
    }

    private void ChangeNetworkPoints(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.ChangeFacilityPoints("network", card.data.facilityAmount);
    }

    private void ChangeFinancialPoints(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.ChangeFacilityPoints("financial", card.data.facilityAmount);
    }

    private void ChangePhysicalPoints(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.ChangeFacilityPoints("physical", card.data.facilityAmount);
    }

    private void AddEffect(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.AddOrRemoveEffect(card.data.effect, true);
    }

    private void RemoveEffectByTeam(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        // TODO: Implement RemoveEffectByTeam logic
    }

    private void NegateEffect(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        // TODO: Implement NegateEffect logic
    }

    private void RemoveEffect(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.AddOrRemoveEffect(card.data.effect, false);
    }

    private void SpreadEffect(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        for (int i = 0; i < facilityActedUpon.sectorItsAPartOf.facilities.Length; i++) {
            facilityActedUpon.sectorItsAPartOf.facilities[i].effect = facilityActedUpon.effect;
        }
    }

    private void ChangeMeepleAmount(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.sectorItsAPartOf.blackMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.blueMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.purpleMeeples += card.data.meepleAmount;
    }

    private void IncreaseOvertimeAmount(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        // TODO: Implement IncreaseOvertimeAmount logic
    }

    private void ShuffleCardsFromDiscard(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        // TODO: Implement ShuffleCardsFromDiscard logic
    }

    private void ReduceCardCost(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        //this assumes that it will always reduce the cost of all meeple types
        cardActedUpon.data.meepleActionAmount.Values.ToList().ForEach(meepleCost => meepleCost -= card.data.meepleAmount);
    }

    #endregion

}
