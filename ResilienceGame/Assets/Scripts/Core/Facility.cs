using System.Collections;
using System.Collections.Generic;
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
    public GameObject facilityCanvas;
    public Sector sectorItsAPartOf;

    public Image[] dependencyIcons;

    private int maxPhysicalPoints, maxFinacialPoints, maxNetworkPoints;
    private int physicalPoints, finacialPoints, networkPoints;

    // private TextMeshProUGUI[] pointsUI;
    public Image[][] pointImages; // [0] for Physical, [1] for Financial, [2] for Network
    private const int MAX_POINTS = 3;
    [SerializeField] private TextMeshProUGUI facilityNameText;

    public FacilityEffectManager effectManager;
    // public FacilityEffect effect;
    //   public bool effectNegated;

    public bool isDown;
    public bool IsFortified { get; set; } = false;
    public bool IsBackdoored { get; set; } = false;


    // Start is called before the first frame update
    public void Initialize() {
        effectManager = GetComponent<FacilityEffectManager>();
        facilityCanvas = this.transform.gameObject;
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
    private void SetupPointImages() {
        pointImages = new Image[3][];
        string[] pointTypes = { "PhysicalPoints", "FinancialPoints", "NetworkPoints" };

        for (int i = 0; i < 3; i++) {
            pointImages[i] = new Image[MAX_POINTS * 2]; // 2 images per point (empty and filled)
            Transform pointsParent = transform.Find("Points").Find(pointTypes[i]);

            for (int j = 0; j < MAX_POINTS; j++) {
                Transform pointTransform = pointsParent.Find($"Point{j + 1}");
                pointImages[i][j * 2] = pointTransform.Find("EmptyPoint").GetComponent<Image>();
                pointImages[i][j * 2 + 1] = pointTransform.Find("FilledPoint").GetComponent<Image>();
            }
        }

    }
    public void UpdatePointsUI() {
        UpdatePointTypeUI(0, physicalPoints, maxPhysicalPoints);
        UpdatePointTypeUI(1, finacialPoints, maxFinacialPoints);
        UpdatePointTypeUI(2, networkPoints, maxNetworkPoints);
    }

    private void UpdatePointTypeUI(int typeIndex, int currentPoints, int maxPoints) {
        for (int i = 0; i < MAX_POINTS; i++) {
            bool shouldShow = i < maxPoints;
            bool isFilled = i < currentPoints;

            SetImageAlpha(pointImages[typeIndex][i * 2], shouldShow ? 1 : 0);     // Empty point
            SetImageAlpha(pointImages[typeIndex][i * 2 + 1], isFilled ? 1 : 0);   // Filled point
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
    public void ChangeFacilityPoints(FacilityPointTarget target, int value) {
        Debug.Log($"Changing {target} points by {value} for facility {facilityName}");

        void UpdatePoints(ref int points, int maxPoints) {
            points = Mathf.Clamp(points + value, 0, maxPoints);
        }

        if (target.HasFlag(FacilityPointTarget.Physical) || target == FacilityPointTarget.All)
            UpdatePoints(ref physicalPoints, maxPhysicalPoints);

        if (target.HasFlag(FacilityPointTarget.Financial) || target == FacilityPointTarget.All)
            UpdatePoints(ref finacialPoints, maxFinacialPoints);

        if (target.HasFlag(FacilityPointTarget.Network) || target == FacilityPointTarget.All)
            UpdatePoints(ref networkPoints, maxNetworkPoints);

        Debug.Log($"Facility {facilityName} now has {physicalPoints} physical points, {finacialPoints} financial points, and {networkPoints} network points.");

        isDown = (physicalPoints == 0 || finacialPoints == 0 || networkPoints == 0);
        UpdateUI();
    }

    public bool HasEffectOfType(FacilityEffectType type) {
        return effectManager.HasEffectOfType(type);
    }

    public void SetFacilityPoints(int physical, int finacial, int network) {
        maxPhysicalPoints = physicalPoints = physical;
        maxFinacialPoints = finacialPoints = finacial;
        maxNetworkPoints = networkPoints = network;

        UpdateUI();
    }

    public void NegateEffect(FacilityEffect effectToNegate) {
        effectManager.NegateEffect(effectToNegate);
    }

    public void AddRemoveEffectsByIdString(string idString, bool isAdding, FacilityTeam team) {

        //var effectIds = idString.Split(';').Select(int.Parse).ToList();
        //TODO: change effect csv storage
        var effects = FacilityEffect.CreateEffectsFromID(idString);
        effects.ForEach(effect => {
            effect.CreatedByTeam = team;
            effectManager.AddRemoveEffect(effect, isAdding);
        });


    }


    private void UpdateUI() {
        //pointsUI[0].text = physicalPoints.ToString();
        //pointsUI[1].text = finacialPoints.ToString();
        //pointsUI[2].text = networkPoints.ToString();
        UpdatePointsUI();
        if (isDown) {
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
