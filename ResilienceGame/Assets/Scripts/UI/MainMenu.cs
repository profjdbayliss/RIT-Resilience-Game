using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [SerializeField] private GameObject rulesMenu;
    [SerializeField] private string rulesPath;
    [SerializeField] private RulesPagesController rulesPagesController;
    AudioSource audio; 
    [SerializeField] private AudioClip noSound;
    [SerializeField] private GameObject tutorialFolder;

// Start is called before the first frame update
void Start() {
        var networkManager = FindObjectOfType<RGNetworkManager>();
        if (networkManager != null) {
            Destroy(networkManager.gameObject);
        }

        //Made so if the player returns to the main menu, the DeckNameHolder can be destroyed
        GameObject DeckNameHolder = GameObject.Find("DeckNameHolder");
        if (DeckNameHolder != null)
        {
            Destroy(DeckNameHolder);
        }

        audio = GetComponent<AudioSource>();
    }
    public void ToggleRulesMenu(bool enable) {
        rulesMenu.SetActive(enable);
        
    }

    // Update is called once per frame
    void Update() {

    }
    //public void NextRulesPage() {
    //    rulesPages[currentPage].SetActive(false);
    //    currentPage++;
    //    if (currentPage >= rulesPages.Count) {
    //        currentPage = 0;
    //    }
    //    pageCountText.text = $"{currentPage + 1}/{rulesPages.Count}";
    //    rulesPages[currentPage].SetActive(true);

    //}
    //public void PreviousRulesPage() {
    //    rulesPages[currentPage].SetActive(false);
    //    currentPage--;
    //    if (currentPage < 0) {
    //        currentPage = rulesPages.Count - 1;
    //    }
    //    pageCountText.text = $"{currentPage + 1}/{rulesPages.Count}";
    //    rulesPages[currentPage].SetActive(true);
    //}


    //public void ToggleRulesMenu(bool enable) {
    //    rulesMenu.SetActive(enable);
    //    rulesPages[currentPage].SetActive(enable);
    //}
    public void QuitGame() {
        Application.Quit();
    }
    public void StartGame() {
        SceneManager.LoadScene("Network Login Scene");
    }
    public void StartBlueTutorial()
    {
        SceneManager.LoadScene("Blue Player Tutorial");
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

    //Plays when the icon is clicked
    public void PlayNoSound()
    {
        audio.PlayOneShot(noSound, 1);
    }

    public void goToEditCards()
    {
        SceneManager.LoadScene(3);
    }

    //Is used when clicking the square icon.
    public void FullScreen()
    {
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, !Screen.fullScreen);
    }

    //For opening and closing the tutorial folder.
    public void OpenCloseFolder()
    {
        tutorialFolder.SetActive(!tutorialFolder.activeSelf);
    }
}
