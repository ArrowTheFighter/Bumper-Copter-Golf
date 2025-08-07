using UnityEngine;
using Unity.Netcode;

public class BallNetwork : NetworkBehaviour
{
    Rigidbody rb;
    [SerializeField] LayerMask groundIgnoreLayers;
    [SerializeField] float groundDampening;
    bool inAir;
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
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject, ~groundIgnoreLayers))
        {
            print("hit ground");
            inAir = false;
            rb.linearDamping = groundDampening;
         }
    }

    public bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }

}
