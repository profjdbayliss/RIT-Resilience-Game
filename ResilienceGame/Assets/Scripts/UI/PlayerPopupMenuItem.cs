using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPopupMenuItem : MonoBehaviour {
    public CardPlayer Player;

    [Header("UI Elements")]
    public TextMeshProUGUI PlayerName;
    public TextMeshProUGUI DeckSizeText;
    public List<TextMeshProUGUI> ProdPoints;
    public List<TextMeshProUGUI> TransPoints;
    public List<TextMeshProUGUI> DistPoints;
    public List<GameObject> FacilityBoxes;
    public Sprite RedIcon;
    public Image SectorIcon;

    public void SetPlayer(CardPlayer player) {
        Player = player;
        PlayerName.text = Player.playerName;
        if (Player.playerTeam == PlayerTeam.Red) {
            FacilityBoxes.ForEach(box => box.SetActive(false));
            SectorIcon.sprite = RedIcon;
        }
        else {
          //  SectorIcon.sprite = Player.PlayerSector.
        }

    }

    public void UpdatePopup() {
        if (Player == null) return;
        DeckSizeText.text = Player.DeckIDs.Count.ToString();
        if (Player.playerTeam == PlayerTeam.Red) return;

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
