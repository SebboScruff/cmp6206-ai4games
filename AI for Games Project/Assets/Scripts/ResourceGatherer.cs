using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// criteria to control the behaviours for resource collection agents: Fisher, Miner, Lumberjack
// If they have Stamina AND Inventory Space, they will gather resources
// If they have no remaining inventory space, they will go back to the storehouse
// If they have no remaining stamina, they will go home and sleep

public class ResourceGatherer : MonoBehaviour
{
    bool showDebugThings = true;
    [SerializeField] GameObject debugWindow;
    [SerializeField] GameObject face;
    [SerializeField] Sensor sensor;
    public bool isScared {get; private set;}
    MeshRenderer bodyRenderer, faceRenderer;
    bool isOutside = true; // for managing whether or not the agent is visible
    bool isWorking, isResting, isDropping, isRelaxing;

    [SerializeField] TextMeshPro stateText, staminaText, inventoryText;

    public enum States{
        WORKING,                // when at the worksite
        RESTING,                // when at home
        DROPPING_OFF_MATERIALS, // when at the storehouse
        SCARED                  // when they detect the player and go AAH!
    }
    public States currentState;
    float distToStart = 5f;
    
    public float stamina;
    private float staminaMax = 100f;
    public float tirednessThreshold {get; private set;}
    public float workStaminaCost; // different jobs will have different stamina costs; e.g. mining > fishing
    
    private uint inventorySize = 100; // how many things this agent can carry  
    public uint inventoryRemaining; // how much space the agent has left; go to the storehouse when this below fullThreshold
    [SerializeField] public uint workInventoryCost; // likewise, mining will take more inventory space than fishing

    private PathFindUnit pfu;
    [SerializeField] Transform workWaypoint, homeWaypoint, storehouseWaypoint;
    [SerializeField] Storehouse storehouse;
    
    // Start is called before the first frame update
    void Start()
    {
        isScared = false;

        pfu = GetComponent<PathFindUnit>();
        pfu.StartCoroutine(pfu.UpdatePath());

        inventoryRemaining = inventorySize;
        stamina = staminaMax;
        tirednessThreshold = 5f;

        bodyRenderer = this.GetComponent<MeshRenderer>();
        faceRenderer = face.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {        
        if(Input.GetKeyDown(KeyCode.F6)){
            showDebugThings = !showDebugThings;
        }
        debugWindow.SetActive(showDebugThings);
        if(debugWindow.activeSelf){
            stateText.text = "Current State: " + currentState;
            staminaText.text = "Stamina: " + stamina.ToString("000");
            inventoryText.text = "Inventory Space: " + inventoryRemaining.ToString("000");
        }

        if(sensor.Hit == true && sensor.info.transform.tag == "Player"){
            isScared = true;
        }
        if(isScared){
            pfu.speed = 10f; // make em run faster if they're scared
        }
        else{
            pfu.speed = 5f;
        }

        switch(currentState){
            case States.WORKING:
                pfu.target = workWaypoint;
                StopCoroutine("Rest");
                StopCoroutine("DropOff");
                StopCoroutine("Relax");
                isOutside = true;
                isResting = false;
                isDropping = false;
                isRelaxing = false;

                if(!isWorking && Vector3.Distance(this.transform.position, pfu.target.position) < distToStart){
                    isWorking = true;
                    StartCoroutine("Work");
                }             

                break;
            case States.RESTING:
                pfu.target = homeWaypoint;
                StopCoroutine("Work");
                StopCoroutine("DropOff");
                StopCoroutine("Relax");
                if(!isResting && Vector3.Distance(this.transform.position, pfu.target.position) < distToStart){
                    isResting = true; 
                    StartCoroutine("Rest");
                }               
                break;
            case States.DROPPING_OFF_MATERIALS:
                pfu.target = storehouseWaypoint;
                StopCoroutine("Work");
                StopCoroutine("Rest");
                StopCoroutine("Relax");
                if(!isDropping && Vector3.Distance(this.transform.position, pfu.target.position) < distToStart){
                    isDropping = true;
                    StartCoroutine("DropOff"); 
                }                
                break;  
            case States.SCARED:
                pfu.target = homeWaypoint;  
                StopCoroutine("Work");
                StopCoroutine("Rest");
                StopCoroutine("DropOff");
                if(!isRelaxing && Vector3.Distance(this.transform.position, pfu.target.position) < distToStart){
                    isRelaxing = true;
                    StartCoroutine("Relax"); 
                }
                break;    
            default:
                break;
        }
        // Make the agent's renderers active whenver they are outside and vice versa
        bodyRenderer.enabled = isOutside;
        faceRenderer.enabled = isOutside;
    }

    IEnumerator Work()
    {
        // Maybe add an animation here
        
        while(true){
            stamina -= workStaminaCost;
            inventoryRemaining -= workInventoryCost;
            yield return new WaitForSeconds(1f);
        }
    }
    IEnumerator Rest()
    {
        // make the agent disappear for a while (goes into his house) and reset stamina
        isWorking = false;        
        isOutside = false;

        while(true){
            yield return new WaitForSeconds(5f);

            stamina = staminaMax;
              
            //yield break;         
        }
    }
    IEnumerator DropOff()
    {
        // todo animation or whatever
        isWorking = false;
        isOutside = false;

        while(true){
           yield return new WaitForSeconds(5);
 
            storehouse.IncreaseInventory(inventorySize - inventoryRemaining);
            inventoryRemaining = inventorySize;            

            yield break; 
        }       
    }
    IEnumerator Relax()
    {
        // make the agent disappear for a while (goes into his house) and calm down
        isWorking = false;       
        isOutside = false;

        while(true){
            yield return new WaitForSeconds(5f);

            isScared = false;
            
            yield break;         
        }     
    }
}
