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

public class Sector : MonoBehaviour {
    #region Fields
    [Header("Player and Sector Info")]
    public SectorType sectorName; // TODO: Move PlayerSector here
    public CardPlayer Owner { get; private set; }
    [SerializeField] private TextMeshProUGUI sectorOwnerText;
    [SerializeField] private GameObject sectorCanvas;
    public bool isCore; // Indicates if this is the core sector
    public HashSet<Facility> selectedFacilities;
    public Facility[] facilities;
    public int overTimeCharges; // Tracks how often a sector can mandate overtime

    

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
                    selectedFacilities.Add(facility);
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
        InitEffectSprites();
      //  sectorCanvas = this.gameObject;
        overTimeCharges = 3;

        
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

    

    #region Interface
    public void ToggleSectorVisuals(bool enable) {
        sectorCanvas.SetActive(enable);
    }
    

    #endregion

    #region Receiving Network Updates
    public void AddUpdateFromPlayer(Update update, GamePhase phase, CardPlayer player) {
        Debug.Log($"Sector {sectorName} received update of type {update.Type} from {player.playerName}");
    }
    #endregion
}
