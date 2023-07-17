using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricityGeneration : FacilityV3
{
    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        SetFacilityData();
        SetMaterial();
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

    override public void SetFacilityData()
    {
        //internal
        output_flow = 0f;

        feedback = Random.Range(1, 11);
        hardness = Random.Range(1, 11); //hardness vulnerability to cyber
        maintenance = Random.Range(1, 11); //maintenance(Age) vulnerability to natural disaster or physical threat
        type = Type.ElectricityGeneration;

        workers = Random.Range(8, 11);
        it_level = Random.Range(8, 11);
        ot_level = Random.Range(8, 11);
        phys_security = Random.Range(8, 11);
        funding = Random.Range(8, 11); //less efficient more expensive things are for players

        electricity = Random.Range(1, 101);
        water = Random.Range(1, 101);
        fuel = Random.Range(1, 101);
        communications = Random.Range(1, 101);
        commodities = Random.Range(1, 101);
        health = Random.Range(1, 101);
        security = Random.Range(1, 101);
        public_goods = Random.Range(1, 101);
        city_resource = Random.Range(1, 101);
        //CalculateFlow();
        FindClosestFacility<ElectricityDistribution>();
        //FindClosestFacilityElectricity();
        Update();
    }

    override public void CalculateFlow()
    {
        //between 3-5
        if (workers <= 5f && workers >= 3f || it_level <= 5f && it_level >= 3f || ot_level <= 5f && ot_level >= 3f || phys_security <= 5f && phys_security >= 3f || funding <= 5f && funding >= 3f)
        {
            //set to 50% flow
            output_flow = 0.5f * 100f;
        }

        //else if any of the fields are 2 or below
        else if (workers <= 2f || it_level <= 2f || ot_level <= 2f || phys_security <= 2f || funding <= 2f)
        {
            //set to 0%
            output_flow = 0f;
        }

        //Otherwise calculate the flow as is 
        else
        {
            output_flow = ((workers + it_level + ot_level + phys_security + funding) / 50f) * 100;
        }
    }

    override public void Update()
    {
        FeedbackPanel();
        CalculateFlow();
    }
}
