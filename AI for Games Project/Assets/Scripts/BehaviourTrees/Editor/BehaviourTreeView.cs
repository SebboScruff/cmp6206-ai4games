using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

// This class manages the entire tree in the editor window
public class BehaviourTreeView : GraphView
{
    public Action<NodeView> OnNodeSelected;

    public new class UxmlFactory : UxmlFactory<BehaviourTreeView, GraphView.UxmlTraits> {}
    BehaviourTree tree;
    public BehaviourTreeView(){
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/BehaviourTrees/Editor/BehaviourTreeEditor.uss");
        styleSheets.Add(styleSheet);

        Insert(0, new GridBackground());
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        PopulateView(tree);
        AssetDatabase.SaveAssets();
    }

    NodeView FindNodeView(BTNode node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }

   internal void PopulateView(BehaviourTree tree)
    {
        this.tree = tree;

        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if(tree.rootNode == null){
            tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        // Creates nodes in the graph
        tree.nodes.ForEach(n => CreateNodeView(n));

        // creates the edges in the graph
        tree.nodes.ForEach(p => {
            var children = tree.GetChildren(p);
            children.ForEach(c => {
                NodeView parentView = FindNodeView(p);
                NodeView childView = FindNodeView(c);

                Edge edge = parentView.output.ConnectTo(childView.input);
                AddElement(edge);
            });
        });
    }

    // allows connections between nodes in the editor window
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return ports.ToList().Where(endPort => 
        endPort.direction != startPort.direction &&
        endPort.node != startPort.node).ToList();
    }

    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null){
            graphViewChange.elementsToRemove.ForEach(elem => {
                NodeView nodeView = elem as NodeView;
                if(nodeView != null){
                    tree.DeleteNode(nodeView.node);
                }

                Edge edge = elem as Edge;
                if(edge != null){
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.RemoveChild(parentView.node, childView.node);
                }
            });
        }

        if(graphViewChange.edgesToCreate != null){
            graphViewChange.edgesToCreate.ForEach(edge => {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;
                tree.AddChild(parentView.node, childView.node);
            });
        }

        if(graphViewChange.movedElements != null){
            nodes.ForEach((n) => {
                NodeView view = n as NodeView;
                view.SortChildren();
            });
        }
        return graphViewChange;
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        // This method manages the creation and display of the pop-up
        // menu created in the editor window by a Right-Click
        {
            var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
            foreach(var type in types){
                evt.menu.AppendAction($"[{type.BaseType.Name}]{type.Name}", (a) => CreateNode(type));
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach(var type in types){
                evt.menu.AppendAction($"[{type.BaseType.Name}]{type.Name}", (a) => CreateNode(type));
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
            foreach(var type in types){
                evt.menu.AppendAction($"[{type.BaseType.Name}]{type.Name}", (a) => CreateNode(type));
            }
        }
    }

    void CreateNode(System.Type type)
    {
        BTNode node = tree.CreateNode(type);
        CreateNodeView(node);
    }
    void CreateNodeView(BTNode node)
    {        
        NodeView nodeView = new NodeView(node);
        nodeView.OnNodeSelected = OnNodeSelected;
        AddElement(nodeView);
    }

    public void UpdateNodeStates(){
        nodes.ForEach(n => {
            NodeView view = n as NodeView;
            view.UpdateState(); 
        });
    }
}
