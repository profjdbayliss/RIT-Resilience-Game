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
    public PlayerSector sectorName; // TODO: Move playersector here
    public Facility[] facilities;
    public bool isCore;
    public int overTimeCharges; //keeps track of the amount of times a sector can mandate overtime

    public float blueMeeples;
    public float blackMeeples;
    public float purpleMeeples;

    public const int STARTING_MEEPLES = 2;
    private readonly float[] maxMeeples = { 2, 2, 2 };

    public TextMeshProUGUI[] meeplesAmountText;

    // filename - directory path is assumed to be Application.streamingAssetsPath
    // extension is assumed to be csv
    [SerializeField] private string csvFileName;
    // filename + directory path
    private string fileLocation;
    // output atlas filename
    public string outputAtlasName;
    public CardPlayer Owner;

    [SerializeField] private GameObject sectorCanvas;
    public RawImage icon;
    public string spriteSheetName = "sectorIconAtlas.png";
    public Texture2D iconAtlasTexture;

    //private const string BACKDOOR_ICON_PATH = "images/Backdoor.png";
    //private const string FORTIFY_ICON_PATH = "images/Fortified.png";
    private const string EFFECT_ICON_PATH = "facilityEffectIcons.png";
    public static Sprite[] EffectSprites;
    [SerializeField] private Button[] meepleButtons;
    [SerializeField] private Image[] meepleImages;
    [SerializeField] private Material outlineMat;
    public int meeplesSpent = 0;
    public int numMeeplesRequired = 0;
    public int numFacilitiesRequired = 0;
    public HashSet<Facility> selectedFacilities;

    private readonly Dictionary<PlayerSector, int> ICON_INDICIES = new Dictionary<PlayerSector, int> {
        { PlayerSector.Communications, 3 },
        { PlayerSector.Energy, 7 },
        { PlayerSector.Water, 15 },
        { PlayerSector.Information, 13 },
        { PlayerSector.Chemical, 1 },
        { PlayerSector.Commercial, 2 },
        { PlayerSector.Manufacturing, 11 },
        { PlayerSector.Dams, 4 },
        { PlayerSector.Defense, 5 },
        { PlayerSector.Emergency, 6 },
        { PlayerSector.Financial, 8 },
        { PlayerSector.Agriculture, 0 },
        { PlayerSector.Government, 9 },
        { PlayerSector.Healthcare, 10 },
        { PlayerSector.Nuclear, 12 },
        { PlayerSector.Transport, 14 }
    };

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
        numFacilitiesRequired = numAvailForSelect; //cap the number required at the number available
        Debug.Log("Enabled facility selection");
        return numAvailForSelect;
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

    public void InformFacilitiesOfNewTurn() {
        foreach (Facility facility in facilities) {
            facility.UpdateForNextActionPhase();
        }
    }
    public void Initialize(PlayerSector sector) {
        InitEffectSprites();
        sectorCanvas = this.gameObject;
        // TODO: Remove when assigning sectors randomly implemented
        sectorName = sector;
        overTimeCharges = 3;

        blackMeeples = blueMeeples = purpleMeeples = STARTING_MEEPLES;
        //facilities = new Facility[3];

        //for (int i = 0; i < sectorCanvas.transform.childCount; i++) // TODO: If children added change count to 3
        //{
        //    facilities[i] = sectorCanvas.transform.GetChild(i).GetComponent<Facility>();
        //    facilities[i].Initialize();
        //    facilities[i].facilityCanvas = sectorCanvas.transform.GetChild(i).gameObject;
        //    facilities[i].sectorItsAPartOf = this;
        //}
        //I really don't like writing it like this but its ok i guess....
        meeplesAmountText = sectorCanvas.transform.GetChild(3).GetComponentsInChildren<TextMeshProUGUI>();

        //added a child so swapping to this
        facilities = sectorCanvas.GetComponentsInChildren<Facility>();
        foreach (Facility facility in facilities) {
            facility.Initialize();
            facility.sectorItsAPartOf = this;
            facility.facilityCanvas = facility.gameObject;
        }


        CSVRead();
        UpdateFacilityDependencyIcons();

        //assign sector icon
        Texture2D tex = new Texture2D(1, 1);
        byte[] tempBytes = File.ReadAllBytes(Application.streamingAssetsPath + "/Images/" + sector.ToString() + ".png");
        tex.LoadImage(tempBytes);
        icon.texture = tex;
        //Debug.Log(Application.streamingAssetsPath + "/images/" + sector.ToString() + ".png");
    }
    public int GetTotalMeeples() {
        return (int)(blueMeeples + blackMeeples + purpleMeeples);
    }
    public int GetMaxMeeples() {
        return (int)Mathf.Floor(maxMeeples.Aggregate((a, b) => a + b));
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
        //// Load Backdoor icon
        //string backdoorFilePath = Path.Combine(Application.streamingAssetsPath, BACKDOOR_ICON_PATH);
        //Texture2D backdoorTexture = LoadTextureFromFile(backdoorFilePath);
        //if (backdoorTexture != null) {
        //    EffectSprites[0] = Sprite.Create(backdoorTexture, new Rect(0, 0, backdoorTexture.width, backdoorTexture.height), new Vector2(0.5f, 0.5f));
        //}

        //// Load Fortified icon
        //string fortifyFilePath = Path.Combine(Application.streamingAssetsPath, FORTIFY_ICON_PATH);
        //Texture2D fortifyTexture = LoadTextureFromFile(fortifyFilePath);
        //if (fortifyTexture != null) {
        //    EffectSprites[1] = Sprite.Create(fortifyTexture, new Rect(0, 0, fortifyTexture.width, fortifyTexture.height), new Vector2(0.5f, 0.5f));
        //}
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

    public Facility[] CheckDownedFacilities() {
        Facility[] facilitiesList = new Facility[3];
        int downedFacilities = 0;
        // TODO: check isDown;
        //I think this should work? - Mukund
        for (int i = 0; i < facilities.Length; i++) {
            if (facilities[i].isDown) {
                facilitiesList[downedFacilities] = facilities[i];
                downedFacilities++;
            }
        }

        return facilitiesList;
    }

    public bool CanAffordCardPlay(Card card) {
        return card.data.blueCost <= blueMeeples &&
            card.data.blackCost <= blackMeeples &&
            card.data.purpleCost <= purpleMeeples;
    }
    public bool TrySpendMeeples(Card card, ref int numMeeplesSpent) {
        if (CanAffordCardPlay(card)) {
            blueMeeples -= card.data.blueCost;
            blackMeeples -= card.data.blackCost;
            purpleMeeples -= card.data.purpleCost;
            numMeeplesSpent += (int)(card.data.blueCost + card.data.blackCost + card.data.purpleCost); //incrememnt the reference variable to hold total meeples spent
            meeplesSpent += numMeeplesSpent;
            UpdateMeepleAmountUI();
            return true;
        }
        return false;
    }
    private void UpdateMeepleAmountUI() {
        meeplesAmountText[0].text = blackMeeples.ToString();
        meeplesAmountText[1].text = blueMeeples.ToString();
        meeplesAmountText[2].text = purpleMeeples.ToString();
    }
    public void ForcePlayerToChoseMeeples(int numMeeplesRequired, Action onFinish) {
        this.numMeeplesRequired = numMeeplesRequired;
        GameManager.instance.DisplayAlertMessage($"Spend {this.numMeeplesRequired} {(this.numMeeplesRequired > 1 ? "meeples" : "meeple")} to continue", Owner, onAlertFinish: onFinish);
        EnableMeepleButtons();

    }

    private void EnableMeepleButtons() {
        foreach (Button button in meepleButtons) {
            button.interactable = true;
        }
    }
    private void DisableMeepleButtons() {
        foreach (Button button in meepleButtons) {
            button.interactable = false;
        }
    }
    //called by the buttons in the sector canvas
    public void TryButtonSpendMeeple(int index) {
        if (meepleButtons[index].interactable) {
            switch (index) {
                case 0:
                    blackMeeples--;
                    meeplesSpent++;
                    if (blackMeeples == 0) {
                        meepleButtons[index].interactable = false;
                    }
                    break;
                case 1:
                    blueMeeples--;
                    meeplesSpent++;
                    if (blueMeeples == 0) {
                        meepleButtons[index].interactable = false;
                    }
                    break;
                case 2:
                    purpleMeeples--;
                    meeplesSpent++;
                    if (purpleMeeples == 0) {
                        meepleButtons[index].interactable = false;
                    }
                    break;
            }
            if (numMeeplesRequired > 0 && meeplesSpent > 0) {
                numMeeplesRequired--;
                if (numMeeplesRequired == 0) {
                    GameManager.instance.mAlertPanel.ResolveTextAlert();
                    DisableMeepleButtons();
                }
                else {
                    GameManager.instance.DisplayAlertMessage($"Spend {numMeeplesRequired} {(numMeeplesRequired > 1 ? "meeples" : "meeple")} to continue", Owner);

                }
            }
            UpdateMeepleAmountUI();
        }
    }
    public void ResetMeepleCount() {
        meeplesSpent = 0;
        blackMeeples = maxMeeples[0];
        blueMeeples = maxMeeples[1];
        purpleMeeples = maxMeeples[2];
        UpdateMeepleAmountUI();
    }
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
                Debug.Log($"Is it a core sector? {isCore}");
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
        for (int j = 3; j < 6; j++) {
            if (Enum.TryParse(values[j], out PlayerSector enumName)) {
                facility.dependencies[j - 3] = enumName;
            }
            else {
                Debug.Log($"Dependency not parsed: {values[j]}");
            }
        }
        facility.SetFacilityPoints(
            int.Parse(values[10]),
            int.Parse(values[11]),
            int.Parse(values[12])
        );
    }

    public void AddSubtractMeepleAmount(int index, float numMeeples) {
        if (index < 0 || index >= 3) return;
        maxMeeples[index] += numMeeples;
        if (maxMeeples[index] < 0) maxMeeples[index] = 0;

        if (blackMeeples > maxMeeples[0]) blackMeeples = maxMeeples[0];
        if (blueMeeples > maxMeeples[1]) blueMeeples = maxMeeples[1];
        if (purpleMeeples > maxMeeples[2]) purpleMeeples = maxMeeples[2];
        UpdateMeepleAmountUI();
    }
    public void MultiplyMeepleAmount(int index, float multiplier) {
        if (index < 0 || index >= 3) return;
        var reduceAmt = (int)Mathf.Floor(maxMeeples[index] * multiplier);   //don't reduce by a half value...why were meeples floats ever
        if (reduceAmt > 0) {
            AddSubtractMeepleAmount(index, reduceAmt);
        }
    }
}
