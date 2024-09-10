using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardPlayValidator {
    public static bool CanPlayCard(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        if (!player.IsPlayerTurn()) return false;


        switch (GameManager.instance.MGamePhase) {
            case GamePhase.Start:
                CanPlayCardDuringStartPhase(player, card, playLocation);
                break;
            case GamePhase.Draw:
                CanPlayCardDuringDrawPhase(player, card, playLocation);
                break;
            case GamePhase.Overtime:
                CanPlayCardDuringOvertimePhase(player, card, playLocation);
                break;
            case GamePhase.Action:
                CanPlayCardDuringActionPhase(player, card, playLocation);
                break;
            case GamePhase.Discard:
                CanPlayCardDuringDiscardPhase(player, card, playLocation);
                break;
            case GamePhase.Donate:
                CanPlayCardDuringDonatePhase(player, card, playLocation);
                break;
            case GamePhase.End:
                CanPlayCardDuringEndPhase(player, card, playLocation);
                break;
            default: return false;
        }
        return false;
    }
    //cant play cards at all during start phase
    private static bool CanPlayCardDuringStartPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        return false;
    }
    //only allowed to discard and draw new cards during this phase (up to 3)
    private static bool CanPlayCardDuringDrawPhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
        //TODO swap this to tags
        if (playLocation.name == "DiscardDrop") {
            
        }
        return false;
    }

    private static bool CanPlayCardDuringOvertimePhase(CardPlayer player, Card card, UnityEngine.GameObject playLocation) {
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
