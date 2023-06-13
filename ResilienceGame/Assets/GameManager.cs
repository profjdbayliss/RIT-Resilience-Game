using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Establish necessary fields
    public float funds;
    public List<GameObject> Facilities;


    // Start is called before the first frame update
    void Start()
    {
        funds = 1000.0f;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Will want to move to a game manager later
    public void EnableAllOutline(bool toggled)
    {
        FacilityOutline[] allOutlines = GameObject.FindObjectsOfType<FacilityOutline>();
        for (int i = 0; i < allOutlines.Length; i++)
        {
            allOutlines[i].outline.SetActive(toggled);
        }
    }



    public void IncreaseOneFeedback()
    {
        // Need to determine how to select
    }

    public void IncreaseAllFeedback()
    {
        if(funds - 50.0f > 0.0f)
        {
            foreach (GameObject obj in Facilities)
            {
                Debug.Log(obj.GetComponent<FacilityV3>().feedback);
                obj.GetComponent<FacilityV3>().feedback += 1;
            }
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void HireWorkers()
    {

    }

    public void BoostIT()
    {

    }

    public void BoostOT()
    {

    }

    public void ImprovePhysSec()
    {

    }

    public void IncreaseFunding()
    {

    }

    public void BoostElectricity()
    {

    }

    public void BoostWater()
    {

    }

    public void BoostFuel()
    {

    }

    public void BoostCommunications()
    {

    }

    public void BoostHealth()
    {

    }
}
