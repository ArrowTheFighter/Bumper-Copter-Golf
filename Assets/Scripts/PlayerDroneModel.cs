using UnityEngine;

public class PlayerDroneModel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // 

    private GameObject hammerJointObj;
    private GameObject hammerObj;
    
    public PlayerNetwork playerNetwork;
    
    public float aimLerpSpeed = 2.0f;
    
    private float aimAngle = 0f;
    
    //private GameObject cameraObj;
    
    private Quaternion jointHomeRotation;
    private Quaternion hammerHomeRotation;

    void Start()
    {
        hammerJointObj = transform.Find("HammerJoint").gameObject;
        jointHomeRotation = hammerJointObj.transform.localRotation;
        jointHomeRotation *= Quaternion.Euler(0, 180, 0);
        hammerObj = transform.Find("Hammer").gameObject;
        hammerHomeRotation = hammerObj.transform.localRotation;
        hammerHomeRotation *= Quaternion.Euler(0, 180, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!playerNetwork.IsOwner) return;
        
        Vector3 upVec = Vector3.up;
        if (playerNetwork.lastVel.magnitude > 0.05f) {
            Vector3 velDir = playerNetwork.lastVel;
            velDir.y = 0;
            velDir = Vector3.ClampMagnitude(velDir, 2.0f);
            velDir = velDir * 0.3f;

            upVec += velDir;
            upVec = upVec.normalized;
            
            Quaternion tilt = CalculateTilt(transform.localRotation, upVec, Time.fixedDeltaTime);
            transform.localRotation = tilt;
        }

        //lastVel = rb.linearVelocity;
        float forwardInput = 0;
        float rightInput = 0;
        if (Input.GetKey(KeyCode.T)) {
            forwardInput += 1f;
        }
        if (Input.GetKey(KeyCode.G)) {
            forwardInput -= 1f;
        }
        if (Input.GetKey(KeyCode.F)) {
            rightInput -= 1;
        }
        if (Input.GetKey(KeyCode.H)) {
            rightInput += 1;
        }

        Vector3 forwardDir = playerNetwork.Camera.transform.forward;
        Vector3 forwardAmt = forwardDir.normalized * forwardInput;
        Vector3 rightAmt = playerNetwork.Camera.transform.right * rightInput;
        Vector3 aimVector = forwardAmt + rightAmt;

        aimVector.y = 0;
        aimVector = aimVector.normalized;
        float targetAimAngle = aimAngle;
        if (aimVector.magnitude > 0.01f) {
            targetAimAngle = Mathf.Atan2(aimVector.x, aimVector.z) * 180 / Mathf.PI;
        }
        
        aimAngle = Mathf.MoveTowardsAngle(aimAngle, targetAimAngle, Time.fixedDeltaTime * 360.0f * aimLerpSpeed);
        hammerJointObj.transform.localRotation = Quaternion.Euler(0, aimAngle, 0) * jointHomeRotation;
        hammerObj.transform.localRotation = Quaternion.Euler(0, aimAngle, 0) * hammerHomeRotation;
    }
    
    private Quaternion CalculateTilt(Quaternion currentRotation, Vector3 newUpVector, float timeDelta)
    {
        Vector3 targetForward = Vector3.Cross(Vector3.right, newUpVector).normalized;
        
        if (Vector3.Dot(targetForward, Vector3.forward) < 0) { targetForward = -targetForward; }
        
        var targetRotation = Quaternion.LookRotation(targetForward, newUpVector);

        return Quaternion.Slerp(currentRotation, targetRotation, timeDelta * 3.0f);
    }
}
