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
    public List<float2> HospitalLocations;
    public List<float2> FireDeptLocations;
    public List<float2> ElectricityLocations;

    private static GameObject canvas;
    private GameObject Hospital;
    private GameObject FireTruck;
    private GameObject Electricity;
    private GameObject CityHall;
   
    // Start is called before the first frame update
    void Start()
    {
        // get the canvas
        canvas = GameObject.Find("Canvas");
        Hospital = GameObject.Find("Hospital");
        FireTruck = GameObject.Find("FireTruck");
        Electricity = GameObject.Find("Electricity");
        CityHall = GameObject.Find("CityHall");

        // take hospital image and place it on canvas at locations specified;
        int locationCount = HospitalLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            GameObject hospital = Instantiate(Hospital);
            // place
            hospital.transform.position = new Vector3(HospitalLocations[i].x*SizeOfBox.x+Offset.x, -1*HospitalLocations[i].y*SizeOfBox.y+Offset.y, 0);
            hospital.transform.SetParent (canvas.transform,false);
        }
        
        locationCount = FireDeptLocations.Count;
        for (int i = 0; i < locationCount; i++)
        {
            // make a copy
            GameObject fire = Instantiate(FireTruck);
            // place
            fire.transform.position = new Vector3(FireDeptLocations[i].x*SizeOfBox.x+Offset.x, -1*FireDeptLocations[i].y*SizeOfBox.y+Offset.y, 0);
            fire.transform.SetParent (canvas.transform,false);
        }
        locationCount = ElectricityLocations.Count;
        for(int i = 0; i < locationCount; i++)
        {
            // Make a copy of the original
            GameObject tempElec = Instantiate(Electricity);
            // Put it in the right spot
            tempElec.transform.position = new Vector3(ElectricityLocations[i].x * SizeOfBox.x + Offset.x, -1 * ElectricityLocations[i].y * SizeOfBox.y + Offset.y, 0);
            tempElec.transform.SetParent(canvas.transform, false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
