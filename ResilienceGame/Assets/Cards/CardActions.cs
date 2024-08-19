using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// TODO: Rewrite card actions for Sector Down
/*
 *                                  "DrawAndDiscardCards":
                                case "ShuffleAndDrawCards":
                                case "ReduceCardCost":
                                case "ChangePoints":
                                case "AddEffect":
                                case "RemoveEffectByTeam":
                                case "NegateEffect":
                                case "ChangeFinancialPoints":
                                case "RemoveEffect":
                                case "SpreadEffect":
                                case "ChangeMeepleAmount":
                                case "IncreaseOvertimeAmount":
                                case "ShuffleCardsFromDiscard":*/
public class DrawAndDiscardCards : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " played.");
        // TODO: Get data from card reader to loop
        for(int i = 0; i < card.data.drawAmount; i++)
        {
            player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        }
        // TODO: Select Card(s) to Discard / reactivate discard box
        player.DiscardAllInactiveCards(DiscardFromWhere.Hand, false, -1);
    }
    public void Canceled(CardPlayer player, CardPlayer opponent, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ShuffleAndDrawCards : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " played to mitigate a card on the selected station.");
        // TODO: Get data from card reader to loop
        player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        // TODO: Select Shuffled Card

    }
    public void Canceled(CardPlayer player, CardPlayer opponent, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ActionImpactFacilityWorth : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " played to attack the selected station.");
        cardActedUpon.DefenseHealth += card.data.facilityAmount;
        card.state = CardState.CardNeedsToBeDiscarded;
        TextMeshProUGUI[] tempTexts = cardActedUpon.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < tempTexts.Length; i++)
        {
            if (tempTexts[i].name.Equals("Description Text"))
            {
                tempTexts[i].color = Color.red;
                tempTexts[i].text = "<size=600%>+" + cardActedUpon.DefenseHealth;
            }
        }
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " attack undone.");
        cardActedUpon.DefenseHealth -= card.data.facilityAmount;      
        TextMeshProUGUI[] tempTexts = cardActedUpon.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < tempTexts.Length; i++)
        {
            if (tempTexts[i].name.Equals("Description Text"))
            {
                tempTexts[i].color = Color.blue;
                tempTexts[i].text = "<size=600%>+" + cardActedUpon.DefenseHealth;
            }
        }
    }
}
