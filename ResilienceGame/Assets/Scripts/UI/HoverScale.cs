using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public GameObject targetObject;
    public float delay = 0.5f; 
    public float maxHeightOffset = 100;
    public bool SlippyOff = false;
    private float timer = 0; 
    private bool isHovering = false; 
    private bool isScaled = false;
    private bool wasDropped = false;
    private bool mPointerDown = false;
    public Vector2 previousScale = Vector2.zero;
    [SerializeField] private GameObject glow;
    AudioSource audio;
    [SerializeField] private AudioClip glowSound;
    private bool playGlowSound;

    void Start()
    {
        previousScale = this.gameObject.transform.localScale;
        glow.SetActive(false);

        audio = GetComponent<AudioSource>();

        playGlowSound = true;
    }

    void Update()
    {
        //Gets the cards value
        var theCard = targetObject.GetComponent<Card>();

        //Checks if the mouse is hovering and if its in the player's hands
        if (isHovering && theCard.cardZone == GameObject.FindGameObjectWithTag("PlayerHandLocation"))
        {
            glow.SetActive(true);
            if (playGlowSound)
            {
                audio.PlayOneShot(glowSound, 0.2f);
                playGlowSound = false;
            }
        }
        else if (playGlowSound == false && !isHovering && theCard.cardZone == GameObject.FindGameObjectWithTag("PlayerHandLocation"))
        {
            playGlowSound = true;
            glow.SetActive(false);
        }

        if (SlippyOff)
        {
            if (isHovering && !isScaled)
            {
                ScaleCard(2.0f);
            }
            else if (isScaled && !isHovering)
            {
                ResetScale();
            }
        }
        else
        // always scale a dragged card to make it easier to get to where you're going
        if (mPointerDown && !SlippyOff)
        {
            if (!isScaled) ScaleCard(.5f);
        } 
      
        else
        if (isHovering && !mPointerDown)
        {
            if (!isScaled) ScaleCard(.5f);

            // current card game has no extra info, so this isn't used
            //timer += Time.deltaTime;
            //if (timer >= delay)
            //{
            //    targetObject.SetActive(true);
            //    //if(targetObject.transform.localPosition.y < originalPosition.y + maxHeightOffset)
            //    //{
            //    //    targetObject.transform.localPosition += new Vector3(0, 1, 0);
            //    //}
            //}
        }
        //toggles the scaling effect to scale it back to its original size
        else if (isScaled && wasDropped) { ResetScale(); wasDropped = false; }
        else if (isScaled && !mPointerDown) { ResetScale(); }
        //else if (isScaled) { ResetScale(); ResetPosition(originalPosition); }
    }

    public void ScaleCard(float scaleAmount)
    {
        if(!isScaled)
        this.gameObject.layer = 30; 
        else
        {
            this.gameObject.layer = 6;
        }

        Vector2 tempScale = targetObject.transform.localScale;
        //Vector3 offset = targetObject.transform.localPosition;
        
        previousScale = tempScale;
        tempScale.x = (float)(targetObject.transform.localScale.x + scaleAmount);
        tempScale.y = (float)(targetObject.transform.localScale.y + scaleAmount);
        //offset.y = offset.y + scaleAmount * 200;
        
        targetObject.transform.localScale = tempScale;
        //targetObject.transform.localPosition = offset;

        isScaled = !isScaled;
    }

    public void ResetScale()
    {
        isScaled = false;
        targetObject.transform.localScale = previousScale;
    }

    public void Drop()
    {
        wasDropped = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true; 
        timer = 0;     
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        timer = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        wasDropped = false;
        isHovering = false;
        timer = 0;
        mPointerDown = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        mPointerDown = false;
        wasDropped = true;
    }
}
