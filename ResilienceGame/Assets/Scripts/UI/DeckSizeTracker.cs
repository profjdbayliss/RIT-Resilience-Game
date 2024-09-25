using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckSizeTracker : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI opponentDeckText;
    [SerializeField] private TextMeshProUGUI playerDeckText;
    [SerializeField] private TextMeshProUGUI opponentHandText;
    [SerializeField] private TextMeshProUGUI playerHandText;

    public void UpdateAllTrackerTexts(int playerDeckSize, int playerHandSize, int opponentDeckSize, int opponentHandSize) {
        playerDeckText.text = playerDeckSize.ToString();
        playerHandText.text = playerHandSize.ToString();
        opponentDeckText.text = opponentDeckSize.ToString();
        opponentHandText.text = opponentHandSize.ToString();
    }
    public void UpdatePlayerDeckSize(int size) {
        playerDeckText.text = size.ToString();
    }

    public void UpdateOpponentDeckSize(int size) {
        opponentDeckText.text = size.ToString();
    }

    public void UpdatePlayerHandSize(int size) {
        playerHandText.text = size.ToString();
    }

    public void UpdateOpponentHandSize(int size) {
        opponentHandText.text = size.ToString();
    }
}
