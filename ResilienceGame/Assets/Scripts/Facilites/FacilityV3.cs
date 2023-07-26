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
        City,
        FireDept,
        Transportation
    };

    public enum FacilityState
    {
        Normal = 0,
        Informed = 1,
        Accessed = 2,
    };

    public Type type;

    public FacilityState state;

    public int facID;

    public bool isOver;
    public bool isDown;
    public bool hasChanged;

    public  float output_flow;
    public float internal_flow;
    public float external_flow;

    public int feedback;
    public int hardness;
    public int maintenance;

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
    public  float commodities;
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

    // Remove the meshrenderer and material[]
    MeshRenderer meshRenderer;
    public Material[] material;

    public FacilityV3[] facilities;
    public List<FacilityV3> connectedFacilities;


    virtual public void Start()
    {
        feedbackPanel = GameObject.Find("Feedback Panel");
        FacilityType = GameObject.Find("Facility Type").GetComponentInChildren<TextMeshProUGUI>(true);
        Flow = GameObject.Find("Flow").GetComponentInChildren<TextMeshProUGUI>(true);

        Electricity = GameObject.Find("Electricity_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Water = GameObject.Find("Water_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Fuel = GameObject.Find("Fuel_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Communications = GameObject.Find("Comms_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Health = GameObject.Find("Health_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Commodities = GameObject.Find("Commodities_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Security = GameObject.Find("Security_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Public_Goods = GameObject.Find("Public Goods_T").GetComponentInChildren<TextMeshProUGUI>(true);

        Electricity_img = GameObject.Find("Electricity_T").GetComponentInChildren<Image>(true);
        Water_img = GameObject.Find("Water_T").GetComponentInChildren<Image>(true);
        Fuel_img = GameObject.Find("Fuel_T").GetComponentInChildren<Image>(true);
        Communications_img = GameObject.Find("Comms_T").GetComponentInChildren<Image>(true);
        Health_img = GameObject.Find("Health_T").GetComponentInChildren<Image>(true);
        Commodities_img = GameObject.Find("Commodities_T").GetComponentInChildren<Image>(true);
        Security_img = GameObject.Find("Security_T").GetComponentInChildren<Image>(true);
        Public_Goods_img = GameObject.Find("Public Goods_T").GetComponentInChildren<Image>(true);

        Workers = GameObject.Find("Workers_T").GetComponentInChildren<TextMeshProUGUI>(true);
        IT = GameObject.Find("IT_T").GetComponentInChildren<TextMeshProUGUI>(true);
        OT = GameObject.Find("OT_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Phys_Security = GameObject.Find("Phys_Sec_T").GetComponentInChildren<TextMeshProUGUI>(true);
        Funding = GameObject.Find("Funding_T").GetComponentInChildren<TextMeshProUGUI>(true);

        Workers_img = GameObject.Find("Workers_T").GetComponentInChildren<Image>(true);
        IT_img = GameObject.Find("IT_T").GetComponentInChildren<Image>(true);
        OT_img = GameObject.Find("OT_T").GetComponentInChildren<Image>(true);
        Phys_Security_img = GameObject.Find("Phys_Sec_T").GetComponentInChildren<Image>(true);
        Funding_img = GameObject.Find("Funding_T").GetComponentInChildren<Image>(true);
        hasChanged = false;
    }

    virtual public void Update()
    {
        if (this.gameObject.name.Contains("Clone"))
        {
            FeedbackPanel();
        }
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
        external_flow = (electricity + water + fuel + communications + commodities + health + security + public_goods + city_resource) / 900f; //900 is max

        //take the min of the two percents
        output_flow = (float)Math.Round((Mathf.Min(internal_flow, external_flow)) * 100f);
    }

    public void SetMaterial()
    {
        //meshRenderer = GetComponent<MeshRenderer>();
        //meshRenderer.material = material[0];
    }

    public FacilityV3 FindClosestFacility<T>()
    {
        FacilityV3[] gos;
        gos = (FacilityV3[])GameObject.FindObjectsOfType(typeof(T));
        FacilityV3 closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (FacilityV3 go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        connectedFacilities.Add(closest);
        return closest;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("ENTERED");
        isOver = true;

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.Log("EXITED");
        isOver = false;
        hasChanged = false;
    }

    virtual public void FeedbackPanel()
    {      
        if (isOver == true && hasChanged == false)
        {
            hasChanged = true;
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
                    Commodities.text = commodities.ToString();
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
                    Commodities.text = commodities.ToString();
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
                    Commodities.text = commodities.ToString();
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
                    Commodities.text = commodities.ToString();
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
                    Commodities.text = commodities.ToString();
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
                    Commodities.text = commodities.ToString();
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
                    Commodities.text = commodities.ToString();
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

    /// <summary>
    /// Change Image Method
    /// This method is utilized to change the images in the feedback panel to alert the player of the status of their facilites.
    /// Depedning on their statuses, the images will either be green (best), yellow (not so good), and red (bad)
    /// </summary>
    private void ChangeImage()
    {
        // Electricity
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

        // Water
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

        // Fuel
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
        
        // Communications
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
        
        // Health
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
        
        // Commodities
        if (commodities > 50)
        {
            Commodities_img.color = Color.green;
        }

        else if (commodities >= 30)
        {
            Commodities_img.color = Color.yellow;
        }

        else if (commodities < 30)
        {
            Commodities_img.color = Color.red;
        }
        //////////////////////////////////////////////
        
        // Public Goods
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
        
        // Security
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
        
        // Workers
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
        
        // IT Levels
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
        
        // 
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
        
        // Physical Security
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
        
        // Funding
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
