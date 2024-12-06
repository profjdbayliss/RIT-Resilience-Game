using SFB;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class DeckNameHolder : MonoBehaviour
{
    public string DECK_NAME = "SectorDownCards.csv";
    public TextMeshProUGUI deckName;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenCSV() {
        // Open a file panel and filter for .csv files
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Select a CSV File", "", "csv", false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0])) {
            DECK_NAME = paths[0]; // Save the selected file path to DECK_NAME
           // Debug.Log($"Selected CSV file: {DECK_NAME}");
            deckName.text = Path.GetFileName(DECK_NAME);
        }
        else {
            Debug.Log("No file selected.");
        }
    }

}
