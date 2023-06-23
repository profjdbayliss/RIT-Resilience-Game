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
    public MaliciousActor maliciousActor;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        player = GameObject.FindObjectOfType<Player>();
        maliciousActor = GameObject.FindObjectOfType<MaliciousActor>();
    }

    // Update is called once per frame
    void Update()
    {
        // Depending on how healthy the output flow of the facility is, change the color.
        if (gameManager.criticalEnabled && outline.activeSelf)
        {
            switch (facility.type)
            {
                case FacilityV3.Type.ElectricityGeneration:
                    outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                    break;
                case FacilityV3.Type.ElectricityDistribution:
                    outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                    break;
                case FacilityV3.Type.Water:
                    outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                    break;
                case FacilityV3.Type.Transportation:
                    outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                    break;
                default:
                    break;

            }
            //outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);
            if(player.seletedFacility == this.gameObject)
            {
                outline.GetComponent<RawImage>().color = Color.cyan;
            }
            if (facility.isDown)
            {
                outline.GetComponent<RawImage>().color = Color.black;

            }
        }
        else if ((player.seletedFacility == this.gameObject) && (gameManager.playerActive))
        {
            outline.GetComponent<RawImage>().color = Color.cyan;

        }
        else if((maliciousActor.targetFacility == this.gameObject) && (gameManager.playerActive == false))
        {
            outline.GetComponent<RawImage>().color = Color.magenta;
        }
        else if (facility.isDown)
        {
            outline.GetComponent<RawImage>().color = Color.black;

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
            if (gameManager.playerActive)
            {
                if (player.seletedFacility == null)
                {
                    player.seletedFacility = this.gameObject;
                }
                else if (player.seletedFacility == this.gameObject)
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
                if (maliciousActor.targetFacility == null)
                {
                    maliciousActor.targetFacility = this.gameObject;
                }
                else if (maliciousActor.targetFacility == this.gameObject)
                {
                    maliciousActor.targetFacility = null;
                }
                else
                {
                    outline.SetActive(false);
                }
            }

        }
        else
        {
            outline.SetActive(true);
        }
    }
}
