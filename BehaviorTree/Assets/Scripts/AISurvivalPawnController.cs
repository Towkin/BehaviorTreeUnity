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

    [SerializeField]
    private bool mRenderTree = false;
    
    void Start () {
        mAimObject = new GameObject(name + " - AimAt");
        mMoveObject = new GameObject(name + " - MoveTowards");
        mDefendObject = new GameObject(name + " - Defend");
        
        mAI = new BehaviorTree<SurvivalPawnController<SurvivalPawn>>(this as SurvivalPawnController<SurvivalPawn>);
        
        //BTNode HealthNode = new BTCond_HealthThreshold<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, 10f);
        //BTNode SeeNode = new BTCond_CanSee<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mBall);

        BTNode AimNode = new BTTask_AimAt<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mAimObject.transform, 0.5f, 180);

        BTNode InSprintRange = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, transform, mBall.transform, mSprintDistance);
        BTNode EnableSprint = new BTTask_Sprint<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, true);
        BTComp_Sequence DisableSprintSequence = new BTComp_Sequence();
        DisableSprintSequence.AddNode(InSprintRange);
        DisableSprintSequence.AddNode(EnableSprint);
        
        BTNode DisableSprint = new BTTask_Sprint<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, false);
        BTComp_Selector WalkOrRunSelector = new BTComp_Selector();
        WalkOrRunSelector.AddNode(DisableSprintSequence);
        WalkOrRunSelector.AddNode(DisableSprint);

            BTNode AttackRangeNode = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, transform, mBall.transform, ControlledPawns[0].AttackConeRadius);
            BTNode AttackNode = new BTTask_Attack<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, 0.2f);

            BTComp_Sequence InRangeAttackSequence = new BTComp_Sequence();
            InRangeAttackSequence.AddNode(AttackRangeNode);
            InRangeAttackSequence.AddNode(AttackNode);

        
        BTNode BallInDefenseDistance = new BTCond_InDistance<SurvivalPawnController<SurvivalPawn>>(mAI, mDefendObject.transform, mBall.transform, mDefendDistance);
        BTNode MoveTowardsDefense = new BTTask_MoveTowards<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mDefendObject.transform);

        BTComp_Sequence DefendSequence = new BTComp_Sequence();
        DefendSequence.AddNode(BallInDefenseDistance);
        DefendSequence.AddNode(MoveTowardsDefense);
        
        
        BTNode MoveNode = new BTTask_MoveTowards<SurvivalPawnController<SurvivalPawn>, SurvivalPawn>(mAI, mMoveObject.transform);
        
        BTComp_Selector AttackOrMoveSelector = new BTComp_Selector();
        AttackOrMoveSelector.AddNode(InRangeAttackSequence);
        AttackOrMoveSelector.AddNode(DefendSequence);
        AttackOrMoveSelector.AddNode(MoveNode);
        
        BTComp_Sequence AimActSequence = new BTComp_Sequence();
        AimActSequence.AddNode(AimNode);
        AimActSequence.AddNode(WalkOrRunSelector);
        AimActSequence.AddNode(AttackOrMoveSelector);
        
        mAI.RootNode = AimActSequence;
	}
	
	public override void Update () {
        mAimObject.transform.position = mBall.transform.position + (mFoeGoal.transform.position - mBall.transform.position).normalized * 2;
        mMoveObject.transform.position = mBall.transform.position + (mBall.transform.position - mFoeGoal.transform.position).normalized * 1.25f;

        RaycastHit Info;
        Ray BallRay = new Ray(mBall.transform.position, mHomeGoal.transform.position - mBall.transform.position);
        Physics.Raycast(BallRay, out Info, 100.0f, LayerMask.GetMask("Goal"), QueryTriggerInteraction.Collide);
        
        mDefendObject.transform.position = Vector3.Lerp(Info.point, mBall.transform.position, 0.5f);

        if (ControlledPawns[0] && ControlledPawns[0].IsAlive) {
            mAI.UpdateTree();
        }

        base.Update();
    }

    void OnGUI() {
        if (mRenderTree && ControlledPawns[0] && ControlledPawns[0].IsAlive) {
            mAI.RootNode.RenderNode(new Vector2(10, 10));
        }
    }
}
