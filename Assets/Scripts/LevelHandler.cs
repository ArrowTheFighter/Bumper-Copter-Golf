using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    public static LevelHandler instance;

    public Transform startPos;
    public Transform ballStartPos;

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

    public Vector3 GetBallSpawnPosition()
    {
        return ballStartPos.position;
    }


}
