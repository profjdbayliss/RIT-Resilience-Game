using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using System;

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
public class DrawAndDiscardCards : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " played.");
        // TODO: Get data from card reader to loop
        for (int i = 0; i < card.data.drawAmount; i++) {
            player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        }
        // TODO: Select Card(s) to Discard / reactivate discard box
        player.DiscardAllInactiveCards(DiscardFromWhere.Hand, false, -1);

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ShuffleAndDrawCards : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " played to mitigate a card on the selected station.");
        // TODO: Get data from card reader to loop
        player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
        // TODO: Select Shuffled Card
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeNetworkPoints : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        // facilityActedUpon.ChangeFacilityPoints("network", card.data.facilityAmount);
        string target = "Network";
        string amount = card.data.facilityAmount switch {
            0 => "Zero",
            1 => "One",
            2 => "Two",
            _ => ""
        };

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeFinancialPoints : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.ChangeFacilityPoints("financial", card.data.facilityAmount);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangePhysicalPoints : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.ChangeFacilityPoints("physical", card.data.facilityAmount);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class AddEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        FacilityTeam playedTeam = card.DeckName.ToLower().Trim() == "blue" ? FacilityTeam.Blue : FacilityTeam.Red;
        facilityActedUpon.AddRemoveAllEffectsByIdString(card.data.effectIds, true, playedTeam);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

//TODO: These are the same, should be combined, difference is handled in the FacilityEffectManager
public class NegateEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
       
        var effectType = card.DeckName.ToLower().Trim() == "blue" ? FacilityTeam.Blue : FacilityTeam.Red;
        
        facilityActedUpon.AddRemoveAllEffectsByIdString(card.data.effectIds, true, effectType);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class RemoveEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        var effectList = facilityActedUpon.effectManager.GetEffects();
        var effectType = card.DeckName.ToLower().Trim() == "blue" ? FacilityTeam.Blue : FacilityTeam.Red;
        //creating this to remove random effect from the facility - almost definitely not the best way to do this, player should get to chose
        if (effectList.Count > 0) {
            var randomEffect = effectList[UnityEngine.Random.Range(0, effectList.Count)];
            facilityActedUpon.AddRemoveEffectByID(randomEffect.CreatedEffectID, false, effectType);
        }

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class SpreadEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        for (int i = 0; i < facilityActedUpon.sectorItsAPartOf.facilities.Length; i++) {
            //can probably slightly optimize this by finding out which of the facilities in the sector is facility acted upon
            //and excluding it from this but i don't think its worth the effort
            // facilityActedUpon.sectorItsAPartOf.facilities[i].effect = facilityActedUpon.effect; 
            //TODO:

            /* Description:
             *  If a facility has a Backdoor; add Backdoors to all facilities in the connected Sectors. These new Backdoors last for 3 turns. You can only play 1 Lateral movement per turn. 
             * 
             * makes me think this will scan the dependencies, then find those sectors, and infect them with the effect, not just facilities in the current sector
             */
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ChangeMeepleAmount : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.sectorItsAPartOf.blackMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.blueMeeples += card.data.meepleAmount;
        facilityActedUpon.sectorItsAPartOf.purpleMeeples += card.data.meepleAmount;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class IncreaseOvertimeAmount : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.sectorItsAPartOf.overTimeCharges++;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

/// <summary>
/// Takes 5 random cards from Discard and adds them back into the hand.
/// NOTE: At the moment, the number 5 is hard coded cause there's only one card that
/// has this mechanic but a new variable would have to be created if this were to be 
/// expanded upon.
/// </summary>
public class ShuffleCardsFromDiscard : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        GameObject[] cardsShuffledFromDiscard = new GameObject[5];
        if (player.Discards.Count == 0) {
            Debug.LogWarning("The discard pile is empty");
            return;
        }

        for (int i = 0; i < cardsShuffledFromDiscard.Length; i++) {
            int randomIndex = UnityEngine.Random.Range(0, player.Discards.Count);
            cardsShuffledFromDiscard[i] = player.Discards.ElementAt(randomIndex).Value;
            int key = player.Discards.ElementAt(randomIndex).Key;
            player.Discards.Remove(key);
            while (player.HandCards.ContainsKey(key)) {
                key++;
            }
            player.HandCards.Add(key, cardsShuffledFromDiscard[i]);
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ReduceCardCost : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        for (int i = 0; i < cardActedUpon.data.meepleType.Length; i++) {
            switch (cardActedUpon.data.meepleType[i]) {
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

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}




