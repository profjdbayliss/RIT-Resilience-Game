using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class slippy : MonoBehaviour, IDragHandler, IScrollHandler
{
    public GameObject gameCanvas;

    public Camera cam;

    public GameObject map;

    public GameObject tiles;

    public float maxScale;

    public float minScale;

    public InputAction resetScale;

    public PlayerInput playerInput;

    // Start is called before the first frame update
    void Start()
    {
        maxScale = 3.0f;
        minScale = 0.5f;
        if(this.gameObject.GetComponentInParent<Player>() == null)
        {
            if (this.gameObject.GetComponentInParent<MaliciousActor>() == null)
            {
                Debug.Log("GOT Slippy");
                resetScale = playerInput.actions["Reset Scale"];
            }
        }

        //if(this.gameObject.GetComponentInParent<Player>().isActiveAndEnabled == false && this.gameObject.GetComponentInParent<MaliciousActor>().isActiveAndEnabled == false)
        //{
        //    resetScale = playerInput.actions["Reset Scale"];

        //}

    }

    // Update is called once per frame
    void Update()
    {
        if (resetScale.WasPressedThisFrame())
        {
            ResetScale();
        }

    }
    public void OnScroll(PointerEventData pointer)
    {
        if (pointer.scrollDelta.y > 0.0f) // Zoom in
        {
            if ((map.transform.localScale.x + 0.05f) <= maxScale) // Only zoom in when the zoom is less than the max, we allow the zoom in
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


        }
        else
        {
            if ((map.transform.localScale.x - 0.05f) >= minScale) // Only zoom out when the zoom is more than the minimum.
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
        if (map.gameObject.activeSelf) // Check to see if the gameobject this is attached to is active in the scene
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
                    map.transform.localPosition = tempPos;
                }
            }
        }
    }

    public void ResetScale()
    {
        map.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
}
