using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DeckNameHolder : MonoBehaviour
{
    public string DECK_NAME = "";
    public TextMeshProUGUI deckName;
    //For disabling and reinabling the all buttons with the big button
    [SerializeField] private GameObject bigButton;
    [SerializeField] private float timeD = 1337;

    // Start is called before the first frame update
    void Start()
    {
        bigButton.SetActive(false);
        DECK_NAME = Path.Join(Application.streamingAssetsPath + "/SavedCSVs/", "SectorDownCards.csv");
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        //For counting the time
        timeD += Time.deltaTime;
        if (timeD >= 0.5f) //disables the big button, which blocks all buttons with it's size
        {
            bigButton.SetActive(false);
        }
    }

    public void OpenCSV() {
        // Open a file panel and filter for .csv files
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a CSV File", "", "csv", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0])) {
            
            /*
            DECK_NAME = paths[0]; // Save the selected file path to DECK_NAME
           // Debug.Log($"Selected CSV file: {DECK_NAME}");
            deckName.text = Path.GetFileName(DECK_NAME);
            FileUtil.CopyFileOrDirectory(DECK_NAME, Application.streamingAssetsPath + "/SavedCSVs/" + deckName.text);
            */

            FileUtil.CopyFileOrDirectory(paths[0], Application.streamingAssetsPath + "/SavedCSVs/" + Path.GetFileName(paths[0]));
        }
        else {
            Debug.Log("No file selected.");
        }
    }

    //Meant to add a slight delay so people who double click won't open file explorer again
    public void buttonAddDelay()
    {
        timeD = 0;
        bigButton.SetActive(true);
        Debug.Log("ENABLE YOU");
    }

    //Used to get info from decks
    public void OpenDeck(string deckLocationAndName, string name)
    {
        DECK_NAME = deckLocationAndName;
        deckName.text = "Deck: " + name.Substring(0, name.Length - 4); //Meant to remove.csv from the textMeshPro
    }
}
