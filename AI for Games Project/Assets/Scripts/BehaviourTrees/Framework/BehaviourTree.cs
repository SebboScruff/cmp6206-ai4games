using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


// This class outlines the underlying behaviours of the tree as a whole
[CreateAssetMenu()]
public class BehaviourTree : ScriptableObject
{
    public BTNode rootNode;
    public BTNode.State treeState = BTNode.State.Running;

    public List<BTNode> nodes = new List<BTNode>();
    public BTNode.State Update()
    {
        // while in runtime, the behaviour tree simply executes the currently-active
        // node's OnUpdate() Method.
        if(rootNode.state == BTNode.State.Running){
            treeState = rootNode.Update();
        }
        return treeState;
    }

    // ** TREE MANAGEMENT METHODS **
    // For altering the tree, i.e. adding/removing entire nodes; altering node children, etc.
    public BTNode CreateNode(System.Type type)
    {
        BTNode node = ScriptableObject.CreateInstance(type) as BTNode;
        node.name = type.Name;
        #if UNITY_EDITOR
        node.guid = GUID.Generate().ToString();
        #endif

        #if UNITY_EDITOR
        Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
        #endif
        nodes.Add(node);

        if(!Application.isPlaying){
            #if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(node, this);
            #endif
        }   
        #if UNITY_EDITOR     
        Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
        #endif

        #if UNITY_EDITOR
        AssetDatabase.SaveAssets();
        #endif
        return node;
    }
    public void DeleteNode(BTNode node)
    {
        if(nodes.Contains(node)){
            #if UNITY_EDITOR
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            #endif
            nodes.Remove(node);

            //AssetDatabase.RemoveObjectFromAsset(node);
            #if UNITY_EDITOR
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
            #endif
        }
    }

    public void AddChild(BTNode parent, BTNode child)
    {
        DecoratorNode decorator = parent as DecoratorNode;
        if(decorator){
            #if UNITY_EDITOR
            Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
            #endif
            decorator.child = child;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(decorator);
            #endif
        }
        CompositeNode composite = parent as CompositeNode;
        if(composite){
            #if UNITY_EDITOR
            Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
            #endif
            composite.children.Add(child);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(composite);
            #endif
        }
        RootNode root = parent as RootNode;
        if(root){
            #if UNITY_EDITOR
            Undo.RecordObject(root, "Behaviour Tree (AddChild)");
            #endif
            root.child = child;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(root);
            #endif
        }
    }
    public void RemoveChild(BTNode parent, BTNode child)
    {
        DecoratorNode decorator = parent as DecoratorNode;
        if(decorator){
            #if UNITY_EDITOR
            Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
            #endif
            decorator.child = null;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(decorator);
            #endif
        }
        CompositeNode composite = parent as CompositeNode;
        if(composite){
            #if UNITY_EDITOR
            Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
            #endif
            composite.children.Remove(child);
            #if UNITY_EDITOR
            EditorUtility.SetDirty(composite);
            #endif
        }
        RootNode root = parent as RootNode;
        if(root){
            #if UNITY_EDITOR
            Undo.RecordObject(root, "Behaviour Tree (RemoveChild)");
            #endif
            root.child = null;
            #if UNITY_EDITOR
            EditorUtility.SetDirty(root);
            #endif
        }
    }
    public List<BTNode> GetChildren(BTNode parent)
    {
        List<BTNode> children = new List<BTNode>();

        DecoratorNode decorator = parent as DecoratorNode;
        if(decorator && decorator.child != null){
            children.Add(decorator.child);
        }
        CompositeNode composite = parent as CompositeNode;
        if(composite && composite.children != null){
            return composite.children;
        }
        RootNode root = parent as RootNode;
        if(root && root.child != null){
            children.Add(root.child);
        }

        return children;
    }

    public void Traverse(BTNode node, System.Action<BTNode> visitor)
    {
        if(node){
            visitor.Invoke(node);
            var children = GetChildren(node);
            children.ForEach((n) => Traverse(n, visitor));
        }
    }
    public BehaviourTree Clone()
    {
        BehaviourTree tree = Instantiate(this);
        tree.rootNode = tree.rootNode.Clone();
        tree.nodes = new List<BTNode>();
        Traverse(tree.rootNode, (n) => {
            tree.nodes.Add(n);
        });

        return tree;
    }

    public void Bind(GameObject _obj) // assign the parent object to each of the nodes in the tree
    {
        Traverse(rootNode, node => {
            node.parentObj = _obj;
        });
    }
}
