using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorReturnNode : ActionNode
{
    Sensor sensor;
    public SensorReturnNode()
    {
        this.description = "Uses the sensor class to return Success if something is sensed, or Failure is something is not sensed";
    }
    
    protected override void OnStart()
    {
        sensor = parentObj.GetComponent<Sensor>();
        if(sensor = null){
            sensor = parentObj.GetComponentInChildren<Sensor>();
        }
    }
    protected override void OnStop()
    {
	
    }
    protected override State OnUpdate()
    {
        if(sensor = null){
            Debug.LogError("No Sensor found on BT Agent " + parentObj.name);
            return State.Failure;
        }
        if(sensor.Hit){
            return State.Success;
        }
        return State.Failure;
    }
}
