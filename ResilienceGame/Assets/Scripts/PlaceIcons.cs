using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlaceIcons : MonoBehaviour
{
    public float2 Offset;
    public float2 SizeOfBox;
    public float2 PoliceLoc;
    public float2 CityHallPos;
    public Vector2 mapScalar;
    public Vector2 OGScalar = new Vector2(1920, 1080);
    public Vector2 OGDeltaScalar;
    public List<float2> HospitalLocations;
    public List<float2> FireDeptLocations;
    public List<float2> ElectricityLocations;
    public List<float2> WaterLocations;
    public List<float2> CommoditiesLocations;
    public List<float2> CommunicationsLocations;
    public List<Material> HexMaterials;

    private static GameObject canvas;
    public GameObject Police;
    private GameObject Hospital;
    private GameObject FireTruck;
    private GameObject Electricity;
    private GameObject Water;
    private GameObject Commodities;
    private GameObject Communications;
    private GameObject CityHall;
    private GameObject Map;

    private GameObject TestHex;
   
    // Start is called before the first frame update
    void Start()
    {
        // get the canvas
        canvas = GameObject.Find("Canvas");
        Police = GameObject.Find("Police");
        Hospital = GameObject.Find("Hospital");
        FireTruck = GameObject.Find("FireTruck");
        Electricity = GameObject.Find("Electricity");
        Water = GameObject.Find("Water");
        Commodities = GameObject.Find("Commodities");
        Communications = GameObject.Find("Communications");
        CityHall = GameObject.Find("City Hall");
        Map = GameObject.Find("Map");
        TestHex = GameObject.Find("NewHexPrefab");

        // Scale the map properly
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvas.GetComponent<RectTransform>().rect.height);
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvas.GetComponent<RectTransform>().rect.width*0.66f);
        mapScalar = Map.GetComponent<RectTransform>().sizeDelta;
        mapScalar.x = Map.GetComponent<RectTransform>().rect.width;
        mapScalar.y = Map.GetComponent<RectTransform>().rect.height;
        Debug.Log("MS: " + mapScalar);
        OGScalar = new Vector2(1920.0f * 0.66f, 1080);
        Debug.Log("OG: " + OGScalar);
        OGDeltaScalar = (mapScalar - OGScalar);
        Debug.Log("OGD: " + OGDeltaScalar);
        //OGScalar = mapScalar; 82.79, -21.9
        //Debug.Log("CHPOS: " + CityHall.transform.position);
        //GameObject tempCityHall = Instantiate(CityHall);
        //Vector3 tempVecCityHall = new Vector3(0,0,0);
        //if (mapScalar != new Vector2(1920.0f * 0.66f, 1080))
        //{
        //    float scaleCorrectionX = math.abs(CityHallPos.x) / (mapScalar.x / 2.0f);
        //    float scaleCorrectionY = math.abs(CityHallPos.y) / (mapScalar.y / 2.0f);
        //    tempVecCityHall.x = CityHallPos.x + ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
        //    tempVecCityHall.y = CityHallPos.y - ((OGDeltaScalar.y * 0.66f) * scaleCorrectionY);
        //}
        //else
        //{
        //    tempVecCityHall = new Vector3(CityHallPos.x, CityHallPos.y, 0);
        //}
        //tempCityHall.transform.position = tempVecCityHall;
        //tempCityHall.transform.SetParent(Map.transform, false);
        //tempCityHall.name = "City Hall (Clone)";
        //CityHall.SetActive(false);
        spawnFacility(CityHall, CityHallPos, "City Hall (Clone)");
        spawnFacility(Police, PoliceLoc, "Police (Clone)");

        SpawnFacilities(Hospital, HospitalLocations, "Hospital (Clone)");
        /*
         // take hospital image and place it on canvas at locations specified;
        int locationCount = HospitalLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            GameObject hospital = Instantiate(Hospital);
            //GameObject hospital = Instantiate(TestHex);

            // place
            //hospital.transform.position = new Vector3(HospitalLocations[i].x*SizeOfBox.x+Offset.x, -1*HospitalLocations[i].y*SizeOfBox.y+Offset.y-5.0f, 0);
            if (mapScalar != new Vector2(1920.0f * 0.66f, 1080))
            {
                Vector3 tempVec = new Vector3(0, 0, 0);
                if (HospitalLocations[i].x < 0.0f)
                {
                    float scaleCorrection = math.abs(HospitalLocations[i].x) / (mapScalar.x / 2.0f);
                    tempVec.x = HospitalLocations[i].x - ((OGDeltaScalar.x * 0.66f) * scaleCorrection);
                }
                else
                {
                    float scaleCorrection = math.abs(HospitalLocations[i].x) / (mapScalar.x / 2.0f);
                    tempVec.x = HospitalLocations[i].x + ((OGDeltaScalar.x * 0.66f) * scaleCorrection);
                }
                if (HospitalLocations[i].y < 0.0f)
                {
                    float scaleCorrection = math.abs(HospitalLocations[i].y) / (mapScalar.y / 2.0f);
                    tempVec.y = HospitalLocations[i].y - ((OGDeltaScalar.y * 0.66f) * scaleCorrection);

                }
                else
                {
                    float scaleCorrection = math.abs(HospitalLocations[i].y) / (mapScalar.y / 2.0f);

                    tempVec.y = HospitalLocations[i].y + ((OGDeltaScalar.y * 0.66f) * scaleCorrection);

                }
                hospital.transform.position = tempVec;

            }
            else
            {
                hospital.transform.position = new Vector3(HospitalLocations[i].x, HospitalLocations[i].y, 0);
            }
            // Apply the health material <-- Uncomment if using hexes
            //hospital.GetComponent<MeshRenderer>().material = HexMaterials[6];

            hospital.transform.SetParent (Map.transform,false);
            hospital.name = "Hospital (Clone)";

        }
        Hospital.SetActive(false);
         */

        SpawnFacilities(FireTruck, FireDeptLocations, "Fire (Clone)");
        //locationCount = FireDeptLocations.Count;
        //for (int i = 0; i < locationCount; i++)
        //{
        //    // make a copy
        //    GameObject fire = Instantiate(FireTruck);
        //    //GameObject fire = Instantiate(TestHex);

        //    // place
        //    //fire.transform.position = new Vector3(FireDeptLocations[i].x*SizeOfBox.x+Offset.x, -1*FireDeptLocations[i].y*SizeOfBox.y+Offset.y-5.0f + 240.0f, 0);
        //    // Old Formula ^^

        //    if(mapScalar != new Vector2(1920.0f*0.66f, 1080))
        //    {
        //        Vector3 tempVec = new Vector3(0, 0, 0);
        //        if(FireDeptLocations[i].x < 0.0f)
        //        {
        //            float scaleCorrection = math.abs(FireDeptLocations[i].x) / (mapScalar.x / 2.0f);
        //            tempVec.x = FireDeptLocations[i].x - ((OGDeltaScalar.x * 0.66f) * scaleCorrection);
        //        }
        //        else
        //        {
        //            float scaleCorrection = math.abs(FireDeptLocations[i].x) / (mapScalar.x / 2.0f);
        //            tempVec.x = FireDeptLocations[i].x + ((OGDeltaScalar.x * 0.66f) * scaleCorrection);
        //        }
        //        if (FireDeptLocations[i].y < 0.0f)
        //        {
        //            float scaleCorrection = math.abs(FireDeptLocations[i].y) / (mapScalar.y / 2.0f);
        //            tempVec.y = FireDeptLocations[i].y - ((OGDeltaScalar.y * 0.66f) * scaleCorrection);

        //        }
        //        else
        //        {
        //            float scaleCorrection = math.abs(FireDeptLocations[i].y) / (mapScalar.y / 2.0f);

        //            tempVec.y = FireDeptLocations[i].y + ((OGDeltaScalar.y * 0.66f) * scaleCorrection);

        //        }
        //        fire.transform.position = tempVec;

        //    }
        //    else
        //    {
        //        fire.transform.position = new Vector3(FireDeptLocations[i].x, FireDeptLocations[i].y, 0);
        //    }

        //    //fire.transform.SetParent (canvas.transform,false);

        //    // Set the material then parent it to the Map
        //    //fire.GetComponent<MeshRenderer>().material = HexMaterials[10];

        //    fire.transform.SetParent(Map.transform, false);
        //    fire.name = "Fire (Clone)";
        //}
        //FireTruck.SetActive(false);

        SpawnFacilities(Electricity, ElectricityLocations, "Electricity (Clone)");

        //int locationCount = ElectricityLocations.Count;
        //for(int i = 0; i < locationCount; i++)
        //{
        //    // Make a copy of the original
        //    GameObject tempElec = Instantiate(Electricity);
        //    //GameObject tempElec = Instantiate(TestHex);
        //
        //    // Put it in the right spot
        //    tempElec.transform.position = new Vector3(ElectricityLocations[i].x * SizeOfBox.x + Offset.x, -1 * ElectricityLocations[i].y * SizeOfBox.y + Offset.y-5.0f, 0);
        //    //tempElec.transform.eulerAngles = new Vector3(0,180,270);
        //
        //
        //    // Set the Material then place it in the right spot <-- Uncomment if using hexes
        //    //tempElec.GetComponent<MeshRenderer>().material = HexMaterials[4];
        //
        //    //tempElec.transform.SetParent(canvas.transform, false);
        //    tempElec.transform.SetParent(Map.transform, false);
        //    tempElec.name = "Electricity (Clone)";
        //}
        //Electricity.SetActive(false);

        // Convert to water
        int locationCount = WaterLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {

            // Instantiate the hex
            //GameObject tempHex = Instantiate(TestHex);
            GameObject tempHex = Instantiate(Water);


            // Put it in a random spot
            tempHex.transform.position = new Vector3(WaterLocations[i].x * SizeOfBox.x + Offset.x, -1 * WaterLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            //tempHex.transform.eulerAngles = new Vector3(0, 180, 90);
            //tempHex.transform.localScale = new Vector3(10000, 10000, 10000);

            // Set the material
            //Unity.Mathematics.Random rng = new Unity.Mathematics.Random();
            //int tempInt = rng.NextInt(0, HexMaterials.Count-1);
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[tempInt];
            //Debug.Log(tempInt);
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[i%10];
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[10];<-- Uncomment if using hexes
            //tempHex.GetComponentInChildren<MeshRenderer>().material = HexMaterials[9];
            
            // Set the parent
            tempHex.transform.SetParent(Map.transform, false);
            tempHex.name = "Water (Clone)";
        }
        Water.SetActive(false);

        // Commodities
        locationCount = CommoditiesLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {
            // Instantiate the new commodoty
            //GameObject tempCommodities = Instantiate(TestHex);
            GameObject tempCommodities = Instantiate(Commodities);


            // Set the position and the rotation
            tempCommodities.transform.position = new Vector3(CommoditiesLocations[i].x * SizeOfBox.x + Offset.x, -1 * CommoditiesLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            //tempCommodities.transform.eulerAngles = new Vector3(0, 180, 90);

            // Set the material<-- Uncomment if using hexes
            //tempCommodities.GetComponentInChildren<MeshRenderer>().material = HexMaterials[1];

            // Set the parent
            tempCommodities.transform.SetParent(Map.transform, false);
            tempCommodities.name = "Commodities (Clone)";
        }
        Commodities.SetActive(false);

        // Communications
        locationCount = CommunicationsLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // Instantiate the new commodoty
            //GameObject tempCommunications = Instantiate(TestHex);
            GameObject tempCommunications = Instantiate(Communications);


            // Set the position and the rotation
            tempCommunications.transform.position = new Vector3(CommunicationsLocations[i].x * SizeOfBox.x + Offset.x, -1 * CommunicationsLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            //tempCommunications.transform.eulerAngles = new Vector3(0, 180, 90);

            // Set the material <-- Uncomment if using hexes
            //tempCommunications.GetComponentInChildren<MeshRenderer>().material = HexMaterials[2];

            // Set the parent
            tempCommunications.transform.SetParent(Map.transform, false);
            tempCommunications.name = "Communications (Clone)";
        }
        Communications.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
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

    void SpawnFacilities(GameObject baseFacility, List<float2> locations, string name)
    {
        int locationCount = locations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            GameObject tempFacility = Instantiate(baseFacility);
            //GameObject fire = Instantiate(TestHex);

            // place
            //fire.transform.position = new Vector3(FireDeptLocations[i].x*SizeOfBox.x+Offset.x, -1*FireDeptLocations[i].y*SizeOfBox.y+Offset.y-5.0f + 240.0f, 0);
            // Old Formula ^^

            if (mapScalar != new Vector2(1920.0f * 0.66f, 1080))
            {
                Vector3 tempVec = new Vector3(0, 0, 0);
                if (locations[i].x < 0.0f)
                {
                    float scaleCorrection = math.abs(locations[i].x) / (mapScalar.x / 2.0f);
                    tempVec.x = locations[i].x - ((OGDeltaScalar.x * 0.66f) * scaleCorrection);
                }
                else
                {
                    float scaleCorrection = math.abs(locations[i].x) / (mapScalar.x / 2.0f);
                    tempVec.x = locations[i].x + ((OGDeltaScalar.x * 0.66f) * scaleCorrection);
                }
                if (FireDeptLocations[i].y < 0.0f)
                {
                    float scaleCorrection = math.abs(locations[i].y) / (mapScalar.y / 2.0f);
                    tempVec.y = locations[i].y - ((OGDeltaScalar.y * 0.66f) * scaleCorrection);

                }
                else
                {
                    float scaleCorrection = math.abs(locations[i].y) / (mapScalar.y / 2.0f);

                    tempVec.y = locations[i].y + ((OGDeltaScalar.y * 0.66f) * scaleCorrection);

                }
                tempFacility.transform.position = tempVec;

            }
            else
            {
                tempFacility.transform.position = new Vector3(locations[i].x, locations[i].y, 0);
            }

            //fire.transform.SetParent (canvas.transform,false);

            // Set the material then parent it to the Map
            //fire.GetComponent<MeshRenderer>().material = HexMaterials[10];

            tempFacility.transform.SetParent(Map.transform, false);
            tempFacility.name = name;
        }
        baseFacility.SetActive(false);
    }

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
        baseFacility.SetActive(false);
    }
}
