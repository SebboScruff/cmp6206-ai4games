using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// This class is the basic outline of each node: all of the lowest-level properties that apply to any node of any depth in the tree
public abstract class BTNode : ScriptableObject
{
    public enum State {
        Running,
        Success,
        Failure
    }

    [HideInInspector]public State state = State.Running; // initialise each one to running by default - it hasnt succeeded or failed yet
    [HideInInspector]public bool started = false; // Has this node ever executed?

    [HideInInspector]public string guid;

    [HideInInspector]public Vector2 position; // to keep the structure of the tree the same in the editor window after deselecting it
    [HideInInspector]public GameObject parentObj;
    [TextArea] public string description;

    public State Update()
    {
        if(!started){
            OnStart();
            started = true;
        }

        state = OnUpdate();

        if(state == State.Failure || state == State.Success){
            OnStop();
            started = false;
        }

        return state;
    }

    public virtual BTNode Clone(){
        return Instantiate(this);
    }

    // Framework behaviours for initialising, ending, and standard runtime
    protected abstract void OnStart();
    protected abstract void OnStop();
    protected abstract State OnUpdate();
}
