using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : FacilityV3
{
    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        SetMaterial();
        FindFacilities();
        Invoke("SetFacilityData", 5);
    }

    public FacilityV3 FindClosestFacilityElectricity()
    {
        FacilityV3[] gos;
        gos = GameObject.FindObjectsOfType<ElectricityDistribution>();
        FacilityV3 closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (FacilityV3 go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        connectedFacilities.Add(closest);
        return closest;
    }
    public FacilityV3 FindClosestFacilityWater()
    {
        FacilityV3[] gos;
        gos = GameObject.FindObjectsOfType<Water>();
        FacilityV3 closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (FacilityV3 go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        connectedFacilities.Add(closest);
        return closest;
    }

    override public void SetFacilityData()
    {
        feedback = Random.Range(1, 11);
        hardness = Random.Range(1, 11); //hardness vulnerability to cyber
        maintenance = Random.Range(1, 11); //maintenance(Age) vulnerability to natural disaster or physical threat
        type = Type.Health;

        //internal
        workers = Random.Range(1, 11);
        it_level = Random.Range(1, 11);
        ot_level = Random.Range(1, 11);
        phys_security = Random.Range(1, 11);
        funding = Random.Range(1, 11);

        //external

        fuel = Random.Range(1, 101);
        communications = Random.Range(1, 101);
        commodities = Random.Range(1, 101); 
        health = Random.Range(1, 101);
        security = Random.Range(1, 101); 
        public_goods = Random.Range(1, 101); 
        city_resource = Random.Range(1, 101);

        electricity = FindClosestFacility<ElectricityDistribution>().output_flow;
        water = FindClosestFacility<Water>().output_flow;

        if (FindClosestFacility<ElectricityDistribution>().output_flow <= 0 || FindClosestFacility<Water>().output_flow <= 0)
        {
            Invoke("SearchAgain", 3);
        }

        output_flow = 0f;

        CalculateFlow();
        Update();
    }

    void SearchAgain()
    {
        electricity = FindClosestFacility<ElectricityDistribution>().output_flow;
        water = FindClosestFacility<Water>().output_flow;
    }
}
