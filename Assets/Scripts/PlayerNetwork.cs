using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine.Animations;
using Unity.Mathematics;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] float speed = 5;
    [SerializeField] float maxSpeed = 20;
    [SerializeField] float hoverForce = 100;
    [SerializeField] float jumpForce = 100;
    [SerializeField] float jumpDownForce = 10;
    [SerializeField] float groundHeight;
    [SerializeField] LayerMask groundIgnore;
    [SerializeField] LayerMask BallLayer;
    [SerializeField] public GameObject Camera;
    public GameObject BallPrefab;
    [HideInInspector] public GameObject BallObject;
    public Rigidbody rb;
    public Vector3 lastVel;
    NetworkObject networkObject;
    float bounceCooldown;
    bool CanJump = true;
    bool Jumping;
    float jumpCooldown;
    public bool chargingShot;
    float chargeShotDuration;
    
    public PlayerDroneModel droneModel;
    
    
    public bool aimFollowsCamera = true;
    public float aimAngle = 0f;

    public float launchStrength = 3f;
    public float launchVelocity = 22f;
    public float chipAngle = 35.0f;
    
    [SerializeField] float maxLaunchStrength = 5.8f;
    [SerializeField] float launchStrengthIncrement = 0.3f;


    void Start()
    {
        PlayerMethodSender.instance.playerNetworks.Add(this);
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
        if (IsOwner)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Camera.SetActive(true);
            transform.position = LevelHandler.instance.GetSpawnPosition();
            SpawnGolfBallServerRpc();
        }
    }

    [ServerRpc]
    void SpawnGolfBallServerRpc(ServerRpcParams rpcParams = default)
    {
        Vector3 ballPos = LevelHandler.instance.GetBallSpawnPosition();
        Transform spawnedTransform = Instantiate(BallPrefab, ballPos, quaternion.identity).transform;

        NetworkObject spawnedNetowrkObj = spawnedTransform.GetComponent<NetworkObject>();
        spawnedNetowrkObj.Spawn(true);

        ulong clientid = rpcParams.Receive.SenderClientId;
        ReturnBallReferenceClientRpc(spawnedNetowrkObj.NetworkObjectId,
        new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientid } } });
        spawnedTransform.GetComponent<BallNetwork>().SetBallColorClientRpc(clientid);
    }

    [ClientRpc]
    void ReturnBallReferenceClientRpc(ulong ballNetId, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(ballNetId, out var netObj))
        {
            // Store your reference here
            GameObject myBall = netObj.gameObject;
            BallObject = myBall;
            if (myBall.TryGetComponent(out BallNetwork ballNetwork))
            {
                ballNetwork.playerNetwork = this;
            }

            // Example: save it in a local variable
            Debug.Log("Ball reference received: " + myBall.name);
        }
    }


    void Update()
    {
        if (!IsOwner) return;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            chargeShotDuration = 0;
            launchVelocity = 0;
            chargingShot = true;
        }
        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.LeftShift))
        {
            TryHitBall();
            chargingShot = false;
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
        
        if (Jumping && Time.time < jumpCooldown)
        {
            if (rb.linearVelocity.y > 0.01f)
            {
                print("adding jump down force");
                rb.AddForce(Vector3.down * jumpDownForce * Time.deltaTime);
            }
        }
        else if (Jumping && Time.time > jumpCooldown)
        {
            if (rb.linearVelocity.y > 0.01f)
            {
                print("adding jump down force");
                rb.AddForce(Vector3.down * jumpDownForce * Time.deltaTime);
            }
            else
            {
                print("finishing jump down force");
                Jumping = false;
            }
        }
        else if (Physics.Raycast(transform.position, Vector3.down, groundHeight, ~groundIgnore))
        {
            if (Time.time > jumpCooldown) CanJump = true;
            rb.AddForce(Vector3.up * hoverForce * Time.deltaTime);
        }
        else if (!Physics.Raycast(transform.position, Vector3.down, groundHeight + .2f, ~groundIgnore))
        {
            rb.AddForce(Vector3.down * hoverForce * 2 * Time.deltaTime);
        }
        else
        {
            if (Time.time > jumpCooldown) CanJump = true;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Lerp(rb.linearVelocity.y, 0, 0.1f), rb.linearVelocity.z);
        }
        
        LaunchStrengthUpdate();
    }

    void LaunchStrengthUpdate()
    {

        if (!chargingShot) return;
        chargeShotDuration += Time.fixedDeltaTime * 2;
        //float scrollAmount = Input.GetAxis("Mouse ScrollWheel");

        float oldLaunchStrength = launchStrength;

        //launchStrength += scrollAmount * launchStrengthIncrement;
        launchStrength = chargeShotDuration;
        launchStrength = Mathf.Clamp(launchStrength, 0, maxLaunchStrength);
        if (launchStrength + 0.01f > maxLaunchStrength)
        {
            launchStrength = 2;
         }

        if (oldLaunchStrength != launchStrength)
        {
            Debug.Log("Launch strength: " + launchStrength);
        }

        launchVelocity = Mathf.Sqrt(launchStrength) * 10;

    }

    void Jump()
    {
        if (!CanJump) return;
        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;
        rb.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
        CanJump = false;
        Jumping = true;
        jumpCooldown = Time.time + 0.05f;
    }

    void TryHitBall()
    {
        if (!IsOwner) return;
        if (Physics.SphereCast(transform.position + Vector3.up, 1, Vector3.down, out RaycastHit hitInfo, 3, BallLayer))
        {
            if (hitInfo.collider.TryGetComponent(out BallNetwork ballNetwork))
            {
                Vector3 upForwardVector = Quaternion.Euler(-chipAngle, 0, 0) * Vector3.forward;
                Vector3 launchImpulse = Quaternion.Euler(0, aimAngle, 0) * (upForwardVector * launchVelocity);
                ballNetwork.HitBallServerRpc(launchImpulse);
            }
        }
        
        if (droneModel != null) {
            droneModel.SwingHammer();
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
