using System;
using System.Reflection;
using Mirror;
using UnityEngine;

public class MessageIdFinder : MonoBehaviour
{
    void Start()
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                if (typeof(NetworkMessage).IsAssignableFrom(type) && type.IsValueType)
                {
                    ushort id = (ushort)type.FullName.GetStableHashCode();
                    if (id == 9353)
                        Debug.Log($"Found message type for ID 9353: {type.FullName}");
                }
            }
        }
    }
}
