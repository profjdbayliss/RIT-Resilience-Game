using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FacilityEffect
{
    None,
    Backdoor,
    Fortify
}

public class Facility : MonoBehaviour
{
    public enum FacilityName
    {
        Production,
        Transmission,
        Distribution
    };

    public FacilityName facilityName;
    public PlayerSector[] products;
    public GameObject facilityCanvas;

    private int maxPhysicalPoints, maxFinacialPoints, maxNetworkPoints;
    private int physicalPoints, finacialPoints, networkPoints;
    private TextMeshProUGUI[] pointsUI;

    public FacilityEffect effect;

    public bool isDown;

    // Start is called before the first frame update
    public void Initialize()
    {
        facilityCanvas = this.transform.gameObject;
        products = new PlayerSector[3];
        pointsUI = new TextMeshProUGUI[3];
        effect = FacilityEffect.None;

        for(int i = 0; i < 3; i++)
        {
            pointsUI[i] = facilityCanvas.transform.Find("Points").GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdateUI();
    }

    public void ChangeFacilityPoints(string target, int value)
    {
        target = target.ToLower().Trim();
        switch (target)
        {

            case "physical": 
                physicalPoints += value;
                physicalPoints = (physicalPoints > maxPhysicalPoints) ? maxPhysicalPoints : (physicalPoints < 0) ? 0 : physicalPoints; //If any problems check here
                                           // if >max                  //Set to max        //else if <0      //Set to 0  //Else set self
                break;
            case "finacial": 
                finacialPoints += value;
                finacialPoints = (finacialPoints > maxFinacialPoints) ? maxFinacialPoints : (finacialPoints < 0) ? 0 : finacialPoints;
                break;
            case "network": 
                networkPoints += value;
                networkPoints = (networkPoints > maxNetworkPoints) ? maxNetworkPoints : (networkPoints < 0) ? 0 : networkPoints;
                break;
        }

        if (physicalPoints == 0 || finacialPoints == 0 || networkPoints == 0) { isDown = true; }
        else { isDown = false; }

        UpdateUI();
    }

    public void SetFacilityPoints(int physical, int finacial, int network)
    {
        maxPhysicalPoints = physicalPoints = physical;
        maxFinacialPoints = finacialPoints = finacial;
        maxNetworkPoints = networkPoints = network;

        UpdateUI();
    }

    public void AddOrRemoveEffect(FacilityEffect effectToAdd, bool isAddingEffect)
    {
        if (isAddingEffect)
            effect = effectToAdd;
        else
            effect = FacilityEffect.None;
    }

    private void UpdateUI()
    {
        pointsUI[0].text = physicalPoints.ToString();
        pointsUI[1].text = finacialPoints.ToString();
        pointsUI[2].text = networkPoints.ToString();

        if(isDown)
        {
            // TODO: Change UI to show that the facility is down
        }
    }
}
