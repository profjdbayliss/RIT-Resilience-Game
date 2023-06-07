using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class FacilityV3 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public enum Type
    {
        ElectricityGeneration,
        ElectricityDistribution,
        Water,
        Fuel,
        Communications,
        Commodities,
        Health,
        Security,
        PublicGoods,
        City
    };

    public Type type;

    public bool isOver;
    public bool isDown;

    public  float output_flow;
    public float internal_flow;
    public float external_flow;

    public int feedback;
    public int hardness;
    public int maintenence;

    //internal dependencies
    //treat these as a scale 1-10
    public float workers;
    public float it_level;
    public float ot_level;
    public float phys_security;
    public float funding;

    //float percentages
    //external dependencies
    public  float electricity;
    public  float water;
    public  float fuel;
    public  float communications;
    public  float commondities;
    public  float health;
    public  float security;
    public  float public_goods;
    public  float city_resource;

    public TextMeshProUGUI FacilityType;
    public TextMeshProUGUI Flow;

    public TextMeshProUGUI Electricity;
    public TextMeshProUGUI Water;
    public TextMeshProUGUI Fuel;
    public TextMeshProUGUI Communications;
    public TextMeshProUGUI Commodities;
    public TextMeshProUGUI Health;
    public TextMeshProUGUI Security;
    public TextMeshProUGUI Public_Goods;

    public TextMeshProUGUI Workers;
    public TextMeshProUGUI IT;
    public TextMeshProUGUI OT;
    public TextMeshProUGUI Phys_Security;
    public TextMeshProUGUI Funding;

    public Image Electricity_img;
    public Image Water_img;
    public Image Fuel_img;
    public Image Communications_img;
    public Image Commodities_img;
    public Image Health_img;
    public Image Security_img;
    public Image Public_Goods_img;

    public Image Workers_img;
    public Image IT_img;
    public Image OT_img;
    public Image Phys_Security_img;
    public Image Funding_img;

    public GameObject feedbackPanel;
    public GameObject actionPanel;


    MeshRenderer meshRenderer;
    public Material[] material;

    public FacilityV3[] facilities;
    public List<FacilityV3> connectedFacilities;


    virtual public void Start()
    {
        feedbackPanel = GameObject.Find("Feedback Panel");
        FacilityType = GameObject.Find("Facility Type").GetComponentInChildren<TextMeshProUGUI>(true);
        Flow = GameObject.Find("Flow").GetComponentInChildren<TextMeshProUGUI>(true);

        Electricity = GameObject.Find("Electricity").GetComponentInChildren<TextMeshProUGUI>(true);
        Water = GameObject.Find("Water").GetComponentInChildren<TextMeshProUGUI>(true);
        Fuel = GameObject.Find("Fuel").GetComponentInChildren<TextMeshProUGUI>(true);
        Communications = GameObject.Find("Comms").GetComponentInChildren<TextMeshProUGUI>(true);
        Health = GameObject.Find("Health").GetComponentInChildren<TextMeshProUGUI>(true);
        Commodities = GameObject.Find("Commodities").GetComponentInChildren<TextMeshProUGUI>(true);
        Security = GameObject.Find("Security").GetComponentInChildren<TextMeshProUGUI>(true);
        Public_Goods = GameObject.Find("Public Goods").GetComponentInChildren<TextMeshProUGUI>(true);

        Electricity_img = GameObject.Find("Electricity").GetComponentInChildren<Image>(true);
        Water_img = GameObject.Find("Water").GetComponentInChildren<Image>(true);
        Fuel_img = GameObject.Find("Fuel").GetComponentInChildren<Image>(true);
        Communications_img = GameObject.Find("Comms").GetComponentInChildren<Image>(true);
        Health_img = GameObject.Find("Health").GetComponentInChildren<Image>(true);
        Commodities_img = GameObject.Find("Commodities").GetComponentInChildren<Image>(true);
        Security_img = GameObject.Find("Security").GetComponentInChildren<Image>(true);
        Public_Goods_img = GameObject.Find("Public Goods").GetComponentInChildren<Image>(true);

        Workers = GameObject.Find("Workers").GetComponentInChildren<TextMeshProUGUI>(true);
        IT = GameObject.Find("IT").GetComponentInChildren<TextMeshProUGUI>(true);
        OT = GameObject.Find("OT").GetComponentInChildren<TextMeshProUGUI>(true);
        Phys_Security = GameObject.Find("Phys_Sec").GetComponentInChildren<TextMeshProUGUI>(true);
        Funding = GameObject.Find("Funding").GetComponentInChildren<TextMeshProUGUI>(true);

        Workers_img = GameObject.Find("Workers").GetComponentInChildren<Image>(true);
        IT_img = GameObject.Find("IT").GetComponentInChildren<Image>(true);
        OT_img = GameObject.Find("OT").GetComponentInChildren<Image>(true);
        Phys_Security_img = GameObject.Find("Phys_Sec").GetComponentInChildren<Image>(true);
        Funding_img = GameObject.Find("Funding").GetComponentInChildren<Image>(true);
    }

    virtual public void Update()
    {      
        FeedbackPanel();
    }

    public void FindFacilities()
    {
        facilities = GameObject.FindObjectsOfType<FacilityV3>();
    }

    virtual public void CreateFacilitiesList()
    {

    }

    virtual public void SetFacilityData()
    {

    }

    virtual public void CalculateFlow()
    {
        internal_flow = (workers + it_level + ot_level + phys_security + funding) / 50f;
        external_flow = (electricity + water + fuel + communications + commondities + health + security + public_goods + city_resource) / 900f; //900 is max

        //take the min of the two percents
        output_flow = (float)Math.Round((Mathf.Min(internal_flow, external_flow)) * 100f);
    }

    public void SetMaterial()
    {
        //meshRenderer = GetComponent<MeshRenderer>();
        //meshRenderer.material = material[0];
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("ENTERED");
        isOver = true;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("EXITED");
        isOver = false;

    }

    public void OnMouseEnter()
    {
        //isOver = true;
        //Debug.Log("Made it inside mouseEnter");
        isOver = true;
    }

    public void OnMouseExit()
    {
        //Debug.Log("Made it inside mouseExit");
        isOver = false;
    }

    public void OnMouseDown()
    { 
        if(isDown == true)
        {
            isDown = false;
        }
        else
        {
            isDown = true;
        }         
    }

    virtual public void FeedbackPanel()
    {      
        if (isOver == true)
        {
            ChangeImage();
            switch (feedback)
            {             
                case 1:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = "?";
                    Water.text = "?";
                    Fuel.text = "?";
                    Communications.text = "?";
                    Health.text = "?";
                    Commodities.text = "?";
                    Security.text = "?";
                    Public_Goods.text = "?";

                    Workers.text = "?";
                    IT.text = "?";
                    OT.text = "?";
                    Phys_Security.text = "?";
                    Funding.text = "?";
                    break;
                case 2:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = "?";
                    Water.text = "?";
                    Fuel.text = "?";
                    Communications.text = "?";
                    Health.text = "?";
                    Commodities.text = "?";
                    Security.text = "?";
                    Public_Goods.text = "?";

                    Workers.text = "?";
                    IT.text = "?";
                    OT.text = "?";
                    Phys_Security.text = "?";
                    Funding.text = "?";
                    break;
                case 3:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = "?";
                    Water.text = "?";
                    Fuel.text = "?";
                    Communications.text = "?";
                    Health.text = "?";
                    Commodities.text = "?";
                    Security.text = "?";
                    Public_Goods.text = "?";

                    Workers.text = "?";
                    IT.text = "?";
                    OT.text = "?";
                    Phys_Security.text = "?";
                    Funding.text = "?";
                    break;
                case 4:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = "?";
                    IT.text = "?";
                    OT.text = "?";
                    Phys_Security.text = "?";
                    Funding.text = "?";

                    break;
                case 5:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = "?";
                    IT.text = "?";
                    OT.text = "?";
                    Phys_Security.text = "?";
                    Funding.text = "?";
                    break;
                case 6:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = "?";
                    IT.text = "?";
                    OT.text = "?";
                    Phys_Security.text = "?";
                    Funding.text = "?";
                    break;
                case 7:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = workers.ToString();
                    IT.text = it_level.ToString();
                    OT.text = ot_level.ToString();
                    Phys_Security.text = phys_security.ToString();
                    Funding.text = funding.ToString();

                    break;
                case 8:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = workers.ToString();
                    IT.text = it_level.ToString();
                    OT.text = ot_level.ToString();
                    Phys_Security.text = phys_security.ToString();
                    Funding.text = funding.ToString();
                    break;
                case 9:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = workers.ToString();
                    IT.text = it_level.ToString();
                    OT.text = ot_level.ToString();
                    Phys_Security.text = phys_security.ToString();
                    Funding.text = funding.ToString();
                    break;
                case 10:
                    FacilityType.text = type.ToString();
                    Flow.text = output_flow.ToString();

                    Electricity.text = electricity.ToString();
                    Water.text = water.ToString();
                    Fuel.text = fuel.ToString();
                    Communications.text = communications.ToString();
                    Health.text = health.ToString();
                    Commodities.text = commondities.ToString();
                    Security.text = security.ToString();
                    Public_Goods.text = public_goods.ToString();

                    Workers.text = workers.ToString();
                    IT.text = it_level.ToString();
                    OT.text = ot_level.ToString();
                    Phys_Security.text = phys_security.ToString();
                    Funding.text = funding.ToString();
                    break;
                default:
                    break;
            }
        }
    }

    void ChangeImage()
    {
        if (electricity > 50)
        {
            Electricity_img.color = Color.green;
        }

        else if (electricity >= 30)
        {
            Electricity_img.color = Color.yellow;
        }

        else if (electricity < 30)
        {
            Electricity_img.color = Color.red;
        }
        //////////////////////////////////////////////

        if (water > 50)
        {
            Water_img.color = Color.green;
        }

        else if (water >= 30)
        {
            Water_img.color = Color.yellow;
        }

        else if (water < 30)
        {
            Water_img.color = Color.red;
        }
        //////////////////////////////////////////////

        if (fuel > 50)
        {
            Fuel_img.color = Color.green;
        }

        else if (fuel >= 30)
        {
            Fuel_img.color = Color.yellow;
        }

        else if (fuel < 30)
        {
            Fuel_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (communications > 50)
        {
            Communications_img.color = Color.green;
        }

        else if (communications >= 30)
        {
            Communications_img.color = Color.yellow;
        }

        else if (communications < 30)
        {
            Communications_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (health > 50)
        {
            Health_img.color = Color.green;
        }

        else if (health >= 30)
        {
            Health_img.color = Color.yellow;
        }

        else if (health < 30)
        {
            Health_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (commondities > 50)
        {
            Commodities_img.color = Color.green;
        }

        else if (commondities >= 30)
        {
            Commodities_img.color = Color.yellow;
        }

        else if (commondities < 30)
        {
            Commodities_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (public_goods > 50)
        {
            Public_Goods_img.color = Color.green;
        }

        else if (public_goods >= 30)
        {
            Public_Goods_img.color = Color.yellow;
        }

        else if (public_goods < 30)
        {
            Public_Goods_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (security > 50)
        {
            Security_img.color = Color.green;
        }

        else if (security >= 30)
        {
            Security_img.color = Color.yellow;
        }

        else if (security < 30)
        {
            Security_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (workers >= 7)
        {
            Workers_img.color = Color.green;
        }

        else if (workers >= 5)
        {
            Workers_img.color = Color.yellow;
        }

        else if (workers <= 3)
        {
            Workers_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (it_level >= 7 )
        {
            IT_img.color = Color.green;
        }

        else if (it_level >= 5)
        {
            IT_img.color = Color.yellow;
        }

        else if (it_level <= 3)
        {
            IT_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (ot_level >= 7)
        {
            OT_img.color = Color.green;
        }

        else if (ot_level >= 5)
        {
            OT_img.color = Color.yellow;
        }

        else if (ot_level <= 3)
        {
            OT_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (phys_security >= 7)
        {
            Phys_Security_img.color = Color.green;
        }

        else if (phys_security >= 5)
        {
            Phys_Security_img.color = Color.yellow;
        }

        else if (phys_security <= 3)
        {
            Phys_Security_img.color = Color.red;
        }
        //////////////////////////////////////////////
        ///
        if (funding >= 7)
        {
            Funding_img.color = Color.green;
        }

        else if (funding >= 5)
        {
            Funding_img.color = Color.yellow;
        }

        else if (funding <= 3)
        {
            Funding_img.color = Color.red;
        }
        //////////////////////////////////////////////
    }
}
