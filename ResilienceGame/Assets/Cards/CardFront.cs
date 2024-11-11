using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Visual parts of the card
public class CardFront : MonoBehaviour
{
    public bool blueCircle;
    public bool blackCircle;
    public bool purpleCircle; 
    public Color color; 
    public string title;
    public string description;
    public string flavor;
    //public GameObject innerTexts;
    public Texture2D background;
    public Texture2D img;

    public Image meepleBgBlack;
    public Image meepleBgBlue;
    public Image meepleBgPurple;

    //public void DisableBlackMeeple() {
    //    meepleBgBlack.enabled = false;
    //}
    //public void DisableBlueMeeple() {
    //    meepleBgBlue.enabled = false;
    //}
    //public void DisablePurpleMeeple() {
    //    meepleBgPurple.enabled = false;
    //}
}
