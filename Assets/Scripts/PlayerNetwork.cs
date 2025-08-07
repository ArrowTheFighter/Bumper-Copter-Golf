using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] float speed = 5;
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float hoverForce = 100;
    [SerializeField] float jumpForce = 100;
    [SerializeField] float groundHeight;
    [SerializeField] LayerMask groundIgnore;
    [SerializeField] LayerMask BallLayer;
    [SerializeField] public GameObject Camera;
    public Rigidbody rb;
    public Vector3 lastVel;
    NetworkObject networkObject;
    float bounceCooldown;
    bool CanJump = true;



    void Start()
    {
        PlayerMethodSender.instance.playerNetworks.Add(this);
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
        if (IsOwner)
        {
            Camera.SetActive(true);
         }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryHitBall();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
         }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsOwner) return;

        lastVel = rb.linearVelocity;
        float forwardInput = 0;
        float rightInput = 0;
        if (Input.GetKey(KeyCode.W))
        {
            forwardInput += 1f;
            //transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.S))
        {

            forwardInput -= 1f;
            //transform.Translate(Vector3.back * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            rightInput -= 1;
            //transform.Translate(Vector3.left * speed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            rightInput += 1;
            //transform.Translate(Vector3.right * speed * Time.deltaTime);
        }

        Vector3 forwardDir = Camera.transform.forward;
        forwardDir.y = 0;
        Vector3 forwardForce = forwardDir.normalized * forwardInput;
        Vector3 rightForce = Camera.transform.right * rightInput;
        Vector3 MoveForce = forwardForce + rightForce;
        rb.AddForce(MoveForce * speed * Time.deltaTime);

        Vector3 FlatVelocity = rb.linearVelocity;
        if (FlatVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        if (Physics.Raycast(transform.position, Vector3.down, groundHeight, ~groundIgnore))
        {
            CanJump = true;
            rb.AddForce(Vector3.up * hoverForce * Time.deltaTime);
        }
        else if (!Physics.Raycast(transform.position, Vector3.down, groundHeight + .2f, ~groundIgnore))
        {
            rb.AddForce(Vector3.down * hoverForce * 2 * Time.deltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Lerp(rb.linearVelocity.y, 0, 0.1f), rb.linearVelocity.z);
        }
    }

    void Jump()
    {
        if (!CanJump) return;
        rb.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
        CanJump = false;
    }

    void TryHitBall()
    {
        if (!IsOwner) return;
        print("trying to hit ball");
        if (Physics.SphereCast(transform.position, 1, Vector3.down, out RaycastHit hitInfo, 2, BallLayer))
        {
            if (hitInfo.collider.TryGetComponent(out BallNetwork ballNetwork))
            {
                Vector3 forwardDir = Camera.transform.forward;
                forwardDir.y = 0;
                Vector3 hitForce = new Vector3(forwardDir.x * 15, 3 * Random.Range(1, 2f), forwardDir.z * 15);
                ballNetwork.HitBallServerRpc(hitForce);
            }
        }
     }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return;
        if (bounceCooldown > Time.time) return;
        if (collision.gameObject.tag == "Player")
        {
            Vector3 currentVel = lastVel;
            Vector3 FlatVel = currentVel;
            FlatVel.y = 0;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            rb.AddForce(-FlatVel * 2, ForceMode.Impulse);
            if (collision.gameObject.TryGetComponent(out NetworkObject hitNetworkObject))
            {
                PlayerHitServerRpc(FlatVel.x * 2, FlatVel.z * 2, hitNetworkObject.OwnerClientId);
            }
            bounceCooldown = Time.time + 0.2f;
            // PlayerHitServerRpc();
        }
    }

    public void PlayerHitReaction(float xSpeed, float zSpeed)
    {
        print("client processing hit");
        Vector3 vel = new Vector3(xSpeed, 0, zSpeed);
        rb.AddForce(vel, ForceMode.Impulse);
    }

    [ClientRpc]
    void PlayerHitClientRpc(float xSpeed, float zSpeed, ulong hitID)
    {
        PlayerMethodSender.instance.CallPlayerHitReaction(xSpeed, zSpeed, hitID);
    }

    [ServerRpc]
    void PlayerHitServerRpc(float xSpeed,float zSpeed,ulong hitID)
    {
        //PlayerMethodSender.instance.CallPlayerHitReaction(xSpeed, zSpeed, hitID);
        PlayerHitClientRpc(xSpeed,zSpeed,hitID);
    }
}
