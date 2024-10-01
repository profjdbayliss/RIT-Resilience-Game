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
            player.DrawNumberOfCards(card.data.drawAmount, drawnCards, highlight: (card.data.removeAmount > 0), updateNetwork: true);
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
       // PlayerTeam playedTeam = card.DeckName.ToLower().Trim() == "blue" ? PlayerTeam.Blue : PlayerTeam.Red;
        facilityActedUpon.AddRemoveEffectsByIdString(card.data.effectString, true, player.playerTeam);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class NegateEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {

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
        var effectType = card.DeckName.ToLower().Trim() == "blue" ? PlayerTeam.Blue : PlayerTeam.Red;

        // TODO: Implement user selection for effect removal
        if (effectList.Count > 0) {
            var randomEffect = effectList[UnityEngine.Random.Range(0, effectList.Count)];
            facilityActedUpon.AddRemoveEffectsByIdString(idString: randomEffect.CreatedEffectID.ToString(),
                                                         isAdding: false, 
                                                         team: player.playerTeam);
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

/// <summary>
/// NW stands for nation wide as this will affect all the sectors. This method intends to
/// give each sector a give number of meeples of each type
/// </summary>
public class NWMeepleChangeEach : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        CardPlayer playerInstance;
        if (GameManager.instance.playerType == PlayerTeam.Blue)
            playerInstance = GameManager.instance.actualPlayer;
        else playerInstance = GameManager.instance.opponentPlayer;
        //at the moment this method doesnt actually handle multiple sectors because
        //we dont know how to implement multiple sectors yet. that being said this doesnt 
        //use stuff like facilityactedupon and relies directly upon the game manager singleton
        foreach (string meepleType in card.data.meepleType)
        {
            switch(meepleType)
            {
                case "Blue":
                    playerInstance.playerSector.blueMeeples += card.data.meepleAmount;
                    break;
                case "Black":
                    playerInstance.playerSector.blackMeeples += card.data.meepleAmount;
                    break;
                case "Purple":
                    playerInstance.playerSector.purpleMeeples += card.data.meepleAmount;
                    break;
                default:
                    Debug.Log("meeple type isnt black blue or purple for some reason");
                    break;
            }
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// NW stands for nation wide as this will affect all the sectors. This method intends to
/// give each sector their choice of a give number of meeples of any given type.
/// </summary>
public class NWMeepleChangeChoice : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Nation-wide increase overtime amount
/// </summary>
public class NWIncOvertimeAmount : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        CardPlayer playerInstance;
        if (GameManager.instance.playerType == PlayerTeam.Blue)
            playerInstance = GameManager.instance.actualPlayer;
        else playerInstance = GameManager.instance.opponentPlayer;
        //at the moment this method doesnt actually handle multiple sectors because
        //we dont know how to implement multiple sectors yet. that being said this doesnt 
        //use stuff like facilityactedupon and relies directly upon the game manager singleton
        playerInstance.playerSector.overTimeCharges++;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Nation-wide shuffle cards from discard
/// TODO: Currently only adds for the one blue team but without using the parameters and
/// instead using the GaneManager static class, need to figure out how to use for multiple
/// blue players
/// </summary>
public class NWShuffleFromDiscard : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        //nowhere in the csv does it indicate the number of cards that should be 
        //shuffled from discard except the description
        GameObject[] cardsShuffledFromDiscard = new GameObject[5];
        CardPlayer playerInstance;
        if (GameManager.instance.playerType == PlayerTeam.Blue)
            playerInstance = GameManager.instance.actualPlayer;
        else playerInstance = GameManager.instance.opponentPlayer;
        if (playerInstance.Discards.Count == 0)
        {
            Debug.LogWarning("The discard pile is empty");
            return;
        }

        for (int i = 0; i < cardsShuffledFromDiscard.Length; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, playerInstance.Discards.Count);
            cardsShuffledFromDiscard[i] = playerInstance.Discards.ElementAt(randomIndex).Value;
            int key = playerInstance.Discards.ElementAt(randomIndex).Key;
            playerInstance.Discards.Remove(key);
            while (playerInstance.HandCards.ContainsKey(key))
            {
                key++;
            }
            playerInstance.HandCards.Add(key, cardsShuffledFromDiscard[i]);
        }
            base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Changes physical points across all sectors if they fail a dice roll
/// </summary>
public class NWChangePhysPointsDice : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        CardPlayer playerInstance;
        if (GameManager.instance.playerType == PlayerTeam.Blue)
            playerInstance = GameManager.instance.actualPlayer;
        else playerInstance = GameManager.instance.opponentPlayer;
        int diceRoll = UnityEngine.Random.Range(1, 6);
        if ( diceRoll < card.data.minDiceRoll)
        {
            Debug.Log("Sector rolled a " + diceRoll + ", roll failed.");
            foreach(Facility facility in playerInstance.playerSector.facilities)
            {
                facility.ChangeFacilityPoints(FacilityPointTarget.Physical, card.data.facilityAmount);
            }
        }
        else
        {
            Debug.Log("Sector rolled a " + diceRoll + ", roll successful!");
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Changes financial points across all sectors if they fail a dice roll
/// </summary>
public class NWChangeFinPointsDice : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        CardPlayer playerInstance;
        if (GameManager.instance.playerType == PlayerTeam.Blue)
            playerInstance = GameManager.instance.actualPlayer;
        else playerInstance = GameManager.instance.opponentPlayer;
        int diceRoll = UnityEngine.Random.Range(1, 6);
        if (diceRoll < card.data.minDiceRoll)
        {
            Debug.Log("Sector rolled a " + diceRoll + ", roll failed.");
            foreach (Facility facility in playerInstance.playerSector.facilities)
            {
                facility.ChangeFacilityPoints(FacilityPointTarget.Financial, card.data.facilityAmount);
            }
        }
        else
        {
            Debug.Log("Sector rolled a " + diceRoll + ", roll successful!");
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Changes meeple amounts across all sectors if they fail a dice roll
/// </summary>
public class NWChangeMeepleAmtDice : ICardAction
{
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        int diceRoll = UnityEngine.Random.Range(1, 6);
        if (diceRoll < card.data.minDiceRoll)
        {
            Debug.Log("Sector rolled a " + diceRoll + ", roll failed.");
        }
        else
        {
            Debug.Log("Sector rolled a " + diceRoll + ", roll successful!");
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card)
    {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}




