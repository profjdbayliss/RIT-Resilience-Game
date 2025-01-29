using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

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
        nameText.text = name;

        DeckNameHolder = GameObject.FindGameObjectWithTag("DeckNameHolder");
        NetworkManager = GameObject.FindGameObjectWithTag("NetworkManager");
    }

    public void buttonFuncCall()
    {
        NetworkManager.GetComponent<PickADeckScript>().ToggleDeckCanvas();
        DeckNameHolder.GetComponent<DeckNameHolder>().OpenDeck(deckLocationAndName, name); 
    }
}
