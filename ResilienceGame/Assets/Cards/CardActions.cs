using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// TODO: Rewrite card actions for Sector Down
/*
 *                                  "DrawAndDiscardCards":
                                case "ShuffleAndDrawCards":
                                case "ReduceCardCost":
                                case "ChangeNetworkPoints":
                                case "ChangeFinancialkPoints":
                                case "ChangePhysicalPoints":
                                case "AddEffect":
                                case "RemoveEffectByTeam":
                                case "NegateEffect":
                                case "RemoveEffect":
                                case "SpreadEffect":
                                case "ChangeMeepleAmount":
                                case "IncreaseOvertimeAmount":
                                case "ShuffleCardsFromDiscard":*/
public class DrawAndDiscardCards : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
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
    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ShuffleAndDrawCards : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " played to mitigate a card on the selected station.");
        // TODO: Get data from card reader to loop
        player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        // TODO: Select Shuffled Card

    }
    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeNetworkPoints : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.ChangeFacilityPoints("network", card.data.facilityAmount);
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeFinancialPoints : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.ChangeFacilityPoints("financial", card.data.facilityAmount);
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangePhysicalPoints : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.ChangeFacilityPoints("physical", card.data.facilityAmount);
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class AddEffect : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        //need to find a way to implement amount of turns this effect is active for
        facilityActedUpon.AddOrRemoveEffect(card.data.effect, true);
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class RemoveEffectByTeam : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {

    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class NegateEffect : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {

    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class RemoveEffect : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.AddOrRemoveEffect(card.data.effect, false);
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class SpreadEffect : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        for(int i = 0; i < facilityActedUpon.sectorItsAPartOf.facilities.Length; i++)
        {
            //can probably slightly optimize this by finding out which of the facilities in the sector is facility acted upon
            //and excluding it from this but i don't think its worth the effort
            facilityActedUpon.sectorItsAPartOf.facilities[i].effect = facilityActedUpon.effect; 
        }
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeMeepleAmount : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.sectorItsAPartOf.blackMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.blueMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.purpleMeeples += card.data.meepleAmount;
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class IncreaseOvertimeAmount : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {

    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ShuffleCardsFromDiscard : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {

    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ReduceCardCost : ICardAction
{
    public void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        for(int i = 0; i < cardActedUpon.data.meepleType.Length; i++)
        {
            switch(cardActedUpon.data.meepleType[i])
            {
                case "Blue":
                    cardActedUpon.data.blueCost--;
                    break;

                case "Black":
                    cardActedUpon.data.blackCost--;
                    break;

                case "Purple":
                    cardActedUpon.data.purpleCost--;
                    break;

                default:
                    Debug.Log("Meeple type not blue, black or purple for some reason");
                    break;
            }
        }
    }

    public void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}


