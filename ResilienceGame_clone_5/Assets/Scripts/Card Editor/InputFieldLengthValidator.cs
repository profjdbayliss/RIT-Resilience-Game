using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InputFieldLengthValidator : MonoBehaviour
{
    public TMP_InputField inputField;
    public int maxLength = 50;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        if (inputField == null)
        {
            Debug.LogError("InputFieldLengthValidator: No input field assigned!");
            return;
        }

        inputField.onValueChanged.AddListener(ValidateInputLength);
    }

    void ValidateInputLength(string input)
    {
        if (input.Length > maxLength)
        {
            inputField.text = input.Substring(0, maxLength);
        }
    }
}
