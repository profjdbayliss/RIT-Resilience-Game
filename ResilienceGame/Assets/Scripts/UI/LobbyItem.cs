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
    [SerializeField] private GameObject PleaseRefresh;
    public Image backgroundImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetPlayerNameAndTeam(string name, PlayerTeam team) { // Called when player disconnects or when host presses "begin"
        PlayerName.text = name;
        PlayerName.enabled = true;
        Debug.Log($"is red? {team == PlayerTeam.Red}");
        Debug.Log($"is blue? {team == PlayerTeam.Blue}");
        PleaseRefresh.SetActive(false);
        if (team == PlayerTeam.Red) {
            backgroundImage.color = redColor;
        }
        else if (team == PlayerTeam.Blue) {
            backgroundImage.color = blueColor;
        }
        else
        {
            SetMissingPlayer();
        }
    }

    public void SetMissingPlayer() {
        PleaseRefresh.SetActive(true);
        PlayerName.enabled = false;
        backgroundImage.color = missingPlayerColor;
    }
}
