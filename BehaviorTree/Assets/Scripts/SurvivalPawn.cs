using UnityEngine;
using System.Collections.Generic;

public class SurvivalPawn : Pawn {

    #region Health stuff
    [SerializeField]
    private float mHealth;
    [SerializeField]
    private float mHealthMax;

    public float Health {
        get { return mHealth; }
        protected set { mHealth = Mathf.Clamp(value, 0, mHealthMax); }
    }
    public float HealthMax {
        get { return mHealthMax; }
        protected set { mHealthMax = value; }
    }
    public float HealthPercentage {
        get { return Health / HealthMax; }
        protected set { Health = value * HealthMax; }
    }
    public bool IsAlive {
        get { return Health != 0; }
    }

    public void TakeDamage(float aDamage) {
        Health -= aDamage;
    }
    #endregion
    #region Attack stuff
    [SerializeField]
    private float mAttackDamage;
    [SerializeField]
    private float mAttackTime;
    private float mLastAttack = 0f;

    [SerializeField]
    private float mAttackConeAngle;
    [SerializeField]
    private float mAttackConeRadius;

    public float AttackDamage {
        get { return mAttackDamage; }
    }
    public float AttackTime {
        get { return mAttackTime; }
    }
    public float LastAttackTime {
        get { return mLastAttack; }
        protected set { mLastAttack = value; }
    }
    public float AttackConeAngle {
        get { return mAttackConeAngle; }
    }
    public float AttackConeRadius {
        get { return mAttackConeRadius; }
    }


    public bool CanAttack {
        get { return Time.time > mLastAttack + mAttackTime; }
    }



    public virtual void PawnAttack() {
        if (CanAttack) {
            LastAttackTime = Time.time;

            SurvivalPawn[] AttackedPawns = PawnsInAttackCone();
            foreach (SurvivalPawn AttackedPawn in AttackedPawns) {
                AttackedPawn.TakeDamage(AttackDamage);
            }
        }
    }
    public SurvivalPawn[] PawnsInAttackCone() {
        List<SurvivalPawn> HitPawns = new List<SurvivalPawn>();

        Collider[] PawnColliders = Physics.OverlapSphere(transform.position, AttackConeRadius);
        foreach (Collider PawnCollider in PawnColliders) {
            SurvivalPawn HitPawn = PawnCollider.GetComponent<SurvivalPawn>();

            if (HitPawn && HitPawn != this) {
                HitPawns.Add(HitPawn);
            }
        }

        return HitPawns.ToArray();
    }
    #endregion
}
