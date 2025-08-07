using UnityEngine;
using Unity.Netcode;

public class BallNetwork : NetworkBehaviour
{
    Rigidbody rb;
    [SerializeField] LayerMask groundIgnoreLayers;
    [SerializeField] LayerMask WaterLayer;
    [SerializeField] float groundDampening;
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

    }

    [ServerRpc(RequireOwnership = false)]
    public void HitBallServerRpc(Vector3 hitVector)
    {
        print("getting hit");
        inAir = true;
        rb.linearVelocity = hitVector;
        rb.linearDamping = 0;
        lastPosition = transform.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject, WaterLayer))
        {
            print("landed in water");
            rb.linearVelocity = Vector3.zero;
            transform.position = lastPosition;
            inAir = false;
            rb.linearDamping = groundDampening;

        }
        else if (IsInLayerMask(collision.gameObject, ~groundIgnoreLayers))
        {
            print("hit ground");
            inAir = false;
            rb.linearDamping = groundDampening;
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
