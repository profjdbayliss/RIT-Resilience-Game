using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Mirror;
using System.Linq;
using Yarn.Unity;
using System.Xml;
using UnityEngine.InputSystem;
using System.IO;

public class GameManager : MonoBehaviour, IRGObservable
{
    //allow for spawning of cards
    public bool DEBUG_ENABLED = true;

    // Deck readers and resulting card lists.
    public CardReader redDeckReader;
    public CardReader blueDeckReader;
    public bool mCreateEnergyAtlas = false;
    public bool mCreateWaterAtlas = false;
    public List<Card> redCards;
    public List<Card> blueCards;

    public List<string> messageLog = new List<string>();

    // where are we in game phases?
    public GamePhase MGamePhase = GamePhase.Start;
    GamePhase mPreviousGamePhase = GamePhase.Start;

    // Various turn and game info.
    bool myTurn = false;
    int turnTotal = 0;
    

    // set up the proper player cards and type
    PlayerTeam playerType = PlayerTeam.Any;
    PlayerTeam opponentType = PlayerTeam.Any;
    
    public GameObject playerDeckList;
    TMPro.TMP_Dropdown playerDeckChoice;
    public bool gameStarted = false;

    // var's for game rules
    public readonly int MAX_DISCARDS = 3;
    public readonly int MAX_DEFENSE = 1;

    public int MNumberDiscarded { get; private set; } = 0;
    int mNumberDefense = 0;
    public bool MIsDiscardAllowed { get; private set; } = false;
    bool mIsActionAllowed = false;
    bool mReceivedEndGame = false;
    bool mStartGameRun = false;

    // has everything been set?
    bool isInit = false;

    // keep track of all game messages
    MessageQueue mMessageQueue = new MessageQueue();

    // network connections
    RGNetworkPlayerList mRGNetworkPlayerList;
    bool isServer = true; 

    // other classes observe this one's gameplay data
    List<IRGObserver> mObservers = new List<IRGObserver>(20);

    // player types
    //public CardPlayer energyPlayer;
    //public CardPlayer waterPlayer;

    // var's we use so we don't have to switch between
    // the player types for generic stuff
    public CardPlayer actualPlayer;
    public CardPlayer opponentPlayer;
    public GameObject opponentPlayedZone;
    public TextMeshProUGUI mTurnText;
    public TextMeshProUGUI mPhaseText;
    public TextMeshProUGUI mPlayerName;
    public TextMeshProUGUI mPlayerDeckType;
    public TextMeshProUGUI mOpponentName;
    public TextMeshProUGUI mOpponentDeckType;
    public GameObject mEndPhaseButton;
    public GameObject gameCanvas;
    public GameObject startScreen;
    public GameObject tiles;

    // game status text
    public TextMeshProUGUI StatusText;
    // active player
    public TextMeshProUGUI activePlayerText;
    public Color activePlayerColor;

    // Tutorial 
    public GameObject yarnSpinner;
    private DialogueRunner runner;
    private GameObject background;
    private bool skip;
    private bool skipClicked;


    // end game info
    public GameObject endGameCanvas;
    public TMP_Text endGameText;

    public int activePlayerNumber;

    public Camera cam;

    public TextMeshProUGUI titlee;

    // static instance for this class
    public static GameManager instance;

    static bool hasStartedAlready = false;


    public void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        mStartGameRun = false;
        Debug.Log("start run on GameManager");
        if (!hasStartedAlready)
        {
            startScreen.SetActive(true);

            //TODO: Read based on number of players/selection

            // read water deck
            CardReader reader = blueDeckReader.GetComponent<CardReader>();
            if (reader != null)
            {
                // TODO: Set with csv
                blueCards = reader.CSVRead(mCreateWaterAtlas); // TODO: Remove var, single atlas
                CardPlayer.AddCards(blueCards);
                //waterPlayer.playerTeam = PlayerTeam.Blue;
                //waterPlayer.DeckName = "blue";
                Debug.Log("number of cards in all cards is: " + CardPlayer.cards.Count);
            }
            else
            {
                Debug.Log("Blue deck reader is null.");
            }


            // TODO: Remove, should be selected by csv
            // read energy deck
            reader = redDeckReader.GetComponent<CardReader>();
            if (reader != null)
            {
                redCards = reader.CSVRead(mCreateEnergyAtlas);
                CardPlayer.AddCards(redCards);
                //energyPlayer.playerTeam = PlayerTeam.Red;
                //energyPlayer.DeckName = "red";
                Debug.Log("number of cards in all cards is: " + CardPlayer.cards.Count);

            }
            else
            {
                Debug.Log("Energy deck reader is null.");
            }

            // Set dialogue runner for tutorial
            runner = yarnSpinner.GetComponent<DialogueRunner>();
            background = yarnSpinner.transform.GetChild(0).GetChild(0).gameObject;
            //Debug.Log(background);
            hasStartedAlready = true;
        } else
        {
            Debug.Log("start is being run multiple times!");
        }

    }

    // Set up the main player of the game

    public void SetupActors()
    {
        // we should know when choice they
        // wanted by now and can set up
        // appropriate values

        // TODO: Change PlayerType
        if (playerType==PlayerTeam.Red)
        {
            //actualPlayer = energyPlayer;
            actualPlayer.playerTeam = PlayerTeam.Red;
            actualPlayer.DeckName = "red";
        }
        else if (playerType==PlayerTeam.Blue)
        {
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
        actualPlayer.playerDropZone.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_ENABLED) {
            if (Keyboard.current.f1Key.wasPressedThisFrame) {
                Debug.Log("Add card to hand");
                actualPlayer.ForceDrawCard();
            }
            else if (Keyboard.current.f2Key.wasPressedThisFrame) {
                Debug.Log("Add card to discard");
                actualPlayer.ForceDiscardRandomCard();
            }
        }
        if (isInit)
        {
            if (gameStarted)
            {
                HandlePhases(MGamePhase);        
            }
           
            // always notify observers in case there's a message
            // waiting to be processed.
            NotifyObservers();

        } else
        {
            // the network takes a while to start up and 
            // we wait for it.
            mRGNetworkPlayerList = RGNetworkPlayerList.instance;
            if (mRGNetworkPlayerList != null)
            {
                // means network init is done
                // and we're joined
                RegisterObserver(mRGNetworkPlayerList);
                isServer = mRGNetworkPlayerList.isServer;
                CardPlayer player = GameObject.FindObjectOfType<CardPlayer>();
                if (player != null)
                {
                    // player is initialized and ready to go
                    // this follows the network init and also
                    // takes a while to happen
                    isInit = true;
                }
            }
        }

    }

    // Handle all the card game phases with
    // this simple state machine.
    public void HandlePhases(GamePhase phase)
    {
        // TODO: Implement team turns

        // keep track of 
        bool phaseJustChanged = false;
        MGamePhase = phase;
        if (!MGamePhase.Equals(mPreviousGamePhase))
        {
            phaseJustChanged = true;
            mPhaseText.text = MGamePhase.ToString();
            mPreviousGamePhase = phase;
            //SkipTutorial();
        }

        switch (phase)
        {
            case GamePhase.Start:
                // start of game phase
                // handled with specialty code outside of this
                break;
            case GamePhase.Draw:
                /*if (phaseJustChanged && !skip)
                {
                    //runner.StartDialogue("DrawAndDiscard"); // TODO: NULL REF
                    //background.SetActive(true);
                }*/

                if (phaseJustChanged)
                {
                    //reset player discard amounts
                    
                    MIsDiscardAllowed = true;
                    // draw cards if necessary
                    actualPlayer.DrawCards();
                    // set the discard area to work if necessary
                    actualPlayer.discardDropZone.SetActive(true);
                    MNumberDiscarded = 0;
                    DisplayGameStatus("[TEAM COLOR] has drawn " + actualPlayer.HandCards.Count + " cards each."); 
                } else
                {
                    // draw cards if necessary
                    actualPlayer.DrawCards();

                    // check for discard and if there's a discard draw again
                    if (MNumberDiscarded == MAX_DISCARDS)
                    {
                        DisplayGameStatus(mPlayerName.text + " has reached the maximum discard number. Please hit end phase to continue.");
                    }
                    else
                    {
                        if (MIsDiscardAllowed)
                        {
                            MNumberDiscarded += actualPlayer.HandlePlayCard(GamePhase.Draw, opponentPlayer);
                        }
                    }
                }
                break;
            case GamePhase.Bonus:
                /*if (phaseJustChanged && !skip) 
                { 
                    runner.StartDialogue("Defense"); 
                    background.SetActive(true);
                }*//*

                if (phaseJustChanged)
                {
                    mIsActionAllowed = true;
                }

                if (!mIsActionAllowed)
                {
                    // do nothing - most common case
                } 
                else
                if (mNumberDefense >= MAX_DEFENSE)
                {
                    mIsActionAllowed = false;
                    DisplayGameStatus(mPlayerName.text + " has played the maximum number of defense cards. Please hit end phase to continue.");
                }/*
                else
                if (!actualPlayer.CheckForCardsOfType(CardType.Defense, actualPlayer.HandCards))
                {
                    mIsDefenseAllowed = false;
                    // if player has no defense cards to play
                    // then let them know.
                    DisplayGameStatus(mPlayerName.text + " has no defense cards. Please hit end phase to continue.");
                }
                else  if (mIsDefenseAllowed)
                { 
                    mNumberDefense += actualPlayer.HandlePlayCard(GamePhase.Defense, opponentPlayer);
                }
                break;*/
            case GamePhase.Action:
                /*if (phaseJustChanged && !skip) 
                {
                    runner.StartDialogue("Vulnerability");
                    background.SetActive(true);
                }*/

                if (!phaseJustChanged)
                {
                    if (!mIsActionAllowed)
                    {
                        // do nothing - most common scenario
                    } 
                    else
                    if (actualPlayer.GetMeeplesSpent() >= actualPlayer.GetMaxMeeples())
                    {
                        Debug.Log($"Spent: {actualPlayer.GetMeeplesSpent()}/{actualPlayer.GetMaxMeeples()}");
                        mIsActionAllowed = false;
                        DisplayGameStatus(mPlayerName.text + " has spent their meeples. Please push End Phase to continue.");
                    } else
                    {
                        actualPlayer.HandlePlayCard(GamePhase.Action, opponentPlayer);
                    }
                }
                else if (phaseJustChanged)
                {
                    mIsActionAllowed = true;
                }

            break;
        /*case GamePhase.Mitigate:
            if (phaseJustChanged && !skip) 
            { 
                //runner.StartDialogue("Mitigate");
                background.SetActive(true);
            }

            if (!phaseJustChanged)
            {
                if (mAllowMitigationPlayed)
                {
                    actualPlayer.HandlePlayCard(GamePhase.Mitigate, opponentPlayer);
                }
            }
            if (phaseJustChanged
                && !actualPlayer.CheckForCardsOfType(CardType.Mitigation, actualPlayer.HandCards))
            {
                mAllowMitigationPlayed = false;
                // if player has no cards to play
                // let them know
                DisplayGameStatus(mPlayerName.text + " has no Mitigation cards. Please push End Phase to continue.");
            }
            else if (phaseJustChanged)
            {
                mAllowMitigationPlayed = true;
            }

            break;*/
            /*case GamePhase.Attack:
                if (phaseJustChanged && !skip) 
                { 
                    //runner.StartDialogue("Attack");
                    background.SetActive(true);
                }

                if (phaseJustChanged)
                {
                    actualPlayer.HandleAttackPhase(opponentPlayer);
                    opponentPlayer.DiscardAllInactiveCards(DiscardFromWhere.MyPlayZone, false, -1);
                }  
                
                break;*/
            /*case GamePhase.AddStation:
                if (phaseJustChanged && !skip)
                {
                    //runner.StartDialogue("AddStation");
                    background.SetActive(true);
                    skip = true;
                }

                if (phaseJustChanged)
                {
                    // we only need one cycle for this particular
                    // phase as it's automated.
                    /*Card card = actualPlayer.DrawFacility(true, 0);
                    // send message about what facility got drawn                 
                    if (card != null)
                    {
                        AddMessage(new Message(CardMessageType.SendPlayedFacility, card.UniqueID, card.data.cardID));
                        DisplayGameStatus("Both players drew a station card. Please push End Phase to continue.");
                    }

                }
                break;*/
            /*case GamePhase.AddConnections:
                if (phaseJustChanged)
                {
                    mAllowConnections = true;
                } else if (mAllowConnections)
                {
                    DisplayGameStatus("Connection phase is not yet implemented! Please push End Phase to continue.");
                    mAllowConnections = false;
                }          
                break;*/
            case GamePhase.End:
                // end of game phase
                if (phaseJustChanged)
                {
                    Debug.Log("end game has happened. Sending message to other player.");
                    int playerScore = actualPlayer.GetScore();
                    AddMessage(new Message(CardMessageType.EndGame));
                }
                break;
            default:
                break;
        }
    }

    // Called when pressing the button to start
    // Doesn't actually start the game until ALL
    // the players connected have pressed their start buttons.
    public void StartGame()
    {
        if (!mStartGameRun)
        {
            Debug.Log("running start of game");
            // basic init of player
            SetPlayerType();
            SetupActors();

            // init various objects to be used in the game
            gameCanvas.SetActive(true);
            startScreen.SetActive(false); // Start menu isn't necessary now
            turnTotal = 0;
            mTurnText.text = "Turn: " + GetTurn();
            mPhaseText.text = "Phase: " + MGamePhase.ToString();
            mPlayerName.text = RGNetworkPlayerList.instance.localPlayerName;
            mPlayerDeckType.text = "" + playerType;

            // tell everybody else of this player's type
            if (!isServer)
            {
                Message msg;
                List<int> tmpList = new List<int>(1);
                tmpList.Add((int)playerType);
                msg = new Message(CardMessageType.SharePlayerType, tmpList);
                AddMessage(msg);
            }
            else
            {
                RGNetworkPlayerList.instance.SetPlayerType(playerType);
            }
            
        }
        mStartGameRun = true;
        Debug.Log("start game set!");
    }

    public void RealGameStart()
    {
        Debug.Log("running 2nd start of game");

        // send out the starting message with all player info
        // and start the next phase
        if (isServer)
        {
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
        foreach (GameObject gameObjectCard in actualPlayer.HandCards.Values)
        {
            gameObjectCard.SetActive(true);
        }

        // set up the opponent name text
        if (RGNetworkPlayerList.instance.playerIDs.Count > 0)
        {
            Debug.Log("player ids greater than zero for realstart");
            if (RGNetworkPlayerList.instance.localPlayerID == 0)
            {
                mOpponentName.text = RGNetworkPlayerList.instance.playerNames[1];
                mOpponentDeckType.text = "" + RGNetworkPlayerList.instance.playerTypes[1];
                opponentType = RGNetworkPlayerList.instance.playerTypes[1];
               
            }
            else
            {
                mOpponentName.text = RGNetworkPlayerList.instance.playerNames[0];
                mOpponentDeckType.text = "" + RGNetworkPlayerList.instance.playerTypes[0];
                opponentType = RGNetworkPlayerList.instance.playerTypes[0];
            }
            // TODO: Probably needs rewrite when more players added
            if (opponentType == PlayerTeam.Red)
            {
                //opponentPlayer = energyPlayer;
                opponentPlayer.playerTeam = PlayerTeam.Red;
                opponentPlayer.DeckName = "red";
            }
            else
            {
                //opponentPlayer = waterPlayer;
                opponentPlayer.playerTeam = PlayerTeam.Blue;
                opponentPlayer.DeckName = "blue";
            }
            opponentPlayer.InitializeCards();
        }
        //Moved this from player setup so that it updates the opponents game canvas as well
        //is this correct?

        // TODO: Set randomly
        actualPlayer.playerSector = gameCanvas.GetComponentInChildren<Sector>();
        actualPlayer.playerSector.Initialize(PlayerSector.Water);

        // in this game people go in parallel to each other
        // per phase
        myTurn = true;
        gameStarted = true;

        // go on to the next phase
        MGamePhase = GamePhase.Draw;

    }

    // display info about the game's status on the screen
    public void DisplayGameStatus(string message)
    {
        StatusText.text = message;
    }
   

    // WORK: rewrite for this card game
    public void ShowEndGameCanvas()
    {
        MGamePhase = GamePhase.End;
        endGameCanvas.SetActive(true);
        endGameText.text = mPlayerName.text + " ends the game with score " + actualPlayer.GetScore() +
            " and " + mOpponentName.text + " ends the game with score " + opponentPlayer.GetScore();

        //WriteListToFile(Path.Combine(Application.streamingAssetsPath, "messages.log"), messageLog);
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

    public bool HasReceivedEndGame()
    {
        return mReceivedEndGame;
    }

    public void SetReceivedEndGame(bool value)
    {
        mReceivedEndGame = value;
    }

    // WORK: there is no menu?????
    public void BackToMenu()
    {

        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        Destroy(RGNetworkManager.singleton.gameObject);
       // SceneManager.LoadScene(0);

    }

    // Called by dropdown list box to set up the player type
    public void SetPlayerType()
    {
        if (playerDeckChoice == null)
        {
            playerDeckChoice = playerDeckList.GetComponent<TMPro.TMP_Dropdown>();
            if (playerDeckChoice == null)
            {
                Debug.Log("deck choice is null!");
            }
        }

        if (playerDeckChoice != null)
        {
            // set this player's type
            switch (playerDeckChoice.value)
            {
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

    // Show the cards and game UI for player.
    public void ShowPlayUI()
    {
        actualPlayer.handDropZone.SetActive(true);
        actualPlayer.discardDropZone.SetActive(true);
    }

    // Hide the cards and game UI for the player.
    public void HidePlayUI()
    {
        actualPlayer.handDropZone.SetActive(false);
        actualPlayer.discardDropZone.SetActive(false);
    }

    // Ends the phase.
    public void EndPhase()
    {
        switch (MGamePhase)
        {
            case GamePhase.Draw:
                {
                    // make sure we have a full hand
                    actualPlayer.DrawCards();
                    // set the discard area to work if necessary
                    actualPlayer.discardDropZone.SetActive(false);
                    MIsDiscardAllowed = false;

                    // clear any remaining drops since we're ending the phase now
                    actualPlayer.ClearDropState();

                    Debug.Log("ending draw and discard game phase!");

                    // send a message with number of discards of the player
                    Message msg;
                    List<int> tmpList = new List<int>(1);
                    tmpList.Add(MNumberDiscarded);
                    msg = new Message(CardMessageType.ShareDiscardNumber, tmpList);
                    AddMessage(msg);
                }
                break;
            case GamePhase.Action:
                {
                    SendUpdatesToOpponent(MGamePhase, actualPlayer);
                    // reset the defense var's for the next turn
                    mIsActionAllowed = false;
                    mNumberDefense = 0;
                }
                break;
            /*case GamePhase.Vulnerability:
                {
                    // reset vulnerability allowance
                    mAllowVulnerabilitiesPlayed = false;
                    // we need to reset vulnerability costs to be used for next turn               
                    SendUpdatesToOpponent(mGamePhase, actualPlayer);
                    actualPlayer.ResetVulnerabilityCost();
                }
                break;
            case GamePhase.Mitigate:
                {
                    mAllowMitigationPlayed = false;
                    SendUpdatesToOpponent(mGamePhase, actualPlayer);
                    SendUpdatesToOpponent(mGamePhase, opponentPlayer);
                }
                break;
            case GamePhase.Attack:
                {
                    SendUpdatesToOpponent(mGamePhase, actualPlayer);
                    SendUpdatesToOpponent(mGamePhase, opponentPlayer);
                }
                break;

            case GamePhase.AddConnections:
                // WORK
                break;*/
            case GamePhase.End:
                break;
            default:
                break;
        }

        if (myTurn)
        {
            Debug.Log("ending the game phase in gamemanager!");
            //HidePlayUI();
            mEndPhaseButton.SetActive(false);
            AddMessage(new Message(CardMessageType.EndPhase));
            myTurn = false;
        }
    }

    public void SendUpdatesToOpponent(GamePhase phase, CardPlayer player)
    {
        // send a message with defense cards played and where they were played
        Message msg;
        List<int> tmpList = new List<int>(4);
        player.GetUpdatesInMessageFormat(ref tmpList, phase);
        msg = new Message(CardMessageType.SendCardUpdates, tmpList);
        AddMessage(msg);
    }

    public void AddUpdatesFromOpponent(ref List<Updates> updates, GamePhase phase)
    {
        if (updates.Count == 0)
        {
            DisplayGameStatus("Opponent did not play any cards during their turn in phase " + phase);
        }
        else
        {
            DisplayGameStatus("Opponent played " + updates.Count + " cards during their turn in phase " + phase);
        }

        switch (phase)
        {
            case GamePhase.Action:
                opponentPlayer.AddUpdates(ref updates, phase, actualPlayer);
                break;
            /*case GamePhase.Vulnerability:
                // This phase is more painful since it's an opponent card on top of a player facility
                foreach (Updates update in updates)
                {
                    // draw opponent card to place on player facility
                    // create card to be displayed
                    Card card = opponentPlayer.DrawCard(false, update.CardID, -1, ref opponentPlayer.DeckIDs, opponentPlayer.playerDropZone, true, ref opponentPlayer.ActiveCards);
                    Debug.Log("phase vuln opponent card with id : " + update.CardID + " should be in active opponent list.");
                    Debug.Log("opponent active list size is : " + opponentPlayer.ActiveCards.Count);
                    GameObject cardGameObject = opponentPlayer.ActiveCards[card.UniqueID];
                    actualPlayer.AddUpdate(update, cardGameObject, actualPlayer.playerDropZone, phase, false);
                }
                break;
            case GamePhase.Mitigate:
                // mitigation is also an opponent card on a player facility
                // note: it's not enough to just have the card id - need to also 
                // know which facility it's connected to
                foreach (Updates update in updates)
                {
                    // draw opponent card to place on player facility
                    // create card to be displayed
                    Debug.Log("phase mitigate opponent card with id : " + update.CardID + " should be in active opponent list.");
                    Debug.Log("opponent active list size is : " + opponentPlayer.ActiveCards.Count);
                    opponentPlayer.AddUpdate(update, null, actualPlayer.playerDropZone, phase, false);
                }
                break;
            case GamePhase.Attack:
                foreach (Updates update in updates)
                {
                    bool getRidOfFacility = false;
                    // first get card type for each update
                    foreach(GameObject facility in opponentPlayer.ActiveFacilities.Values)
                    {
                        Card facilityCard = facility.GetComponent<Card>();
                        if (facilityCard.data.cardID == update.CardID)
                        {
                            getRidOfFacility = true;
                            break;
                        }
                    }
                    if (getRidOfFacility)
                    {
                        Debug.Log("should be getting rid of facility");
                        opponentPlayer.AddUpdate(update, null, actualPlayer.playerDropZone, phase, true);
                    }
                    else
                    {
                        // this card is owned by the player
                        // NOTE: if facility died then this won't actually do anything except tell
                        // us the facility isn't there in a debug message
                        opponentPlayer.AddUpdate(update, null, actualPlayer.playerDropZone, phase, false);
                    }

                    Debug.Log("phase attack needs to change card with id : " + update.CardID + " should be in active opponent list.");
                    Debug.Log("opponent active list size is : " + opponentPlayer.ActiveCards.Count);
                    Debug.Log("player active list size is : " + actualPlayer.ActiveCards.Count);

                }
                break;*/
            default:
                break;
        }

    }

    public void AddOpponentFacility(int facilityId, int uniqueId)
    {
        opponentPlayer.DrawCard(false, facilityId, uniqueId, ref opponentPlayer.FacilityIDs, opponentPlayedZone,
            false, ref opponentPlayer.ActiveFacilities);
    }

    // Gets the next phase.
    public GamePhase GetNextPhase()
    {
        GamePhase nextPhase = GamePhase.Start;

        switch(MGamePhase)
        {
            case GamePhase.Start:
                nextPhase = GamePhase.Draw;
                break;
            case GamePhase.Draw:
                nextPhase = GamePhase.Bonus;
                break;
            case GamePhase.Bonus:
                nextPhase = GamePhase.Action;
                break;
            case GamePhase.Action:
                // end the game if we're out of cards or have
                // no stations left on the board
                if (actualPlayer.DeckIDs.Count == 0 || (actualPlayer.ActiveFacilities.Count == 0))
                {
                    nextPhase = GamePhase.End;
                } else
                {
                    nextPhase = GamePhase.Draw;
                }      
                break;
            case GamePhase.End:
                nextPhase = GamePhase.End;
                break;
            default:
                break;
        }

        return nextPhase;
    }

    // Increments a turn. Note that turns consist of multiple phases.
    public void IncrementTurn()
    {
        turnTotal++;
        mTurnText.text = "Turn: " + GetTurn();
        if (isServer)
        {
            Debug.Log("server adding increment turn message");
            AddMessage(new Message(CardMessageType.IncrementTurn));
        }
    }

    // Starts the next phase.
    public void StartNextPhase()
    {
        if (!myTurn)
        {
            myTurn = true;
            MGamePhase = GetNextPhase();
            mEndPhaseButton.SetActive(true);
        }

    }

    // Gets which turn it is.
    public int GetTurn()
    {
        return turnTotal;
    }

    public bool CanStationsBeHighlighted()
    {
        return MGamePhase == GamePhase.Bonus || MGamePhase == GamePhase.Action;
    }

    // Adds a message to the message queue for the network.
    public void AddMessage(Message msg)
    {
        mMessageQueue.Enqueue(msg);
    }

    // Registers an observer of the message queue.
    public void RegisterObserver(IRGObserver o)
    {
        if (!mObservers.Exists(x => x == o))
        {
            mObservers.Add(o);
        }
    }
   
    // Registers and observer of the message queue.
    public void RemoveObserver(IRGObserver o)
    { 
        if (mObservers.Exists(x => x == o) )
        {
            mObservers.Remove(o);
        }
    }

    // Notifies all observers that there is a message.
    public void NotifyObservers()
    {
        if (!mMessageQueue.IsEmpty())
        {
            while (!mMessageQueue.IsEmpty())
            {
                Message m = mMessageQueue.Dequeue();
                foreach (IRGObserver o in mObservers)
                {
                    o.UpdateObserver(m);
                    //messageLog.Add(m.ToString()); not correct spot?
                }
            }
        }
    }
    //Sets dialogue to inactive
    // TODO: For all tutorial methods, rework to display different text depending on player
    private void SkipTutorial()
    {
        if (!yarnSpinner.activeInHierarchy) { return; }

        if ((skip && mPreviousGamePhase != GamePhase.Start && MGamePhase == GamePhase.Draw)
            || skipClicked)
        {
            skip = true;
            runner.Stop();
            yarnSpinner.SetActive(false);
            background.SetActive(false);
        }
    }
    
    public void SkipClick()
    {
        skipClicked = true;
        SkipTutorial();
    }

    public void ViewTutorial()
    {
        if (yarnSpinner.activeInHierarchy) { return; }

        runner.Stop();

        yarnSpinner.SetActive(true);
        background.SetActive(true);
        skipClicked = false;
        skip = false;
        Debug.Log(MGamePhase.ToString());
        runner.StartDialogue(MGamePhase.ToString());
    }

    public void ResetForNewGame()
    {
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
}
