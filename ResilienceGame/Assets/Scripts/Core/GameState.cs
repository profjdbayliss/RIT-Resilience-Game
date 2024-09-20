using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameState : NetworkBehaviour {
    public static GameState Instance { get; private set; }

    [SyncVar(hook = nameof(OnCurrentFacilityIDChanged))]
    private int currentFacilityID = 0;

    [SyncVar(hook = nameof(OnCurrentSectorIDChanged))]
    private int currentSectorID = 0;

    public readonly SyncDictionary<int, Facility> AllFacilities = new SyncDictionary<int, Facility>();

    public readonly SyncDictionary<int, Sector> AllSectors = new SyncDictionary<int, Sector>();

    public override void OnStartServer() {
        base.OnStartServer();
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Server created game state");
        }
        else {
            Destroy(gameObject);
        }
    }

    public override void OnStartClient() {
        base.OnStartClient();
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    [Server]
    public int AddFacility(Facility facility) {

        AllFacilities.Add(currentFacilityID, facility);
        int id = currentFacilityID;
        currentFacilityID++;
        return id;


    }

    [Server]
    public int AddSector(Sector sector) {
        AllSectors.Add(currentSectorID, sector);
        int id = currentSectorID;
        currentSectorID++;
        return id;
    }

    public Facility GetFacility(int id) {
        if (AllFacilities.TryGetValue(id, out Facility facility)) {
            return facility;
        }
        return null;
    }

    public Sector GetSector(int id) {
        if (AllSectors.TryGetValue(id, out Sector sector)) {
            return sector;
        }
        return null;
    }

    private void OnCurrentFacilityIDChanged(int oldValue, int newValue) {
        currentFacilityID = newValue;
    }

    private void OnCurrentSectorIDChanged(int oldValue, int newValue) {
        currentSectorID = newValue;
    }
}