using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringAgent : MonoBehaviour
{
    [SerializeField] protected bool showDebugThings;

    [SerializeField] public Transform target;

    //[SerializeField] float mass = 10f;
    [SerializeField] protected float maxSpeed = 10f;
    private float actualSpeed; // for arrival behaviours
    [SerializeField] protected float maxForce = 5f;
    [SerializeField] protected float rotateSpeed = 2f;

    protected float minDistance = 2f; // how close should it seek/arrive/pursue to
    private float safeDistance = 10f; // how far away should it flee/evade to
    public Vector3 currentVelocity {get; protected set;}
    public Vector3 instantVelocity; // the frame-to-frame change in velocity

    public enum SteeringTypes{
        IDLE,   // slow to a stop
        PLAYER_CONTROLLED, // Player-Based WASD Movement, for debugging purposes
        SEEK,   // move towards the target
        FLEE,   // move away from the target
        ARRIVE, // Seek but slow down when near target 
        PURSUE, // move towards where the target will be
        EVADE,  // move away from where the target will be
        WANDER, // move randomly but generally forwards
        BOID,
        DEBUG_MANUAL
    }
    [SerializeField] public SteeringTypes steeringType;
    public LayerMask unwalkableMask;
    [Header("Arrive Parameters")]
    public float arriveSlowDistance = 7.5f;

    [Header("Pursue/Evade Parameters")]
    public float iterationsAhead = 30f;

    [Header("Wander Parameters")]
    public int iterationsToChange = 150;
    private float wanderMinDistance = 0.01f;
    private int currentIteration = 0;
    private Vector3 wanderPoint;
    public float wanderDistance = 5f;
    public float wanderRadius = 3f;
    public float wanderJitter = 1.7f;

    // Start is called before the first frame update
    void Start()
    {
        instantVelocity = Vector3.zero;
        currentVelocity = transform.forward;
        actualSpeed = maxSpeed;

        // make the wander point start somewhere other than (0,0,0)
        MoveWanderPoint(transform.position + transform.forward * wanderDistance);

        if(target == null){
            Debug.LogWarning(this.name + " does not have a target. Is this intended?");
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(Input.GetKeyDown(KeyCode.F9)){
            showDebugThings = !showDebugThings;
        }

        Vector3 pos = transform.position;
        Vector3 steeringForce = Vector3.zero;
        // Step 1: Select steering type
        // Step 2: Calculate steering force
        // Step 3: Update velocity
        // Step 4: Update position based on new velocity (Move)
        switch(this.steeringType){
            case SteeringTypes.IDLE:
                steeringForce = Vector3.ClampMagnitude(-currentVelocity, maxForce);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), maxSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.PLAYER_CONTROLLED:
                InputMove();
                break;
            case SteeringTypes.SEEK:
                steeringForce = Vector3.ClampMagnitude(SeekFlee(true, target.position, minDistance), maxForce);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), maxSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.FLEE:
                steeringForce = Vector3.ClampMagnitude(SeekFlee(false, target.position, minDistance), maxForce);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), maxSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.ARRIVE:
                steeringForce = Vector3.ClampMagnitude(Arrive(target.position, minDistance), maxForce);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), actualSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.PURSUE:
                steeringForce = PursueEvade(true);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), maxSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.EVADE:
                steeringForce = PursueEvade(false);
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), maxSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.WANDER:
                steeringForce = Wander();
                currentVelocity = Vector3.ClampMagnitude(currentVelocity + (steeringForce * Time.deltaTime), maxSpeed);
                transform.position += currentVelocity * Time.deltaTime;
                break;
            case SteeringTypes.DEBUG_MANUAL:
                InputSlide();
                break;
            default:
                break;
        }
        instantVelocity = transform.position - pos;
    }

    void InputMove()
    {
        float hMov = Input.GetAxis("Horizontal");
        float fMov = Input.GetAxis("Vertical");

        if(hMov != 0f){
            transform.eulerAngles += new Vector3(0f, rotateSpeed*Time.deltaTime*hMov, 0f);
        }
        if(fMov != 0f){
            transform.Translate(Vector3.forward * fMov * Time.deltaTime * maxSpeed, Space.Self);
        }
    }
    void InputSlide()
    {
        float hMov = Input.GetAxis("Horizontal");
        float fMov = Input.GetAxis("Vertical");

        if(hMov != 0f){
            transform.Translate(Vector3.right * hMov * Time.deltaTime * maxSpeed, Space.Self);
        }
        if(fMov != 0f){
            transform.Translate(Vector3.forward * fMov * Time.deltaTime * maxSpeed, Space.Self);
        }
    }

    // These functions all return a vector3 for steering the agent in the direction they want to head
    // Most of them lead back to this original Seek/Flee method as it is the most fundamental behaviour
    protected Vector3 SeekFlee(bool _isSeeking, Vector3 _target, float _minDist)
    {
        Vector3 desiredVelocity = Vector3.ClampMagnitude(_target - transform.position, maxSpeed);
        float distance = desiredVelocity.magnitude;
        if(steeringType != SteeringTypes.BOID){
            desiredVelocity.y = 0; // reset the y rotation to 0 unless it is a boid
        }
        // Do not add any more steering force if the agent is either close enough (seeking) or far enough away (fleeing)
        if(distance < _minDist && _isSeeking){
            return Vector3.zero;
        }
        else if(distance > safeDistance && !_isSeeking){
            return Vector3.zero;
        }

        Vector3 steering = desiredVelocity - currentVelocity;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredVelocity), rotateSpeed * Time.deltaTime);

        // return either the positive vector for seeking, or the negative vector for fleeing
        return _isSeeking ? steering : -steering;
    }
    Vector3 Arrive(Vector3 _target, float _minDist) // functionally very similar to Seek, but it slows down as it approaches the target
    {   
        Vector3 desiredVelocity = _target - transform.position;
        desiredVelocity.y = 0;
        float distance = desiredVelocity.magnitude;

        if(distance < _minDist){
            return Vector3.zero;
        }
        transform.rotation = transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredVelocity), rotateSpeed * Time.deltaTime);

        float speedFactor = 1;
        if(distance < arriveSlowDistance){
            speedFactor = distance/arriveSlowDistance;
        }
        actualSpeed = maxSpeed * speedFactor;
        Vector3 steering = desiredVelocity - currentVelocity;
        return steering;
    }
    Vector3 PursueEvade(bool _isSeeking)
    {
        float tPred = iterationsAhead; // constant k = iterationsAhead: at this point tPred = k
        float dist = (target.position - transform.position).magnitude;
        Vector3 targetsInstantVelocity = target.gameObject.GetComponent<SteeringAgent>().instantVelocity;
        Vector3 targetsCurrentVelocity = target.gameObject.GetComponent<SteeringAgent>().currentVelocity;

        // scale k by distance from agent to target
        // tPred *= dist;

        // or scale k by the tiem for the agent to reach the target
        // tPred *= dist / this.currentVelocity.magnitude;

        // or scale k by relative speeds, so if the target is moving really fast the agent will try to move faster
        tPred *= dist / (this.currentVelocity.magnitude + targetsCurrentVelocity.magnitude);

        if(targetsInstantVelocity == null){
            Debug.LogError("Could Not Find Steering Agent on Target!");
            return Vector3.zero;
        }
        Vector3 anticipatedPos = target.position + (targetsInstantVelocity * tPred);

        if(showDebugThings){
            Debug.DrawLine(transform.position, anticipatedPos, Color.red, 0);
        }
        
        return SeekFlee(_isSeeking, anticipatedPos, minDistance);
    }
    Vector3 Wander()
    {   
        Vector3 centre = transform.position + transform.forward * wanderDistance;

        // Move the point around once every few frames
        currentIteration++;
        if(currentIteration > iterationsToChange){
            MoveWanderPoint(centre);

            currentIteration = 0;
        }       

        if(showDebugThings){
            Debug.DrawLine(transform.position, wanderPoint, Color.red, 0);
        }

        return SeekFlee(true, wanderPoint, wanderMinDistance);
    }

    void MoveWanderPoint(Vector3 _centre)
    {
        wanderPoint.x += wanderJitter * RandomPositiveNegative();
        wanderPoint.x = Mathf.Clamp(wanderPoint.x, _centre.x - wanderRadius, _centre.x + wanderRadius);

        wanderPoint.y = transform.position.y;

        wanderPoint.z += wanderJitter * RandomPositiveNegative();
        wanderPoint.z = Mathf.Clamp(wanderPoint.z, _centre.z - wanderRadius, _centre.z + wanderRadius);

        // if this wander point is through an unwalkable object, make a half-turn
        // TODO Make this better
        if(Physics.Linecast(transform.position, wanderPoint, unwalkableMask)){
            transform.eulerAngles += new Vector3(0f, 180f, 0f);
        }
    }

    // simple function to randomly return either 1 or -1 for wandering
    int RandomPositiveNegative()
    {
        return Random.Range(0,2)* 2 - 1;
    }

    void OnDrawGizmosSelected()
    {
        if(showDebugThings){
            Gizmos.color = Color.red;
            if(showDebugThings){
                switch(this.steeringType){
                case SteeringTypes.IDLE:
                    break;
                case SteeringTypes.SEEK:
                    Gizmos.DrawWireSphere(transform.position, minDistance);
                    break;
                case SteeringTypes.FLEE:
                    Gizmos.DrawWireSphere(target.position, safeDistance);
                    break;
                case SteeringTypes.ARRIVE:
                    Gizmos.DrawWireSphere(transform.position, minDistance);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(target.position, arriveSlowDistance);
                    break;
                case SteeringTypes.PURSUE:
                    Gizmos.DrawWireSphere(transform.position, minDistance);
                    break;
                case SteeringTypes.EVADE:
                    Gizmos.DrawWireSphere(target.position, safeDistance);
                    break;
                case SteeringTypes.WANDER:
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireSphere(transform.position + transform.forward * wanderDistance, wanderRadius);
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(wanderPoint, 0.1f);
                    break;
                }
            }
        }
        
    }
}
