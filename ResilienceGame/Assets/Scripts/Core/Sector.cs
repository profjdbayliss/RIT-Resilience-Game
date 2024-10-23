using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Security.Principal;
using System.Runtime.InteropServices.ComTypes;
using System.ComponentModel;
using static Facility;

public class Sector : MonoBehaviour {
    #region Fields
    [Header("Simulation")]
    [SerializeField] float facilityDownPercentage = 0.1f;   //10% chance of facility going down each red turn
    [SerializeField] float facilityUpPercentage = 0.5f;     //50% chance of facility going up each blue turn
    public bool[] SimulatedFacilities = new bool[3] { true, true, true }; //simulated facility up/down status
    public bool IsSimulated { get; set; } = false;

    [Header("Player and Sector Info")]
    public SectorType sectorName;
    public CardPlayer Owner { get; private set; }
    [SerializeField] private TextMeshProUGUI sectorOwnerText;
    [SerializeField] private Canvas sectorCanvas;
    [SerializeField] private BoxCollider2D[] facilityColliders;
    public bool isCore; // Indicates if this is the core sector
    public HashSet<Facility> selectedFacilities;
    public Facility[] facilities;
    public bool HasOngoingUpdates => playerCardPlayQueue.Any();

    public Sprite SectorIcon { get; private set; }
    public Image SectorIconImage;

    [Header("Facility Selection")]
    public int numFacilitiesRequired = 0;

    [Header("CSV Reading")]
    [SerializeField] private string csvFileName;
    private string fileLocation; // Filename + directory path
    public string outputAtlasName;

    [Header("Interface")]
    // public RawImage icon;
    public string spriteSheetName = "sectorIconAtlas.png";
    public Texture2D iconAtlasTexture;

    private const string EFFECT_ICON_PATH = "facilityEffectIcons.png";
    public static Sprite[] EffectSprites;
    [SerializeField] private Material outlineMat;

    public bool IsDown => facilities.Any(facility => facility.IsDown) || (IsSimulated && SimulatedFacilities.Any(x => x == false));

    [Header("Game State")]
    public Queue<(Update, GamePhase, CardPlayer)> playerCardPlayQueue = new Queue<(Update, GamePhase, CardPlayer)>();
    public bool IsAnimating { get; set; } = false;


    private readonly Dictionary<SectorType, int> ICON_INDICIES = new Dictionary<SectorType, int> {
        { SectorType.Communications, 3 },
        { SectorType.Energy, 7 },
        { SectorType.Water, 15 },
        { SectorType.Information, 13 },
        { SectorType.Chemical, 1 },
        { SectorType.Commercial, 2 },
        { SectorType.Manufacturing, 11 },
        { SectorType.Dams, 4 },
        { SectorType.Defense, 5 },
        { SectorType.Emergency, 6 },
        { SectorType.Financial, 8 },
        { SectorType.Agriculture, 0 },
        { SectorType.Government, 9 },
        { SectorType.Healthcare, 10 },
        { SectorType.Nuclear, 12 },
        { SectorType.Transport, 14 }
    };
    #endregion

    #region Facility Selection
    public int EnableFacilitySelection(int numRequired, PlayerTeam opponentTeam, bool removeEffect, FacilityEffectType preReqEffect) {
        if (numRequired <= 0) {
            Debug.LogError("Must require more than 0 facilities to select");
            return 0;
        }
        int numAvailForSelect = 0;
        selectedFacilities = new HashSet<Facility>();
        //special case to select all facilities
        if (numRequired == 3) {
            foreach (Facility facility in facilities) {
                if (facility != null) {
                    if (facility.HasEffectOfType(preReqEffect) || preReqEffect == FacilityEffectType.None) {
                        selectedFacilities.Add(facility);
                    }
                }
            }
            return 3;
        }
        //get each of the facilities that can be selected
        foreach (Facility facility in facilities) {
            if (facility != null) {
                //narrow down facility selection based on if the facility has removable effects
                //for now, ignore preqreq effects for remove
                if (removeEffect) {
                    if (facility.HasRemovableEffects(opponentTeam: opponentTeam, true)) {
                        numAvailForSelect++;
                        facility.EnableFacilitySelection();
                    }
                }
                else {
                    if (preReqEffect == FacilityEffectType.None || facility.HasEffectOfType(preReqEffect)) {
                        facility.EnableFacilitySelection();
                        numAvailForSelect++;
                    }

                }
            }
        }
        numFacilitiesRequired = Mathf.Min(numAvailForSelect, numRequired); //cap the number required at the number available
        Debug.Log("Enabled facility selection");
        return numFacilitiesRequired;
    }
    public void DisableFacilitySelection() {
        foreach (Facility facility in facilities) {
            if (facility != null) {
                facility.DisableFacilitySelection();
            }
        }
        selectedFacilities = null;
        Debug.Log("Disabled facility selection");
    }
    public void AddFacilityToSelection(Facility facility) {
        if (selectedFacilities == null)
            return;
        selectedFacilities.Add(facility);
        Debug.Log($"Added {facility.facilityName} to selected facilities");
    }
    public bool HasSelectedFacilities() {
        if (numFacilitiesRequired <= 0) return true;
        if (selectedFacilities != null) {
            return selectedFacilities.Count >= numFacilitiesRequired;
        }
        return false;
    }
    public List<Facility> GetSelectedFacilities() {
        if (selectedFacilities != null)
            return selectedFacilities.ToList();
        return null;
    }
    #endregion

    #region Initialization
    private void CSVRead() {
        //  0: Sector
        //  1: Facility Name
        //  2: Facility Type
        //  3: Dependency 1		
        //  4: Dependency 2
        //  5: Dependency 3
        //  6: Number of Dependant Sectors
        //  7: Number of Sector Dependencies
        //  8: Core Facility T/F
        //  9: Sector Appeal
        //  10: Physical Health		
        //  11: Financial Health
        //  12: Network Health
        //  13: Facility ID // TODO: Use this if possible otherwise remove/replace
        string fileLocation = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!File.Exists(fileLocation)) {
            Debug.Log("Sector file not found");
            return;
        }

        using var reader = new StreamReader(fileLocation);
        // Skip the header row
        reader.ReadLine();

        string line;
        //read one line at a time
        while ((line = reader.ReadLine()) != null) {
            string[] values = line.Split(',');
            if (values.Length <= 1) continue;

            //grab all the values from the line and trim white space
            if (!values[0].Trim().Equals(sectorName.ToString(), StringComparison.OrdinalIgnoreCase)) {
                continue;
            }

            ProcessFacility(values);

            if (!string.IsNullOrEmpty(values[8])) {
                isCore = bool.Parse(values[8].Trim());
                // Debug.Log($"Is it a core sector? {isCore}");
            }
        }
        reader.Close();
    }

    /// <summary>
    /// Processes the facility data from the CSV file
    /// </summary>
    /// <param name="values">array of string values for a single facility (1 line of csv data)</param>
    private void ProcessFacility(string[] values) {
        if (!Enum.TryParse(values[2].Trim(), out Facility.FacilityType facilityType)) {
            Debug.Log($"Unknown facility type: {values[2]}");
            return;
        }
        int index = ((int)facilityType) - 1;
        if (index < 0 || index >= facilities.Length) {
            Debug.LogError($"Invalid facility index: {index}");
            return;
        }
        Facility facility = facilities[index];
        facility.facilityType = facilityType;
        facility.facilityName = values[1];
        facility.UpdateNameText();

        // Debug.Log(facility.dependencies.Length);
        for (int j = 3; j < 6; j++) {
            if (Enum.TryParse(values[j], out SectorType enumName)) {
                facility.dependencies[j - 3] = enumName;
            }
            else {
                Debug.Log($"Dependency not parsed: {values[j]}");
            }
        }
        facility.SetupFacilityPoints(
            int.Parse(values[10]),
            int.Parse(values[11]),
            int.Parse(values[12])
        );
    }
    private void InitEffectSprites() {

        string effectAtlasPath = Path.Combine(Application.streamingAssetsPath, EFFECT_ICON_PATH);
        Texture2D effectAtlasTexture = LoadTextureFromFile(effectAtlasPath);
        if (effectAtlasTexture != null) {
            EffectSprites = SliceSpriteSheet(
                texture: effectAtlasTexture,
                spriteWidth: 50,
                spriteHeight: 50,
                columns: 3,
                rows: 3);
        }
        else {
            Debug.LogError("Failed to load effect icon atlas");
        }
    }
    void UpdateFacilityDependencyIcons() {
        string filePath = Path.Combine(Application.streamingAssetsPath, spriteSheetName);
        LoadIconAtlasTexture(filePath);

        Sprite[] sprites = SliceSpriteSheet(iconAtlasTexture, 256, 256, 4, 4); // Slices into 16 sprites

        //assign the sprites to the facilities
        foreach (Facility facility in facilities) {
            for (int i = 0; i < facility.dependencies.Length; i++) {
                facility.dependencyIcons[i].sprite = sprites[ICON_INDICIES[facility.dependencies[i]]];
            }
        }

    }
    void LoadIconAtlasTexture(string filePath) {
        byte[] fileData = File.ReadAllBytes(filePath);
        iconAtlasTexture = new Texture2D(2, 2);
        iconAtlasTexture.LoadImage(fileData);

    }
    public void Initialize() {
        SectorIcon = SectorIconImage.sprite;
        InitEffectSprites();
        //  sectorCanvas = this.gameObject;
        //overTimeCharges = 3;


        foreach (Facility facility in facilities) {
            facility.Initialize();
        }
        CSVRead();
        UpdateFacilityDependencyIcons();

    }
    public void SetOwner(CardPlayer player) {
        Owner = player;
        sectorOwnerText.text = player.playerName;
    }
    #endregion

    #region New Round

    public void InformFacilitiesOfNewTurn() {
        foreach (Facility facility in facilities) {
            facility.UpdateForNextActionPhase();
        }
    }

    #endregion

    #region Helpers
    public Facility GetLocalFacilityByType(FacilityType type) {
        foreach (Facility facility in facilities) {
            if (facility.facilityType == type) {
                return facility;
            }
        }
        return null;
    }
    public bool HasRemovableEffectsOnFacilities(PlayerTeam opponentTeam) {
        return facilities.Any(facility => facility.HasRemovableEffects(opponentTeam));
    }
    Sprite[] SliceSpriteSheet(Texture2D texture, int spriteWidth, int spriteHeight, int columns, int rows) {
        Sprite[] sprites = new Sprite[columns * rows];

        for (int y = 0; y < rows; y++) {
            for (int x = 0; x < columns; x++) {
                Rect rect = new Rect(x * spriteWidth, y * spriteHeight, spriteWidth, spriteHeight);
                sprites[y * columns + x] = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            }
        }

        return sprites;
    }


    // Helper function to load Texture2D from file
    private Texture2D LoadTextureFromFile(string filePath) {
        if (File.Exists(filePath)) {
            byte[] fileData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData)) {
                return texture;
            }
            else {
                Debug.LogError("Failed to load image from: " + filePath);
            }
        }
        else {
            Debug.LogError("File not found: " + filePath);
        }
        return null;
    }

    #endregion

    #region Facility Downing
    public Facility[] CheckDownedFacilities() {
        Facility[] facilitiesList = new Facility[3];
        int downedFacilities = 0;
        // TODO: check isDown;
        //I think this should work? - Mukund
        for (int i = 0; i < facilities.Length; i++) {
            if (facilities[i].IsDown) {
                facilitiesList[downedFacilities] = facilities[i];
                downedFacilities++;
            }
        }

        return facilitiesList;
    }

    #endregion

    #region Animation
    private void OnAnimationComplete() {
        Debug.Log("animation complete");
        IsAnimating = false;
        if (GameManager.Instance.IsActualPlayersTurn()) 
            UserInterface.Instance.ToggleEndPhaseButton(true);

        // Check if there are more cards in the queue
        if (playerCardPlayQueue.Count > 0) {
            var nextCardPlay = playerCardPlayQueue.Dequeue();
            Debug.Log($"Playing next card update in queue: {nextCardPlay.Item1.Type}");
            ProcessCardPlay(nextCardPlay.Item1, nextCardPlay.Item2, nextCardPlay.Item3);
        }
        GameManager.Instance.CheckIfCanEndPhase();
    }
    #endregion

    #region Interface
    public void ToggleSectorVisuals(bool enable) {
        sectorCanvas.enabled = enable;
        foreach (BoxCollider2D collider in facilityColliders) {
            collider.enabled = enable;
        }
    }


    #endregion

    #region Receiving Network Updates
    //TODO: add more effects that need to call the play function here
    public static bool DoesUpdateCallCardPlay(Card card) {
        var action = card.ActionList.First();

        return action is ReduceTurnsLeftByBackdoor ||
            action is TemporaryReductionOfTurnsLeft ||
            action is CancelTemporaryReductionOfTurns ||
            action is BackdoorCheckNetworkRestore ||
            action is ConvertFortifyToBackdoor;
    }
    public void AddUpdateFromPlayer(Update update, GamePhase phase, CardPlayer player) {
        Debug.Log($"Sector {sectorName} received update of type {update.Type} from {player.playerName}");
        if (IsAnimating) {
            // Debug.Log($"Queueing card update due to ongoing animation: {update.Type}, Facility: {update.FacilityType}");
            playerCardPlayQueue.Enqueue((update, phase, player));
            return;
        }
        // If no animation is in progress, handle the card play immediately
        ProcessCardPlay(update, phase, player);
    }
    void ProcessCardPlay(Update update, GamePhase phase, CardPlayer player) {
        Debug.Log($"Sector {sectorName} is processing a card update from {player.playerName} of type {update.Type} on sector {update.sectorPlayedOn}");

        if (update.Type == CardMessageType.CardUpdate || update.Type == CardMessageType.CardUpdateWithExtraFacilityInfo) {
            IsAnimating = true;
            //disable the ability to end phase during opponent card plays
            //I think this is necessary to stop potential issues
            UserInterface.Instance.ToggleEndPhaseButton(false);
            //handle facility card play
            if (update.FacilityPlayedOnType != FacilityType.None) {
                HandleFacilityOpponentPlay(update, phase, player);
            }
            //handle non facility card
            else if (update.FacilityPlayedOnType == FacilityType.None) {

                HandleFreeOpponentPlay(update, phase, player);
            }
            else {
                Debug.LogError($"Failed to find facility of type: {update.FacilityPlayedOnType}");
            }
        }
        else if (update.Type == CardMessageType.DiscardCard) {
            if (player.TryDiscardFromHandByUID(update.UniqueID)) {
                Debug.Log($"Successfully removed card with uid {update.UniqueID} from {player.playerName}'s hand");
            }
            else {
                Debug.LogError($"Did not find card with uid {update.UniqueID} in {player.playerName}'s hand!!");
            }
        }
        else {
            Debug.Log($"Unhandled update type or facility: {update.Type}, {update.FacilityPlayedOnType}");
        }
    }
    //handles when the opponent plays a non facility/effect card
    void HandleFreeOpponentPlay(Update update, GamePhase phase, CardPlayer player) {
        //Card card = DrawCard(random: false, cardId: update.CardID, uniqueId: -1,
        //    deckToDrawFrom: ref opponent.DeckIDs, dropZone: null,
        //    allowSlippy: false, activeDeck: ref ActiveCards);
        if (player.HandCards.TryGetValue(update.UniqueID, out GameObject cardObject)) {

            Action<Update, Card> OnAnimationResolveCardAction = null;

            Card tempCard = cardObject.GetComponent<Card>();
            Debug.Log($"Found {tempCard.data.name} with uid {tempCard.UniqueID} in {player.playerTeam}'s hand");
            Debug.Log($"Update is of type: {update.Type}");
            //check for extra facility info
            if (update.Type == CardMessageType.CardUpdateWithExtraFacilityInfo
                ) {
                Debug.Log("Extra facility info found in card update");
                if (tempCard.data.effectString == "Remove") {
                    //remove the effect from the facilities
                    OnAnimationResolveCardAction = (update, card) => RemoveFacilityEffectsFromCardUpdate(update);
                }
                else {
                    //add the effect if possible
                    OnAnimationResolveCardAction = (update, card) => AddFacilityEffectsFromCardUpdate(update, tempCard, player.playerTeam);
                }
            }
            //grab the first facility in the sector, this is fine because these card types are not for specific facilities
            Facility tempFacility = GameManager.Instance.AllSectors[update.sectorPlayedOn].facilities[0];
            //Debug.Log($"Update:\ncard uid: {update.UniqueID}\ncard id: " +
            //    $"{update.CardID}\ntype: {update.Type}\nSector: {update.sectorPlayedOn}\nFacility: {update.FacilityPlayedOnType}");
            CreateCardAnimation(
                tempCard,
                gameObject,
                player,
                callPlay: DoesUpdateCallCardPlay(tempCard),//dont actually call the play function of the card once its been passed in, the draw/discard messages are already sent elsewhere
                resolveCardAction: OnAnimationResolveCardAction,
                cUpdate: update,
                facility: tempFacility);
        }
        else {
            Debug.LogError($"{sectorName} was looking for card with uid {update.UniqueID} but did not find it in {player.playerName}'s hand which has size [{player.HandCards.Count}]");
        }


    }
    //handles when the opponent plays a facility/effect card
    void HandleFacilityOpponentPlay(Update update, GamePhase phase, CardPlayer player) {
        Debug.Log($"Handling {player.playerName}'s facility card play with id {update.CardID} and name '{player.GetCardNameFromID(update.CardID)}'");

        var facility = GetLocalFacilityByType(update.FacilityPlayedOnType);
        //get the facility played on and facility object
        if (facility != null) {

            Debug.Log($"{sectorName} is creating card played on facility: {facility.facilityName}");

            //pull the card out of the opponents hand by unique id
            //not sure what will happen here when we add more players
            if (player.HandCards.TryGetValue(update.UniqueID, out GameObject cardObject)) {
                Card tempCard = cardObject.GetComponent<Card>();

                Debug.Log($"Found {tempCard.data.name} with uid {tempCard.UniqueID} in {player.playerTeam}'s hand");

                CreateCardAnimation(tempCard, facility.gameObject, player, facility);
            }
            else {
                Debug.LogError($"{sectorName} was looking for card with uid {update.UniqueID} but did not find it in " +
                    $"{player.playerName}'s hand which has size [{player.HandCards.Count}]");
            }
        }
    }
    void RemoveFacilityEffectsFromCardUpdate(Update update) {
        Debug.Log("looking to remove debuffs from selected facilities:");
        var rm1 = TryRemoveEffectFromPlayerFacilityByType(update.AdditionalFacilitySelectedOne, update.FacilityEffectToRemoveType);
        var rm2 = TryRemoveEffectFromPlayerFacilityByType(update.AdditionalFacilitySelectedTwo, update.FacilityEffectToRemoveType);
        var rm3 = TryRemoveEffectFromPlayerFacilityByType(update.AdditionalFacilitySelectedThree, update.FacilityEffectToRemoveType);

        if (rm1 || rm2 || rm3) {
            Debug.Log($"Successfully removed {update.FacilityEffectToRemoveType} from facilities");
        }
        else {
            Debug.Log($"Failed to remove {update.FacilityEffectToRemoveType} from facilities");
        }
    }
    void AddFacilityEffectsFromCardUpdate(Update update, Card card, PlayerTeam playerTeam) {
        Debug.Log("looking to add debuffs to selected facilities:");
        FacilityEffectType preReqEffect = card.data.preReqEffectType;
        var facilities = new[]{update.AdditionalFacilitySelectedOne,
                                        update.AdditionalFacilitySelectedTwo,
                                        update.AdditionalFacilitySelectedThree };

        List<Facility> facilitiesToAffect = new List<Facility>(3);
        // Loop through the facilities tuple
        foreach (var facilityType in facilities) {
            // Check if the facility type is not None
            if (facilityType != FacilityType.None) {
                var facility = GetLocalFacilityByType(facilityType);
                // Try to get the facility from the ActiveFacilities dictionary
                if (facility != null) {
                    if (preReqEffect == FacilityEffectType.None || facility.HasEffectOfType(preReqEffect)) {
                        facilitiesToAffect.Add(facility);
                    }
                }
                else {
                    // Handle the case where the facility is not found in ActiveFacilities
                    Debug.LogError($"Facility of type {facilityType} not found in ActiveFacilities.");
                }
            }
        }
        //add the effects, already filtered for prereq effects
        facilitiesToAffect.ForEach(facility => {
            facility.AddRemoveEffectsByIdString(card.data.effectString, true, playerTeam);
        });

    }
    private bool TryRemoveEffectFromPlayerFacilityByType(FacilityType facilityType, FacilityEffectType effectTypeToRemove) {
        if (facilityType == FacilityType.None || effectTypeToRemove == FacilityEffectType.None) {
            //Debug.Log("Invalid facility type or effect type (probably just didnt select 3 facilities)"); //actually expected if passing in 2/3 or 1/3 facilities
            return false;
        }
        var facility = GetLocalFacilityByType(facilityType);
        if (facility != null) {
            return facility.TryRemoveEffectByType(effectTypeToRemove);
        }
        Debug.LogError("Facility type not found in active facilities");
        return false;
    }
    //handles 
    private void CreateCardAnimation(Card card, GameObject dropZone, CardPlayer player, Facility facility = null,
        bool callPlay = true, Action<Update, Card> resolveCardAction = null, Update cUpdate = new Update()) {

        Debug.Log($"Handling {player.playerName}'s card play of {card.data.name}");
        if (card != null) {
            if (player.HandCards.TryGetValue(card.UniqueID, out GameObject cardGameObject)) {
                RectTransform cardRect = cardGameObject.GetComponent<RectTransform>();

                // Set the card's parent to nothing, in order to position it in world space
                cardRect.SetParent(null, true);
                Vector2 topMiddle = new Vector2(Screen.width / 2, Screen.height + cardRect.rect.height / 2); // top middle just off the screen
                cardRect.anchoredPosition = topMiddle;
                card.transform.localRotation = Quaternion.Euler(0, 0, 180); // flip upside down as if played by opponent
                cardRect.SetParent(sectorCanvas.transform, true); //parent to the sector canvas to only show anim on active sector
                cardGameObject.SetActive(true);
                //Debug.Log($"Added card to screen, starting animation");
                // Start the card animation
                StartCoroutine(card.MoveAndRotateToCenter(cardRect, dropZone, () => {
                    card.SetCardState(CardState.CardInPlay);
                    player.HandCards.Remove(card.UniqueID); //remove the card from the opponent's hand
                    if (callPlay)
                        card.Play(player: player, facilityActedUpon: facility);

                    //handle extra stuff from card actions
                    //many of them work very differently from the standard card.Play so those Play functions are not called
                    resolveCardAction?.Invoke(cUpdate, card);

                    UserInterface.Instance.UpdateUISizeTrackers();//update hand size ui possibly deck size depending on which card was played
                    // After the current animation is done, check if there's another card queued
                    OnAnimationComplete();
                }));
            }
            else {
                Debug.Log($"Card with uid {card.UniqueID} was not found in {player.playerName}'s Hand which has size {player.HandCards.Count}");
            }


        }
    }
    #endregion

    #region Simulation
    public void SimulateAttack() {
        
        for (int i = 0; i < SimulatedFacilities.Length; i++) {
            SimulatedFacilities[i] = (UnityEngine.Random.value > facilityDownPercentage) || !SimulatedFacilities[i];
        }
        int downedFacilities = SimulatedFacilities.Count(x => x == false);
        Debug.Log($"{sectorName} simulated red attack and lost {downedFacilities} facilities");
    }
    public void SimulateRestore() {
        for (int i = 0; i < SimulatedFacilities.Length; i++) {
            SimulatedFacilities[i] = (UnityEngine.Random.value > facilityUpPercentage) || SimulatedFacilities[i];
        }
        int downedFacilities = SimulatedFacilities.Count(x => x == false);
        Debug.Log($"{sectorName} simulated blue restore and has {downedFacilities} facilities");
    }
    public void SetSimulatedFacilityStatus(bool[] status) {
        
        for (int i = 0; i < status.Length; i++) {
            SimulatedFacilities[i] = status[i];
        }
    }
    #endregion
}
