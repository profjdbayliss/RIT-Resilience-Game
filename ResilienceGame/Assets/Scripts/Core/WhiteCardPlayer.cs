using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class WhiteCardPlayer : CardPlayer {

    [SerializeField] private RectTransform handParent;
    public int DiceRoll { get; private set; }

    // Start is called before the first frame update
    public override void Start() {
        InitDropLocations();
        MAX_DRAW_AMOUNT = 1000;
        NetID = 999;
        playerName = "WhitePlayer";
        playerTeam = PlayerTeam.White;
        DeckName = "white";

    }

    public override void InitializeCards() {
        // Debug.Log("Init white cards");
        DeckIDs.Clear();
        //manager = GameObject.FindObjectOfType<GameManager>();
        //  Debug.Log("card count is: " + cards.Count);
        foreach (Card card in cards.Values) {
            if (card != null && card.DeckName.Contains("white")) {
                //    Debug.Log("adding card " + card.name + " with id " + card.data.cardID + " to deck " + DeckName);
                for (int j = 0; j < card.data.numberInDeck; j++) {
                    DeckIDs.Add(card.data.cardID);
                }
            }
        }

        
        //  Debug.Log("white deck count is: " + DeckIDs.Count);
        DrawCardsToFillHand(false);


        //no idea why the cards were in a different order??

        var cardList = HandCards.Values.Select(x => x.GetComponent<Card>()).ToList();
        cardList.Sort((card1, card2) => card1.data.name.CompareTo(card2.data.name));

        HandCards.Clear();
        for (int i = 0; i < cardList.Count; i++) {
            cardList[i].UniqueID = i;
            HandCards.Add(i, cardList[i].gameObject);
        }


    }

    //handles the playing of a card both on the network and locally
    public void PlayCard(Card card = null, bool sendUpdate = true) {
        Debug.Log($"White player is playing a card");

        var _card = card != null ? card : GetRandomPlayableCard(positive: true);
        HandCards.Remove(_card.UniqueID);
        if (_card) {
            Debug.Log("White player is playing card: " + _card.data.name);
            _card.transform.SetParent(UserInterface.Instance.gameCanvas.transform, true);

            //if we are sending an update, include a dice roll with the update to be used everywhere
            if (sendUpdate) {
                DiceRoll = UnityEngine.Random.Range(1, 7);
                Debug.Log($"$$Creating White Card play with DiceRoll: {DiceRoll}");
                EnqueueAndSendCardMessageUpdate(CardMessageType.CardUpdate,
                _card.data.cardID,
                _card.UniqueID,
                Amount: DiceRoll);
            }
           
            

            card.transform.localScale = new Vector3(.5f, .5f, .5f);
            StartCoroutine(
                MoveToPositionAndScale(
                    card: _card.GetComponent<RectTransform>(),
                    targetPos: new Vector2(0, 0),
                    onComplete: () => {
                        StartCoroutine(
                            MoveToPositionAndScale(
                                card: _card.GetComponent<RectTransform>(),
                                targetPos: UserInterface.Instance.discardPile.anchoredPosition,
                                onComplete: () => {
                                    _card.Play(this);
                                    Destroy(_card.gameObject);
                                },
                                duration: 1f,
                                scaleUpAmt: .01f));

                    },
                    duration: 1f,
                    scaleUpAmt: 2f));

        }

    }
    public void DebugPlayCard(Card card) {
        PlayCard(
            HandCards.Values
            .Select(c => c.GetComponent<Card>())
            .FirstOrDefault(c => c.data.cardID == card.data.cardID),
            true);
    }
    private IEnumerator MoveToPositionAndScale(RectTransform card, Vector2 targetPos, Action onComplete, float duration, float scaleUpAmt) {

        var startingPos = card.anchoredPosition;
        var endingPos = targetPos;
        var time = 0f;
        var currentScale = card.localScale;
        var endScale = new Vector3(card.localScale.x * scaleUpAmt,
                                card.localScale.y * scaleUpAmt,
                                card.localScale.z * scaleUpAmt);

        while (time < duration) {
            time += Time.deltaTime;
            var t = time / duration;
            t = CubicEaseInOut(t);
            card.anchoredPosition = Vector2.Lerp(startingPos, endingPos, t);
            card.localScale = Vector3.Lerp(currentScale, endScale, t);
            yield return null;
        }

        card.anchoredPosition = endingPos;
        card.localScale = endScale;

        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();


    }
    private float CubicEaseInOut(float t) {
        if (t < 0.5f)
            return 4f * t * t * t;
        else {
            float f = (2f * t) - 2f;
            return 0.5f * f * f * f + 1f;
        }
    }
    public void HandleNetworkUpdate(Update update, GamePhase phase) {
        if (HandCards.TryGetValue(update.UniqueID, out GameObject cardgo)) {
            Card card = cardgo.GetComponent<Card>();
            if (update.Amount > 0) {
                DiceRoll = update.Amount;
                Debug.Log($"$$Setting Dice Roll to {DiceRoll} from network update");
                
            }
            else {
                Debug.LogWarning($"$$No dice roll found in network update");
            }
            PlayCard(card, false);
        }
    }


    public Card GetRandomPlayableCard(bool positive) {
        var cardPlays = HandCards.Values
            .Select(x => x.GetComponent<Card>())
            .Where(x => x.DeckName == (positive ? "positive" : "negative")).ToList();

        if (cardPlays.Count > 0) {
            return cardPlays[UnityEngine.Random.Range(0, cardPlays.Count)];
        }

        Debug.LogError($"Couldnt find any cards in white player hand");
        return null;

    }
    public override void DrawNumberOfCards(int num, List<Card> cardsDrawn = null, bool highlight = false, bool updateNetwork = false) {

        Card cardDrawn = null;
        if (DeckIDs.Count > 0) {
            for (int i = 0; i < num; i++) {
                if (DeckIDs.Count <= 0) {
                    return;
                }
                cardDrawn = DrawCard(
                    random: true,
                    cardId: 0,
                    uniqueId: -1,
                    deckToDrawFrom: ref DeckIDs,
                    dropZone: handParent.gameObject,
                    allowSlippy: false,
                    activeDeck: ref HandCards,
                    sendUpdate: false);
                cardsDrawn?.Add(cardDrawn);
            }
        }
        
        
        
    }
    protected override Card DrawCard(bool random, int cardId, int uniqueId, ref List<int> deckToDrawFrom,
        GameObject dropZone, bool allowSlippy, ref Dictionary<int, GameObject> activeDeck, bool sendUpdate = false) {
        var card = base.DrawCard(random, cardId, uniqueId, ref deckToDrawFrom, dropZone, false, ref activeDeck, sendUpdate);
        card.DeckName = card.DeckName.Replace("white;", "").Trim();
        return card;
    }


    // Update is called once per frame
    void Update() {
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            PlayCard();
        }
    }
}
