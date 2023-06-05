using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    // Establish necessary variables
    public string locationName;
    public TextMesh locationText;

    // Start is called before the first frame update
    void Start()
    {
        locationText = GetComponent<TextMesh>();
        locationText.text = inputFix(locationName);
        locationText.color = new Color(0, 0, 0, 0);
    }


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

    private void OnMouseOver()
    {
        //Debug.Log(locationName);
        locationText.text = inputFix(locationName);
        locationText.color = new Color(0, 0, 0, 255);
    }

    private void OnMouseExit()
    {
        locationText.color = new Color(0, 0, 0, 0);
    }
}
