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
        tooltipObject.SetActive(true);
        headerContent.text = title;
        captionContent.text = caption;
        float toolTipWidth = tooltipObject.GetComponent<RectTransform>().rect.width;
        Vector3 tempPos = this.transform.position;
        tempPos.x += (toolTipWidth / 3.0f);
        tooltipObject.transform.position = tempPos;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipObject.SetActive(false);
    }
}
