using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Unity.Collections;

public class Card : MonoBehaviour, IDropHandler
{

    // Enum to track card type in reference for which team it goes to
    public enum Type
    {
        Resilient,
        Malicious,
        GlobalModifier
    };

    // Enum to track the state of the card
    public enum CardState
    {
        NotInDeck,
        CardInDeck,
        CardDrawn,
        CardInPlay,
        CardDiscarded
    };

    // Enum to the state of the facility required by the card
    public enum FacilityStateRequirements
    {
        Normal = 0,
        Informed = 1,
        Accessed = 2,
    };

    // Enum to the specific type of card a malicious card is
    public enum MalCardType
    {
        Reconnaissance,
        InitialAccess,
        Collection,
        Impact,
        Exfiltration,
        LateralMovement
    }

    // Enum to the specific type of card a resilient card is
    public enum ResCardType
    {
        Detection,
        Prevention,
        Mitigation
    }

    // Establish necessary fields

    // Static fields that are only utilized on spawn and cardloading.
    //public Type type;

    // Properties needed by the new design
    public string cardTitle;
    public string cardDescription;
    public List<Meeple> cardCost = new List<Meeple>();
    public List<string> targetFacilityTypes = new List<string>();
    public int targetCount;
    public int duration;
    public List<Effect> prerequisiteEffects = new List<Effect>();
    public List<CardAction> actions = new List<CardAction>();

    // Separate these -- As they will change more often, will need type
    public float percentSuccess;
    public float percentSpread;
    public float potentcy;
    public int cardID;
    public int teamID;
    public int cost;
    //public Hashtable blueTargetMits;
    public List<int> blueTargetMits;
    public int[] blueCardTargets;
    public CardFront front;
    public CardState state;
    public FacilityStateRequirements facilityStateRequirements;
    public ResCardType resCardType;
    public MalCardType malCardType;
    public GameObject cardDropZone;
    public GameObject originalParent;
    public GameObject handDropZone;
    public GameObject gameCanvas;

    public CardReader reader;
    // Need to add Target

    // Start is called before the first frame update
    void Start()
    {
        //img = this.gameObject.GetComponent<RawImage>();
        //Debug.Log("Card Made");
        //reader = GameObject.Find("Card Reader").GetComponent<CardReader>();
        handDropZone = this.gameObject.transform.parent.gameObject;
        originalParent = this.gameObject.transform.parent.transform.parent.gameObject;
        this.gameObject.transform.localScale = Vector3.one;
        gameCanvas = GameObject.Find("Central Map");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool PlayCard(CardPlayer player)
    {
        if (CanAfford(player))
        {
            foreach (var cost in cardCost)
            {
                player.meeples.Remove(cost);
            }

            foreach (var action in actions)
            {
                action.ExecuteAction(player);
            }

            return true;
        }
        else
        {
            Debug.Log("Not enough resources to play this card.");
            return false;
        }
    }

    private bool CanAfford(CardPlayer player)
    {
        // Checks if the player has enough meeples for each cost item
        foreach (var cost in cardCost)
        {
            if (!player.meeples.Contains(cost))
            {
                return false;
            }
        }
        return true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("trying to drop card");
        if (cardDropZone != null && this.state == CardState.CardDrawn) // Make sure that the card actually has a reference to the card drop location where it will be dropped and that it is currently in the players hand
        {

            // Get the bounds of the card Drop Zone
            Vector2 cardDropMin = new Vector2();
            cardDropMin.x = cardDropZone.GetComponent<RectTransform>().localPosition.x - (cardDropZone.GetComponent<RectTransform>().rect.width / 2);
            cardDropMin.y = cardDropZone.GetComponent<RectTransform>().localPosition.y - (cardDropZone.GetComponent<RectTransform>().rect.height / 2);
            Vector2 cardDropMax = new Vector2();

            cardDropMax.x = cardDropZone.GetComponent<RectTransform>().localPosition.x + (cardDropZone.GetComponent<RectTransform>().rect.width / 2);
            cardDropMax.y = cardDropZone.GetComponent<RectTransform>().localPosition.y + (cardDropZone.GetComponent<RectTransform>().rect.height / 2);
            this.gameObject.transform.SetParent(originalParent.transform, true); // Now set the card's parent to be the player they are attached to instead of the hand zone

            // DO a AABB collision test to see if the card is on the card drop

            if (this.transform.localPosition.y < cardDropMax.y &&
               this.transform.localPosition.y > cardDropMin.y &&
               this.transform.localPosition.x < cardDropMax.x &&
               this.transform.localPosition.x > cardDropMin.x)
            {
                Debug.Log("card dropped in card drop zone");
                // check the cards teamID to see which team they belong to so they can call the proper Select facility method to then see if they have met all conditions to play the card
                if (this.teamID == 0)
                {
                    if (this.gameObject.GetComponentInParent<Player>().SelectFacility(this.cardID))
                    {
                        if (FindObjectOfType<GameManager>()) // Reduce funds of the local player when play a card
                        {
                            GameManager gm = FindObjectOfType<GameManager>();
                            gm.AddFunds(-100);
                            foreach (var facility in this.gameObject.GetComponentInParent<Player>().facilitiesActedUpon)
                            {
                                facility.GetComponent<FacilityV3>().health += 20;
                                if (facility.GetComponent<FacilityV3>().health > 100)
                                {
                                    facility.GetComponent<FacilityV3>().health = 100;
                                }
                                facility.GetComponent<FacilityV3>().Health.text = facility.GetComponent<FacilityV3>().health.ToString();
                            }
                            List<FacilityV3Info> tempFacs = new List<FacilityV3Info>();
                            Debug.Log("Facilities Count: " + gm.allFacilities.Count + ", " + teamID);
                            for (int i = 0; i < gm.allFacilities.Count; i++)
                            {
                                tempFacs.Add(gm.allFacilities[i].GetComponent<FacilityV3>().ToFacilityV3Info());
                            }
                            //RGNetworkPlayerList.instance.AskUpdateFacilities(tempFacs); //Update facilities' info
                        }
                        this.state = CardState.CardInPlay;
                        this.gameObject.GetComponentInParent<slippy>().enabled = false;
                        // Set the time the card is to be disposed of by adding the duration of the card to the current turn count
                    }
                    else
                    {
                        this.gameObject.transform.SetParent(handDropZone.transform, false);
                       
                    }
                }
                else if (this.teamID == 1)
                {
                    if (this.gameObject.GetComponentInParent<MaliciousActor>().SelectFacility(this.cardID))
                    {
                        if (FindObjectOfType<GameManager>()) // Reduce funds of the local player when play a card
                        {
                            GameManager gm = FindObjectOfType<GameManager>();
                            gm.AddFunds(-100);
                            foreach (var facility in this.gameObject.GetComponentInParent<MaliciousActor>().facilitiesActedUpon)
                            {
                                facility.GetComponent<FacilityV3>().health -= 30;
                                if (facility.GetComponent<FacilityV3>().health <= 0)
                                {
                                    gm.EndGame(1, true);
                                    //RGNetworkPlayerList.instance.CmdEndGame(1);
                                }
                                facility.GetComponent<FacilityV3>().Health.text = facility.GetComponent<FacilityV3>().health.ToString();
                            }

                            List<FacilityV3Info> tempFacs = new List<FacilityV3Info>();
                            Debug.Log("Facilities Count: " + gm.allFacilities.Count + ", " + teamID);
                            for (int i = 0; i < gm.allFacilities.Count; i++)
                            {
                                tempFacs.Add(gm.allFacilities[i].GetComponent<FacilityV3>().ToFacilityV3Info());
                            }
                            //RGNetworkPlayerList.instance.AskUpdateFacilities(tempFacs); //Update facilities' info
                        }
                        this.state = CardState.CardInPlay;
                        this.gameObject.GetComponentInParent<slippy>().enabled = false;
                        this.gameObject.GetComponentInParent<slippy>().ResetScale();
                    }
                    else
                    {
                        this.gameObject.transform.SetParent(handDropZone.transform, false);
                    }
                }
            }
            else
            {
                Debug.Log("card not dropped in card drop zone");
                // If it fails, parent it back to the hand location and then set its state to be in hand and make it grabbable again
                this.gameObject.transform.SetParent(handDropZone.transform, false);
                this.state = CardState.CardDrawn;
                this.gameObject.GetComponentInParent<slippy>().enabled = true;
                this.gameObject.GetComponent<slippy>().ResetPosition();
            }
        }
    }
}
public struct CardFront2
{
    public Card.Type type;
    //public NativeArray<byte> title;
    public byte[] title;
    public byte[] description;
    //public NativeArray<byte> description;
    public Texture2D img;


    public void OnDestroy()
    {
        // Must dispose of the allocated memory
        //title.Dispose();
        //description.Dispose();
    }
};