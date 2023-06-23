using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScaler : MonoBehaviour
{
    // Establish necessary fields
    public GameObject Map;
    public Vector2 mapScalar;
    public int corner; // Top left: 0, Top Right: 1, Bottom Left: 2, Bottom Right: 3


    // Start is called before the first frame update
    void Start()
    {
        Map = GameObject.Find("Central Map");

        // Scale the Feedback menu
        mapScalar.x = Map.GetComponent<RectTransform>().rect.width;
        mapScalar.y = Map.GetComponent<RectTransform>().rect.height;
        Vector3 tempPos = this.transform.position;

        switch (corner)
        {
            case 0:
                tempPos = this.transform.position;
                tempPos = this.transform.localPosition;
                tempPos.x = mapScalar.x * -0.75f; // Multiplying by 3/4ths of the map width because that is the edge of the screen, doing so by -1 to ensure it is on the left side of the screen.
                tempPos.x += (this.GetComponent<RectTransform>().rect.width / 2.0f);
                this.transform.localPosition = tempPos;
                break;

            case 1:
                tempPos = this.transform.position;
                tempPos = this.transform.localPosition;
                tempPos.x = mapScalar.x * 0.75f; // Multiplying by 3/4ths of the map width because that is the edge of the screen, doing so by -1 to ensure it is on the left side of the screen.
                tempPos.x -= (this.GetComponent<RectTransform>().rect.width / 2.0f);
                this.transform.localPosition = tempPos;
                break;

            case 2:
                tempPos = this.transform.position;
                tempPos = this.transform.localPosition;
                tempPos.x = mapScalar.x * -0.75f; // Multiplying by 3/4ths of the map width because that is the edge of the screen, doing so by -1 to ensure it is on the left side of the screen.
                tempPos.x += (this.GetComponent<RectTransform>().rect.width / 2.0f);
                tempPos.y = mapScalar.y * -1.0f;
                tempPos.x += (this.GetComponent<RectTransform>().rect.height / 2.0f);
                this.transform.localPosition = tempPos;
                break;

            case 3:
                tempPos = this.transform.position;
                tempPos = this.transform.localPosition;
                tempPos.x = mapScalar.x * 0.75f; // Multiplying by 3/4ths of the map width because that is the edge of the screen, doing so by -1 to ensure it is on the left side of the screen.
                tempPos.x += (this.GetComponent<RectTransform>().rect.width / 2.0f);
                tempPos.y = mapScalar.y * -1.0f;
                tempPos.x += (this.GetComponent<RectTransform>().rect.height / 2.0f);
                this.transform.localPosition = tempPos;
                break;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
