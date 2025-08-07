using UnityEngine;
using Unity.Netcode;

public class NetworkShutdownHandler : MonoBehaviour
{
    void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown();
         }
     }
}
