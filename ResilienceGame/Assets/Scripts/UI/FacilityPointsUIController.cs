using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FacilityPointsUIController : MonoBehaviour {
    public enum FacilityPointState {
        Empty,
        Full,
        Temp,
        Disabled
    }

    private class FacilityPointUIElement {
        Image empty;
        Image full;
        Image temp;
        FacilityPointState state;
        public FacilityPointUIElement(Image empty, Image full, Image temp = null) {
            this.empty = empty;
            this.full = full;
            this.temp = temp;
        }
        public void SetState(FacilityPointState state) {
            this.state = state;
            empty.enabled = state == FacilityPointState.Empty;
            full.enabled = state == FacilityPointState.Full;
            if (temp != null)
                temp.enabled = state == FacilityPointState.Temp;
        }
    }

    [SerializeField] List<GameObject> physPoints = new List<GameObject>();
    [SerializeField] List<GameObject> netPoints = new List<GameObject>();
    [SerializeField] List<GameObject> finPoints = new List<GameObject>();
    [SerializeField] Facility facility;

    [SerializeField] List<FacilityPointUIElement> physPointsUI = new List<FacilityPointUIElement>();
    [SerializeField] List<FacilityPointUIElement> netPointsUI = new List<FacilityPointUIElement>();
    [SerializeField] List<FacilityPointUIElement> finPointsUI = new List<FacilityPointUIElement>();
    // Start is called before the first frame update
    void Start() {
        physPoints.ForEach(pointParent => {
            physPointsUI.Add(
                new FacilityPointUIElement(
                    pointParent.transform.GetChild(0).GetComponent<Image>(),
                    pointParent.transform.GetChild(1).GetComponent<Image>(),
                    pointParent.transform.childCount > 2 ? pointParent.transform.GetChild(2).GetComponent<Image>() : null));
        });
        netPoints.ForEach(pointParent => {
            netPointsUI.Add(
                new FacilityPointUIElement(
                    pointParent.transform.GetChild(0).GetComponent<Image>(),
                    pointParent.transform.GetChild(1).GetComponent<Image>(),
                    pointParent.transform.childCount > 2 ? pointParent.transform.GetChild(2).GetComponent<Image>() : null));
        });
        finPoints.ForEach(pointParent => {
            finPointsUI.Add(
                new FacilityPointUIElement(
                    pointParent.transform.GetChild(0).GetComponent<Image>(),
                    pointParent.transform.GetChild(1).GetComponent<Image>(),
                    pointParent.transform.childCount > 2 ? pointParent.transform.GetChild(2).GetComponent<Image>() : null));
        });
    }

    // Update is called once per frame
    void Update() {

    }
    public void SetPointAmt(int type, int amt) {

    }
    public void UpdateAllMax() {
        if (facility != null) {
            InitMaxPointValues(0, facility.MaxPhysicalPoints);
            InitMaxPointValues(1, facility.MaxNetworkPoints);
            InitMaxPointValues(2, facility.MaxFinancialPoints);
        }
    }
    public void Init(Facility facility) {
        this.facility = facility;
        Debug.Log($"Init Facility Points UI {facility.MaxPhysicalPoints}, {facility.MaxNetworkPoints}, {facility.MaxFinancialPoints}");
        UpdateAllMax();
    }
    private void InitMaxPointValues(int type, int maxAmt) {
        if (type < 0 || type > 2) {
            Debug.LogError("Invalid point type");
            return;
        }
        List<FacilityPointUIElement> points = type == 0 ? physPointsUI : type == 1 ? netPointsUI : finPointsUI;

        for (int i = 0; i < points.Count; i++) {
            points[i].SetState(i < maxAmt ? FacilityPointState.Full : FacilityPointState.Disabled);
        }
    }

    
}
