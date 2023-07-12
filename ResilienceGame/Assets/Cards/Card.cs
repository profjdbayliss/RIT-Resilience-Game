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

    // Static fields that are only utilized on spawn and cardloading.
    public Type type;
    public static string title;
    public static string description;
    public RawImage img;

    // Separate these -- As they will change more often, will need type
    public float percentSuccess;
    public float potentcy;
    public int duration;
    public int cost;
    // Need to add Target

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
