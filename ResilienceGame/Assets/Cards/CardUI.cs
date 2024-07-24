using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    public Card card;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public RawImage cardImage;
    public Image cardBackground;
    public TMP_Text blueCostText;
    public TMP_Text blackCostText;
    public TMP_Text purpleCostText;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCardUI()
    {
        titleText.text = card.cardTitle;
        descriptionText.text = card.cardDescription;
        cardBackground.color = card.backgroundColor;

        int blueCount = card.cardCost.Count(meeple => meeple.type == "Blue");
        int blackCount = card.cardCost.Count(meeple => meeple.type == "Black");
        int purpleCount = card.cardCost.Count(meeple => meeple.type == "Purple");

        blueCostText.text = blueCount.ToString();
        blackCostText.text = blackCount.ToString();
        purpleCostText.text = purpleCount.ToString();

    }

    public void SetCardUI(Card card)
    {
        this.card = card;
        SetCardUI();
    }
}
