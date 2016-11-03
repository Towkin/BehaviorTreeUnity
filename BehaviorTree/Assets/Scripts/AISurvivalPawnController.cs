using UnityEngine;
using System.Collections;

public class AISurvivalPawnController : SurvivalPawnController<SurvivalPawn> {

    BehaviorTree<SurvivalPawnController<SurvivalPawn>> mAI;

    [SerializeField]
    private GameObject mBall;
    [SerializeField]
    private GameObject mFoeGoal;
    [SerializeField]
    private GameObject mHomeGoal;

    private GameObject mAimObject;
    private GameObject mMoveObject;
    private GameObject mDefendObject;

    [SerializeField]
    private float mDefendDistance = 10.0f;
    [SerializeField]
    private float mSprintDistance = 4.0f;
    
    void Start () {
        mAimObject = new GameObject(name + " - AimAt");
        mMoveObject = new GameObject(name + " - MoveTowards");
        mDefendObject = new GameObject(name + " - Defend");

        mAI = new BehaviorTree<SurvivalPawnController<SurvivalPawn>>(this as SurvivalPawnController<SurvivalPawn>);
        
        BTNode HealthNode = new BTCond_HealthThreshold<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, 10f);

        //BTNode SeeNode = new BTCond_CanSee<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mBall);
        BTNode AimNode = new BTTask_AimAt<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mAimObject.transform, 0.5f, 180);

        BTNode AttackRangeNode = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, transform, mBall.transform, ControlledPawns[0].AttackConeRadius);
        BTNode AttackNode = new BTTask_Attack<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, 0.2f);

        BTComp_Sequence InRangeAttackSequence = new BTComp_Sequence();
        InRangeAttackSequence.AddNode(AttackRangeNode);
        InRangeAttackSequence.AddNode(AttackNode);

        
        BTNode BallInDefenseDistance = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, mHomeGoal.transform, mBall.transform, mDefendDistance);
        BTNode MoveTowardsDefense = new BTTask_MoveTowards<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mDefendObject.transform);

        BTComp_Sequence DefendSequence = new BTComp_Sequence();
        DefendSequence.AddNode(BallInDefenseDistance);
        DefendSequence.AddNode(MoveTowardsDefense);
        
        BTNode InWalkRange = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, transform, mBall.transform, mSprintDistance);
        BTNode DisableSprint = new BTTask_Sprint<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, false);
        BTComp_Sequence DisableSprintSequence = new BTComp_Sequence();
        DisableSprintSequence.AddNode(InWalkRange);
        DisableSprintSequence.AddNode(DisableSprint);

        BTNode EnableSprint = new BTTask_Sprint<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, true);
        
        BTComp_Selector WalkOrRunSelector = new BTComp_Selector();
        WalkOrRunSelector.AddNode(DisableSprintSequence);
        WalkOrRunSelector.AddNode(EnableSprint);
        
        BTNode MoveNode = new BTTask_MoveTowards<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mMoveObject.transform);

        BTComp_Sequence MovementSequence = new BTComp_Sequence();
        MovementSequence.AddNode(WalkOrRunSelector);
        MovementSequence.AddNode(MoveNode);
        
        
        BTComp_Selector AttackOrMoveSelector = new BTComp_Selector();
        AttackOrMoveSelector.AddNode(InRangeAttackSequence);
        AttackOrMoveSelector.AddNode(DefendSequence);
        AttackOrMoveSelector.AddNode(MovementSequence);
        
        BTComp_Sequence AimActSequence = new BTComp_Sequence();
        //AimActSequence.AddNode(SeeNode);
        AimActSequence.AddNode(AimNode);
        AimActSequence.AddNode(AttackOrMoveSelector);

        BTComp_Selector RootSelectorNode = new BTComp_Selector();
        RootSelectorNode.AddNode(AimActSequence);

        mAI.RootNode = RootSelectorNode;
	}
	
	public override void Update () {
        mAimObject.transform.position = mBall.transform.position + (mFoeGoal.transform.position - mBall.transform.position).normalized * 2;
        mMoveObject.transform.position = mBall.transform.position + (mBall.transform.position - mFoeGoal.transform.position).normalized * 1.25f;
        mDefendObject.transform.position = Vector3.Lerp(mBall.transform.position, mHomeGoal.transform.position, 0.5f);

        if (ControlledPawns[0] && ControlledPawns[0].IsAlive) {
            mAI.UpdateTree();
        }

        base.Update();
    }

    void OnGUI() {
        if (Input.GetKey(KeyCode.Backspace) && ControlledPawns[0] && ControlledPawns[0].IsAlive) {
            mAI.RootNode.RenderNode(new Vector2(10, 10));
        }
    }
}
