using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Establish necessary fields
    public Player player;
    public MaliciousActor maliciousActor;
    public bool playerActive;
    public GameObject playerMenu;
    public GameObject maliciousActorMenu;
    public float turnCount;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        maliciousActor = GetComponent<MaliciousActor>();
        playerActive = true;
        turnCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerActive)
        {
            // Player
            playerMenu.SetActive(true);
            maliciousActorMenu.SetActive(false);
        }
        else
        {
            // Malicious actor
            playerMenu.SetActive(false);
            maliciousActorMenu.SetActive(true);
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
