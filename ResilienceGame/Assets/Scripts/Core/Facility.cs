using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



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
    public Image effectIcon;
    public FacilityEffectManager effectManager;
    // public FacilityEffect effect;
    //   public bool effectNegated;

    public bool isDown;
    public bool IsFortified { get; set; } = false;
    public bool IsBackdoored { get; set; } = false;


    // Start is called before the first frame update
    public void Initialize()
    {
        effectManager = new FacilityEffectManager();
        facilityCanvas = this.transform.gameObject;
        dependencies = new PlayerSector[3];
        pointsUI = new TextMeshProUGUI[3];
     //   effect = FacilityEffect.None;
     //   effectNegated = false;

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

    public bool HasEffect(FacilityEffectType type, FacilityEffectTarget target) {
        return effectManager.HasEffect(type, target);
    }
    public bool HasEffect(int id) {
        return effectManager.HasEffect(id);
    }

    public void SetFacilityPoints(int physical, int finacial, int network)
    {
        maxPhysicalPoints = physicalPoints = physical;
        maxFinacialPoints = finacialPoints = finacial;
        maxNetworkPoints = networkPoints = network;

        UpdateUI();
    }
    public void ToggleEffectImageAlpha() {
        Color color = effectIcon.color;
        var newColor = color.a == 1 ? new Color(color.r, color.g, color.b, 0f) : new Color(color.r, color.g, color.b, 1);
        effectIcon.color = newColor;
    }
    public void NegateEffect(FacilityEffect effectToNegate) {
        effectManager.NegateEffect(effectToNegate);
    }
    //called below after creating the effect from ID
    private void AddOrRemoveEffect(FacilityEffect effectToAdd, bool isAddingEffect, FacilityTeam type)
    {

        if (isAddingEffect) {
            effectManager.AddEffect(effectToAdd);
        }
        else {
            effectManager.RemoveEffectByCreatedId(effectToAdd.CreatedEffectID);
        }
    }
    private void RemoveEffectByCreatedId(int id) {
        effectManager.RemoveEffectByCreatedId(id);
        
    }
    //called by the Card.Play() function
    public void AddRemoveEffectByID(int id, bool isAddingEffect, FacilityTeam team) {
        FacilityEffect effect = FacilityEffect.CreateEffectFromID(id);
        effect.CreatedByTeam = team;
        AddOrRemoveEffect(effect, isAddingEffect, team);
    }
    public void AddRemoveAllEffectsByIdString(string idString, bool isAddingEffect, FacilityTeam team) {
        var effectsToAdd = idString.Split(';').ToList().Select(s => int.Parse(s));
        
        foreach (var effectId in effectsToAdd) {
            AddRemoveEffectByID(effectId, isAddingEffect, team);
        }
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

    public void LogFacilityDebug() {
        StringBuilder facilityInfo = new StringBuilder();
        facilityInfo.Append($"Facility Name: {facilityName} ");
        facilityInfo.Append($"Physical Points: {physicalPoints}/{maxPhysicalPoints} ");
        facilityInfo.Append($"Financial Points: {finacialPoints}/{maxFinacialPoints} ");
        facilityInfo.Append($"Network Points: {networkPoints}/{maxNetworkPoints} ");

        var effects = effectManager.GetEffects();
        if (effects.Count > 0) {
            facilityInfo.Append("Active Effects: ");
            foreach (var effect in effects) {
                facilityInfo.Append(effect.ToString()).Append(" ");
            }
        }
        else {
            facilityInfo.Append("No active effects.");
        }

        Debug.Log(facilityInfo.ToString());
    }
}
