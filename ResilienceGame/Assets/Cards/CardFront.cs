using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Visual parts of the card
public class CardFront : MonoBehaviour {
    public bool blueCircle;
    public bool blackCircle;
    public bool purpleCircle;
    public Color color;
    public string title;
    public string description;
    public string flavor;
    public Texture2D background;
    public Texture2D img;

    public Image meepleBgBlack;
    public Image meepleBgBlue;
    public Image meepleBgPurple;

    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI flavorText;
    public TextMeshProUGUI blueCost;
    public TextMeshProUGUI blackCost;
    public TextMeshProUGUI purpleCost;

    public RawImage backgroundImage;
    public RawImage cardImage;

    public void SetTitle(string title) {
        this.title = title;
        titleText.text = title;

    }
    public void SetDescription(string description) {
        this.description = description;
        descriptionText.text = description;
    }
    public void SetFlavor(string flavor) {
        this.flavor = flavor;
        flavorText.text = flavor;
    }
    public void SetBlueCost(int blueCost) {
        blueCircle = true;
        this.blueCost.text = blueCost.ToString();
    }
    public void SetBlackCost(int blackCost) {
        blackCircle = true;
        this.blackCost.text = blackCost.ToString();
    }
    public void SetPurpleCost(int purpleCost) {
        purpleCircle = true;
        this.purpleCost.text = purpleCost.ToString();
    }
    public void SetColor(Color c) {
        color = c;
        backgroundImage.color = c;
    }
    public void SetImage(string path) {
        var imgPath = Path.Combine(Application.streamingAssetsPath, path);
        if (File.Exists(imgPath)) {
            var fileData = File.ReadAllBytes(imgPath);
            img = new Texture2D(256, 256);
            img.LoadImage(fileData);
            cardImage.texture = img;
        }
    }

}
