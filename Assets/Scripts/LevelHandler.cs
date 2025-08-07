using Unity.Netcode;
using Unity.VisualScripting;
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
        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                GameObject player = Instantiate(playerPrefab);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
            }
        }
          
    }

    public Vector3 GetSpawnPosition()
    {
        Vector3 randompos = new Vector3(startPos.position.x + Random.Range(-3f, 3f), startPos.position.y, startPos.position.z + Random.Range(-3f, 3f));
        return randompos;
    }

    public Vector3 GetBallSpawnPosition()
    {
        return ballStartPos.position;
    }


}
