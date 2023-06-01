using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlaceIcons : MonoBehaviour
{
    public float2 Offset;
    public float2 SizeOfBox;
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
    private GameObject Hospital;
    private GameObject FireTruck;
    private GameObject Electricity;
    private GameObject CityHall;
    private GameObject Map;

    private GameObject TestHex;
   
    // Start is called before the first frame update
    void Start()
    {
        // get the canvas
        canvas = GameObject.Find("Canvas");
        Hospital = GameObject.Find("Hospital");
        FireTruck = GameObject.Find("FireTruck");
        Electricity = GameObject.Find("Electricity");
        CityHall = GameObject.Find("CityHall");
        Map = GameObject.Find("Map");
        TestHex = GameObject.Find("NewHexPrefab");

        // Scale the map properly
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvas.GetComponent<RectTransform>().rect.height);
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvas.GetComponent<RectTransform>().rect.width*0.66f);
        mapScalar = Map.GetComponent<RectTransform>().sizeDelta;
        OGScalar = new Vector2(1920.0f * 0.66f, 1080);
        OGDeltaScalar = (mapScalar - OGScalar);
        //OGScalar = mapScalar;

        // take hospital image and place it on canvas at locations specified;
        int locationCount = HospitalLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            //GameObject hospital = Instantiate(Hospital);
            GameObject hospital = Instantiate(TestHex);

            // place
            hospital.transform.position = new Vector3(HospitalLocations[i].x*SizeOfBox.x+Offset.x, -1*HospitalLocations[i].y*SizeOfBox.y+Offset.y-5.0f, 0);
            //hospital.transform.localRotation = new Quaternion(0, 0, 0.5f, 0);
            //hospital.transform.rotation = new Quaternion(0, 0, 0.5f, 0);
            Vector3 tempRot = hospital.transform.rotation.eulerAngles;
            tempRot.x = 0;
            tempRot.y = 180;
            tempRot.z = 90.0f;
            hospital.transform.eulerAngles = tempRot;
            Vector2 delta = new Vector2();
            //delta.x = Map.GetComponent<RectTransform>().sizeDelta.x * OGDeltaScalar.x;
            //delta.y = Map.GetComponent<RectTransform>().sizeDelta.y * OGDeltaScalar.y;
            delta.x = OGDeltaScalar.x;
            delta.y = OGDeltaScalar.y;
            hospital.transform.position -= new Vector3(delta.x * 0.33f, delta.y * 0.33f, 0);
            //hospital.transform.position += delta.y;
            float tempX = hospital.transform.position.x * (mapScalar.x * 0.66f);
            float tempY = hospital.transform.position.y * (mapScalar.y);
            //float tempX = hospital.transform.position.x * (canvas.GetComponent<RectTransform>().rect.width * 0.66f);
            //float tempY = hospital.transform.position.y * (canvas.GetComponent<RectTransform>().rect.height);
            //hospital.transform.position = new Vector3(tempX, tempY, 0);

            // Apply the health material 
            hospital.GetComponent<MeshRenderer>().material = HexMaterials[6];

            hospital.transform.SetParent (Map.transform,false);
            hospital.name = "Hospital (Clone)";

            Debug.Log("HOSP LROT: " + hospital.transform.localRotation);
            Debug.Log("HOSP ROT: " + hospital.transform.rotation);
            Debug.Log("HOSP EUA: " + hospital.transform.eulerAngles);

        }

        locationCount = FireDeptLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            //GameObject fire = Instantiate(FireTruck);
            GameObject fire = Instantiate(TestHex);

            // place
            fire.transform.position = new Vector3(FireDeptLocations[i].x*SizeOfBox.x+Offset.x, -1*FireDeptLocations[i].y*SizeOfBox.y+Offset.y-5.0f, 0);
            //fire.transform.SetParent (canvas.transform,false);

            // Set the material then parent it to the Map
            fire.GetComponent<MeshRenderer>().material = HexMaterials[10];

            fire.transform.SetParent(Map.transform, false);
            fire.name = "Fire (Clone)";
        }
        locationCount = ElectricityLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {
            // Make a copy of the original
            //GameObject tempElec = Instantiate(Electricity);
            GameObject tempElec = Instantiate(TestHex);

            // Put it in the right spot
            tempElec.transform.position = new Vector3(ElectricityLocations[i].x * SizeOfBox.x + Offset.x, -1 * ElectricityLocations[i].y * SizeOfBox.y + Offset.y-5.0f, 0);
            tempElec.transform.eulerAngles = new Vector3(0,180,270);
            // Set the Material then place it in the right spot
            tempElec.GetComponent<MeshRenderer>().material = HexMaterials[4];

            //tempElec.transform.SetParent(canvas.transform, false);
            tempElec.transform.SetParent(Map.transform, false);
            tempElec.name = "Electricity (Clone)";
        }

        // Convert to water
        locationCount = WaterLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {

            // Instantiate the hex
            GameObject tempHex = Instantiate(TestHex);


            // Put it in a random spot
            tempHex.transform.position = new Vector3(WaterLocations[i].x * SizeOfBox.x + Offset.x, -1 * WaterLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            tempHex.transform.eulerAngles = new Vector3(0, 180, 90);
            //tempHex.transform.localScale = new Vector3(10000, 10000, 10000);

            // Set the material
            //Unity.Mathematics.Random rng = new Unity.Mathematics.Random();
            //int tempInt = rng.NextInt(0, HexMaterials.Count-1);
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[tempInt];
            //Debug.Log(tempInt);
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[i%10];
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[10];
            tempHex.GetComponentInChildren<MeshRenderer>().material = HexMaterials[9];
            // Set the parent
            tempHex.transform.SetParent(Map.transform, false);
            tempHex.name = "Water (Clone)";
        }

        // Commodities
        locationCount = CommoditiesLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {
            // Instantiate the new commodoty
            GameObject tempCommodoties = Instantiate(TestHex);

            // Set the position and the rotation
            tempCommodoties.transform.position = new Vector3(CommoditiesLocations[i].x * SizeOfBox.x + Offset.x, -1 * CommoditiesLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            tempCommodoties.transform.eulerAngles = new Vector3(0, 180, 90);

            // Set the material
            tempCommodoties.GetComponentInChildren<MeshRenderer>().material = HexMaterials[1];
            
            // Set the parent
            tempCommodoties.transform.SetParent(Map.transform, false);
            tempCommodoties.name = "Commodities (Clone)";
        }

        // Communications
        locationCount = CommunicationsLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // Instantiate the new commodoty
            GameObject tempCommunications = Instantiate(TestHex);

            // Set the position and the rotation
            tempCommunications.transform.position = new Vector3(CommunicationsLocations[i].x * SizeOfBox.x + Offset.x, -1 * CommunicationsLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            tempCommunications.transform.eulerAngles = new Vector3(0, 180, 90);

            // Set the material
            tempCommunications.GetComponentInChildren<MeshRenderer>().material = HexMaterials[2];

            // Set the parent
            tempCommunications.transform.SetParent(Map.transform, false);
            tempCommunications.name = "Communications (Clone)";
        }
    }

    // Update is called once per frame
    void Update()
    {
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, canvas.GetComponent<RectTransform>().rect.height);
        Map.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, canvas.GetComponent<RectTransform>().rect.width * 0.66f);
        Vector2 tempScalar = Map.GetComponent<RectTransform>().sizeDelta;
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
}
