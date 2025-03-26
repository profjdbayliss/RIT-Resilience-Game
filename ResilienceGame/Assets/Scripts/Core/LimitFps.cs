using UnityEngine;

public class LimitFps : MonoBehaviour
{
    // Script to reduce heat and energy consumption of the game.
    void Start()
    {
        Application.targetFrameRate = 60;
    }

}
