using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
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

    public CardFront[] CardFronts;
    public NativeArray<int> CardIDs;
    public NativeArray<int> CardTeam;
    //public NativeArray<CardFront> CardFronts;
    // public NativeArray<CardFront> cardFronts;
    // public NativeArray<char[]> cardTitle; // Need to figure this out
    public NativeArray<int> CardCost;
    // public NativeArray<Texture2D> Image;
    // public NativeArray<char[]> cardDescription; // Need to figure this out
    public NativeArray<float> CardImpact;
    public NativeArray<float> CardSpreadChance;
    public NativeArray<float> CardPercentChance;
    public NativeArray<int> CardDuration;
    public NativeArray<int> CardTarget;
    public NativeArray<int> CardCount;

    public string cardFileLoc;


    public List<Player> players;

    public MaliciousActor maliciousActor;

    public GameObject cardPrefab;


    // Start is called before the first frame update
    void Start()
    {
        //CSVRead();
    }

    // Update is called once per frame
    void Update()
    {

    }


    // Reformat to an SOA style 
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

            CardFronts = new CardFront[allCSVObjects.Length];

            // Allocate the space in memory for the Cards data
            CardIDs = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardTeam = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            //CardFronts = new NativeArray<CardFront>(allCSVObjects.Length, Allocator.Persistent); //Need to figure out how to handle these without throwing erros due to unmanaged type
            CardCost = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardImpact = new NativeArray<float>(allCSVObjects.Length, Allocator.Persistent);
            CardSpreadChance = new NativeArray<float>(allCSVObjects.Length, Allocator.Persistent);
            CardPercentChance = new NativeArray<float>(allCSVObjects.Length, Allocator.Persistent);
            CardDuration = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardTarget = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);
            CardCount = new NativeArray<int>(allCSVObjects.Length, Allocator.Persistent);


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
                CardIDs[i] = i;

                // Get a reference to the Card component on the card gameobject.
                Card tempCard = tempCardObj.GetComponent<Card>();
                CardFront tempCardFront = new CardFront();

                // Assign the cards type based on a switch statement of either Resilient, Malicious, or a Global Modifier
                switch (individualCSVObjects[0].Trim())
                {
                    case "Resilient":
                        //tempCard.type = Card.Type.Resilient;
                        //Debug.Log("Res: " + i);
                        CardTeam[i] = 0;
                        tempCardFront.type = Card.Type.Resilient;
                        break;

                    case "Blue":
                        //tempCard.type = Card.Type.Resilient;
                        //Debug.Log("Blue: " + i);
                        CardTeam[i] = 0;
                        tempCardFront.type = Card.Type.Resilient;
                        break;

                    case "Malicious":

                        //tempCard.type = Card.Type.Malicious;
                        //Debug.Log("Mal: " + i);
                        CardTeam[i] = 1;
                        tempCardFront.type = Card.Type.Malicious;
                        break;

                    case "Red":
                        //tempCard.type = Card.Type.Malicious;
                        //Debug.Log("Red: " + i);
                        CardTeam[i] = 1;
                        tempCardFront.type = Card.Type.Malicious;
                        break;

                    case "Global":
                        //tempCard.type = Card.Type.GlobalModifier;
                        //Debug.Log("Global: " + i);
                        CardTeam[i] = 2;
                        tempCardFront.type = Card.Type.GlobalModifier;
                        break;

                }

                // Then assign the necessary values to each card based off of their csv input.
                tempCardObj.name = individualCSVObjects[1];

                //tempCardFront.title = individualCSVObjects[1];
                //tempCardFront.description = individualCSVObjects[2];

                tempCardFront.title = individualCSVObjects[1];
                tempCardFront.description = individualCSVObjects[4];

                //tempCard.title = individualCSVObjects[1]; //NEED THESE JUST COMMENTING TO DROP ERRORS RN

                //tempCard.description = individualCSVObjects[2]; // NEED THESE JUST COMMENTING TO DROP ERRORS RN


                

                if (individualCSVObjects[6].Contains("|") == false)
                {
                    if (individualCSVObjects[6].Contains(" ") == false)
                    {
                        if (individualCSVObjects[6].Contains("e") == false)
                        {
                            if (individualCSVObjects[6].Length > 1)
                            {
                                tempCard.percentSuccess = float.Parse(individualCSVObjects[6]); // Parse bc it is a percent
                                CardPercentChance[i] = float.Parse(individualCSVObjects[6]);
                            }

                        }
                    }

                }


                // Check to make sure that this is actually a number, but if it has text in it then we don't parse it.
                if (individualCSVObjects[5].Contains("|") == false)
                {
                    if(individualCSVObjects[5].Contains(" ") == false)
                    {
                        if (individualCSVObjects[5].Contains("e") == false)
                        {
                            if(individualCSVObjects[5].Length > 1)
                            {
                                tempCard.potentcy = float.Parse(individualCSVObjects[5]);
                                CardImpact[i] = float.Parse(individualCSVObjects[5]);
                            }

                        }
                    }

                }

                tempCard.duration = int.Parse(individualCSVObjects[8]);
                CardDuration[i] = int.Parse(individualCSVObjects[8]);

                if(individualCSVObjects[2].Length > 0)
                {
                    tempCard.cost = int.Parse(individualCSVObjects[2]);
                    CardCost[i] = int.Parse(individualCSVObjects[2]);
                }


                CardCount[i] = int.Parse(individualCSVObjects[10]);
                Debug.Log("Card Count for " + i + ": " + CardCount[i]);




                // Then we use a for loop to check the image location of the current CSV and the textureUVs
                // made when the atlas is made. If it is the matching image, then we take a sub-section of the atlas
                // and add it to the card.
                // ** VERY IMPORTANT ** The texture2D width and Height need to match what is in the TextureAtlas.cs file and all images for cards need to adhere to this size for this to work properly.
                for (int j = 0; j < TextureAtlas.textureUVs.Count; j++)
                {
                    TextureUV texUV = TextureAtlas.textureUVs[j];
                    if (texUV.location.Trim() == individualCSVObjects[3].Trim()) // Check to make sure that the TextureUV and the current CSV objects image are the same
                    {

                        Texture2D tex3 = new Texture2D(128, 128); // This needs to match the textureatlas pixel width


                        //tempCardObj.GetComponentInChildren<RawImage>().texture = tex3;
                        //tempCard.img.texture = tex3;
                        tempCardFront.img = tex3;

                        Color[] tempColors = tex.GetPixels(texUV.column * 128, texUV.row * 128, 128, 128); // This needs to match the textureatlas pixel width
                        tex3.SetPixels(tempColors);
                        tex3.Apply();
                        break;
                    }
                }
                Debug.Log("CARD FRONT: " + tempCardFront);
                CardFronts[i] = tempCardFront;
                tempCardObj.SetActive(false);

                // Add the card to all card list and then based off a switch on the cards type we add it to a list of all resilient, malicious, or global modifier cards.
                allCards.Add(tempCard);
                //switch (tempCard.type)
                //{
                //    case Card.Type.Resilient:
                //        resilientCards.Add(tempCard);
                //        break;

                //    case Card.Type.Malicious:
                //        maliciousCards.Add(tempCard);
                //        break;

                //    case Card.Type.GlobalModifier:
                //        globalModifiers.Add(tempCard);
                //        break;
                //}
            }
            // Close at the end
            reader.Close();
            stream.Close();
        
        }

    }

    public void OnDestroy()
    {
        // Must dispose of the allocated memory
        CardIDs.Dispose();
        CardTeam.Dispose();
        //CardFronts.Dispose();
        CardCost.Dispose();
        CardImpact.Dispose();
        CardSpreadChance.Dispose();
        CardPercentChance.Dispose();
        CardDuration.Dispose();
        CardTarget.Dispose();
        CardCount.Dispose();
    }
}
