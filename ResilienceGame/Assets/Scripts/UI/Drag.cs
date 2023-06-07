using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    // Establish necessary fields
    public Vector3 minPos;
    public Vector3 maxPos;
    public RaycastHit hit;
    public Ray mouseRay;
    public BoxCollider2D collider;
    public bool isDragging = false;


    // Start is called before the first frame update
    void Start()
    {
        minPos = transform.position;
        maxPos.y = transform.position.y + 5.0f;
        collider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Camera.main.ScreenToViewportPoint(Input.mousePosition));
        if (Input.GetMouseButtonDown(0))
        {
            RaycastDrag();
            if (collider == Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition)))
            {
                Debug.Log(this.name);
                isDragging = true;
                transform.position = maxPos;
            }
            else
            {
                isDragging = false;
                transform.position = minPos;
            }
        }
        if (isDragging == true)
        {
            if(Camera.main.ScreenToViewportPoint(Input.mousePosition).x < 0.5f)
            {
                Vector3 tempPos = transform.position;
                tempPos.x = tempPos.x * (1.0f - Camera.main.ScreenToViewportPoint(Input.mousePosition).x);
                transform.position = tempPos;
            }
            //tempPos.x += (1.0f + Camera.main.ScreenToViewportPoint(Input.mousePosition).x);
            //transform.position = tempPos;
            //Debug.Log("GRABBED: " + transform.position);
            //if (transform.position.y > maxPos.y)
            //{
            //    Vector3 tempPos = transform.position;
            //    tempPos.y = maxPos.y;
            //    transform.position = tempPos;

            //}
            //else if (transform.position.y < minPos.y)
            //{
            //    Vector3 tempPos = transform.position;
            //    tempPos.y = minPos.y;
            //    transform.position = tempPos;
            //}
            //else
            //{
            //    Vector3 tempPos = transform.position;
            //    tempPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //    transform.position = tempPos;
            //}
        }
        else
        {
            //Debug.Log("NORM" + transform.position);

        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void OnMouseDown()
    {

    }

    private void OnMouseDrag()
    {
        Debug.Log("Touch");
        if (transform.position.y > maxPos.y)
        {
            Vector3 tempPos = transform.position;
            tempPos.y = maxPos.y;
            transform.position = tempPos;

        }
        else if(transform.position.y < minPos.y)
        {
            Vector3 tempPos = transform.position;
            tempPos.y = minPos.y;
            transform.position = tempPos;
        }
        else
        {
            Vector3 tempPos = transform.position;
            tempPos = Input.mousePosition;
            transform.position = tempPos;
        }
    }
    public void RaycastDrag()
    {
        mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(mouseRay,out hit))
        {
            GameObject hitObj = hit.collider.gameObject;
            Debug.Log(hitObj.name);
        }
    }

    public void OnBeginDrag(PointerEventData dragEventData)
    {
        if (this.gameObject.activeSelf)
        {
            Vector2 tempVec2 = default(Vector2);
            RectTransform target = this.gameObject.GetComponent<RectTransform>();

            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(target,dragEventData.position, dragEventData.pressEventCamera, out tempVec2) == true)
            {
                Debug.Log("GOTTEM");
                Vector2 anchoredPos = target.anchoredPosition;
                
                //this.transform.position += target.localRotation * 
            }
        }
    }

    public void OnDrag(PointerEventData dragEventData)
    {

    }

    public void OnEndDrag(PointerEventData dragEventData)
    {

    }

    //private void RaycastTest()
    //{
    //    Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (Physics.Raycast(mouseRay, out hitInfo))
    //    {
    //        Debug.Log("Hit something");
    //        GameObject hitObject = hitInfo.collider.gameObject;
    //        Debug.Log(hitObject.name);
    //        if (hitObject.tag.Equals("Test"))
    //        {
    //            Debug.Log("It should work");
    //            //hitObject.GetComponent<MeshRenderer>().material = materialTest;
    //        }
    //        if (hitObject.tag.Equals("Interactable"))
    //        {
    //            if (hitObject.GetComponent<Interactables>().isPreReqMet)
    //            {
    //                //Debug.Log("It should work");
    //                grabbedObject = hitObject;
    //                //hitObject.GetComponent<MeshRenderer>().material = materialTest;
    //                //bool to flip scroll wheel functionallity
    //                cameraRotationScript.ItemIsHeld = true;
    //                grabbedScreenPos = Camera.main.WorldToScreenPoint(grabbedObject.transform.position);
    //            }
    //
    //        }
    //    }
    //
    //}
}
