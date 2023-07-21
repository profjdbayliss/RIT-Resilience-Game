using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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

    // Establish necessary fields

    // Static fields that are only utilized on spawn and cardloading.
    //public Type type;


    // Separate these -- As they will change more often, will need type
    public float percentSuccess;
    public float percentSpread;
    public float potentcy;
    public int cardID;
    public int teamID;
    public int duration;
    public int cost;
    public CardFront front;
    public CardState state;
    public GameObject cardDropZone;

    public CardReader reader;
    // Need to add Target

    // Start is called before the first frame update
    void Start()
    {
        //img = this.gameObject.GetComponent<RawImage>();
        //Debug.Log("Card Made");
        //reader = GameObject.Find("Card Reader").GetComponent<CardReader>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrop(PointerEventData eventData)
    {
        if(cardDropZone != null && this.state == CardState.CardDrawn)
        {
            Vector2 cardDropMin = new Vector2();
            cardDropMin.x = cardDropZone.GetComponent<RectTransform>().localPosition.x - (cardDropZone.GetComponent<RectTransform>().rect.width/2);
            cardDropMin.y = cardDropZone.GetComponent<RectTransform>().localPosition.y - (cardDropZone.GetComponent<RectTransform>().rect.height / 2);
            Vector2 cardDropMax = new Vector2();
            cardDropMax.x = cardDropZone.GetComponent<RectTransform>().localPosition.x + (cardDropZone.GetComponent<RectTransform>().rect.width / 2);
            cardDropMax.y = cardDropZone.GetComponent<RectTransform>().localPosition.y + (cardDropZone.GetComponent<RectTransform>().rect.height / 2);
            if(this.transform.localPosition.x > cardDropMin.x)
            {
                if(this.transform.localPosition.x < cardDropMax.x)
                {
                    if(this.transform.localPosition.y > cardDropMin.y)
                    {
                        if(this.transform.localPosition.y < cardDropMax.y)
                        {
                            int rng = Random.Range(0, 5);
                            this.gameObject.GetComponentInParent<Player>().PlayCard(this.cardID, rng);
                            this.state = CardState.CardInPlay;
                            this.gameObject.GetComponentInParent<slippy>().enabled = false;
                        }
                    }
                }
            }

        }
    }

    //public void OnDrag(PointerEventData pointer)
    //{
    //    if (map.gameObject.activeSelf) // Check to see if the gameobject this is attached to is active in the scene
    //    {
    //        // Create a vector2 to hold the previous position of the element and also set our target of what we want to actually drag.
    //        Vector2 tempVec2 = default(Vector2);
    //        RectTransform target = map.gameObject.GetComponent<RectTransform>();
    //        Vector2 tempPos = target.transform.localPosition;

    //        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position - pointer.delta, pointer.pressEventCamera, out tempVec2) == true) // Check the older position of the element and see if it was previously
    //        {
    //            Vector2 tempNewVec = default(Vector2); // Create a new Vec2 to track the current position of the object
    //            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position, pointer.pressEventCamera, out tempNewVec) == true)
    //            {
    //                tempPos.x += tempNewVec.x - tempVec2.x;
    //                tempPos.y += tempNewVec.y - tempVec2.y;
    //                map.transform.localPosition = tempPos;
    //            }
    //        }
    //    }
    //}
}
public struct CardFront
{
    public Card.Type type;
    public string title;
    public string description;
    public Texture2D img;
};