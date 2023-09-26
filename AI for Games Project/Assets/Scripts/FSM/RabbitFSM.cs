using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*  A simple 2-state FSM to manage the
    behaviours of rabbits. They will
    alternate between wandering randomly
    and fleeing 
*/

public class RabbitFSM : MonoBehaviour
{

    public bool showDebugThings = false;
    StateManager sm = new StateManager();

    [SerializeField] Sensor generalSensor;
    [SerializeField] Sensor obstacleAvoidSensor; // raycast sensor on a separate game object to stop the rabbit from walking into stuff
    //[SerializeField] float obstacleTurnSpd = 5f;
    [SerializeField] bool isScared = false;
    [SerializeField] float scaredTimer = 0.0f;

    public GameObject currentGoal;

    public GameObject debugWindow;
    public TextMeshPro stateText;

    // Start is called before the first frame update
    void Start()
    {
        generalSensor = GetComponent<Sensor>();

        sm.ChangeState(new WanderState(this)); 
    }

    // Update is called once per frame
    void Update()
    {
        sm.Update();

        if(scaredTimer > 0){
            scaredTimer -= Time.deltaTime;
        }

        if(Input.GetKeyDown(KeyCode.F6)){
            showDebugThings = !showDebugThings;
        }
        debugWindow.SetActive(showDebugThings);

        //State Transitions:
        // If the rabbit's general sensor is triggered, it will fleeworkinve
        if(generalSensor.Hit == true){
            //Debug.Log("Rabbit is scared");
            isScared = true;
            scaredTimer = 5.0f;
            if(generalSensor.detectedObjects.Count > 0){
                if(generalSensor.detectedObjects[0].tag != "Rabbit"){
                    sm.ChangeState(new FleeingState(this, generalSensor.detectedObjects[0].transform));
                    //Debug.Log("Fleeing from " + generalSensor.detectedObjects[0].name); 
                }
            }   
            else if(generalSensor.info.transform){
                if(generalSensor.info.transform.tag != "Rabbit"){
                    sm.ChangeState(new FleeingState(this, generalSensor.info.transform)); 
                    //Debug.Log("Fleeing from " + generalSensor.info.transform.name);  
                }
            }            
        }

        // When the scared timer runs out, return to standard movement behaviour
        if(scaredTimer <=0 && isScared){
            //Debug.Log("Rabbit is not longer scared");
            isScared = false;
            sm.ChangeState(new WanderState(this));            
        }      
    }
}