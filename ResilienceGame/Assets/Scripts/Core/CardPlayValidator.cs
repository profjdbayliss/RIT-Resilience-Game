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

    private static bool CanPlayCardDuringBonusPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }

    private static bool CanPlayCardDuringActionPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
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
