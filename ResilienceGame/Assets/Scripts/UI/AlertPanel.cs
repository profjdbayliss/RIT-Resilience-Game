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

    Queue<string> textAlertQueue = new Queue<string>();
    List<GameObject> effectListItems = new List<GameObject>();

    public void ShowTextAlert(string message, float duration = -1) {
        textAlertTextMesh.text = message;
        textAlertPanel.SetActive(true);
        if (duration != -1) {
            StartCoroutine(HideTextFrame(duration));
        }
    }

    public void ResolveTextAlert() {
        textAlertPanel.SetActive(false);
        if (textAlertQueue.Count > 0) {
            ShowTextAlert(textAlertQueue.Dequeue()); //assume all infinite duration (currently the case)

        }
    }
    private IEnumerator HideTextFrame(float time) {
        yield return new WaitForSeconds(time);
        ResolveTextAlert();
    }
   

}
