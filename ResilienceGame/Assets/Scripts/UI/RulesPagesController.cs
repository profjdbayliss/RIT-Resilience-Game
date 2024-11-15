using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RulesPagesController : MonoBehaviour
{
    
    [SerializeField] private RectTransform rulesParent;
    [SerializeField] private TextMeshProUGUI pageCountText;
    [SerializeField] private List<GameObject> rulesPages = new List<GameObject>();
    private int currentPage = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (rulesParent != null && rulesPages != null) {
            // rulesPages = rulesParent.GetComponentsInChildren<RectTransform>().Select(s => s.gameObject).ToList();
            rulesPages.ForEach(x => x.SetActive(false));
            pageCountText.text = $"{currentPage + 1}/{rulesPages.Count}";
            SetCurrentPageActive();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void NextRulesPage() {
        rulesPages[currentPage].SetActive(false);
        currentPage++;
        if (currentPage >= rulesPages.Count) {
            currentPage = 0;
        }
        pageCountText.text = $"{currentPage + 1}/{rulesPages.Count}";
        SetCurrentPageActive();

    }
    public void PreviousRulesPage() {
        rulesPages[currentPage].SetActive(false);
        currentPage--;
        if (currentPage < 0) {
            currentPage = rulesPages.Count - 1;
        }
        pageCountText.text = $"{currentPage + 1}/{rulesPages.Count}";
        SetCurrentPageActive();
    }

    public void SetCurrentPageActive() {
        rulesPages[currentPage].SetActive(true);
    }

    
}
