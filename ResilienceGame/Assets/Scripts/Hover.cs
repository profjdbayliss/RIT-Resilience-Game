using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Establish necessary variables
    public string locationName;
    public TextMeshProUGUI locationTextTMP;

    // Start is called before the first frame update
    void Start()
    {
        locationTextTMP = GetComponent<TextMeshProUGUI>();
        locationTextTMP.text = inputFix(locationName);
        locationTextTMP.color = new Color(0, 0, 0, 0);
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
