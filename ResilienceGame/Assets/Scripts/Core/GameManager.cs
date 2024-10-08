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
    public static GameManager instance;
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
    public CardPlayer opponentPlayer;
    private bool myTurn = false;
    public int activePlayerNumber;

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
    public GameObject gameCanvas;
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
    private int turnTotal = 0;

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
                // TODO: this is tied to the drop down menu
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
            // basic init of player
            SetPlayerType();
            SetupActors();

            // init various objects to be used in the game
            //gameCanvas.SetActive(true);
            alertScreenParent.SetActive(false); //Turn off the alert (selection) screen
            turnTotal = 0;
            mTurnText.text = "Turn: " + GetTurn();
            mPhaseText.text = "Phase: " + MGamePhase.ToString();
            mPlayerName.text = RGNetworkPlayerList.instance.localPlayerName;
            actualPlayer.playerName = RGNetworkPlayerList.instance.localPlayerName;
            mPlayerDeckType.text = "" + playerType;

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
        gameCanvas.SetActive(true);
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
                mOpponentName.text = RGNetworkPlayerList.instance.playerNames[1];
                mOpponentDeckType.text = "" + RGNetworkPlayerList.instance.playerTypes[1];
                opponentType = RGNetworkPlayerList.instance.playerTypes[1];
                opponentPlayer.playerName = RGNetworkPlayerList.instance.playerNames[1];
            }
            else {
                mOpponentName.text = RGNetworkPlayerList.instance.playerNames[0];
                opponentPlayer.playerName = RGNetworkPlayerList.instance.playerNames[0];
                mOpponentDeckType.text = "" + RGNetworkPlayerList.instance.playerTypes[0];
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
        //Moved this from player setup so that it updates the opponents game canvas as well
        //is this correct?

        // TODO: Set randomly
        
        Sector sector = gameCanvas.GetComponentInChildren<Sector>();
        //assign the owner of the sector as the blue player
        sector.Owner = actualPlayer.playerTeam == PlayerTeam.Blue ? actualPlayer : opponentPlayer;
        //give the sector to both players
        //all players will most likely need a list of all sectors
        actualPlayer.AssignSector(sector);
        opponentPlayer.AssignSector(sector);


        sector.Initialize(PlayerSector.Water);

        // in this game people go in parallel to each other
        // per phase
        myTurn = true;
        gameStarted = true;

        // go on to the next phase
        // MGamePhase = GamePhase.DrawRed;
        StartNextPhase();
        startScreen.SetActive(false);

    }
    public void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        mStartGameRun = false;
        Debug.Log("start run on GameManager");
        if (!hasStartedAlready) {
            startScreen.SetActive(true);

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
            if(reader != null)
            {
                positiveWhiteCards = reader.CSVRead(mCreatePosWhiteAtlas);
                CardPlayer.AddCards(positiveWhiteCards);
            }
            else
            {
                Debug.Log("Positive white deck reader is null.");
            }

            reader = negativeWhiteDeckReader.GetComponent<CardReader>();
            if(reader != null)
            {
                negativeWhiteCards = reader.CSVRead(mCreateNegWhiteAtlas); 
                CardPlayer.AddCards(negativeWhiteCards);
            }
            else
            {
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
        actualPlayer.discardDropZone.SetActive(true);
        actualPlayer.handDropZone.SetActive(true);

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
    public void UpdateUISizeTrackers() {
        //TODO: Add check for all red players
        deckSizeTracker.UpdateAllTrackerTexts(
            playerDeckSize: actualPlayer.DeckIDs.Count, 
            playerHandSize: actualPlayer.HandCards.Count, 
            opponentDeckSize: opponentPlayer.DeckIDs.Count, 
            opponentHandSize: opponentPlayer.HandCards.Count);
    }
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

    // Show the cards and game UI for player.
    public void ShowPlayUI() {
        actualPlayer.handDropZone.SetActive(true);
        actualPlayer.discardDropZone.SetActive(true);
    }

    // Hide the cards and game UI for the player.
    public void HidePlayUI() {
        actualPlayer.handDropZone.SetActive(false);
        actualPlayer.discardDropZone.SetActive(false);
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
    public void DisplayCardChoiceMenu(Card card, int numRequired)
    {
        mAlertPanel.AddCardToSelectionMenu(card.gameObject);
        if (numRequired == 0)
            mAlertPanel.ToggleCardSelectionPanel(true);

    }
    // WORK: rewrite for this card game
    public void ShowEndGameCanvas() {
        MGamePhase = GamePhase.End;
        endGameCanvas.SetActive(true);
        endGameText.text = mPlayerName.text + " ends the game with score " + actualPlayer.GetScore() +
            " and " + mOpponentName.text + " ends the game with score " + opponentPlayer.GetScore();

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
            MIsDiscardAllowed = true;
            player.StopDiscard();
            UpdateUISizeTrackers();
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
                mEndPhaseButton.SetActive(true);
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
            GamePhase.ActionRed => GamePhase.DiscardRed,
            GamePhase.DiscardRed => (turnTotal % 3 == 0) ? GamePhase.DrawBlue : GamePhase.PlayWhite,
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
            mPhaseText.text = MGamePhase.ToString();
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
                        actualPlayer.discardDropZone.SetActive(true);
                        MNumberDiscarded = 0;
                    }
                    DisplayGameStatus("[TEAM COLOR] has drawn " + actualPlayer.HandCards.Count + " cards each.");
                }
                else {
                    // draw cards if necessary
                    if (IsActualPlayersTurn())
                        actualPlayer.DrawCardsToFillHand();

                    // check for discard and if there's a discard draw again
                    if (MNumberDiscarded == MAX_DISCARDS) {
                        DisplayGameStatus(mPlayerName.text + " has reached the maximum discard number. Please hit end phase to continue.");
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
                        DisplayGameStatus(mPlayerName.text + " has spent their meeples. Please push End Phase to continue.");
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
                    actualPlayer.discardDropZone.SetActive(true);
                    MNumberDiscarded = 0;
                    DisplayGameStatus(mPlayerName.text + " has " + actualPlayer.HandCards.Count + " cards in hand.");
                    DisplayAlertMessage($"You must discard {actualPlayer.HandCards.Count - CardPlayer.MAX_HAND_SIZE_AFTER_ACTION} cards before continuing", actualPlayer);
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
                            DisplayAlertMessage($"You must discard {actualPlayer.HandCards.Count - CardPlayer.MAX_HAND_SIZE_AFTER_ACTION} cards before continuing", actualPlayer);
                        }
                    }
                }
                break;
            case GamePhase.PlayWhite:
                //if(turnTotal % 9 == 0)
                //{
                //    //positive white
                //    Debug.Log("Playing positive white card on turn " + turnTotal);
                //    int randCard = UnityEngine.Random.Range(0, positiveWhiteCards.Count - 1);
                //    positiveWhiteCards[randCard].Play(null, null);
                //    positiveWhiteCards.RemoveAt(randCard);
                //}
                //else
                //{
                //    //negative white
                //    Debug.Log("Playing negative white card on turn " + turnTotal);
                //    int randCard = UnityEngine.Random.Range(0, negativeWhiteCards.Count - 1);
                //    negativeWhiteCards[randCard].Play(null, null);
                //    negativeWhiteCards.RemoveAt(randCard);
                //}
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
        mAlertPanel.ResolveTextAlert(); //resolve any alerts, there currently should not be alerts that persist to the next phase so we can auto hide any leftover alerts here
        switch (MGamePhase) {
            case GamePhase.DiscardRed:
            case GamePhase.DiscardBlue:
            case GamePhase.DrawBlue:
            case GamePhase.DrawRed: {
                    // make sure we have a full hand
                    // actualPlayer.DrawCards();
                    // set the discard area to work if necessary
                    actualPlayer.discardDropZone.SetActive(false);
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
            mEndPhaseButton.SetActive(false);
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

    public void AddUpdateFromOpponent(Update update, GamePhase phase, uint playerIndex) {

        actualPlayer.AddUpdateFromOpponent(update, phase, opponentPlayer);

    }
    #endregion

    #region Turn Handling

    // Increments a turn. Note that turns consist of multiple phases.
    public void IncrementTurn() {
        // OnRoundEnd?.Invoke(); //inform listeners that the round ended

        actualPlayer.ResetMeepleCount();
        opponentPlayer.ResetMeepleCount();
        turnTotal++;
        mTurnText.text = "Turn: " + GetTurn();
        if (IsServer) {
            Debug.Log("server adding increment turn message");
            AddMessage(new Message(CardMessageType.IncrementTurn));
        }
    }
    // Gets which turn it is.
    public int GetTurn() {
        return turnTotal;
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
        startScreen.SetActive(true);
        gameCanvas.SetActive(false);
        endGameCanvas.SetActive(false);

        // set the network player ready to play again
        RGNetworkPlayerList.instance.ResetAllPlayersToNotReady();
        RGNetworkPlayerList.instance.SetPlayerType(actualPlayer.playerTeam);
    }
    #endregion
}
