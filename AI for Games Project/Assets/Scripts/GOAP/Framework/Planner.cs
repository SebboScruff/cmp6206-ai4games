using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPNode // helper class to organise the action tree
{
    public GOAPNode parent;
    public float cost;
    public Dictionary<string, int> state;
    public Action action;

    public GOAPNode(GOAPNode _parent, float _cost, Dictionary<string, int> _allstates, Action _action)
    {
        this.parent = _parent;
        this.cost = _cost;
        this.state = new Dictionary<string, int>(_allstates);
        this.action = _action;
    }
}

public class Planner : MonoBehaviour
{
    // Function to return an ordered sequence of actions to achieve an incoming goal
    public Queue<Action> Plan(List<Action> _actions, Dictionary<string, int> _goal, WorldStates _states)
    {
        List<Action> usableActions = new List<Action>();
        foreach(Action a in _actions){
            if(a.IsAchievable()){
                usableActions.Add(a);
            }
        }
        List<GOAPNode> leaves = new List<GOAPNode>();
        GOAPNode start = new GOAPNode(null, 0, World.Instance.GetWorld().GetStates(), null);

        bool success = BuildGraph(start, leaves, usableActions, _goal);
        if(!success){
            Debug.Log("No action plan found, oopsie");
            return null;
        }

        // A path has now been found - now the agent wants to find the cheapest node
        GOAPNode cheapest = null;
        foreach(GOAPNode leaf in leaves){

            // Find the cheapest leaf first
            if(cheapest == null){
                cheapest = leaf;
            }
            else{
                if(leaf.cost < cheapest.cost){
                    cheapest = leaf;
                }
            }
            
            // Create and fill a list of actions
            List<Action> result = new List<Action>();
            GOAPNode n = cheapest;
            while( n!= null){
                if(n.action != null){
                    result.Insert(0, n.action);
                }
                n = n.parent; // change n to 'climb up' the tree - eventually n.parent == null and then the while loop will stop
            }

            // Convert the action list into a queue that can be returned
            Queue<Action> queue = new Queue<Action>();
            foreach(Action a in result){
                queue.Enqueue(a);
            }

            // Visualise in console:
            Debug.Log("The Plan is: ");
            foreach(Action a in queue){
                Debug.Log("Q: " + a.actionName);
            }
            return queue;
        }
        // If there are no leaves, an action queue cannot be returned
        return null;
    }

    // Function to construct a graph of actions
    private bool BuildGraph(GOAPNode _parent, List<GOAPNode> _leaves, List<Action> _usableActions, Dictionary<string, int> _goal)
    {   
        bool foundPath = false;

        foreach(Action a in _usableActions){
            if(a.IsAchievableGiven(_parent.state)){
                Dictionary<string, int> currentState = new Dictionary<string, int>(_parent.state);

                foreach(KeyValuePair<string, int> eff in a.effects){
                    if(!currentState.ContainsKey(eff.Key)){
                        currentState.Add(eff.Key, eff.Value);
                    }
                }

                GOAPNode node = new GOAPNode(_parent, _parent.cost + a.cost, currentState, a);
                if(GoalAchieved(_goal, currentState)){
                    _leaves.Add(node);
                    foundPath = true;
                }
                else{
                    List<Action> subset = ActionSubset(_usableActions, a);
                    bool found = BuildGraph(node, _leaves, subset, _goal);
                    if(found){
                        foundPath = true;
                    }
                }                
            }
        }

        return foundPath;
    }

    private List<Action> ActionSubset(List<Action> _actions, Action _removeMe)
    {
        List<Action> subset = new List<Action>();

        foreach(Action a in _actions){
            if(!a.Equals(_removeMe)){
                subset.Add(a);
            }
        }
        return subset;
    }

    private bool GoalAchieved(Dictionary<string, int> _goal, Dictionary<string, int> _state)
    {
        // if the current state does not contain the goal, then return false
        foreach(KeyValuePair<string, int> g in _goal){
            if(!_state.ContainsKey(g.Key)){
                return false;
            }
        }
        return true;
    }
}
