using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertPanel : MonoBehaviour {
    [SerializeField] TextMeshProUGUI textAlertTextMesh;
    [SerializeField] GameObject textAlertPanel;
    [SerializeField] Transform ListPanel;
    [SerializeField] GameObject ListItemPrefab;
    private readonly Queue<Action> onAlertFinish = new Queue<Action>();
    Queue<string> textAlertQueue = new Queue<string>();
    List<GameObject> cardList = new List<GameObject>();

    public void ShowTextAlert(string message, float duration = -1) {
        textAlertTextMesh.text = message;
        textAlertPanel.SetActive(true);
        if (duration != -1) {
            StartCoroutine(HideTextFrame(duration));
            
        }
    }
    
    public void ShowTextAlert(string message, Action onFinish) {
        ShowTextAlert(message);
        onAlertFinish.Enqueue(onFinish);
    }

    public void ResolveTextAlert() {
        textAlertPanel.SetActive(false);
        if (onAlertFinish.Count > 0) {
            onAlertFinish.Dequeue()();  //callback when the alert is finished
        }
        if (textAlertQueue.Count > 0) {
            
            ShowTextAlert(textAlertQueue.Dequeue()); //assume all infinite duration (currently the case)
        }
    }
    private IEnumerator HideTextFrame(float time) {
        yield return new WaitForSeconds(time);
        ResolveTextAlert();
    }
    public int AddCardToSelectionMenu(GameObject card) {
        cardList.Add(card);
        card.transform.SetParent(ListPanel);
        return cardList.Count;
        

    }
    public void ToggleCardSelectionPanel(bool enable) {
        ListPanel.gameObject.SetActive(enable;
    }
   

}
