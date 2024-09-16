using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum FacilityEffect
{
    None,
    Backdoor,
    Fortify,
    MeepleReduction
}

public class Facility : MonoBehaviour
{
    public enum FacilityType
    {
        Production,
        Transmission,
        Distribution
    };


    public FacilityType facilityType;
    public string facilityName;
    public PlayerSector[] dependencies;
    public GameObject facilityCanvas;
    public Sector sectorItsAPartOf;

    public Image[] dependencyIcons;

    private int maxPhysicalPoints, maxFinacialPoints, maxNetworkPoints;
    private int physicalPoints, finacialPoints, networkPoints;
    private TextMeshProUGUI[] pointsUI;
    [SerializeField] private TextMeshProUGUI facilityNameText;

    public FacilityEffect effect;
    public bool effectNegated;

    public bool isDown;

    // Start is called before the first frame update
    public void Initialize()
    {
        facilityCanvas = this.transform.gameObject;
        dependencies = new PlayerSector[3];
        pointsUI = new TextMeshProUGUI[3];
        effect = FacilityEffect.None;
        effectNegated = false;

        for (int i = 0; i < 3; i++)
        {
            pointsUI[i] = facilityCanvas.transform.Find("Points").GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdateUI();
    }
    public void UpdateNameText() {
        facilityNameText.text = facilityName;
    }
    public void ChangeFacilityPoints(string target, int value) {
        target = target.ToLower().Trim();

        switch (target) {
            case "physical":
                physicalPoints += value;
                physicalPoints = Mathf.Clamp(physicalPoints, 0, maxPhysicalPoints);
                break;
            case "finacial":
                finacialPoints += value;
                finacialPoints = Mathf.Clamp(finacialPoints, 0, maxFinacialPoints);
                break;
            case "network":
                networkPoints += value;
                networkPoints = Mathf.Clamp(networkPoints, 0, maxNetworkPoints);
                break;
        }

        // Update isDown based on points
        isDown = (physicalPoints == 0 || finacialPoints == 0 || networkPoints == 0);

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
