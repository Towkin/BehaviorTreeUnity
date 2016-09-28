using UnityEngine;
using System.Collections;

public class AISurvivalPawnController<T> : SurvivalPawnController<T> where T : SurvivalPawn {

    BehaviorTree<SurvivalPawnController<SurvivalPawn>> mAI;

	void Start () {
        mAI = new BehaviorTree<SurvivalPawnController<SurvivalPawn>>(this as SurvivalPawnController<SurvivalPawn>);

        BTNode HealthNode = new BTCond_HealthThreshold<SurvivalPawnController<SurvivalPawn>>(mAI, 10f);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
