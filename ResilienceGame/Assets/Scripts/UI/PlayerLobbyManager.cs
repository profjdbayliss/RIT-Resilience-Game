using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerLobbyManager : MonoBehaviour {
    [SerializeField] private GameObject playerPopupPrefab;
    [SerializeField] private RectTransform blueLeftParent;
    [SerializeField] private RectTransform blueRightParent;
    [SerializeField] private RectTransform redParent;

    [SerializeField] private Color redColor;
    [SerializeField] private Color blueColor;

    [SerializeField] private List<LobbyItem> bluePlayers = new List<LobbyItem>();
    [SerializeField] private List<LobbyItem> redPlayers = new List<LobbyItem>();
    private int numBluePlayers = 0;
    private int numRedPlayers = 0;


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void AddPlayer(string name, PlayerTeam team) {
        switch (team) {
            case PlayerTeam.Red:
                if (numRedPlayers <= redPlayers.Count)
                    AddToRed(name);
                else {
                    Debug.LogWarning("Red player list is full, adding to blue instead");
                    AddToBlue(name);
                }
                break;
            case PlayerTeam.Blue:
                if (numBluePlayers <= bluePlayers.Count)
                    AddToBlue(name);
                else {
                    Debug.LogWarning("Blue player list is full, adding to red instead");
                    AddToRed(name);
                }
                break;
        }
    }
    public void AddToRed(string name) {
        redPlayers[numRedPlayers].SetPlayerNameAndTeam(name, PlayerTeam.Red);
        numRedPlayers++;


    }
    public void AddToBlue(string name) {
        bluePlayers[numBluePlayers].SetPlayerNameAndTeam(name, PlayerTeam.Blue);
        numBluePlayers++;
    }
    public void ChangePlayerTeam(string name, PlayerTeam team) {
        switch (team) {
            case PlayerTeam.Red:
                foreach (var player in bluePlayers) {
                    if (player.PlayerName.text == name) {
                        player.SetPlayerNameAndTeam(name, PlayerTeam.Red);
                        numBluePlayers--;
                        numRedPlayers++;
                        return;
                    }
                }
                break;
            case PlayerTeam.Blue:
                foreach (var player in redPlayers) {
                    if (player.PlayerName.text == name) {
                        player.SetPlayerNameAndTeam(name, PlayerTeam.Blue);
                        numRedPlayers--;
                        numBluePlayers++;
                        return;
                    }
                }
                break;
        }
    }
}
