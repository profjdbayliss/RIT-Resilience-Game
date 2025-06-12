using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedPlayerUIHider : MonoBehaviour
{
    [SerializeField] private GameObject objectToHide;

    void Update()
    {
        if (objectToHide != null)
        {
            // Hide if red, show otherwise
            objectToHide.SetActive(GameManager.Instance.playerTeam != PlayerTeam.Red);
        }
    }
}