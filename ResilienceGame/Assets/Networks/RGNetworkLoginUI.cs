using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RGNetworkLoginUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] internal TMP_InputField playernameInput;
    [SerializeField] internal Button hostButton;
    [SerializeField] internal Button clientButton;

    public static RGNetworkLoginUI s_instance;

    void Awake()
    {
        s_instance = this;
    }

    // Called by UI element UsernameInput.OnValueChanged
    public void ToggleButtons(string username)
    {
        hostButton.interactable = !string.IsNullOrWhiteSpace(username);
        clientButton.interactable = !string.IsNullOrWhiteSpace(username);
    }

    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
        
    }
}
