using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Establish necessary fields.
    public string title;
    public string caption;
    public GameObject tooltipObject;
    public TextMeshProUGUI headerContent;
    public TextMeshProUGUI captionContent;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        // When the facility info is hovered set the tooltip to be active.
        tooltipObject.SetActive(true);
        
        // Set the title and the caption of the tooltip
        headerContent.text = title;
        captionContent.text = caption;
        

        // Make sure to place the tooltip in the correctlocation.
        float toolTipWidth = tooltipObject.GetComponent<RectTransform>().rect.width;
        Vector3 tempPos = this.transform.localPosition;
        tempPos.x += (toolTipWidth/2.0f);
        tooltipObject.transform.localPosition = tempPos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // When the facility info is no longer being hovered, disable it.
        tooltipObject.SetActive(false);
    }
}
