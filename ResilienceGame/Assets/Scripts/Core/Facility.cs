using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Facility : NetworkBehaviour {
    public enum FacilityType
    {
        Production,
        Transmission,
        Distribution
    };
    [SyncVar]
    public int UniqueID;


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

    [Server]
    private void RegisterWithGameState() {
        this.UniqueID = GameState.Instance.AddFacility(this);
    }

    // Start is called before the first frame update
    public void Initialize()
    {
        effectManager = new FacilityEffectManager(this);
        facilityCanvas = this.transform.gameObject;
        dependencies = new PlayerSector[3];
        pointsUI = new TextMeshProUGUI[3];
     //   effect = FacilityEffect.None;
     //   effectNegated = false;

        for (int i = 0; i < 3; i++)
        {
            pointsUI[i] = facilityCanvas.transform.Find("Points").GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
        }
        RegisterWithGameState();


        UpdateUI();
    }
    public void UpdateNameText() {
        facilityNameText.text = facilityName;
    }
    public void ChangeFacilityPoints(string target, int value) {
        target = target.ToLower().Trim();
      //  Debug.Log($"Changing {target} points by {value} for facility {facilityName}");
        switch (target) {
            case "physical":
                physicalPoints += value;
                physicalPoints = Mathf.Clamp(physicalPoints, 0, maxPhysicalPoints);
                break;
            case "financial":
                finacialPoints += value;
                finacialPoints = Mathf.Clamp(finacialPoints, 0, maxFinacialPoints);
                break;
            case "network":
                networkPoints += value;
                networkPoints = Mathf.Clamp(networkPoints, 0, maxNetworkPoints);
                break;
        }
    //    Debug.Log($"Facility {facilityName} now has {physicalPoints} physical points, {finacialPoints} financial points, and {networkPoints} network points.");
        // Update isDown based on points
        isDown = (physicalPoints == 0 || finacialPoints == 0 || networkPoints == 0);

        UpdateUI();
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
    
    public void AddRemoveEffectsByIdString(string idString, bool isAdding, FacilityTeam team) {
        var effectIds = idString.Split(';').Select(int.Parse).ToList();
        foreach (var id in effectIds) {
            FacilityEffect effect = FacilityEffect.CreateEffectFromID(id);
            effect.CreatedByTeam = team;
            effectManager.AddRemoveEffect(effect, isAdding);
        }
    }
    public void UpdateEffectUI(FacilityEffect effect) {
        // Update UI based on effect type
        switch (effect.EffectType) {
            case FacilityEffectType.Backdoor:
                effectIcon.sprite = Sector.EffectSprites[0];
                break;
            case FacilityEffectType.Fortify:
                effectIcon.sprite = Sector.EffectSprites[1];
                break;
                // Add more cases for other effect types
        }
        ToggleEffectImageAlpha();
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
        facilityInfo.Append($"Facility UID: {UniqueID} ");
        facilityInfo.Append($"Physical Points: {physicalPoints}/{maxPhysicalPoints} ");
        facilityInfo.Append($"Financial Points: {finacialPoints}/{maxFinacialPoints} ");
        facilityInfo.Append($"Network Points: {networkPoints}/{maxNetworkPoints} ");

        var effects = effectManager.GetEffects();
        if (effects.Count > 0) {
            facilityInfo.Append($"Active Effects ({effects.Count}): ");
            foreach (var effect in effects) {
                facilityInfo.Append("\n  ").Append(effect.ToString());
            }
        }
        else {
            facilityInfo.Append("No active effects.");
        }

        Debug.Log(facilityInfo.ToString());
    }
}
