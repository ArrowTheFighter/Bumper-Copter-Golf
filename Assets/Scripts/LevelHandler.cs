using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    public static LevelHandler instance;

    public Transform startPos;

    void Awake()
    {
        if (instance != this)
        {
            instance = this;
        }
    }

    public Vector3 GetSpawnPosition()
    {
        return startPos.position;
     }

    
}
