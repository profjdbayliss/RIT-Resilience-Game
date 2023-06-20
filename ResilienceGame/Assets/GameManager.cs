using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Establish necessary fields
    public Player player;
    public MaliciousActor maliciousActor;
    public bool playerActive;
    public GameObject playerMenu;
    public GameObject maliciousActorMenu;

    public bool gameStarted = false;

    public GameObject gameCanvas;
    public GameObject startScreen;

    public float turnCount;
    
    public TextMeshProUGUI fundText;
    public TextMeshProUGUI activePlayerText;
    
    public GameObject yarnSpinner;
    
    public Color activePlayerColor;
    
    public GameObject continueButton;

    public GameObject maliciousPlayerEndMenu;
    public GameObject resilientPlayerEndMenu;

    public Toggle policeToggle;
    public Toggle hospitalToggle;
    public Toggle fireDeptToggle;
    public Toggle elecGenToggle;
    public Toggle waterToggle;
    public Toggle commoditiesToggle;
    public Toggle commToggle;
    public Toggle elecDistToggle;
    public Toggle cityHallToggle;
    public Toggle fuelToggle;




    // Start is called before the first frame update
    void Start()
    {
        startScreen.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (playerActive)
            {
                // Player
                playerMenu.SetActive(true);
                maliciousActorMenu.SetActive(false);
                yarnSpinner.SetActive(true);
                fundText.text = "Funds: " + player.funds;
                // If enough of the facilites are down, trigger response from the govt


            }
            else
            {
                // Malicious actor
                playerMenu.SetActive(false);
                maliciousActorMenu.SetActive(true);
                yarnSpinner.SetActive(false);
                fundText.text = "Funds: " + maliciousActor.funds;

            }
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

    public void EnableCriticalOutline(bool toggled)
    {
        FacilityOutline[] criticalOutlines = GameObject.FindObjectsOfType<FacilityOutline>();
        for(int i = 0; i < criticalOutlines.Length; i++)
        {
            // Comms
            if(criticalOutlines[i].gameObject.GetComponent<Communications>() != null)
            {
                criticalOutlines[i].outline.SetActive(toggled);
            }
            
            // Water
            else if(criticalOutlines[i].gameObject.GetComponent<Water>() != null)
            {
                criticalOutlines[i].outline.SetActive(toggled);

            }

            // Power
            else if(criticalOutlines[i].gameObject.GetComponent<ElectricityDistribution>() != null)
            {
                criticalOutlines[i].outline.SetActive(toggled);

            }
            else if(criticalOutlines[i].gameObject.GetComponent<ElectricityGeneration>() != null)
            {
                criticalOutlines[i].outline.SetActive(toggled);

            }

            // IT

            // Transport

        }
    }

    public void SwapPlayer()
    {
        if((continueButton.activeSelf == false) && (yarnSpinner.activeSelf == true))
        {
            return;
        }
        else
        {
            maliciousPlayerEndMenu.SetActive(false);
            resilientPlayerEndMenu.SetActive(false);
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
        
    }
    public void DisableAllOutline()
    {
        FacilityOutline[] allOutlines = GameObject.FindObjectsOfType<FacilityOutline>();
        for (int i = 0; i < allOutlines.Length; i++)
        {
            allOutlines[i].outline.SetActive(false);
        }
    }

    public void EnableSwapPlayerMenu()
    {
        if ((continueButton.activeSelf == false) && (yarnSpinner.activeSelf == true))
        {
            return;
        }
        else
        {
            if (playerActive)
            {
                resilientPlayerEndMenu.SetActive(true);
            }
            else
            {
                maliciousPlayerEndMenu.SetActive(true);
            }
        }

    }

    public void StartGame()
    {
        gameCanvas.SetActive(true);
        this.GetComponent<PlaceIcons>().spawnAllFacilities(policeToggle.isOn, hospitalToggle.isOn, fireDeptToggle.isOn, elecGenToggle.isOn, waterToggle.isOn, commToggle.isOn, cityHallToggle.isOn, commoditiesToggle.isOn, elecDistToggle.isOn, fuelToggle.isOn);
        startScreen.SetActive(false);
        gameStarted = true;
        player = GetComponent<Player>();
        maliciousActor = GetComponent<MaliciousActor>();
        playerActive = true;
        turnCount = 0;
        player.seletedFacility = null;
        maliciousActor.targetFacility = null;
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
}
