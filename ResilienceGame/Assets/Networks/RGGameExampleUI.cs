using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RGGameExampleUI : MonoBehaviour
{
    [Header("UI Elements")]
    //TMP_Text cardHistory;
    public Scrollbar scrollbar;
    public TextMeshProUGUI activePlayer;
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI turnText;
    public Canvas cardCanvas;
    public GameObject cardHolder;
    public GameObject cardPlayedHolder;
    public Button endTurnButton;
    public Button[] cards;

    // This is only set on client to the name of the local player
    internal static string localPlayerName;
    internal static int localPlayerID;

    // Server-only cross-reference of connections to player names
    //internal static readonly Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();

    private int localPlayerTeamID = 1; // 0 = red, 1 = blue
    private int teamNum = 2; // The number of teams

   
    string[] red_name = { "System Shutdown", "Disk Wipe", "Ransom", "Phishing", "Brute Force", "Input Capture" };
    string[] blue_name = { "Access Processes", "User Training", "Restrict Web-Based Content", "Pay Ransom", "Data Backup", "User Acount Management" };

  
    public void SetStartTeamInfo(CardPlayer player, int teamID, float funds)
    {
        localPlayerTeamID = teamID;
        
        if (teamID == 0)
        {
            // malicious player goes first
            ShowPlayUI();
            activePlayer.text = "Malicious " + localPlayerName;
            
            foreach (Button card in cards)
            {
                card.GetComponent<Image>().color = Color.red;
            }
        } else
        {
            activePlayer.text = "Resilient " + localPlayerName;
            for (int i = 0; i < cards.Length; i++)
            {
                GetNewCard(i);
            }
            HidePlayUI();
        }

        turnText.text = "Turn: " + GameManager.instance.GetTurn();
    }

   
    void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
       // cardHistory.text += message + "\n";

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }

    // Called by UI element ExitButton.OnClick
    public void ExitButtonOnClick()
    {
        // StopHost calls both StopClient and StopServer
        // StopServer does nothing on remote clients
        NetworkManager.singleton.StopHost();
    }

    void GetNewCard(int index)
    {
        TMP_Text tex = cards[index].transform.Find("CardName").GetComponent<TMP_Text>();

        if (localPlayerTeamID == 0)
        {
            int ri = Random.Range(0, red_name.Length);
            tex.text = red_name[ri];
        }
        else
        {
            int ri = Random.Range(0, blue_name.Length);
            tex.text = blue_name[ri];
        }
    }

    // Need to call this upon card play
    public void PlayCard(int index)
    {
        string message = "plays the <color=";
        if (localPlayerTeamID == 0)
            message += "red";
        else
            message += "blue";
        message += ">" + cards[index].transform.Find("CardName").GetComponent<TMP_Text>().text + "</color>.";

        GetNewCard(0);
    }

    public void ShowPlayUI()
    { 
       
            endTurnButton.gameObject.SetActive(true);
            cardCanvas.gameObject.SetActive(true);
            cardHolder.SetActive(true);
            cardPlayedHolder.SetActive(true);
    }

    public void HidePlayUI()
    {
      
            endTurnButton.gameObject.SetActive(false);
            cardHolder.SetActive(false);
            cardCanvas.gameObject.SetActive(false);
            cardPlayedHolder.SetActive(false);
    }

    public void EndTurn()
    {
        GameManager.instance.EndTurn();
        Debug.Log("manager end turn called!");
    }

    public void ShowAllCards()
    {
        Debug.Log("button for showing all cards clicked");
        GameManager.instance.ShowMyCardsToEverybody();
    }
}
