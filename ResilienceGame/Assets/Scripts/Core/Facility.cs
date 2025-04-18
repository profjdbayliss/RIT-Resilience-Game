using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class Facility : MonoBehaviour {

    public Action OnPointsChanged;
    public enum FacilityType {
        None,
        Production,
        Transmission,
        Distribution
    };
    public enum FacilityDependencyMenuState {
        Open,
        Closed,
        LockedOpen
    }


    public FacilityType facilityType;
    public string facilityName;
    public SectorType[] dependencies;
    public List<Sector> connectedSectors;
    public int downedConnections = 0;
    //public GameObject facilityCanvas;
    public Sector sectorItsAPartOf;

    public Image[] dependencyIcons;
    public Image[] dependencyXs;


    public int MaxPhysicalPoints { get; private set; }
    public int MaxFinancialPoints { get; private set; }
    public int MaxNetworkPoints { get; private set; }
    private int tempPhysPoints, tempFinPoints, tempNetPoints;
    public int[] Points = new int[3];
    //public int Points[0];
    //  public int FinancialPoints;
    //  public int NetworkPoints;

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
    [SerializeField] private HoverMoveUI sectorPopoutMenu;
    [SerializeField] FacilityPointsUIController pointsUIController;

    [SerializeField] private GameObject backDoorIcon;

    // public FacilityEffect effect;
    //   public bool effectNegated;
    public bool WasEverDowned = false;
    public bool IsDown => Points[0] == 0 ||
                            Points[2] == 0 ||
                            Points[1] == 0 ||
                            downedConnections == 3;
    public bool IsDamaged => Points[0] < MaxPhysicalPoints ||
                            Points[1] < MaxNetworkPoints ||
                            Points[2] < MaxFinancialPoints;
    public bool HasMaxPhysicalPoints => Points[0] == MaxPhysicalPoints;
    public bool HasMaxNetworkPoints => Points[1] == MaxNetworkPoints;
    public bool HasMaxFinancialPoints => Points[2] == MaxFinancialPoints;

    //Audio, mostly for loosing a point 
    [Header("SFX")]
    [SerializeField] private AudioSource audio;
    [SerializeField] private AudioClip loosePoint;

    private void Update() {
        if (IsDown && !WasEverDowned)
            WasEverDowned = true;

        if (effectManager.HasEffectOfType(FacilityEffectType.Backdoor))
        {
            backDoorIcon.SetActive(true);
        }
        else
        {
            backDoorIcon.SetActive(false);
        }
    }


    // Start is called before the first frame update
    public void Initialize() {
        hoverEffect = GetComponent<HoverActivateObject>();
        effectManager = GetComponent<FacilityEffectManager>();
        pointsUIController.Init(this);
        // facilityCanvas = this.transform.gameObject;
        dependencies = new SectorType[3];
        // pointsUI = new TextMeshProUGUI[3];
        //   effect = FacilityEffect.None;
        //   effectNegated = false;

        //for (int i = 0; i < 3; i++)
        //{
        //    pointsUI[i] = facilityCanvas.transform.Find("Points").GetChild(i).GetComponentInChildren<TextMeshProUGUI>();
        //}
        tempFinPoints = tempNetPoints = tempPhysPoints = 0;
        //SetupPointImages();
        UpdateUI();
    }
    //Init the facility with the connection icons and references
    public void ProcessConnections(List<Sprite> icons) {
        connectedSectors = new List<Sector>();
        try {
            for (int i = 0; i < dependencies.Length; i++) {
                //  Debug.Log($"Facility {facilityName} has dependency {dependencies[i]}");
                var sec = GameManager.Instance.AllSectors[dependencies[i]];
                //  Debug.Log($"Found {sec.sectorName} from game manager AllSectors");
                connectedSectors.Add(sec);
                dependencyIcons[i].sprite = icons[(int)sec.sectorName];
                dependencyXs[i].enabled = false;
            }
        }
        catch (System.Exception e) {
            Debug.LogError(e);
        }
    }
    public void AddEffectToConnectedSectors(string idString, PlayerTeam createdBy, int playerId) {
        foreach (var sector in connectedSectors) {
            sector.AddEffectToFacilities(idString, createdBy, playerId);
        }
    }
    public void UpdateForNextActionPhase() {
        effectManager.UpdateForNextActionPhase();
    }
    public bool IsFortified() {
        return effectManager.IsFortified();
    }
    private void SetupPointImages() {
        pointImages = new Image[MAX_POINTS * 6];

        // Physical points (counting from bottom to top)
        var physicalPoints = pointsParent.GetChild(0); // PhysPointParent
        for (int i = 0; i < physicalPoints.childCount; i++) {
            var reverseIndex = physicalPoints.childCount - 1 - i;
            var emptyPoint = physicalPoints.GetChild(reverseIndex).GetChild(0).GetComponent<Image>(); // EmptyPoint
            var filledPoint = physicalPoints.GetChild(reverseIndex).GetChild(1).GetComponent<Image>(); // FilledPoint

            pointImages[i * 2] = emptyPoint; // Even index for EmptyPoint
            pointImages[i * 2 + 1] = filledPoint; // Odd index for FilledPoint
        }

        // Network points (counting from bottom to top)
        var networkPoints = pointsParent.GetChild(1); // NetworkPointParent
        for (int i = 0; i < networkPoints.childCount; i++) {
            var reverseIndex = networkPoints.childCount - 1 - i;
            var emptyPoint = networkPoints.GetChild(reverseIndex).GetChild(0).GetComponent<Image>();
            var filledPoint = networkPoints.GetChild(reverseIndex).GetChild(1).GetComponent<Image>();

            pointImages[MAX_POINTS * 2 + i * 2] = emptyPoint;
            pointImages[MAX_POINTS * 2 + i * 2 + 1] = filledPoint;
        }

        // Financial points (counting from bottom to top)
        var financialPoints = pointsParent.GetChild(2); // FinancialPointParent
        for (int i = 0; i < financialPoints.childCount; i++) {
            var reverseIndex = financialPoints.childCount - 1 - i;
            var emptyPoint = financialPoints.GetChild(reverseIndex).GetChild(0).GetComponent<Image>();
            var filledPoint = financialPoints.GetChild(reverseIndex).GetChild(1).GetComponent<Image>();

            pointImages[MAX_POINTS * 4 + i * 2] = emptyPoint;
            pointImages[MAX_POINTS * 4 + i * 2 + 1] = filledPoint;
        }
    }

    public void CheckDownedConnections() {
        //Debug.Log("Checking downed connections for " + facilityName);
        downedConnections = 0;
        // bool isDown = IsDown;
        for (int i = 0; i < connectedSectors.Count; i++) {
            if (connectedSectors[i].IsDown) {
                dependencyXs[i].enabled = true;
                downedConnections++;
            }
            else {
                dependencyXs[i].enabled = false;
            }
        }
        //we arent down now but we will be going down based on lack of connections
        //if (!isDown) {
        //    if (downedConnections == 3) {
        //        Debug.Log($"Facility {facilityName} is being downed by lack of connections");

        //       // GameManager.Instance.CheckDownedFacilities();
        //    }
        //}
        //else {
        //    if (downedConnections != 3) {
        //        Debug.Log($"Facility {facilityName} is being brought back up by restored connections");
        //       // GameManager.Instance.CheckDownedFacilities();
        //    }
        //}

        if (downedConnections > 0) {
            sectorPopoutMenu.SetLockedOpen();
        }
        else {
            sectorPopoutMenu.DisableLockedOpen();
        }
        UpdateUI();
        //Debug.Log($"Found {downedConnections} downed connections");
    }



    public void UpdatePointsUI() {
        pointsUIController.UpdateAllPoints();
        //UpdatePointTypeUI(0, Points[0], MaxPhysicalPoints);
        //UpdatePointTypeUI(2, Points[2], MaxFinancialPoints);
        //UpdatePointTypeUI(1, Points[1], MaxNetworkPoints);
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
    public void ActivatePointProtectionUI(bool protectPhys = false, bool protectNet = false, bool protectFin = false) {
        if (protectPhys)
            pointsUIController.EnablePhysProtection();
        if (protectNet)
            pointsUIController.EnableNetProtection();
        if (protectFin)
            pointsUIController.EnableFinProtection();
    }
    public void DisableSinglePointProtection(bool disablePhys = false, bool disableNet = false, bool disableFin = false) {
        if (disablePhys)
            pointsUIController.DisablePhysProtection();
        if (disableNet)
            pointsUIController.DisableNetProtection();
        if (disableFin)
            pointsUIController.DisableFinProtection();
    }
    public void DisableAllPointsProtection() {
        pointsUIController.DisablePhysProtection();
        pointsUIController.DisableNetProtection();
        pointsUIController.DisableFinProtection();
    }

    public void ChangeTemporaryPoints(FacilityEffectTarget target, int value)
    {
        switch(target)
        {
            case FacilityEffectTarget.Physical:
                tempPhysPoints += value;
                break;
            case FacilityEffectTarget.Financial:
                tempFinPoints += value;
                break;
            case FacilityEffectTarget.Network:
                tempNetPoints += value;
                break;
            case FacilityEffectTarget.NetworkPhysical:
                tempNetPoints += value;
                tempPhysPoints += value;
                break;
            case FacilityEffectTarget.FinancialNetwork:
                tempFinPoints += value;
                tempNetPoints += value;
                break;
            case FacilityEffectTarget.FinancialPhysical:
                tempFinPoints += value;
                tempPhysPoints += value;
                break;
            case FacilityEffectTarget.All:
                tempPhysPoints += value;
                tempFinPoints += value;
                tempNetPoints += value;
                break;
        }
        pointsUIController.UpdateTempPoints(tempPhysPoints, tempNetPoints, tempFinPoints);
    }
    public void ChangeFacilityPoints(FacilityEffectTarget target, int createdById, int value) {

        //Check if it's a negative value
        if (value < 0)
        {
            audio.PlayOneShot(loosePoint, 1);
        }

        Debug.Log($"Changing {target} points by {value} for facility {facilityName}");
        List<int> currentPoints = new List<int>(Points);

        void UpdatePoints(ref int points, int maxPoints) {
            points = Mathf.Clamp(points + value, 0, maxPoints);
        }
        switch (target) {
            case FacilityEffectTarget.Physical:
                UpdatePoints(ref Points[0], MaxPhysicalPoints);
                break;
            case FacilityEffectTarget.Financial:
                UpdatePoints(ref Points[2], MaxFinancialPoints);
                break;
            case FacilityEffectTarget.Network:
                UpdatePoints(ref Points[1], MaxNetworkPoints);
                break;
            case FacilityEffectTarget.NetworkPhysical:
                UpdatePoints(ref Points[1], MaxNetworkPoints);
                UpdatePoints(ref Points[0], MaxPhysicalPoints);
                break;
            case FacilityEffectTarget.FinancialPhysical:
                UpdatePoints(ref Points[2], MaxFinancialPoints);
                UpdatePoints(ref Points[0], MaxPhysicalPoints);
                break;
            case FacilityEffectTarget.FinancialNetwork:
                UpdatePoints(ref Points[2], MaxFinancialPoints);
                UpdatePoints(ref Points[1], MaxNetworkPoints);
                break;
            case FacilityEffectTarget.All:
                UpdatePoints(ref Points[0], MaxPhysicalPoints);
                UpdatePoints(ref Points[2], MaxFinancialPoints);
                UpdatePoints(ref Points[1], MaxNetworkPoints);
                break;

        }

        Debug.Log($"Facility {facilityName} now has {Points[0]} physical points, {Points[2]} financial points, and {Points[1]} network points.");
        //add the number of changed points to the player score
        int pointsChanged = currentPoints
            .Select((oldValue, index) => Mathf.Abs(Points[index] - oldValue))
            .Sum();



        ScoreManager.Instance.AddResistancePointsRestored(createdById, pointsChanged);
        OnPointsChanged?.Invoke();

        GameManager.Instance.CheckDownedFacilities();
        UpdateUI();
    }
    //TODO update for new card effects
    public bool HasRemovableEffects(PlayerTeam opponentTeam, bool removePointsPerTurn = true) {

        return opponentTeam switch {
            PlayerTeam.Blue => effectManager.HasEffectOfType(FacilityEffectType.Fortify),
            PlayerTeam.Red => effectManager.HasEffectOfType(FacilityEffectType.Backdoor) ||
                (removePointsPerTurn && effectManager.HasEffectOfType(FacilityEffectType.ModifyPointsPerTurn)),
            _ => false
        };
    }

    public bool HasEffectOfType(FacilityEffectType type) {
        var hasEffect = effectManager.HasEffectOfType(type);
        // Debug.Log($"Checking if facility {facilityName} has effect of type {type} : {hasEffect}");
        return hasEffect;
    }

    public void SetupFacilityPoints(int physical, int finacial, int network) {
        MaxPhysicalPoints = Points[0] = physical;
        MaxFinancialPoints = Points[2] = finacial;
        MaxNetworkPoints = Points[1] = network;

        UpdateUI();
    }

    //adds or remove effect by the string in the csv file 
    public void AddRemoveEffectsByIdString(string idString, bool isAdding, PlayerTeam team, int createdById, int duration = -1) {
        var effects = FacilityEffect.CreateEffectsFromID(idString);
        effects.ForEach(effect => {
            effect.CreatedByTeam = team;
            if (duration != -1) {
                effect.Duration = duration;
            }
            
        });
        Debug.Log($"Adding a new effect in Facility of type {effects[0].EffectType} created by player {createdById}");
        effectManager.AddRemoveEffect(effects, isAdding, createdById);
    }
    public bool TryRemoveEffect(FacilityEffect effect, int createdById) {
        return effectManager.TryRemoveEffect(effect, createdById);
    }
    public bool TryRemoveEffectByType(FacilityEffectType effect, int createdById) {
        return effectManager.TryRemoveEffectByType(effect, createdById);
    }
    private void UpdateUI() {
        //pointsUI[0].text = physicalPoints.ToString();
        //pointsUI[1].text = finacialPoints.ToString();
        //pointsUI[2].text = networkPoints.ToString();
        UpdatePointsUI();
        bool isDown = false;
        if (sectorItsAPartOf.IsSimulated) {
            isDown = !sectorItsAPartOf.SimulatedFacilities[sectorItsAPartOf.facilities.ToList().IndexOf(this)];
        }
        else {
            isDown = IsDown;
        }

        if (isDown) {
            downedOverlay.enabled = true;
        }
        else {
            downedOverlay.enabled = false;
        }
    }

    public void LogFacilityDebug() {
        StringBuilder facilityInfo = new StringBuilder();
        facilityInfo.Append($"Facility Name: {facilityName} ");
        facilityInfo.Append($"Physical Points: {Points[0]}/{MaxPhysicalPoints} ");
        facilityInfo.Append($"Financial Points: {Points[2]}/{MaxFinancialPoints} ");
        facilityInfo.Append($"Network Points: {Points[1]}/{MaxNetworkPoints} ");
        facilityInfo.Append($"Connected Sectors: ");
        foreach (var sector in connectedSectors) {
            facilityInfo.Append(sector.sectorName).Append(", ");
        }
        facilityInfo.AppendLine();

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

        facilityInfo.AppendLine($"\nFacility is being {(sectorItsAPartOf.IsSimulated ? "simulated" : "played")} ");
        facilityInfo.AppendLine($"Facility is down: " +
            $"{(sectorItsAPartOf.IsSimulated ? sectorItsAPartOf.SimulatedFacilities[sectorItsAPartOf.facilities.ToList().IndexOf(this)] : IsDown)}");

        if (downedConnections == 3) {
            facilityInfo.AppendLine("Facility is down due to lack of connections");
        }
        else if (IsDown) {
            facilityInfo.AppendLine("Facility is down due to lack of points");
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
