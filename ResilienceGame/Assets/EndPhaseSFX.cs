using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndPhaseSFX : MonoBehaviour
{
    [SerializeField] private AudioSource audio;
    [SerializeField] private Button self;

    //This is made for the end phase button so it could play a press and release sound without being play if it's disabled.
    public void playButtonSFX(AudioClip sound)
    {
        if (self.interactable == true)
        {
            audio.PlayOneShot(sound, 0.4f);
        }
    }
}
