using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class BallNetwork : NetworkBehaviour
{
    Rigidbody rb;
    [SerializeField] LayerMask groundIgnoreLayers;
    [SerializeField] LayerMask WaterLayer;
    [SerializeField] float groundDampening;
    [SerializeField] float dampTime = 2f;
    public PlayerNetwork playerNetwork;
    float TimeWhenHitGround;
    float fullDampTime;

    bool inAir;
    Vector3 lastPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inAir)
        {
            float remainingTime = fullDampTime - Time.time;
            float dampPercent = remainingTime / dampTime;
            rb.linearDamping = Mathf.Lerp(groundDampening, 0, dampPercent);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void HitBallServerRpc(Vector3 hitVector)
    {
        print("getting hit");
        rb.linearDamping = 0;
        lastPosition = transform.position;
        inAir = true;
        StartCoroutine(delayHit(hitVector));
    }

    IEnumerator delayHit(Vector3 hitVector)
    {
        yield return null;
        rb.linearVelocity = hitVector;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject, WaterLayer))
        {
            print("landed in water");
            rb.linearVelocity = Vector3.zero;
            transform.position = lastPosition;
            inAir = false;
            //rb.linearDamping = groundDampening;
            //fullDampTime = Time.time + dampTime;

        }
        else if (IsInLayerMask(collision.gameObject, ~groundIgnoreLayers))
        {
            print("hit ground");
            if (inAir)
            {
                fullDampTime = Time.time + dampTime;
            }
            inAir = false;
            //rb.linearDamping = groundDampening;
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Goal")
        {
            print("entered goal");
         }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Goal")
        {
            print("exited goal");
        }
    }

    public bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }

}
