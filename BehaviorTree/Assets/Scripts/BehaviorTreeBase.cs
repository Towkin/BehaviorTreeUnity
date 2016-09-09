using UnityEngine;
using System.Collections.Generic;

public struct BehaviorState {
    public static readonly byte Success = 0;
    public static readonly byte Failure = 1;
    public static readonly byte Running = 2;
    public static readonly byte Error = 3;

    public byte Value;

    public static BehaviorState operator !(BehaviorState aOther) {
        if(aOther.Value == Success) {
            return new BehaviorState(Failure);
        } else if(aOther.Value == Failure) {
            return new BehaviorState(Success);
        }

        return new BehaviorState(aOther);
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public static bool operator ==(BehaviorState aThis, byte aOther) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(BehaviorState aThis, byte aOther) {
        return aThis.Value != aOther;
    }
    public static implicit operator BehaviorState(byte aOther) {
        return new BehaviorState(aOther);
    }

    public BehaviorState(byte aValue) {
        Value = aValue;
    }
    public BehaviorState(BehaviorState aOther) {
        Value = aOther.Value;
    }
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
    public abstract string NodeText {
        get;
    }
    public override string ToString() {
        return NodeText;
    }
    public abstract BehaviorState UpdateNode();
    
#if UNITY_EDITOR
    private Vector2 mNodeSize = new Vector2(160, 40);
    public Vector2 NodeSize {
        get { return mNodeSize; }
    }
    private Vector2 mChildNodeOffset = new Vector2(10, 20);
    protected Vector2 ChildNodeOffset {
        get { return mChildNodeOffset; }
    }
    protected virtual Color BoxColor {
        get { return Color.blue; }
    }
    protected virtual Texture BoxTexture {
        get {
            Texture2D ReturnTexture = new Texture2D(1, 1);

            ReturnTexture.wrapMode = TextureWrapMode.Repeat;
            ReturnTexture.SetPixel(0, 0, BoxColor);
            ReturnTexture.Apply();

            return ReturnTexture;
        }
    }
    /// <summary>
    /// Get the size of the node.
    /// </summary>
    /// <returns>Node size</returns>
    public virtual Vector2 GetRenderSize() {
        return NodeSize;
    }
    public virtual void RenderNode(Vector2 aFrom) {

        GUI.Box(new Rect(aFrom, NodeSize), NodeText);
    }
#endif
}

// --- Composites ---
public abstract class BTComposite : BTNode {
    private int mChildIndex = 0;
    public override string NodeText {
        get { return "Composite"; }
    }
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

#if UNITY_EDITOR
    public override Vector2 GetRenderSize() {
        Vector2 TotalRenderSize = new Vector2();

        foreach(BTNode ChildNode in ChildNodes) {
            Vector2 ChildRenderSize = ChildNode.GetRenderSize();
            TotalRenderSize.x += ChildRenderSize.x + ChildNodeOffset.x;
            TotalRenderSize.y = Mathf.Max(TotalRenderSize.y, ChildRenderSize.y + NodeSize.y + ChildNodeOffset.y);
        }
        TotalRenderSize.x -= ChildNodeOffset.x;

        return new Vector2(Mathf.Max(NodeSize.x, TotalRenderSize.x), Mathf.Max(NodeSize.y, TotalRenderSize.y));
    }
    public override void RenderNode(Vector2 aFrom) {
        GUI.Box(new Rect(aFrom - ChildNodeOffset / 2, GetRenderSize() + ChildNodeOffset), GUIContent.none);

        

        base.RenderNode(new Vector2(GetRenderSize().x / 2 - NodeSize.x / 2, 0) + aFrom);
        //UnityEditor.EditorGUI.DrawRect(new Rect(aFrom, NodeSize), Color.gray);

        float CoordXAdd = 0;

        foreach(BTNode ChildNode in ChildNodes) {
            ChildNode.RenderNode(aFrom + new Vector2(CoordXAdd, NodeSize.y + ChildNodeOffset.y));

            CoordXAdd += ChildNode.GetRenderSize().x + ChildNodeOffset.x;
        }
    }
#endif
}
public class BTComp_Sequence : BTComposite {
    public override string NodeText {
        get { return "Composite: Sequence"; }
    }
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
    public override string NodeText {
        get { return "Composite: Selector"; }
    }
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

                return BehaviorState.Success;
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
    private BTNode mNode = null;
    public BTNode Node {
        get { return mNode; }
    }
    
    protected BTDecorator(BTNode aDecoratedNode) {
        mNode = aDecoratedNode;
    }
#if UNITY_EDITOR
    public override Vector2 GetRenderSize() {
        Vector2 ChildNodeSize = Node.GetRenderSize();

        return new Vector2(Mathf.Max(NodeSize.x, ChildNodeSize.x), NodeSize.y + ChildNodeSize.y + ChildNodeOffset.y);
    }
    public override void RenderNode(Vector2 aFrom) {
        base.RenderNode(aFrom);

        Node.RenderNode(aFrom + new Vector2(0, NodeSize.y + ChildNodeOffset.y));
    }
#endif
}
public class BTDeco_Inverter : BTDecorator {
    public override string NodeText {
        get { return "Decorator: Inverter"; }
    }

    public BTDeco_Inverter(BTNode aDecoratedNode) : base(aDecoratedNode) { }

    public override BehaviorState UpdateNode() {
        if(Node == null) {
            return BehaviorState.Error;
        }

        BehaviorState ChildState = Node.UpdateNode();
        return !ChildState;
    }
}

// --- Leafs ---
public abstract class BTLeaf<T> : BTNode {
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
    public override string NodeText {
        get { return "Condition: In Distance\n" + TargetName + ", " + Distance.ToString(); }
    }

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
    public override string NodeText {
        get { return "Task: Move Towards\n" + TargetName + ", " + Speed.ToString(); }
    }

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
        Agent.transform.position += Offset.normalized * (Speed > 0 ? Mathf.Min(Offset.magnitude, Speed) : Speed);

        return BehaviorState.Success;
    }
}

