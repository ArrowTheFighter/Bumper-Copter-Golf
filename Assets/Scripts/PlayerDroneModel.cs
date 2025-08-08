using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDroneModel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // 

    private GameObject hammerJointObj;
    private GameObject hammerObj;
    
    public GameObject arcPivotObj;
    
    public PlayerNetwork playerNetwork;
    
    public float aimLerpSpeed = 2.0f;
    
    
    //private GameObject cameraObj;
    
    private Quaternion jointHomeRotation;
    private Quaternion hammerHomeRotation;
    
    private bool hammerSwinging = false;
    private float hammerSwingAmount = 0.0f;
    private float hammerSwingSpeed = 3.4f;
    private float hammerSwingProgress = 0f;
    private float hammerSwingFullAngle = -150f;


    void Start()
    {
        hammerJointObj = transform.Find("HammerJoint").gameObject;
        jointHomeRotation = hammerJointObj.transform.localRotation;
        jointHomeRotation *= Quaternion.Euler(0, 180, 0);
        hammerObj = transform.Find("Hammer").gameObject;
        hammerHomeRotation = hammerObj.transform.localRotation;
        hammerHomeRotation *= Quaternion.Euler(0, 180, 0);
        
        Invoke("UpdateHammerColor", 0.1f);
    }
    
    private void UpdateHammerColor() {
        Debug.Log("Updating hammer color");
        var hammerHeadMat = hammerObj.transform.Find("HammerHead").GetComponent<MeshRenderer>().material;
        if (hammerHeadMat) {
            if (hammerHeadMat.HasColor("_BaseColor") && GetComponent<PlayerColor>() != null) {
                Debug.Log("Setting hammer _BaseColor to " + GetComponent<PlayerColor>().GetColor(playerNetwork.OwnerClientId));
                hammerHeadMat.SetColor("_BaseColor", GetComponent<PlayerColor>().GetColor(playerNetwork.OwnerClientId));
                Debug.Log("Color set to " + hammerHeadMat.GetColor("_BaseColor"));
            } else {
                Debug.LogError("No color found for hammer head material");
            }
        } else {
            Debug.LogError("No hammer head material found");
        }
    }

    void Update()
    {
        if (playerNetwork.chargingShot)
        {

            arcPivotObj.GetComponentInChildren<TrajectoryPreview>().ShowTrajectory(playerNetwork.chipAngle, playerNetwork.launchVelocity);
            if (!arcPivotObj.activeInHierarchy)
            {
                StartCoroutine(delayShotShow());
            }
        }
        else
        {
            arcPivotObj.SetActive(false);
        }
    }

    IEnumerator delayShotShow()
    {
        yield return null;
        if (!arcPivotObj.activeInHierarchy)
        {
            arcPivotObj.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        if (!playerNetwork.IsOwner) return;
        
        if (hammerSwinging) {
            hammerSwingUpdate();
        }
        
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

        float targetAimAngle = playerNetwork.aimAngle;
        Vector3 aimVector = Vector3.zero;
        if (!playerNetwork.aimFollowsCamera) {
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
            aimVector = forwardAmt + rightAmt;
        } else {
            aimVector = playerNetwork.Camera.transform.forward;
        }
        aimVector.y = 0;
        aimVector = aimVector.normalized;

        if (aimVector.magnitude > 0.01f) {
            targetAimAngle = Mathf.Atan2(aimVector.x, aimVector.z) * 180 / Mathf.PI;
        }
        
        playerNetwork.aimAngle = Mathf.MoveTowardsAngle(playerNetwork.aimAngle, targetAimAngle, Time.fixedDeltaTime * 360.0f * aimLerpSpeed);
        hammerJointObj.transform.localRotation = Quaternion.Euler(0, playerNetwork.aimAngle, 0) * jointHomeRotation;
        arcPivotObj.transform.localRotation = Quaternion.Euler(0, playerNetwork.aimAngle, 0);

        hammerObj.transform.localRotation = Quaternion.Euler(hammerSwingAmount * hammerSwingFullAngle, 0, 0) * hammerHomeRotation;
        hammerObj.transform.localRotation = Quaternion.Euler(0, playerNetwork.aimAngle, 0) * hammerObj.transform.localRotation;
    }
    
    private void hammerSwingUpdate() {
        hammerSwingProgress += Time.fixedDeltaTime * hammerSwingSpeed;
        if (hammerSwingProgress < 0.5f) {
            hammerSwingAmount = easeOutPow(hammerSwingProgress * 2f, 4);
        } else {
            hammerSwingAmount = 1f - Mathf.Lerp(0, 1, (hammerSwingProgress - 0.5f) * 2f);
        }
        
        if (hammerSwingProgress >= 1) {
            hammerSwingProgress = 0f;
            hammerSwingAmount = 0f;
            hammerSwinging = false;
        }
    }
    
    private float easeOutPow(float x, int power) {
        return 1 - Mathf.Pow(1 - x, power);
    }
    
    private Quaternion CalculateTilt(Quaternion currentRotation, Vector3 newUpVector, float timeDelta)
    {
        Vector3 targetForward = Vector3.Cross(Vector3.right, newUpVector).normalized;
        
        if (Vector3.Dot(targetForward, Vector3.forward) < 0) { targetForward = -targetForward; }
        
        var targetRotation = Quaternion.LookRotation(targetForward, newUpVector);

        return Quaternion.Slerp(currentRotation, targetRotation, timeDelta * 3.0f);
    }
    
    public void SwingHammer() {
        hammerSwingAmount = 0f;
        hammerSwinging = true;
    }
}
