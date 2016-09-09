using UnityEngine;
using System.Collections.Generic;
using System;

public struct BehaviorState {
    public const byte Success = 0;
    public const byte Failure = 1;
    public const byte Running = 2;
    public const byte Error = 3;
    public static readonly string[] StateText = { "Success", "Failure", "Running", "Error" };
    
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

    public static bool operator ==(BehaviorState aThis, BehaviorState aOther) {
        return aThis.Value == aOther.Value;
    }
    public static bool operator !=(BehaviorState aThis, BehaviorState aOther) {
        return aThis.Value != aOther.Value;
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
            RootNode.RunNode();
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

    public BehaviorState RunNode() {
        BehaviorState NewState = UpdateNode();

#if UNITY_EDITOR
        LastBehaviorState = NewState;
#endif

        return NewState;
    }
    protected abstract BehaviorState UpdateNode();

#if UNITY_EDITOR
    private BehaviorState mLastBehaviorState = BehaviorState.Error;
    public BehaviorState LastBehaviorState {
        get { return mLastBehaviorState; }
        private set {
            LastBehaviorUpdateTime = Time.realtimeSinceStartup;
            mLastBehaviorState = value;
        }
    }
    private float mLastBehaviorUpdateTime = 0f;
    public float LastBehaviorUpdateTime {
        get { return mLastBehaviorUpdateTime; }
        private set { mLastBehaviorUpdateTime = value; }
    }

    private Vector2 mNodeSize = new Vector2(160, 40);
    public Vector2 NodeSize {
        get { return mNodeSize; }
    }
    private Vector2 mChildNodeOffset = new Vector2(10, 20);
    protected Vector2 ChildNodeOffset {
        get { return mChildNodeOffset; }
    }
    protected virtual Color BoxColor {
        get {
            Color ReturnColor;
            switch(LastBehaviorState.Value) {
                case BehaviorState.Success:
                    ReturnColor = Color.green;
                    break;
                case BehaviorState.Failure:
                    ReturnColor = Color.red;
                    break;
                case BehaviorState.Running:
                    ReturnColor = Color.yellow;
                    break;
                default: 
                    ReturnColor = Color.magenta;
                    break;
            }
            ReturnColor = Color.Lerp(ReturnColor, Color.gray, 1 - Mathf.Pow(0.15f, Time.realtimeSinceStartup - LastBehaviorUpdateTime));

            ReturnColor.a = 0.15f;
            return ReturnColor;
        }
    }
    protected virtual Texture2D BoxTexture {
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
        GUIStyle BoxStyle = new GUIStyle();
        BoxStyle.normal.background = BoxTexture;
        BoxStyle.padding = new RectOffset(8, 8, 8, 8);
        BoxStyle.overflow = new RectOffset(-1, -1, -1, -1);
        GUI.Box(new Rect(aFrom, NodeSize), NodeText, BoxStyle);
        
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
        GUIStyle BoxStyle = new GUIStyle();
        BoxStyle.normal.background = BoxTexture;
        BoxStyle.padding = new RectOffset(8, 8, 8, 8);
        BoxStyle.overflow = new RectOffset(-2, -2, -2, -2);
        GUI.Box(new Rect(aFrom - ChildNodeOffset / 2, GetRenderSize() + ChildNodeOffset), GUIContent.none, BoxStyle);

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
    protected override BehaviorState UpdateNode() {
        while(ChildIndex < ChildNodes.Count) {
            BehaviorState NodeState = ChildNodes[ChildIndex].RunNode();
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
    protected override BehaviorState UpdateNode() {
        while(ChildIndex < ChildNodes.Count) {
            BehaviorState NodeState = ChildNodes[ChildIndex].RunNode();

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
        ChildIndex = 0;

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

    protected override BehaviorState UpdateNode() {
        if(Node == null) {
            return BehaviorState.Error;
        }

        BehaviorState ChildState = Node.RunNode();
        return !ChildState;
    }
}
public class BTDeco_Succeeder : BTDecorator {
    public override string NodeText {
        get { return "Decorator: Succeeder"; }
    }

    public BTDeco_Succeeder(BTNode aDecoratedNode) : base(aDecoratedNode) { }

    protected override BehaviorState UpdateNode() {
        if(Node == null) {
            return BehaviorState.Error;
        }

        Node.RunNode();

        return BehaviorState.Success;
    }
}
public class BTDeco_Repeater : BTDecorator {
    public override string NodeText {
        get { return "Decorator: Repeater\n" + (RepeatOnCheckState? "RepeatState: " : "ReturnState: ") + BehaviorState.StateText[CheckState.Value] + ", " + (RepeatTimes < 0 ? "indefinately" : RepeatTimes.ToString()); }
    }

    private BehaviorState mCheckState = BehaviorState.Failure;
    public BehaviorState CheckState {
        get { return mCheckState; }
        set { mCheckState = value; }
    }
    private int mRepeatTimes = 1;
    public int RepeatTimes {
        get { return mRepeatTimes; }
        set { mRepeatTimes = value; }
    }
    private bool mRepeatOnCheckState = true;
    public bool RepeatOnCheckState {
        get { return mRepeatOnCheckState; }
        set { mRepeatOnCheckState = value; }
    }

    public BTDeco_Repeater(BTNode aDecoratedNode) : base(aDecoratedNode) { }
    /// <summary>
    /// Create a repeater decorator node. The child node will always run at least once.
    /// </summary>
    /// <param name="aDecoratedNode">The node to be repeated.</param>
    /// <param name="aCheckState">The state to check the decorated node against.</param>
    /// <param name="aRepeatTimes">Number of times to repeat. If less than 0, will repeat until state change.</param>
    /// <param name="aRepeatOnCheckState">Whether the repeater shall repeat, when the decorated node is in the 'aCheckState', or, when it's not.</param>
    public BTDeco_Repeater(BTNode aDecoratedNode, BehaviorState aCheckState, int aRepeatTimes = 1, bool aRepeatOnCheckState = true) : this(aDecoratedNode) {
        CheckState = aCheckState;
        RepeatTimes = aRepeatTimes;
        RepeatOnCheckState = aRepeatOnCheckState;
    }

    protected override BehaviorState UpdateNode() {
        if(Node == null) {
            return BehaviorState.Error;
        }

        BehaviorState ChildState;
        int RepeatCounter = 0;

        do {
            ChildState = Node.RunNode();
            RepeatCounter++;
        } while (
            (RepeatTimes < 0 || RepeatCounter < RepeatTimes) && 
            (
                (RepeatOnCheckState && ChildState == CheckState) || 
                (!RepeatOnCheckState && ChildState != CheckState)
            )
        );
        
        return ChildState;
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

    protected override BehaviorState UpdateNode() {
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

    protected override BehaviorState UpdateNode() {
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
public class BTTask_MoveTo<T> : BTTask<T> where T : MonoBehaviour {
    public override string NodeText {
        get { return "MoveTo"; }
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

    public BTTask_MoveTo(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
    public BTTask_MoveTo(BehaviorTree<T> aBehaviorTree, string aTargetName, float aSpeed) : this(aBehaviorTree) {
        TargetName = aTargetName;
        Speed = aSpeed;
    }

    protected override BehaviorState UpdateNode() {
        if (mTargetName == "" || Speed <= 0f) {
            return BehaviorState.Failure;
        }

        GameObject Target = (GameObject)Blackboard.Objects[TargetName];
        if (Target == null) {
            // TODO: Error?
            return BehaviorState.Failure;
        }

        Vector3 Offset = Target.transform.position - Agent.transform.position;
        Agent.transform.position += Offset.normalized * Mathf.Min(Offset.magnitude, Speed);

        return (Target.transform.position - Agent.transform.position).magnitude <= 0.01f ? BehaviorState.Success : BehaviorState.Running;
    }
}
