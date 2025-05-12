using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionTutorial : MonoBehaviour
{
    [SerializeField] GameObject popUp;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip noSound;

    //Opens the connection tutorial
    public void OpenConnectionVideo()
    {
        Application.OpenURL("https://youtu.be/PMPxtg1oOv4");
    }

    //Closes the popup
    public void ClosePopUp()
    {
        popUp.SetActive(false);
    }

    //Plays when the icon is clicked
    public void PlayNoSound()
    {
        audio.PlayOneShot(noSound, 1);
    }

    //Is used when clicking the square icon.
    public void FullScreen()
    {
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, !Screen.fullScreen);
    }
}
