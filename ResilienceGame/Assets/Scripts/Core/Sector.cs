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

        facilities = new Facility[3];

        for (int i = 0; i < sectorCanvas.transform.childCount; i++) // TODO: If children added change count to 3
        {
            facilities[i] = sectorCanvas.transform.GetChild(i).GetComponent<Facility>();
            facilities[i].Initialize();
            facilities[i].facilityCanvas = sectorCanvas.transform.GetChild(i).gameObject;
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
                    //  1: Facility Type
                    //  2: Dependency 1		
                    //  3: Dependency 2
                    //  4: Dependency 3
                    //  5: Number of Dependant Sectors
                    //  6: Number of Sector Dependencies
                    //  7: Core Facility T/F
                    //  8: Sector Appeal
                    //  9: Physical Health		
                    // 10: Financial Health
                    // 11: Network Health
                    // 12: Facility ID // TODO: Use this if possible otherwise remove/replace

                    //  0: Sector	
                    if (individualCSVObjects[0].Trim().ToLower() != sectorName.ToString().ToLower())
                    {
                        continue;
                    }

                    //  1: Facility Type
                    switch (individualCSVObjects[1].Trim())
                    {
                        case "Production":
                            facilities[0].facilityName = Facility.FacilityName.Production;

                            //  2-4: Dependencies
                            for (int j = 2; j < 5; j++)
                            {
                                if (Enum.TryParse(individualCSVObjects[j], out PlayerSector enumName)) { facilities[0].products[(j-2)] = enumName; }
                                else { Debug.Log("Dependency not parsed"); }
                            }

                            // 9-11: Health
                            facilities[0].SetFacilityPoints(int.Parse(individualCSVObjects[9]), int.Parse(individualCSVObjects[10]), int.Parse(individualCSVObjects[11]));
                            break;

                        case "Transmission":
                            facilities[1].facilityName = Facility.FacilityName.Transmission;

                            for (int j = 2; j < 5; j++)
                            {
                                if (Enum.TryParse(individualCSVObjects[j], out PlayerSector enumName)) { facilities[1].products[(j - 2)] = enumName; }
                                else { Debug.Log("Dependency not parsed"); }
                            }

                            facilities[1].SetFacilityPoints(int.Parse(individualCSVObjects[9]), int.Parse(individualCSVObjects[10]), int.Parse(individualCSVObjects[11]));
                            break;

                        case "Distribution":
                            facilities[2].facilityName = Facility.FacilityName.Distribution;

                            for (int j = 2; j < 5; j++)
                            {
                                if (Enum.TryParse(individualCSVObjects[j], out PlayerSector enumName)) { facilities[2].products[(j - 2)] = enumName; }
                                else { Debug.Log("Dependency not parsed"); }
                            }

                            facilities[2].SetFacilityPoints(int.Parse(individualCSVObjects[9]), int.Parse(individualCSVObjects[10]), int.Parse(individualCSVObjects[11]));
                            break;
                    }

                    // 7: Core Sector?
                    if(individualCSVObjects[7] != "") { isCore = bool.Parse(individualCSVObjects[7].Trim()); }
                }
            }

            // Close at the end
            reader.Close();
            stream.Close();
        }
        else { Debug.Log("Sector file not found"); }
    }
}
