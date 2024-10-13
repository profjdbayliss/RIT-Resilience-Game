using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Facility : MonoBehaviour {
    public enum FacilityType {
        None,
        Production,
        Transmission,
        Distribution
    };


    public FacilityType facilityType;
    public string facilityName;
    public PlayerSector[] dependencies;
    //public GameObject facilityCanvas;
    public Sector sectorItsAPartOf;

    public Image[] dependencyIcons;

    private int maxPhysicalPoints, maxFinacialPoints, maxNetworkPoints;
    private int physicalPoints, finacialPoints, networkPoints;

    // private TextMeshProUGUI[] pointsUI;
    [SerializeField] private Transform pointsParent;
    [SerializeField] private Image[] pointImages;
    private const int MAX_POINTS = 3;
    [SerializeField] private TextMeshProUGUI facilityNameText;
    [SerializeField] private Button facilitySelectionButton;
    [SerializeField] private Image facilityBoxImage;
    public FacilityEffectManager effectManager;
    private HoverActivateObject hoverEffect;
    [SerializeField] private Material outlineMat;
    [SerializeField] private Image downedOverlay;


    // public FacilityEffect effect;
    //   public bool effectNegated;

    public bool IsDown { get; private set; }
    // public bool IsFortified { get; set; } = false;
    // public bool IsBackdoored { get; set; } = false;


    // Start is called before the first frame update
    public void Initialize() {
        hoverEffect = GetComponent<HoverActivateObject>();
        effectManager = GetComponent<FacilityEffectManager>();
        // facilityCanvas = this.transform.gameObject;
        dependencies = new PlayerSector[3];
        // pointsUI = new TextMeshProUGUI[3];
        //   effect = FacilityEffect.None;
        //   effectNegated = false;

        //for (int i = 0; i < 3; i++)
        //{
        //    pointsUI[i] = facilityCanvas.transform.Find("Points").GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
        //}
        SetupPointImages();
        UpdateUI();
    }
    public void UpdateForNextActionPhase() {
        effectManager.UpdateForNextActionPhase();
    }
    public bool IsFortified() {
        return effectManager.IsFortified();
    }
    private void SetupPointImages() {
        pointImages = new Image[MAX_POINTS * 6];

        //physical
        var physicalPoints = pointsParent.GetChild(0); // PhysPointParent
        for (int i = 0; i < physicalPoints.childCount; i++) {
            var emptyPoint = physicalPoints.GetChild(i).GetChild(0).GetComponent<Image>(); // EmptyPoint
            var filledPoint = physicalPoints.GetChild(i).GetChild(1).GetComponent<Image>(); // FilledPoint

            pointImages[i * 2] = emptyPoint; // Even index for EmptyPoint
            pointImages[i * 2 + 1] = filledPoint; // Odd index for FilledPoint
        }



        //network
        var networkPoints = pointsParent.GetChild(1); // NetworkPointParent
        for (int i = 0; i < networkPoints.childCount; i++) {
            var emptyPoint = networkPoints.GetChild(i).GetChild(0).GetComponent<Image>();
            var filledPoint = networkPoints.GetChild(i).GetChild(1).GetComponent<Image>();

            pointImages[MAX_POINTS * 2 + i * 2] = emptyPoint;
            pointImages[MAX_POINTS * 2 + i * 2 + 1] = filledPoint;
        }

        //financial
        var financialPoints = pointsParent.GetChild(2); // FinancialPointParent
        for (int i = 0; i < financialPoints.childCount; i++) {
            var emptyPoint = financialPoints.GetChild(i).GetChild(0).GetComponent<Image>();
            var filledPoint = financialPoints.GetChild(i).GetChild(1).GetComponent<Image>();

            pointImages[MAX_POINTS * 4 + i * 2] = emptyPoint;
            pointImages[MAX_POINTS * 4 + i * 2 + 1] = filledPoint;
        }
    }



    public void UpdatePointsUI() {
        UpdatePointTypeUI(0, physicalPoints, maxPhysicalPoints);
        UpdatePointTypeUI(2, finacialPoints, maxFinacialPoints);
        UpdatePointTypeUI(1, networkPoints, maxNetworkPoints);
    }

    private void UpdatePointTypeUI(int typeIndex, int currentPoints, int maxPoints) {
        for (int i = 0; i < MAX_POINTS; i++) {
            bool shouldShow = i < maxPoints;
            bool isFilled = i < currentPoints;

            // Each point has two images: Empty (index i*2) and Filled (index i*2+1)
            SetImageAlpha(pointImages[typeIndex * MAX_POINTS * 2 + i * 2], shouldShow ? 1 : 0);     // Empty point
            SetImageAlpha(pointImages[typeIndex * MAX_POINTS * 2 + i * 2 + 1], isFilled ? 1 : 0);   // Filled point
        }
    }


    private void SetImageAlpha(Image image, float alpha) {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
    public void UpdateNameText() {
        facilityNameText.text = facilityName;
    }
    public void ChangeFacilityPoints(FacilityEffectTarget target, int value) {
        Debug.Log($"Changing {target} points by {value} for facility {facilityName}");

        void UpdatePoints(ref int points, int maxPoints) {
            points = Mathf.Clamp(points + value, 0, maxPoints);
        }
        switch (target) {
            case FacilityEffectTarget.Physical:
                UpdatePoints(ref physicalPoints, maxPhysicalPoints);
                break;
            case FacilityEffectTarget.Financial:
                UpdatePoints(ref finacialPoints, maxFinacialPoints);
                break;
            case FacilityEffectTarget.Network:
                UpdatePoints(ref networkPoints, maxNetworkPoints);
                break;
            case FacilityEffectTarget.NetworkPhysical:
                UpdatePoints(ref networkPoints, maxNetworkPoints);
                UpdatePoints(ref physicalPoints, maxPhysicalPoints);
                break;
            case FacilityEffectTarget.FinancialPhysical:
                UpdatePoints(ref finacialPoints, maxFinacialPoints);
                UpdatePoints(ref physicalPoints, maxPhysicalPoints);
                break;
            case FacilityEffectTarget.FinancialNetwork:
                UpdatePoints(ref finacialPoints, maxFinacialPoints);
                UpdatePoints(ref networkPoints, maxNetworkPoints);
                break;
            case FacilityEffectTarget.All:
                UpdatePoints(ref physicalPoints, maxPhysicalPoints);
                UpdatePoints(ref finacialPoints, maxFinacialPoints);
                UpdatePoints(ref networkPoints, maxNetworkPoints);
                break;

        }

        Debug.Log($"Facility {facilityName} now has {physicalPoints} physical points, {finacialPoints} financial points, and {networkPoints} network points.");

        IsDown = (physicalPoints == 0 || finacialPoints == 0 || networkPoints == 0);
        UpdateUI();
    }
    public bool HasRemovableEffects(PlayerTeam opponentTeam, bool removePointsPerTurn = true) {

        return opponentTeam switch {
            PlayerTeam.Blue => effectManager.HasEffectOfType(FacilityEffectType.Fortify),
            PlayerTeam.Red => effectManager.HasEffectOfType(FacilityEffectType.Backdoor) ||
                (removePointsPerTurn && effectManager.HasEffectOfType(FacilityEffectType.ModifyPointsPerTurn)),
            _ => false
        };
    }

    public bool HasEffectOfType(FacilityEffectType type) {
        return effectManager.HasEffectOfType(type);
    }

    public void SetupFacilityPoints(int physical, int finacial, int network) {
        maxPhysicalPoints = physicalPoints = physical;
        maxFinacialPoints = finacialPoints = finacial;
        maxNetworkPoints = networkPoints = network;

        UpdateUI();
    }

    //adds or remove effect by the string in the csv file 
    public void AddRemoveEffectsByIdString(string idString, bool isAdding, PlayerTeam team) {
        var effects = FacilityEffect.CreateEffectsFromID(idString);
        effects.ForEach(effect => {
            effect.CreatedByTeam = team;
            effectManager.AddRemoveEffect(effect, isAdding);
        });
    }
    public bool TryRemoveEffect(FacilityEffect effect) {
        return effectManager.TryRemoveEffect(effect);
    }
    public bool TryRemoveEffectByType(FacilityEffectType effect) {
        return effectManager.TryRemoveEffectByType(effect);
    }
    private void UpdateUI() {
        //pointsUI[0].text = physicalPoints.ToString();
        //pointsUI[1].text = finacialPoints.ToString();
        //pointsUI[2].text = networkPoints.ToString();
        UpdatePointsUI();
        if (IsDown) {
            downedOverlay.enabled = true;
        }
        else {
            downedOverlay.enabled = false;
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


    public void DebugAddNewEffect() {
        effectManager.DebugAddEffect();
    }
    public void DebugAddSpecificEffect(string effect) {
        effectManager.DebugAddEffect(effect);
    }
    public void EnableFacilitySelection() {

        facilityBoxImage.material = outlineMat;
        facilitySelectionButton.interactable = true;
    }
    public void DisableFacilitySelection() {
        facilityBoxImage.material = null;
        facilitySelectionButton.interactable = false;
        hoverEffect.DeactivateHover();
    }
    //called by the button
    public void AddFacilityToSectorSelection() {
        sectorItsAPartOf.AddFacilityToSelection(this);
        hoverEffect.ActivateHover();
    }

}
