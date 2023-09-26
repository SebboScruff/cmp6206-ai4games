using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidFlockManager : MonoBehaviour
{
    [SerializeField]bool showDebugThings = false;

    public Transform groundAnchor; // a grounded point that the flock moves around: centre of a village, the player, an island, etc...
    public int airSize = 50; // the dimensions of the virtual box within which the boids are spawned
    public int minHeight = 10;

    public int numOfUnits = 50;
    public GameObject[] units{get; private set;}
    public GameObject boidPrefab;

    public Transform flockTarget;
    MeshRenderer targetVis;


    void Awake()
    {
        units = new GameObject[numOfUnits];
    }
    // Start is called before the first frame update
    void Start()
    {
        targetVis = flockTarget.gameObject.GetComponent<MeshRenderer>();
        InvokeRepeating("MoveTargetRandom", 0f, 7.5f);
        //MoveTargetRandom();
        
        for(int i = 0; i < numOfUnits; i++){
            Vector3 spawnPos = new Vector3(Random.Range(groundAnchor.position.x - airSize, groundAnchor.position.x + airSize), // x
                                    Random.Range(groundAnchor.position.y + minHeight, groundAnchor.position.y + airSize), // y
                                    Random.Range(groundAnchor.position.z - airSize, groundAnchor.position.z + airSize)); // z

            Vector3 lookRotation = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

            units[i] = Instantiate(boidPrefab, spawnPos, Quaternion.LookRotation(lookRotation));
            units[i].transform.parent = this.transform;
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F8)){
            showDebugThings = !showDebugThings;
        }
        targetVis.enabled = showDebugThings;
    }

    void MoveTargetRandom()
    {
        flockTarget.position = new Vector3(Random.Range(groundAnchor.position.x - airSize, groundAnchor.position.x + airSize),
                            Random.Range(groundAnchor.position.y + minHeight, groundAnchor.position.y + airSize),
                            Random.Range(groundAnchor.position.z - airSize, groundAnchor.position.z + airSize));
    }

    void OnDrawGizmos()
    {
        if(showDebugThings){
            // Visualises the Boid Bounds
            Gizmos.color = Color.red;
            Vector3 spawnAreaCentre = groundAnchor.position + new Vector3(0, minHeight + (airSize-minHeight)/2, 0);
            Gizmos.DrawWireCube(spawnAreaCentre, new Vector3(airSize*2, (airSize - minHeight), airSize*2));

            // Visualises Minimum Height
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundAnchor.position, groundAnchor.position + new Vector3(0, minHeight, 0));
        }
    }     
}
