using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;
using Yarn.Unity;
using UnityEngine.InputSystem;
using System.IO;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, IRGObservable {

    #region fields
    // Static Members
    public static GameManager Instance;
    private bool hasStartedAlready = false;

    // Debug
    public bool DEBUG_ENABLED = true;
    public bool simulateSectors = true;
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
    public PlayerTeam playerTeam = PlayerTeam.Any;
    public PlayerTeam opponentType = PlayerTeam.Any;
    public CardPlayer actualPlayer;
    public WhiteCardPlayer whitePlayer;
    public AICardPlayer aiPlayer;
    public List<CardPlayer> networkPlayers = new List<CardPlayer>();
    private bool myTurn = false;
    public int activePlayerNumber;
    public Dictionary<int, CardPlayer> playerDictionary = new Dictionary<int, CardPlayer>();

    [Header("Sectors")]
    [SerializeField] List<Sector> activeSectors;
    public readonly Dictionary<SectorType, Sector> AllSectors = new Dictionary<SectorType, Sector>();
    public List<Sector> AssignableSectors { get; private set; }
    private Dictionary<SectorType, Sector> SimulatedSectors = new Dictionary<SectorType, Sector>();
    private List<SectorType> SimulatedSectorList = new List<SectorType>();
    public Sector sectorInView;
    int sectorIndex = -1;
    public int NumDownedSectors => AllSectors.Values.Where(sector => sector.IsDown).ToList().Count;

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
    public const int OVERTIME_DURATION = 2;
    public const int EXHAUSTED_DURATION = 2;
    public int MNumberDiscarded { get; private set; } = 0;
    private int mNumberDefense = 0;
    public bool MIsDiscardAllowed { get; private set; } = false;
    private bool mIsActionAllowed = false;
    public bool IsDoomClockActive { get; set; } = false;
    private int numDoomClockTurnsLeft = 3;
    public bool IsBluffActive { get; set; } = false;
    public int bluffTurnCount = 0;
    public int bluffTurnCheck { get; set; } = 0;
    public bool IsRedLayingLow { get; set; } = false;
    public bool IsRedAggressive { get; set; } = false;
    public int aggressionTurnCount = 0;
    public int aggressionTurnCheck { get; set; } = 0;

    public readonly int MAX_MEEPLE_SHARE = 2;

    //// UI Elements
    public GameObject playerSlide;

    // Tutorial
    [Header("Tutorial")]
    public GameObject yarnSpinner;
    private DialogueRunner runner;
    private GameObject background;
    private bool skip;
    private bool skipClicked;
    [SerializeField] private TutorialToggle tutorialToggle;
    private bool isTutorialMode;

    // Networking
    [Header("Networking")]
    private RGNetworkPlayerList mRGNetworkPlayerList;
    public bool IsServer = true;
    private MessageQueue mMessageQueue = new MessageQueue();
    // Observers
    private List<IRGObserver> mObservers = new List<IRGObserver>(20);

    // Logging
    public List<string> messageLog = new List<string>();

    [Header("AI")]
    public bool IsLocalPlayerAI = false;

    [Header("White Cards")]
    private const bool ENABLE_WHITE_CARDS = true;
    private int numTurnsTillWhiteCard = 0;
    private int numWhiteCardOfSameTypePlayed = 0;
    private const int MIN_TURNS_TILL_WHITE_CARD = 2;
    private const int MAX_TURNS_TILL_WHITE_CARD = 5;
    private const float WHITE_CARD_POS_CHANCE = 0.5f;
    private bool playWhite = false;
    private bool playedPosWhiteCard = false;
    private bool hasWhiteCardPlayed = true;

    [Header("Altas Creation")]
    public bool mCreateEnergyAtlas = false;
    public bool mCreateWaterAtlas = false;
    public bool mCreatePosWhiteAtlas = false;
    public bool mCreateNegWhiteAtlas = false;

    [Header("Turn Tracking")]
    private int roundsLeft = 30;
    private int turnTotal = 0;
    private const int BASE_MAX_TURNS = 30;

    private int assignedSectors = 0;
    private int numBluePlayers = 0;
    private const int SECTOR_SIM_TURN_START = 2;
    public int UniqueCardIdCount = 0;
    public bool IsInLobby { get; private set; } = false;
    public int UniqueFacilityEffectIdCount { get; set; }
    public bool WaitingForAnimations { get; private set; } = false; //flag to check if all sectors are ready to progress phase
    public bool CanEndPhase => !activeSectors.Any(sector => sector.HasOngoingUpdates || sector.IsAnimating);

    // Reference to PlayerLobbyManager
    private PlayerLobbyManager playerLobbyManager;

    [Header("SFX")]
    [SerializeField] private AudioSource audio;
    [SerializeField] private AudioClip cardEvent;
    [SerializeField] private AudioClip handCards; 
    [SerializeField] private AudioClip endJingle;

    [Header("debug")]
    private bool canPlaySolo = false;

    #endregion

    #region Initialization
    // Called by the begin game button to set up the player type
    public void SetPlayerType() {
        //Creates the float by getting the value from the slider
        float sliderValue = playerSlide.GetComponent<Slider>().value;

        playerDeckChoice = playerDeckList.GetComponent<TMPro.TMP_Dropdown>();
        if (playerDeckChoice == null) {
            Debug.Log("deck choice is null!");
        }


        // set this player's type
        switch (sliderValue) {
            case 0: //red
                playerTeam = PlayerTeam.Red;
                break;
            case 1: //blue
                playerTeam = PlayerTeam.Blue;
                break;
            default:
                break;
        }

        // display player type on view???
        RGNetworkPlayerList.instance.NotifyPlayerChanges();
        Debug.Log("player type set to be " + playerTeam);
    }

    // Used for Lobby "Begin" Button
    public void BeginLobby()
    {
        if (RGNetworkPlayerList.instance.CheckReadyToStart())
        {
            // Check for minimum number of players
            
            if (RGNetworkPlayerList.instance.playerIDs.Count < 3 && canPlaySolo == false)
            {
                UserInterface.Instance.hostLobbyBeginError.SetActive(true);
                UserInterface.Instance.hostLobbyBeginError.GetComponentInChildren<TextMeshProUGUI>().text = "Not enough players to start the game. Minimum 3 players required.";
                return;
            }

            // Check for at least 1 player on each team
            int redTeamCount = 0;
            int blueTeamCount = 0;
            foreach (var playerType in RGNetworkPlayerList.instance.playerTypes)
            {
                if (playerType.Value == PlayerTeam.Red)
                {
                    redTeamCount++;
                }
                else if (playerType.Value == PlayerTeam.Blue)
                {
                    blueTeamCount++;
                }
            }

            if ((redTeamCount < 1 || blueTeamCount < 1) && canPlaySolo == false)
            {
                UserInterface.Instance.hostLobbyBeginError.SetActive(true);
                UserInterface.Instance.hostLobbyBeginError.GetComponentInChildren<TextMeshProUGUI>().text = "Each team must have at least 1 player.";
                return;
            }
            

            RGNetworkPlayerList.instance.AddWhitePlayer();
            RealGameStart();
            // get the turn taking flags ready to go again
            var keys = RGNetworkPlayerList.instance.playerTurnTakenFlags.Keys.ToList();
            foreach (var key in keys)
            {
                RGNetworkPlayerList.instance.playerTurnTakenFlags[key] = false;
            }

        }
        else
        {
            UserInterface.Instance.hostLobbyBeginError.SetActive(true);
        }
    }

    // Called when pressing the button to start
    // Doesn't actually start the game until ALL
    // the players connected have pressed their start buttons.
    public void StartGame() {
        Debug.Log("Start Game called");
        if (!mStartGameRun) {
            Debug.Log("running start of game");

            AssignableSectors = new List<Sector>(activeSectors);
            activeSectors.ForEach(sector => AllSectors.Add(sector.sectorName, sector));

            // basic init of players
            SetPlayerType();
            SetupActors();

            // init various objects to be used in the game
            roundsLeft = BASE_MAX_TURNS;
            turnTotal = 0;
            numTurnsTillWhiteCard = UnityEngine.Random.Range(MIN_TURNS_TILL_WHITE_CARD, MAX_TURNS_TILL_WHITE_CARD); //2-5 turns


            actualPlayer.playerName = RGNetworkPlayerList.instance.localPlayerName;

            // tell everybody else of this player's type
            if (!IsServer) {
                Message msg;
                List<int> tmpList = new List<int>() {
                    (int)playerTeam
                };
                msg = new Message(CardMessageType.SharePlayerType, (uint)RGNetworkPlayerList.instance.localPlayerID, tmpList);
                AddMessage(msg);


            }
            else {
                RGNetworkPlayerList.instance.SetPlayerType(playerTeam);
            }

        }

        UserInterface.Instance.StartGame(playerTeam);
        //RGNetworkPlayerList.instance.RpcUpdatePlayerList(RGNetworkPlayerList.instance.playerIDs.ToArray(), RGNetworkPlayerList.instance.playerNames.ToArray());
        RGNetworkPlayerList.instance.NotifyPlayerChanges();

        mStartGameRun = true;
        Debug.Log("start game set!");
    }
    //called by the network server to assign the sectors to the players
    public void AssignSectorToPlayer(int playerIndex, int sectorType) {
        if (playerDictionary.ContainsKey(playerIndex)) {
            assignedSectors++;
            var player = playerDictionary[playerIndex];
            Debug.Log($"sector type int: {sectorType}");
            Debug.Log($"Attempting to assign {AllSectors[(SectorType)sectorType]} to {playerDictionary[playerIndex].playerName}");
            if (sectorType < 0 || sectorType >= AssignableSectors.Count) {
                Debug.LogError($"Invalid sector index {sectorType} for player {playerIndex}");
                return;
            }
            var sector = AssignableSectors[sectorType];
            if (sector == null) {
                Debug.LogError($"Missing sector when assigning to player");
                return;
            }
            sector.SetOwner(player);
            player.AssignSector(sector);

            //all sectors have been assigned
            if (assignedSectors >= numBluePlayers) {
                Debug.Log($"Client {actualPlayer.playerName} has finished assigning all sectors to blue players");
                //turn off leftover sectors
                var unAssignedSectors = AssignableSectors.Where(sector => sector.Owner == null).ToList();
                Debug.Log($"Unassigned sectors: {unAssignedSectors.Count}");
                SimulatedSectors = new Dictionary<SectorType, Sector>();
                SimulatedSectors = unAssignedSectors.ToDictionary(sector => sector.sectorName,
                                                                  sector => sector);
                SimulatedSectorList = SimulatedSectors.Keys.ToList();
                SimulatedSectors.Values.ToList().ForEach(sector => sector.SetSimulated(true));
                activeSectors.RemoveAll(unAssignedSectors.Contains);

                //blue player look at their sector
                if (actualPlayer.playerTeam == PlayerTeam.Blue) {
                    SetSectorInView(actualPlayer.PlayerSector);
                }
                else { //red player look at the first sector for now
                    SetSectorInView(activeSectors[0]);
                    //temp assign in view sector to red player
                    actualPlayer.PlayerSector = activeSectors[0];
                }
                //create player menu items now that all players sectors assigned
                playerDictionary.Values.ToList().ForEach(player => UserInterface.Instance.SpawnPlayerMenuItem(player));
                //assign sectors to map icons and enable them
                AllSectors.Values.ToList().ForEach(sector => UserInterface.Instance.AssignSectorToIcon(sector));
            }

        }
        else {
            Debug.LogError($"Player index {playerIndex} not found in player dictionary");
        }
    }

    public void RealGameStart() {
        Debug.Log("running 2nd start of game");
        //gameCanvas.SetActive(true);
        audio.PlayOneShot(handCards, 1);
        UserInterface.Instance.ToggleGameCanvas(true);
        // send out the starting message with all player info
        // and start the next phase
        if (IsServer) {
            // Ensure white player exists before creating start message
            if (RGNetworkPlayerList.instance.playerTypes.All(p => p.Value != PlayerTeam.White))
            {
                RGNetworkPlayerList.instance.AddWhitePlayer();
            }

            Message msg = RGNetworkPlayerList.instance.CreateStartGameMessage();
            AddMessage(msg);
        }



        // make sure to show all our cards
        foreach (GameObject gameObjectCard in actualPlayer.HandCards.Values) {
            gameObjectCard.SetActive(true);
        }

        // make sure to show all our cards
        foreach (GameObject gameObjectCard in actualPlayer.HandCards.Values) {
            gameObjectCard.SetActive(true);
        }
        activeSectors.ForEach(sector => {
            sector.Initialize();
            sector.ToggleSectorVisuals(false);
        });


        var rgNetPlayers = FindObjectsOfType<RGNetworkPlayer>();

        actualPlayer.NetID = RGNetworkPlayerList.instance.localPlayerID;
        // Set the local player's NetID
        actualPlayer.NetID = RGNetworkPlayerList.instance.localPlayerID;

        // Create and populate players using the network player list's ID registry
        foreach (var playerEntry in RGNetworkPlayerList.instance.playerIDs)
        {
            int id = playerEntry.Key;
            RGNetworkPlayer rgPlayer = FindObjectsOfType<RGNetworkPlayer>()
                .FirstOrDefault(p => p.mPlayerID == id);

            if (rgPlayer != null)
            {
                var cardPlayer = rgPlayer.GetComponent<CardPlayer>();
                Debug.Log($"Creating player: {RGNetworkPlayerList.instance.playerNames[id]} ({id})");

                // Use server-synced team data
                PlayerTeam team = RGNetworkPlayerList.instance.playerTypes[id];
                cardPlayer.playerTeam = team;
                cardPlayer.playerName = RGNetworkPlayerList.instance.playerNames[id];
                cardPlayer.NetID = id;
                cardPlayer.DeckName = team == PlayerTeam.Red ? "red" : "blue";
                cardPlayer.InitializeCards();

                // Add to dictionary regardless of team
                playerDictionary[id] = cardPlayer;

                // Track blue players for sector assignment
                if (team == PlayerTeam.Blue)
                {
                    numBluePlayers++;
                }
            }
        }

        //assign sectors to all players
        if (IsServer) {

            var sectorList = new List<Sector>();
            var coreSectors = activeSectors.Where(sector => sector.isCore).ToList();
            var nonCoreSectors = activeSectors.Where(sector => !sector.isCore).ToList();
            var bluePlayers = playerDictionary.Where(kvp => kvp.Value.playerTeam == PlayerTeam.Blue).ToList();
            for (int i = 0; i < bluePlayers.Count; i++) {
                Sector sectorToAssign;
                if (i % 4 == 0) {
                    sectorToAssign = coreSectors[UnityEngine.Random.Range(0, coreSectors.Count)];
                }
                else {
                    sectorToAssign = nonCoreSectors[UnityEngine.Random.Range(0, nonCoreSectors.Count)];
                }
                sectorList.Add(sectorToAssign);
            }
            Debug.Log($"number of sectors to assign: {sectorList.Count}");
            int numAssigned = 0;
            foreach (var kvp in playerDictionary) {
                var player = kvp.Value;
                if (player.playerTeam == PlayerTeam.Red) continue;

                Sector sector = sectorList[numAssigned++];
                int sectorIndex = AssignableSectors.IndexOf(sector);

                sector.SetOwner(player);
                player.AssignSector(sector);

                var sectorPayload = new List<int> { kvp.Key, sectorIndex }; // player id, sector index pairs
                var sectorMsg = new Message(CardMessageType.SectorAssignment, (uint)RGNetworkPlayerList.instance.localPlayerID, sectorPayload);
                AddMessage(sectorMsg);

            }
            //turn off leftover sectors
            AssignableSectors.RemoveAll(sectorList.Contains);
            SimulatedSectors = AssignableSectors.ToDictionary(Sector => Sector.sectorName, Sector => Sector);
            SimulatedSectorList = SimulatedSectors.Keys.ToList();
            SimulatedSectors.Values.ToList().ForEach(sector => sector.SetSimulated(true));


            //remove assgined sectors from the sector pages
            activeSectors.RemoveAll(AssignableSectors.Contains);


            //blue player look at their sector
            if (actualPlayer.playerTeam == PlayerTeam.Blue) {
                SetSectorInView(actualPlayer.PlayerSector);
            }
            else { //red player look at the first sector for now
                SetSectorInView(activeSectors[0]);
                //temp assign in view sector to red player.
                actualPlayer.PlayerSector = activeSectors[0];
            }

            playerDictionary.Values.ToList().ForEach(player => {
                //create player menu items
                UserInterface.Instance.SpawnPlayerMenuItem(player);

            });
            //assign sectors to map icons and enable them
            AllSectors.Values.ToList().ForEach(sector => UserInterface.Instance.AssignSectorToIcon(sector));

        } //end isServer



        // In this game people go in parallel to each other per phase
        myTurn = true;
        gameStarted = true;

        // go on to the next phase
        StartNextPhase();
        UserInterface.Instance.ToggleOvertimeButton(false);
        UserInterface.Instance.ToggleStartScreen(false);

    }
    public void Awake() {
        Application.targetFrameRate = 60;
        Instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        mStartGameRun = false;
        Debug.Log("start run on GameManager");
        if (!hasStartedAlready) {
            CardPlayer.cards.Clear();
            UserInterface.Instance.ToggleStartScreen(true);

            // read water deck
            CardReader reader = blueDeckReader.GetComponent<CardReader>();
            if (reader != null) {
                blueCards = reader.CSVRead(mCreateWaterAtlas);
                CardPlayer.AddCards(blueCards);
            }
            else {
                Debug.Log("Blue deck reader is null.");
            }

            // read energy deck
            reader = redDeckReader.GetComponent<CardReader>();
            if (reader != null) {
                redCards = reader.CSVRead(mCreateEnergyAtlas);
                CardPlayer.AddCards(redCards);
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
            hasStartedAlready = true;
        }
        else {
            Debug.Log("start is being run multiple times!");
        }

    }

    // Set up the main player of the game
    public void SetupActors() {
        if (playerTeam == PlayerTeam.Red) {
            actualPlayer.playerTeam = PlayerTeam.Red;
            actualPlayer.DeckName = "red";

        }
        else if (playerTeam == PlayerTeam.Blue) {
            actualPlayer.playerTeam = PlayerTeam.Blue;
            actualPlayer.DeckName = "blue";

        }

        // Initialize the deck info and set various player zones active
        whitePlayer.InitializeCards();
        actualPlayer.InitMeeples();
        UserInterface.Instance.discardDropZone.SetActive(true);
        UserInterface.Instance.handDropZone.SetActive(true);
        tutorialToggle.WhenGameStarts();
    }

    public void InitializeTutorialMode() 
    {
        isTutorialMode = true;
        playerTeam = PlayerTeam.Blue;
        actualPlayer.playerTeam = PlayerTeam.Blue;
        actualPlayer.DeckName = "blue";
        SetupActors();
        StartGame();
    }

    #endregion

    #region Debug Logging
    public void DebugRollSectorDice() {
        List<SectorType> sectorTypes = new List<SectorType>();
        AllSectors.Values.ToList().ForEach(sector => {
            sector.SectorRollDie();
            sectorTypes.Add(sector.sectorName);
        });

        UserInterface.Instance.ShowDiceRollingPanel(
            sectorTypes,
            "Test Rolling",
            3);

    }
    public void DebugChangeMeepleAmountDiceRoll() {
        DebugRollSectorDice();
        foreach (var sector in AllSectors.Values) {
            if (sector.IsSimulated) continue;
            sector.AddOnDiceRollChangeMeepleAmtMulti(
                    minRoll: 6,
                    meepleType: new string[] { "Black", "Purple" },
                    removalTime: 2,
                    amount: 2);

        }
        //show the UI dice roll panel to everyone
        UserInterface.Instance.ShowDiceRollingPanel(
            AllSectors.Values.Select(x => x.sectorName).ToList(),
            "Test Meeple change on roll",
            6);
    }

    public void DebugHandleWhiteCardPress(Card card) {
        whitePlayer.DebugPlayCard(card);
    }
    public void DebugAiPlayCard() {
        aiPlayer.DebugPlayCard();
    }
    private void HandleDebugLogInput() {
        if (Keyboard.current.f1Key.wasPressedThisFrame) {
            Debug.Log($"{(IsServer ? "**[SERVER]**" : "**[CLIENT]**")}");
            foreach (var kvp in playerDictionary) {
                kvp.Value.LogPlayerInfo();
            }
            whitePlayer.LogPlayerInfo();
        }

        if (Keyboard.current.f2Key.wasPressedThisFrame) {
            string s = "";
            foreach (var sector in AllSectors.Values) {
                s += $"Sector {sector.sectorName} is {(sector.IsDown ? "down" : "up")}\n";
                s += $"Sector is {(sector.IsSimulated ? "simulated" : "human played\n\n")}";
            }
            Debug.Log(s);
        }
        if (Keyboard.current.f5Key.wasPressedThisFrame) {
            playerDictionary.Values.ToList().ForEach(player => Debug.Log($"Player {player.playerName} has {ScoreManager.Instance.GetPlayerScore(player.NetID)}"));
        }
        if (Keyboard.current.f6Key.wasPressedThisFrame) {
            RGNetworkPlayerList.instance.DebugLogPlayerLists();
        }
        if (Keyboard.current.f7Key.wasPressedThisFrame) {
            UserInterface.Instance.DebugToggleSectorMenu();
        }

        if (Keyboard.current.f8Key.wasPressedThisFrame) {
            DebugAiPlayCard();
        }
        if (Keyboard.current.f9Key.wasPressedThisFrame) {
            Debug.Log($"Player has {actualPlayer.mUpdatesThisPhase.Count} updates in queue:");
            foreach (Update update in actualPlayer.mUpdatesThisPhase) {
                Debug.Log(
                          $"Type: {update.Type}\n" +
                          $"Card ID: {update.CardID}\n" +
                          $"Unique ID: {update.UniqueID}\n" +
                          $"Amount: {update.Amount}\n" +
                          $"Facility Played On Type: {update.FacilityPlayedOnType}\n" +
                          $"Facility Effect to Remove Type: {update.FacilityEffectToRemoveType}\n" +
                          $"Additional Facility Selected 1: {update.AdditionalFacilitySelectedOne}\n" +
                          $"Additional Facility Selected 2: {update.AdditionalFacilitySelectedTwo}\n" +
                          $"Additional Facility Selected 3: {update.AdditionalFacilitySelectedThree}\n" +
                          $"Sector Played On: {update.sectorPlayedOn}\n" +
                          $"==============================");
            }
        }



    }
    #endregion

    #region Update
    // Update is called once per frame
    void Update()
    {
        if (DEBUG_ENABLED)
        {
            HandleDebugLogInput();
        }
        if (isInit)
        {
            if (gameStarted)
            {
                HandlePhases(MGamePhase);
            }

            // always notify observers in case there's a message waiting to be processed.
            NotifyObservers();

        }
        else
        {
            // the network takes a while to start up and we wait for it.
            mRGNetworkPlayerList = RGNetworkPlayerList.instance;
            if (mRGNetworkPlayerList != null)
            {  // means network init is done and we're joined
                RegisterObserver(mRGNetworkPlayerList);
                IsServer = mRGNetworkPlayerList.isServer;
                CardPlayer player = GameObject.FindObjectOfType<CardPlayer>();
                if (player != null)
                {
                    // player is initialized
                    isInit = true;
                }
            }
        }

        if (Keyboard.current.numpad9Key.wasPressedThisFrame && !canPlaySolo)
        {
            canPlaySolo = true;
            Debug.Log("Can play solo");
        }

        //check for tab key to show player menu
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            UserInterface.Instance.ToggleMapGUI();
        }
    }


    #endregion

    #region Interface Updates
    public void AddActionLogMessage(string message, bool fromNet = false) {
        UserInterface.Instance.AddActionLogMessage(message, fromNet, IsServer, ref messageLog);
    }
    public void AddCardToPlayHistory(Card card, CardPlayer player, string message, bool fromNet = false) {
        // string s = $"{player.playerName} played {card.front.title} {message}";
        //  UserInterface.Instance.AddActionLogMessage(s, fromNet, IsServer, ref messageLog);

        HistoryMenuController.Instance.AddNewHistoryItem(card, player, message, fromNet, IsServer);
    }
    public void SetSectorInView(Sector sector) {
        // Debug.Log("setting sector in view to " + sector?.sectorName);
        if (sector != null)
            SetSectorInView(activeSectors.IndexOf(sector));
    }
    public void SetSectorInView(int index) {
        // Debug.Log("setting sector in view to " + index);
        if (index >= 0 && index < activeSectors.Count) {
            if (sectorInView != null)
                sectorInView.ToggleSectorVisuals(false);
            sectorInView = activeSectors[index];
            sectorInView.ToggleSectorVisuals(true);
            sectorIndex = index;
            if (actualPlayer.playerTeam == PlayerTeam.Red) {
                actualPlayer.PlayerSector = sectorInView;
            }
            //else {
            //    //change color based on if you are the owner
            //    UserInterface.Instance.SetInfoBackground(sectorInView == actualPlayer.PlayerSector ? 1 : 2);
            //}
            //if (UserInterface.Instance.mapState == UserInterface.MapState.SectorPopup) {
            //    UserInterface.Instance.HandleSectorIconClick(sectorInView);
            //}
        }
    }
    public void ViewPreviousSector() {
        sectorIndex = sectorIndex - 1 >= 0 ? sectorIndex - 1 : activeSectors.Count - 1;
        SetSectorInView(sectorIndex);

    }

    public void ViewNextSector() {
        sectorIndex = sectorIndex + 1 < activeSectors.Count ? sectorIndex + 1 : 0;
        SetSectorInView(sectorIndex);


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

    #endregion

    #region Scoring

    #endregion

    #region Helpers
    public void ToggleAllSectorColliders(bool enable) {
        activeSectors.ForEach(s => s.ToggleFacilityColliders(enable));
    }
    public void QuitToMenu() {
        if (IsServer)
            NetworkManager.singleton.StopHost();
        else {
            NetworkManager.singleton.StopClient();
        }
        SceneManager.LoadScene(0);

    }
    public void QuitToLogin()
    {
        if (IsServer)
            NetworkManager.singleton.StopHost();
        else
        {
            NetworkManager.singleton.StopClient();
        }
        SceneManager.LoadScene(1);

    }
    public void QuitToDesktop() {
        if (IsServer)
            NetworkManager.singleton.StopHost();
        else {
            NetworkManager.singleton.StopClient();
        }
        Application.Quit();
    }
    public List<CardPlayer> GetPlayerByTeam(PlayerTeam team) {
        return playerDictionary.Values.Where(player => player.playerTeam == team).ToList();
    }
    public PlayerTeam GetPlayerTeam(int id) {
        try {
            return playerDictionary[id].playerTeam;
        }
        catch { }
        return PlayerTeam.Any;
    }
    public void CheckIfCanEndPhase() {
        if (WaitingForAnimations) {
            if (CanEndPhase) {
                Debug.Log("All sectors are ready progressing phase");
                WaitingForAnimations = false;
                EndPhase();
            }
        }
    }


    public void EnableOvertime() {
        Debug.Log("EnableOvertime called");
        actualPlayer.StartOvertime();
    }
    public void CheckDownedFacilities() {
        //Debug.Log("Checking downed facilities for each sector");
        //Debug.Log(AllSectors.Count);
        foreach (var kvp in AllSectors) {
            foreach (var fac in kvp.Value.facilities) {
                fac.CheckDownedConnections();
            }
            //if (kvp.Value.IsSimulated)
            //    Debug.Log($"Simulated Sector {kvp.Value.sectorName}'s Facility status: [{kvp.Value.SimulatedFacilities[0]}," +
            //        $"{kvp.Value.SimulatedFacilities[1]}," +
            //        $"{kvp.Value.SimulatedFacilities[2]}]");
            //else
            //    Debug.Log($"Actual Sector {kvp.Value.sectorName}'s Facility status: [{!kvp.Value.facilities[0].IsDown}," +
            //        $"{!kvp.Value.facilities[1].IsDown}," +
            //        $"{!kvp.Value.facilities[2].IsDown}]");
            //Debug.Log($"Sector {kvp.Value.sectorName} thinks it is {(!kvp.Value.IsDown ? "up" : "down")}");
            UserInterface.Instance.ToggleDownedSectorInMenu((int)kvp.Key, kvp.Value.IsDown);

        }
        var downedSectors = activeSectors.Where(sector => sector.IsDown).ToList();
        //Debug.Log($"Found {downedSectors.Count} downed sectors");
        //foreach (var sector in downedSectors) {
        //    Debug.Log($"Downed sector: {sector.sectorName}");
        //}
        if (downedSectors.Count > activeSectors.Count / 2 || downedSectors.Any(sector => sector.isCore)) {
            if (!IsDoomClockActive) {
                Debug.Log($"Starting doom clock, downed sectors: {downedSectors.Count}");
                StartDoomClock();
            }
        }
        else {
            if (IsDoomClockActive) {
                Debug.Log("Stopping doom clock");
                StopDoomClock();
            }
        }
    }
    public void StartDoomClock() {

        IsDoomClockActive = true;
        numDoomClockTurnsLeft = 3;
        UserInterface.Instance.SetDoomClockActive(true);
        ScoreManager.Instance.AddDoomClockActivation();
    }
    public void StopDoomClock() {

        ScoreManager.Instance.AddDoomClockPrevention();
        IsDoomClockActive = false;
        UserInterface.Instance.SetDoomClockActive(false);
    }
    public void ReduceDoomClockTurns() {
        numDoomClockTurnsLeft--;
        Debug.Log($"Reducing doom clock turns: {numDoomClockTurnsLeft} left");
        if (numDoomClockTurnsLeft > 0) {
            UserInterface.Instance.UpdateDoomClockTurnsLeft(numDoomClockTurnsLeft);
        }
    }
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

    public void ForcePlayerDiscardOneCard(int playerID) {
        // Debug.Log($"Forcing player {playerID} to discard one card");
        if (playerDictionary.TryGetValue(playerID, out CardPlayer player)) {
            //  Debug.Log($"force {playerDictionary[playerID].playerName} to discard one card");
            player.TryDiscardRandomCard();
        }
        else {
            Debug.LogError($"Cannot find player to force discard");
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
            return MGamePhase == GamePhase.DrawRed || MGamePhase == GamePhase.ActionRed || MGamePhase == GamePhase.DiscardRed;
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

    #region White Card Handling
    private void HandleWhitePlayer(GamePhase phase, bool phaseJustChanged) {
        if (phaseJustChanged) {
            switch (phase) {
                case GamePhase.PlayWhite:
                    HandleWhiteTurn();
                    break;
                default:
                    EndWhitePlayerTurn();
                    break;
            }
        }
    }
    public void EndWhitePlayerTurn() {
        // Ensure this runs only on the server to avoid duplicate calls
        if (IsServer) {
            //  RGNetworkPlayerList.instance.SetWhitePlayerEndPhase();
            AddMessage(new Message(CardMessageType.EndPhase, (uint)RGNetworkPlayerList.instance.playerIDs.Count - 1));
            Debug.Log("White player's turn ended by server.");
        }
    }

    private void HandleWhiteCardUpdate() {

    }
    private void HandleWhiteTurn() {
        if (!ENABLE_WHITE_CARDS) return;

        if (IsServer) {
            numTurnsTillWhiteCard--;
            if (numTurnsTillWhiteCard == 0) {
                Debug.Log($"Server wants to play a white card");
                playWhite = true;
                audio.PlayOneShot(cardEvent, 1);
                numTurnsTillWhiteCard = UnityEngine.Random.Range(MIN_TURNS_TILL_WHITE_CARD, MAX_TURNS_TILL_WHITE_CARD); //[2-5)
            }
        }
        else {
            //only server handles white card plays
            return;
        }
        //    private const bool ENABLE_WHITE_CARDS = true;
        //private int numTurnsTillWhiteCard = 0;
        //private int numWhiteCardOfSameTypePlayed = 0;
        //private const int MIN_TURNS_TILL_WHITE_CARD = 2;
        //private const int MAX_TURNS_TILL_WHITE_CARD = 5;
        //private const float WHITE_CARD_POS_CHANCE = 0.5f;
        //private bool playWhite = false;
        //private bool playedPosWhiteCard = false;
        //private bool hasWhiteCardPlayed = true;

        if (playWhite) {
            var rand = UnityEngine.Random.Range(0, 1f);
            if (rand < WHITE_CARD_POS_CHANCE) {
                if (playedPosWhiteCard) {
                    if (numWhiteCardOfSameTypePlayed >= 2) {
                        numWhiteCardOfSameTypePlayed = 0;
                        whitePlayer.PlayRandomNegativeCard();
                        playedPosWhiteCard = false;
                    }
                    else {
                        numWhiteCardOfSameTypePlayed++;
                        whitePlayer.PlayRandomPositiveCard();
                        playedPosWhiteCard = true;
                    }
                }
                else {
                    playedPosWhiteCard = true;
                    numWhiteCardOfSameTypePlayed = 0;
                    whitePlayer.PlayRandomPositiveCard();
                }
            }
            else {
                if (!playedPosWhiteCard) {
                    if (numWhiteCardOfSameTypePlayed >= 2) {
                        numWhiteCardOfSameTypePlayed = 0;
                        whitePlayer.PlayRandomPositiveCard();
                        playedPosWhiteCard = true;
                    }
                    else {
                        numWhiteCardOfSameTypePlayed++;
                        whitePlayer.PlayRandomNegativeCard();
                        playedPosWhiteCard = false;
                    }
                }
                else {
                    playedPosWhiteCard = false;
                    numWhiteCardOfSameTypePlayed = 0;
                    whitePlayer.PlayRandomNegativeCard();
                }
            }
            playWhite = false;
            Debug.Log($"{numTurnsTillWhiteCard} turns until next white card");

        }
        else {
            Debug.Log($"{numTurnsTillWhiteCard} turns until next white card");
            EndWhitePlayerTurn();
        }

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
                // Debug.Log($"{actualPlayer.playerTeam}'s end phase button set active");
            }
        }
        else {
            if (activeSectors.Any(sector => sector.HasOngoingUpdates || sector.IsAnimating)) {
                Debug.Log($"A sector was not ready");
                WaitingForAnimations = true;
            }
            else {
                EndPhase(); // end the phase if it isn't your turn, to automatically go to the next phase, still requires the player who's turn it is to end their phase
                Debug.Log("Auto ending phase for " + actualPlayer.playerTeam);
            }

        }
    }
    // Starts the next phase.
    public void StartNextPhase() {

        if (MGamePhase == GamePhase.Start) {
            ProgressPhase();
            actualPlayer.DrawCardsToFillHand();
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
            GamePhase.DrawRed => GamePhase.ActionRed,
            //GamePhase.BonusRed => GamePhase.ActionRed,
            GamePhase.ActionRed => (roundsLeft == 0 ? GamePhase.End : GamePhase.DiscardRed), //end game after red action if turn counter is 0
            GamePhase.DiscardRed => ENABLE_WHITE_CARDS ? GamePhase.PlayWhite : GamePhase.DrawBlue,
            GamePhase.PlayWhite => GamePhase.DrawBlue,
            GamePhase.DrawBlue => GamePhase.ActionBlue,
            GamePhase.ActionBlue => GamePhase.DiscardBlue,
            GamePhase.DiscardBlue => GamePhase.BonusBlue,
            GamePhase.BonusBlue => GamePhase.DrawRed,
            _ => GamePhase.End
        };
    }
    // Handle all the card game phases with
    // this simple state machine.
    public void HandlePhases(GamePhase phase) {


        // keep track of 
        bool phaseJustChanged = false;
        MGamePhase = phase;
        if (!MGamePhase.Equals(mPreviousGamePhase)) {
            phaseJustChanged = true;
            UserInterface.Instance.SetPhaseText(MGamePhase);
            actualPlayer.ReadyState = CardPlayer.PlayerReadyState.ReadyToPlay;
            //mPhaseText.text = MGamePhase.ToString();
            mPreviousGamePhase = phase;
            //SkipTutorial();
        }
        HandleWhitePlayer(phase, phaseJustChanged);

        switch (phase) {
            case GamePhase.Start:
                // start of game phase
                // handled with specialty code outside of this
                break;
            case GamePhase.DrawRed:
            case GamePhase.DrawBlue:


                if (phaseJustChanged) {
                    UserInterface.Instance.ToggleMeepleSharingMenu(false);

                    //do nothing until incoming card actions are resolved
                    //this should prevent the doom clock from turning on after this code was called
                    //which meant the OT button wouldnt get turned on when it should
                    if (activeSectors.Any(sector => sector.HasOngoingUpdates))
                        return;
                    //enable the overtime button if the doom clock is active
                    if (actualPlayer.playerTeam == PlayerTeam.Blue && IsActualPlayersTurn()) {
                        if (IsDoomClockActive) {
                            Debug.Log($"Looking at ot state to enable ot: {actualPlayer.otState} ot charge: {actualPlayer.overTimeCharges}");
                            if (actualPlayer.otState == OverTimeState.None && actualPlayer.overTimeCharges > 0)
                                UserInterface.Instance.ToggleOvertimeButton(true);
                        }
                    }
                    MIsDiscardAllowed = true;
                    // draw cards if necessary
                    if (IsActualPlayersTurn()) {
                        actualPlayer.DrawCardsToFillHand();
                        // set the discard area to work if necessary
                        UserInterface.Instance.EnableDiscardDrop();

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
                            MNumberDiscarded += actualPlayer.HandlePlayCard(MGamePhase);
                        }
                    }
                }
                break;

            case GamePhase.BonusBlue:
                if (phaseJustChanged) {
                    playerDictionary.Values.ToList()
                        .FindAll(player => player.playerTeam == PlayerTeam.Blue)
                        .ForEach(player => player.UpdateMeepleSharing());

                    if (actualPlayer.playerTeam == PlayerTeam.Blue && actualPlayer.LentMeepleAmount < MAX_MEEPLE_SHARE) {
                        UserInterface.Instance.ToggleMeepleSharingMenu(true);
                    }
                }
                break;
            case GamePhase.ActionBlue:
            case GamePhase.ActionRed:
                if (!phaseJustChanged) {
                    if (!mIsActionAllowed) {
                        // do nothing - most common scenario
                    }
                    else if (actualPlayer.GetMeeplesSpent() >= actualPlayer.GetMaxMeeples()) {
                        actualPlayer.HandlePlayCard(MGamePhase); //still need to resolve the card played that spend the final meeples
                        Debug.Log($"Spent: {actualPlayer.GetMeeplesSpent()}/{actualPlayer.GetMaxMeeples()}");
                        mIsActionAllowed = false;
                        UserInterface.Instance.DisplayGameStatus(actualPlayer.playerName + " has spent their meeples. Please push End Phase to continue.");
                    }
                    else {
                        actualPlayer.HandlePlayCard(MGamePhase);
                    }
                }
                else if (phaseJustChanged) {
                    //disable the button since it would have been activated in the previous phase
                    if (actualPlayer.playerTeam == PlayerTeam.Blue) {
                        if (IsDoomClockActive) {
                            UserInterface.Instance.ToggleOvertimeButton(false);
                        }
                    }


                    //simulate the sectors getting attacked/restored 
                    if (simulateSectors && turnTotal >= SECTOR_SIM_TURN_START)
                        SimulateSectors();

                    mIsActionAllowed = true;
                    //actualPlayer.InformSectorOfNewTurn();
                    if (IsActualPlayersTurn()) {
                        actualPlayer.ResetMeeplesSpent();
                        actualPlayer.CheckOnTurnEffects();
                    }
                    //else {
                    //    opponentPlayer.ResetMeeplesSpent();
                    //}
                    //opponentPlayer.InformSectorOfNewTurn();
                }

                break;
            case GamePhase.DiscardRed:
            case GamePhase.DiscardBlue:
                if (phaseJustChanged) {
                    UserInterface.Instance.ToggleMeepleSharingMenu(false);
                    if (!actualPlayer.NeedsToDiscard()) {
                        if (CanEndPhase)
                            EndPhase(); //immediately end phase if no discards needed
                        else
                            WaitingForAnimations = true;
                        return;
                    }
                    //reset player discard amounts
                    MIsDiscardAllowed = true;
                    Debug.Log($"setting discard drop active");
                    // UserInterface.Instance.discardDropZone.SetActive(true);
                    UserInterface.Instance.EnableDiscardDrop();
                    MNumberDiscarded = 0;
                    UserInterface.Instance.DisplayGameStatus(actualPlayer.playerName + " has " + actualPlayer.HandCards.Count + " cards in hand.");
                    UserInterface.Instance.DisplayAlertMessage($"You must discard {actualPlayer.HandCards.Count - CardPlayer.MAX_HAND_SIZE_AFTER_ACTION} cards before continuing", actualPlayer);
                }
                else {

                    if (MIsDiscardAllowed) {
                        MNumberDiscarded += actualPlayer.HandlePlayCard(MGamePhase);

                        if (!actualPlayer.NeedsToDiscard()) {
                            if (CanEndPhase) {

                                Debug.Log("Ending discard phase after finishing discarding");
                                MIsDiscardAllowed = false;
                                EndPhase(); //end phase when done discarding
                            }
                            else
                                WaitingForAnimations = true;

                        }
                        //update alert when discarding                              
                        if (MNumberDiscarded > 0) {
                            UserInterface.Instance.DisplayAlertMessage($"You must discard {actualPlayer.HandCards.Count - CardPlayer.MAX_HAND_SIZE_AFTER_ACTION} cards before continuing", actualPlayer);
                        }
                    }
                }
                break;
            case GamePhase.PlayWhite:
                //if (phaseJustChanged) {
                //    HandleWhiteTurn();
                //}
                break;
            case GamePhase.End:
                // end of game phase
                if (phaseJustChanged) {
                    Debug.Log("end game has happened. Sending message to other player.");
                    int playerScore = actualPlayer.GetScore();
                    audio.PlayOneShot(endJingle, 1);
                    AddMessage(new Message(CardMessageType.EndGame, (uint)RGNetworkPlayerList.instance.localPlayerID));
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
                    UserInterface.Instance.DisableDiscardDrop();
                    //  UserInterface.Instance.discardDropZone.SetActive(false);
                    MIsDiscardAllowed = false;

                    // clear any remaining drops since we're ending the phase now (dont think this is needed anymore)
                    //actualPlayer.ClearDropState();

                    // Debug.Log("ending draw and discard game phase!");

                    // send a message with number of discards of the player'
                    //TODO pretty sure this can be removed
                    //Message msg;
                    //List<int> tmpList = new List<int>(1);
                    //tmpList.Add(MNumberDiscarded);
                    //msg = new Message(CardMessageType.ShareDiscardNumber, (uint)RGNetworkPlayerList.instance.localPlayerID, tmpList);
                    //AddMessage(msg);
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
            AddMessage(new Message(CardMessageType.EndPhase, (uint)RGNetworkPlayerList.instance.localPlayerID));
            myTurn = false;
            actualPlayer.ReadyState = CardPlayer.PlayerReadyState.EndedPhase;
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

    // Returns the list of observers.
    public List<IRGObserver> GetObservers()
    {
        return mObservers;
    }

    // Notifies all observers that there is a message.
    public void NotifyObservers()
    {
        if (!mMessageQueue.IsEmpty())
        {
            while (!mMessageQueue.IsEmpty())
            {
                Message m = mMessageQueue.Dequeue();

                // Skip if sender ID no longer valid
                if (!RGNetworkPlayerList.instance.playerIDs.ContainsKey((int)m.senderID))
                {
                    Debug.LogWarning($"Skipping message from disconnected player ID: {m.senderID}");
                    continue;
                }

                foreach (IRGObserver o in mObservers.ToList()) // Safe against modification
                {
                    try
                    {
                        o.UpdateObserver(m);
                    }
                    catch (KeyNotFoundException e)
                    {
                        //Debug.LogError($"Observer error (likely player disconnected): {e.Message}. Removing observer.");
                        mObservers.Remove(o); // auto-remove bad observer
                    }
                }
            }
        }
    }

    public void HandlePlayerDisconnect(int playerId)
    {
        if (playerDictionary.TryGetValue(playerId, out var player))
        {
            // Cleanup player resources
            // (call a cleanup method if needed)

            // Remove from lists
            playerDictionary.Remove(playerId);
            networkPlayers.Remove(player);

            // If it was their turn, force phase progression
            if (activePlayerNumber == playerId)
            {
                // Option 1: End their phase and move to next
                EndPhase();
                // Option 2: Or directly start next phase
                // StartNextPhase();
            }
        }
    }

    public void SendUpdateFromSector(GamePhase phase, Update update) {
        if (update.Type == CardMessageType.SectorDieRoll) {
            var msg = new Message(CardMessageType.SectorDieRoll,
                (uint)RGNetworkPlayerList.instance.localPlayerID,
                new List<int> { (int)update.sectorPlayedOn, update.Amount });
            AddMessage(msg);
        }
    }
    public void SendUpdatesToOpponent(GamePhase phase, CardPlayer sender) {
        while (sender.HasUpdates()) {
            // Debug.Log("Checking for updates to send to opponent");
            Message msg;
            List<int> tmpList = new List<int>(4);
            CardMessageType messageType = sender.GetNextUpdateInMessageFormat(ref tmpList, phase);
            if (messageType != CardMessageType.None) {
                msg = new Message(messageType, (uint)sender.NetID, tmpList);
                AddMessage(msg);
            }
        }

    }
    public void AddUpdateFromPlayer(Update update, GamePhase phase, uint playerIndex) {
        AddUpdateFromPlayer(update, phase, (int)playerIndex);
    }
    public void AddUpdateFromPlayer(Update update, GamePhase phase, int playerIndex) {
        Debug.Log($"Adding update from player {playerIndex} in phase {phase}");
        if (phase == GamePhase.PlayWhite) {
            HandleWhiteCardUpdate();
        }
        Debug.Log($"Update type: {update.Type}\n" +
            $"card id: {update.CardID}");
        //send card updates to the sector the card was played on
        switch (update.Type) {
            case CardMessageType.CardUpdate:
            case CardMessageType.CardUpdateWithExtraFacilityInfo:
                if (AllSectors.TryGetValue(update.sectorPlayedOn, out Sector sector)) {
                    sector.AddUpdateFromPlayer(update, phase, playerDictionary[playerIndex]);
                }
                else {
                    //no sector played on so its a white card
                    whitePlayer.HandleNetworkUpdate(update, phase);
                }
                break;
            case CardMessageType.MeepleShare:
                if (update.UniqueID != actualPlayer.NetID) {
                    Debug.Log($"meeple share messaage with player target {update.UniqueID} was not for {actualPlayer.playerName}");
                }
                else {
                    actualPlayer.ReceiveBorrowedMeeple(update.CardID);
                }
                break;
            //pass other updates to the card player
            //TODO: list of all players
            default:
                try {
                    Debug.Log($"{actualPlayer.playerName} is adding an update from player id {playerIndex} who it thinks is: {playerDictionary[(int)playerIndex].playerName}");
                    actualPlayer.AddUpdateFromPlayer(update, phase, playerDictionary[playerIndex]);
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
        if (IsDoomClockActive) {
            ReduceDoomClockTurns();
            if (numDoomClockTurnsLeft == 0) {
                MGamePhase = GamePhase.End;
            }
        }

        activeSectors.ForEach(sector => sector.InformFacilitiesOfNewTurn()); //update all facilities of turn end
        playerDictionary.Values.ToList().ForEach(player => {
            player.ResetMeepleCount();
            player.updatedMeeplesThisPhase = false;
        }); //return meeples to max values
        turnTotal++;
        Debug.Log("Red is laying low: " + IsRedLayingLow);
        if (IsRedLayingLow)
            roundsLeft++;
        else roundsLeft--;

        Debug.Log("Red is aggressive: " + IsRedAggressive);
        if (IsRedAggressive) {
            roundsLeft--; //its already subtracting once in line 1466
            aggressionTurnCount++;
            if (aggressionTurnCount >= aggressionTurnCheck) {
                IsRedAggressive = false;
                aggressionTurnCount = 0;
                if (actualPlayer.playerTeam == PlayerTeam.Red)
                    actualPlayer.ResetMaxColorlessMeeples(); //dont have to check for blue cause its colorless
                foreach (CardPlayer player in networkPlayers) {
                    if (player.playerTeam == PlayerTeam.Red)
                        player.ResetMaxColorlessMeeples();
                }
            }
        }

        //update the overtime state
        if (actualPlayer.otState == OverTimeState.Overtime) {
            actualPlayer.OverTimeCounter++;
            if (actualPlayer.OverTimeCounter > OVERTIME_DURATION) {
                actualPlayer.EndOvertime();
            }
            else {
                UserInterface.Instance.UpdateOTText();
            }
        }
        else if (actualPlayer.otState == OverTimeState.Exhausted) {
            actualPlayer.OverTimeCounter++;
            if (actualPlayer.OverTimeCounter > EXHAUSTED_DURATION) {
                actualPlayer.EndExhaustion();
            }
            else {
                UserInterface.Instance.UpdateOTText();
            }
        }

        if (IsBluffActive) {
            bluffTurnCount++;
            BluffCountdown(bluffTurnCheck);
        }
        else bluffTurnCount = 0;



        // mTurnText.text = "" + GetTurnsLeft();
        UserInterface.Instance.SetTurnText(GetTurnsLeft() + "");
        if (IsServer) {
            Debug.Log("server adding increment turn message");
            AddMessage(new Message(CardMessageType.IncrementTurn, (uint)RGNetworkPlayerList.instance.localPlayerID));
        }
    }
    // Gets which turn it is.
    public int GetTurnsLeft() {
        return roundsLeft;
    }

    public void ChangeRoundsLeft(int change) {
        if (roundsLeft + change < 0) {
            roundsLeft = 0;
        }
        else roundsLeft += change;
        UserInterface.Instance.SetTurnText($"{roundsLeft}");
    }

    public void HandleBluffStart(int bluffTurns) {
        //should the number the turns are divided by also not be hard coded?
        //only called at the start of the bluff
        if (!IsBluffActive) {
            roundsLeft /= 2;
            UserInterface.Instance.SetTurnText($"{roundsLeft}");
            IsBluffActive = true;
        }
        else BluffCountdown(bluffTurns);

    }

    public void BluffCountdown(int bluffTurns) {
        if (bluffTurns <= bluffTurnCount) {
            IsBluffActive = false;
            roundsLeft *= 2;
            UserInterface.Instance.SetTurnText($"{roundsLeft}");
        }
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

    #region Sector Simulation
    public void SimulateSectors() {
        if (!simulateSectors) return;
        if (!IsServer) return;
        string response = "";
        var sectorStatus = new List<(int sectorType, bool[] facilityStatus)>();
        if (MGamePhase == GamePhase.ActionBlue) {
            //Debug.Log($"Simulating restore on {SimulatedSectors.Count} sectors");
            SimulatedSectors.Values.ToList().ForEach(sector => {
                response = sector.SimulateRestore();
                sectorStatus.Add(((int)sector.sectorName, sector.SimulatedFacilities));
            });
        }
        else if (MGamePhase == GamePhase.ActionRed) {
            // Debug.Log($"Simulating attack on {SimulatedSectors.Count} sectors");
            SimulatedSectors.Values.ToList().ForEach(sector => {
                response = sector.SimulateAttack();
                sectorStatus.Add(((int)sector.sectorName, sector.SimulatedFacilities));
            });
        }
        // Debug.Log(response);

        RGNetworkPlayerList.instance.SendSectorDataMessage(0, sectorStatus);
        CheckDownedFacilities();

    }
    public void GetSimulationStatusFromNetwork(SectorType sector, bool[] facilityStatus) {
        if (IsServer) return;
        // Debug.Log($"Received simulation status for {sector}");
        if (SimulatedSectors.TryGetValue(sector, out Sector simSector)) {
            simSector.SetSimulatedFacilityStatus(facilityStatus);
            CheckDownedFacilities();
        }
        else {
            Debug.LogError($"Sector {sector} not found in simulation dict");
            SimulatedSectors.Keys.ToList().ForEach(key => Debug.Log(key));
            CheckDownedFacilities();

        }
    }

    #endregion

    #region Reset

    public void ResetForNewGame() {
        //actualPlayer.ResetForNewGame();
        //opponentPlayer.ResetForNewGame();

        playerDictionary.Values.ToList().ForEach(player => player.ResetForNewGame());
        playerDictionary.Clear();
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
        //endGameCanvas.SetActive(false);
        UserInterface.Instance.HideEndGameCanvas();

        // set the network player ready to play again
        RGNetworkPlayerList.instance.ResetAllPlayersToNotReady();
        RGNetworkPlayerList.instance.SetPlayerType(actualPlayer.playerTeam);
    }
    #endregion

    #region Meeple Sharing UI Buttons
    // Add temporary storage for pending worker sharing
    private int mPendingWorkerType = -1;

    public void HandleShareWorkerButtonPress(int index)
    {
        Debug.Log("HandleShareWorkerButtonPress in GameManager");
        if (index < 0 || index >= actualPlayer.BaseMaxMeeples.Length) return;
        if (actualPlayer.BaseMaxMeeples[index] == 0) return;

        // Store worker type but don't deduct yet
        mPendingWorkerType = index;
        UserInterface.Instance.ShowAllySelectionMenu(index);
    }

    public void HandleChoosePlayerToShareWithButtonPress(int playerNetId)
    {
        if (mPendingWorkerType == -1) return;

        if (playerDictionary.TryGetValue(playerNetId, out CardPlayer targetPlayer))
        {
            // Deduct the meeple ONLY after confirming the share
            if (actualPlayer.LendWorker(mPendingWorkerType))
            {
                actualPlayer.ShareWorkerWithPlayer(mPendingWorkerType, targetPlayer);
            }
        }
        mPendingWorkerType = -1;
        UserInterface.Instance.DisableAllySelectionMenu();
    }
    #endregion

    #region End Game
    public void ShowEndGameCanvas() {
        MGamePhase = GamePhase.End;
        ScoreManager.Instance.AddEndgameScore();
        UserInterface.Instance.ShowEndGameCanvas();

    }
    private bool CheckForEmptyDecks(out CardPlayer player) {
        foreach (var kvp in playerDictionary) {
            if (!kvp.Value.HasCardsInDeck) {
                player = kvp.Value;
                return true;
            }
        }
        player = null;
        return false;
    }
    public void Quit() {
        Application.Quit();
    }
    #endregion
}
