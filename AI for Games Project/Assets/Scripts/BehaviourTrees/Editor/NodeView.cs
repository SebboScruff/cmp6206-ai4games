using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

// This class is for managing the physical appearance and properties of the nodes in the editor window
public class NodeView : UnityEditor.Experimental.GraphView.Node
{
    public Action<NodeView> OnNodeSelected;

    public BTNode node;

    public Port input;
    public Port output;

    public NodeView(BTNode node) : base("Assets/Scripts/BehaviourTrees/Editor/NodeView.uxml"){
        this.node = node;
        this.title = node.name;
        this.viewDataKey = node.guid;

        style.left = node.position.x;
        style.top = node.position.y;

        CreateInputPorts();
        CreateOutputPorts();

        SetupClasses();

        Label descriptionLabel = this.Q<Label>("description");
        descriptionLabel.bindingPath = "description";
        descriptionLabel.Bind(new SerializedObject(node));
    }

    private void SetupClasses()
    {
        if(node is ActionNode){
            AddToClassList("action");
        } 
        else if(node is CompositeNode){
            AddToClassList("composite");
        }
        else if(node is DecoratorNode){
            AddToClassList("decorator");
        }
        else if(node is RootNode){
            AddToClassList("root");
        }
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        Undo.RecordObject(node, "Behaviour Tree (Set Position)");
        node.position.x = newPos.xMin;
        node.position.y = newPos.yMin;
        EditorUtility.SetDirty(node);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        if(OnNodeSelected != null){
            OnNodeSelected.Invoke(this);
        }
    }

    public void CreateInputPorts()
    {
        // root nodes have no inputs since they are the start of the behaviour tree
        if(node is RootNode){
            return;
        }

        // each node can only ever have 1 input (parent node)
        input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
        if(input != null){
            input.portName = ""; // remove the name so it doesn't just say 'boolean'
            input.style.flexDirection = FlexDirection.Column;
            inputContainer.Add(input);
        }
    }
    public void CreateOutputPorts()
    {
        if(node is ActionNode){
            // actions have no outputs
        } 
        else if(node is CompositeNode){
            // composites have any number of outputs
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
        }
        else if(node is DecoratorNode){
            // decorators have exactly 1 output
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
        }
        else if(node is RootNode){
            // decorators have exactly 1 output
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
        }

        if(output != null){
            output.portName = "";
            output.style.flexDirection = FlexDirection.ColumnReverse;
            outputContainer.Add(output);
        }
    }

    public void SortChildren() // organise the children of a Comp Node so they always execute left-to-right
    {
        CompositeNode composite = node as CompositeNode; // no point organising the children if there's fewer than 2
        if(composite){
            composite.children.Sort(SortByHorizontalPosition);
        }
    }
    private int SortByHorizontalPosition(BTNode left, BTNode right)
    {
        return left.position.x < right.position.x ? -1 : 1;
    }

    public void UpdateState()
    {
        RemoveFromClassList("running");
        RemoveFromClassList("failure");
        RemoveFromClassList("success");

        if(Application.isPlaying){
           switch(node.state){
            case BTNode.State.Running:
                if(node.started){
                    AddToClassList("running");
                }
                break;
            case BTNode.State.Failure:
                AddToClassList("failure");
                break;   
            case BTNode.State.Success:
                AddToClassList("success");
                break;   
            } 
        }        
    }
}
