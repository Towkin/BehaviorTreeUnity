using UnityEngine;
using System.Collections.Generic;
using System;

public struct DamageType {
    public const byte Default = 0;
    public const byte Slashing = 1;
    public const byte Piercing = 2;
    public const byte Blunt = 3;

    public byte Value;

    public DamageType(byte aValue) : this() {
        Value = aValue;
    }
    public DamageType(DamageType aOther) : this() {
        Value = aOther.Value;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    public static bool operator ==(DamageType aThis, DamageType aOther) {
        return aThis.Value == aOther.Value;
    }
    public static bool operator !=(DamageType aThis, DamageType aOther) {
        return aThis.Value != aOther.Value;
    }
    public static bool operator ==(DamageType aThis, byte aOther) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(DamageType aThis, byte aOther) {
        return aThis.Value != aOther;
    }
    public static bool operator ==(byte aOther, DamageType aThis) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(byte aOther, DamageType aThis) {
        return aThis.Value != aOther;
    }
    public static implicit operator DamageType(byte aOther) {
        return new DamageType(aOther);
    }
}
public struct DamageInfo {
    public float Amount;
    public float Force;
    public DamageType Type;

    public GameObject Dealer;
    public Vector3 Direction;

    public DamageInfo(float aAmount, DamageType aType = new DamageType(), float aForce = 0f, GameObject aDealer = null, Vector3 aDirection = new Vector3()) {
        Amount = aAmount;
        Force = aForce;
        Type = aType;
        Dealer = aDealer;
        Direction = aDirection;
    }
}

public interface Interactable {
    void Interact(SurvivalPawn aUser);
}

public class SurvivalPawn : Pawn {

    #region Health stuff
    [SerializeField]
    private float mHealth = 0f;
    [SerializeField]
    private bool mStartWithFullHealth = true;
    [SerializeField]
    private float mHealthMax = 100f;

    public float Health {
        get { return mHealth; }
        protected set {
            mHealth = Mathf.Clamp(value, 0, mHealthMax);
            if(mHealth == 0) {
                OnDeath();
            }
        }
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

    public void TakeDamage(DamageInfo aInfo) {
        Health -= aInfo.Amount;

        if (aInfo.Type == DamageType.Blunt) {
            Velocity += aInfo.Direction * (aInfo.Force / Mass);
        }
    }
    #endregion
    #region Attack stuff
    [SerializeField]
    private float mAttackDamage = 20f;
    [SerializeField]
    private float mAttackTime = 0.5f;
    private float mLastAttack = 0f;

    [SerializeField]
    private float mAttackConeAngle = 30f;
    [SerializeField]
    private float mAttackConeRadius = 1.2f;

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
                AttackedPawn.TakeDamage(new DamageInfo(AttackDamage, DamageType.Blunt, 1000f, gameObject, ControlRotation * Vector3.forward));
            }

            for (float Rad = 0; Rad < Mathf.PI * 2; Rad += Mathf.PI * 2 / 32) {
                Vector3 RayDirection = transform.rotation * new Vector3(Mathf.Sin(Rad), Mathf.Cos(Rad), AttackConeRadius);

                Debug.DrawRay(transform.position, RayDirection, Color.yellow, 2.5f);
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
    #region Interact stuff
    [SerializeField]
    private float mInteractRange = 1f;
    [SerializeField]
    private float mInteractRadius = 0.1f;
    
    public float InteractRange {
        get { return mInteractRange; }
        protected set { mInteractRange = value; }
    }
    public float InteractRadius {
        get { return mInteractRadius; }
        protected set { mInteractRadius = value; }
    }

    protected Interactable[] GetInteractablesInRange() {
        List<Interactable> InteractableList = new List<Interactable>();

        Vector3 StartPos = transform.position;
        Vector3 EndPos = StartPos + ControlRotation * Vector3.forward * InteractRange;

        Collider[] InteractColliders = Physics.OverlapCapsule(StartPos, EndPos, InteractRadius);

        Debug.DrawLine(StartPos, EndPos, Color.blue, 2f);

        foreach(Collider InteractCollider in InteractColliders) {
            if (InteractCollider.tag == "Interactable") {
                Component[] HitComponents = InteractCollider.GetComponents<Component>();

                foreach(Component HitComponent in HitComponents) {
                    if (HitComponent is Interactable) {
                        InteractableList.Add((Interactable)HitComponent);
                    } 
                }
            }
        }

        return InteractableList.ToArray();
    }

    #endregion

    public override void Start() {
        base.Start();
        if (mStartWithFullHealth) {
            Health = HealthMax;
        }
    }

    #region Event Recievers
    public virtual void AttackStart() {
        PawnAttack();
    }
    public virtual void AttackEnd() {

    }
    public virtual void InteractStart() {
        Interactable[] Interacts = GetInteractablesInRange();

        foreach(Interactable InteractInRange in Interacts) {
            InteractInRange.Interact(this);
        }
    }
    public virtual void InteractEnd() {

    }

    public virtual void OnDeath() {
        
    }

    #endregion
}
