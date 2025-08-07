using Unity.Netcode;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    public static LevelHandler instance;

    public Transform startPos;
    public Transform ballStartPos;

    public GameObject playerPrefab;

    void Awake()
    {
        if (instance != this)
        {
            instance = this;
        }
    }

    void Start()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            GameObject player = Instantiate(playerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
         }   
    }

    public Vector3 GetSpawnPosition()
    {
        return startPos.position;
    }

    public Vector3 GetBallSpawnPosition()
    {
        return ballStartPos.position;
    }


}
