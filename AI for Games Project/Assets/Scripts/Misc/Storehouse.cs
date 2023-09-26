using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storehouse : MonoBehaviour
{
    public uint currentInventorySize {get; private set;}
    [SerializeField] Player player;
    [SerializeField] MeshRenderer mesh;
    [SerializeField] Material defaultMat, playerCloseMat;
    // [SerializeField] float guardRadius;
    
    // Start is called before the first frame update
    void Start()
    {
        currentInventorySize = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(transform.position, player.transform.position) < player.stealRadius){
            // Some Visual Effect to indicate that the player is close enough to use the storehouse
            mesh.material = playerCloseMat;
        }
        else{
            // Undo the above effect
            mesh.material = defaultMat;
        }
    }

    public void IncreaseInventory(uint _amountToAdd)
    {
        currentInventorySize += _amountToAdd;
        Debug.Log("Storehouse Inventory Increased by " + _amountToAdd + ": Storehouse Inventory is now " + currentInventorySize);
    }
    public void DecreaseInventory(uint _amountToRemove)
    {
        currentInventorySize -= _amountToRemove;
        Debug.Log("Storehouse Inventory Decreased by " + _amountToRemove + ": Storehouse Inventory is now " + currentInventorySize);
    }

    // void OnDrawGizmos(){
    //     Gizmos.color = Color.black;
    //     Gizmos.DrawWireSphere(transform.position, guardRadius);
    // }
}
