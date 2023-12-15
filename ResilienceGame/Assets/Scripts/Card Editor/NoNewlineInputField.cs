using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class NoNewlineInputField : MonoBehaviour
{
    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValidateInput += ValidateInput;
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (addedChar == '\n' || addedChar == '\r')
        {
            return '\0';
        }
        return addedChar;
    }
}
