using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectHelp : MonoBehaviour
{
    [SerializeField] private GameObject connectHelpCanvas;
    [SerializeField] private GameObject connectHostCanvas;
    private bool isConnectHelpCanvasActive;
    private bool isHostHelpCanvasActive;
    // Start is called before the first frame update
    void Start()
    {
        isConnectHelpCanvasActive = false;
        isHostHelpCanvasActive = false;
        connectHelpCanvas.SetActive(isConnectHelpCanvasActive);
        connectHostCanvas.SetActive(isHostHelpCanvasActive);
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
}
