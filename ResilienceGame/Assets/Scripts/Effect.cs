using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public string type;
    public string team;
    public string description;
    public int duration;

    public Effect()
    {

    }

    public Effect(string type)
    {
        this.type = type;
    }

    public Effect(string type, int duration)
    {
        this.type = type;
        this.duration = duration;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
