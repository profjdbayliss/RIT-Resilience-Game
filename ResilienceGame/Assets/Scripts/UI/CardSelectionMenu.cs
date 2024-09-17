using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectionMenu : MonoBehaviour {
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public bool IsMenuActive { get; private set; } = false;
    public bool IsInitialized { get; private set; } = false;

    [SerializeField] Color blueButtonColor, redButtonColor;

    // Function to create buttons for each card in the deck
    public void CreateCardButtons(List<Card> deck) {
        IsInitialized = true;
        // Clear any existing buttons first
        foreach (Transform child in buttonContainer) {
            Destroy(child.gameObject);
        }

        // Iterate through each card in the deck
        foreach (Card card in deck) {
            // Instantiate a new button from the prefab
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);

            // Set the text on the button 
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            //buttonText.text = card.front.title;
            buttonText.text = card.data.name;
            newButton.GetComponent<Image>().color = card.DeckName.ToLower().Trim() == "blue" ? blueButtonColor : redButtonColor;
            // Assign the card to be drawn when the button is clicked
            var button = newButton.GetComponent<Button>();
            button.onClick.AddListener(() => OnCardButtonPressed(card));
        }
    }

    // Function to handle the card being drawn when the button is pressed
    private void OnCardButtonPressed(Card selectedCard) {
        GameManager.instance.actualPlayer.ForceDrawSpecificCard(selectedCard.data.cardID);
        DisableMenu();
    }

    public void EnableMenu(List<Card> deck) {
        IsMenuActive = true;
        if (!IsInitialized) CreateCardButtons(deck);
        buttonContainer.gameObject.SetActive(true);
    }
    public void DisableMenu() {
        IsMenuActive = false;
        buttonContainer.gameObject.SetActive(false);
    }
}
