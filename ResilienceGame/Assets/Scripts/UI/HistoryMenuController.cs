using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HistoryMenuController : MonoBehaviour {
    public GameObject historyItemPrefab;
    [SerializeField]  private RectTransform menuParent;
    [SerializeField] private GameObject[] startHistoryItems = new GameObject[8];
    [SerializeField] private Queue<HistoryItem> historyItems = new Queue<HistoryItem>();
    [SerializeField] private GameObject historyTooltip;
    [SerializeField] private TextMeshProUGUI historyToolTipText;
    public static HistoryMenuController Instance { get; private set; }
    // Start is called before the first frame update
    void Awake() {
        if (Instance == null) {
            Instance = this;
            foreach (var item in startHistoryItems) {
                historyItems.Enqueue(item.GetComponent<HistoryItem>());
            }
        }
        else {
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update() {

    }
    public void AddNewHistoryItem(Card card, CardPlayer player, string message, bool fromNet, bool IsServer) {
        string s = $"{player.playerName} played {card.front.title} {message}. Description: '{card.front.description}'";
        Destroy(historyItems.Dequeue().gameObject);
        var newItem = Instantiate(historyItemPrefab, menuParent).GetComponent<HistoryItem>();
        var texture = card.front.img;
        newItem.SetCardImage(
            this,
            Sprite.Create(texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(.5f, .5f))
            );
        newItem.Tooltip = s;
        newItem.transform.SetAsFirstSibling();
        historyItems.Enqueue(newItem);

       
    }
    public void ShowHistoryTooltip(string text) {
        historyTooltip.SetActive(true);
        historyToolTipText.text = text;
    }
    public void HideHistoryTooltip() {
        historyTooltip.SetActive(false);
    }
}
