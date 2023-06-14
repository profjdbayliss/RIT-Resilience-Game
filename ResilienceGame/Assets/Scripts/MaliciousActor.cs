using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaliciousActor : MonoBehaviour
{
    // Establish necessary fields
    public float funds;
    public GameObject targetFacility;

    // Start is called before the first frame update
    void Start()
    {
        funds = 1000.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CompromiseWorkers()
    {
        // Lower associated value

    }

    public void CompromiseIT()
    {
        // Lower associated value

    }

    public void CompromiseOT()
    {
        // Lower associated value

    }

    public void CompromisePhysSec()
    {
        // Lower associated value

    }

    public void CompromiseFunding()
    {
        // Lower associated value

    }

    public void ComprpomiseElectricity()
    {
        // Lower associated value

    }

    public void CompromiseWater()
    {
        // Lower associated value

    }

    public void CompromiseFuel()
    {
        // Lower associated value

    }

    public void CompromiseCommunications()
    {
        // Lower associated value

    }

    public void CompromiseHealth()
    {
        // Lower associated value

    }

    public void DataBreach()
    {
        // Lower associated value

    }

    public void GasLineEvent()
    {
        // Lower associated value

    }

    public void ElectricityFlowEvent()
    {
        // Lower associated value

    }

    public void RansomwareEvent()
    {
        // Could be a percent chance of happening based off of the preparedness of a facility??

        // Save the current turn count to the target facility

        // if they do not solve it by X turns (I am imagening 2, maybe 3?), deal X amount of damage

        // can be solved by .... (paying, cracking the ransomware, etc.)
    }
}
