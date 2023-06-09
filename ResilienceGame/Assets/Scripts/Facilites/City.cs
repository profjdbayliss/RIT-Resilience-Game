using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : FacilityV3
{
    // Start is called before the first frame update
    override public void Start()
    {
        base.Start();
        SetFacilityData();
        SetMaterial();
    }

    override public void SetFacilityData()
    {
        feedback = 10;//Random.Range(1, 11);
        hardness = Random.Range(1, 11); //hardness vulnerability to cyber
        maintenence = Random.Range(1, 11); //Maintenence(Age) vulnerability to natural disaster or physical threat
        type = Type.City;

        //internal
        workers = Random.Range(1, 11);
        it_level = Random.Range(1, 11);
        ot_level = Random.Range(1, 11);
        phys_security = Random.Range(1, 11);
        funding = Random.Range(1, 11);

        //external
        electricity = GameObject.FindObjectOfType<ElectricityDistribution>().output_flow;
        water = Random.Range(1, 101);
        fuel = Random.Range(1, 101);
        communications = Random.Range(1, 101);
        commodities = Random.Range(1, 101);
        health = Random.Range(1, 101);
        security = Random.Range(1, 101);
        public_goods = Random.Range(1, 101);
        city_resource = Random.Range(1, 101);

        output_flow = 0f;

        CalculateFlow();
        Update();
    }
}
