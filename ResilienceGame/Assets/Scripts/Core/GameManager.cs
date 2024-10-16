using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Yarn.Unity;
using UnityEngine.InputSystem;
using System.IO;
using System;

public class GameManager : MonoBehaviour, IRGObservable {

    #region fields
    // Static Members
    public static GameManager Instance;
    private static bool hasStartedAlready = false;
    //  public static event Action OnRoundEnd;

    // Debug
    public bool DEBUG_ENABLED = true;

    // Game State
    [Header("Game State")]
    public GamePhase MGamePhase = GamePhase.Start;
    private GamePhase mPreviousGamePhase = GamePhase.Start;
    public bool gameStarted = false;
    private bool mStartGameRun = false;
    private bool isInit = false;
    private bool mReceivedEndGame = false;

    // Players and Teams
    [Header("Players and Teams")]
    public PlayerTeam playerType = PlayerTeam.Any;
    public PlayerTeam opponentType = PlayerTeam.Any;
    public CardPlayer actualPlayer;
    public List<CardPlayer> networkPlayers = new List<CardPlayer>();
    public CardPlayer opponentPlayer;
    private bool myTurn = false;
    public int activePlayerNumber;
   // [SerializeField] private List<CardPlayer> playerList;
    public Dictionary<int, CardPlayer> playerDictionary = new Dictionary<int, CardPlayer>();

    [Header("Sectors")]
    [SerializeField] List<Sector> activeSectors;
    public readonly Dictionary<SectorType, Sector> AllSectors = new Dictionary<SectorType, Sector>();
    public List<Sector> AssignableSectors { get; private set; }
    public Sector sectorInView;
    int sectorIndex = -1;

    // Decks and Cards
    [Header("Decks and Cards")]
    public CardReader redDeckReader;
    public CardReader blueDeckReader;
    public CardReader positiveWhiteDeckReader;
    public CardReader negativeWhiteDeckReader;
    public List<Card> redCards;
    public List<Card> blueCards;
    public List<Card> positiveWhiteCards;
    public List<Card> negativeWhiteCards;
    public GameObject playerDeckList;
    private TMPro.TMP_Dropdown playerDeckChoice;

    // Game Rules
    [Header("Game Rules")]
    public readonly int MAX_DISCARDS = 3;
    public readonly int MAX_DEFENSE = 1;
    public int MNumberDiscarded { get; private set; } = 0;
    private int mNumberDefense = 0;
    public bool MIsDiscardAllowed { get; private set; } = false;
    private bool mIsActionAllowed = false;
    private bool isDoomClockActive = false;


    // UI Elements
    [Header("UI Elements")]
    //public UserInterface userInterface;
    //public GameObject gameCanvas;
    //public GameObject startScreen;
    //public GameObject alertScreenParent;
    //public GameObject tiles;
    //public TextMeshProUGUI StatusText;
    //public TextMeshProUGUI mTurnText;
    //public TextMeshProUGUI mPhaseText;
    //public TextMeshProUGUI mPlayerName;
    //public TextMeshProUGUI mPlayerDeckType;
    //public TextMeshProUGUI mOpponentName;
    //public TextMeshProUGUI mOpponentDeckType;
    //public TextMeshProUGUI activePlayerText;
    //public Color activePlayerColor;
    //public GameObject mEndPhaseButton;
    //public GameObject opponentPlayedZone;
    //public Camera cam;
    //public TextMeshProUGUI titlee;
    //public AlertPanel mAlertPanel;
    //public DeckSizeTracker deckSizeTracker;
    //[SerializeField] private GameObject gameLogMessagePrefab;
    //[SerializeField] private Transform gameLogParent;


    // End Game
    [Header("End Game")]
    public GameObject endGameCanvas;
    public TMP_Text endGameText;

    // Tutorial
    [Header("Tutorial")]
    public GameObject yarnSpinner;
    private DialogueRunner runner;
    private GameObject background;
    private bool skip;
    private bool skipClicked;

    // Networking
    [Header("Networking")]
    private RGNetworkPlayerList mRGNetworkPlayerList;
    public bool IsServer = true;
    private MessageQueue mMessageQueue = new MessageQueue();
    // Observers
    private List<IRGObserver> mObservers = new List<IRGObserver>(20);

    // Logging
    public List<string> messageLog = new List<string>();


    // Misc
    public bool mCreateEnergyAtlas = false;
    public bool mCreateWaterAtlas = false;
    public bool mCreatePosWhiteAtlas = false;
    public bool mCreateNegWhiteAtlas = false;
    private int roundsLeft = 30;
    private int turnTotal = 0;
    private const int BASE_MAX_TURNS = 30;
    private int numTurnsTillWhiteCard = 0;
    private int numWhiteCardOfSameTypePlayed = 0;
    private const int MIN_TURNS_TILL_WHITE_CARD = 2;
    private const int MAX_TURNS_TILL_WHITE_CARD = 5;
    private const float WHITE_CARD_POS_CHANCE = 0.5f;
    private bool playWhite = false;
    private bool playedPosWhiteCard = false;
    private bool hasWhiteCardPlayed = true; //TODO change when implementing white cards

    public int UniqueCardIdCount = 0;

    public int UniqueFacilityEffectIdCount { get; set; }


    #endregion

    #region Initialization
    // Called by dropdown list box to set up the player type
    public void SetPlayerType() {
        if (playerDeckChoice == null) {
            playerDeckChoice = playerDeckList.GetComponent<TMPro.TMP_Dropdown>();
            if (playerDeckChoice == null) {
                Debug.Log("deck choice is null!");
            }
        }

        if (playerDeckChoice != null) {
            // set this player's type
            switch (playerDeckChoice.value) {
                case 0:
                    playerType = PlayerTeam.Red;
                    break;
                case 1:
                    playerType = PlayerTeam.Blue;
                    break;
                default:
                    break;
            }

            // display player type on view???
            Debug.Log("player type set to be " + playerType);
        }

    }
    // Called when pressing the button to start
    // Doesn't actually start the game until ALL
    // the players connected have pressed their start buttons.
    public void StartGame() {
        if (!mStartGameRun) {
            Debug.Log("running start of game");

            AssignableSectors = new List<Sector>(activeSectors);
            activeSectors.ForEach(sector => AllSectors.Add(sector.sectorName, sector)); //store for O(1) access

            // basic init of player
            SetPlayerType();
            SetupActors();
            UserInterface.Instance.StartGame(playerType);
            // init various objects to be used in the game
            //gameCanvas.SetActive(true);
            // alertScreenParent.SetActive(false); //Turn off the alert (selection) screen
            roundsLeft = BASE_MAX_TURNS;
            turnTotal = 0;
            numTurnsTillWhiteCard = UnityEngine.Random.Range(MIN_TURNS_TILL_WHITE_CARD, MAX_TURNS_TILL_WHITE_CARD); //2-5 turns
                                                                                                                    // mTurnText.text = "" + GetTurnsLeft();
                                                                                                                    // mPhaseText.text = "Phase: " + MGamePhase.ToString();
                                                                                                                    //   mPlayerName.text = RGNetworkPlayerList.instance.localPlayerName;
            actualPlayer.playerName = RGNetworkPlayerList.instance.localPlayerName;
            //   mPlayerDeckType.text = "" + playerType;



            // tell everybody else of this player's type
            if (!IsServer) {
                Message msg;
                List<int> tmpList = new List<int>(1);
                tmpList.Add((int)playerType);
                msg = new Message(CardMessageType.SharePlayerType, tmpList);
                AddMessage(msg);
            }
            else {
                RGNetworkPlayerList.instance.SetPlayerType(playerType);
            }

        }
        mStartGameRun = true;
        Debug.Log("start game set!");
    }

    public void RealGameStart() {
        Debug.Log("running 2nd start of game");
        //gameCanvas.SetActive(true);
        UserInterface.Instance.ToggleGameCanvas(true);
        // send out the starting message with all player info
        // and start the next phase
        if (IsServer) {
            Message msg = RGNetworkPlayerList.instance.CreateStartGameMessage();
            AddMessage(msg);
        }

        // if it's a network rejoin we already have our facility
        /*if (actualPlayer.ActiveFacilities.Count==0 )
        {
            // draw our first 2 pt facility
            Card card = actualPlayer.DrawFacility(false, 2);
            // send message about what facility got drawn
            if (card != null)
            {
                AddMessage(new Message(CardMessageType.SendPlayedFacility, card.UniqueID, card.data.cardID));
            }
            else
            {
                Debug.Log("problem in drawing first facility as it's null!");
            }
        }*/


        // make sure to show all our cards
        foreach (GameObject gameObjectCard in actualPlayer.HandCards.Values) {
            gameObjectCard.SetActive(true);
        }

        // set up the opponent name text
        if (RGNetworkPlayerList.instance.playerIDs.Count > 0) {
            Debug.Log("player ids greater than zero for realstart");
            if (RGNetworkPlayerList.instance.localPlayerID == 0) {
                UserInterface.Instance.SetOpponentNameAndDeckText(
                    RGNetworkPlayerList.instance.playerNames[1],
                    RGNetworkPlayerList.instance.playerTypes[1].ToString());
                //mOpponentName.text = RGNetworkPlayerList.instance.playerNames[1];
                //mOpponentDeckType.text = "" + RGNetworkPlayerList.instance.playerTypes[1];
                opponentType = RGNetworkPlayerList.instance.playerTypes[1];
                opponentPlayer.playerName = RGNetworkPlayerList.instance.playerNames[1];
            }
            else {
                UserInterface.Instance.SetOpponentNameAndDeckText(
                    RGNetworkPlayerList.instance.playerNames[0],
                    RGNetworkPlayerList.instance.playerTypes[0].ToString());
                //mOpponentName.text = RGNetworkPlayerList.instance.playerNames[0];
                opponentPlayer.playerName = RGNetworkPlayerList.instance.playerNames[0];
                // mOpponentDeckType.text = "" + RGNetworkPlayerList.instance.playerTypes[0];
                opponentType = RGNetworkPlayerList.instance.playerTypes[0];
            }
            // TODO: Probably needs rewrite when more players added
            if (opponentType == PlayerTeam.Red) {
                //opponentPlayer = energyPlayer;
                opponentPlayer.playerTeam = PlayerTeam.Red;
                opponentPlayer.DeckName = "red";
            }
            else {
                //opponentPlayer = waterPlayer;
                opponentPlayer.playerTeam = PlayerTeam.Blue;
                opponentPlayer.DeckName = "blue";
            }
            opponentPlayer.InitializeCards();
        }
        activeSectors.ForEach(sector => {
            sector.Initialize();
            sector.ToggleSectorVisuals(false);
        });

        if (IsServer) {
            // Select and assign sector to both players
            Sector sector = AssignableSectors[UnityEngine.Random.Range(0, AssignableSectors.Count)];
            int sectorIndex = AssignableSectors.IndexOf(sector);

            // Assign sector ownership to the players
            sector.SetOwner(actualPlayer.playerTeam == PlayerTeam.Blue ? actualPlayer : opponentPlayer);
            actualPlayer.AssignSector(sector);
            opponentPlayer.AssignSector(sector);
            //sectorInView = sector;

            // Remove the sector from the list and activate it
            AssignableSectors.Remove(sector);
            SetSectorInView(sector);

            // Create a message to send the sector assignment to all clients
            Message sectorMsg = new Message(CardMessageType.SectorAssignment, new List<int> { sectorIndex });
            AddMessage(sectorMsg);
        }

        // Debug.Log($"actual player game object: {actualPlayer.gameObject.name}");
        //  Debug.Log($"opponent player game object: {opponentPlayer.gameObject.name}");



        //sector.Initialize();

        // in this game people go in parallel to each other
        // per phase
        myTurn = true;
        gameStarted = true;

        // go on to the next phase
        // MGamePhase = GamePhase.DrawRed;
        StartNextPhase();
        UserInterface.Instance.ToggleStartScreen(false);
        //startScreen.SetActive(false);

    }
    public void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        mStartGameRun = false;
        Debug.Log("start run on GameManager");
        if (!hasStartedAlready) {
            //startScreen.SetActive(true);
            UserInterface.Instance.ToggleStartScreen(true);

            //TODO: Read based on number of players/selection

            // read water deck
            CardReader reader = blueDeckReader.GetComponent<CardReader>();
            if (reader != null) {
                // TODO: Set with csv
                blueCards = reader.CSVRead(mCreateWaterAtlas); // TODO: Remove var, single atlas
                CardPlayer.AddCards(blueCards);
                //waterPlayer.playerTeam = PlayerTeam.Blue;
                //waterPlayer.DeckName = "blue";
                //Debug.Log("number of cards in all cards is: " + CardPlayer.cards.Count);
            }
            else {
                Debug.Log("Blue deck reader is null.");
            }


            // TODO: Remove, should be selected by csv
            // read energy deck
            reader = redDeckReader.GetComponent<CardReader>();
            if (reader != null) {
                redCards = reader.CSVRead(mCreateEnergyAtlas);
                CardPlayer.AddCards(redCards);
                //energyPlayer.playerTeam = PlayerTeam.Red;
                //energyPlayer.DeckName = "red";
                //   Debug.Log("number of cards in all cards is: " + CardPlayer.cards.Count);

            }
            else {
                Debug.Log("Energy deck reader is null.");
            }

            reader = positiveWhiteDeckReader.GetComponent<CardReader>();
            if (reader != null) {
                positiveWhiteCards = reader.CSVRead(mCreatePosWhiteAtlas);
                CardPlayer.AddCards(positiveWhiteCards);
            }
            else {
                Debug.Log("Positive white deck reader is null.");
            }

            reader = negativeWhiteDeckReader.GetComponent<CardReader>();
            if (reader != null) {
                negativeWhiteCards = reader.CSVRead(mCreateNegWhiteAtlas);
                CardPlayer.AddCards(negativeWhiteCards);
            }
            else {
                Debug.Log("Negative white deck reader is null.");
            }

            // Set dialogue runner for tutorial
            runner = yarnSpinner.GetComponent<DialogueRunner>();
            background = yarnSpinner.transform.GetChild(0).GetChild(0).gameObject;
            //Debug.Log(background);
            hasStartedAlready = true;
        }
        else {
            Debug.Log("start is being run multiple times!");
        }

    }

    // Set up the main player of the game

    public void SetupActors() {
        // we should know when choice they
        // wanted by now and can set up
        // appropriate values

        // TODO: Change PlayerType
        if (playerType == PlayerTeam.Red) {
            //actualPlayer = energyPlayer;
            actualPlayer.playerTeam = PlayerTeam.Red;
            actualPlayer.DeckName = "red";

        }
        else if (playerType == PlayerTeam.Blue) {
            //actualPlayer = waterPlayer;
            actualPlayer.playerTeam = PlayerTeam.Blue;
            actualPlayer.DeckName = "blue";

            //// TODO: Set randomly
            //actualPlayer.playerSector = gameCanvas.GetComponentInChildren<Sector>();
            //actualPlayer.playerSector.Initialize(PlayerSector.Water);
        }

        // Initialize the deck info and set various
        // player zones active
        actualPlayer.InitializeCards();
        UserInterface.Instance.discardDropZone.SetActive(true);
        UserInterface.Instance.handDropZone.SetActive(true);

    }

    #endregion

    #region Update
    // Update is called once per frame
    void Update() {
        if (DEBUG_ENABLED) {
            if (Keyboard.current.f1Key.wasPressedThisFrame) {
                Debug.Log($"{(IsServer ? "**[SERVER]**" : "**[CLIENT]**")}");
                actualPlayer.LogPlayerInfo();
                opponentPlayer.LogPlayerInfo();
            }
            if (Keyboard.current.mKey.wasPressedThisFrame) {
                AddActionLogMessage("Test message", IsServer);
                if (IsServer) {
                    mRGNetworkPlayerList.SendStringToClients("Test message");
                }

            }



        }
        if (isInit) {
            if (gameStarted) {
                HandlePhases(MGamePhase);
            }

            // always notify observers in case there's a message
            // waiting to be processed.
            NotifyObservers();

        }
        else {
            // the network takes a while to start up and 
            // we wait for it.
            mRGNetworkPlayerList = RGNetworkPlayerList.instance;
            if (mRGNetworkPlayerList != null) {
                // means network init is done
                // and we're joined
                RegisterObserver(mRGNetworkPlayerList);
                IsServer = mRGNetworkPlayerList.isServer;
                CardPlayer player = GameObject.FindObjectOfType<CardPlayer>();
                if (player != null) {
                    // player is initialized and ready to go
                    // this follows the network init and also
                    // takes a while to happen
                    isInit = true;
                }
            }
        }

    }



    #endregion

    #region Interface Updates
    public void AddActionLogMessage(string message, bool fromNet) {
        UserInterface.Instance.AddActionLogMessage(message, fromNet, IsServer, ref messageLog);
    }
    public void SetSectorInView(Sector sector) {
        Debug.Log("setting sector in view to " + sector?.sectorName);
        if (sector != null)
            SetSectorInView(activeSectors.IndexOf(sector));
    }
    public void SetSectorInView(int index) {
        Debug.Log("setting sector in view to " + index);
        if (index >= 0 && index < activeSectors.Count) {
            if (sectorInView != null)
                sectorInView.ToggleSectorVisuals(false);
            sectorInView = activeSectors[index];
            sectorInView.ToggleSectorVisuals(true);
            sectorIndex = index;
        }
    }
    public void ViewPreviousSector() {
        sectorIndex = sectorIndex - 1 > 0 ? sectorIndex - 1 : activeSectors.Count - 1;
        SetSectorInView(sectorIndex);
    }
    public void ViewNextSector() {
        sectorIndex = sectorIndex + 1 < activeSectors.Count ? sectorIndex + 1 : 0;
        SetSectorInView(sectorIndex);
    }


    //public void UpdateUISizeTrackers() {
    //    //TODO: Add check for all red players
    //    deckSizeTracker.UpdateAllTrackerTexts(
    //        playerDeckSize: actualPlayer.DeckIDs.Count,
    //        playerHandSize: actualPlayer.HandCards.Count,
    //        opponentDeckSize: opponentPlayer.DeckIDs.Count,
    //        opponentHandSize: opponentPlayer.HandCards.Count);
    //}
    // WORK: there is no menu?????
    public void BackToMenu() {

        if (NetworkServer.active && NetworkClient.isConnected) {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkServer.active) {
            NetworkManager.singleton.StopServer();
        }
        else if (NetworkClient.isConnected) {
            NetworkManager.singleton.StopClient();
        }
        Destroy(RGNetworkManager.singleton.gameObject);
        // SceneManager.LoadScene(0);

    }


    //// display info about the game's status on the screen
    //public void DisplayGameStatus(string message) {
    //    StatusText.text = message;
    //}

    //public void DisplayAlertMessage(string message, CardPlayer player, int duration = -1, Action onAlertFinish = null) {
    //    if (player == actualPlayer) {
    //        if (onAlertFinish == null)
    //            mAlertPanel.ShowTextAlert(message, duration);
    //        else
    //            mAlertPanel.ShowTextAlert(message, onAlertFinish);
    //    }
    //}

    //public void DisplayCardChoiceMenu(Card card, int numRequired) {
    //    mAlertPanel.AddCardToSelectionMenu(card.gameObject);
    //    if (numRequired < 3)
    //        mAlertPanel.ToggleCardSelectionPanel(true);

    //}
    // WORK: rewrite for this card game
    public void ShowEndGameCanvas() {
        MGamePhase = GamePhase.End;
        endGameCanvas.SetActive(true);
        endGameText.text = actualPlayer.playerName + " ends the game with score " + actualPlayer.GetScore() +
            " and " + opponentPlayer.playerName + " ends the game with score " + opponentPlayer.GetScore();

        //WriteListToFile(Path.Combine(Application.streamingAssetsPath, "messages.log"), messageLog);
    }
    #endregion

    #region Helpers
    public void AllowPlayerDiscard(CardPlayer player, int amount, List<Card> cardsAllowedToDiscard = null) {
        if (actualPlayer == player) {
            Debug.Log($"Player {player.playerName} must discard {amount} cards");
            MIsDiscardAllowed = true;
            player.AddDiscardEvent(amount, cardsAllowedToDiscard);
        }
    }
    public void DisablePlayerDiscard(CardPlayer player) {
        if (actualPlayer == player) {
            MIsDiscardAllowed = false;
            player.StopDiscard();
            UserInterface.Instance.UpdateUISizeTrackers();
        }
    }
    public bool CanHighlight() {
        if (actualPlayer.playerTeam == PlayerTeam.Red && MGamePhase == GamePhase.ActionRed) {
            return actualPlayer.ReadyState == CardPlayer.PlayerReadyState.ReadyToPlay;
        }
        if (actualPlayer.playerTeam == PlayerTeam.Blue && MGamePhase == GamePhase.ActionBlue) {
            return actualPlayer.ReadyState == CardPlayer.PlayerReadyState.ReadyToPlay;
        }
        return false;

    }
    public bool IsActualPlayersTurn() {
        if (actualPlayer.playerTeam == PlayerTeam.Red) {
            return MGamePhase == GamePhase.DrawRed || MGamePhase == GamePhase.ActionRed || MGamePhase == GamePhase.BonusRed || MGamePhase == GamePhase.DiscardRed;
        }
        else {
            return MGamePhase == GamePhase.DrawBlue || MGamePhase == GamePhase.ActionBlue || MGamePhase == GamePhase.BonusBlue || MGamePhase == GamePhase.DiscardBlue;
        }

    }
    public void WriteListToFile(string filePath, List<string> stringList) {
        // Ensure the directory exists
        string directory = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(directory)) {
            Directory.CreateDirectory(directory);
        }

        // Write all the lines to the file
        File.WriteAllLines(filePath, stringList);

        Debug.Log("File written successfully to: " + filePath);
    }

    public bool HasReceivedEndGame() {
        return mReceivedEndGame;
    }

    public void SetReceivedEndGame(bool value) {
        mReceivedEndGame = value;
    }
    #endregion

    #region Phase Handling
    void ProgressPhase() {
        var curPhase = MGamePhase;

        MGamePhase = GetNextPhase();
        Debug.Log($"Progressing phase {curPhase} to {MGamePhase}");
        if (IsActualPlayersTurn()) {
            if (!(MGamePhase == GamePhase.DiscardBlue || MGamePhase == GamePhase.DiscardRed)) {
                //mEndPhaseButton.SetActive(true);
                UserInterface.Instance.ToggleEndPhaseButton(true);
                Debug.Log($"{actualPlayer.playerTeam}'s end phase button set active");
            }
        }
        else {
            EndPhase(); // end the phase if it isn't your turn, to automatically go to the next phase, still requires the player who's turn it is to end their phase
            Debug.Log("Auto ending phase for " + actualPlayer.playerTeam);
        }
    }
    // Starts the next phase.
    public void StartNextPhase() {

        if (MGamePhase == GamePhase.Start) {
            ProgressPhase();
        }
        else {
            if (!myTurn) {
                myTurn = true;
                ProgressPhase();

            }
        }

    }
    // Gets the next phase.
    public GamePhase GetNextPhase() {
        return MGamePhase switch {
            GamePhase.Start => GamePhase.DrawRed,
            GamePhase.DrawRed => isDoomClockActive ? GamePhase.BonusRed : GamePhase.ActionRed,
            GamePhase.BonusRed => GamePhase.ActionRed,
            GamePhase.ActionRed => (roundsLeft == 0 ? GamePhase.End : GamePhase.DiscardRed), //end game after red action if turn counter is 0
            GamePhase.DiscardRed => playWhite ? GamePhase.DrawBlue : GamePhase.PlayWhite,
            GamePhase.PlayWhite => GamePhase.DrawBlue,
            GamePhase.DrawBlue => isDoomClockActive ? GamePhase.BonusBlue : GamePhase.ActionBlue,
            GamePhase.BonusBlue => GamePhase.ActionBlue,
            GamePhase.ActionBlue => GamePhase.DiscardBlue,
            GamePhase.DiscardBlue => (actualPlayer.DeckIDs.Count == 0 || actualPlayer.ActiveFacilities.Count == 0) ? GamePhase.End : GamePhase.DrawRed,
            _ => GamePhase.End
        };
    }
    // Handle all the card game phases with
    // this simple state machine.
    public void HandlePhases(GamePhase phase) {
        // TODO: Implement team turns

        // keep track of 
        bool phaseJustChanged = false;
        MGamePhase = phase;
        if (!MGamePhase.Equals(mPreviousGamePhase)) {
            phaseJustChanged = true;
            UserInterface.Instance.SetPhaseText(MGamePhase.ToString());
            //mPhaseText.text = MGamePhase.ToString();
            mPreviousGamePhase = phase;
            //SkipTutorial();
        }

        switch (phase) {
            case GamePhase.Start:
                // start of game phase
                // handled with specialty code outside of this
                break;
            case GamePhase.DrawRed:
            case GamePhase.DrawBlue:


                if (phaseJustChanged) {
                    //reset player discard amounts

                    MIsDiscardAllowed = true;
                    // draw cards if necessary
                    if (IsActualPlayersTurn()) {
                        actualPlayer.DrawCardsToFillHand();
                        // set the discard area to work if necessary
                        UserInterface.Instance.discardDropZone.SetActive(true);

                        MNumberDiscarded = 0;
                    }
                    UserInterface.Instance.DisplayGameStatus("[TEAM COLOR] has drawn " + actualPlayer.HandCards.Count + " cards each.");
                }
                else {
                    // draw cards if necessary
                    if (IsActualPlayersTurn())
                        actualPlayer.DrawCardsToFillHand();

                    // check for discard and if there's a discard draw again
                    if (MNumberDiscarded == MAX_DISCARDS) {
                        UserInterface.Instance.DisplayGameStatus(actualPlayer.playerName + " has reached the maximum discard number. Please hit end phase to continue.");
                    }
                    else {
                        if (MIsDiscardAllowed) {
                            MNumberDiscarded += actualPlayer.HandlePlayCard(MGamePhase, opponentPlayer);
                        }
                    }
                }
                break;
            case GamePhase.BonusRed:
            case GamePhase.BonusBlue:
                break;
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                if (!phaseJustChanged) {
                    if (!mIsActionAllowed) {
                        // do nothing - most common scenario
                    }
                    else if (actualPlayer.GetMeeplesSpent() >= actualPlayer.GetMaxMeeples()) {
                        actualPlayer.HandlePlayCard(MGamePhase, opponentPlayer); //still need to resolve the card played that spend the final meeples
                        Debug.Log($"Spent: {actualPlayer.GetMeeplesSpent()}/{actualPlayer.GetMaxMeeples()}");
                        mIsActionAllowed = false;
                        UserInterface.Instance.DisplayGameStatus(actualPlayer.playerName + " has spent their meeples. Please push End Phase to continue.");
                    }
                    else {
                        actualPlayer.HandlePlayCard(MGamePhase, opponentPlayer);
                    }
                }
                else if (phaseJustChanged) {
                    mIsActionAllowed = true;
                    actualPlayer.InformSectorOfNewTurn();
                    if (IsActualPlayersTurn()) {
                        actualPlayer.ResetMeeplesSpent();
                    }
                    else {
                        opponentPlayer.ResetMeeplesSpent();
                    }
                    //opponentPlayer.InformSectorOfNewTurn();
                }

                break;
            case GamePhase.DiscardRed:
            case GamePhase.DiscardBlue:
                if (phaseJustChanged) {
                    if (!actualPlayer.NeedsToDiscard()) {
                        EndPhase(); //immediately end phase if no discards needed
                        return;
                    }
                    //reset player discard amounts
                    MIsDiscardAllowed = true;
                    Debug.Log($"setting discard drop active");
                    UserInterface.Instance.discardDropZone.SetActive(true);
                    MNumberDiscarded = 0;
                    UserInterface.Instance.DisplayGameStatus(actualPlayer.playerName + " has " + actualPlayer.HandCards.Count + " cards in hand.");
                    UserInterface.Instance.DisplayAlertMessage($"You must discard {actualPlayer.HandCards.Count - CardPlayer.MAX_HAND_SIZE_AFTER_ACTION} cards before continuing", actualPlayer);
                }
                else {

                    if (MIsDiscardAllowed) {
                        MNumberDiscarded += actualPlayer.HandlePlayCard(MGamePhase, opponentPlayer);

                        if (!actualPlayer.NeedsToDiscard()) {
                            Debug.Log("Ending discard phase after finishing discarding");
                            MIsDiscardAllowed = false;
                            EndPhase(); //end phase when done discarding
                        }
                        //update alert when discarding
                        if (MNumberDiscarded > 0) {
                            UserInterface.Instance.DisplayAlertMessage($"You must discard {actualPlayer.HandCards.Count - CardPlayer.MAX_HAND_SIZE_AFTER_ACTION} cards before continuing", actualPlayer);
                        }
                    }
                }
                break;
            case GamePhase.PlayWhite:
                void PlayPos() {
                    int randCard = UnityEngine.Random.Range(0, positiveWhiteCards.Count - 1);
                    Debug.Log("Playing positive white card on turn " + turnTotal);
                    hasWhiteCardPlayed = true;
                    //TODO: Play the card
                    //positiveWhiteCards[randCard].Play(null, null);
                    //  positiveWhiteCards.RemoveAt(randCard);
                }

                void PlayNeg() {
                    Debug.Log("Playing negative white card on turn " + turnTotal);
                    int randCard = UnityEngine.Random.Range(0, negativeWhiteCards.Count - 1);
                    hasWhiteCardPlayed = true;
                    //TODO: Play the card
                    //  negativeWhiteCards[randCard].Play(null, null);
                    //  negativeWhiteCards.RemoveAt(randCard);
                }

                if (!hasWhiteCardPlayed) {


                    if (UnityEngine.Random.Range(0f, 1f) > WHITE_CARD_POS_CHANCE) {

                        if (numWhiteCardOfSameTypePlayed >= 2) {
                            PlayNeg();
                            playedPosWhiteCard = false;
                            numWhiteCardOfSameTypePlayed = 0;
                        }
                        if (playedPosWhiteCard)
                            numWhiteCardOfSameTypePlayed++;

                        playedPosWhiteCard = true;
                        PlayPos();
                    }
                    else {
                        if (numWhiteCardOfSameTypePlayed >= 2) {
                            PlayPos();
                            playedPosWhiteCard = true;
                            numWhiteCardOfSameTypePlayed = 0;
                        }
                        if (!playedPosWhiteCard)
                            numWhiteCardOfSameTypePlayed++;

                        playedPosWhiteCard = false;
                        PlayNeg();
                    }
                }
                break;
            case GamePhase.End:
                // end of game phase
                if (phaseJustChanged) {
                    Debug.Log("end game has happened. Sending message to other player.");
                    int playerScore = actualPlayer.GetScore();
                    AddMessage(new Message(CardMessageType.EndGame));
                }
                break;
            default:
                break;
        }
    }
    // Ends the phase.
    public void EndPhase() {
        UserInterface.Instance.ResolveTextAlert(); //resolve any alerts, there currently should not be alerts that persist to the next phase so we can auto hide any leftover alerts here
        switch (MGamePhase) {
            case GamePhase.DiscardRed:
            case GamePhase.DiscardBlue:
            case GamePhase.DrawBlue:
            case GamePhase.DrawRed: {
                    // make sure we have a full hand
                    // actualPlayer.DrawCards();
                    // set the discard area to work if necessary
                    UserInterface.Instance.discardDropZone.SetActive(false);
                    MIsDiscardAllowed = false;

                    // clear any remaining drops since we're ending the phase now (dont think this is needed anymore)
                    //actualPlayer.ClearDropState();

                    // Debug.Log("ending draw and discard game phase!");

                    // send a message with number of discards of the player
                    Message msg;
                    List<int> tmpList = new List<int>(1);
                    tmpList.Add(MNumberDiscarded);
                    msg = new Message(CardMessageType.ShareDiscardNumber, tmpList);
                    AddMessage(msg);
                }
                break;
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:

                //SendUpdatesToOpponent(MGamePhase, actualPlayer);
                // reset the defense var's for the next turn
                mIsActionAllowed = false;
                mNumberDefense = 0;

                break;
            case GamePhase.End:
                break;
            default:
                break;
        }

        if (myTurn) {
            Debug.Log("ending the game phase in gamemanager!");
            //HidePlayUI();
            //mEndPhaseButton.SetActive(false);
            UserInterface.Instance.ToggleEndPhaseButton(false);
            AddMessage(new Message(CardMessageType.EndPhase));
            myTurn = false;
        }
    }
    #endregion

    #region Networking
    // Adds a message to the message queue for the network.
    public void AddMessage(Message msg) {
        mMessageQueue.Enqueue(msg);
    }

    // Registers an observer of the message queue.
    public void RegisterObserver(IRGObserver o) {
        if (!mObservers.Exists(x => x == o)) {
            mObservers.Add(o);
        }
    }

    // Registers and observer of the message queue.
    public void RemoveObserver(IRGObserver o) {
        if (mObservers.Exists(x => x == o)) {
            mObservers.Remove(o);
        }
    }

    // Notifies all observers that there is a message.
    public void NotifyObservers() {
        if (!mMessageQueue.IsEmpty()) {
            while (!mMessageQueue.IsEmpty()) {
                Message m = mMessageQueue.Dequeue();
                foreach (IRGObserver o in mObservers) {
                    o.UpdateObserver(m);
                    //messageLog.Add(m.ToString()); not correct spot?
                }
            }
        }
    }
    public void SendUpdatesToOpponent(GamePhase phase, CardPlayer player) {
        while (player.HasUpdates()) {
            // Debug.Log("Checking for updates to send to opponent");
            Message msg;
            List<int> tmpList = new List<int>(4);
            CardMessageType messageType = player.GetNextUpdateInMessageFormat(ref tmpList, phase);
            if (messageType != CardMessageType.None) {
                msg = new Message(messageType, tmpList);
                AddMessage(msg);
            }
        }

    }

    public void AddUpdateFromPlayer(Update update, GamePhase phase, uint playerIndex) {

        //send card updates to the sector the card was played on
        switch (update.Type) {
            case CardMessageType.CardUpdate:
            case CardMessageType.CardUpdateWithExtraFacilityInfo:
                if (AllSectors.TryGetValue(update.sectorPlayedOn, out Sector sector)) {
                    sector.AddUpdateFromPlayer(update, phase, opponentPlayer); //TODO: change to reference a list of players somewhere
                }
                else {
                    Debug.LogWarning($"sector type not found in update not passing to sector");
                }
                break;
            //pass other updates to the card player
            //TODO: list of all players
            default:
                try {
                    actualPlayer.AddUpdateFromOpponent(update, phase, opponentPlayer);
                }
                catch (Exception e) {
                    Debug.LogError("Error in adding update from opponent: " + e.Message);
                }
                break;
        }


    }
    #endregion

    #region Turn Handling

    // Increments a turn. Note that turns consist of multiple phases.
    public void IncrementTurn() {
        // OnRoundEnd?.Invoke(); //inform listeners that the round ended

        actualPlayer.ResetMeepleCount();
        opponentPlayer.ResetMeepleCount();
        turnTotal++;
        roundsLeft--;
        numTurnsTillWhiteCard--;
        if (numTurnsTillWhiteCard == 0) {
            playWhite = true;
            numTurnsTillWhiteCard = UnityEngine.Random.Range(MIN_TURNS_TILL_WHITE_CARD, MAX_TURNS_TILL_WHITE_CARD); //2-5
        }
        // mTurnText.text = "" + GetTurnsLeft();
        UserInterface.Instance.SetTurnText(GetTurnsLeft() + "");
        if (IsServer) {
            Debug.Log("server adding increment turn message");
            AddMessage(new Message(CardMessageType.IncrementTurn));
        }
    }
    // Gets which turn it is.
    public int GetTurnsLeft() {
        return roundsLeft;
    }

    public void ChangeRoundsLeft(int change)
    {
        if (roundsLeft + change < 0)
        {
            roundsLeft = 0;
        }
        else roundsLeft += change;
    }

    #endregion

    #region Tutorial
    //Sets dialogue to inactive
    // TODO: For all tutorial methods, rework to display different text depending on player
    private void SkipTutorial() {
        if (!yarnSpinner.activeInHierarchy) { return; }

        if ((skip && mPreviousGamePhase != GamePhase.Start && MGamePhase == GamePhase.DrawRed)
            || skipClicked) {
            skip = true;
            runner.Stop();
            yarnSpinner.SetActive(false);
            background.SetActive(false);
        }
    }

    public void SkipClick() {
        skipClicked = true;
        SkipTutorial();
    }

    public void ViewTutorial() {
        if (yarnSpinner.activeInHierarchy) { return; }

        runner.Stop();

        yarnSpinner.SetActive(true);
        background.SetActive(true);
        skipClicked = false;
        skip = false;
        Debug.Log(MGamePhase.ToString());
        runner.StartDialogue(MGamePhase.ToString());
    }

    #endregion

    #region Reset

    public void ResetForNewGame() {
        actualPlayer.ResetForNewGame();
        opponentPlayer.ResetForNewGame();

        // where are we in game phases?
        MGamePhase = GamePhase.Start;
        mPreviousGamePhase = GamePhase.Start;

        // Various turn and game info.
        myTurn = false;
        turnTotal = 0;
        roundsLeft = BASE_MAX_TURNS;
        gameStarted = false;
        MNumberDiscarded = 0;
        mNumberDefense = 0;
        MIsDiscardAllowed = false;
        mIsActionAllowed = false;
        mReceivedEndGame = false;
        mStartGameRun = false;

        // has everything been set?
        isInit = false;

        // keep track of all game messages
        mMessageQueue.Clear();

        // now start the game again
        UserInterface.Instance.ToggleStartScreen(true);
        UserInterface.Instance.ToggleGameCanvas(false);
        // startScreen.SetActive(true);
        // gameCanvas.SetActive(false);
        endGameCanvas.SetActive(false);

        // set the network player ready to play again
        RGNetworkPlayerList.instance.ResetAllPlayersToNotReady();
        RGNetworkPlayerList.instance.SetPlayerType(actualPlayer.playerTeam);
    }
    #endregion
}
