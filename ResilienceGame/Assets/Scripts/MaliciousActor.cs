using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaliciousActor : MonoBehaviour
{
    // Establish necessary fields
    public float funds = 750.0f;
    public GameObject targetFacility;
    public GameObject ransomwaredFacility;
    public GameManager manager;
    public float ransomwareTurn;

    // Start is called before the first frame update
    void Start()
    {
        funds = 750.0f;
        manager = gameObject.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CompromiseWorkers()
    {
        // Lower associated value
        if((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().workers -= 5.0f;
            funds -= 20.0f;
        }
    }

    public void CompromiseIT()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().it_level -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseOT()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().ot_level -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromisePhysSec()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().phys_security -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseFunding()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().funding -= 2.0f;
            funds -= 20.0f;

        }
    }

    public void ComprpomiseElectricity()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().electricity -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseWater()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().water -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseFuel()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().fuel -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseCommunications()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().communications -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void CompromiseHealth()
    {
        // Lower associated value
        if ((targetFacility != null) && (funds - 20.0f > 0.0f))
        {
            targetFacility.GetComponent<FacilityV3>().health -= 5.0f;
            funds -= 20.0f;

        }
    }

    public void DataBreach()
    {
        // Lower associated value
        if (targetFacility != null)
        {

        }
    }

    public void GasLineEvent()
    {
        // Attack this facility heavily in gas, but affect nearby facilities fuel levels as well
        if (targetFacility != null)
        {
            if(funds - 100.0f >= 0.0f)
            {
                targetFacility.GetComponent<FacilityV3>().fuel -= 15.0f;
                foreach (FacilityV3 fac in targetFacility.GetComponent<FacilityV3>().connectedFacilities)
                {
                    fac.fuel -= 5.0f;
                }
                funds -= 100.0f;
            }

        }
    }

    public void ElectricityFlowEvent()
    {
        // Potentially attack this facility heavily, but affect nearby facilities as well
        if (targetFacility != null)
        {
            if (funds - 100.0f >= 0.0f)
            {
                targetFacility.GetComponent<FacilityV3>().electricity -= 15.0f;
                foreach (FacilityV3 fac in targetFacility.GetComponent<FacilityV3>().connectedFacilities)
                {
                    fac.electricity -= 5.0f;
                }
                funds -= 100.0f;
            }
        }
    }

    public void RansomwareEvent()
    {
        // Could be a percent chance of happening based off of the preparedness of a facility??

        // Save the current turn count to the target facility

        // if they do not solve it by X turns (I am imagening 2, maybe 3?), deal X amount of damage

        // can be solved by .... (paying, cracking the ransomware, etc.)
        if(targetFacility != null)
        {
            if(funds -100.0f >= 0.0f)
            {
                ransomwaredFacility = targetFacility;
                ransomwareTurn = manager.GetComponent<GameManager>().turnCount + 2.0f;
                funds -= 100.0f;
                if ((ransomwaredFacility != null) && (manager.GetComponent<GameManager>().turnCount >= ransomwareTurn))
                {
                    ransomwaredFacility.GetComponent<FacilityV3>().output_flow /= 2.0f;
                    ransomwareTurn = float.MaxValue;
                }
            }

        }
    }
}
