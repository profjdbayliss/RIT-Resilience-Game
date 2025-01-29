using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckValues : MonoBehaviour
{
    //Location of the card deck
    public string deckLocationAndName;
    public string name;
    [SerializeField] private TMP_Text nameText;

    //Note: Depending on how the code goes, start and update could possibily be removed, so whomever is reading this, if the card selector is up n running without issue, and those are still there. Go ahead and delete them for me.
    // Start is called before the first frame update
    void Start()
    {
        nameText.text = name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
