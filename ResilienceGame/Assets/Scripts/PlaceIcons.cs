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
    public List<float2> TestHexLocations;
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
            GameObject hospital = Instantiate(Hospital);
            // place
            hospital.transform.position = new Vector3(HospitalLocations[i].x*SizeOfBox.x+Offset.x, -1*HospitalLocations[i].y*SizeOfBox.y+Offset.y-5.0f, 0);
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
            hospital.transform.SetParent (Map.transform,false);
        }
        
        locationCount = FireDeptLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            GameObject fire = Instantiate(FireTruck);
            // place
            fire.transform.position = new Vector3(FireDeptLocations[i].x*SizeOfBox.x+Offset.x, -1*FireDeptLocations[i].y*SizeOfBox.y+Offset.y-5.0f, 0);
            //fire.transform.SetParent (canvas.transform,false);
            fire.transform.SetParent(Map.transform, false);
        }
        locationCount = ElectricityLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {
            // Make a copy of the original
            GameObject tempElec = Instantiate(Electricity);
            // Put it in the right spot
            tempElec.transform.position = new Vector3(ElectricityLocations[i].x * SizeOfBox.x + Offset.x, -1 * ElectricityLocations[i].y * SizeOfBox.y + Offset.y-5.0f, 0);
            //tempElec.transform.SetParent(canvas.transform, false);
            tempElec.transform.SetParent(Map.transform, false);
        }
        locationCount = TestHexLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {

            // Instantiate the hex
            GameObject tempHex = Instantiate(TestHex);


            // Put it in a random spot
            tempHex.transform.position = new Vector3(TestHexLocations[i].x * SizeOfBox.x + Offset.x, -1 * TestHexLocations[i].y * SizeOfBox.y + Offset.y - 5.0f, 0);
            tempHex.transform.rotation = new Quaternion(0, 180, 0, 0);
            //tempHex.transform.localScale = new Vector3(10000, 10000, 10000);

            // Set the material
            //Unity.Mathematics.Random rng = new Unity.Mathematics.Random();
            //int tempInt = rng.NextInt(0, HexMaterials.Count-1);
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[tempInt];
            //Debug.Log(tempInt);
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[i%10];
            //tempHex.GetComponent<MeshRenderer>().material = HexMaterials[10];
            tempHex.GetComponentInChildren<MeshRenderer>().material = HexMaterials[10];
            // Set the parent
            tempHex.transform.SetParent(Map.transform, false);
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
