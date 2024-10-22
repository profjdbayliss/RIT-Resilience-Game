using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;
    [SerializeField] private Color missingPlayerColor;
    [Header("UI Elements")]
    public TextMeshProUGUI PlayerName;
    public Image backgroundImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetPlayerNameAndTeam(string name, PlayerTeam team) {
        PlayerName.text = name;
        PlayerName.enabled = true;
        if (team == PlayerTeam.Red) {
            backgroundImage.color = redColor;
        }
        else {
            backgroundImage.color = blueColor;
        }
    }
    public void SetMissingPlayer() {
        PlayerName.enabled = false;
        backgroundImage.color = missingPlayerColor;
    }
}
