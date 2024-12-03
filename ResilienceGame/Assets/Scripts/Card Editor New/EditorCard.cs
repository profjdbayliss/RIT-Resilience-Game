using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public int meepleChangeAmt;
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
    // Start is called before the first frame update
    void Start() {

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
            //7-10 are atlas locations for the image and background
            var meepleChangeData = csvData[11];
            var meepleChangeAmtData = csvData[12];

            if (meepleChangeData != "") {
                cardData.meeplesChanged = meepleChangeData;
                cardData.meepleChangeAmt = int.Parse(meepleChangeAmtData);
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
            Debug.LogError(e);
        }



    }
    public void UpdateCardVisuals() {
        Debug.Log("Updating card visuals for " + cardData.title);
        cardFront.SetTitle(cardData.title);
        cardFront.SetDescription(cardData.description);
        cardFront.SetFlavor(cardData.flavourText);

        cardFront.SetColor(
            cardData.team.ToLower() switch {
                "blue" => new Color(106f / 255f, 137f / 255f, 220f / 255f),
                "red" => new Color(222f / 255f, 0, 0),
                _ => Color.white,
            }
        );
    }
}
