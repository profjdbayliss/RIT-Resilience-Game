using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {

    public CardPlayer actualPlayer;
    public CardPlayer opponentPlayer; //TODO: remove this and replace with a list of players

    public static UserInterface Instance;


    [Header("UI Elements")]
    public Canvas gameCanvas;
    public GameObject startScreen;
    public GameObject alertScreenParent;
    public GameObject tiles;
    public TextMeshProUGUI StatusText;
    public TextMeshProUGUI mTurnText;
    public TextMeshProUGUI mPhaseText;
    public TextMeshProUGUI mPlayerName;
    public TextMeshProUGUI mPlayerDeckType;
    public TextMeshProUGUI mOpponentName;
    public TextMeshProUGUI mOpponentDeckType;
    public TextMeshProUGUI activePlayerText;
    public Color activePlayerColor;
    public GameObject mEndPhaseButton;
    public GameObject opponentPlayedZone;
    public Camera cam;
    public TextMeshProUGUI titlee;
    public AlertPanel mAlertPanel;
    public DeckSizeTracker deckSizeTracker;
    public GameObject discardDropZone;
    public GameObject handDropZone;
    public GameObject opponentDropZone;
    public TextMeshProUGUI[] meeplesAmountText;
    [SerializeField] private Button[] meepleButtons;
    [SerializeField] private GameObject gameLogMessagePrefab;
    [SerializeField] private Transform gameLogParent;

    [Header("Drag and Drop")]
    public GameObject hoveredDropLocation;
    // Start is called before the first frame update
    public void StartGame(PlayerTeam playerType) {
        alertScreenParent.SetActive(false); //Turn off the alert (selection) screen
        mTurnText.text = "" + GameManager.Instance.GetTurnsLeft();
        mPhaseText.text = "Phase: " + GameManager.Instance.MGamePhase.ToString();
        mPlayerName.text = RGNetworkPlayerList.instance.localPlayerName;
        mPlayerDeckType.text = "" + playerType;
        Debug.Log($"Creating UI for {RGNetworkPlayerList.instance.localPlayerName} who is type {playerType}");
        if (playerType != PlayerTeam.Red) {
            meepleButtons[3].gameObject.SetActive(false);
        }
    }

    private void Awake() {
        Instance = this;
    }


    public void UpdateMeepleAmountUI(float blackMeeples, float blueMeeples, float purpleMeeples) {
        meeplesAmountText[0].text = blackMeeples.ToString();
        meeplesAmountText[1].text = blueMeeples.ToString();
        meeplesAmountText[2].text = purpleMeeples.ToString();
    }
    public void EnableMeepleButtons() {
        foreach (Button button in meepleButtons) {
            button.interactable = true;
        }
    }
    public void DisableMeepleButtons() {
        foreach (Button button in meepleButtons) {
            button.interactable = false;
        }
    }
    //called by the buttons in the sector canvas
    public void TryButtonSpendMeeple(int index) {
        if (meepleButtons[index].interactable) {
            actualPlayer.SpendMeepleWithButton(index);
        }
    }
    public void EnableDiscardDrop() {
        discardDropZone.SetActive(true);
    }
    public void DisableDiscardDrop() {
        discardDropZone.SetActive(false);
    }
    public void EnableMeepleButtonByIndex(int index) {
        meepleButtons[index].interactable = true;
    }
    public void DisableMeepleButtonByIndex(int index) {
        meepleButtons[index].interactable = false;
    }
    public void ToggleGameCanvas(bool enable) {
        gameCanvas.enabled = enable;
    }
    public void SetOpponentNameAndDeckText(string name, string deckName) {
        mOpponentName.text = name;
        mOpponentDeckType.text = deckName;
    }
    public void ToggleStartScreen(bool enable) {
        startScreen.SetActive(enable);
    }
    public void ToggleEndPhaseButton(bool enable) {
        mEndPhaseButton.SetActive(enable);
    }
    //adds the string message to the game log
    //if the message is from the network, or this is the server, it will be added to the game log
    //otherwise its sent to the server and returned to ensure log consistency
    public void AddActionLogMessage(string message, bool fromNet, bool IsServer, ref List<string> messageLog) {
        if (fromNet || IsServer) {
            Instantiate(gameLogMessagePrefab, gameLogParent).GetComponent<TextMeshProUGUI>().text = message;
            messageLog.Add(message);
        }
        else {
            RGNetworkPlayerList.instance.SendStringToServer(message);
        }

    }
    public void UpdateUISizeTrackers() {
        //TODO: Add check for all red players
        deckSizeTracker.UpdateAllTrackerTexts(
            playerDeckSize: actualPlayer.DeckIDs.Count,
            playerHandSize: actualPlayer.HandCards.Count,
            opponentDeckSize: opponentPlayer.DeckIDs.Count,
            opponentHandSize: opponentPlayer.HandCards.Count);
    }
    // Show the cards and game UI for player.
    public void ShowPlayUI() {
        handDropZone.SetActive(true);
        discardDropZone.SetActive(true);
    }

    // Hide the cards and game UI for the player.
    public void HidePlayUI() {
        handDropZone.SetActive(false);
        discardDropZone.SetActive(false);
    }
    // display info about the game's status on the screen
    public void DisplayGameStatus(string message) {
        StatusText.text = message;
    }
    public void DisplayAlertMessage(string message, CardPlayer player, int duration = -1, Action onAlertFinish = null) {
        if (player == actualPlayer) {
            if (onAlertFinish == null)
                mAlertPanel.ShowTextAlert(message, duration);
            else
                mAlertPanel.ShowTextAlert(message, onAlertFinish);
        }
    }
    public void DisplayCardChoiceMenu(Card card, int numRequired) {
        mAlertPanel.AddCardToSelectionMenu(card.gameObject);
        if (numRequired < 3)
            mAlertPanel.ToggleCardSelectionPanel(true);

    }
    public void SetPhaseText(string text) {
        mPhaseText.text = text;
    }
    public void ResolveTextAlert() {
        mAlertPanel.ResolveTextAlert();
    }
    public void SetTurnText(string text) {
        mTurnText.text = text;
    }

}
