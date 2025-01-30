using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class DeckValues : MonoBehaviour
{
    //Location of the card deck
    public string deckLocationAndName;
    //Name of the deck
    public string name;
    [SerializeField] private GameObject DeckNameHolder;
    [SerializeField] private GameObject NetworkManager;
    [SerializeField] private TMP_Text nameText;

    // Start is called before the first frame update
    void Start()
    {
        nameText.text = name.Substring(0, name.Length - 4); //Meant to remove.csv from the textMeshPro

        //Finds these objects later when spawned in
        DeckNameHolder = GameObject.FindGameObjectWithTag("DeckNameHolder");
        NetworkManager = GameObject.FindGameObjectWithTag("NetworkManager");
    }

    public void buttonFuncCall()
    {
        //Sicne you can't assign buttons functions to a prefab, the deck prefab manually activates these functions from the game object
        NetworkManager.GetComponent<PickADeckScript>().ToggleDeckCanvas();
        DeckNameHolder.GetComponent<DeckNameHolder>().OpenDeck(deckLocationAndName, name); 
    }
}
