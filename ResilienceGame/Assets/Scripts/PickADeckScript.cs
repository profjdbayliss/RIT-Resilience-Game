using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PickADeckScript : MonoBehaviour
{
    [SerializeField] private GameObject connectPickACardCanvas;
    [SerializeField] private List<GameObject> decksOfCards;
    public string folderPath;
    public string[] filePaths;

    private bool isPickACardCanvasActive;
    // Start is called before the first frame update
    void Start()
    {
        folderPath = Application.streamingAssetsPath;  //Get path of folder
        filePaths = Directory.GetFiles(folderPath, "*.csv"); // Get all files of type .csv in this folder

        isPickACardCanvasActive = false;
        connectPickACardCanvas.SetActive(isPickACardCanvasActive);

        decksOfCards = new List<GameObject>();
    }

    public void ToggleConnectHelpCanvas()
    {
        isPickACardCanvasActive = !isPickACardCanvasActive;
        connectPickACardCanvas.SetActive(isPickACardCanvasActive);
    }

    public void refreshCards()
    {
        decksOfCards.Clear();
/*
        foreach (.csv in fileInfo)
        {

        }
*/
    }
}

//https://discussions.unity.com/t/solved-loading-image-from-streamingassets/752274/3