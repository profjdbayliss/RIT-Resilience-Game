using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardViewer : MonoBehaviour
{
    public static CardViewer instance;

    public FileBrowser filebrowser;
    public List<CardForEditor> cards = new List<CardForEditor>();

    [Header("UI Objects")]
    public GameObject fileSelectionObject;
    public GameObject cardViewerObject;
    public GameObject cardEditorObject;

    [Header("Card View")]
    public GameObject cardViewPrefab;
    public GameObject cardViewContentParent;
    public GameObject addNewCardButtonPrefab;

    [Header("Card Preview")]
    public Image titleBackground;
    public TMP_Text titleText;
    public RawImage cardImage;
    public TMP_Text impactText;
    public TMP_Text descriptionText;
    public TMP_Text costText;

    [Header("Card Editor Inputs")]
    public TMP_InputField teamInput;
    public TMP_InputField titleInput;
    public TMP_InputField costInput;
    public TMP_InputField imageInput;
    public Button imageSelectionButton;
    public TMP_InputField descriptionInput;
    public TMP_InputField impactInput;
    public TMP_InputField percentInput;
    public TMP_InputField spreadChangeInput;
    public TMP_InputField durationInput;
    public TMP_InputField delayInput;
    public TMP_InputField targetCountInput;
    public TMP_InputField targetTypeInput;
    public TMP_InputField cardCountInput;
    public TMP_InputField typeInput;

    public TMP_Text warningText;

    [Header("Color Setting For Teams")]
    public Color redTeamColor;
    public Color blueTeamColor;

    private CardForEditor selectedCard;
    private CardForEditor editingCard = new CardForEditor();

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        teamInput.onEndEdit.AddListener(UpdateTeam);
        titleInput.onEndEdit.AddListener(UpdateTitle);
        costInput.onEndEdit.AddListener(UpdateCost);
        imageInput.onEndEdit.AddListener(UpdateImage);
        descriptionInput.onEndEdit.AddListener(UpdateDescription);
        impactInput.onEndEdit.AddListener(UpdateImpact);
        percentInput.onEndEdit.AddListener(UpdatePercent);
        spreadChangeInput.onEndEdit.AddListener(UpdateSpreadChange);
        durationInput.onEndEdit.AddListener(UpdateDuration);
        delayInput.onEndEdit.AddListener(UpdateDelay);
        targetCountInput.onEndEdit.AddListener(UpdateTargetCount);
        targetTypeInput.onEndEdit.AddListener(UpdateTargetType);
        cardCountInput.onEndEdit.AddListener(UpdateCardCount);
        typeInput.onEndEdit.AddListener(UpdateType);
    }

    public void OpenFile()
    {
        cards = LoadCsv(filebrowser.filePath);

        if(cards.Count > 0)
        {
            //Go to the viewer panel
            fileSelectionObject.SetActive(false);
            cardViewerObject.SetActive(true);

            //Clear the content
            foreach (Transform child in cardViewContentParent.transform)
            {
                Destroy(child.gameObject);
            }

            //Generate "Add new card" button
            Instantiate(addNewCardButtonPrefab, cardViewContentParent.transform);

            //Generate cards
            for (int i = 0; i < cards.Count; i++)
            {
                GameObject cardViewObject = Instantiate(cardViewPrefab, cardViewContentParent.transform);
                string imageFolderDirectory = GetDirectoryFromFilePath(filebrowser.filePath) + "\\Images\\";
                cardViewObject.GetComponent<CardObjectForView>().Initialize(cards[i], imageFolderDirectory);
            }

            cardViewContentParent.GetComponent<DynamicContentAdjuster>().AdjustContentSize();
        }
        else
        {
            Debug.LogError("Error File: " + filebrowser.filePath);
        }
    }

    private List<CardForEditor> LoadCsv(string path)
    {
        List<CardForEditor> cards = new List<CardForEditor>();

        // Ensure the file exists
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
            return cards;
        }

        if (!IsValidCsvFile(path))
        {
            Debug.LogError("File is not correct: " + path);
            return cards;
        }

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] entries = lines[i].Split(',');

            // Ensure the row has enough entries
            if (entries.Length >= 14)
            {
                CardForEditor card = new CardForEditor
                {
                    team = entries[0],
                    title = entries[1],
                    cost = int.Parse(entries[2]),
                    image = entries[3],
                    description = entries[4],
                    impact = entries[5],
                    percent = int.Parse(entries[6]),
                    spreadChange = int.Parse(entries[7]),
                    duration = int.Parse(entries[8]),
                    delay = int.Parse(entries[9]),
                    targetCount = int.Parse(entries[10]),
                    targetType = entries[11],
                    cardCount = int.Parse(entries[12]),
                    type = entries[13]
                };

                cards.Add(card);
            }
        }

        return cards;
    }

    public static bool IsCsvFile(string path)
    {
        return string.Equals(Path.GetExtension(path), ".csv", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsValidCsvFile(string path)
    {
        // First, check if it's a .csv file based on the extension
        if (!IsCsvFile(path))
            return false;

        // Attempt to read the file to see if it can be processed as a CSV
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue; // Empty lines are okay

                    string[] entries = line.Split(',');

                    // This is a basic check. If there are no commas, it might not be a valid CSV.
                    // You can add more rules here, depending on the specifics of your expected CSV format.
                    if (entries.Length < 1)
                        return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading CSV file: {ex.Message}");
            return false;
        }
    }
    public string GetDirectoryFromFilePath(string filePath)
    {
        return Path.GetDirectoryName(filePath);
    }

    public void ShowCardEditor()
    {
        cardViewerObject.SetActive(false);
        cardEditorObject.SetActive(true);
        warningText.text = "";
    }

    public void ShowCardViewer()
    {
        cardViewerObject.SetActive(true);
        cardEditorObject.SetActive(false);
    }

    public void ClearSelectedCard()
    {
        selectedCard = null;
        editingCard = new CardForEditor();
        ClearPreviewCard();
    }

    public void UpdateSelectedCard(CardForEditor card)
    {
        selectedCard = card;
        editingCard = new CardForEditor(selectedCard);
        InitPreviewCard();
    }

    public void ClearPreviewCard()
    {
        //Preview card
        titleBackground.color = Color.white;
        titleText.text = "Title";
        cardImage.texture = null;
        impactText.text = "Impact";
        descriptionText.text = "Description";
        costText.text = "0";

        //InputFields
        teamInput.text = "";
        titleInput.text = "";
        costInput.text = "";
        imageInput.text = "";
        descriptionInput.text = "";
        impactInput.text = "";
        percentInput.text = "";
        spreadChangeInput.text = "";
        durationInput.text = "";
        delayInput.text = "";
        targetCountInput.text = "";
        targetTypeInput.text = "";
        cardCountInput.text = "";
        typeInput.text = "";
    }

    public void InitPreviewCard()
    {
        //Preview card
        if (editingCard.team.Equals("Red"))
        {
            titleBackground.color = redTeamColor;
        }
        else if (editingCard.team.Equals("Blue"))
        {
            titleBackground.color = blueTeamColor;
        }
        else
        {
            titleBackground.color = Color.white;
            Debug.LogError("Undefined Team: " + editingCard.team);
        }

        titleText.text = editingCard.title;
        string imageFolderDirectory = GetDirectoryFromFilePath(filebrowser.filePath) + "\\Images\\";
        LoadImageIntoRawImage(imageFolderDirectory + editingCard.image);
        impactText.text = editingCard.impact;
        descriptionText.text = editingCard.description;
        costText.text = editingCard.cost.ToString();

        //InputFields
        teamInput.text = editingCard.team;
        titleInput.text = editingCard.title;
        costInput.text = editingCard.cost.ToString();
        imageInput.text = editingCard.image;
        descriptionInput.text = editingCard.description;
        impactInput.text = editingCard.impact;
        percentInput.text = editingCard.percent.ToString();
        spreadChangeInput.text = editingCard.spreadChange.ToString();
        durationInput.text = editingCard.duration.ToString();
        delayInput.text = editingCard.delay.ToString();
        targetCountInput.text = editingCard.targetCount.ToString();
        targetTypeInput.text = editingCard.targetType;
        cardCountInput.text = editingCard.cardCount.ToString();
        typeInput.text = editingCard.type;
    }

    public void UpdatePreviewCard()
    {
        //Preview card
        if (editingCard.team.Equals("Red"))
        {
            titleBackground.color = redTeamColor;
        }
        else if (editingCard.team.Equals("Blue"))
        {
            titleBackground.color = blueTeamColor;
        }

        titleText.text = editingCard.title;
        if(titleText.text.Equals(""))
        {
            titleText.text = "Title";
        }
        string imageFolderDirectory = GetDirectoryFromFilePath(filebrowser.filePath) + "\\Images\\";
        if(!editingCard.image.Equals(""))
        {
            bool isFoundImage = LoadImageIntoRawImage(imageFolderDirectory + editingCard.image);
            if (!isFoundImage)
            {
                warningText.text = "Can't find the image.";
            }
        }
        impactText.text = editingCard.impact;
        if (impactText.text.Equals(""))
        {
            impactText.text = "Impact";
        }
        descriptionText.text = editingCard.description;
        if (descriptionText.text.Equals(""))
        {
            descriptionText.text = "Description";
        }
        costText.text = editingCard.cost.ToString();
        if (costText.text.Equals(""))
        {
            costText.text = "0";
        }
    }

    public bool LoadImageIntoRawImage(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError("Image file not found at path: " + imagePath);
            return false;
        }

        // Load the image bytes
        byte[] imageBytes;
        try
        {
            imageBytes = File.ReadAllBytes(imagePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to read image bytes: " + e.Message);
            return false;
        }

        // Create a texture and assign the loaded bytes
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            // If successfully loaded, assign the texture to the RawImage
            cardImage.texture = texture;
            return true;
        }
        else
        {
            Debug.LogError("Failed to load image at path: " + imagePath);
            return false;
        }
    }

    public void DeleteCard()
    {
        if(selectedCard == null)
        {
            warningText.text = "You can only delete an existing card.";
            return;
        }

        RemoveCardFromCsv(filebrowser.filePath, selectedCard.title);
        ShowCardViewer();
        OpenFile();
    }

    public void SaveCard()
    {
        if (!AreRequiredFieldsFilled())
        {
            warningText.text = "All the fields need to be filled except for the Type.";
            return;
        }

        if(selectedCard == null && CheckIfCardExists(filebrowser.filePath, editingCard.title))
        {
            warningText.text = "Card with same title already exists.";
            return;
        }

        if(selectedCard != null)
        {
            if(!selectedCard.title.Equals(editingCard.title) && CheckIfCardExists(filebrowser.filePath, editingCard.title))
            {
                warningText.text = "Card with same title already exists.";
                return;
            }
            RemoveCardFromCsv(filebrowser.filePath, selectedCard.title);
        }

        AddCardToCsv(filebrowser.filePath, editingCard);

        ShowCardViewer();
        OpenFile();
    }

    private void UpdateTeam(string value)
    {
        editingCard.team = value;
        UpdatePreviewCard();
    }

    private void UpdateTitle(string value)
    {
        editingCard.title = value;
        UpdatePreviewCard();
    }

    private void UpdateCost(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.cost = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("Cost must be an integer.");
        }
    }

    private void UpdateImage(string value)
    {
        editingCard.image = value;
        UpdatePreviewCard();
    }

    public void UpdateImageWithFileBrowser()
    {
        editingCard.image = filebrowser.imageFileName;
        imageInput.text = filebrowser.imageFileName;
        UpdatePreviewCard();
    }

    private void UpdateDescription(string value)
    {
        editingCard.description = value;
        UpdatePreviewCard();
    }

    private void UpdateImpact(string value)
    {
        editingCard.impact = value;
        UpdatePreviewCard();
    }

    private void UpdatePercent(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.percent = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("Percent must be an integer.");
        }
    }

    private void UpdateSpreadChange(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.spreadChange = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("SpreadChange must be an integer.");
        }
    }

    private void UpdateDuration(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.duration = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("Duration must be an integer.");
        }
    }

    private void UpdateDelay(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.delay = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("Delay must be an integer.");
        }
    }

    private void UpdateTargetCount(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.targetCount = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("TargetCount must be an integer.");
        }
    }

    private void UpdateTargetType(string value)
    {
        editingCard.targetType = value;
        UpdatePreviewCard();
    }

    private void UpdateCardCount(string value)
    {
        if (int.TryParse(value, out int result))
        {
            editingCard.cardCount = result;
            UpdatePreviewCard();
        }
        else
        {
            Debug.LogWarning("CardCount must be an integer.");
        }
    }

    private void UpdateType(string value)
    {
        editingCard.type = value;
        UpdatePreviewCard();
    }

    public bool AreRequiredFieldsFilled()
    {
        if (string.IsNullOrEmpty(teamInput.text)) return false;
        if (string.IsNullOrEmpty(titleInput.text)) return false;
        if (string.IsNullOrEmpty(costInput.text)) return false;
        if (string.IsNullOrEmpty(imageInput.text)) return false;
        if (string.IsNullOrEmpty(descriptionInput.text)) return false;
        if (string.IsNullOrEmpty(impactInput.text)) return false;
        if (string.IsNullOrEmpty(percentInput.text)) return false;
        if (string.IsNullOrEmpty(spreadChangeInput.text)) return false;
        if (string.IsNullOrEmpty(durationInput.text)) return false;
        if (string.IsNullOrEmpty(delayInput.text)) return false;
        if (string.IsNullOrEmpty(targetCountInput.text)) return false;
        if (string.IsNullOrEmpty(targetTypeInput.text)) return false;
        if (string.IsNullOrEmpty(cardCountInput.text)) return false;

        // typeInput is optional, so we don't check it here

        return true;
    }

    public void AddCardToCsv(string path, CardForEditor card)
    {
        // Ensure the file exists
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
            return;
        }

        // Ensure the file is a valid .csv file
        if (!IsValidCsvFile(path))
        {
            Debug.LogError("File is not a valid .csv file: " + path);
            return;
        }

        try
        {
            // Open the file in append mode
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                // Convert the card to a CSV line and write it to the file
                string line = CardToCsvLine(card);
                sw.WriteLine(line);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("An error occurred while writing to the file: " + e.Message);
        }
    }

    private string CardToCsvLine(CardForEditor card)
    {
        // Convert the card properties to a CSV line (assuming no commas in the string values)
        return $"{card.team},{card.title},{card.cost},{card.image},{card.description},{card.impact},{card.percent},{card.spreadChange},{card.duration},{card.delay},{card.targetCount},{card.targetType},{card.cardCount},{card.type}";
    }

    public void RemoveCardFromCsv(string path, string title)
    {
        // Ensure the file exists
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
            return;
        }

        // Ensure the file is a valid .csv file
        if (!IsValidCsvFile(path))
        {
            Debug.LogError("File is not a valid .csv file: " + path);
            return;
        }

        try
        {
            // Read all lines from the file
            List<string> lines = new List<string>(File.ReadAllLines(path));
            bool cardFound = false;

            // Iterate through the lines to find a card with the given title
            for (int i = 0; i < lines.Count; i++)
            {
                string[] entries = lines[i].Split(',');

                // Assuming the title is in the second element (index 1)
                if (entries.Length > 1 && entries[1].Trim().Equals(title))
                {
                    // Found the matching card, remove it from the list
                    lines.RemoveAt(i);
                    cardFound = true;
                    break; // Assuming titles are unique, exit the loop after finding one
                }
            }

            if (cardFound)
            {
                // Write the updated list back to the file
                File.WriteAllLines(path, lines);
                //Debug.Log("Card has been removed from the .csv file");
            }
            else
            {
                //Debug.LogWarning("No card found with the title: " + title);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("An error occurred while writing to the file: " + e.Message);
        }
    }

    public bool CheckIfCardExists(string path, string title)
    {
        // Ensure the file exists
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist: " + path);
            return false;
        }

        // Ensure the file is a valid .csv file
        if (!IsValidCsvFile(path))
        {
            Debug.LogError("File is not a valid .csv file: " + path);
            return false;
        }

        try
        {
            // Read all lines from the file
            string[] lines = File.ReadAllLines(path);

            // Iterate through the lines to find a card with the given title
            foreach (var line in lines)
            {
                string[] entries = line.Split(',');

                // Assuming the title is in the second element (index 1)
                if (entries.Length > 1 && entries[1].Trim().Equals(title))
                {
                    //Debug.Log("Card with title: " + title + " found in the CSV file.");
                    return true;
                }
            }

            //Debug.Log("No card found with the title: " + title);
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("An error occurred while reading the file: " + e.Message);
            return false;
        }
    }

    public string CopyImageAndReturnFileName(string imagePath)
    {
        // Ensure the image file exists
        if (!File.Exists(imagePath))
        {
            Debug.LogError("File does not exist: " + imagePath);
            return "";
        }

        // Get the directory for images
        string imageFolderDirectory = GetDirectoryFromFilePath(filebrowser.filePath) + "\\Images\\";

        // Ensure the target directory exists
        if (!Directory.Exists(imageFolderDirectory))
        {
            Directory.CreateDirectory(imageFolderDirectory);
        }

        // Extract the file name with extension
        string fileNameWithExtension = Path.GetFileName(imagePath);

        // Build the target file path
        string targetFilePath = Path.Combine(imageFolderDirectory, fileNameWithExtension);

        if (!imagePath.Equals(targetFilePath))
        {
            // Copy the file
            File.Copy(imagePath, targetFilePath, true);
        }


        return fileNameWithExtension;
    }

}
