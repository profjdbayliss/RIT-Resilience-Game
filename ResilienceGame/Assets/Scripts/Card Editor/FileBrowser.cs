using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;

public class FileBrowser : MonoBehaviour
{
    public string filePath;
    public TMP_InputField inputField;

    public string imagePath;
    public string imageFileName;

    private void Start()
    {
        inputField.onEndEdit.AddListener(UpdateFilePathByInputField);
    }

    public void OpenCSVFileBrowser()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", "csv", false); //Use the standaline file browser
        if (paths.Length > 0)
        {
            filePath = paths[0];
            inputField.text = filePath;
        }
    }

    public void OpenImageFileBrowser()
    {
        string[] filters = { "Image files", "png,jpg,jpeg", "All files", ".*" };
        ExtensionFilter[] extensions = { new ExtensionFilter(filters[0], filters[1].Split(',')), new ExtensionFilter(filters[2], filters[3]) };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);

        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            imagePath = paths[0];
            imageFileName = CardViewer.instance.CopyImageAndReturnFileName(imagePath);
            CardViewer.instance.UpdateImageWithFileBrowser();
        }
        else
        {
            imagePath = "";
        }
    }


    public void UpdateFilePathByInputField()
    {
        filePath = inputField.text;
    }

    public void UpdateFilePathByInputField(string value)
    {
        filePath = value;
    }

    public void SaveCSVFileBrowser()
    {
        string defaultFileName = "NewFile.csv";
        string extension = "csv";
        string[] filters = { "CSV files", extension, "All files", ".*" };
        ExtensionFilter[] extensions = { new ExtensionFilter(filters[0], filters[1]), new ExtensionFilter(filters[2], filters[3]) };

        string path = StandaloneFileBrowser.SaveFilePanel("Save File", "", defaultFileName, extensions);

        if (!string.IsNullOrEmpty(path))
        {
            if (!Path.HasExtension(path) || Path.GetExtension(path).ToLower() != "." + extension)
            {
                path += "." + extension;
            }

            File.WriteAllText(path, "");

            if (inputField != null)
            {
                inputField.text = path;
            }

            filePath = path;
        }
    }

}
