using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class Sector : MonoBehaviour
{
    public PlayerSector sectorName; // TODO: Move playersector here
    public Facility[] facilities;
    public bool isCore;
    public float blueMeeples;
    public float blackMeeples;
    public float purpleMeeples;

    // filename - directory path is assumed to be Application.streamingAssetsPath
    // extension is assumed to be csv
    [SerializeField] private string csvFileName;
    // filename + directory path
    private string fileLocation;
    // output atlas filename
    public string outputAtlasName;

    [SerializeField] private GameObject sectorCanvas;
    public RawImage icon;

    public void Initialize(PlayerSector sector)
    {
        sectorCanvas = this.gameObject;
        // TODO: Remove when assigning sectors randomly implemented
        sectorName = sector;
        blackMeeples = blueMeeples = purpleMeeples = 2;
        facilities = new Facility[3];

        for (int i = 0; i < sectorCanvas.transform.childCount; i++) // TODO: If children added change count to 3
        {
            facilities[i] = sectorCanvas.transform.GetChild(i).GetComponent<Facility>();
            facilities[i].Initialize();
            facilities[i].facilityCanvas = sectorCanvas.transform.GetChild(i).gameObject;
            facilities[i].sectorItsAPartOf = this;
        }

        CSVRead();

        Texture2D tex = new Texture2D(1, 1);
        byte[] tempBytes = File.ReadAllBytes(Application.streamingAssetsPath + "/Images/" + sector.ToString() + ".png");
        tex.LoadImage(tempBytes);
        icon.texture = tex;
        //Debug.Log(Application.streamingAssetsPath + "/images/" + sector.ToString() + ".png");
    }

    public Facility[] CheckDownedFacilities()
    {
        Facility[] facilitiesList = new Facility[3];
        int downedFacilities = 0;
        // TODO: check isDown;
        //I think this should work? - Mukund
        for(int i = 0; i < facilities.Length; i++)
        {
            if(facilities[i].isDown)
            {
                facilitiesList[downedFacilities] = facilities[i];
                downedFacilities++;
            }
        }

        return facilitiesList;
    }
    
    public bool CanAffordCardPlay(Card card) {
        return card.data.blueCost <= blueMeeples &&
            card.data.blackCost <= blackMeeples &&
            card.data.purpleCost <= purpleMeeples;
    }
    public bool SpendMeeples(Card card) {
        if (CanAffordCardPlay(card)) {
            blueMeeples -= card.data.blueCost;
            blackMeeples -= card.data.blackCost;
            purpleMeeples -= card.data.purpleCost;
            return true;
        }
        return false;
    }

    private void CSVRead()
    {
        fileLocation = Application.streamingAssetsPath + "/" + csvFileName;

        if (File.Exists(fileLocation))
        {
            FileStream stream = File.OpenRead(fileLocation);
            TextReader reader = new StreamReader(stream);
            string allCSVText = reader.ReadToEnd();

            // Split the read in CSV file into seperate objects at the new line character
            string[] allCSVObjects = allCSVText.Split("\n");
            //Debug.Log("Number of lines in csv file is: " + allCSVObjects.Length);

            // get all the textual elements in the csv file
            // NOTE: row 0 is always headings and not data
            for (int i = 1; i < allCSVObjects.Length; i++)
            {
                string[] individualCSVObjects = allCSVObjects[i].Split(",");
                if (individualCSVObjects.Length > 1)
                {
                    //  0: Sector
                    //  1: Facility Name
                    //  2: Facility Type
                    //  3: Dependency 1		
                    //  4: Dependency 2
                    //  5: Dependency 3
                    //  6: Number of Dependant Sectors
                    //  7: Number of Sector Dependencies
                    //  8: Core Facility T/F
                    //  9: Sector Appeal
                    //  10: Physical Health		
                    //  11: Financial Health
                    //  12: Network Health
                    //  13: Facility ID // TODO: Use this if possible otherwise remove/replace

                    //  0: Sector	
                    if (individualCSVObjects[0].Trim().ToLower() != sectorName.ToString().ToLower())
                    {
                        continue;
                    }

                    //  1: Facility Type
                    switch (individualCSVObjects[2].Trim())
                    {
                        case "Production":
                            facilities[0].facilityType = Facility.FacilityType.Production;
                            facilities[0].facilityName = individualCSVObjects[1];

                            //  3-5: Dependencies
                            for (int j = 3; j < 6; j++)
                            {
                                if (Enum.TryParse(individualCSVObjects[j], out PlayerSector enumName)) { facilities[0].products[(j-3)] = enumName; }
                                else { Debug.Log("Dependency not parsed"); }
                            }

                            // 10-12: Health
                            facilities[0].SetFacilityPoints(int.Parse(individualCSVObjects[10]), int.Parse(individualCSVObjects[11]), int.Parse(individualCSVObjects[12]));
                            break;

                        case "Transmission":
                            facilities[1].facilityType = Facility.FacilityType.Transmission;
                            facilities[1].facilityName = individualCSVObjects[1];

                            for (int j = 3; j < 6; j++)
                            {
                                if (Enum.TryParse(individualCSVObjects[j], out PlayerSector enumName)) { facilities[1].products[(j - 3)] = enumName; }
                                else { Debug.Log("Dependency not parsed"); }
                            }

                            facilities[1].SetFacilityPoints(int.Parse(individualCSVObjects[10]), int.Parse(individualCSVObjects[11]), int.Parse(individualCSVObjects[12]));
                            break;

                        case "Distribution":
                            facilities[2].facilityType = Facility.FacilityType.Distribution;
                            facilities[2].facilityName = individualCSVObjects[1];

                            for (int j = 3; j < 6; j++)
                            {
                                if (Enum.TryParse(individualCSVObjects[j], out PlayerSector enumName)) { facilities[2].products[(j - 3)] = enumName; }
                                else { Debug.Log("Dependency not parsed"); }
                            }

                            facilities[2].SetFacilityPoints(int.Parse(individualCSVObjects[10]), int.Parse(individualCSVObjects[11]), int.Parse(individualCSVObjects[12]));
                            break;
                    }

                    // 7: Core Sector?
                    if(individualCSVObjects[8] != "")
                    {
                        Debug.Log("Is it a core facility?"+ individualCSVObjects[8]);
                        isCore = bool.Parse(individualCSVObjects[8].Trim()); 
                    }
                }
            }

            // Close at the end
            reader.Close();
            stream.Close();
        }
        else { Debug.Log("Sector file not found"); }
    }
}
