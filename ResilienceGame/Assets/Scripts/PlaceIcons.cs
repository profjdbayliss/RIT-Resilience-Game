using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlaceIcons : MonoBehaviour
{
    // Establish necessary fields

    // float2s to hold the single facility locations
    public float2 PoliceLoc;
    public float2 CityHallPos;
    public float2 elecDistPos;

    // Vec2s to track the mapscalar, the original scaler, and the different between the two
    public Vector2 mapScalar;
    public Vector2 OGScalar = new Vector2(1920, 1080);
    public Vector2 OGDeltaScalar;


    public Vector3 feedbackPos;
    public GameManager gameManager;
    public Player player;

    // List of locations to place the facilities at
    public List<float2> HospitalLocations;
    public List<float2> FireDeptLocations;
    public List<float2> ElectricityLocations;
    public List<float2> WaterLocations;
    public List<float2> CommoditiesLocations;
    public List<float2> CommunicationsLocations;
    public List<float2> FuelLocations;
    public List<float2> ElectricityDistributionLocations;
    public List<float2> TransportationLocations;


    public List<Material> HexMaterials;
    
    // Lists to output each of the instantiated facilities
    public List<GameObject> Hospitals;
    public List<GameObject> FireDepartments;
    public List<GameObject> ElectricityFacilities;
    public List<GameObject> WaterFacilities;
    public List<GameObject> CommoditiesFacilities;
    public List<GameObject> CommunicationsFacilities;
    public List<GameObject> FuelFacilities;
    public List<GameObject> ElectricityDistributionFacilities;
    public List<GameObject> TransportationFacilities;


    private GameObject feedbackPanel;


    private static GameObject canvas;
    public  GameObject Map;

    // Private gameobjects of the base version of the facilities to instantiate
    private GameObject Police;
    private GameObject Hospital;
    private GameObject FireTruck;
    private GameObject Electricity;
    private GameObject Water;
    private GameObject Commodities;
    private GameObject Communications;
    private GameObject CityHall;
    private GameObject ElectricityDistributor;
    private GameObject Fuel;
    private GameObject Transportation;




    private GameObject TestHex;
   
    // Start is called before the first frame update
    void Start()
    {
        // get the canvas and the bases of all of the facilities
        feedbackPanel = GameObject.Find("Feedback Panel");
        canvas = GameObject.Find("Canvas");
        Police = GameObject.Find("Police");
        Hospital = GameObject.Find("Hospital");
        FireTruck = GameObject.Find("FireTruck");
        Electricity = GameObject.Find("Electricity");
        Water = GameObject.Find("Water");
        Commodities = GameObject.Find("Commodities");
        Communications = GameObject.Find("Communications");
        CityHall = GameObject.Find("City Hall");
        //Map = GameObject.Find("Map");
        TestHex = GameObject.Find("NewHexPrefab");
        ElectricityDistributor = GameObject.Find("Electricity Distributor");
        Fuel = GameObject.Find("Fuel");
        Transportation = GameObject.Find("Transportation");
        player = GetComponent<Player>();

        // Scale the map properly
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvas.GetComponent<RectTransform>().rect.height);
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvas.GetComponent<RectTransform>().rect.width*0.66f);
       
        mapScalar.x = Map.GetComponent<RectTransform>().rect.width;
        mapScalar.y = Map.GetComponent<RectTransform>().rect.height;

        // Debug the scalars to make sure that we have the right scaling
        Debug.Log("MS: " + mapScalar);
        OGScalar = new Vector2(1920.0f * 0.66f, 1080);
        Debug.Log("OG: " + OGScalar);
        OGDeltaScalar = (mapScalar - OGScalar);
        Debug.Log("OGD: " + OGDeltaScalar);


        // Spawn the one off facilities
        //if (cityHallToggle.isOn)
        //{
        //    spawnFacility(CityHall, CityHallPos, "City Hall (Clone)");
        //}
        //else
        //{
        //    CityHall.SetActive(false);
        //}

        //if (policeToggle.isOn)
        //{
        //    spawnFacility(Police, PoliceLoc, "Police (Clone)");
        //}
        //else
        //{
        //    Police.SetActive(false);
        //}

        //// Spawn the Hospitals
        //if (hospitalToggle.isOn)
        //{
        //    SpawnFacilities(Hospital, HospitalLocations, Hospitals, "Hospital (Clone)");
        //}
        //else
        //{

        //}

        //// Spawn the fire departments
        //SpawnFacilities(FireTruck, FireDeptLocations, FireDepartments, "Fire (Clone)");

        //// Spawn the electricity facilities
        //SpawnFacilities(Electricity, ElectricityLocations, ElectricityFacilities, "Electricity (Clone)");

        //// Convert to water
        //SpawnFacilities(Water, WaterLocations, WaterFacilities, "Water (Clone)");

        //// Commodities
        //SpawnFacilities(Commodities, CommoditiesLocations, CommoditiesFacilities, "Commodities (Clone)");

        //// Communications
        //SpawnFacilities(Communications, CommunicationsLocations, CommunicationsFacilities, "Communications (Clone)");

        //// Electricity Distributor
        //SpawnFacilities(ElectricityDistributor, ElectricityDistributionLocations, ElectricityDistributionFacilities, "Electricity Distributor (Clone)");

        //// Fuel
        //SpawnFacilities(Fuel, FuelLocations, FuelFacilities, "Fuel (Clone)");
    }

    // Update is called once per frame
    void Update()
    {
        // Check to see if the size has changed since the start
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvas.GetComponent<RectTransform>().rect.height);
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvas.GetComponent<RectTransform>().rect.width * 0.66f);

        Vector2 tempScalar = new Vector2(Map.GetComponent<RectTransform>().rect.width, Map.GetComponent<RectTransform>().rect.height);
        Vector2 deltaScale = tempScalar - mapScalar;
        if(Mathf.Abs(deltaScale.x + deltaScale.y) > 0)
        {
            // If there is actual change, change the positions of all of the icons to scale properly
            Debug.Log("CHANGE");
            Debug.Log("Map Rect Width: " + Map.GetComponent<RectTransform>().rect.width);
            Debug.Log("Map Rect Height: " + Map.GetComponent<RectTransform>().rect.height);
            Debug.Log("OG Scalar: " + OGScalar);
            Debug.Log("UTD Scalar: " + tempScalar);
            Debug.Log("Delta Scalar: " + deltaScale);
            Debug.Log("OGDELTA: " + OGDeltaScalar);
            mapScalar = tempScalar;

        }
        else
        {
            // Do nothing
            //Debug.Log("OGDELTA: " + OGDeltaScalar);

            // Debug.Log("OG Scalar: " + OGScalar);
            // Debug.Log("Map Scalar: " + mapScalar);
            // Debug.Log("UTD Scalar: " + tempScalar);
            // Debug.Log("Map Rect Width: " + Map.GetComponent<RectTransform>().rect.width);
            // Debug.Log("Map Rect Height: " + Map.GetComponent<RectTransform>().rect.height);
        }


    }

    /// <summary>
    /// Spawn Facilities Method
    /// 
    /// Spawn Facilities takes in the following parameters to then instantiate and position the instantiated facilities in their proper locations. Spawn facilities
    /// will also sclae the locations based upon the scale of the map to make sure that they are always in the correct spot. Scale scorrection is utilized to make sure that
    /// the farther away an icon is from the center that they are being scaled more to ensure that all locations are placed in the correct spot. As when the map scales, the icons
    /// that are closer to the center of the map are not as impacted by the scale, while farther ones are scaled more heavily.
    /// </summary>
    /// <param name="baseFacility">
    /// The parameter base facility is the original facility that is going to be instantiated and used to make the rest of the facilities of that type.
    /// </param>
    /// <param name="locations">
    /// Locations is a list of float2 locations to place each of the instantiated facilities at.
    /// </param>
    /// <param name="output">
    /// The parameter output is the list of gameobjects that holds the instantiated facilities to track them later.
    /// </param>
    /// <param name="name">
    /// The param name is a parameter that is a string used to name the instantiated facility properly so it is easier to notice.
    /// </param>
    void SpawnFacilities(GameObject baseFacility, List<float2> locations, List<GameObject> output, string name)
    {
        int locationCount = locations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            GameObject tempFacility = Instantiate(baseFacility);

            // Check to see if we are at the original scale, if we aren't then we will utilize our scalars, if not, we can just use the 
            // passed in locations
            if (mapScalar != new Vector2(1920.0f * 0.66f, 1080))
            {
                Vector3 tempVec = new Vector3(0, 0, 0);
                float scaleCorrectionX = math.abs(locations[i].x) / (mapScalar.x / 2.0f);
                float scaleCorrectionY = math.abs(locations[i].y) / (mapScalar.y / 2.0f);


                // If the y is negative, we want to subtract tom make sure we go in the right direction.
                if (locations[i].x < 0.0f)
                {
                    tempVec.x = locations[i].x - ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
                }
                else
                {
                    tempVec.x = locations[i].x + ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
                }

                // If the y is negative, we want to subtract to make sure we go in the right direction.
                if (locations[i].y < 0.0f)
                {
                    tempVec.y = locations[i].y - ((OGDeltaScalar.y * 0.66f) * scaleCorrectionY);
                }
                else
                {
                    tempVec.y = locations[i].y + ((OGDeltaScalar.y * 0.66f) * scaleCorrectionY);
                }

                // Set the instantiated facilities position to the temporary Vec3 we used to put it in the correct location and add it to the proper list
                tempFacility.transform.position = tempVec;
                player.Facilities.Add(tempFacility);
                output.Add(tempFacility);
            }
            else
            {
                tempFacility.transform.position = new Vector3(locations[i].x, locations[i].y, 0);
                player.Facilities.Add(tempFacility);
                output.Add(tempFacility);
            }

            //fire.transform.SetParent (canvas.transform,false);

            // Set the material then parent it to the Map
            //fire.GetComponent<MeshRenderer>().material = HexMaterials[10]; <-- Only needed when utilizing hexes

            tempFacility.transform.SetParent(Map.transform, false);
            tempFacility.name = name;
        }
        // Disable the original facility
        baseFacility.SetActive(false);
    }

    /// <summary>
    /// Spawn Facility Method
    /// 
    /// Spawn Facility method is the same method as SpawnFacilities, however it is utilized for one off locations like City Hall, etc.
    /// </summary>
    /// <param name="baseFacility">
    /// The parameter base facility is the original facility that is going to be instantiated and used to make the rest of the facilities of that type.
    /// </param>
    /// <param name="loc">
    /// Loc is a float2 to know to place the instantiated facility at.
    /// </param>
    /// <param name="name">
    /// The param name is a parameter that is a string used to name the instantiated facility properly so it is easier to notice.
    /// </param>
    void spawnFacility(GameObject baseFacility, float2 loc, string name)
    {
        GameObject tempFacility = Instantiate(baseFacility);
        Vector3 tempFacilityPos = new Vector3(0, 0, 0);

        if (mapScalar != new Vector2(1920.0f * 0.66f, 1080))
        {
            float scaleCorrectionX = math.abs(loc.x) / (mapScalar.x / 2.0f);
            float scaleCorrectionY = math.abs(loc.y) / (mapScalar.y / 2.0f);

            tempFacilityPos.x = loc.x + ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
            tempFacilityPos.y = loc.y - ((OGDeltaScalar.y * 0.66f) * scaleCorrectionY);
        }
        else
        {
            tempFacilityPos = new Vector3(loc.x, loc.y, 0);
        }

        tempFacility.transform.position = tempFacilityPos;
        tempFacility.transform.SetParent(Map.transform, false);
        tempFacility.name = name;
        player.Facilities.Add(tempFacility);
        baseFacility.SetActive(false);

    }


    public void spawnAllFacilities(bool police = true, bool hospital = true, bool firedpt = true, bool elecGen = true, bool water = true, bool comms = true, bool cityHall = true, bool commodities = true, bool elecDist = true, bool fuel = true)
    {

        // Spawn the Hospitals
        if (hospital)
        {
            SpawnVariableFacilities(Hospital, gameManager.HospitalInputCount, HospitalLocations, Hospitals, "Hospital (Clone)");
            //SpawnFacilities(Hospital, HospitalLocations, Hospitals, "Hospital (Clone)");
        }
        else
        {
            Hospital.SetActive(false);
        }

        if (police)
        {
            //SpawnVariableFacilities(police, gameManager.policeInputCount, poli)
            spawnFacility(Police, PoliceLoc, "Police (Clone)");
        }
        else
        {
            Police.SetActive(false);
        }

        if (firedpt)
        {
            SpawnVariableFacilities(FireTruck, gameManager.FireDeptInputCount, FireDeptLocations, FireDepartments, "Fire (Clone)");
            //SpawnFacilities(FireTruck, FireDeptLocations, FireDepartments, "Fire (Clone)");
        }
        else
        {
            FireTruck.SetActive(false);
        }

        if (elecGen)
        {
            SpawnVariableFacilities(Electricity, gameManager.ElecGenInputCount, ElectricityLocations, ElectricityFacilities, "Electricity (Clone)");
            //SpawnFacilities(Electricity, ElectricityLocations, ElectricityFacilities, "Electricity (Clone)");
        }
        else
        {
            Electricity.SetActive(false);
        }

        if (water)
        {
            SpawnVariableFacilities(Water, gameManager.WaterInputCount, WaterLocations, WaterFacilities, "Water (Clone)");
            //SpawnFacilities(Water, WaterLocations, WaterFacilities, "Water (Clone)");
        }
        else
        {
            Water.SetActive(false);
        }

        if (comms)
        {
            SpawnVariableFacilities(Communications, gameManager.CommsInputCount, CommunicationsLocations, CommunicationsFacilities, "Communications (Clone)");
            //SpawnFacilities(Communications, CommunicationsLocations, CommunicationsFacilities, "Communications (Clone)");
        }
        else
        {
            Communications.SetActive(false);
        }

        if (cityHall)
        {

            spawnFacility(CityHall, CityHallPos, "City Hall (Clone)");
        }
        else
        {
            CityHall.SetActive(false);
        }

        if (commodities)
        {
            SpawnVariableFacilities(Commodities, gameManager.CommoditiesInputCount, CommoditiesLocations, CommoditiesFacilities, "Commodities (Clone)");
            //SpawnFacilities(Commodities, CommoditiesLocations, CommoditiesFacilities, "Commodities (Clone)");
        }
        else
        {
            Commodities.SetActive(false);
        }

        if (elecDist)
        {
            SpawnVariableFacilities(ElectricityDistributor, gameManager.ElecDistInputCount, ElectricityDistributionLocations, ElectricityDistributionFacilities, "Electricity Distribution (Clone)");
            //SpawnFacilities(ElectricityDistributor, ElectricityDistributionLocations, ElectricityDistributionFacilities, "Electricity Distribution (Clone)");
        }
        else
        {
            ElectricityDistributor.SetActive(false);
        }

        if (fuel)
        {
            SpawnVariableFacilities(Fuel, gameManager.FuelInputCount, FuelLocations, FuelFacilities, "Fuel (Clone)");
            //SpawnFacilities(Fuel, FuelLocations, FuelFacilities, "Fuel (Clone)");
        }
        else
        {
            Fuel.SetActive(false);
        }

        SpawnVariableFacilities(Transportation, gameManager.TransportationInputCount, TransportationLocations, TransportationFacilities, "Transportation (Clone)");
    }

    void SpawnVariableFacilities(GameObject baseFacility, int amount, List<float2> locations, List<GameObject> output, string name)
    {
        int locationCount = locations.Count;
        for (int i = 0; i < amount; i++)
        {
            // make a copy
            GameObject tempFacility = Instantiate(baseFacility);
            if (i == locations.Count)
            {
                float newLocX = UnityEngine.Random.Range(-mapScalar.x / 2.0f, mapScalar.x / 2.0f);
                float newLocY = UnityEngine.Random.Range(-mapScalar.y / 2.0f, mapScalar.y / 2.0f);
                float2 newLoc = new float2(newLocX, newLocY);
                locations.Add(newLoc);
            }
            // Check to see if we are at the original scale, if we aren't then we will utilize our scalars, if not, we can just use the 
            // passed in locations
            if (mapScalar != new Vector2(1920.0f * 0.66f, 1080))
            {
                Vector3 tempVec = new Vector3(0, 0, 0);
                float scaleCorrectionX = math.abs(locations[i].x) / (mapScalar.x / 2.0f);
                float scaleCorrectionY = math.abs(locations[i].y) / (mapScalar.y / 2.0f);


                // If the y is negative, we want to subtract tom make sure we go in the right direction.
                if (locations[i].x < 0.0f)
                {
                    tempVec.x = locations[i].x - ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
                }
                else
                {
                    tempVec.x = locations[i].x + ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
                }

                // If the y is negative, we want to subtract to make sure we go in the right direction.
                if (locations[i].y < 0.0f)
                {
                    tempVec.y = locations[i].y - ((OGDeltaScalar.y * 0.66f) * scaleCorrectionY);
                }
                else
                {
                    tempVec.y = locations[i].y + ((OGDeltaScalar.y * 0.66f) * scaleCorrectionY);
                }

                // Set the instantiated facilities position to the temporary Vec3 we used to put it in the correct location and add it to the proper list
                tempFacility.transform.position = tempVec;
                //player.Facilities.Add(tempFacility);
                output.Add(tempFacility);
                gameManager.allFacilities.Add(tempFacility);
            }
            else
            {
                tempFacility.transform.position = new Vector3(locations[i].x, locations[i].y, 0);
                //player.Facilities.Add(tempFacility);
                output.Add(tempFacility);
                gameManager.allFacilities.Add(tempFacility);
            }

            //fire.transform.SetParent (canvas.transform,false);

            // Set the material then parent it to the Map
            //fire.GetComponent<MeshRenderer>().material = HexMaterials[10]; <-- Only needed when utilizing hexes

            tempFacility.transform.SetParent(Map.transform, false);
            tempFacility.name = name;
        }
        // Disable the original facility
        baseFacility.SetActive(false);
    }
}
