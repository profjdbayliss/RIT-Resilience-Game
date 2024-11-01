using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhiteCardPlayer : CardPlayer
{
    // Start is called before the first frame update
    public override void Start()
    {
        InitDropLocations();
        MAX_DRAW_AMOUNT = 1000;
        NetID = 999;
        playerName = "WhitePlayer";
        playerTeam = PlayerTeam.White;
        DeckName = "white";
        
    }
    
    public override void InitializeCards() {
        DeckIDs.Clear();
        //manager = GameObject.FindObjectOfType<GameManager>();
        Debug.Log("card count is: " + cards.Count);
        foreach (Card card in cards.Values) {
            if (card != null && card.DeckName.Equals(DeckName)) {
                //    Debug.Log("adding card " + card.name + " with id " + card.data.cardID + " to deck " + DeckName);
                for (int j = 0; j < card.data.numberInDeck; j++) {
                    DeckIDs.Add(card.data.cardID);
                }
            }
        }
        Debug.Log("white deck count is: " + DeckIDs.Count);
        DrawCardsToFillHand();
    }

    public void PlayCard() {
        Debug.Log($"White player is playing a card");
        
        var card = GetRandomPlayableCard(positive: true);
        if (card) {
            Debug.Log("White player is playing card: " + card.data.name);   
        }

    }
    public Card GetRandomPlayableCard(bool positive) {
        var index = Random.Range(0, HandCards.Count);
        if (HandCards.Any()) {
            return HandCards.ElementAt(index).Value.GetComponent<Card>();
        }
        Debug.LogError($"Couldnt find any cards in white player hand");
        return null;
       

    }


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            PlayCard();
        }
    }
}
