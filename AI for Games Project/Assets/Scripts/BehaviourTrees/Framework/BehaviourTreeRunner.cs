using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    public BehaviourTree tree;

    // Start is called before the first frame update
    void Start()
    {
        tree = tree.Clone();
        tree.Bind(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        // when in runtime, run the tree's update - which essentially
        // just takes the current active node and runs its OnUpdate() method.
        tree.Update();
    }
}
