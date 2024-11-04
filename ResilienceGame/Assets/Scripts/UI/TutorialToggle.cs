using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialToggle : MonoBehaviour
{
    // Start is called before the first frame update
    #region SerializeFields
    [SerializeField] private GameManager manager;
    [SerializeField] private Canvas tutorialCanvas;
    [SerializeField] private GameObject colorLessPanel;
    [SerializeField] private GameObject regSectorPanel;
    [SerializeField] private GameObject coreSectorPanel;
    [SerializeField] private GameObject blueWeeksPanel;
    [SerializeField] private GameObject redWeeksPanel;
    #endregion
    private bool isTutorialActive;
    private PlayerTeam playerTeam;
    private bool isCoreSector;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WhenGameStarts()
    {
        playerTeam = manager.playerTeam;
        if (playerTeam == PlayerTeam.Red)
        {
            colorLessPanel.SetActive(true);
            redWeeksPanel.SetActive(true);
            blueWeeksPanel.SetActive(false);
        }
        else if (playerTeam == PlayerTeam.Blue)
        {
            colorLessPanel.SetActive(false);
            redWeeksPanel.SetActive(false);
            blueWeeksPanel.SetActive(true);
        }
        else Debug.Log("Player team not red or blue");

        isTutorialActive = false;
        tutorialCanvas.gameObject.SetActive(isTutorialActive);
    }

    public void ToggleCanvas()
    {
        UpdateCoreSectorCheck();
        isTutorialActive = !isTutorialActive;
        tutorialCanvas.gameObject.SetActive(isTutorialActive);
    }

    public void UpdateCoreSectorCheck()
    {
        isCoreSector = manager.sectorInView.isCore;
        regSectorPanel.SetActive(!isCoreSector);
        coreSectorPanel.SetActive(isCoreSector);
    }
}
