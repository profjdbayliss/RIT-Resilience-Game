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
        List<Card> drawnCards = new List<Card>();
        if (player == GameManager.instance.actualPlayer) {

            //for (int i = 0; i < card.data.drawAmount; i++) {
            //    Card cardDrawn = player.DrawCard(true, 0, -1, ref player.DeckIDs, player.handDropZone, true, ref player.HandCards);
            //    if (card.data.removeAmount > 0) {
            //        cardDrawn.ToggleOutline(true);
            //        drawnCards.Add(cardDrawn);
            //    }

            //}
            player.DrawNumberOfCards(card.data.drawAmount, drawnCards, true, updateNetwork: true);
            if (card.data.removeAmount > 0) {
                GameManager.instance.DisplayAlertMessage($"Discard {card.data.removeAmount} of the highlighted cards", player); //display alert message
                GameManager.instance.AllowPlayerDiscard(player, card.data.removeAmount, drawnCards);    //allow player to discard cards  
            }
            base.Played(player, opponent, facilityActedUpon, cardActedUpon, card); 
        }
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ShuffleAndDrawCards : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " played.");
        if (player == GameManager.instance.actualPlayer) {
            player.ForcePlayerReturnCardsToDeck(card.data.removeAmount, () => {
                player.DrawNumberOfCards(card.data.drawAmount, updateNetwork: true);
            });
            base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
        }
        
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}
public class ReturnHandToDeckAndDraw : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " played.");
        if (player == GameManager.instance.actualPlayer) {
            player.ReturnHandToDeckAndDraw(card.data.drawAmount);
            base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
        }
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class AddEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        FacilityTeam playedTeam = card.DeckName.ToLower().Trim() == "blue" ? FacilityTeam.Blue : FacilityTeam.Red;
        facilityActedUpon.AddRemoveEffectsByIdString(card.data.effectIds, true, playedTeam);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

//TODO: These are the same, should be combined, difference is handled in the FacilityEffectManager
public class NegateEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
       // var effectType = card.DeckName.ToLower().Trim() == "blue" ? FacilityTeam.Blue : FacilityTeam.Red;

        // Get all active effects on the facility
        var activeEffects = facilityActedUpon.effectManager.GetEffects();

        // If there are any active effects, negate a random one
        if (activeEffects.Count > 0) {
            int randomIndex = UnityEngine.Random.Range(0, activeEffects.Count);
            var effectToNegate = activeEffects[randomIndex];
            facilityActedUpon.effectManager.NegateEffect(effectToNegate);

            Debug.Log($"Negated random effect: {effectToNegate.EffectType} on {facilityActedUpon.facilityName}");
        }
        else {
            Debug.Log($"No active effects to negate on {facilityActedUpon.facilityName}");
        }

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

        // TODO: Implement user selection for effect removal
        if (effectList.Count > 0) {
            var randomEffect = effectList[UnityEngine.Random.Range(0, effectList.Count)];
            facilityActedUpon.AddRemoveEffectsByIdString(randomEffect.CreatedEffectID.ToString(), false, effectType);
        }

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class SpreadEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        // TODO: Implement effect spreading to connected sectors
        foreach (var dependency in facilityActedUpon.dependencies) {
            // Placeholder: Apply the effect to all facilities in the connected sector
            //foreach (var facility in dependency.facilities) {
            //    facility.AddRemoveEffectsByIdString(card.data.effectIds, true, card.DeckName.ToLower().Trim() == "blue" ? FacilityTeam.Blue : FacilityTeam.Red);
            //}
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




