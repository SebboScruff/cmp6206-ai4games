using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*  Player Script, for all the player behaviours
    Not much else to say upfront :/
*/

public class Player : SteeringAgent // player inherits from Steering Agents so that the Steering Agent Guards can access its velocities
{
    [Header("Player Variables")]
    public float health;
    float maxHealth = 100f;
    public float stealRadius {get; private set;}
    uint currentlyCarrying; // how much of the inventory is filled?
    uint inventorySpaceMax = 100;
    uint inventorySpaceLeft;
    public TextMeshProUGUI inventoryText, healthText;
    [SerializeField] Transform spawnLocation; // Set a location for the player to spawn (and respawn) in-client
    [SerializeField] CameraTargetControls camControls;
    [SerializeField] Storehouse villageSH, goblinSH; 

    void Awake()
    {
        this.transform.position = spawnLocation.position;
        this.steeringType = SteeringTypes.PLAYER_CONTROLLED;
        Cursor.lockState = CursorLockMode.Locked;
    }
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        currentlyCarrying = 0;
        stealRadius = 8f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        inventorySpaceLeft = inventorySpaceMax - currentlyCarrying;
        inventoryText.text = "Currently Carrying: " + currentlyCarrying.ToString("000") + "/" + inventorySpaceMax.ToString("000");
        healthText.text = "Current Health: " + health.ToString("000") + "/" + maxHealth.ToString("000");
        // player inputs have no effect if the camera is not currently on them
        if(camControls.isTrackingPlayer){
            base.Update(); // manage movement, setting instant velocities, etc. as per the base level steering agent
            this.currentVelocity = this.instantVelocity * maxSpeed;
        }
        if(Input.GetKeyDown(KeyCode.Space) && Vector3.Distance(transform.position, villageSH.transform.position) < stealRadius){
            UseStorehouse(villageSH);
        }
        if(Input.GetKeyDown(KeyCode.Space) && Vector3.Distance(transform.position, goblinSH.transform.position) < stealRadius){
            UseStorehouse(goblinSH);
        }
    }

    public void TakeDamage(float _dmg)
    {
        health -= _dmg;
        if(health <= 0){
            Die();
        }
    }

    void Die()
    {
        this.transform.position = spawnLocation.position;
        this.health = maxHealth;
        currentlyCarrying = 0;
    }

    void UseStorehouse(Storehouse _storehouse)
    {
        if(_storehouse == villageSH){
            // Steal
            uint maxStealAmount = (uint)villageSH.currentInventorySize/2;
            uint amountStolen = maxStealAmount;

            if(maxStealAmount > inventorySpaceLeft){
                amountStolen = inventorySpaceLeft;
            }

            villageSH.DecreaseInventory(amountStolen);
            currentlyCarrying += amountStolen;
        }
        else if(_storehouse == goblinSH){
            // Deposit
            goblinSH.IncreaseInventory(currentlyCarrying);
            currentlyCarrying= 0;
            if(health < maxHealth){
                health += Mathf.Min(maxHealth-health, 20f);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stealRadius);
    }
}
