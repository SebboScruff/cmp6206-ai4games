using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : SteeringAgent
{
    [Header("Boid Variables")]
    [Range(30, 360)]public float neighbourhoodAngle = 360f; // this doesnt do anything currently :)
    public Rigidbody rb{get; private set;}
    [SerializeField]float neighbourhoodRadius = 10f;
    private List<GameObject> neighbours = new List<GameObject>();
    Transform groundAnchor;
    Vector3 aboveGroundAnchor;
    [SerializeField]bool turning;
    [SerializeField]BoidFlockManager flockManager;
    public float speed {get; private set;}
    public float groupSpeed {get; private set;}

    Vector3 cohesion, separation, alignment;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        showDebugThings = false;

        flockManager = this.GetComponentInParent<BoidFlockManager>(); // all the boids are instantiated as children of the flock manager
        if(flockManager  == null){
            Debug.LogError("Couldn't find flock manager!");
        }
        // grab some useful stuff from the flock manager
        groundAnchor = flockManager.groundAnchor;
        
        this.steeringType = SteeringTypes.BOID; // so it doesn't accidentally get some other behaviours from being idle/seek/etc.

        speed = Random.Range(0.5f, 2f);
        groupSpeed = 0f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if(Input.GetKeyDown(KeyCode.F8)){
            showDebugThings = !showDebugThings;
        }

        target = flockManager.flockTarget;
        Vector3 pos = transform.position;

        // **START OF BOID BEHAVIOURS**

        // update the list of neighbours
        foreach(GameObject b in flockManager.units){
            if(b != this){
                float dist = Vector3.Distance(transform.position, b.transform.position);
                if(!neighbours.Contains(b) && dist < neighbourhoodRadius){
                    neighbours.Add(b);
                }
                else if(neighbours.Contains(b) && dist > neighbourhoodRadius){
                    neighbours.Remove(b);
                }
            }
        }

        // Bool switch to make sure boids dont go too far away from their ground anchor, 
        // or too close to the ground
        if(transform.position.x <= -(flockManager.airSize) || transform.position.x >= flockManager.airSize){
            turning = true;
        }
        else if(transform.position.y <= flockManager.minHeight || transform.position.y >= flockManager.airSize){
            turning = true;
        }
        else if(transform.position.z <= -(flockManager.airSize) || transform.position.z >= flockManager.airSize){
            turning = true;
        }
        else{
            turning = false;
        }

        if(turning){
            // set the orientation direction to be somewhere above the ground anchor
            Vector3 dir = (groundAnchor.transform.position + new Vector3(0, Random.Range(flockManager.minHeight, flockManager.airSize), 0)) - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotateSpeed*Time.deltaTime);
            speed = Random.Range(0.5f, 3.0f);
        }
        // 20% chance per frame for each boid to navigate, really cheap and pretty not good optimisation
        if(Random.Range(0, 5) == 0){

            // initialise cohesion to be this boid, and separation to be away from the flock's ground anchor
            cohesion = this.transform.position;
            separation = groundAnchor.position;

            groupSpeed = this.speed;

            // calculate steering forces
            foreach(GameObject neighbour in neighbours){
                Boid b = neighbour.GetComponent<Boid>(); // gonna need this for later

                cohesion += neighbour.transform.position;
                separation += (this.transform.position - neighbour.transform.position);

                groupSpeed += b.speed;
            }

            if(neighbours.Count != 0){
                // finalise the cohesion point, including the flock's goal
                cohesion /= neighbours.Count;
                cohesion += (target.position - this.transform.position);

                // set the speed to the neighbour's average speed, with a bit of variance, 
                // make sure the speed gets caught so it doesnt infinitely upscale
                speed = (groupSpeed / neighbours.Count) + Random.Range(-0.1f, 0.1f);
                if(speed > maxSpeed){
                    speed = Random.Range(0.5f, 3.0f);
                }
            }   

            // Set and apply alignment - this also sets up the boid so that a forward speed can be applied
            // to cover both cohesion and alignment
            Vector3 dir = (cohesion + separation) - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), rotateSpeed*Time.deltaTime);
        }
        // Move forwards after having the bearing altered
        transform.Translate(0, 0, speed*Time.deltaTime);

        // **END OF BOID BEHAVIOURS**

        instantVelocity = transform.position - pos;
    }

    void OnDrawGizmosSelected()
    {
        if(showDebugThings){
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, neighbourhoodRadius);

            Gizmos.color = Color.red;
            // connect this boid to each of its neighbours with a red line
            foreach(GameObject n in neighbours){                
                Gizmos.DrawLine(transform.position, n.transform.position);
            }
        }
    }
}
