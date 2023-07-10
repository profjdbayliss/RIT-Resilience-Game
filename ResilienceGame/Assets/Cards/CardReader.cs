using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class CardReader : MonoBehaviour
{
    // Establish necessary fields
    
    public List<Card> allCards;
    public List<Card> resilientCards;
    public List<Card> maliciousCards;

    public string cardFileLoc;

    public List<Card> globalModifiers;

    public List<Player> players;

    public MaliciousActor actor;

    public GameObject cardPrefab;

    public string[] icons;



    // Start is called before the first frame update
    void Start()
    {
        icons = Directory.GetFiles("Assets/Icons/EmilyIcons/Game Icons");
        CSVRead();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CSVRead()
    {
        // Check to see if the file exists
        if (File.Exists(cardFileLoc))
        {
            FileStream stream = File.OpenRead(cardFileLoc);
            TextReader reader = new StreamReader(stream);
            string allCardText = reader.ReadToEnd();
            string[] allCSVObjects = allCardText.Split("\n");
            Debug.Log(allCSVObjects.Length);
            for(int i = 0; i < allCSVObjects.Length; i++)
            {
                Debug.Log(allCSVObjects[i]);
                string[] individualCSVObjects = allCSVObjects[i].Split(",");
                GameObject tempCardObj = Instantiate(cardPrefab);
                //Card tempCard = new Card();
                Card tempCard = tempCardObj.GetComponent<Card>();

                switch (individualCSVObjects[0])
                {
                    case "Resilient":
                        tempCard.type = Card.Type.Resilient;
                        Debug.Log("RESGIVEN");
                        break;

                    case "Malicious":
                        tempCard.type = Card.Type.Malicious;
                        Debug.Log("MALGIVEN");
                        break;

                    case "GlobalModifier":
                        tempCard.type = Card.Type.GlobalModifier;
                        break;

                }

                tempCardObj.name = individualCSVObjects[1];
                tempCard.title = individualCSVObjects[1];

                tempCard.description = individualCSVObjects[2];

                tempCard.percentSuccess = float.Parse(individualCSVObjects[3]);

                tempCard.potentcy = float.Parse(individualCSVObjects[4]);

                tempCard.duration = int.Parse(individualCSVObjects[5]);

                tempCard.cost = int.Parse(individualCSVObjects[6]);

                Debug.Log(individualCSVObjects[7]);
                for(int j = 0; j < icons.Length; j++)
                {
                    if (icons[j].Contains(individualCSVObjects[7]))
                    {
                        Texture2D tex = new Texture2D(1, 1);
                        Debug.Log("FOUND");
                        byte[] tempBytes = File.ReadAllBytes(individualCSVObjects[7]);
                        //Debug.Log(Directory.GetFiles(individualCSVObjects[7]));
                        //byte[] tempBytes2 = File.ReadAllBytes(Directory.GetFiles(individualCSVObjects[7])[0]);
                        //Directory.GetFiles(individualCSVObjects[7]);
                        tex.LoadImage(tempBytes);
                        //tex.LoadImage(tempBytes2);
                        tempCardObj.GetComponent<RawImage>().texture = tex;
                    }
                }
                
                //if (File.Exists(individualCSVObjects[7]))
                //{
                //    Texture2D tex = new Texture2D(1, 1);
                //    Debug.Log("FOUND");
                //    byte[] tempBytes = File.ReadAllBytes(individualCSVObjects[7]);
                //    //Debug.Log(Directory.GetFiles(individualCSVObjects[7]));
                //    //byte[] tempBytes2 = File.ReadAllBytes(Directory.GetFiles(individualCSVObjects[7])[0]);
                //    //Directory.GetFiles(individualCSVObjects[7]);
                //    tex.LoadImage(tempBytes);
                //    //tex.LoadImage(tempBytes2);
                //    tempCardObj.GetComponent<RawImage>().texture = tex;
                //}



                allCards.Add(tempCard);
                switch (tempCard.type)
                {
                    case Card.Type.Resilient:
                        resilientCards.Add(tempCard);
                        break;

                    case Card.Type.Malicious:
                        maliciousCards.Add(tempCard);
                        break;

                    case Card.Type.GlobalModifier:
                        globalModifiers.Add(tempCard);
                        break;
                }
            }
            // Close at the end
            reader.Close();
            stream.Close();
        
        }

    }
}
