using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScaler : MonoBehaviour
{
    // Establish necessary fields
    public GameObject Map;
    public Vector2 mapScalar;


    // Start is called before the first frame update
    void Start()
    {
        Map = GameObject.Find("Map");

        // Scale the Feedback menu
        mapScalar.x = Map.GetComponent<RectTransform>().rect.width;
        mapScalar.y = Map.GetComponent<RectTransform>().rect.height;

        Vector3 tempPos = this.transform.position;
        tempPos = this.transform.localPosition;
        tempPos.x = mapScalar.x * -0.75f; // Multiplying by 3/4ths of the map width because that is the edge of the screen, doing so by -1 to ensure it is on the left side of the screen.
        tempPos.x += (this.GetComponent<RectTransform>().rect.width/2.0f);
        this.transform.localPosition = tempPos;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
