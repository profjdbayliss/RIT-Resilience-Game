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
    [SerializeField] private GameObject discardPanel;
    [SerializeField] private GameObject colorLessPanel;
    [SerializeField] private GameObject regSectorPanel;
    [SerializeField] private GameObject coreSectorPanel;
    [SerializeField] private GameObject blueWeeksPanel;
    [SerializeField] private GameObject redWeeksPanel;
    [SerializeField] private GameObject sectorOwnerPanel;
    [SerializeField] private GameObject numCardsInDeckPanel;
    [SerializeField] private GameObject overtimePanel;
    [SerializeField] private GameObject currentlySelectedSectorPanel;
    [SerializeField] private GameObject phaseTrackerPanel;
    [SerializeField] private GameObject cardWorkerCostPanel;
    [SerializeField] private GameObject cardHistoryPanel;
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
        if (UserInterface.Instance.mapState == UserInterface.MapState.FullScreen)
        {
            tutorialCanvas.gameObject.SetActive(false);
        }
        else
        {
            tutorialCanvas.gameObject.SetActive(isTutorialActive);
        }

        // Show overtimePanel only during the draw phase
        if (manager.MGamePhase == GamePhase.DrawRed || manager.MGamePhase == GamePhase.DrawBlue)
        {
            overtimePanel.SetActive(true);
        }
        else
        {
            overtimePanel.SetActive(false);
        }
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
        if(UserInterface.Instance.mapState == UserInterface.MapState.FullScreen)
        {
            tutorialCanvas.gameObject.SetActive(false);
        }
        else tutorialCanvas.gameObject.SetActive(isTutorialActive);
    }

    public void UpdateCoreSectorCheck()
    {
        isCoreSector = manager.sectorInView.isCore;
        regSectorPanel.SetActive(!isCoreSector);
        coreSectorPanel.SetActive(isCoreSector);
    }

    public void DiscardPanelToggle(bool shouldDiscardPanelBeActive)
    {
        discardPanel.SetActive(shouldDiscardPanelBeActive);
    }

    public void StartTutorial()
    {
        manager.InitializeTutorialMode();
    }
}
