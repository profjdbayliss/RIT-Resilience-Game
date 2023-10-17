using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardObjectForView : MonoBehaviour
{
    [Header("UI Elements")]
    public Image titleBackground;
    public TMP_Text titleText;
    public RawImage cardImage;
    public TMP_Text impactText;
    public TMP_Text descriptionText;
    public TMP_Text costText;

    [Header("Color Setting For Teams")]
    public Color redTeamColor;
    public Color blueTeamColor;

    public void Initialize(string team, string title, string cardImagePath, string impact, string description, int cost)
    {
        if (team.Equals("Red"))
        {
            titleBackground.color = redTeamColor;
        }
        else if (team.Equals("Blue"))
        {
            titleBackground.color = blueTeamColor;
        }
        else
        {
            titleBackground.color = Color.white;
            Debug.LogError("Undefined Team: " + team);
        }

        titleText.text = title;
        LoadImageIntoRawImage(cardImagePath);
        impactText.text = impact;
        descriptionText.text = description;
        costText.text = cost.ToString();
    }

    public void LoadImageIntoRawImage(string imagePath)
    {
        // Load the image bytes
        byte[] imageBytes = File.ReadAllBytes(imagePath);

        // Create a texture and assign the loaded bytes
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            // If successfully loaded, assign the texture to the RawImage
            cardImage.texture = texture;
        }
        else
        {
            Debug.LogError("Failed to load image at path: " + imagePath);
        }
    }
}
