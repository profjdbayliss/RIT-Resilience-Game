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
    [SerializeField] private Image effectIcon;
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

    private Color ToggleColorAlpha(Color color) {
        return color.a == 1 ? new Color(color.r, color.g, color.b, 0f) : new Color(color.r, color.g, color.b, 1);
    }
    public void AddOrRemoveEffect(FacilityEffect effectToAdd, bool isAddingEffect)
    {

        if (isAddingEffect) {
            effect = effectToAdd;
            effectIcon.sprite = effectToAdd switch {
                FacilityEffect.Backdoor => Sector.EffectSprites[0],
                FacilityEffect.Fortify => Sector.EffectSprites[1],
                _ => null
            };
            
        }
        else {
            effect = FacilityEffect.None; 
        }
        effectIcon.color = ToggleColorAlpha(effectIcon.color);
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
