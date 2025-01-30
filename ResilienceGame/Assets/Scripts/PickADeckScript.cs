using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PickADeckScript : MonoBehaviour
{
    [SerializeField] private GameObject connectPickACardCanvas;

    //Card Prefab and list
    [SerializeField] private GameObject deckPrefab;
    [SerializeField] private GameObject deckArea;
    [SerializeField] private List<GameObject> decksOfCards;
    public string folderPath;

    private bool isPickACardCanvasActive;
    // Start is called before the first frame update
    void Start()
    {
        folderPath = Application.streamingAssetsPath + "/SavedCSVs/";  //Get path of folder

        isPickACardCanvasActive = false;
        connectPickACardCanvas.SetActive(isPickACardCanvasActive);

        decksOfCards = new List<GameObject>();

        //refreshCards();
    }

    public void ToggleDeckCanvas()
    {
        isPickACardCanvasActive = !isPickACardCanvasActive;
        connectPickACardCanvas.SetActive(isPickACardCanvasActive);
    }

    public void refreshCards()
    {
        //Destroys all the cards to comepletly refresh the list
        for (int i = 0; i < decksOfCards.Count; i++)
        {
            Destroy(decksOfCards[i]);
        }

        //Clears the list
        decksOfCards.Clear();

        //Temp NEW array to push into the list
        string[] filePathsArray = Directory.GetFiles(folderPath, "*.csv"); // Get all files of type .csv in this folder

        //extra I, meant for desks in the list that actually work/are formated for sector down.
        int iExtra = 0;
        //Adds to the list
        for (int i = 0; i < filePathsArray.Length; i++)
        {
            //Temp file that's the file name. Will be parsed in later
            string fileName = Path.GetFileName(filePathsArray[i]);

            string[] linesCSV = File.ReadAllLines(filePathsArray[i]);

            if (linesCSV[0] == "Team,Duplication,Method,Target,SectorsAffected,TargetAmount,Title,imgRow,imgCol,bgCol,bgRow,MeeplesChanged,MeepleIChange,BlueCost,BlackCost,PurpleCost,FacilityPoint,CardsDrawn,CardsRemoved,Effect,EffectCount,PrerequisiteEffect,Duration,DoomEffect,DiceRoll,FlavourText,Description,imgLocation,Obfuscate")
            {
            //Instatiates the deck prefab (it's empty)
            decksOfCards.Add(Instantiate(deckPrefab));

            //Parrents the cards
            decksOfCards[iExtra].transform.parent = deckArea.transform;

            //So the scale doesn't mess up when they spawn in. 
            decksOfCards[iExtra].GetComponent<RectTransform>().localScale = new Vector3(1, 1, 0.33f);

            //Adds in the data to the deck
            decksOfCards[iExtra].GetComponent<DeckValues>().deckLocationAndName = filePathsArray[i];
            decksOfCards[iExtra].GetComponent<DeckValues>().name = fileName;

            iExtra++;
            }
            else
            {
                Debug.Log($"{fileName} is invalid! I'm gonna kill you!"); //To check if a deck is invalid
            }
        }
    }
}

//https://discussions.unity.com/t/solved-loading-image-from-streamingassets/752274/3