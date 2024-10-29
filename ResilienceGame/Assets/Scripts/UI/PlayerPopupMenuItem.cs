using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPopupMenuItem : MonoBehaviour {
    public CardPlayer Player;
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;
    [Header("UI Elements")]
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI DeckSizeText;
    public List<TextMeshProUGUI> ProdPoints;
    public List<TextMeshProUGUI> TransPoints;
    public List<TextMeshProUGUI> DistPoints;
    public List<GameObject> FacilityBoxes;
    public Sprite RedIcon;
    public Image SectorIcon;
    public Image SectorXIcon;
    private Image popupBg;
    private bool initSprite = false;

    public void SetPlayer(CardPlayer player) {
        Player = player;
        PlayerName.text = Player.playerName;
        popupBg = GetComponent<Image>();
        if (Player.playerTeam == PlayerTeam.Red) {
            FacilityBoxes.ForEach(box => box.SetActive(false));
            SectorIcon.sprite = RedIcon;
            popupBg.color = redColor;
        }
        else {
            Debug.Log(player.playerName);
            if (player.PlayerSector == null) return;
            SectorIcon.sprite = Player.PlayerSector.SectorIcon;
            popupBg.color = blueColor;
            initSprite = true;
        }
        UpdatePopup();

    }

    public void UpdatePopup() {
        if (Player == null) return;
        DeckSizeText.text = Player.DeckIDs.Count.ToString();
        if (Player.playerTeam == PlayerTeam.Red) return;
        Debug.Log($"Updating player popup menu item for {Player.playerName}");

        if (Player.PlayerSector == null) {
            Debug.LogError("Player sector is null");
        }
        if (!initSprite) {
            SectorIcon.sprite = Player.PlayerSector.SectorIcon;
            popupBg.color = blueColor;
            initSprite = true;
        }

        SectorXIcon.enabled = Player.PlayerSector.IsDown;

        for (int i = 0; i < ProdPoints.Count; i++) {
            ProdPoints[i].text = Player.PlayerSector.facilities[0].Points[i].ToString();
        }
        for (int i = 0; i < TransPoints.Count; i++) {
            TransPoints[i].text = Player.PlayerSector.facilities[1].Points[i].ToString();
        }
        for (int i = 0; i < DistPoints.Count; i++) {
            DistPoints[i].text = Player.PlayerSector.facilities[2].Points[i].ToString();
        }
    }



}
