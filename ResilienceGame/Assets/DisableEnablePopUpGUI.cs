using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableEnablePopUpGUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    public void enableOrDisable()
    {
        gameObject.SetActive(!isActiveAndEnabled);
    }
}
