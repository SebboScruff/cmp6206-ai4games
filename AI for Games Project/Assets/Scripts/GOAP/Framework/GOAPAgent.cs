using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SubGoal
{
    public Dictionary<string, int> sgoals;
    public bool remove;
    public SubGoal(string s, int i, bool r)
    {
        sgoals = new Dictionary<string, int>();
        sgoals.Add(s, i);

        remove = r;
    }
}

public class GOAPAgent : MonoBehaviour
{
    public List<Action> actions = new List<Action>();
    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();
    Planner planner; // return a queue of actions
    Queue<Action> actionQueue;

    public Action currentAction;
    SubGoal currentGoal;

    // Start is called before the first frame update
    public void Start()
    {
        Action[] acts = this.GetComponents<Action>(); // drag actions onto the agent from the inspector
        foreach(Action a in acts){
            actions.Add(a);
        }         
    }
    bool invoked = false;

    void CompleteAction() // generic framework for after the agent has finished their current action
    {
        currentAction.running = false;
        currentAction.PostPerform();
        invoked = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Implement the planner:
        // - Get a list of goals that need to be satisfied
        // - Get a list of actions that can be performed
        // - Get a list current world states
        // - Build a tree of actions
        // - Undertake a graph search to find the path of actions that leads to the goal

        // Priority 1: Undertake the current action
        if(currentAction != null && currentAction.running){ // a) Is there a current action and is it running?
            if(currentAction.agent.hasPath && currentAction.agent.remainingDistance < 1f){ // b) is the agent where it needs to be?
                if(!invoked){ // c) has this action already been done?
                    Invoke("CompleteAction", currentAction.duration);
                    invoked = true;
                }
            }
            return; // break out of update here if there are still actions to be done
        }

        // Priority 2: Create a new plan
        if(planner == null || actionQueue == null){
            planner = new Planner();
            var sortedGoals = from entry in goals orderby entry.Value descending select entry;
            foreach(KeyValuePair<SubGoal, int> sg in sortedGoals){
                actionQueue = planner.Plan(actions, sg.Key.sgoals, null);
                if(actionQueue != null){
                    currentGoal = sg.Key;
                    break; // exit out of update after creating a new plan
                }
            }
        }

        // Priority 3: Destroy the planner if the agent has done every action
        if(actionQueue != null && actionQueue.Count == 0){
            if(currentGoal.remove){
                goals.Remove(currentGoal);
            }
            planner = null;
        }

        // Priority 4: Set a new action
        if(actionQueue != null && actionQueue.Count > 0){
            currentAction = actionQueue.Dequeue();
            if(currentAction.PrePerform()){ // check that all of the pre-performance criteria are met
                if(currentAction.target == null && currentAction.targetTag != ""){ // find a target if there isn't one already
                    currentAction.target = GameObject.FindGameObjectWithTag(currentAction.targetTag);
                }
                if(currentAction.target != null){ // start the action, go go go
                    currentAction.running = true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position);
                }
            }
            else{ // if pre-performance criteria are not met
                actionQueue = null;
            }
        }
    }
}
