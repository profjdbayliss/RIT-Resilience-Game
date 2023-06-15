using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Establish necessary fields
    public Player player;
    public MaliciousActor maliciousActor;
    public bool playerActive;
    public GameObject playerMenu;
    public GameObject maliciousActorMenu;
    public float turnCount;
    public TextMeshProUGUI fundText;
    public TextMeshProUGUI activePlayerText;
    public GameObject yarnSpinner;
    public Color activePlayerColor;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        maliciousActor = GetComponent<MaliciousActor>();
        playerActive = true;
        turnCount = 0;
        if (playerActive)
        {
            fundText.text = "Funds: " + player.funds;
            activePlayerText.text = "Resilient Player";
            activePlayerColor = new Color(0.0f, 0.4209991f, 1.0f, 1.0f);
            activePlayerText.color = activePlayerColor;
            yarnSpinner.SetActive(true);
        }
        else
        {
            fundText.text = "Funds: " + maliciousActor.funds;
            activePlayerText.text = "Malicious Player";
            activePlayerColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            activePlayerText.color = activePlayerColor;
            yarnSpinner.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerActive)
        {
            // Player
            playerMenu.SetActive(true);
            maliciousActorMenu.SetActive(false);
            yarnSpinner.SetActive(true);
        }
        else
        {
            // Malicious actor
            playerMenu.SetActive(false);
            maliciousActorMenu.SetActive(true);
            yarnSpinner.SetActive(false);
        }
    }

    // Will want to move to a game manager later
    public void EnableAllOutline(bool toggled)
    {
        FacilityOutline[] allOutlines = GameObject.FindObjectsOfType<FacilityOutline>();
        for (int i = 0; i < allOutlines.Length; i++)
        {
            allOutlines[i].outline.SetActive(toggled);
        }
    }

    public void SwapPlayer()
    {
        playerActive = !playerActive;
        DisableAllOutline();
        player.seletedFacility = null;
        maliciousActor.targetFacility = null;
        turnCount += 0.5f;
        if (playerActive)
        {
            fundText.text = "Funds: " + player.funds;
            activePlayerText.text = "Resilient Player";
            activePlayerColor = new Color(0.0f, 0.4209991f, 1.0f, 1.0f);
            activePlayerText.color = activePlayerColor;
            yarnSpinner.SetActive(true);

        }
        else
        {
            fundText.text = "Funds: " + maliciousActor.funds;
            activePlayerText.text = "Malicious Player";
            activePlayerColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            activePlayerText.color = activePlayerColor;
            yarnSpinner.SetActive(false);
        }
    }
    public void DisableAllOutline()
    {
        FacilityOutline[] allOutlines = GameObject.FindObjectsOfType<FacilityOutline>();
        for (int i = 0; i < allOutlines.Length; i++)
        {
            allOutlines[i].outline.SetActive(false);
        }
    }
}
