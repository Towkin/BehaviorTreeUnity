using UnityEngine;
using System.Collections;

public class SurvivalPawnController<T> : PawnController<T> where T : SurvivalPawn {

    protected event InputButtonEvent eOnAttackStart;    protected void CallAttackStart()    { if (eOnAttackStart != null)   eOnAttackStart(); }
    protected event InputButtonEvent eOnAttackEnd;      protected void CallAttackEnd()      { if (eOnAttackEnd != null)     eOnAttackEnd(); }

    protected override void AddEvents(T aPawn) {
        base.AddEvents(aPawn);

        eOnAttackStart += aPawn.AttackStart;
        eOnAttackEnd += aPawn.AttackEnd;
    }
    protected override void RemoveEvents(T aPawn) {
        base.RemoveEvents(aPawn);

        eOnAttackStart -= aPawn.AttackStart;
        eOnAttackEnd -= aPawn.AttackEnd;
    }
}
