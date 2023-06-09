using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // Establish necessary fields
    public float funds = 1000.0f;
    public List<GameObject> Facilities;
    public GameObject seletedFacility;
    public TextMeshProUGUI fundsText;
    public FacilityV3.Type type;
    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        funds = 1000.0f;
        foreach (GameObject fac in gameManager.allFacilities)
        {
            if (fac.GetComponent<FacilityV3>().type == type)
            {
                Facilities.Add(fac);
            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration)
            {
                Facilities.Add(fac);
            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution)
            {
                Facilities.Add(fac);

            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Water)
            {
                Facilities.Add(fac);

            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation)
            {
                Facilities.Add(fac);

            }
            else if (fac.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
            {
                Facilities.Add(fac);
            }
            else
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void IncreaseOneFeedback()
    {
        // Need to determine how to select
        if (funds - 50.0f > 0.0f)
        {
            seletedFacility.GetComponent<FacilityV3>().feedback += 1;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void IncreaseAllFeedback()
    {
        if (funds - 50.0f > 0.0f)
        {
            foreach (GameObject obj in Facilities)
            {
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
        if (funds - 100.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().workers += 5.0f;
            funds -= 100.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostIT()
    {
        if (funds - 50.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().it_level += 5.0f;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostOT()
    {
        if (funds - 50.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().ot_level += 5.0f;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void ImprovePhysSec()
    {
        if (funds - 70.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().phys_security += 7.0f;
            funds -= 70.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void IncreaseFunding()
    {
        if (funds - 150.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().funding += 2.0f;
            funds -= 150.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostElectricity()
    {
        if (funds - 50.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().electricity += 5.0f;
            funds -= 50.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostWater()
    {
        if (funds - 75.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().water += 7.5f;
            funds -= 75.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostFuel()
    {
        if (funds - 75.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().fuel += 7.5f;
            funds -= 75.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostCommunications()
    {
        if (funds - 90.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().communications += 9.0f;
            funds -= 90.0f;
        }
        else
        {
            // Show they are broke
        }
    }

    public void BoostHealth()
    {
        if (funds - 150.0f > 0.0f)
        {
            // Do something
            seletedFacility.GetComponent<FacilityV3>().health += 15.0f;
            funds -= 150.0f;
        }
        else
        {
            // Show they are broke
        }
    }
}
