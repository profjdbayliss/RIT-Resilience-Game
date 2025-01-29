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

        refreshCards();
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

        //Adds to the list
        for (int i = 0; i < filePathsArray.Length; i++)
        {
            //Temp file that's the file name. Will be parsed in later
            string fileName = Path.GetFileName(filePathsArray[i]);

            //Instatiates the deck prefab (it's empty)
            decksOfCards.Add(Instantiate(deckPrefab));

            //Parrents the cards
            decksOfCards[i].transform.parent = deckArea.transform;

            //So the scale doesn't mess up when they spawn in. 
            decksOfCards[i].GetComponent<RectTransform>().localScale = new Vector3(1, 1, 0.33f);

            //Adds in the data to the deck
            decksOfCards[i].GetComponent<DeckValues>().deckLocationAndName = filePathsArray[i];
            decksOfCards[i].GetComponent<DeckValues>().name = fileName;
        }
    }
}

//https://discussions.unity.com/t/solved-loading-image-from-streamingassets/752274/3