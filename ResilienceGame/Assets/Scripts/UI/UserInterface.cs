using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {

    //public CardPlayer actualPlayer;
    //public CardPlayer opponentPlayer; //TODO: remove this and replace with a list of players

    public static UserInterface Instance;
    [SerializeField]
    private Sprite[] infoBackgrounds;
    [SerializeField] private Sprite[] doomClockSprites;
    [SerializeField] private GameObject playerMenuPrefab;
    [SerializeField] private PlayerLobbyManager playerLobbyManager;
    [SerializeField] private Sprite[] PhaseButtonSprites;

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
    //public TextMeshProUGUI mOpponentName;
    //public TextMeshProUGUI mOpponentDeckType;
    public TextMeshProUGUI activePlayerText;
    public Color activePlayerColor;
    public GameObject mEndPhaseButton;
    public GameObject opponentPlayedZone;
    public Camera cam;
    public TextMeshProUGUI titlee;
    public AlertPanel mAlertPanel;
    //public DeckSizeTracker deckSizeTracker;
    public GameObject discardDropZone;
    public GameObject handDropZone;
    public GameObject opponentDropZone;
    public TextMeshProUGUI[] meeplesAmountText;
    [SerializeField] private Button[] meepleButtons;
    [SerializeField] private GameObject gameLogMessagePrefab;
    [SerializeField] private Transform gameLogParent;
    [SerializeField] private Image infoBg;
    [SerializeField] private Image doomClockImage;
    [SerializeField] private GameObject doomClockBg;
    [SerializeField] private RectTransform playerMenuContainer;
    [SerializeField] private RectTransform playerMenuParent;
    [SerializeField] private GameObject overTimeButton;
    [SerializeField] private TextMeshProUGUI overTimeChargesText;
    [SerializeField] private TextMeshProUGUI overTimeTurnsLeftText;
    [SerializeField] private TextMeshProUGUI overTimeTurnsLabelText;
    [SerializeField] private TextMeshProUGUI playerDeckText;
    [SerializeField] private TextMeshProUGUI playerHandText;
    [Header("Meeple Sharing")]
    [SerializeField] private GameObject MeepleSharingPanel;
    [SerializeField] private TextMeshProUGUI[] MeepleSharingTextCurrent = new TextMeshProUGUI[3];
    [SerializeField] private TextMeshProUGUI[] MeepleSharingTextMax = new TextMeshProUGUI[3];
    [SerializeField] private GameObject allySelectionMenu;
    [SerializeField] private RectTransform leftSelectionparent;
    [SerializeField] private RectTransform rightSelectionparent;
    [SerializeField] private List<Button> allySelectionButtons;
    [SerializeField] private GameObject allySelectionButtonPrefab;

    [Header("Player Info Menu")]
    [SerializeField] private List<PlayerPopupMenuItem> playerMenuItems;
    private List<Image> sectorXIcons = new List<Image>();


    [Header("Drag and Drop")]
    public GameObject hoveredDropLocation;
    // Start is called before the first frame update
    public void StartGame(PlayerTeam playerType) {
        AddPlayerToLobby(name: RGNetworkPlayerList.instance.localPlayerName, team: playerType);
        alertScreenParent.SetActive(false); //Turn off the alert (selection) screen
        mTurnText.text = "" + GameManager.Instance.GetTurnsLeft();
        mPhaseText.text = "Phase: " + GameManager.Instance.MGamePhase.ToString();
        mPlayerName.text = RGNetworkPlayerList.instance.localPlayerName;
        mPlayerDeckType.text = "" + playerType;
        Debug.Log($"Creating UI for {RGNetworkPlayerList.instance.localPlayerName} who is type {playerType}");
        if (playerType != PlayerTeam.Red) {
            meepleButtons[3].gameObject.SetActive(false);
        }
        playerMenuItems = new List<PlayerPopupMenuItem>();
        var sectorMenu = playerMenuContainer.GetChild(0);
        if (sectorMenu.CompareTag("SectorMenu")) {
            for (int i = 0; i < sectorMenu.childCount; i++) {
                sectorXIcons.Add(sectorMenu.GetChild(i).GetChild(0).GetComponent<Image>());
            }
        }
        UpdateMeepleAmountUI();
    }

    private void Awake() {
        Instance = this;
    }


    public void UpdateMeepleAmountUI(/*float blackMeeples, float blueMeeples, float purpleMeeples, float colorlessMeeples*/) {
        for (int i = 0; i < meeplesAmountText.Length; i++) {
            if (i == 3 && GameManager.Instance.actualPlayer.playerTeam != PlayerTeam.Red) {
                break;
            }
            meeplesAmountText[i].text = $"{GameManager.Instance.actualPlayer.currentMeeples[i]}/{GameManager.Instance.actualPlayer.GetMaxMeepleAmount(i)}";

        }
        //meeplesAmountText[0].text = blackMeeples.ToString();
        //meeplesAmountText[1].text = blueMeeples.ToString();
        //meeplesAmountText[2].text = purpleMeeples.ToString();
        //if (GameManager.Instance.actualPlayer.playerTeam == PlayerTeam.Red)
        //    meeplesAmountText[3].text = colorlessMeeples.ToString();
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
            GameManager.Instance.actualPlayer.SpendMeepleWithButton(index);
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

    public void ToggleStartScreen(bool enable) {
        startScreen.SetActive(enable);
    }
    public void ToggleEndPhaseButton(bool enable) {
        mEndPhaseButton.GetComponent<Button>().interactable = enable;
        mEndPhaseButton.GetComponent<Image>().sprite = PhaseButtonSprites[enable ? 0 : 1];
        //mEndPhaseButton.SetActive(enable);
    }
    public void ToggleOvertimeButton(bool enable) {
        if (!overTimeButton.activeSelf) return;
        overTimeButton.GetComponent<Button>().interactable = enable;
        overTimeButton.GetComponent<Image>().sprite = PhaseButtonSprites[enable ? 0 : 1];
    }
    public void DisableOverTimeButtonForRed() {
        overTimeButton.SetActive(false);
    }
    //adds the string message to the game log
    //if the message is from the network, or this is the server, it will be added to the game log
    //otherwise its sent to the server and returned to ensure log consistency
    public void AddActionLogMessage(string message, bool fromNet, bool IsServer, ref List<string> messageLog) {
        if (fromNet || IsServer) {
            Instantiate(gameLogMessagePrefab, gameLogParent).GetComponent<TextMeshProUGUI>().text = message;
            Debug.Log($"Created log message: {message}");
            messageLog.Add(message);
            if (IsServer) {
                RGNetworkPlayerList.instance.SendStringToClients(message);
            }
        }
        else {
            RGNetworkPlayerList.instance.SendStringToServer(message);
        }

    }
    public void UpdateUISizeTrackers() {
        playerDeckText.text = GameManager.Instance.actualPlayer.DeckIDs.Count.ToString();
        playerHandText.text = GameManager.Instance.actualPlayer.HandCards.Count.ToString();
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
        if (player == GameManager.Instance.actualPlayer) {
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
    public void SetInfoBackground(int index) {
        if (index >= 0 && index < infoBackgrounds.Length)
            infoBg.sprite = infoBackgrounds[index];
    }
    public void SetDoomClockActive(bool active, int index = 0) {
        doomClockBg.SetActive(active);
        if (active) {
            doomClockImage.sprite = doomClockSprites[index];
        }
    }
    public void UpdateDoomClockTurnsLeft(int turnsLeft) {
        if (turnsLeft <= 0 || turnsLeft >= doomClockSprites.Length) {
            Debug.LogWarning("Invalid doom clock index: " + turnsLeft);
            SetDoomClockActive(false);
            return;
        }
        doomClockImage.sprite = doomClockSprites[^turnsLeft];
    }
    public void SpawnPlayerMenuItem(CardPlayer player) {
        var newPlayerMenu = Instantiate(playerMenuPrefab, playerMenuContainer).GetComponent<PlayerPopupMenuItem>();
        newPlayerMenu.SetPlayer(player);
        playerMenuItems.Add(newPlayerMenu);
    }
    public void UpdatePlayerMenuItems() {
        playerMenuItems.ForEach(item => item.UpdatePopup());
    }
    public void UpdatePlayerMenuItem(CardPlayer player) {
        var item = playerMenuItems.Find(item => item.Player == player);
        if (item != null) {
            item.UpdatePopup();
        }
    }

    public void TogglePlayerMenu(bool enable) {
        playerMenuParent.gameObject.SetActive(enable);
    }
    public void ToggleDownedSectorInMenu(int sectorIndex, bool enable) {
        if (sectorIndex < 0 || sectorIndex >= sectorXIcons.Count) {
            Debug.Log($"Invalid sector index: {sectorIndex}");
            return;
        }

        sectorXIcons[sectorIndex].color = new Color(255, 255, 255, enable ? 1 : 0);

    }
    public void AddPlayerToLobby(string name, PlayerTeam team) {
        playerLobbyManager.AddPlayer(name, team);
    }
    public void ChangePlayerTeam(string name, PlayerTeam team) {
        playerLobbyManager.ChangePlayerTeam(name, team);
    }
    public void UpdateOTText() {
        var max = GameManager.Instance.actualPlayer.otState == OverTimeState.Overtime ?
            GameManager.OVERTIME_DURATION : GameManager.EXHAUSTED_DURATION;

        overTimeTurnsLeftText.text = $"{max - GameManager.Instance.actualPlayer.OverTimeCounter}";
    }
    public void StartOvertime() {
        overTimeTurnsLabelText.text = "OT Turns Left";
        overTimeChargesText.text = GameManager.Instance.actualPlayer.overTimeCharges.ToString();
        overTimeTurnsLeftText.text = $"{GameManager.OVERTIME_DURATION}";
    }
    public void StartExhaustion() {
        overTimeTurnsLabelText.text = "Exhausted Left";
        // overTimeChargesText.text = GameManager.Instance.actualPlayer.overTimeCharges.ToString();
        overTimeTurnsLeftText.text = $"{GameManager.EXHAUSTED_DURATION}";
    }
    public void ToggleMeepleSharingMenu(bool enable) {
        if (enable) {
            for (int i = 0; i < 3; i++) {
                MeepleSharingTextMax[i].text = GameManager.Instance.actualPlayer.GetMaxMeepleAmount(i).ToString();
                MeepleSharingTextMax[i].text = GameManager.Instance.actualPlayer.currentMeeples[i].ToString();
            }
            //MeepleSharingTextCurrent[1].text = GameManager.Instance.actualPlayer.BlueMeeples.ToString();
            //MeepleSharingTextCurrent[0].text = GameManager.Instance.actualPlayer.BlackMeeples.ToString();
            //MeepleSharingTextCurrent[2].text = GameManager.Instance.actualPlayer.PurpleMeeples.ToString();
        }
        MeepleSharingPanel.SetActive(enable);
    }
    public void ShowAllySelectionMenu(int meepleTypeIndex) {
        Debug.Log($"Enabling ally selection menu after pressing meeple button {meepleTypeIndex}");
        allySelectionButtons.ForEach(button => Destroy(button.gameObject));

        for (int i = 0; i < GameManager.Instance.playerDictionary.Count; i++) {
            var cardPlayer = GameManager.Instance.playerDictionary.ElementAt(i).Value;
            if (cardPlayer.playerTeam == PlayerTeam.Red) continue;
            if (cardPlayer.NetID == RGNetworkPlayerList.instance.localPlayerID) continue;
            var parent = i < 8 ? leftSelectionparent : rightSelectionparent;
            var newButton = Instantiate(allySelectionButtonPrefab, parent).GetComponent<Button>();
            newButton.onClick.AddListener(() => GameManager.Instance.HandleChoosePlayerToShareWithButtonPress(meepleTypeIndex, cardPlayer.NetID));
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = cardPlayer.playerName;
            allySelectionButtons.Add(newButton);
        }
        allySelectionMenu.SetActive(true);
    }
    public void DisableAllySelectionMenu() {
        allySelectionMenu.SetActive(false);
        Debug.Log($"Disabling Ally Selection after {GameManager.Instance.actualPlayer.playerName} " +
            $"has shared {GameManager.Instance.actualPlayer.LentMeepleAmount} meeples");
        if (GameManager.Instance.actualPlayer.LentMeepleAmount < 2) {
            ToggleMeepleSharingMenu(true);
        }
        else {
            ToggleMeepleSharingMenu(false);
        }
    }
    public void UpdateMeepleSharingMenu() {
        for (int i = 0; i < 3; i++) {
            MeepleSharingTextMax[i].text = GameManager.Instance.actualPlayer.GetMaxMeepleAmount(i).ToString();
            MeepleSharingTextCurrent[i].text = GameManager.Instance.actualPlayer.currentMeeples[i].ToString();
        }
    }

}
