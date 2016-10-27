using UnityEngine;
using System.Collections;

public class SurvivalPawnController<T> : PawnController<T> where T : SurvivalPawn {

    protected event InputButtonEvent eOnAttackStart;    public void CallAttackStart()    { if (eOnAttackStart != null)   eOnAttackStart(); }
    protected event InputButtonEvent eOnAttackEnd;      public void CallAttackEnd()      { if (eOnAttackEnd != null)     eOnAttackEnd(); }

    protected event InputButtonEvent eOnInteractStart;  public void CallInteractStart()  { if (eOnInteractStart != null) eOnInteractStart(); }
    protected event InputButtonEvent eOnInteractEnd;    public void CallInteractEnd()    { if (eOnInteractEnd != null)   eOnInteractEnd(); }

    protected override void AddEvents(T aPawn) {
        base.AddEvents(aPawn);

        eOnAttackStart += aPawn.AttackStart;
        eOnAttackEnd += aPawn.AttackEnd;

        eOnInteractStart += aPawn.InteractStart;
        eOnInteractEnd += aPawn.InteractEnd;
    }
    protected override void RemoveEvents(T aPawn) {
        base.RemoveEvents(aPawn);

        eOnAttackStart -= aPawn.AttackStart;
        eOnAttackEnd -= aPawn.AttackEnd;

        eOnInteractStart -= aPawn.InteractStart;
        eOnInteractEnd -= aPawn.InteractEnd;
    }
}
