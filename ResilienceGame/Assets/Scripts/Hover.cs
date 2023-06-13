using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Mathematics;
using TMPro;

public class Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Establish necessary variables
    public string locationName;
    public TextMeshProUGUI locationTextTMP;
    public GameObject Map;
    public Vector2 mapScalar;
    public Vector2 OGScalar = new Vector2(1920, 1080);
    public Vector2 OGDeltaScalar;

    // Start is called before the first frame update
    void Start()
    {
        locationTextTMP = GetComponent<TextMeshProUGUI>();
        locationTextTMP.text = inputFix(locationName);
        locationTextTMP.color = new Color(0, 0, 0, 0);
        Map = GameObject.Find("Map");

        // Scale the Feedback menu
        mapScalar.x = Map.GetComponent<RectTransform>().rect.width;
        mapScalar.y = Map.GetComponent<RectTransform>().rect.height;
        OGScalar = new Vector2(1920.0f * 0.66f, 1080);
        OGDeltaScalar = (mapScalar - OGScalar);

        Vector3 tempVec = new Vector3(0, 0, 0);
        float scaleCorrectionX = math.abs(this.transform.localPosition.x) / (mapScalar.x / 2.0f);
        float scaleCorrectionY = math.abs(this.transform.localPosition.y) / (mapScalar.y / 2.0f);


        // If the y is negative, we want to subtract tom make sure we go in the right direction.
        if (this.transform.localPosition.x < 0.0f)
        {
            tempVec.x = this.transform.localPosition.x - ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
        }
        else
        {
            tempVec.x = this.transform.localPosition.x + ((OGDeltaScalar.x * 0.66f) * scaleCorrectionX);
        }

        // If the y is negative, we want to subtract tom make sure we go in the right direction.
        if (this.transform.localPosition.y < 0.0f)
        {
            tempVec.y = this.transform.localPosition.y - ((OGDeltaScalar.y) * scaleCorrectionY);
        }
        else
        {
            tempVec.y = this.transform.localPosition.y + ((OGDeltaScalar.y) * scaleCorrectionY);
        }
        this.transform.localPosition = tempVec;
    }

    /// <summary>
    /// inputFix Method
    /// </summary>
    /// <param name="input">
    /// Receives a string named "input", which is the base string that is to be corrected in this method.
    /// </param>
    /// <returns>
    /// This method will convert the passed in string (known as "input") to an array of characters. Then from there we check to see
    /// if the array contains an underscore, and if it does, we will consider it as a new line to separate it. To do this, we take a substring
    /// of input up until the '_' and then add in '\n' to add a new line. Then we take another substring of input with everything after the '_'
    /// then concatenate the substrings back together.
    /// </returns>
    public string inputFix(string input)
    {
        string final = new string("test");
        char[] newString = new char[input.Length];
        if (input.Contains('_'))
        {
            string tempStr1 = input.Substring(0, input.IndexOf('_'));
            string tempStr2 = "\n";
            string tempStr3 = input.Substring(input.IndexOf('_') + 1);
            final = tempStr1 + tempStr2 + tempStr3;
        }
        else
        {
            final = input;
        }
        return final;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        locationTextTMP.text = inputFix(locationName);
        locationTextTMP.color = new Color(0, 0, 0, 255);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        locationTextTMP.color = new Color(0, 0, 0, 0);

    }
}
