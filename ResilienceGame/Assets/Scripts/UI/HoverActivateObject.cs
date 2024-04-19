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

    void Update()
    {
        if (isHovering)
        {
            timer += Time.deltaTime;  
            if (timer >= delay)
            {
                targetObject.SetActive(true); 
            }
        }
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
