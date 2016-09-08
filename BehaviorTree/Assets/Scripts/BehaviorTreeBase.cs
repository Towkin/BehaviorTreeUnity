using UnityEngine;
using System.Collections.Generic;

public enum BehaviorState {
    Success,
    Failure,
    Running,
    Error
}

public class BehaviorTree<T> {
    private BTNode mRootNode;
    public BTNode RootNode {
        get { return mRootNode; }
        set { mRootNode = value; }
    }
    private BTBlackboard mBlackboard = new BTBlackboard();
    public BTBlackboard Blackboard {
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

    private BehaviorTree() { }
    public BehaviorTree(T aAgent) {
        Agent = aAgent;
    }
    
    /// <summary>
    /// Updates the root node of the tree, and locks itself while running.
    /// </summary>
    public void UpdateTree() {
        if(!IsRunning && RootNode != null) {

            IsRunning = true;
            RootNode.UpdateNode();
            IsRunning = false;
        }
    }
}

public class BTBlackboard {
    private Dictionary<string, object> mObjects = new Dictionary<string, object>();
    public Dictionary<string, object> Objects {
        get { return mObjects; }
    }
}

public abstract class BTNode {
    protected string mName = "Node";
    public string Name {
        get { return mName; }
    }
    public override string ToString() {
        return Name;
    }
    public abstract BehaviorState UpdateNode();
}

// --- Composites ---
public abstract class BTComposite : BTNode {
    private int mChildIndex = 0;
    protected new string mName = "Composite";
    /// <summary>
    /// The current index in the Composite.
    /// </summary>
    public int ChildIndex {
        get { return mChildIndex; }
        protected set { mChildIndex = value; }
    }
    private List<BTNode> mChildNodes = new List<BTNode>();
    protected List<BTNode> ChildNodes {
        get { return mChildNodes; }
    }
    public void AddNode(BTNode aNewNode) {
        if(aNewNode != null) {
            ChildNodes.Add(aNewNode);
        }
    }
    public void RemoveNode(BTNode aNode) {
        ChildNodes.Remove(aNode);
    }
    public void RemoveNode(int aIndex) {
        if(aIndex < 0 || aIndex >= ChildNodes.Count) {
            return;
        }
        ChildNodes.RemoveAt(aIndex);
    }
}
public class BTComp_Sequence : BTComposite {
    protected new string mName = "Composite: Sequence";
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
            if(NodeState == BehaviorState.Error)
            {
                // TODO: Debug friendly error.

                return BehaviorState.Error;
            }

            // Iterate
            ChildIndex++;
        }

        // Reset Iterator
        ChildIndex = 0;

        return BehaviorState.Success;
    }
}
public class BTComp_Selector : BTComposite {
    protected new string mName = "Composite: Selector";
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
            if (NodeState == BehaviorState.Error)
            {
                // TODO: Debug friendly error.

                return BehaviorState.Error;
            }

            // Iterate
            ChildIndex++;
        }

        // Reset Iterator
        return BehaviorState.Failure;
    }
}

// --- Decorators ---
public abstract class BTDecorator : BTNode {
    protected new string mName = "Decorator";
    private BTNode mNode = null;
    public BTNode Node {
        get { return mNode; }
    }
    
    protected BTDecorator(BTNode aDecoratedNode) {
        mNode = aDecoratedNode;
    }
}
public class BTDeco_Inverter : BTDecorator {
    protected new string mName = "Decorator: Inverter";

    public BTDeco_Inverter(BTNode aDecoratedNode) : base(aDecoratedNode) { }

    public override BehaviorState UpdateNode() {
        if(Node == null) {
            return BehaviorState.Error;
        }

        BehaviorState ChildState = Node.UpdateNode();

        if(ChildState == BehaviorState.Running || ChildState == BehaviorState.Error) {
            return ChildState;
        }

        if(ChildState == BehaviorState.Success) {
            return BehaviorState.Failure;
        }
        return BehaviorState.Success;
    }
}

// --- Leafs ---
public abstract class BTLeaf<T> : BTNode {
    protected new string mName = "Leaf";
    private BehaviorTree<T> mBehaviorTree = null;
    protected BTBlackboard Blackboard {
        get { return mBehaviorTree.Blackboard; }
    }
    protected T Agent {
        get { return mBehaviorTree.Agent; }
    }

    // Default constructor uncallable.
    private BTLeaf() { }
    protected BTLeaf(BehaviorTree<T> aBehaviorTree) {
        mBehaviorTree = aBehaviorTree;
    }
}
public abstract class BTCondition<T> : BTLeaf<T> {
    protected BTCondition(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
}
public class BTCond_InDistance<T> : BTCondition<T> where T : MonoBehaviour {
    protected new string mName = "Condition: In Distance";

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

    public BTCond_InDistance(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
    public BTCond_InDistance(BehaviorTree<T> aBehaviorTree, string aTargetName, float aDistance) : this(aBehaviorTree) {
        TargetName = aTargetName;
        Distance = aDistance;
    }

    public override BehaviorState UpdateNode() {
        if(mTargetName == "" || Distance < 0) {
            return BehaviorState.Failure;
        }

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

public abstract class BTTask<T> : BTLeaf<T> {
    protected BTTask(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
}
public class BTTask_MoveTowards<T> : BTTask<T> where T : MonoBehaviour {
    protected new string mName = "Task: Move Towards";

    private string mTargetName = "";
    public string TargetName {
        get { return mTargetName; }
        set { mTargetName = value; }
    }
    private float mSpeed = 0;
    public float Speed {
        get { return mSpeed; }
        set { mSpeed = value; }
    }
    
    public BTTask_MoveTowards(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
    public BTTask_MoveTowards(BehaviorTree<T> aBehaviorTree, string aTargetName, float aSpeed) : this(aBehaviorTree) {
        TargetName = aTargetName;
        Speed = aSpeed;
    }

    public override BehaviorState UpdateNode() {
        if (mTargetName == "") {
            return BehaviorState.Failure;
        }

        GameObject Target = (GameObject)Blackboard.Objects[TargetName];
        if (Target == null) {
            // TODO: Error?
            return BehaviorState.Failure;
        }

        Vector3 Offset = Target.transform.position - Agent.transform.position;
        Agent.transform.position += Offset.normalized * Mathf.Min(Offset.magnitude, Speed);

        return BehaviorState.Success;
    }
}
