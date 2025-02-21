using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacilityProxy : MonoBehaviour
{
    public Facility facility;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddListeners() {
        facility.OnPointsChanged += UpdatePoints;
    }
    public void UpdatePoints(MapSector sector = null) {
        if (sector == null) 
            sector = GetComponentInParent<MapSector>();
        if (sector != null ) {
            sector.UpdateFacilityPoints(this);
        }
    }
    public void UpdatePoints() {
        UpdatePoints(null);
    }
}
