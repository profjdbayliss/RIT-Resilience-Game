using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacilityEvents : MonoBehaviour
{
    public GameManager manager;
    public FacilityV3[] allFacilities;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TriggerEvent(FacilityV3 fac)
    {
        float rng = Random.Range(0, 101f);


        // Cyber
        if(rng >= 90f)
        {
            fac.isDown = true;
            if(rng >= 90f + fac.hardness)
            {
                fac.hardness -= 2;
            }
        }

        // Logic
        else if(rng >= 80f)
        {
            fac.isDown = true;

            if(rng >= (80 + fac.it_level) || rng >= (80 + fac.ot_level))
            {
                fac.it_level -= 1;
                fac.ot_level -= 1;
            }
        }

        // GEO or Physical
        // Geo compares against maintenance
        // Physical compares against maintenace, phys sec, or ot
        else if(rng >= 70f)
        {
            fac.isDown = true;
            float geoOrPhys = Random.Range(0, 2);
            if(geoOrPhys == 1)
            {
                fac.isDown = true;

                if(rng >= (70 + fac.maintenance))
                {
                    fac.maintenance -= 1;
                }
            }
            else
            {
                fac.isDown = true;
                if(rng >= (70 + fac.maintenance) || rng >= (70 + fac.phys_security) || rng >= (70 + fac.ot_level))
                {
                    fac.maintenance -= 1;
                    fac.phys_security -= 1;
                    fac.ot_level -= 1;
                }
            }
        }
        else
        {
            fac.isDown = false;
        }

    }

    public void SpawnEvent()
    {
        allFacilities = GameObject.FindObjectsOfType<FacilityV3>();
        foreach(FacilityV3 fac in allFacilities)
        {
            TriggerEvent(fac);
        }
    }
}
