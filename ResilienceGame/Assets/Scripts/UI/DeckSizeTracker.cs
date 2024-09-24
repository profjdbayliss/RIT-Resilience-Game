using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckSizeTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI opponentDeckText;
    [SerializeField] private TextMeshProUGUI playerDeckText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdatePlayerDeckSize(int size) {
        playerDeckText.text = size.ToString();
    }
    public void UpdateOpponentDeckSize(int size) {
        opponentDeckText.text = size.ToString();
    }

}
