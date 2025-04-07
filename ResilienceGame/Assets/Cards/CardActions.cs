using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using static Facility;

public class DrawAndDiscardCards : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " played.");
        List<Card> drawnCards = new List<Card>();
        if (player == GameManager.Instance.actualPlayer) {
            player.DrawNumberOfCards(card.data.drawAmount, drawnCards, highlight: (card.data.removeAmount > 0), updateNetwork: true);
            if (card.data.removeAmount > 0) {
                UserInterface.Instance.DisplayAlertMessage($"Discard {card.data.removeAmount} of the highlighted cards", player); //display alert message
                GameManager.Instance.AllowPlayerDiscard(player, card.data.removeAmount, drawnCards);    //allow player to discard cards  
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
        if (player == GameManager.Instance.actualPlayer) {
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
        if (player == GameManager.Instance.actualPlayer) {
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
        facilityActedUpon.AddRemoveEffectsByIdString(
            card.data.effectString,
            true,
            player.playerTeam,
            player.NetID,
            (card.data.duration != 0 ? card.data.duration : -1));
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class NegateEffect : ICardAction {

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class RemoveEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {

        var effectsToRemove = facilityActedUpon.effectManager.GetEffectsRemovableByTeam(player.playerTeam, true);
        if (effectsToRemove != null && effectsToRemove.Count > 0) {
            if (effectsToRemove.Count > 1) {
                for (var i = 0; i < card.data.effectCount; i++) {
                    var effectToRemove = effectsToRemove[i];
                    if (effectToRemove != null) {
                        RemoveEffect(facilityActedUpon, effectToRemove, player);
                    }
                }
            }
            else { //1 element
                RemoveEffect(facilityActedUpon, effectsToRemove[0], player);
            }
        }
        else {
            // Debug.LogError($"No removable effects to remove on {facilityActedUpon.facilityName}");
            Debug.Log("No Removable Effect, this is fine, just a wasted card play");
        }

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class SpreadEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {

        if (facilityActedUpon != null) {
            facilityActedUpon.AddEffectToConnectedSectors(card.data.effectString, player.playerTeam, player.NetID);
        }

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}
public class SelectFacilitiesAddRemoveEffect : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        if (player == GameManager.Instance.actualPlayer) {
            Debug.Log($"Executing Select Facilities and Remove Effect action for card {card.data.name}");
            FacilityEffectType effectTypeToRemove = FacilityEffectType.None;

            var removeEffect = card.data.effectString.Contains("Remove");

            //uses the facility to grab the sector it was played on
            if (facilityActedUpon == null) {
                Debug.LogError(card.data.name + " was played without a facility acted upon");
                return;
            }

            Sector sectorToActOn = facilityActedUpon.sectorItsAPartOf;

            if (sectorToActOn == null) {
                Debug.LogError(card.data.name + " was played without a sector to act on");
                return;
            }

            // Show the EndTurnBlocker
            GameObject tempBlocker = GameObject.FindWithTag("EndTurnBlocker");
            Image tempImage = tempBlocker.GetComponent<Image>();
            var tempColor = tempImage.color;
            tempColor.a = 1f;
            tempImage.color = tempColor;
            tempImage.raycastTarget = true;

            player.ForcePlayerSelectFacilities(
                numFacilitiesToSelect: card.data.targetAmount,
                removeEffect: removeEffect,
                preReqEffect: card.data.preReqEffectType,
                onFacilitySelect: (selectedFacilities) => {

                    //Allows the player from starting a new turn after selecting the facilities 
                    GameObject tempBlocker = GameObject.FindWithTag("EndTurnBlocker");
                    Image tempImage = tempBlocker.GetComponent<Image>();
                    var tempColor = tempImage.color;
                    tempColor.a = 0f;
                    tempImage.color = tempColor;
                    tempImage.raycastTarget = false;

                    //this code is run when the player has selected the facilities they want to remove effects from
                    selectedFacilities.ForEach(facility => {
                        Debug.Log($"Selected facility: {facility.facilityName}");
                        //get the effect to remove before removing it so we can pass it across the network
                        if (removeEffect) {
                            //finds backdoor or fortify only 1 from each facility
                            var effectsToRemove = facility.effectManager.GetEffectsRemovableByTeam(player.playerTeam, removePointsPerTurnEffects: false);
                            effectTypeToRemove = effectsToRemove[0].EffectType;
                        }
                        facility.AddRemoveEffectsByIdString(
                            idString: card.data.effectString,
                            isAdding: true,
                            team: player.playerTeam,
                            createdById: player.NetID);

                    });



                    FacilityType facilityType1 = FacilityType.None;
                    FacilityType facilityType2 = FacilityType.None;
                    FacilityType faciltiyType3 = FacilityType.None;

                    if (selectedFacilities.Count > 0) {
                        facilityType1 = selectedFacilities[0].facilityType;
                    }
                    if (selectedFacilities.Count > 1) {
                        facilityType2 = selectedFacilities[1].facilityType;
                    }
                    if (selectedFacilities.Count > 2) {
                        faciltiyType3 = selectedFacilities[2].facilityType;
                    }

                    //update the message in queue with the new facility info, then send the update
                    player.UpdateNextInQueueMessage(
                        cardMessageType: CardMessageType.CardUpdateWithExtraFacilityInfo,
                        CardID: card.data.cardID,
                        UniqueID: card.UniqueID,
                        Amount: card.data.effectCount, //not needed maybe?
                        effectTargetType: effectTypeToRemove,
                        facilityDroppedOnType: FacilityType.None, //ensure it gets picked up by the sector update code in card player
                        facilityType1: facilityType1,
                        facilityType2: facilityType2,
                        facilityType3: faciltiyType3,
                        sendUpdate: true //TODO: move the network update to the end of animation?
                    );
                });
        }
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

public class ReduceTurnsLeftByBackdoor : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        int turnsRemoved = 0;
        Debug.Log($"Facility: {facilityActedUpon.name}");
        Sector sectorActedUpon = facilityActedUpon.sectorItsAPartOf;
        foreach (Facility facility in sectorActedUpon.facilities) {
            if (facility.HasEffectOfType(FacilityEffectType.Backdoor))
                turnsRemoved += card.data.facilityAmount;
        }
        Debug.Log(card.front.title + " played. Turns removed: " + turnsRemoved);
        GameManager.Instance.ChangeRoundsLeft(-turnsRemoved);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class TemporaryReductionOfTurnsLeft : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        GameManager.Instance.bluffTurnCheck = card.data.duration;
        GameManager.Instance.HandleBluffStart(card.data.duration);
        Debug.Log("Played Hard Bluff");
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class CancelTemporaryReductionOfTurns : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        if (GameManager.Instance.IsBluffActive) {
            GameManager.Instance.BluffCountdown(-1);
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class BackdoorCheckNetworkRestore : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        if (facilityActedUpon.HasEffectOfType(FacilityEffectType.Backdoor)) {
            Debug.Log(card.front.title + " played. Turns removed: " + card.data.facilityAmount);
            GameManager.Instance.ChangeRoundsLeft(-card.data.facilityAmount);
            if (facilityActedUpon.TryRemoveEffectByType(FacilityEffectType.Backdoor, player.NetID))
                Debug.Log($"Backdoor on {facilityActedUpon.facilityName} removed");
            else Debug.Log($"Backdoor unable to be removed on {facilityActedUpon.facilityName} for some reason");
        }
        else {
            facilityActedUpon.AddRemoveEffectsByIdString(
                card.data.effectString,
                true,
                player.playerTeam,
                player.NetID,
                (card.data.duration != 0 ? card.data.duration : -1));
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class ConvertFortifyToBackdoor : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Sector sectorActedUpon = facilityActedUpon.sectorItsAPartOf;
        foreach (Facility facility in sectorActedUpon.facilities) {
            //if it has fortify then it removes it and returns true, which i can detect and add backdoor
            if (facility.TryRemoveEffectByType(FacilityEffectType.Fortify, player.NetID)) {
                facility.AddRemoveEffectsByIdString(card.data.effectString,
                                                    true,
                                                    player.playerTeam,
                                                    player.NetID,
                                                    duration: (card.data.duration != 0 ? card.data.duration : -1));
            }
        }
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class IncreaseTurnsDuringPeace : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        GameManager.Instance.IsRedLayingLow = true;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class IncColorlessMeeplesRoundReduction : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        GameManager.Instance.IsRedAggressive = true;
        player.IncMaxColorlessMeeples((int)card.data.meepleAmtMulti);
        GameManager.Instance.aggressionTurnCheck = card.data.duration;
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

public class ChangeMeepleAmount : ICardAction {
    //deprecated?
    //this action existed for like 3 cards all of which actually did different things
    //TODO: readd this for the change meeple card cost card (Training?)
    //public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
    //    facilityActedUpon.sectorItsAPartOf.blackMeeples += card.data.meepleAmount;
    //    facilityActedUpon.sectorItsAPartOf.blueMeeples += card.data.meepleAmount;
    //    facilityActedUpon.sectorItsAPartOf.purpleMeeples += card.data.meepleAmount;
    //    base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    //}

    //public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
    //    Debug.Log("card " + card.front.title + " canceled.");
    //}
}

public class IncreaseOvertimeAmount : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        facilityActedUpon.sectorItsAPartOf.Owner.overTimeCharges++;
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
        player.ChooseMeeplesThenReduceCardCost((int)card.data.meepleAmtMulti, player, card);
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " canceled.");
    }
}

/// <summary>
/// Nation-wide increase overtime amount
/// </summary>
public class NWIncOvertimeAmount : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        GameManager.Instance.playerDictionary.Values.ToList().ForEach(player =>
            player.AddOvertimeCharge()
        );
        GameManager.Instance.EndWhitePlayerTurn(); //end white player turn
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Nation-wide shuffle cards from discard
/// TODO: Currently only adds for the one blue team but without using the parameters and
/// instead using the GaneManager static class, need to figure out how to use for multiple
/// blue players
/// </summary>
public class NWShuffleFromDiscard : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("card " + card.front.title + " played.");
        Debug.Log($"facility amt: {card.data.facilityAmount}\n dur: {card.data.duration}");
        GameManager.Instance.GetPlayerByTeam(PlayerTeam.Blue).ForEach(player => player.AddDiscardsToDeck(card.data.facilityAmount));
        GameManager.Instance.EndWhitePlayerTurn(); //end white player turn
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

/// <summary>
/// Changes physical points across all sectors if they fail a dice roll
/// </summary>
public class ChangeAllFacPointsBySectorType : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {

        //get all the sectors by type
        var sectors = card.data.onlyPlayedOn.Contains(SectorType.All) ?
            GameManager.Instance.AllSectors.Values.ToList().Select(sector => sector.sectorName).ToList() :
            card.data.onlyPlayedOn;

        //get a list of played sectors (not simulated)
        var playedSectors = GameManager.Instance.AllSectors.Values.Where(
            sector => !sector.IsSimulated && sectors.Contains(sector.sectorName)).ToList();

        //Add the on dice roll effect to each sector
        foreach (var sector in playedSectors) {
            sector.AddOnDiceRollEffect(
                    minRoll: card.data.minDiceRoll,
                    effectString: card.data.effectString,
                    playerTeam: player.playerTeam,
                    playerId: player.NetID);

        }
        //show the UI dice roll panel to everyone
        UserInterface.Instance.ShowDiceRollingPanel(
            playedSectors.Select(x => x.sectorName).ToList(),
            card.front.description,
            card.data.minDiceRoll);




        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}
public class ChangeTransFacPointsAllSectors : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {

        var sectors = card.data.onlyPlayedOn.Contains(SectorType.All) ?
            GameManager.Instance.AllSectors.Values.ToList().Select(sector => sector.sectorName).ToList() :
            card.data.onlyPlayedOn;

        var playedSectors = GameManager.Instance.AllSectors.Values.Where(
            sector => !sector.IsSimulated && sectors.Contains(sector.sectorName)).ToList();

        foreach (var sector in playedSectors) {
            sector.AddOnDiceRollEffect(
                    minRoll: card.data.minDiceRoll,
                    effectString: card.data.effectString,
                    playerTeam: player.playerTeam,
                    playerId: player.NetID,
                    FacilityType.Transmission);

        }

        UserInterface.Instance.ShowDiceRollingPanel(
            playedSectors.Select(x => x.sectorName).ToList(),
            card.front.description,
            card.data.minDiceRoll);




        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}


/// <summary>
/// Changes meeple amounts across all sectors if they fail a dice roll
/// TODO: Loaned meeples not yet a mechanic, those should be excluded from this halving.
/// Also todo: its reduced by half for two turns but we haven't quite implemented the turn
/// stuff yet to my knowledge
/// </summary>
public class CheckAllSectorsChangeMeepleAmtMulti : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("Handling Meeple Change Amount on Dice Roll");
        var sectors = card.data.onlyPlayedOn.Contains(SectorType.All) ?
            GameManager.Instance.AllSectors.Values.ToList().Select(sector => sector.sectorName).ToList() :
            card.data.onlyPlayedOn;

        var playedSectors = GameManager.Instance.AllSectors.Values.Where(
            sector => !sector.IsSimulated && sectors.Contains(sector.sectorName)).ToList();

        //Add the on dice roll effect to each sector
        foreach (var sector in playedSectors) {
            sector.AddOnDiceRollChangeMeepleAmtMulti(
                    minRoll: card.data.minDiceRoll,
                    meepleType: card.data.meeplesChanged,
                    removalTime: card.data.duration,
                    amount: card.data.meepleAmtMulti);

        }
        //show the UI dice roll panel to everyone
        UserInterface.Instance.ShowDiceRollingPanel(
            playedSectors.Select(x => x.sectorName).ToList(),
            card.front.description,
            card.data.minDiceRoll);

        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}
public class IncreaseBaseMaxMeeples : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("Handling Meeple Change Amount on Dice Roll");
        GameManager.Instance.GetPlayerByTeam(PlayerTeam.Blue).ForEach(player => player.PermaIncAllMeeplesByOne());
        GameManager.Instance.EndWhitePlayerTurn(); //end white player turn
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}
//TODO: needs a network update to keep blue players counts updated, cut for now
public class IncreaseBaseMaxMeeplesRandom : ICardAction {
    public override void Played(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        Debug.Log("Handling Change base meeple amount random");
        GameManager.Instance.GetPlayerByTeam(PlayerTeam.Blue).ForEach(player => player.PermaIncRandomMeepleByFlatAmt((int)card.data.meepleAmtMulti));
        GameManager.Instance.EndWhitePlayerTurn(); //end white player turn
        base.Played(player, opponent, facilityActedUpon, cardActedUpon, card);
    }

    public override void Canceled(CardPlayer player, CardPlayer opponent, Facility facilityActedUpon, Card cardActedUpon, Card card) {
        base.Canceled(player, opponent, facilityActedUpon, cardActedUpon, card);
    }
}

