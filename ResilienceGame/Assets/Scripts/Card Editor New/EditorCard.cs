using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;

public class EditorCard : MonoBehaviour {
    public class EditorCardData {
        public string team;
        public int duplication;
        public string methods;
        public string sectorsAffected;
        public int targetAmt;
        public CardTarget target;
        public string title;
        public int imgRow, imgCol, bgRow, bgCol;
        public int blueCost, blackCost, purpleCost;
        public string meeplesChanged;
        public double meepleChangeAmt;
        public int cardsDrawn;
        public int cardsRemoved;
        public string effect;
        public int numEffects;
        public string preReqEffect;
        public int duration;
        public bool hasDoom;
        public int diceRoll;
        public string flavourText;
        public string description;
        public bool obfuscated;
        public string imgLocation;
    }


    public string data;
    public Card card;
    public CardFront cardFront;
    public GameObject cardGameObject;
    public EditorCardData cardData = new EditorCardData();
    public Action onClick;
    
    [Header("Error message")]
    [SerializeField] private GameObject errorMessage;

    // Start is called before the first frame update
    void Start() {
        try
        {
            errorMessage = GameObject.Find("card popup 2");
            //errorMessageText = GameObject.Find("card popup").GetComponent<TextMeshProUGUI>();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
    public void UpdateCSVData() {
        data = $"{cardData.team}," +
            $"{cardData.duplication}," +
            $"{cardData.methods}," +
            $"{cardData.target}," +
            $"{cardData.sectorsAffected}," +
            $"{cardData.targetAmt}," +
            $"{cardData.title}," +
            $"{cardData.imgRow}," +
            $"{cardData.imgCol}," +
            $"{cardData.bgRow}," +
            $"{cardData.bgCol}," +
            $"{cardData.meeplesChanged}," +
            $"{cardData.meepleChangeAmt}," +
            $"{cardData.blueCost}," +
            $"{cardData.blackCost}," +
            $"{cardData.purpleCost},," +
            $"{cardData.cardsDrawn}," +
            $"{cardData.cardsRemoved}," +
            $"{cardData.effect}," +
            $"{cardData.numEffects}," +
            $"{cardData.preReqEffect}," +
            $"{cardData.duration}," +
            $"{cardData.hasDoom}," +
            $"{cardData.diceRoll}," +
            $"{cardData.flavourText}," +
            $"{cardData.description}," +
            $"{cardData.imgLocation}," +
            $"{cardData.obfuscated}";
    }
    public void HandleClick() {
        onClick?.Invoke();
    }

    // Update is called once per frame
    void Update() {

    }
    public void Init(string data) {
        this.data = data;
        List<string> csvData = data.Split(',').ToList();

        try {
            cardData.team = csvData[0];
            cardData.duplication = int.Parse(csvData[1]);
            cardData.methods = csvData[2];
            cardData.target = (CardTarget)System.Enum.Parse(typeof(CardTarget), csvData[3]);
            cardData.sectorsAffected = csvData[4];
            cardData.targetAmt = int.Parse(csvData[5]);
            cardData.title = csvData[6];
            
            if (int.TryParse(csvData[7], out int result)) {
                cardData.imgRow = result;
            }
            else {
                cardData.imgRow = 0;
            }
            if (int.TryParse(csvData[8], out result)) {
                cardData.imgCol = result;
            }
            else {
                cardData.imgCol = 0;
            }
            //9-10 are atlas locations for the background
            var meepleChangeData = csvData[11];
            var meepleChangeAmtData = csvData[12];

            if (meepleChangeData != "") {
                cardData.meeplesChanged = meepleChangeData;
                cardData.meepleChangeAmt = double.Parse(meepleChangeAmtData);
            }

            cardData.blueCost = int.Parse(csvData[13]);
            cardData.blackCost = int.Parse(csvData[14]);
            cardData.purpleCost = int.Parse(csvData[15]);
            //16 unused facility points
            cardData.cardsDrawn = int.Parse(csvData[17]);
            cardData.cardsRemoved = int.Parse(csvData[18]);
            cardData.effect = csvData[19];
            cardData.numEffects = int.Parse(csvData[20]);
            cardData.preReqEffect = csvData[21];
            cardData.duration = int.Parse(csvData[22]);
            cardData.hasDoom = bool.Parse(csvData[23]);
            cardData.diceRoll = int.Parse(csvData[24]);
            cardData.flavourText = csvData[25];
            cardData.description = csvData[26];
            cardData.imgLocation = csvData[27];
            cardData.obfuscated = bool.Parse(csvData[28]);

            UpdateCardVisuals();
        }
        catch (System.Exception e) {
            Debug.LogError("Error reading card " + data);
            errorMessage.SetActive(true); 
            Debug.LogError(e);
        }



    }
    public void UpdateCardVisuals() {
        //Debug.Log("Updating card visuals for " + cardData.title);
        cardFront.SetTitle(cardData.title);
        cardFront.SetDescription(cardData.description);
        cardFront.SetFlavor(cardData.flavourText);
        cardFront.SetBlueCost(cardData.blueCost);
        cardFront.SetBlackCost(cardData.blackCost);
        cardFront.SetPurpleCost(cardData.purpleCost);
        cardFront.SetImage(cardData.imgLocation);

        cardFront.SetColor(
            cardData.team.ToLower() switch {
                "blue" => new Color(106f / 255f, 137f / 255f, 220f / 255f),
                "red" => new Color(222f / 255f, 0, 0),
                _ => Color.white,
            }
        );


    }

    /// <summary>
    /// Initializes the card data from a CSV string and returns the index of the column that caused a parsing error, if any.
    /// This is useful for debugging which column in the CSV is invalid.
    /// Returns -1 if all columns are parsed successfully.
    /// </summary>
    /// <param name="data">A comma-separated string representing card data.</param>
    /// <returns>
    /// The zero-based index of the column that failed to parse, or -1 if all columns were parsed without error.
    /// </returns>
    public int InitWithColumnTracking(string data)
    {
        this.data = data;
        List<string> csvData = data.Split(',').ToList();

        try
        {
            // Attempt to parse and assign each field in order.
            cardData.team = csvData[0];
            cardData.duplication = int.Parse(csvData[1]);
            cardData.methods = csvData[2];
            cardData.target = (CardTarget)Enum.Parse(typeof(CardTarget), csvData[3]);
            cardData.sectorsAffected = csvData[4];
            cardData.targetAmt = int.Parse(csvData[5]);
            cardData.title = csvData[6];
            cardData.imgRow = int.TryParse(csvData[7], out int imgRow) ? imgRow : 0;
            cardData.imgCol = int.TryParse(csvData[8], out int imgCol) ? imgCol : 0;
            cardData.bgRow = int.TryParse(csvData[9], out int bgRow) ? bgRow : 0;
            cardData.bgCol = int.TryParse(csvData[10], out int bgCol) ? bgCol : 0;
            cardData.meeplesChanged = csvData[11];
            cardData.meepleChangeAmt = string.IsNullOrEmpty(csvData[11]) ? 0 : double.Parse(csvData[12]);
            cardData.blueCost = int.Parse(csvData[13]);
            cardData.blackCost = int.Parse(csvData[14]);
            cardData.purpleCost = int.Parse(csvData[15]);
            // 16: FacilityPoints (unused)
            cardData.cardsDrawn = int.Parse(csvData[17]);
            cardData.cardsRemoved = int.Parse(csvData[18]);
            cardData.effect = csvData[19];
            cardData.numEffects = int.Parse(csvData[20]);
            cardData.preReqEffect = csvData[21];
            cardData.duration = int.Parse(csvData[22]);
            cardData.hasDoom = bool.Parse(csvData[23]);
            cardData.diceRoll = int.Parse(csvData[24]);
            cardData.flavourText = csvData[25];
            cardData.description = csvData[26];
            cardData.imgLocation = csvData[27];
            cardData.obfuscated = bool.Parse(csvData[28]);

            UpdateCardVisuals();
        }
        catch (Exception)
        {
            // If an exception occurs, iterate through each column and try to parse it individually.
            // Return the index of the first column that fails to parse.
            for (int i = 0; i < csvData.Count; i++)
            {
                try
                {
                    switch (i)
                    {
                        case 0: var t0 = csvData[0]; break;
                        case 1: var t1 = int.Parse(csvData[1]); break;
                        case 2: var t2 = csvData[2]; break;
                        case 3: var t3 = (CardTarget)Enum.Parse(typeof(CardTarget), csvData[3]); break;
                        case 4: var t4 = csvData[4]; break;
                        case 5: var t5 = int.Parse(csvData[5]); break;
                        case 6: var t6 = csvData[6]; break;
                        case 7: var t7 = int.TryParse(csvData[7], out _); break;
                        case 8: var t8 = int.TryParse(csvData[8], out _); break;
                        case 9: var t9 = int.TryParse(csvData[9], out _); break;
                        case 10: var t10 = int.TryParse(csvData[10], out _); break;
                        case 11: var t11 = csvData[11]; break;
                        case 12: if (!string.IsNullOrEmpty(csvData[11])) { var t12 = double.Parse(csvData[12]); } break;
                        case 13: var t13 = int.Parse(csvData[13]); break;
                        case 14: var t14 = int.Parse(csvData[14]); break;
                        case 15: var t15 = int.Parse(csvData[15]); break;
                        // 16: FacilityPoints (unused)
                        case 17: var t17 = int.Parse(csvData[17]); break;
                        case 18: var t18 = int.Parse(csvData[18]); break;
                        case 19: var t19 = csvData[19]; break;
                        case 20: var t20 = int.Parse(csvData[20]); break;
                        case 21: var t21 = csvData[21]; break;
                        case 22: var t22 = int.Parse(csvData[22]); break;
                        case 23: var t23 = bool.Parse(csvData[23]); break;
                        case 24: var t24 = int.Parse(csvData[24]); break;
                        case 25: var t25 = csvData[25]; break;
                        case 26: var t26 = csvData[26]; break;
                        case 27: var t27 = csvData[27]; break;
                        case 28: var t28 = bool.Parse(csvData[28]); break;
                    }
                }
                catch
                {
                    // Return the index of the column that failed to parse
                    return i;
                }
            }
            // If no specific column is found, return -1 as a fallback
            return -1;
        }
        // Return -1 if all columns were parsed successfully
        return -1;
    }
}
