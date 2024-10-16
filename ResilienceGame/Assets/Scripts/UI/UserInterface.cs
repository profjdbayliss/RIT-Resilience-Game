using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInterface : MonoBehaviour {

    public CardPlayer actualPlayer;
    public TextMeshProUGUI[] meeplesAmountText;
    [SerializeField] private Button[] meepleButtons;

    public GameObject discardDropZone;
    public GameObject handDropZone;
    public GameObject opponentDropZone;

    [Header("Drag and Drop")]
    public GameObject hoveredDropLocation;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void UpdateMeepleAmountUI(float blackMeeples, float blueMeeples, float purpleMeeples) {
        meeplesAmountText[0].text = blackMeeples.ToString();
        meeplesAmountText[1].text = blueMeeples.ToString();
        meeplesAmountText[2].text = purpleMeeples.ToString();
    }
    public void EnableMeepleButtons() {
        foreach (Button button in meepleButtons) {
            button.interactable = true;
        }
    }
    public void DisableMeepleButtons() {
        foreach (Button button in meepleButtons) {
            button.interactable = false;
        }
    }
    //called by the buttons in the sector canvas
    public void TryButtonSpendMeeple(int index) {
        if (meepleButtons[index].interactable) {
            actualPlayer.SpendMeepleWithButton(index);
        }
    }
    public void EnableDropZone() {
        hoveredDropLocation.SetActive(true);
    }
    public void DisableDropZone() {
        hoveredDropLocation.SetActive(false);
    }
    public void EnableMeepleButtonByIndex(int index) {
        meepleButtons[index].interactable = true;
    }
    public void DisableMeepleButtonByIndex(int index) {
        meepleButtons[index].interactable = false;
    }
}
