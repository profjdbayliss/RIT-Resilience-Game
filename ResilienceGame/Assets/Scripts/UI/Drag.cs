using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IDragHandler
{
    // Establish necessary fields
    public Vector3 minPos;
    public Vector3 maxPos;
    public bool isDragging = false;
    public GameObject dragObject;
    public GameObject Map;
    public Vector2 mapScalar;


    // Start is called before the first frame update
    void Start()
    {
        Map = GameObject.Find("Map");

        // Scale the Feedback menu

        mapScalar.x = Map.GetComponent<RectTransform>().rect.width;
        mapScalar.y = Map.GetComponent<RectTransform>().rect.height;
        Vector3 tempPos = dragObject.transform.position;
        tempPos = dragObject.transform.localPosition;
        tempPos.x = mapScalar.x * -0.75f;
        dragObject.transform.localPosition = tempPos;

        // Establish the minimum points and the maximum points it can drag out to, and move the min and max just slightly outside what we want to avoid bouncing.
        minPos = dragObject.transform.position;
        minPos.x = dragObject.transform.position.x - 0.001f;

        maxPos = dragObject.transform.position;
        maxPos.x = dragObject.transform.position.x + (dragObject.GetComponent<RectTransform>().rect.width * 1.25f);
    }



    public void OnDrag(PointerEventData dragEventData)
    {
        if (this.gameObject.activeSelf) // Check to see if the gameobject this is attached to is active in the scene
        {
            // Create a vector2 to hold the previous position of the element and also set our target of what we want to actually drag.
            Vector2 tempVec2 = default(Vector2);
            RectTransform target = dragObject.gameObject.GetComponent<RectTransform>();
            Vector2 tempPos = target.transform.position;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, dragEventData.position - dragEventData.delta, dragEventData.pressEventCamera, out tempVec2) == true) // Check the older position of the element and see if it was previously
            {
                Vector2 tempNewVec = default(Vector2); // Create a new Vec2 to track the current position of the object
                if(RectTransformUtility.ScreenPointToLocalPointInRectangle(target, dragEventData.position, dragEventData.pressEventCamera, out tempNewVec) == true) 
                {
                    if (dragObject.transform.position.x < maxPos.x && dragObject.transform.position.x > minPos.x) // Make sure the object is actually within the bounds we want.
                    {

                        if(tempNewVec.x < tempVec2.x) // To see if we are now retracting the drag object
                        {
                            tempPos.x += tempNewVec.x - tempVec2.x;
                            if (tempPos.x < maxPos.x && tempPos.x > minPos.x) // Make sure where we are moving it to is a valid place so we don't bounce back
                            {
                                tempPos.y = dragObject.transform.position.y;
                                dragObject.transform.position = tempPos;
                            }

                        }
                        else
                        {
                            tempPos.x += tempNewVec.x - tempVec2.x;
                            if (tempPos.x < maxPos.x && tempPos.x > minPos.x) // Make sure where we are moving it to is a valid place so we don't bounce back
                            {
                                tempPos.y = dragObject.transform.position.y;
                                dragObject.transform.position = tempPos;
                            }
                        }
                    }
                    else if(dragObject.transform.position.x >= maxPos.x) // Edge case to make sure we are not going too far, if so, set it to just below the max possibel position.
                    {
                        maxPos.x -= 0.001f;
                        dragObject.transform.position = maxPos;
                    }

                    else if(dragObject.transform.position.x <= minPos.x) // Edge case to make sure we are not going too far back, if so, set it to just abovet he minimum possible position
                    {
                        minPos.x += 0.001f;
                        dragObject.transform.position = minPos;
                    }
                }
            }
        }
    }
}
