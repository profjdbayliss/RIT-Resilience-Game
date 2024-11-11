using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectHelp : MonoBehaviour
{
    [SerializeField] private GameObject connectHelpCanvas;
    private bool isConnectHelpCanvasActive;
    // Start is called before the first frame update
    void Start()
    {
        isConnectHelpCanvasActive = false;
        connectHelpCanvas.SetActive(isConnectHelpCanvasActive);
    }

    public void ToggleConnectHelpCanvas()
    {
        isConnectHelpCanvasActive = !isConnectHelpCanvasActive;
        connectHelpCanvas.SetActive(isConnectHelpCanvasActive);
    }
}
