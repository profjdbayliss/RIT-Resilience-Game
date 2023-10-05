using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RGGameExampleUI : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] TMP_Text cardHistory;
    [SerializeField] Scrollbar scrollbar;
    [SerializeField] TMP_Text waitingText;
    [SerializeField] Canvas cardCanvas;
    [SerializeField] Button endTurnButton;
    [SerializeField] Button[] cards;

    // This is only set on client to the name of the local player
    internal static string localPlayerName;
    internal static int localPlayerID;

    // Server-only cross-reference of connections to player names
    internal static readonly Dictionary<NetworkConnectionToClient, string> connNames = new Dictionary<NetworkConnectionToClient, string>();


    int red_or_blue = 1;

    [SyncVar] public int turn = 0; //The player whose id = turn will play this turn. turn will cycle between 0 to # of player.

    string[] red_name = { "System Shutdown", "Disk Wipe", "Ransom", "Phishing", "Brute Force", "Input Capture" };
    string[] blue_name = { "Access Processes", "User Training", "Restrict Web-Based Content", "Pay Ransom", "Data Backup", "User Acount Management" };

    public override void OnStartServer()
    {
        connNames.Clear();

        foreach (Button card in cards)
        {
            card.GetComponent<Image>().color = Color.red;
        }
        red_or_blue = 0;
        
    }

    public override void OnStartClient()
    {
        cardHistory.text = "";

        for (int i = 0; i < cards.Length; i++)
        {
            GetNewCard(i);
        }
        if (isServer)
        {
            ShowPlayUI();
        }
        else
        {
            HidePlayUI();
        }
    }

    [Command(requiresAuthority = false)]
    void CmdSend(string message, NetworkConnectionToClient sender = null)
    {
        if (!connNames.ContainsKey(sender))
            connNames.Add(sender, sender.identity.GetComponent<RGNetworkPlayer>().playerName);

        if (!string.IsNullOrWhiteSpace(message))
            RpcReceive(connNames[sender], message.Trim());
    }

    [ClientRpc]
    void RpcReceive(string playerName, string message)
    {
        string prettyMessage = playerName == localPlayerName ?
            $"<color=red>{playerName}:</color> {message}" :
            $"<color=blue>{playerName}:</color> {message}";
        AppendMessage(prettyMessage);
    }

    [Command(requiresAuthority = false)]
    public void CmdAskNextTurn()
    {
        RGNetworkPlayerList playerList = RGNetworkPlayerList.instance;
        if(playerList == null)
        {
            Debug.LogError("Can't find playerList object!");
        }
        turn += 1;
        if (turn >= playerList.playerIDs.Count)
        {
            turn = 0;
        }
        RpcNextTurn(turn); //Update the turn value to the clients
    }

    [ClientRpc]
    public void RpcNextTurn(int newTurn)
    {
        int turn = newTurn;
        RGNetworkPlayerList playerList = RGNetworkPlayerList.instance;
        if (playerList == null)
        {
            Debug.LogError("Can't find playerList object!");
        }
        if(playerList.playerIDs[turn] == localPlayerID)
        {
            ShowPlayUI();
        }
        else
        {
            HidePlayUI();
        }
    }

    void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        cardHistory.text += message + "\n";

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

        if (red_or_blue == 0)
        {
            int ri = Random.Range(0, red_name.Length);
            tex.text = red_name[ri];
        }
        else
        {
            int ri = Random.Range(0, red_name.Length);
            tex.text = blue_name[ri];
        }
    }

    public void PlayCard(int index)
    {
        string message = "plays the <color=";
        if (red_or_blue == 0)
            message += "red";
        else
            message += "blue";
        message += ">" + cards[index].transform.Find("CardName").GetComponent<TMP_Text>().text + "</color>.";
        CmdSend(message);

        GetNewCard(0);
    }

    public void ShowPlayUI()
    {
        endTurnButton.gameObject.SetActive(true);
        cardCanvas.gameObject.SetActive(true);
        waitingText.gameObject.SetActive(false);
    }

    public void HidePlayUI()
    {
        endTurnButton.gameObject.SetActive(false);
        cardCanvas.gameObject.SetActive(false);
        waitingText.gameObject.SetActive(true);
    }

}
