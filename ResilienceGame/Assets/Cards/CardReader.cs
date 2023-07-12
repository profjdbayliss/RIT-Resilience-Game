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
    public List<Card> globalModifiers;

    public string cardFileLoc;


    public List<Player> players;

    public MaliciousActor maliciousActor;

    public GameObject cardPrefab;


    // Start is called before the first frame update
    void Start()
    {
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

            // Split the read in CSV file into seperate objects at the new line character
            string[] allCSVObjects = allCardText.Split("\n");

            // Make sure to get the atlas first, as we only need to query it once.
            Texture2D tex = new Texture2D(1, 1);
            byte[] tempBytes = File.ReadAllBytes(GetComponent<CreateTextureAtlas>().mOutputFileName); // This gets the entire atlast right now.
            tex.LoadImage(tempBytes);

            for (int i = 0; i < allCSVObjects.Length; i++)
            {
                // Then in each of the lines of csv data, split them based on commas to get the different pieces of information on each object
                // and instantiate a base card object to then fill in with data.
                string[] individualCSVObjects = allCSVObjects[i].Split(",");
                GameObject tempCardObj = Instantiate(cardPrefab);

                // Get a reference to the Card component on the card gameobject.
                Card tempCard = tempCardObj.GetComponent<Card>();

                // Assign the cards type based on a switch statement of either Resilient, Malicious, or a Global Modifier
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

                // Then assign the necessary values to each card based off of their csv input.
                tempCardObj.name = individualCSVObjects[1];
                tempCard.title = individualCSVObjects[1];

                tempCard.description = individualCSVObjects[2];

                tempCard.percentSuccess = float.Parse(individualCSVObjects[3]); // Parse bc it is a percent

                // Check to make sure that this is actually a number, but if it has text in it then we don't parse it.
                if (individualCSVObjects[4].Contains("|") == false)
                {
                    tempCard.potentcy = float.Parse(individualCSVObjects[4]);
                }

                tempCard.duration = int.Parse(individualCSVObjects[5]);

                tempCard.cost = int.Parse(individualCSVObjects[6]);




                // Then we use a for loop to check the image location of the current CSV and the textureUVs
                // made when the atlas is made. If it is the matching image, then we take a sub-section of the atlas
                // and add it to the card.
                // ** VERY IMPORTANT ** The texture2D width and Height need to match what is in the TextureAtlas.cs file and all images for cards need to adhere to this size for this to work properly.
                for(int j = 0; j < TextureAtlas.textureUVs.Count; j++)
                {
                    TextureUV texUV = TextureAtlas.textureUVs[j];
                    if (texUV.location.Trim() == individualCSVObjects[7].Trim()) // Check to make sure that the TextureUV and the current CSV objects image are the same
                    {

                        Texture2D tex3 = new Texture2D(128, 128); // This needs to match the textureatlas pixel width

                        tempCardObj.GetComponent<RawImage>().texture = tex3;
                        Color[] tempColors = tex.GetPixels(texUV.column * 128, texUV.row * 128, 128, 128); // This needs to match the textureatlas pixel width
                        tex3.SetPixels(tempColors);
                        tex3.Apply();

                        break;
                    }
                }



                // Add the card to all card list and then based off a switch on the cards type we add it to a list of all resilient, malicious, or global modifier cards.
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
