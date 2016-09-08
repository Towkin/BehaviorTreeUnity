using UnityEngine;
using System.Collections;

public class TestAI : MonoBehaviour {

    [SerializeField]
    private GameObject mTargetObject;
    public GameObject TargetObject {
        get { return mTargetObject; }
        set { mTargetObject = value; }
    }

    private BehaviorTree<TestAI> mBehaviorTree;
    public BehaviorTree<TestAI> BehaviorTree {
        get { return mBehaviorTree; }
        protected set { mBehaviorTree = value; }
    }

    void Start () {
        BehaviorTree = new BehaviorTree<TestAI>(this);

        BTComp_Sequence Sequence = new BTComp_Sequence();
        Sequence.AddNode(new BTDeco_Inverter(new BTCond_InDistance<TestAI>(BehaviorTree, "Target", 1)));
        Sequence.AddNode(new BTTask_MoveTowards<TestAI>(BehaviorTree, "Target", 0.01f));

        BehaviorTree.Blackboard.Objects["Target"] = mTargetObject;

        BehaviorTree.RootNode = Sequence;
    }
	
	void Update () {
        BehaviorTree.UpdateTree();
	}
}
