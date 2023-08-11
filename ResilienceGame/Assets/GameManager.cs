using System.Collections;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VectorGraphics;

public class GameManager : MonoBehaviour, IDragHandler
{
    // Establish necessary fields
    public GameObject[] allPlayers;
    public GameObject MalActorObject;
    public NativeArray<int> playerIDs;

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
    public Toggle transportationToggle;


    // Utilize if you want to incorporate player input to determine the number of facilities of each type
    public int transportationInputCount = 1;
    public int policeInputCount;
    public int hospitalInputCount;
    public int fireDeptInputCount;
    public int elecGenInputCount = 1;
    public int waterInputCount = 1;
    public int commsInputCount = 1;
    public int commoditiesInputCount;
    public int elecDistInputCount = 1;
    public int fuelInputCount = 1;


    public List<GameObject> allFacilities;

    public int activePlayerNumber;

    public Camera cam;

    public GameObject map;

    public GameObject tiles;

    public FacilityEvents facilityEvents;

    public bool criticalEnabled;

    // Start is called before the first frame update
    void Start()
    {
        startScreen.SetActive(true);
        Debug.Log("Test Start");
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
                fundText.text = "Funds: " + allPlayers[activePlayerNumber].GetComponent<Player>().funds;
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
        for (int i = 0; i < criticalOutlines.Length; i++)
        {
            // Comms
            if (criticalOutlines[i].gameObject.GetComponent<Communications>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);
                criticalOutlines[i].outline.SetActive(toggled);
            }

            // Water
            else if (criticalOutlines[i].gameObject.GetComponent<Water>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                criticalOutlines[i].outline.SetActive(toggled);

            }

            // Power
            else if (criticalOutlines[i].gameObject.GetComponent<ElectricityDistribution>() != null)
            {
                criticalOutlines[i].outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                criticalOutlines[i].outline.SetActive(toggled);

            }
            else if (criticalOutlines[i].gameObject.GetComponent<ElectricityGeneration>() != null)
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
        if ((continueButton.activeSelf == false) && (yarnSpinner.activeSelf == true))
        {
            return;
        }
        else
        {
            maliciousPlayerEndMenu.SetActive(false);
            resilientPlayerEndMenu.SetActive(false);
            playerActive = !playerActive;

            DisableAllOutline();
            foreach(GameObject obj in allPlayers)
            {
                obj.GetComponent<Player>().seletedFacilities.Clear();
            }
            //allPlayers[activePlayerNumber].GetComponent<Player>().seletedFacility = null;
            maliciousActor.targetFacilities.Clear();
            maliciousActor.targetIDList.Clear();
            turnCount += 0.5f;
            if (playerActive)
            {
                activePlayerText.text = allPlayers[activePlayerNumber].GetComponent<Player>().type + " Player";
                fundText.text = "Funds: " + allPlayers[activePlayerNumber].GetComponent<Player>().funds;

                activePlayerColor = new Color(0.0f, 0.4209991f, 1.0f, 1.0f);
                activePlayerText.color = activePlayerColor;
                yarnSpinner.SetActive(true);
                facilityEvents.SpawnEvent();
                ChangePlayers();
                foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().HandList)
                {
                    card.SetActive(false);
                }
                foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().ActiveCardList)
                {
                    card.SetActive(false);
                }
                foreach (GameObject obj in allPlayers)
                {
                    obj.GetComponent<Player>().seletedFacilities.Clear();
                }
                foreach (GameObject obj in allPlayers)
                {
                    obj.GetComponent<Player>().targetIDList.Clear();
                }
                maliciousActor.GetComponent<MaliciousActor>().cardDropZone.SetActive(false);
                maliciousActor.GetComponent<MaliciousActor>().handDropZone.SetActive(false);

            }
            else
            {
                fundText.text = "Funds: " + maliciousActor.funds;
                activePlayerText.text = "Malicious Player";
                activePlayerColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
                activePlayerText.color = activePlayerColor;
                yarnSpinner.SetActive(false);
                MalActorObject.SetActive(true);
                foreach (GameObject fac in allFacilities)
                {
                    Color tempColor = fac.GetComponent<SVGImage>().color;
                    tempColor.a = 1.0f;
                    fac.GetComponent<SVGImage>().color = tempColor;
                }
                Debug.Log(maliciousActor.handSize);
                if (maliciousActor.handSize < 5)
                {
                    maliciousActor.DrawCard();
                }
                foreach (GameObject players in allPlayers)
                {
                    players.GetComponent<Player>().cardDropZone.SetActive(false);
                    players.GetComponent<Player>().handDropZone.SetActive(false);
                    foreach (GameObject card in players.GetComponent<Player>().HandList)
                    {
                        card.SetActive(false);
                    }
                    foreach (GameObject card in players.GetComponent<Player>().ActiveCardList)
                    {
                        card.SetActive(false);
                    }
                }
                foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().HandList)
                {
                    card.SetActive(true);
                }
                foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().ActiveCardList)
                {
                    card.SetActive(true);
                }
                maliciousActor.targetIDList.Clear();
                maliciousActor.targetFacilities.Clear();
                maliciousActor.GetComponent<MaliciousActor>().cardDropZone.SetActive(true);
                maliciousActor.GetComponent<MaliciousActor>().handDropZone.SetActive(true);
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
        this.GetComponent<PlaceIcons>().spawnAllFacilities(policeToggle.isOn, hospitalToggle.isOn, fireDeptToggle.isOn, elecGenToggle.isOn, waterToggle.isOn, commToggle.isOn, cityHallToggle.isOn, commoditiesToggle.isOn, elecDistToggle.isOn, fuelToggle.isOn, transportationToggle.isOn);
        //this.GetComponent<PlaceIcons>().spawnAllFacilities(true, true, true, true, true, true, true, true, true, true);
        //this.GetComponent<PlaceIcons>().spawnAllFacilities(policeToggle.isOn, hospitalToggle.isOn, fireDeptToggle.isOn, true, true, true, cityHallToggle.isOn, commoditiesToggle.isOn, true, fuelToggle.isOn); // The trues are requried facilities

        startScreen.SetActive(false); // DUsable the start menu where you determine how many of each facility you would like


        gameStarted = true;


        // Spawn the players
        int playerCount = 0;
        if (policeToggle.isOn)
        {
            playerCount++;
        }
        if (hospitalToggle.isOn)
        {
            playerCount++;
        }
        if (fireDeptToggle.isOn)
        {
            playerCount++;

        }
        if (elecGenToggle.isOn)
        {
            playerCount++;
        }
        if (waterToggle.isOn)
        {
            playerCount++;
        }
        if (commToggle.isOn)
        {
            playerCount++;
        }
        if (elecDistToggle.isOn)
        {
            playerCount++;
        }
        if (cityHallToggle.isOn)
        {
            playerCount++;

        }
        if (commoditiesToggle.isOn)
        {
            playerCount++;

        }
        if (transportationToggle.isOn)
        {
            playerCount++;

        }
        SpawnPlayers(playerCount);

        //SpawnPlayers(6); // <-- Need to change this to an input
        SpawnMaliciousActor();

        //maliciousActor.SpawnDeck();
        //for (int i = 0; i < maliciousActor.maxHandSize; i++)
        //{
        //    maliciousActor.DrawCard();
        //}
        activePlayerNumber = 0;

        playerActive = true;

        turnCount = 0;


        for (int i = 0; i < allPlayers.Length; i++)
        {
            allPlayers[activePlayerNumber].GetComponent<Player>().seletedFacilities.Clear();
        }
        maliciousActor.targetFacilities.Clear();

        if (playerActive)
        {
            activePlayerText.text = allPlayers[activePlayerNumber].GetComponent<Player>().type + " Player";
            fundText.text = "Funds: " + allPlayers[activePlayerNumber].GetComponent<Player>().funds;
            activePlayerColor = new Color(0.0f, 0.4209991f, 1.0f, 1.0f);
            activePlayerText.color = activePlayerColor;
            yarnSpinner.SetActive(true);
            Debug.Log("Starting player: " + allPlayers[activePlayerNumber].name);
            foreach(GameObject players in allPlayers)
            {
                if(players == allPlayers[activePlayerNumber])
                {
                    players.SetActive(true);
                    foreach(GameObject card in players.GetComponent<Player>().HandList)
                    {
                        card.SetActive(true);
                    }
                }
                else
                {
                    foreach (GameObject card in players.GetComponent<Player>().HandList)
                    {
                        card.SetActive(false);
                    }
                    players.SetActive(false);
                }
            }
            foreach(GameObject cards in maliciousActor.HandList)
            {
                cards.SetActive(false);
            }
            foreach (GameObject fac in allFacilities)
            {
                if (fac.GetComponent<FacilityV3>().type == allPlayers[activePlayerNumber].GetComponent<Player>().type)
                {
                    Color tempColor = fac.GetComponent<SVGImage>().color;
                    tempColor.a = 1.0f;
                    fac.GetComponent<SVGImage>().color = tempColor;
                }
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Water)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                else
                {
                    Color tempColor = fac.GetComponent<SVGImage>().color;
                    tempColor.a = 0.5f;
                    fac.GetComponent<SVGImage>().color = tempColor;
                }
            }
            MalActorObject.SetActive(false);
        }
        else
        {
            fundText.text = "Funds: " + maliciousActor.funds;
            activePlayerText.text = "Malicious Player";
            activePlayerColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            activePlayerText.color = activePlayerColor;
            yarnSpinner.SetActive(false);

            foreach (GameObject players in allPlayers)
            {
                foreach (GameObject card in players.GetComponent<Player>().HandList)
                {
                    card.SetActive(false);
                }
                foreach (GameObject card in players.GetComponent<Player>().ActiveCardList)
                {
                    card.SetActive(false);
                }
            }
            foreach(GameObject card in maliciousActor.GetComponent<MaliciousActor>().HandList)
            {
                card.SetActive(true);
            }
            foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().ActiveCardList)
            {
                card.SetActive(true);
            }
        }
    }

    public void ChangePlayers()
    {
        if (playerActive)
        {
            activePlayerNumber++;
            if (activePlayerNumber >= allPlayers.Length)
            {
                activePlayerNumber = 0;
            }
            activePlayerText.text = allPlayers[activePlayerNumber].GetComponent<Player>().type + " Player";
            fundText.text = "Funds: " + allPlayers[activePlayerNumber].GetComponent<Player>().funds;
            allPlayers[activePlayerNumber].SetActive(true);
            foreach (GameObject players in allPlayers)
            {
                if (players != allPlayers[activePlayerNumber])
                {
                    players.GetComponent<Player>().cardDropZone.SetActive(false);
                    players.GetComponent<Player>().handDropZone.SetActive(false);
                    foreach (GameObject card in players.GetComponent<Player>().HandList)
                    {
                        card.SetActive(false);

                    }
                    foreach (GameObject card in players.GetComponent<Player>().ActiveCardList)
                    {
                        card.SetActive(false);
                    }
                }
                else
                {
                    players.GetComponent<Player>().cardDropZone.SetActive(true);
                    players.GetComponent<Player>().handDropZone.SetActive(true);
                    if (allPlayers[activePlayerNumber].GetComponent<Player>().handSize < 5)
                    {
                        allPlayers[activePlayerNumber].GetComponent<Player>().DrawCard();
                    }
                    foreach (GameObject card in players.GetComponent<Player>().HandList)
                    {
                        card.SetActive(true);

                    }
                    foreach (GameObject card in players.GetComponent<Player>().ActiveCardList)
                    {
                        card.SetActive(true);
                    }
                }
            }
            foreach (GameObject fac in allFacilities)
            {
                if (fac.GetComponent<FacilityV3>().type == allPlayers[activePlayerNumber].GetComponent<Player>().type)
                {
                    Color tempColor = fac.GetComponent<SVGImage>().color;
                    tempColor.a = 1.0f;
                    fac.GetComponent<SVGImage>().color = tempColor;
                }
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Water)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                //else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
                //{
                //    Color tempColor = fac.GetComponent<SVGImage>().color;
                //    tempColor.a = 1.0f;
                //    fac.GetComponent<SVGImage>().color = tempColor;
                //}
                else
                {
                    Color tempColor = fac.GetComponent<SVGImage>().color;
                    tempColor.a = 0.5f;
                    fac.GetComponent<SVGImage>().color = tempColor;
                }
            }
            foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().HandList)
            {
                card.SetActive(false);
            }
            foreach (GameObject card in maliciousActor.GetComponent<MaliciousActor>().ActiveCardList)
            {
                card.SetActive(false);
            }
            DisableAllOutline();
        }
        else
        {

            foreach (GameObject fac in allFacilities)
            {
                Color tempColor = fac.GetComponent<SVGImage>().color;
                tempColor.a = 1.0f;
                fac.GetComponent<SVGImage>().color = tempColor;
            }
        }
    }

    public void SpawnPlayers(int playerCount)
    {
        GameObject basePlayer = GameObject.Find("Base Player");
        allPlayers = new GameObject[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            GameObject newPlayer = Instantiate(basePlayer);
            switch (i)
            {
                case 0:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.Fuel;
                    break;

                case 1:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.Commodities;
                    break;

                case 2:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.Health;
                    break;

                case 3:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.Security;
                    break;

                case 4:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.FireDept;
                    break;

                case 5:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.City;
                    break;

                case 6:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.PublicGoods;
                    break;

                case 7:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.ElectricityGeneration;
                    break;
                case 8:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.ElectricityDistribution;
                    break;
                case 9:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.Water;
                    break;
                case 10:
                    newPlayer.GetComponent<Player>().type = FacilityV3.Type.Transportation;
                    break;
            }
            newPlayer.transform.SetParent(map.transform, false);
            newPlayer.name = newPlayer.GetComponent<Player>().type + " Player";
            allPlayers[i] = newPlayer;
        }
        basePlayer.SetActive(false);
    }
    public void SpawnMaliciousActor()
    {
        GameObject baseMalPlayer = GameObject.Find("Base Malicious Actor");
        GameObject newPlayer = Instantiate(baseMalPlayer);
        newPlayer.transform.SetParent(map.transform, false);
        newPlayer.name = "Malicious Actor";
        maliciousActor = newPlayer.GetComponent<MaliciousActor>();
        MalActorObject = maliciousActor.gameObject;
        baseMalPlayer.SetActive(false);
    }


    public void OnDrag(PointerEventData pointer)
    {
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

    //public void ActivePlayerIncreaseOneFeedback()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().IncreaseOneFeedback();
    //}

    //public void ActivePlayerAllFeedback()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().IncreaseAllFeedback();

    //}

    //public void ActivePlayerHireWorkers()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().HireWorkers();


    //}
    //public void ActivePlayerBoostIT()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostIT();

    //}
    //public void ActivePlayerBoostOT()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostOT();


    //}
    //public void ActivePlayerImprovePhysSec()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().ImprovePhysSec();


    //}
    //public void ActivePlayerIncreaseFunding()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().IncreaseFunding();


    //}

    //public void ActivePlayerBoostElectricity()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostElectricity();


    //}
    //public void ActivePlayerBoostWater()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostWater();


    //}
    //public void ActivePlayerBoostFuel()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostFuel();


    //}
    //public void ActivePlayerBoostComms()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostCommunications();


    //}
    //public void ActivePlayerBoostHealth()
    //{
    //    allPlayers[activePlayerNumber].GetComponent<Player>().BoostHealth();


    //}
}
