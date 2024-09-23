using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlertPanel : MonoBehaviour {
    [SerializeField] TextMeshProUGUI alertText;
    [SerializeField] GameObject alertPanel;

    bool isShowing;
    Queue<string> alertQueue = new Queue<string>();
 

    public void ShowAlert(string message, float duration = -1) {
        alertText.text = message;
        alertPanel.SetActive(true);
        if (duration != -1) {
            StartCoroutine(HideFrameAfter(duration));
        }
    }

    public void ResolveAlert() {
        alertPanel.SetActive(false);
        if (alertQueue.Count > 0) {
            ShowAlert(alertQueue.Dequeue());

        }
    }
    private IEnumerator HideFrameAfter(float time) {
        yield return new WaitForSeconds(time);
        ResolveAlert();
    }
}
