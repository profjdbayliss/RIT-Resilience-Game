using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MapSector : MonoBehaviour {
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI sectorName;
    [SerializeField] private TextMeshProUGUI sectorOwner;
    [SerializeField] private List<TextMeshProUGUI> facilityNames;
    public Sector sector;
    [SerializeField] private Canvas sectorVisuals;
    [SerializeField] List<FacilityPointsUIController> pointsUIControllers;
    [SerializeField] List<BoxCollider2D> facilityColliders;

    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
    public void Init() {

        sectorName.text = sector.sectorName.ToString();
        sectorOwner.text = sector.Owner != null ? sector.Owner.playerName : "Unclaimed";
        var facilityProxies = GetComponentsInChildren<FacilityProxy>();
        

        for (int i = 0; i < sector.facilities.Length; i++) {
            facilityNames[i].text = sector.facilities[i].facilityName;
            pointsUIControllers[i].Init(sector.facilities[i]);
            facilityProxies[i].facility = sector.facilities[i];
            facilityProxies[i].AddListeners();
        }

    }
    public void ToggleVisuals(bool enable) {
        sectorVisuals.enabled = enable;
        foreach (BoxCollider2D collider in facilityColliders) {
            collider.enabled = enable;
        }
    }
    public void UpdateFacilityPoints(FacilityProxy proxy) {
        int index = facilityColliders.FindIndex(collider => collider.GetComponent<FacilityProxy>() == proxy);
        if (index != -1) {
            pointsUIControllers[index].UpdateAllPoints();
        }
    }
}
