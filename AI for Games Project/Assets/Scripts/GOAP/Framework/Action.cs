using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class Action : MonoBehaviour
{
    public string actionName = "Action";
    public float cost = 1.0f;
    public GameObject target; // where will the action happen?
    public string targetTag; // used to make acquiring the target object easier
    public float duration = 0f; // how long will the action take?

    // Acquire conditions and effects through the inspector and add them to the corresponding dictionaries
    public WorldState[] preConditions;
    public WorldState[] afterEffects;
    public Dictionary<string, int> preconditions;
    public Dictionary<string, int> effects;

    public NavMeshAgent agent; // attached for navmesh navigation for now
    public WorldStates agentBeliefs; // what does the agent know?
    public bool running = false;

    public Action() // dynamic memory allocation in the constructor
    {
        preconditions = new Dictionary<string, int>();
        effects = new Dictionary<string, int>();
    }

    public void Awake()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();

        // fill the dictionaries when the action is instantiated
        if(preConditions != null){
            foreach(WorldState w in preConditions){
                preconditions.Add(w.key, w.value);
            }
        }

        if(afterEffects != null){
            foreach(WorldState w in afterEffects){
                effects.Add(w.key, w.value);
            }
        }
    }

    public bool IsAchievable()
    {
        // add conditions for returning false here at an action-by-action basis - under what conditions is this action impossible?
        return true;
    }
    public bool IsAchievableGiven(Dictionary<string, int> conditions)
    {
        foreach(KeyValuePair<string, int> p in conditions)
        {
            if(!conditions.ContainsKey(p.Key))
                return false;
        }
        return true; // all of the given conditions are in the action's preconditions, so the action is possible
    }

    // custom code that allows things to be done before and after this action
    public abstract bool PrePerform();
    public abstract bool PostPerform();

}
