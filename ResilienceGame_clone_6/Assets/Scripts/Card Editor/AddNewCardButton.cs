using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AddNewCardButton : MonoBehaviour
{
    public Button button;


    void Start()
    {
        button.onClick.AddListener(AddNewCard);
    }

    void Update()
    {
        
    }

    public void AddNewCard()
    {
        CardViewer.instance.ClearSelectedCard();
        CardViewer.instance.ShowCardEditor();
    }
}
