using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum Type
    {
        Resilient,
        Malicious,
        GlobalModifier
    };

    // Establish necessary fields
    public Type type;
    public string title;
    public string description;
    public RawImage img;

    // Separate these
    public float percentSuccess;
    public float potentcy;
    public int duration;
    public int cost;

    // Start is called before the first frame update
    void Start()
    {
        img = this.gameObject.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
