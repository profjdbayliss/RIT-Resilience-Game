using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class slippy : MonoBehaviour, IDragHandler, IScrollHandler
{
    public GameObject gameCanvas;

    public Camera cam;

    public GameObject map;

    public GameObject tiles;

    public float maxScale;

    public float minScale;

    // Start is called before the first frame update
    void Start()
    {
        maxScale = 3.0f;
        minScale = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnScroll(PointerEventData pointer)
    {
        Debug.Log("SCrolling slippy");
        if (pointer.scrollDelta.y > 0.0f)
        {
            if (map.transform.localScale.x <= maxScale)
            {
                Vector2 tempScale = map.transform.localScale;
                tempScale.x += 0.05f;
                tempScale.y += 0.05f;
                map.transform.localScale = tempScale;
            }
            else
            {
                Vector2 tempScale = map.transform.localScale;
                tempScale.x = maxScale;
                tempScale.y = maxScale;
                map.transform.localScale = tempScale;
            }


            //gameCanvas.GetComponent<Canvas>().planeDistance -= 0.01f;
        }
        else
        {
            //gameCanvas.GetComponent<Canvas>().planeDistance += 0.01f;
            if (map.transform.localScale.x >= minScale)
            {
                Vector2 tempScale = map.transform.localScale;
                tempScale.x -= 0.05f;
                tempScale.y -= 0.05f;
                map.transform.localScale = tempScale;
            }
            else
            {
                Vector2 tempScale = map.transform.localScale;
                tempScale.x = minScale;
                tempScale.y = minScale;
                map.transform.localScale = tempScale;
            }

        }
    }

    public void OnDrag(PointerEventData pointer)
    {
        Debug.Log("DRAG SLIPPY");
        if (tiles.gameObject.activeSelf) // Check to see if the gameobject this is attached to is active in the scene
        {
            // Create a vector2 to hold the previous position of the element and also set our target of what we want to actually drag.
            Vector2 tempVec2 = default(Vector2);
            RectTransform target = map.gameObject.GetComponent<RectTransform>();
            Vector2 tempPos = target.transform.localPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position - pointer.delta, pointer.pressEventCamera, out tempVec2) == true) // Check the older position of the element and see if it was previously
            {
                Vector2 tempNewVec = default(Vector2); // Create a new Vec2 to track the current position of the object
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position, pointer.pressEventCamera, out tempNewVec) == true)
                {
                    tempPos.x += tempNewVec.x - tempVec2.x;
                    tempPos.y += tempNewVec.y - tempVec2.y;
                    //tempPos.y = map.transform.localPosition.y;
                    map.transform.localPosition = tempPos;
                }
            }
        }
    }
}
