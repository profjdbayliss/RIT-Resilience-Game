using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

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
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " played.");
        // TODO: Get data from card reader to loop
        for(int i = 0; i < card.data.drawAmount; i++)
        {
            player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        }
        // TODO: Select Card(s) to Discard / reactivate discard box
        player.DiscardAllInactiveCards(DiscardFromWhere.Hand, false, -1);

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ShuffleAndDrawCards : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " played to mitigate a card on the selected station.");
        // TODO: Get data from card reader to loop
        player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        // TODO: Select Shuffled Card
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeNetworkPoints : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.ChangeFacilityPoints("network", card.data.facilityAmount);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeFinancialPoints : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.ChangeFacilityPoints("financial", card.data.facilityAmount);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangePhysicalPoints : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.ChangeFacilityPoints("physical", card.data.facilityAmount);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class AddEffect : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        //need to find a way to implement amount of turns this effect is active for
        if(!facilityActedUpon.effectNegated)
            facilityActedUpon.AddOrRemoveEffect(card.data.effect, true);
        else
        {
            facilityActedUpon.AddOrRemoveEffect(card.data.effect, false);
            facilityActedUpon.effectNegated = false;
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class NegateEffect : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.effectNegated = true;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class RemoveEffect : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.AddOrRemoveEffect(card.data.effect, false);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class SpreadEffect : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        for(int i = 0; i < facilityActedUpon.sectorItsAPartOf.facilities.Length; i++)
        {
            //can probably slightly optimize this by finding out which of the facilities in the sector is facility acted upon
            //and excluding it from this but i don't think its worth the effort
            facilityActedUpon.sectorItsAPartOf.facilities[i].effect = facilityActedUpon.effect; 
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeMeepleAmount : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.sectorItsAPartOf.blackMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.blueMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.purpleMeeples += card.data.meepleAmount;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class IncreaseOvertimeAmount : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        facilityActedUpon.sectorItsAPartOf.overTimeCharges++;
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

/// <summary>
/// Takes 5 random cards from Discard and adds them back into the hand.
/// NOTE: At the moment, the number 5 is hard coded cause there's only one card that
/// has this mechanic but a new variable would have to be created if this were to be 
/// expanded upon.
/// </summary>
public class ShuffleCardsFromDiscard : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        GameObject[] cardsShuffledFromDiscard = new GameObject[5];
        if(player.Discards.Count == 0)
        {
            Debug.LogWarning("The discard pile is empty");
            return;
        }

        for(int i = 0; i < cardsShuffledFromDiscard.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, player.Discards.Count);
            cardsShuffledFromDiscard[i] = player.Discards.ElementAt(randomIndex).Value;
            int key = player.Discards.ElementAt(randomIndex).Key;
            player.Discards.Remove(key);
            while(player.HandCards.ContainsKey(key))
            {
                key++;
            }
            player.HandCards.Add(key, cardsShuffledFromDiscard[i]);
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public  class ReduceCardCost : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        for (int i = 0; i < cardActedUpon.data.meepleType.Length; i++)
        {
            switch (cardActedUpon.data.meepleType[i])
            {
                case "Blue":
                    cardActedUpon.data.blueCost -= card.data.meepleAmount; ;
                    break;

                case "Black":
                    cardActedUpon.data.blackCost -= card.data.meepleAmount; ;
                    break;

                case "Purple":
                    cardActedUpon.data.purpleCost -= card.data.meepleAmount; ;
                    break;

                default:
                    Debug.Log("Meeple type not blue, black or purple for some reason");
                    break;
            }
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}




