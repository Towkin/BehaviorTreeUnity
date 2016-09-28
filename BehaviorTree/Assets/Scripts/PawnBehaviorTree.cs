using UnityEngine;
using System.Collections;
using System;

public class BTCond_HealthThreshold<T> : BTCondition<T> where T : SurvivalPawnController<SurvivalPawn> {
    public override string NodeText {
        get { return "Condition: Has more than " + HealthThreshold.ToString() + " Health"; }
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