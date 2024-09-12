using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardPlayValidator {
    public static bool CanPlayCard(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        if (!player.IsPlayerTurn()) return false;


        return GameManager.instance.MGamePhase switch {
            GamePhase.Start => CanPlayCardDuringStartPhase(player, card, playLocation),
            GamePhase.Draw => CanPlayCardDuringDrawPhase(player, card, playLocation),
            GamePhase.Bonus => CanPlayCardDuringBonusPhase(player, card, playLocation),
            GamePhase.Action => CanPlayCardDuringActionPhase(player, card, playLocation),
            GamePhase.End => CanPlayCardDuringEndPhase(player, card, playLocation),
            _ => false,
        };
    }
    //cant play cards at all during start phase
    private static bool CanPlayCardDuringStartPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }
    //only allowed to discard and draw new cards during this phase (up to 3)
    private static bool CanPlayCardDuringDrawPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {

        return playLocation.tag switch {
            "DiscardDropLocation" => player.CardsDiscardedThisPhase < GameManager.MAX_DISCARDS,
            _ => false,
        };
    }

    //TODO: Get clarification on what you do in this phase
    private static bool CanPlayCardDuringBonusPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }
    private static bool CanAffordCard(CardPlayer player, Card card) {
        return player.CanAffordToPlay(card);
    }

    private static bool CanPlayCardDuringActionPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {

        if (!CanAffordCard(player, card)) return false;

        return playLocation.tag switch {
            "FreePlayLocation" => CheckActionFreePlay(player, card, playLocation),
            "DiscardDropLocation" => false,
            "FacilityDropLocation" => CheckActionFacilityPlay(player, card, playLocation),
            _ => false,
        };
    }

    private static bool CheckActionFreePlay(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return card.data.playableTarget switch {
            CardTarget.Hand => true,        //play card on 'hand' (draw/discard) cards 
            CardTarget.Card => true,        //play card on 'card' (reduces cost of card) cards TODO: this will need a seperate handler script to run the action
            CardTarget.Effect => false,     //This doesn't current exist in the game
            CardTarget.Facility => false,   //Cannot play a facility card in the free play zone
            CardTarget.Sector => true,      //Assume this is correct? the free play zone is the whole sector?
            _ => false,
        };
    }

    private static bool CheckActionFacilityPlay(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return card.data.playableTarget switch {
            CardTarget.Hand => false,       //Cannot play a hand card in the facility zone
            CardTarget.Card => false,       //Cannot play a card card in the facility zone
            CardTarget.Effect => false,     //Cannot play an effect card in the facility zone
            CardTarget.Facility => true,    //Can play a facility card in the facility zone
            CardTarget.Sector => false,     //Cannot play a sector card in the facility zone
            _ => false,
        };
        return false;
    }

    private static bool CanPlayCardDuringDiscardPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }

    private static bool CanPlayCardDuringDonatePhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }

    private static bool CanPlayCardDuringEndPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }

}
