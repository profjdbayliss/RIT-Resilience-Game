using Mirror;

public class HostComponent : NetworkBehaviour
{
    void Start()
    {
        if (!isServer)
        {
            gameObject.SetActive(false);
        }
    }
}