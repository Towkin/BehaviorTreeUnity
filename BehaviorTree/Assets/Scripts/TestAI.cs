using UnityEngine;
using System.Collections;

public class TestAI : MonoBehaviour {

    [SerializeField]
    private GameObject mTargetObject;
    public GameObject TargetObject {
        get { return mTargetObject; }
        set { mTargetObject = value; }
    }
    [SerializeField]
    private GameObject mEnemyObject;
    public GameObject EnemyObject {
        get { return mEnemyObject; }
        set { mEnemyObject = value; }
    }

    private BehaviorTree<TestAI> mBehaviorTree;
    public BehaviorTree<TestAI> BehaviorTree {
        get { return mBehaviorTree; }
        protected set { mBehaviorTree = value; }
    }

    void Start () {
        BehaviorTree = new BehaviorTree<TestAI>(this);
        BTComp_Selector RootSelector = new BTComp_Selector();

        BTComp_Sequence FleeSequence = new BTComp_Sequence();
        FleeSequence.AddNode(new BTCond_InDistance<TestAI>(BehaviorTree, "Enemy", 15));
        FleeSequence.AddNode(new BTTask_MoveTowards<TestAI>(BehaviorTree, "Enemy", -0.05f));

        BTComp_Sequence TargetSequence = new BTComp_Sequence();
        TargetSequence.AddNode(new BTDeco_Inverter(new BTCond_InDistance<TestAI>(BehaviorTree, "Target", 1)));
        TargetSequence.AddNode(new BTTask_MoveTowards<TestAI>(BehaviorTree, "Target", 0.02f));

        RootSelector.AddNode(FleeSequence);
        RootSelector.AddNode(TargetSequence);

        BehaviorTree.Blackboard.Objects["Target"] = TargetObject;
        BehaviorTree.Blackboard.Objects["Enemy"] = EnemyObject;

        BehaviorTree.RootNode = RootSelector;
    }
	
	void Update () {
        BehaviorTree.UpdateTree();
	}
    void OnGUI() {
        BehaviorTree.RootNode.RenderNode(new Vector2(50, 50));
    }

    void OnDrawGizmos() {
        //BehaviorTree.RootNode.RenderNode(new Vector2(100, 300));
    }
}
