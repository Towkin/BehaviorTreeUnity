using UnityEngine;
using System.Collections;

public class AISurvivalPawnController : SurvivalPawnController<SurvivalPawn> {

    BehaviorTree<SurvivalPawnController<SurvivalPawn>> mAI;

    [SerializeField]
    private GameObject mTarget;

	void Start () {
        mAI = new BehaviorTree<SurvivalPawnController<SurvivalPawn>>(this as SurvivalPawnController<SurvivalPawn>);

        BTNode HealthNode = new BTCond_HealthThreshold<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, 10f);

        BTNode SeeNode = new BTCond_CanSee<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mTarget);
        BTNode AimNode = new BTTask_AimAt<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mTarget.transform, 0.5f, 180);

        BTNode AttackRangeNode = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, mTarget, ControlledPawns[0].AttackConeRadius);
        BTNode AttackNode = new BTTask_Attack<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, 0.2f);

        BTComp_Sequence InRangeAttackSequence = new BTComp_Sequence();
        InRangeAttackSequence.AddNode(AttackRangeNode);
        InRangeAttackSequence.AddNode(AttackNode);

        BTNode MoveNode = new BTTask_MoveTowards<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mTarget.transform);

        
        BTComp_Selector AttackOrMoveSelector = new BTComp_Selector();
        AttackOrMoveSelector.AddNode(InRangeAttackSequence);
        AttackOrMoveSelector.AddNode(MoveNode);
        
        BTComp_Sequence AimActSequence = new BTComp_Sequence();
        AimActSequence.AddNode(SeeNode);
        AimActSequence.AddNode(AimNode);
        AimActSequence.AddNode(AttackOrMoveSelector);

        BTComp_Selector RootSelectorNode = new BTComp_Selector();
        RootSelectorNode.AddNode(AimActSequence);

        mAI.RootNode = RootSelectorNode;
	}
	
	public override void Update () {
        if (ControlledPawns[0] && ControlledPawns[0].IsAlive) {
            mAI.UpdateTree();
        }

        base.Update();
    }

    void OnGUI() {
        if (ControlledPawns[0] && ControlledPawns[0].IsAlive) {
            mAI.RootNode.RenderNode(new Vector2(10, 10));
        }
    }
}
