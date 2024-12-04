using SFB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardEditor : MonoBehaviour {
    public enum CardMethods {
        DrawAndDiscardCards,
        ShuffleAndDrawCards,
        ReturnHandToDeckAndDraw,
        AddEffect,
        RemoveEffect,
        SpreadEffect,
        SelectFacilitiesAddRemoveEffect,
        ReduceTurnsLeftByBackdoor,
        TemporaryReductionOfTurnsLeft,
        CancelTemporaryReductionOfTurns,
        BackdoorCheckNetworkRestore,
        ConvertFortifyToBackdoor,
        IncreaseTurnsDuringPeace,
        IncColorlessMeeplesRoundReduction,
        ChangeMeepleAmount,
        IncreaseOvertimeAmount,
        ShuffleCardsFromDiscard,
        ReduceCardCost,
        NWIncOvertimeAmount,
        NWShuffleFromDiscard,
        ChangeAllFacPointsBySectorType,
        ChangeTransFacPointsAllSectors,
        CheckAllSectorsChangeMeepleAmtMulti,
        IncreaseBaseMaxMeeples
    }


    [SerializeField] private TextMeshProUGUI deckTitle;
    [SerializeField] private GameObject editorCardPrefab;
    [SerializeField] private RectTransform editorCardContainer;
    [SerializeField] private RectTransform cardContainerParent;
    [SerializeField] private RectTransform containerOpenPos;
    [SerializeField] private RectTransform containerClosedPos;
    [SerializeField] private GameObject editCardParent;
    [SerializeField] private EditorCard selectedCard;
    private Coroutine moveCoroutine;
    [SerializeField] private RectTransform toggleBtnTransform;

    [Header("Card Edit Fields")]
    [SerializeField] private TMP_InputField titleInput;
    [SerializeField] private TMP_InputField dupeInput;
    [SerializeField] private TMP_Dropdown actionDropdown;
    [SerializeField] private TMP_Dropdown targetDropdown;
    [SerializeField] private TMP_InputField cardsDrawn;
    [SerializeField] private TMP_InputField cardsDiscarded;
    [SerializeField] private TMP_InputField team;
    [SerializeField] private TMP_InputField sectorsAffected;
    [SerializeField] private TMP_InputField numTargets;
    [SerializeField] private TMP_InputField blackCost;
    [SerializeField] private TMP_InputField blueCost;
    [SerializeField] private TMP_InputField purpleCost;
    [SerializeField] private TMP_InputField duration;
    [SerializeField] private TMP_InputField diceRoll;
    [SerializeField] private TMP_InputField description;
    [SerializeField] private TMP_InputField flavor;




    private List<EditorCard> cards = new List<EditorCard>();
    private const string DEFAULT_NAME = "SectorDownCards.csv";
    private string setName;
    string headers;
    bool isOpen = true;
    bool cardSelected = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {

    }

    #region Card Selection

    public void SetSelectedCard(EditorCard card) {
        if (!cardSelected) {
            editCardParent.SetActive(true);
        }
        cardSelected = true;
        selectedCard.cardData = card.cardData;
        selectedCard.UpdateCardVisuals();
        SetEditFieldsForNewCard(card);
    }

    public void SetEditFieldsForNewCard(EditorCard card) {
        titleInput.text = card.cardData.title;
        dupeInput.text = card.cardData.duplication.ToString();
        cardsDrawn.text = card.cardData.cardsDrawn.ToString();
        cardsDiscarded.text = card.cardData.cardsRemoved.ToString();
        team.text = card.cardData.team;
        sectorsAffected.text = card.cardData.sectorsAffected;
        numTargets.text = card.cardData.targetAmt.ToString();
        blackCost.text = card.cardData.blackCost.ToString();
        blueCost.text = card.cardData.blueCost.ToString();
        purpleCost.text = card.cardData.purpleCost.ToString();
        duration.text = card.cardData.duration.ToString();
        diceRoll.text = card.cardData.diceRoll.ToString();
        description.text = card.cardData.description;
        flavor.text = card.cardData.flavourText;
        SetDropdownOptionByText(card.cardData.methods, actionDropdown);
        SetDropdownOptionByText(card.cardData.target.ToString(), targetDropdown);
    }

    public void SetDropdownOptionByText(string optionText, TMP_Dropdown dropdown) {
        for (int i = 0; i < dropdown.options.Count; i++) {
            if (dropdown.options[i].text == optionText) {
                dropdown.value = i; // Set the dropdown to the matching index
                dropdown.RefreshShownValue(); // Update the visual display
                return;
            }
        }

        Debug.LogWarning($"Option with text '{optionText}' not found in the dropdown.");
    }


    #endregion

    #region card grid menu
    public void ToggleMenuOpen() {
        isOpen = !isOpen;
        MoveCardContainer(isOpen);
    }

    public void MoveCardContainer(bool open) {
        // Stop any existing movement coroutine
        if (moveCoroutine != null) {
            StopCoroutine(moveCoroutine);
        }
        toggleBtnTransform.rotation = Quaternion.Euler(0, 0, !isOpen ? 0 : 180);
        // Start a new movement coroutine
        moveCoroutine = StartCoroutine(MoveContainerCoroutine(open));
    }

    private IEnumerator MoveContainerCoroutine(bool open) {
        float duration = 0.5f; // Duration of the animation
        float elapsed = 0f;

        Vector3 start = cardContainerParent.position;
        Vector3 target = open ? containerOpenPos.position : containerClosedPos.position;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Apply easing using SmoothStep
            t = Mathf.SmoothStep(0, 1, t);

            cardContainerParent.position = Vector3.Lerp(start, target, t);
            yield return null;
        }

        // Ensure final position is precise
        cardContainerParent.position = target;
    }

    #endregion

    #region File IO

    public void OpenDeck() {
        // Check if the application supports file dialogs
        string s = "";
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.OSXPlayer) {
            // Show the file browser dialog
            string[] paths = StandaloneFileBrowser.OpenFilePanel("Open CSV File", "", "csv", false);

            if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0])) {
                string filePath = paths[0];
                try {
                    // Read the CSV file contents
                    var rawLines = File.ReadAllLines(filePath);
                    headers = rawLines[0];
                    string[] lines = rawLines.Skip(1).ToArray();

                    // Process each line (for example, split by commas)
                    foreach (string line in lines) {


                        var editorCard = Instantiate(editorCardPrefab, editorCardContainer).GetComponent<EditorCard>();
                        editorCard.onClick += () => SetSelectedCard(editorCard);
                        editorCard.Init(line);
                        cards.Add(editorCard);
                    }
                    setName = Path.GetFileName(filePath);
                    deckTitle.text = setName;
                    
                }
                catch (Exception e) {
                    Debug.LogError($"Error reading CSV file: {e.Message}");
                }
            }
            else {
                Debug.Log("No file selected.");
            }
        }
        else {
            Debug.LogError("File browser dialogs are not supported on this platform.");
        }
    }
    public void Save() {
        if (string.IsNullOrEmpty(setName) || setName == DEFAULT_NAME) {
            // Prevent overwriting the default file
            Debug.LogWarning("Default file cannot be overwritten. Use Save As instead.");
            return;
        }

        try {
            string filePath = Path.Combine(Application.persistentDataPath, setName);
            WriteToFile(filePath);
            Debug.Log($"File saved: {filePath}");
        }
        catch (Exception e) {
            Debug.LogError($"Error saving file: {e.Message}");
        }
    }

    public void SaveAs() {
        // Open the file browser for the user to select a location and name
        string path = StandaloneFileBrowser.SaveFilePanel("Save As", "", "NewDeck", "csv");

        if (!string.IsNullOrEmpty(path)) {
            try {
                WriteToFile(path);
                setName = Path.GetFileName(path);
                deckTitle.text = setName;
                Debug.Log($"File saved as: {path}");
            }
            catch (Exception e) {
                Debug.LogError($"Error saving file: {e.Message}");
            }
        }
        else {
            Debug.Log("Save operation canceled.");
        }
    }

    private void WriteToFile(string filePath) {
        // Combine headers and card data into one list
        List<string> allLines = new List<string> { headers };
        allLines.AddRange(cards.Select(card => card.data));

        // Write all lines to the specified file
        File.WriteAllLines(filePath, allLines);
    }

    #endregion

}
