using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour, IScrollHandler, IDragHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
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

    // Utilize if you want to have a set number of facilities and have it be toggled on and off
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


    // Utilize if you want to incorporate player input to determine the number of facilities of each type
    public int transportationInputCount;
    public int policeInputCount;
    public int hospitalInputCount;
    public int fireDeptInputCount;
    public int elecGenInputCount;
    public int waterInputCount;
    public int commsInputCount;
    public int commoditiesInputCount;
    public int elecDistInputCount;
    public int fuelInputCount;

    public Camera cam;

    public GameObject map;

    public GameObject tiles;

    public FacilityEvents facilityEvents;

    public bool criticalEnabled;

    // Start is called before the first frame update
    void Start()
    {
        startScreen.SetActive(true);
    }


    // Created properties to allow for player input to decide how many facilities of each type to have.
    public int TransportationInputCount
    {
        get
        {
            return transportationInputCount;
        }
        set
        {
            transportationInputCount = value;
        }
    }

    public int PoliceInputCount
    {
        get
        {
            return policeInputCount;
        }
        set
        {
            policeInputCount = value;
        }
    }

    public int HospitalInputCount
    {
        get
        {
            return hospitalInputCount;
        }
        set
        {
            hospitalInputCount = value;
        }
    }

    public int FireDeptInputCount
    {
        get
        {
            return fireDeptInputCount;
        }
        set
        {
            fireDeptInputCount = value;
        }
    }

    public int ElecGenInputCount
    {
        get
        {
            return elecGenInputCount;
        }
        set
        {
            elecGenInputCount = value;
        }
    }

    public int WaterInputCount
    {
        get
        {
            return waterInputCount;
        }
        set
        {
            waterInputCount = value;
        }
    }

    public int CommsInputCount
    {
        get
        {
            return commsInputCount;
        }
        set
        {
            commsInputCount = value;
        }
    }

    public int CommoditiesInputCount
    {
        get 
        {
            return commoditiesInputCount; 
        }
        set
        {
            commoditiesInputCount = value;
        }
    }

    public int ElecDistInputCount
    {
        get
        {
            return elecDistInputCount;
        }
        set
        {
            elecDistInputCount = value;
        }
    }

    public int FuelInputCount
    {
        get
        {
            return fuelInputCount;
        }
        set
        {
            fuelInputCount = value;
        }
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
        criticalEnabled = toggled;
        FacilityOutline[] criticalOutlines = GameObject.FindObjectsOfType<FacilityOutline>();
        for(int i = 0; i < criticalOutlines.Length; i++)
        {
            // Comms
            if(criticalOutlines[i].gameObject.GetComponent<Communications>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);
                criticalOutlines[i].outline.SetActive(toggled);
            }
            
            // Water
            else if(criticalOutlines[i].gameObject.GetComponent<Water>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                criticalOutlines[i].outline.SetActive(toggled);

            }

            // Power
            else if(criticalOutlines[i].gameObject.GetComponent<ElectricityDistribution>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                criticalOutlines[i].outline.SetActive(toggled);

            }
            else if(criticalOutlines[i].gameObject.GetComponent<ElectricityGeneration>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                criticalOutlines[i].outline.SetActive(toggled);

            }

            // IT

            // Transport
            else if (criticalOutlines[i].gameObject.GetComponent<Transportation>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                criticalOutlines[i].outline.SetActive(toggled);

            }
            else
            {
                criticalOutlines[i].outline.SetActive(false);

            }
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
                facilityEvents.SpawnEvent();
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
        //this.GetComponent<PlaceIcons>().spawnAllFacilities(policeToggle.isOn, hospitalToggle.isOn, fireDeptToggle.isOn, elecGenToggle.isOn, waterToggle.isOn, commToggle.isOn, cityHallToggle.isOn, commoditiesToggle.isOn, elecDistToggle.isOn, fuelToggle.isOn);
        this.GetComponent<PlaceIcons>().spawnAllFacilities(true, true, true, true, true, true, true, true, true, true);

        startScreen.SetActive(false);

        Debug.Log("STARTED");

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

    public void OnPointerDown(PointerEventData pointer)
    {
        Debug.Log("Down");
    }

    public void OnPointerUp(PointerEventData pointer)
    {
        Debug.Log("UP");
    }



    public void OnPointerClick(PointerEventData pointer)
    {
        Debug.Log("Click");
    }


    public void OnScroll(PointerEventData pointer)
    {
        Debug.Log("Scrolled");
        if(pointer.scrollDelta.y > 0.0f)
        {
            gameCanvas.GetComponent<Canvas>().planeDistance -= 0.01f;
        }
        else
        {
            gameCanvas.GetComponent<Canvas>().planeDistance += 0.01f;
            
        }
    }



    public void OnDrag(PointerEventData pointer)
    {
        Debug.Log("DRAG");
        if (tiles.gameObject.activeSelf) // Check to see if the gameobject this is attached to is active in the scene
        {
            // Create a vector2 to hold the previous position of the element and also set our target of what we want to actually drag.
            Vector2 tempVec2 = default(Vector2);
            RectTransform target = tiles.gameObject.GetComponent<RectTransform>();
            Vector2 tempPos = target.transform.localPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position - pointer.delta, pointer.pressEventCamera, out tempVec2) == true) // Check the older position of the element and see if it was previously
            {
                Vector2 tempNewVec = default(Vector2); // Create a new Vec2 to track the current position of the object
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position, pointer.pressEventCamera, out tempNewVec) == true)
                {
                    tempPos.x += tempNewVec.x - tempVec2.x;
                    tempPos.y = tiles.transform.localPosition.y;
                    tiles.transform.localPosition = tempPos;
                }
            }
        }
    }


}
