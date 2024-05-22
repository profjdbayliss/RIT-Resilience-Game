using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverActivateObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject targetObject;
    public float delay = 0.5f; 

    private float timer = 0; 
    private bool isHovering = false; 
    private bool isScaled = false;

    void Update()
    {
        //will scale the object that is being hovered over and will count a timer until it shows extra card info. 
        if (isHovering)
        {
            if (!isScaled) ScaleCard(.5f);

            timer += Time.deltaTime;  
            if (timer >= delay)
            {
                targetObject.SetActive(true);
            }
        }
        //toggles the scaling effect to scale it back to its original size
        else if (isScaled) ScaleCard(-.5f);
    }

    public void ScaleCard(float scaleAmount)
    {
        Vector2 tempScale = targetObject.transform.parent.localScale;
        Vector2 offset = targetObject.transform.parent.localPosition;
        tempScale.x = (float)(targetObject.transform.parent.localScale.x + scaleAmount);
        tempScale.y = (float)(targetObject.transform.parent.localScale.y + scaleAmount);
        offset.y = offset.y + scaleAmount * 100;
        targetObject.transform.parent.localScale = tempScale;
        targetObject.transform.parent.localPosition = offset;

        isScaled = !isScaled;

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true; 
        timer = 0;     
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;       
        timer = 0;    
        targetObject.SetActive(false);
    }
}
