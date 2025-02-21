using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

public class AICardPlayer : CardPlayer {

    [SerializeField] private RectTransform handParent;
    public override Card HandleCardDrop(Card card) {


        //check for a card play or card discard
        
            // Debug.Log("Card is valid to play and player is ready");
            //set var to hold where the card was dropped

            //set card state to played
            card.SetCardState(CardState.CardDrawnDropped);
            //remove card from hand
            handPositioner.cards.Remove(card);
            //set the parent to where it was played
            card.transform.transform.SetParent(cardDroppedOnObject.transform);
            return card;
        

        
    }
    public void DebugPlayCard() {
         PlayCard(sendUpdate: false);
        
    }
    public void PlayCard(bool sendUpdate = true) {
        Debug.Log($"ai player {playerName} is trying to play a card");

        Card _card = GetRandomPlayableCard(out GameObject dropLocation);
        
        HandleCardDrop(_card);
        


        if (_card == null) {
            Debug.LogWarning($"No valid Card found for ai player {playerName}");
            return;
        }
        HandCards.Remove(_card.UniqueID);
        if (_card) {
            Debug.Log("ai player is playing card: " + _card.data.name);
            _card.transform.SetParent(UserInterface.Instance.gameCanvas.transform, true);

            if (sendUpdate) {
                EnqueueAndSendCardMessageUpdate(CardMessageType.CardUpdate,
                    _card.data.cardID,
                    _card.UniqueID);
            }



            //card.transform.localScale = new Vector3(.5f, .5f, .5f);
            //StartCoroutine(
            //    MoveToPositionAndScale(
            //        card: _card.GetComponent<RectTransform>(),
            //        targetPos: new Vector2(0, 0),
            //        onComplete: () => {
            //            StartCoroutine(
            //                MoveToPositionAndScale(
            //                    card: _card.GetComponent<RectTransform>(),
            //                    targetPos: UserInterface.Instance.discardPile.anchoredPosition,
            //                    onComplete: () => {
            //                        _card.Play(this); //play the card

            //                        Destroy(_card.gameObject); //destroy it after
            //                    },
            //                    duration: 1f,
            //                    scaleUpAmt: .01f));

            //        },
            //        duration: 1f,
            //        scaleUpAmt: 2f));

        }

    }
    public Sector GetFirstPlayableSector() {
        return playerTeam == PlayerTeam.Blue ?
           PlayerSector :
           GameManager.Instance.AllSectors.Values.FirstOrDefault(x => !x.IsSimulated);
    }
    public GameObject GetFirstPossibleDropLocation(Card card) {
        Sector sector = GetFirstPlayableSector();
        if (sector != null) {
            for (int i = 0; i < sector.facilities.Length; i++) {
                if (ValidateCardPlay(card, sector.facilities[i].gameObject)) {
                    return sector.facilities[i].gameObject;
                }
            }
        }
        return null;
    }

    public Card GetRandomPlayableCard(out GameObject dropLocation) {
        var cards = HandCards.Values.Select(x => x.GetComponent<Card>());
        Card cardToPlay = null;
        foreach (Card card in cards) {
            dropLocation = GetFirstPossibleDropLocation(card);
            if (dropLocation != null) {
                if (ValidateCardPlay(card, dropLocation)) {
                    cardDroppedOnObject = dropLocation;
                    cardToPlay = card;
                    break;
                }
            }

        }

        dropLocation = null;
        return null;
    }

}
