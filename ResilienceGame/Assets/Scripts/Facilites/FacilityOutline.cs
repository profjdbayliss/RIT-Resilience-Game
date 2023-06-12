using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FacilityOutline : MonoBehaviour, IPointerClickHandler
{
    // Establish necessary fields
    public GameObject outline;
    public FacilityV3 facility;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Depending on how healthy the output flow of the facility is, change the color.
        if(facility.output_flow > 75.0f)
        {
            outline.GetComponent<RawImage>().color = Color.green;
        }
        else if(facility.output_flow > 50.0f)
        {
            outline.GetComponent<RawImage>().color = Color.yellow;

        }
        else
        {
            outline.GetComponent<RawImage>().color = Color.red;

        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // When the facility is clicked, if it is currently being outlined, disable the outline, if not then activate it.
        if(outline.activeSelf == true)
        {
            outline.SetActive(false);
        }
        else
        {
            outline.SetActive(true);
        }
    }
}
