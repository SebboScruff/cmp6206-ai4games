using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    [SerializeField] bool showDebugThings;

    public LayerMask hitMask; // which objects does the sensor want to hit?

    public enum Type{
        Line,
        RayBundle,
        SphereCast,
        DetectionBox,
        DetectionSphere
    }

    public Type sensorType = Type.Line;
    
    [Header("Sensor Settings")]
    public float raycastLength = 1.0f;
    [Range(2, 21)]public int numOfRays = 5;
    [Range(20, 360)]public int searchArc = 60;
    public float sphereRadius = 7.0f;
    public Vector3 boxExtents = new Vector3(10.0f, 1.0f, 10.0f);


    Transform cachedTransform;

    public bool Hit {get; private set;}

    public RaycastHit info;
    Collider[] detectedCol;
    public List<GameObject> detectedObjects {get; private set;}

    public Ray[] raysInBundle;

    // Start is called before the first frame update    
    void Start()
    {
        cachedTransform = GetComponent<Transform>();
        detectedObjects = new List<GameObject>();
        raysInBundle = new Ray[numOfRays];
    }

    public bool Scan(){
        Hit = false;

        Vector3 dir = cachedTransform.forward;

        switch(sensorType){
            case Type.Line:
                if(Physics.Linecast(cachedTransform.position, cachedTransform.position + dir*raycastLength, out info, hitMask, QueryTriggerInteraction.Ignore)){
                    Hit = true;
                    //Debug.Log("Hit");
                    return true;
                }
            break;

            case Type.RayBundle: // I wrote the Gizmos Draw part of this first because it was easier to visually debug.
                //fill the array
                dir = Quaternion.Euler(0f, -searchArc/2, 0f) * dir;
                raysInBundle[0] = new Ray(cachedTransform.position, dir);
                for(int i = 1; i < numOfRays; i++){
                    dir = Quaternion.Euler(0f, searchArc/(numOfRays-1), 0f) * dir;
                    raysInBundle[i] = new Ray(cachedTransform.position, dir);
                }
                // cycle through each ray to detect for collisions
                // TODO: out into array of hit info so we can actually do something with it
                // currently it just tells you whether or not any of the rays have hit something
                for(int i = 0; i < raysInBundle.Length; i++){
                    if(Physics.Raycast(raysInBundle[i], out info, raycastLength, hitMask, QueryTriggerInteraction.Ignore)){
                        Hit=true;
                        Debug.Log("Hit");
                        return true;
                    }
                }                
            break;

            case Type.SphereCast:
                if(Physics.SphereCast(new Ray(cachedTransform.position, dir*raycastLength), sphereRadius, out info, raycastLength, hitMask, QueryTriggerInteraction.Ignore)){
                    Hit = true;
                    //Debug.Log("Hit");
                    return true;
                }
            break;

            case Type.DetectionBox:
                detectedCol = Physics.OverlapBox(cachedTransform.position, boxExtents/2.0f, transform.rotation, hitMask, QueryTriggerInteraction.Ignore);
                if(detectedCol.Length > 0){
                    foreach(Collider c in detectedCol){
                        if(!detectedObjects.Contains(c.gameObject)){
                            detectedObjects.Add(c.gameObject);
                        }
                    }
                    Hit = true;
                    Debug.Log("Hit");
                    return true;
                }
                else{
                    detectedObjects.Clear();
                }
            break;

            case Type.DetectionSphere:
                detectedCol = Physics.OverlapSphere(cachedTransform.position, sphereRadius, hitMask, QueryTriggerInteraction.Ignore);
                if(detectedCol.Length > 0){
                    foreach(Collider c in detectedCol){
                        if(!detectedObjects.Contains(c.gameObject)){
                            detectedObjects.Add(c.gameObject);
                        }
                    }
                    Hit = true;
                    //Debug.Log("Hit");
                    return true;
                }
                else{
                    detectedObjects.Clear();
                }
            break;

            default:
                Debug.LogError("Error: No Behaviours for current Sensor Type");
                break;
        }
        return false;
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.F3)){
            showDebugThings = !showDebugThings;
        }
        Scan();
        if(Hit == false){
            // empty out each of the arrays of hit info if nothing is detected
            detectedObjects.Clear();
        }
    }

    void OnDrawGizmos(){
        if(showDebugThings){
            Gizmos.color = Color.white;
            if(cachedTransform == null){cachedTransform = GetComponent<Transform>();}

            if(Hit) Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            float length = raycastLength;

            switch(sensorType){
                case Type.Line:
                    if(Hit){
                        length = Vector3.Distance(this.transform.position, info.point);
                    }
                    Gizmos.DrawLine(Vector3.zero, Vector3.forward * length);
                break;

                case Type.RayBundle:
                    Vector3 dir = cachedTransform.forward;
                    // // Method 1: Start on V3.forward and then draw the rest
                    // // This method didn't work very well but I've left it here as a grim and ominous reminder
                    // int raysLeft = numOfRays;
                    // if(raysLeft%2 != 0){ // if odd number of rays, draw 1 forward then decrease i by 1
                    //     Gizmos.DrawLine(Vector3.zero, dir * length);
                    //     raysLeft--;    
                    // } // at this point the number of rays left is guaranteed to be even
                    // dir = Quaternion.Euler(0f, searchArc/2, 0f) * dir;
                    // do{ // this loop draws lines in pairs starting from the furthest out
                    //     if(dir != Vector3.forward){
                    //         Gizmos.DrawLine(Vector3.zero, dir * length);
                    //         raysLeft--;
                    //     }                    
                    //     dir = Quaternion.Euler(0f, -1*angleBetweenRays, 0f) * dir;                     
                    // }while(raysLeft > 0);  
        
                    // Method 2: Start at 1 side and incrementally draw, then rotate; draw, then rotate, etc.
                    dir = Quaternion.Euler(0f, -1 * searchArc/2, 0f) * dir; // set direction to far left side
                    Gizmos.DrawLine(Vector3.zero, dir * length);
                    for(int i = 1; i < numOfRays; i++){
                        if(Hit){
                            length = Vector3.Distance(this.transform.position, info.point);
                        }
                        dir = Quaternion.Euler(0f, searchArc/(numOfRays-1), 0f) * dir;
                        Gizmos.DrawLine(Vector3.zero, dir * length);
                    }
                break;

                case Type.SphereCast:
                    Gizmos.DrawWireSphere(Vector3.zero, sphereRadius); // sphere around the agent
                    if(Hit) {
                        Vector3 ballCenter = info.point + info.normal * sphereRadius;
                        length = Vector3.Distance(cachedTransform.position, ballCenter);
                    }
                    Gizmos.DrawWireSphere(Vector3.zero + Vector3.forward * length, sphereRadius); // sphere around the end of the ray
                    // Draw connecting lines between the spheres
                    Gizmos.DrawLine(Vector3.up * sphereRadius, Vector3.forward * length + Vector3.up * sphereRadius);
                    Gizmos.DrawLine(Vector3.down * sphereRadius, Vector3.forward * length + Vector3.down * sphereRadius);
                    Gizmos.DrawLine(Vector3.left * sphereRadius, Vector3.forward * length + Vector3.left * sphereRadius);
                    Gizmos.DrawLine(Vector3.right * sphereRadius, Vector3.forward * length + Vector3.right * sphereRadius);
                break;

                case Type.DetectionBox:
                    Gizmos.DrawWireCube(Vector3.zero, boxExtents);
                break;

                case Type.DetectionSphere:
                    Gizmos.DrawWireSphere(Vector3.zero, sphereRadius);
                break;

                default:
                    break;
            }
        }
    }
}
