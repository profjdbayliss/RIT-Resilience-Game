using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectHelp : MonoBehaviour
{
    // Due to how Unity works, these fields must be assigned otherwise it leads to an error.
    // On the Login screen, connectLobbyCanvas is unused, and during the main game- only connectLobbyCanvas is used
    // Currently they are just assigned arbitrarily to avoid the error, this doesn't lead to any other errors so it's an easy fix that can stay as-is
    [SerializeField] private GameObject connectHelpCanvas;
    [SerializeField] private GameObject connectHostCanvas;
    [SerializeField] private GameObject connectJoinCanvas;
    [SerializeField] private GameObject connectLobbyCanvas;
    private bool isConnectHelpCanvasActive;
    private bool isHostHelpCanvasActive;
    private bool isJoinHelpCanvasActive;
    private bool isLobbyHelpCanvasActive;
    // Start is called before the first frame update
    void Start()
    {
        isConnectHelpCanvasActive = false;
        isHostHelpCanvasActive = false;
        isJoinHelpCanvasActive = false;
        if (connectLobbyCanvas != null)
        {
            isLobbyHelpCanvasActive = false;
        }
        else
        {
            isLobbyHelpCanvasActive = true; // Default to true if connectLobbyCanvas is not assigned
        }
        //connectHelpCanvas.SetActive(isConnectHelpCanvasActive);
        
        if (connectHostCanvas != null)
        {
            isHostHelpCanvasActive = false;
        }
        else
        {
            isHostHelpCanvasActive = true; // Default to true if connectHostCanvas is not assigned
        }
        //connectHostCanvas.SetActive(isHostHelpCanvasActive);

        if (connectJoinCanvas != null)
        {
            isJoinHelpCanvasActive = false;
        }
        else
        {
            isJoinHelpCanvasActive = true; // Default to true if connectJoinCanvas is not assigned
        }
    }

    public void ToggleConnectHelpCanvas()
    {
        isConnectHelpCanvasActive = !isConnectHelpCanvasActive;
        connectHelpCanvas.SetActive(isConnectHelpCanvasActive);
    }

    public void ToggleHostHelpCanvas()
    {
        isHostHelpCanvasActive = !isHostHelpCanvasActive;
        connectHostCanvas.SetActive(isHostHelpCanvasActive);
    }

    public void ToggleJoinHelpCanvas()
    {
        isJoinHelpCanvasActive = !isJoinHelpCanvasActive;
        connectJoinCanvas.SetActive(isJoinHelpCanvasActive);
    }

    public void ToggleLobbyHelpCanvas()
    {
        isLobbyHelpCanvasActive = !isLobbyHelpCanvasActive;
        connectLobbyCanvas.SetActive(isLobbyHelpCanvasActive);
    }
}
