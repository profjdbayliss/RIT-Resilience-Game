using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class Sector : MonoBehaviour
{
    public PlayerSector sectorName; // TODO: Move playersector here
    public Facility[] facilities;
    public bool isCore;
    public float blueMeeples;
    public float blackMeeples;
    public float purpleMeeples;

    public const int STARTING_MEEPLES = 2;

    public TextMeshProUGUI[] meeplesAmountText;

    // filename - directory path is assumed to be Application.streamingAssetsPath
    // extension is assumed to be csv
    [SerializeField] private string csvFileName;
    // filename + directory path
    private string fileLocation;
    // output atlas filename
    public string outputAtlasName;

    [SerializeField] private GameObject sectorCanvas;
    public RawImage icon;
    public string spriteSheetName = "sectorIconAtlas.png";
    public Texture2D texture;

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


    public void Initialize(PlayerSector sector)
    {
        sectorCanvas = this.gameObject;
        // TODO: Remove when assigning sectors randomly implemented
        sectorName = sector;
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
    void UpdateFacilityDependencyIcons() {
        string filePath = Path.Combine(Application.streamingAssetsPath, spriteSheetName);
        LoadTexture(filePath);

        Sprite[] sprites = SliceSpriteSheet(texture, 256, 256, 4, 4); // Slices into 16 sprites

        //assign the sprites to the facilities
        foreach (Facility facility in facilities) {
            for (int i = 0; i < facility.dependencies.Length; i++) {
                facility.dependencyIcons[i].sprite = sprites[ICON_INDICIES[facility.dependencies[i]]];
            }
        }

    }
    void LoadTexture(string filePath) {
        byte[] fileData = File.ReadAllBytes(filePath);
        texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
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

    public Facility[] CheckDownedFacilities()
    {
        Facility[] facilitiesList = new Facility[3];
        int downedFacilities = 0;
        // TODO: check isDown;
        //I think this should work? - Mukund
        for(int i = 0; i < facilities.Length; i++)
        {
            if(facilities[i].isDown)
            {
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
    public void ResetMeepleCount() {
        blueMeeples = blackMeeples = purpleMeeples = STARTING_MEEPLES;
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

            // TODO: arent sectors core not facilities? Actually i just think this print statement is misleading, isCore is part of sector
            if (!string.IsNullOrEmpty(values[8])) {
                isCore = bool.Parse(values[8].Trim());
                Debug.Log($"Is it a core facility? {isCore}");
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

        int index = (int)facilityType;
        if (index < 0 || index >= facilities.Length) {
            Debug.Log($"Invalid facility index: {index}");
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
}
