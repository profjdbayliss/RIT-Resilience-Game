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
    public GameManager gameManager;
    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        player = GameObject.FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        // Depending on how healthy the output flow of the facility is, change the color.
        if (player.seletedFacility == this.gameObject)
        {
            outline.GetComponent<RawImage>().color = Color.magenta;

        }
        else if (facility.output_flow > 75.0f)
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
            if(player.seletedFacility == null)
            {
                player.seletedFacility = this.gameObject;
            }
            else if(player.seletedFacility == this.gameObject)
            {
                player.seletedFacility = null;
            }
            else
            {
                outline.SetActive(false);
            }
        }
        else
        {
            outline.SetActive(true);
        }
    }
}
