using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class FileBrowser : MonoBehaviour
{
    public string filePath;
    public TMP_InputField inputField;

    public void OpenFileBrowser()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "csv", false); //Use the standaline file browser
        if (paths.Length > 0)
        {
            filePath = paths[0];
            inputField.text = filePath;
        }
    }


    public void UpdateFilePathByInputField()
    {
        filePath = inputField.text;
    }

}
