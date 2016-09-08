using UnityEngine;
using System.Collections.Generic;
using System;

public enum BehaviorState {
    Success,
    Failure,
    Running
}

public class BehaviorTree<T> {
    private BehaviorNode mRootNode;
    protected BehaviorNode RootNode {
        get { return mRootNode; }
        set { mRootNode = value; }
    }
    private BehaviorBlackboard mBlackboard;
    public BehaviorBlackboard Blackboard {
        get { return mBlackboard; }
    }

    private T mAgent;
    public T Agent {
        get { return mAgent; }
        protected set { mAgent = value; }
    }

    private bool mIsRunning = false;
    public bool IsRunning {
        get { return mIsRunning; }
        private set { mIsRunning = value; }
    }

    /// <summary>
    /// Updates the root node of the tree.
    /// </summary>
    public void UpdateTree() {
        if(!IsRunning && RootNode != null) {

            IsRunning = true;
            RootNode.UpdateNode();
            IsRunning = false;
        }
    }
}

public class BehaviorBlackboard {
    private Dictionary<string, object> mObjects = new Dictionary<string, object>();
    public Dictionary<string, object> Objects {
        get { return mObjects; }
    }
    
    //private Dictionary<string, Transform> mTransforms = new Dictionary<string, Transform>();
    //public Transform GetTransformData(string aName) {
    //    try {
    //        return mTransforms[aName];
    //    } catch(KeyNotFoundException E) {
    //        Debug.LogException(E);
    //        return null;
    //    }
    //}
    //public void SetTransformData(string aName, Transform aTransform) {
    //    mTransforms[aName] = aTransform;
    //}
    //private Dictionary<string, Vector3> mVector3s = new Dictionary<string, Vector3>();
    //private Dictionary<string, float> mFloats = new Dictionary<string, float>();
    //private Dictionary<string, int> mInts = new Dictionary<string, int>();
}

public abstract class BehaviorNode {
    protected string mName = "Node";
    public string Name {
        get { return mName; }
    }
    public abstract BehaviorState UpdateNode();
}

// --- Composites ---
public abstract class BehaviorComposite : BehaviorNode {
    private int mChildIndex = 0;
    protected new string mName = "Composite";
    /// <summary>
    /// The current index in the Composite.
    /// </summary>
    public int ChildIndex {
        get { return mChildIndex; }
        protected set { mChildIndex = value; }
    }
    private List<BehaviorNode> mChildNodes = new List<BehaviorNode>();
    protected List<BehaviorNode> ChildNodes {
        get { return mChildNodes; }
    }
    public void AddNode(BehaviorNode aNewNode) {
        if(aNewNode != null) {
            ChildNodes.Add(aNewNode);
        }
    }
    public void RemoveNode(BehaviorNode aNode) {
        ChildNodes.Remove(aNode);
    }
    public void RemoveNode(int aIndex) {
        if(aIndex < 0 || aIndex >= ChildNodes.Count) {
            return;
        }
        ChildNodes.RemoveAt(aIndex);
    }
}
public class BehaviorSequence : BehaviorComposite {
    protected new string mName = "Sequence";
    /// <summary>
    /// Iterates through child nodes. 
    /// Break execution on Failure, pauses execution on Running, return Success if all children succeed.
    /// </summary>
    /// <returns>Node State</returns>
    public override BehaviorState UpdateNode() {
        while(ChildIndex < ChildNodes.Count) {
            BehaviorState NodeState = ChildNodes[ChildIndex].UpdateNode();
            if(NodeState == BehaviorState.Failure) {
                // Reset Iterator
                ChildIndex = 0;

                return BehaviorState.Failure;
            }
            if(NodeState == BehaviorState.Running) {
                // Keep Iterator
                
                return BehaviorState.Running;
            }

            // Iterate
            ChildIndex++;
        }

        // Reset Iterator
        ChildIndex = 0;

        return BehaviorState.Success;
    }
}
public class BehaviorSelector : BehaviorComposite {
    protected new string mName = "Selector";
    /// <summary>
    /// Iterates through child nodes.
    /// Breaks exectution on Success, pauses execution on Running, return Failure if all children fail.
    /// </summary>
    /// <returns>Node State</returns>
    public override BehaviorState UpdateNode() {
        while(ChildIndex < ChildNodes.Count) {
            BehaviorState NodeState = ChildNodes[ChildIndex].UpdateNode();

            if(NodeState == BehaviorState.Success) {
                // Reset Iterator
                ChildIndex = 0;
            }
            if(NodeState == BehaviorState.Running) {
                // Keep Iterator

                return BehaviorState.Running;
            }

            // Iterate
            ChildIndex++;
        }

        // Reset Iterator
        return BehaviorState.Failure;
    }
}

// --- Decorators ---
public abstract class BehaviorDecorator : BehaviorNode {
    protected new string mName = "Decorator";
    private BehaviorNode mNode;
    public BehaviorNode Node {
        get { return mNode; }
    }

    protected BehaviorDecorator(BehaviorNode aDecoratedNode) {
        mNode = aDecoratedNode;
    }
}

// --- Leafs ---
public abstract class BehaviorLeaf<T> : BehaviorNode {
    protected new string mName = "Leaf";
    private BehaviorTree<T> mBehaviorTree = null;
    protected BehaviorBlackboard Blackboard {
        get { return mBehaviorTree.Blackboard; }
    }
    protected T Agent {
        get { return mBehaviorTree.Agent; }
    }
}

public class BTConditional_InDistance : BehaviorLeaf<GameObject> {

    private string mTargetName = "";
    public string TargetName {
        get { return mTargetName; }
        set { mTargetName = value; }
    }
    private float mDistance = 0;
    public float Distance {
        get { return mDistance; }
        set { mDistance = value; }
    }

    public override BehaviorState UpdateNode() {
        GameObject Target = (GameObject)Blackboard.Objects[TargetName];
        if(Target == null) {
            // TODO: Error?
            return BehaviorState.Failure;
        }
        
        if((Target.transform.position - Agent.transform.position).magnitude > Distance) {
            return BehaviorState.Failure;
        }
        return BehaviorState.Success;
    }
}
