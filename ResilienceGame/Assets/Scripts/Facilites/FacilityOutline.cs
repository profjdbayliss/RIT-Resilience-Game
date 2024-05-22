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
    public Player[] players;
    public MaliciousActor maliciousActor;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        if (GameObject.FindObjectOfType<Player>() != null)
        {
            players = GameObject.FindObjectsOfType<Player>();
        }
        //maliciousActor = GameObject.FindObjectOfType<MaliciousActor>();
        maliciousActor = gameManager.maliciousActor;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.resPlayer != null)
        {
            // Depending on how healthy the output flow of the facility is, change the color.
            if (gameManager.criticalEnabled && outline.activeSelf)
            {
                // Only have players care for their facility type
                //if (facility.type == gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().type)
                if (facility.type == gameManager.resPlayer.GetComponent<Player>().type)
                {
                    outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);
                }
                //switch (facility.type)
                //{
                //    case FacilityV3.Type.ElectricityGeneration:
                //        outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                //        break;
                //    case FacilityV3.Type.ElectricityDistribution:
                //        outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                //        break;
                //    case FacilityV3.Type.Water:
                //        outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                //        break;
                //    case FacilityV3.Type.Transportation:
                //        outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                //        break;


                //    case FacilityV3.Type.Communications:
                //        outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);

                //        break;

                //    default:
                //        if (facility.type == gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().type)
                //        {
                //            outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);
                //        }
                //        break;

                //}
                //outline.GetComponent<RawImage>().color = new Color(1.0f, 0.8431372549f, 0.0f, 1.0f);
                if (gameManager.resPlayer.GetComponent<Player>().facilitiesActedUpon.Contains(this.gameObject) == true)
                //if (gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().seletedFacilities.Contains(this.gameObject) == true)
                {
                    outline.GetComponent<RawImage>().color = Color.cyan;
                }
                if (facility.isDown)
                {
                    outline.GetComponent<RawImage>().color = Color.black;

                }
            }
            else if ((gameManager.resPlayer.GetComponent<Player>().facilitiesActedUpon.Contains(this.gameObject) == true) && (gameManager.playerActive))
            //else if ((gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().seletedFacilities.Contains(this.gameObject) == true) && (gameManager.playerActive))
            {
                outline.GetComponent<RawImage>().color = Color.cyan;

            }
            else if ((gameManager.maliciousActor != null)) // gameManager.maliciousActor.gameObject.activeSelf
            {
                if ((gameManager.maliciousActor.facilitiesActedUpon.Contains(this.gameObject)) && (gameManager.playerActive == false))
                {
                    outline.GetComponent<RawImage>().color = Color.magenta;
                }
            }
            else if (facility.isDown)
            {
                outline.GetComponent<RawImage>().color = Color.black;

            }
            else if (facility.output_flow > 75.0f)
            {
                outline.GetComponent<RawImage>().color = Color.green;
            }
            else if (facility.output_flow > 50.0f)
            {
                outline.GetComponent<RawImage>().color = Color.yellow;

            }
            else
            {
                outline.GetComponent<RawImage>().color = Color.red;

            }
        }
        else if (gameManager.maliciousActor != null)
        {
            if ((gameManager.maliciousActor.gameObject.activeSelf))
            {
                if ((gameManager.maliciousActor.facilitiesActedUpon.Contains(this.gameObject)) && (gameManager.playerActive == false))
                {
                    outline.GetComponent<RawImage>().color = Color.magenta;
                }
            }
            else if (facility.isDown)
            {
                outline.GetComponent<RawImage>().color = Color.black;

            }
            else if (facility.output_flow > 75.0f)
            {
                outline.GetComponent<RawImage>().color = Color.green;
            }
            else if (facility.output_flow > 50.0f)
            {
                outline.GetComponent<RawImage>().color = Color.yellow;

            }
            else
            {
                outline.GetComponent<RawImage>().color = Color.red;

            }
        }

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // When the facility is clicked, if it is currently being outlined, disable the outline, if not then activate it.
        if (outline.activeSelf == true)
        {
            if (gameManager.playerActive)
            {
                //if (gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().seletedFacilities.Contains(this.gameObject) == false)
                if (gameManager.resPlayer.GetComponent<Player>().facilitiesActedUpon.Contains(this.gameObject) == false)
                {
                    //if (gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().type == this.gameObject.GetComponent<FacilityV3>().type)
                    if (gameManager.resPlayer.GetComponent<Player>().type == this.gameObject.GetComponent<FacilityV3>().type)
                    {
                        //gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().seletedFacilities.Add(this.gameObject);
                        gameManager.resPlayer.GetComponent<Player>().facilitiesActedUpon.Add(this.gameObject);

                    }
                    //if (gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().type == this.gameObject.GetComponent<FacilityV3>().type || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.Water || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
                    //{
                    //    gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().seletedFacilities.Add(this.gameObject);
                    //}
                }
                else
                {
                    outline.SetActive(false);
                    gameManager.resPlayer.GetComponent<Player>().facilitiesActedUpon.Remove(this.gameObject);
                    //gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().seletedFacilities.Remove(this.gameObject);
                }
            }
            else
            {
                if(gameManager.maliciousActor.facilitiesActedUpon.Contains(this.gameObject) == false)
                {
                    gameManager.maliciousActor.facilitiesActedUpon.Add(this.gameObject);

                }
                else
                {
                    outline.SetActive(false);
                    gameManager.maliciousActor.facilitiesActedUpon.Remove(this.gameObject);
                }
                //if (maliciousActor.targetFacility == null)
                //{
                //    maliciousActor.targetFacility = this.gameObject;
                //}
                //else if (maliciousActor.targetFacility == this.gameObject)
                //{
                //    maliciousActor.targetFacility = null;
                //}
                //else
                //{
                //    outline.SetActive(false);
                //}
            }

        }
        else
        {
            if (gameManager.playerActive)
            {
                //if (gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().type == this.gameObject.GetComponent<FacilityV3>().type)
                if (gameManager.resPlayer.GetComponent<Player>().type == this.gameObject.GetComponent<FacilityV3>().type)
                {
                    outline.SetActive(true);

                }
                //if (gameManager.allPlayers[gameManager.activePlayerNumber].GetComponent<Player>().type == this.gameObject.GetComponent<FacilityV3>().type || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityGeneration || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.Water || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.Transportation || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.ElectricityDistribution || this.gameObject.GetComponent<FacilityV3>().type == FacilityV3.Type.Communications)
                //{
                //    outline.SetActive(true);
                //}
            }
            else
            {
                outline.SetActive(true);
            }
        }
    }
}
