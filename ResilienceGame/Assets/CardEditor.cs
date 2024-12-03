using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardEditor : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI deckTitle;
    [SerializeField] private GameObject editorCardPrefab;
    [SerializeField] private RectTransform editorCardContainer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {

    }

    public void OpenDeck() {
        // Check if the application supports file dialogs
        string s = "";
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer) {
            // Show the file browser dialog
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open CSV File", "", "csv", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0])) {
                string filePath = paths[0];
                try {
                    // Read the CSV file contents
                    string[] lines = File.ReadAllLines(filePath).Skip(1).ToArray();

                    // Process each line (for example, split by commas)
                    foreach (string line in lines) {


                        var editorCard = Instantiate(editorCardPrefab, editorCardContainer).GetComponent<EditorCard>();
                        editorCard.Init(line);


                        //string[] values = line.Split(',');

                        //// Example: Output the CSV values to the console
                        //Debug.Log($"Line Data: {string.Join(", ", values)}");
                        //s += string.Join(", ", values) + "\n";


                    }
                    deckTitle.text = Path.GetFileName(filePath);
                }
                catch (Exception e) {
                    Debug.LogError($"Error reading CSV file: {e.Message}");
                }
            }
            else {
                Debug.Log("No file selected.");
            }
        }
        else {
            Debug.LogError("File browser dialogs are not supported on this platform.");
        }
    }

}
