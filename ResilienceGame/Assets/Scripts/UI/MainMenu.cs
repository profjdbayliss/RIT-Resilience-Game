using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject rulesMenu;
    [SerializeField] private string rulesPath;
    // Start is called before the first frame update
    void Start()
    {
        var networkManager = FindObjectOfType<RGNetworkManager>();
        if (networkManager != null) {
            Destroy(networkManager.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleRulesMenu(bool enable) {
        rulesMenu.SetActive(enable);
    }
    public void QuitGame() {
        Application.Quit();
    }
    public void StartGame() {
        SceneManager.LoadScene("Network Login Scene");
    }
    public void OpenPDF() {
        string filePath = Path.Combine(Application.streamingAssetsPath, rulesPath + ".pdf");

        if (File.Exists(filePath)) {
#if UNITY_EDITOR
            Process.Start(filePath);
#elif UNITY_STANDALONE_WIN
            Process.Start(filePath);
#elif UNITY_STANDALONE_OSX
            Process.Start("open", filePath);
#elif UNITY_STANDALONE_LINUX
            Process.Start("xdg-open", filePath);
#else
            Debug.LogError("Opening files not supported on this platform.");
#endif
        }
        else {
            UnityEngine.Debug.LogError("File not found: " + filePath);
        }
    }


}
