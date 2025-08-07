using System.Collections.Generic;
using UnityEngine;

public class PlayerMethodSender : MonoBehaviour
{
    public static PlayerMethodSender instance;
    public List<PlayerNetwork> playerNetworks = new List<PlayerNetwork>();


    void Awake()
    {
        if (instance != this)
        {
            Destroy(instance);
        }
        instance = this;
    }

    public void CallPlayerHitReaction(float xSpeed, float zSpeed, ulong playerClientID)
    {
        foreach (PlayerNetwork playerNetwork in playerNetworks)
        {
            if (playerNetwork.OwnerClientId == playerClientID)
            {
                playerNetwork.PlayerHitReaction(xSpeed, zSpeed);
             }
         }
     }
}
