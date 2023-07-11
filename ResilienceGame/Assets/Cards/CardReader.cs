using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Threading.Tasks;

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

    bool called;

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
                string[] individualCSVObjects = allCSVObjects[i].Split(",");
                GameObject tempCardObj = Instantiate(cardPrefab);
                //Card tempCard = new Card();
                Card tempCard = tempCardObj.GetComponent<Card>();

                switch (individualCSVObjects[0])
                {
                    case "Resilient":
                        tempCard.type = Card.Type.Resilient;
                        break;

                    case "Blue":
                        tempCard.type = Card.Type.Resilient;
                        break;

                    case "Malicious":
                        tempCard.type = Card.Type.Malicious;
                        break;

                    case "Red":
                        tempCard.type = Card.Type.Malicious;
                        break;

                    case "GlobalModifier":
                        tempCard.type = Card.Type.GlobalModifier;
                        break;

                }

                tempCardObj.name = individualCSVObjects[1];
                tempCard.title = individualCSVObjects[1];

                tempCard.description = individualCSVObjects[2];

                tempCard.percentSuccess = float.Parse(individualCSVObjects[3]);

                if (individualCSVObjects[4].Contains("|") == false)
                {
                    tempCard.potentcy = float.Parse(individualCSVObjects[4]);
                }

                tempCard.duration = int.Parse(individualCSVObjects[5]);

                tempCard.cost = int.Parse(individualCSVObjects[6]);

                //Debug.Log(individualCSVObjects[7]);

                Texture2D tex = new Texture2D(1, 1);

                byte[] tempBytes = File.ReadAllBytes(GetComponent<CreateTextureAtlas>().mOutputFileName); // This gets the entire atlast right now.

                tex.LoadImage(tempBytes);
                for(int j = 0; j < TextureAtlas.textureUVs.Count; j++)
                {
                    TextureUV texUV = TextureAtlas.textureUVs[j];
                    if (texUV.location.Trim() == individualCSVObjects[7].Trim())
                    {
                        Debug.Log("SUCCESSFUL TUV: " + texUV.location + " SUCC CSV: " + individualCSVObjects[7]);

                        //Texture2D tex3 = new Texture2D((int)(texUV.pixelEndX - texUV.pixelStartX), (int)(texUV.pixelEndY - texUV.pixelStartY));
                        Texture2D tex3 = new Texture2D(128, 128);
                        Debug.Log("X: " + (texUV.pixelEndX) + " Y : " + (texUV.pixelEndY));
                        tempCardObj.GetComponent<RawImage>().texture = tex3;
                        Color[] tempColors = tex.GetPixels(texUV.column * 128, texUV.row * 128, 128, 128);
                        tex3.SetPixels(tempColors);
                        tex3.Apply();
                        break;
                    }
                }

                //Texture2D tex2 = TextureAtlas.textureUVs[i];
                //tempCardObj.GetComponent<RawImage>().texture = tex;



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
