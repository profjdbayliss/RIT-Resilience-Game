using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverActivateObject : MonoBehaviour {
    public Image hoverImage;
    public void ActivateHover() {
        hoverImage.enabled = true;
    }
    public void DeactivateHover() {
        hoverImage.enabled = false;

    }

    #region old

    //public float delay = 0.5f; 
    //public float maxHeightOffset = 100;
    //public Boolean highlight = false;
    //public GameObject highlightObject;
    //public Boolean scale = false;
    //public GameObject scaleObject;
    //private Vector3 originalPosition;
    //private float timer = 0; 
    //private bool isHovering = false; 
    //private bool isScaled = false;

    //void Start()
    //{
    //    originalPosition = transform.position;
    //}
    //void Update()
    //{
    //    //will scale the object that is being hovered over and will count a timer until it shows extra card info. 
    //    if (isHovering)
    //    {
    //        if (!isScaled & scale)
    //        {
    //            ScaleCard(.5f);

    //            timer += Time.deltaTime;
    //            if (timer >= delay)
    //            {
    //                // hoverObject.SetActive(true);
    //                if (scaleObject.transform.localPosition.y < originalPosition.y + maxHeightOffset)
    //                {
    //                    scaleObject.transform.localPosition += new Vector3(0, 1, 0);
    //                }
    //            }
    //        }
    //        if (highlight) highlightObject.SetActive(true);
    //    }
    //    //toggles the scaling effect to scale it back to its original size
    //    else
    //    { if (isScaled & scale) ScaleCard(-.5f); 
    //      if (highlight) highlightObject.SetActive(false);
    //    }
    //}

    //public void ScaleCard(float scaleAmount)
    //{
    //    if(!isScaled)
    //    this.gameObject.layer = 30; 
    //    else
    //    {
    //        this.gameObject.layer = 6;
    //    }

    //    Vector2 tempScale = scaleObject.transform.parent.localScale;
    //    Vector3 offset = scaleObject.transform.parent.localPosition;
    //    tempScale.x = (float)(scaleObject.transform.parent.localScale.x + scaleAmount);
    //    tempScale.y = (float)(scaleObject.transform.parent.localScale.y + scaleAmount);
    //    offset.y = offset.y + scaleAmount * 200;

    //    scaleObject.transform.parent.localScale = tempScale;
    //    scaleObject.transform.parent.localPosition = offset;

    //    isScaled = !isScaled;
    //}
    //public void OnPointerEnter(PointerEventData eventData)
    //{
    //    isHovering = true; 
    //    timer = 0;     
    //}

    //public void OnPointerExit(PointerEventData eventData)
    //{
    //    isHovering = false;       
    //    timer = 0;
    //    if (scale)
    //    {
    //        scaleObject.transform.SetLocalPositionAndRotation(new Vector3(), scaleObject.transform.rotation);
    //        //scaleObject.SetActive(false);
    //    }
    //}

    #endregion
}