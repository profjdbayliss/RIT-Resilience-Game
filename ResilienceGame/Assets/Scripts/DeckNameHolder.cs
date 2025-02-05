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
    [SerializeField] private TextMeshProUGUI errorMessage;
    [SerializeField] private float timeD = 1337;
    AudioSource audio;
    [SerializeField] private AudioClip errorSound;
    public string folderPath;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();

        folderPath = Application.persistentDataPath + "/";  //Get path of folder

        bigButton.SetActive(false);
        DECK_NAME = Path.Join(Application.streamingAssetsPath + "/SavedCSVs/", "SectorDownCards.csv");
        try //Quick catch
        {
            File.Copy(DECK_NAME, folderPath + Path.GetFileName(DECK_NAME));
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
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
        if (timeD >= 1.5f)
        {
            errorMessage.text = "";
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

            //To make sure there isn't a copy of the file already in the local storage 
            try
            {
                File.Copy(paths[0], folderPath + Path.GetFileName(paths[0]));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                audio.PlayOneShot(errorSound, 1);
                errorMessage.text = (Path.GetFileName(paths[0]) + " already exists!");
            }
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
        Debug.Log("Disable buttons");
    }

    //Used to get info from decks
    public void OpenDeck(string deckLocationAndName, string name)
    {
        DECK_NAME = deckLocationAndName;
        deckName.text = "Deck: " + name.Substring(0, name.Length - 4); //Meant to remove.csv from the textMeshPro
    }
}
