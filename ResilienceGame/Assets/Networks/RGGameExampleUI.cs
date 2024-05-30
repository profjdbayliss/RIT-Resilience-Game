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
    public Scrollbar scrollbar;
    public TextMeshProUGUI activePlayer;
    public TextMeshProUGUI fundsText;
    public TextMeshProUGUI turnText;
    public Canvas cardCanvas;
    public GameObject cardHolder;
    public GameObject cardPlayedHolder;
    public GameObject showCardHolder;
    public Button endTurnButton;

    // This is set on client to the name of the local player
    internal static string localPlayerName;
    internal static int localPlayerID;
  
    public void SetStartTeamInfo(CardPlayer player, int teamID, float funds)
    {        
        if (teamID == 0)
        { 
            activePlayer.text = "Malicious " + localPlayerName;
            cardHolder = GameManager.instance.maliciousActor.handDropZone;
            cardPlayedHolder = GameManager.instance.maliciousActor.cardDropZone;
            ShowPlayUI();
        } else
        {
            activePlayer.text = "Resilient " + localPlayerName;
            cardHolder = GameManager.instance.resiliencePlayer.handDropZone;
            cardPlayedHolder = GameManager.instance.resiliencePlayer.cardDropZone;   
        }

        showCardHolder.SetActive(false);
        ShowPlayUI();
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


    public void ShowPlayUI()
    {      
            endTurnButton.gameObject.SetActive(true);
            cardCanvas.gameObject.SetActive(true);
            cardHolder.SetActive(true);
            cardPlayedHolder.SetActive(true);
            //  to get rid of the show cards command because it's annoying after a while
            showCardHolder.SetActive(false);
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
