using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Guard : MonoBehaviour
{
    [SerializeField] bool showDebugThings;
    [SerializeField] GameObject debugWindow;
    [SerializeField] TextMeshPro stateText, atkCDText, isAggroTxt;
    SteeringAgent steering;
    [SerializeField] Sensor sensor;
    public Transform  storehouse, storehouseWaypoint, playerPos, guardhouseWaypoint;
    public Player player;
    public bool isAggro;
    public float aggroTimerMax{get; private set;}
    public float currentAggroTimer;

    [Header("Attack Variables")]
    [SerializeField] float damage;
    public float atkCooldownCurrent;
    public float atkCooldownMax{get; private set;}
    public float atkRange = 3f;
    // Start is called before the first frame update
    void Start()
    {
        steering = GetComponent<SteeringAgent>();

        aggroTimerMax = 5f;
        atkCooldownMax = 2f;
        atkCooldownCurrent = 0;

        playerPos = player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Debug Info Toggle
        if(Input.GetKeyDown(KeyCode.F6)){
            showDebugThings = !showDebugThings;
        }
        debugWindow.SetActive(showDebugThings);
        if(debugWindow.activeSelf){
            stateText.text = "Current State: " + steering.steeringType;
            atkCDText.text = "Attack Cooldown: " + atkCooldownCurrent.ToString("00.0");
            isAggroTxt.text = isAggro ? "Is Aggro" : " Is Not Aggro";
        }

        if(sensor.Hit ){
            if(sensor.info.transform.tag == "Player"){
                isAggro = true;
                currentAggroTimer = aggroTimerMax;
            }
            else if(sensor.info.transform.tag != "Agent"){
                // if we're here, it means the guard has seen something on either Default (Agent) or Unwalkable (Houses etc.) 
                transform.eulerAngles += new Vector3(0f, 180f, 0f); // spin around for now
            }
        }  

        // aggro timer:
        // set to max whenver player is detected
        // decrease whenver player is not detected
        if(currentAggroTimer > 0 && isAggro && !sensor.Hit){
            currentAggroTimer -= Time.deltaTime;
        }
        if(currentAggroTimer <= 0){
            isAggro = false;
        }

        if(isAggro){
            steering.target = playerPos;
        }
        else{
            steering.target = storehouseWaypoint;
        }
        
        // attack cooldown - when ready, cooldown should be 0
        if(atkCooldownCurrent > 0){
            atkCooldownCurrent -= Time.deltaTime;
        }

    }

    public void Attack()
    {
        // do some damage to, or just kill the player, or something
        player.TakeDamage(damage);

        atkCooldownCurrent = atkCooldownMax;
    }

    void OnDrawGizmos()
    {
        if(showDebugThings){
            // Visualise area around storehouse
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(storehouse.position, 27f);

            // Visualise attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, atkRange);
        }
    }
}
