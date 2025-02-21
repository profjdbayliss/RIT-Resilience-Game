using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IntegerInputFieldValidator : MonoBehaviour
{
    public TMP_InputField inputField;
    public int minValue = 0;
    public int maxValue = 100;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        if (inputField == null)
        {
            Debug.LogError("InputFieldValidator: No input field assigned!");
            return;
        }

        inputField.onValueChanged.AddListener(ValidateInput);
    }

    void ValidateInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return;

        int inputValue;
        if (int.TryParse(input, out inputValue))
        {
            if (inputValue < minValue)
            {
                inputField.text = minValue.ToString();
            }
            else if (inputValue > maxValue)
            {
                inputField.text = maxValue.ToString();
            }
        }
        else
        {
            Debug.Log("Invalid input!");
            inputField.text = "";
        }
    }
}
