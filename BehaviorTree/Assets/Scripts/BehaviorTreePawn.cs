using UnityEngine;
using System.Collections;
using System;

public class BTCond_HealthThreshold<T, Y> : BTCondition<T> where T : PawnController<Y> where Y : SurvivalPawn {
    public override string NodeText {
        get { return "Condition: Has more than " + HealthThreshold.ToString() + " Health?"; }
    }

    public BTCond_HealthThreshold(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
    public BTCond_HealthThreshold(BehaviorTree<T> aBehaviorTree, float aHealthThreshold) : this(aBehaviorTree) {
        HealthThreshold = aHealthThreshold;
    }
    
    private float mHealthThreshold = 0f;
    public float HealthThreshold {
        get { return mHealthThreshold; }
        set { mHealthThreshold = value; }
    }
    protected override BehaviorState UpdateNode() {
        if(Agent == null || Agent.ControlledPawns.Count == 0) {
            return BehaviorState.Error;
        }
        if (Agent.ControlledPawns[0].Health > HealthThreshold) {
            return BehaviorState.Success;
        }
        return BehaviorState.Failure;
    }
}
public class BTTask_MoveTowards<T, Y> : BTTask<T> where T : PawnController<Y> where Y : Pawn {
    public override string NodeText {
        get { return "Task: Move towards\n" + (Target == null ? "Nothing" : Target.name); }
    }

    public BTTask_MoveTowards(BehaviorTree<T> aBehaviorTree, Transform aTarget = null, bool aCanJump = true) : base(aBehaviorTree) {
        mTarget = aTarget;
        mCanJump = aCanJump;
    }

    private Transform mTarget;
    public Transform Target {
        get { return mTarget; }
        set { mTarget = value; }
    }
    private bool mCanJump;
    public bool CanJump {
        get { return mCanJump; }
        set { mCanJump = value; }
    }

    protected override BehaviorState UpdateNode() {
        if (Target == null) {
            return BehaviorState.Failure;
        }
        if (Agent.ControlledPawns.Count == 0) {
            return BehaviorState.Failure;
        }
        
        Agent.AddMoveInput(Quaternion.Inverse(Agent.ControlRotationQuat) * (Target.position - Agent.ControlledPawns[0].transform.position));

        if(CanJump && Agent.GetMoveCondition() == PawnMoveCondition.Ground && Target.position.y > Agent.ControlledPawns[0].transform.position.y + 1.0f) {
            Agent.CallJumpStart();

        }

        return BehaviorState.Success;
    }
}
public class BTTask_AimAt<T, Y> : BTTask<T> where T : PawnController<Y> where Y : Pawn {
    public override string NodeText {
        get { return "Task: Aim at\n" + (Target == null ? "Nothing" : Target.name); }
    }

    public BTTask_AimAt(BehaviorTree<T> aBehaviorTree) : base(aBehaviorTree) { }
    public BTTask_AimAt(BehaviorTree<T> aBehaviorTree, Transform aTarget, float aSmoothing = 0, float aMaxAngle = 0) : this(aBehaviorTree) {
        mTarget = aTarget;
        mLerpTarget = 1 - aSmoothing;
        mMaxAngle = aMaxAngle;
    }

    private Transform mTarget;
    public Transform Target {
        get { return mTarget; }
        set { mTarget = value; }
    }
    /// <summary>
    /// The maximum angle mod per second. A value of zero or below means no max. 
    /// </summary>
    private float mMaxAngle;
    public float MaxAngle {
        get { return mMaxAngle; }
        set { mMaxAngle = value; }
    }
    private float mLerpTarget;
    public float LerpTarget {
        get { return mLerpTarget; }
        set { mLerpTarget = Mathf.Clamp01(value); }
    }

    protected override BehaviorState UpdateNode() {
        if(Target == null) {
            return BehaviorState.Failure;
        }
        if(Agent.ControlledPawns.Count == 0) {
            return BehaviorState.Failure;
        }

        if (MaxAngle <= 0 && LerpTarget == 1) {
            Agent.ControlRotationQuat = Quaternion.LookRotation(Target.position - Agent.ControlledPawns[0].transform.position);
        } else {
            Agent.ControlRotationQuat = 
            Quaternion.RotateTowards(
                Agent.ControlRotationQuat,
                Quaternion.Lerp(
                    Agent.ControlRotationQuat,
                    Quaternion.LookRotation(Target.position - Agent.ControlledPawns[0].transform.position),
                    LerpTarget
                ),
            MaxAngle * Time.deltaTime);
        }

        return BehaviorState.Success;
    }
}

public class BTCond_CanSee<T, Y> : BTCondition<T> where T : PawnController<Y> where Y : Pawn {
    public override string NodeText {
        get { return "Condition: Can see\n" + (mTarget == null ? "Nothing" : Target.name) + "?"; }
    }

    public BTCond_CanSee(BehaviorTree<T> aBehaviorTree, GameObject aTarget = null) : base(aBehaviorTree) {
        mTarget = aTarget;
    }

    private GameObject mTarget;
    public GameObject Target {
        get { return mTarget; }
        set { mTarget = value; }
    }

    protected override BehaviorState UpdateNode() {
        if(Agent == null || Agent.ControlledPawns.Count == 0 || Target == null) {
            return BehaviorState.Error;
        }
        if (Physics.Linecast(Agent.ControlledPawns[0].transform.position, Target.transform.position, 1, QueryTriggerInteraction.Ignore)) {
            Debug.DrawLine(Agent.ControlledPawns[0].transform.position, Target.transform.position, Color.red, 0.2f);
            return BehaviorState.Failure;
        }

        Debug.DrawLine(Agent.ControlledPawns[0].transform.position, Target.transform.position, Color.green, 0.2f);
        return BehaviorState.Success;
    }
}

public class BTTask_Attack<T, Y> : BTTask<T> where T : SurvivalPawnController<Y> where Y : SurvivalPawn {
    public override string NodeText {
        get { return "Task: Attack"; }
    }

    public BTTask_Attack(BehaviorTree<T> aBehaviorTree, float aAttackTime = 0) : base(aBehaviorTree) {
        mAttackTime = aAttackTime;
    }

    private float mAttackTime;
    public float AttackTime {
        get { return mAttackTime; }
        set { mAttackTime = value; }
    }
    private bool mIsAttacking;
    private float mAttackStop;

    protected override BehaviorState UpdateNode() {
        if(Agent == null) {
            return BehaviorState.Error;
        }

        if (mIsAttacking) {
            if (Time.time > mAttackStop) {
                mIsAttacking = false;
                Agent.CallAttackEnd();
                return BehaviorState.Success;
            }
        } else {
            mIsAttacking = true;
            mAttackStop = Time.time + mAttackTime;
            Agent.CallAttackStart();
        }
        
        return BehaviorState.Running;
    }
}
class BTTask_Sprint<T, Y> : BTTask<T> where T : PawnController<Y> where Y : Pawn {
    public override string NodeText {
        get { return "Task: Sprint\n" + (EnableSprint ? "Enable" : "Disable"); }
    }

    private bool mEnableSprint;
    public bool EnableSprint {
        get { return mEnableSprint; }
        set { mEnableSprint = value; }
    }

    

    public BTTask_Sprint(BehaviorTree<T> aBehaviorTree, bool aEnableSprint = true) : base(aBehaviorTree) {
        mEnableSprint = aEnableSprint;
    }

    protected override BehaviorState UpdateNode() {
        if(Agent == null) {
            return BehaviorState.Error;
        }
        if(mEnableSprint) {
            if (Agent.GetMoveState() != PawnMoveState.Sprinting) {
                Agent.CallSprintStart();
            }
        } else {
            if (Agent.GetMoveState() == PawnMoveState.Sprinting) {
                Agent.CallSprintEnd();
            }
        }
        
        return BehaviorState.Success;
    }
}