using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverTimeBlocker : MonoBehaviour
{
    [SerializeField] private Image blockerImage;

    void Awake()
    {
        // Auto-assign if not set in inspector
        if (blockerImage == null)
            blockerImage = GetComponent<Image>();
    }

    void Update()
    {
        var phase = GameManager.Instance.MGamePhase;
        var team = GameManager.Instance.playerTeam;

        // Blocker should show on any phase EXCEPT the player's draw phase
        bool shouldShow = !(team == PlayerTeam.Red && phase == GamePhase.DrawRed)
                       && !(team == PlayerTeam.Blue && phase == GamePhase.DrawBlue);

        if (blockerImage != null)
        {
            var color = blockerImage.color;
            color.a = shouldShow ? 1f : 0f;
            blockerImage.color = color;
            blockerImage.raycastTarget = shouldShow;
        }
        else
        {
            // fallback: just enable/disable the object
            gameObject.SetActive(shouldShow);
        }
    }
}