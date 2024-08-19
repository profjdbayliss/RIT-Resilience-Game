using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class slippy : MonoBehaviour, IDragHandler, IScrollHandler
{
    public GameObject gameCanvas;

    public Camera cam;

    public GameObject DraggableObject;

    public float maxScale;

    public float minScale;

    public Vector2 originalScale;

    public Vector3 originalPosition;

    public InputAction resetScale;

    public InputAction resetPosition;

    public PlayerInput playerInput;

    private Vector2 mOffsetPos;

    // Start is called before the first frame update
    void Start()
    {
        originalScale = this.gameObject.transform.localScale;
        originalPosition = this.gameObject.transform.position;
        // initial offset is always zero
        mOffsetPos = new Vector2();
      
        ResetScale();
    }

    // Update is called once per frame
    void Update()
    {
        if (resetScale.WasPressedThisFrame())
        {
            ResetScale();
        }

        //forces a cap in case anything gets too large or small accidentally 
        if(DraggableObject.transform.localScale.x > maxScale)
        {
            //Debug.Log("greater than max scale!");
            Vector2 tempScale = DraggableObject.transform.localScale;
            tempScale.x = maxScale;
            tempScale.y = maxScale;
            DraggableObject.transform.localScale = tempScale;
        }
        else if (DraggableObject.transform.localScale.x < minScale)
        {
            Vector2 tempScale = DraggableObject.transform.localScale;
            tempScale.x = minScale;
            tempScale.y = minScale;
            DraggableObject.transform.localScale = tempScale;
        }

    }

    public void OnScroll(PointerEventData pointer)
    {
        Debug.Log("onscroll is being called");
        if (pointer.scrollDelta.y > 0.0f) // Zoom in
        {
            if ((DraggableObject.transform.localScale.x + 0.05f) <= maxScale) // Only zoom in when the zoom is less than the max, we allow the zoom in
            {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x += 0.05f;
                tempScale.y += 0.05f;
                DraggableObject.transform.localScale = tempScale;
            }
            else
            {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x = maxScale;
                tempScale.y = maxScale;
                DraggableObject.transform.localScale = tempScale;
            }
        }
        else
        {
            if ((DraggableObject.transform.localScale.x - 0.05f) >= minScale) // Only zoom out when the zoom is more than the minimum.
            {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x -= 0.05f;
                tempScale.y -= 0.05f;
                DraggableObject.transform.localScale = tempScale;
            }
            else
            {
                Vector2 tempScale = DraggableObject.transform.localScale;
                tempScale.x = minScale;
                tempScale.y = minScale;
                DraggableObject.transform.localScale = tempScale;
            }
        }
    }

    public void OnDrag(PointerEventData pointer)
    {
        if (DraggableObject.gameObject.activeSelf)
        {
            UpdatePosition();
            RectTransform target = DraggableObject.gameObject.GetComponent<RectTransform>();
            Vector2 localPos = target.transform.localPosition;          

            // check to see where we're dragging
            Vector2 tempNewVec = default(Vector2);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(target, pointer.position, pointer.pressEventCamera, out tempNewVec))
            {
                DraggableObject.transform.localPosition = new Vector2(localPos.x + tempNewVec.x,
                    localPos.y + tempNewVec.y );
            }
        }
    }

    public void UpdatePosition()
    {
        originalPosition = gameObject.transform.position;
    }
    public void UpdateScale()
    {
        originalPosition = this.gameObject.transform.localScale;
    }

    public void ResetScale()
    {
        Transform parent = this.gameObject.transform.parent;
        this.gameObject.transform.SetParent(null,true);
        this.gameObject.transform.localScale = originalScale;
        this.gameObject.transform.SetParent(parent, true);
    }

    public void ResetPosition()
    {
        Transform parent = this.gameObject.transform.parent;
        this.gameObject.transform.SetParent(null, true);
        this.gameObject.transform.SetPositionAndRotation(new Vector3(),gameObject.transform.rotation);
        this.gameObject.transform.SetParent(parent, true);
    }

}
