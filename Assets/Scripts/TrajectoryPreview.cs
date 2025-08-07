using UnityEngine;

[System.Serializable]
public class TrajectorySettings
{
    [Header("Trajectory Parameters")]
    [Range(0f, 90f)]
    public float launchAngle = 45.0f;
    public float initialSpeed = 10.0f;
    
    [Header("Visual Settings")]
    public float stripLength = 1.0f;
    public float stripWidth = 0.5f;
    [Range(0f, 1f)]
    public float alpha = 0.8f;
    public Color trajectoryColor = new Color(1f, 0.5f, 0f, 1f);
}

public class TrajectoryPreview : MonoBehaviour
{
    [Header("Settings")]
    public TrajectorySettings settings = new TrajectorySettings();
    
    [Header("Components")]
    //public Material trajectoryMaterial;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    
    private Mesh trajectoryMesh;
    private bool isVisible = false;
    
    // Physics constants
    private const float GRAVITY = 9.81f;
    
    public bool startVisible = false;
    
    void Start()
    {
        // Create the trajectory mesh if components are assigned
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
            
        //if (trajectoryMaterial != null)
        //{
            // Create a material instance to avoid modifying the original
            //meshRenderer.material = new Material(trajectoryMaterial);
        //}
        
        UpdateMaterialProperties();
        
        SetVisible(startVisible);
    }
    
    void Update()
    {
        // Update material properties if they've changed
        UpdateMaterialProperties();
    }
    
    public void SetVisible(bool visible)
    {
        isVisible = visible;
        if (meshRenderer != null)
            meshRenderer.enabled = visible;
    }
    
    public void ShowTrajectory(float angle, float speed)
    {
        // Set trajectory parameters
        settings.launchAngle = angle;
        settings.initialSpeed = speed;
        
        UpdateMaterialProperties();
        SetVisible(true);
    }
    
    public void HideTrajectory()
    {
        SetVisible(false);
    }
    
    private void UpdateMaterialProperties()
    {
        if (meshRenderer?.material == null) return;
        
        Material mat = meshRenderer.material;
        mat.SetFloat("_LaunchAngle", settings.launchAngle);
        mat.SetFloat("_InitialSpeed", settings.initialSpeed);
        mat.SetFloat("_StripLength", settings.stripLength);
        mat.SetFloat("_StripWidth", settings.stripWidth);
        mat.SetFloat("_Alpha", settings.alpha);
        mat.SetColor("_Color", settings.trajectoryColor);
    }
    
    // Utility method to calculate where projectile will land
    public Vector3 CalculateLandingPoint(Vector3 startPos, Vector3 direction, float angle, float speed)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate horizontal velocity component
        float vx = speed * Mathf.Cos(angleRad);
        float vy = speed * Mathf.Sin(angleRad);
        
        // Calculate flight time (when projectile hits ground, y = 0)
        float flightTime = (2f * vy) / GRAVITY;
        
        // Calculate horizontal distance
        float distance = vx * flightTime;
        
        Vector3 horizontal = new Vector3(direction.x, 0, direction.z).normalized;
        Vector3 landingPoint = startPos + horizontal * distance;
        
        return landingPoint;
    }
    
    // Calculate the actual trajectory points for debugging or other uses
    public Vector3[] GetTrajectoryPoints(Vector3 startPos, Vector3 direction, float angle, float speed, int pointCount = 50)
    {
        Vector3[] points = new Vector3[pointCount];
        Vector3 horizontal = new Vector3(direction.x, 0, direction.z).normalized;
        
        float angleRad = angle * Mathf.Deg2Rad;
        
        // Calculate velocity components
        float vx = speed * Mathf.Cos(angleRad);
        float vy = speed * Mathf.Sin(angleRad);
        
        // Calculate total flight time
        float totalTime = (2f * vy) / GRAVITY;
        
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1) * totalTime;
            
            // Calculate position at time t
            float x = vx * t;
            float y = vy * t - 0.5f * GRAVITY * t * t;
            
            // Don't go below ground level
            y = Mathf.Max(0, y);
            
            points[i] = startPos + horizontal * x + Vector3.up * y;
        }
        
        return points;
    }
    
    // Overload for backward compatibility
    public Vector3[] GetTrajectoryPoints(Vector3 startPos, Vector3 direction, float power, int pointCount = 50)
    {
        return GetTrajectoryPoints(startPos, direction, settings.launchAngle, power, pointCount);
    }
    
    private void OnValidate()
    {
        // Update mesh when values change in editor
        if (Application.isPlaying && trajectoryMesh != null)
        {
            UpdateMaterialProperties();
        }
    }
    
    private void OnDestroy()
    {
        if (trajectoryMesh != null)
        {
            DestroyImmediate(trajectoryMesh);
        }
    }
}
