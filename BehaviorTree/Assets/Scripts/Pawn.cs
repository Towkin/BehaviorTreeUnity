using UnityEngine;
using System.Collections;


public struct PawnMoveCondition {
    public const byte Ground = 0;
    public const byte Air = 1;
    public const byte Water = 2;

    public byte Value;

    public PawnMoveCondition(byte aValue) : this() {
        Value = aValue;
    }
    public PawnMoveCondition(PawnMoveCondition aOther) : this() {
        Value = aOther.Value;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    public static bool operator ==(PawnMoveCondition aThis, PawnMoveCondition aOther) {
        return aThis.Value == aOther.Value;
    }
    public static bool operator !=(PawnMoveCondition aThis, PawnMoveCondition aOther) {
        return aThis.Value != aOther.Value;
    }
    public static bool operator ==(PawnMoveCondition aThis, byte aOther) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(PawnMoveCondition aThis, byte aOther) {
        return aThis.Value != aOther;
    }
    public static bool operator ==(byte aOther, PawnMoveCondition aThis) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(byte aOther, PawnMoveCondition aThis) {
        return aThis.Value != aOther;
    }
    public static implicit operator PawnMoveCondition(byte aOther) {
        return new PawnMoveCondition(aOther);
    }
}
public struct PawnMoveState {
    public const byte Walking = 0;
    public const byte Sprinting = 1;
    public const byte Crouching = 2;
    public const byte Swimming = 2;

    public byte Value;

    public PawnMoveState(byte aValue) : this() {
        Value = aValue;
    }
    public PawnMoveState(PawnMoveState aOther) : this() {
        Value = aOther.Value;
    }

    public override bool Equals(object obj) {
        return base.Equals(obj);
    }
    public override int GetHashCode() {
        return base.GetHashCode();
    }
    public static bool operator ==(PawnMoveState aThis, PawnMoveState aOther) {
        return aThis.Value == aOther.Value;
    }
    public static bool operator !=(PawnMoveState aThis, PawnMoveState aOther) {
        return aThis.Value != aOther.Value;
    }
    public static bool operator ==(PawnMoveState aThis, byte aOther) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(PawnMoveState aThis, byte aOther) {
        return aThis.Value != aOther;
    }
    public static bool operator ==(byte aOther, PawnMoveState aThis) {
        return aThis.Value == aOther;
    }
    public static bool operator !=(byte aOther, PawnMoveState aThis) {
        return aThis.Value != aOther;
    }
    public static implicit operator PawnMoveState(byte aOther) {
        return new PawnMoveState(aOther);
    }
}

[RequireComponent(typeof(CharacterController))]
public class Pawn : MonoBehaviour {

    #region Controller
    
    private CharacterController mMovementController = null;
    protected CharacterController MovementController {
        get { return mMovementController; }
        private set { mMovementController = value; }
    }
    public virtual Controller<Pawn> PawnController {
        get { return GetComponent<Controller<Pawn>>(); }
    }
    [Header("Controller Settings")]
    [SerializeField]
    private bool mBodyUseControllerPitch = false;
    /// <summary>
    /// Should the Controller's pitch be applied to the Pawn's GameObject's rotation?
    /// </summary>
    public bool BodyUseControllerPitch {
        get { return mBodyUseControllerPitch; }
    }
    [SerializeField]
    private bool mBodyUseControllerYaw = true;
    /// <summary>
    /// Should the Controller's yaw be applied to the Pawn's GameObject's rotation?
    /// </summary>
    public bool BodyUseControllerYaw {
        get { return mBodyUseControllerYaw; }
    }
    [SerializeField]
    private bool mBodyUseControllerRoll = false;
    /// <summary>
    /// Should the Controller's yaw be applied to the Pawn's GameObject's rotation?
    /// </summary>
    public bool BodyUseControllerRoll {
        get { return mBodyUseControllerRoll; }
    }
    #endregion
    #region Movement - Base

    private float mSpeed = 0f;
    private Vector3 mDirection = Vector3.forward;
    private Vector2 mPlanarDirection = Vector2.up;
    [SerializeField]
    private Vector3 mPlanarNormal = Vector3.up;
    /// <summary>
    /// The pawn's speed as scalar in m/s. same as Velocity magnitude, but cheaper performance-wise.
    /// Setting this to negative will cause Direction to invert.
    /// </summary>
    public float Speed {
        get { return mSpeed; }
        set {
            if(value < 0f) {
                Direction = -Direction;
                mSpeed = -value;
            } else {
                mSpeed = value;
            }
        }
    }
    public float PlanarSpeed {
        get { return PlanarVelocity.magnitude; }
        set { PlanarVelocity = PlanarDirection * value; }
    }
    /// <summary>
    /// The last updated Velocity direction of the Pawn. Also updates PlanarDirection is applicable.
    /// </summary>
    public Vector3 Direction {
        get { return mDirection; }
        set {
            // Keep old direction if new value is zero.
            if(value != Vector3.zero) {
                mDirection = value.normalized;

                // If the set Direction is parallell to the PlanarNormal, do not update the PlanarDirection.
                // 0.999f is a number close enough to 1, where the new Direction is essentially parallell to PlanarNormal.
                if(Mathf.Abs(Vector3.Dot(Direction, PlanarNormal)) <= 0.999f) {
                    mPlanarDirection = ToPlanar(Direction).normalized;
                }
            }
        }
    }
    /// <summary>
    /// The 2d Direction on plane defined by the PlanarNormal. Automatically updated on Direction change.
    /// </summary>
    public Vector2 PlanarDirection {
        get { return mPlanarDirection; }
    }
    /// <summary>
    /// The up-normal of the Pawn. Should mostly be 'Vector3.up'. All 'Planar'-variables use this normal to determine their plane.
    /// </summary>
    public Vector3 PlanarNormal {
        get { return mPlanarNormal; }
        protected set {
            if(value != Vector3.zero) {
                mPlanarNormal = value.normalized;
                // Update planar direction, which is only updated through Direction update... Properties, yay!
                Direction = Direction;
            }
        }
    }
    /// <summary>
    /// Projects and rotates a given vector by the Pawn's PlanarNormal, returning the resulting vector on the 2d-plane defined by the PlanarNormal.
    /// </summary>
    /// <param name="aVector">The vector to project and rotate.</param>
    /// <returns>The vector given on the 2d-plane.</returns>
    public Vector2 ToPlanar(Vector3 aVector) {
        // Project the Direction onto the plane defined by PlanarNormal. Rotate that projection to the plane's coordinate system.
        Vector3 ProjectedRotated = Quaternion.FromToRotation(Quaternion.Inverse(transform.rotation) * PlanarNormal, Vector3.up) * Vector3.ProjectOnPlane(aVector, PlanarNormal);

        // Note, Vector3.forward is represented as Vector2.up here.
        return new Vector2(ProjectedRotated.x, ProjectedRotated.z);
    }
    /// <summary>
    /// Rotates a given 2d-vector from the plane defined by the PlanarNormal to the global coordinate system, returning a 3d-vector.
    /// </summary>
    /// <param name="aVector">The vector to rotate.</param>
    /// <returns>The 3d representation of the 2d-vector on the PlanarNormal.</returns>
    public Vector3 FromPlanar(Vector2 aVector) {
        // Rotate the vector to global coordinate system.
        return Quaternion.FromToRotation(Vector3.up, Quaternion.Inverse(transform.rotation) * PlanarNormal) * new Vector3(aVector.x, 0, aVector.y);
    }
    /// <summary>
    /// Global velocity in m/s.
    /// </summary>
    public Vector3 Velocity {
        get { return mDirection * mSpeed; }
        protected set {
            Direction = value;
            mSpeed = value.magnitude;
        }
    }
    /// <summary>
    /// Velocity rotated by the Pawn's transform, relative to global, in m/s.
    /// </summary>
    public Vector3 ForwardVelocity {
        get { return Quaternion.Inverse(transform.rotation) * Velocity; }
        set { Velocity = transform.rotation * value; }
    }
    /// <summary>
    /// The velocity along the plane defined by the PlanarNormal.
    /// </summary>
    public Vector2 PlanarVelocity {
        get { return ToPlanar(Velocity); }
        set {
            Vector3 InputGlobal = FromPlanar(value);
            Velocity = InputGlobal + (Velocity - FromPlanar(PlanarVelocity));
        }
    }
    /// <summary>
    /// The forward velocity along the plane defined by the PlanarNormal.
    /// </summary>
    public Vector2 PlanarForwardVelocity {
        get { return ToPlanar(ForwardVelocity); }
        set {
            Vector3 InputForward = FromPlanar(value);
            ForwardVelocity = InputForward + (ForwardVelocity - FromPlanar(PlanarForwardVelocity));
        }
    }
    #endregion
    #region Movement - Serialized Variables

    [Header("Movement Settings")]
    [SerializeField]
    private float mWalkAcceleration = 15f;
    [SerializeField]
    private float mSprintAcceleration = 25f;
    [SerializeField]
    private float mCrouchAcceleration = 10f;
    [SerializeField]
    private float mSwimAcceleration = 15f;


    [SerializeField]
    private float mGroundAccelerationFactor = 1f;
    [SerializeField]
    private float mAirAccelerationFactor = 0.15f;
    [SerializeField]
    private float mWaterAccelerationFactor = 0.8f;

    [SerializeField]
    private float mJumpAcceleration = 50f;

    [SerializeField]
    private float mGroundFriction = 10.0f;
    [SerializeField]
    private float mGroundDrag = 0.5f;
    [SerializeField]
    private float mAirFriction = 0.1f;
    [SerializeField]
    private float mAirDrag = 0.53f;
    [SerializeField]
    private float mWaterFriction = 6.0f;
    [SerializeField]
    private float mWaterDrag = 0.7f;

    [SerializeField]
    private float mMaxWalkSpeed = 4f;
    [SerializeField]
    private float mMaxSprintSpeed = 12f;
    [SerializeField]
    private float mMaxCrouchSpeed = 2f;
    [SerializeField]
    private float mMaxSwimSpeed = 4f;

    [SerializeField]
    private Vector3 mGroundGravity = Physics.gravity * 0.1f;
    [SerializeField]
    private Vector3 mAirGravity = Physics.gravity;
    [SerializeField]
    private Vector3 mWaterGravity = Physics.gravity;


    public float MoveAcceleration {
        get {
            return
            (
                MoveState == PawnMoveState.Walking ? mWalkAcceleration :
                MoveState == PawnMoveState.Sprinting ? mSprintAcceleration :
                MoveState == PawnMoveState.Crouching ? mCrouchAcceleration :
                MoveState == PawnMoveState.Swimming ? mSwimAcceleration :
                0f
            ) * (
                MoveCondition == PawnMoveCondition.Ground ? mGroundAccelerationFactor :
                MoveCondition == PawnMoveCondition.Air ? mAirAccelerationFactor :
                MoveCondition == PawnMoveCondition.Water ? mWaterAccelerationFactor :
                1f
            );
        }
    }
    public float JumpAcceleration {
        get { return mJumpAcceleration; }
    }
    public float MoveFriction {
        get {
            return 
                MoveCondition == PawnMoveCondition.Ground ? mGroundFriction :
                MoveCondition == PawnMoveCondition.Air ? mAirFriction :
                MoveCondition == PawnMoveCondition.Water ? mWaterFriction :
                0f;
        }
    }
    public float MoveDrag {
        get {
            return
                MoveCondition == PawnMoveCondition.Ground ? mGroundDrag :
                MoveCondition == PawnMoveCondition.Air ? mAirDrag :
                MoveCondition == PawnMoveCondition.Water ? mWaterDrag :
                0f;
        }
    }
    public float MaxControlSpeed {
        get {
            return 
                MoveState == PawnMoveState.Walking ? mMaxWalkSpeed :
                MoveState == PawnMoveState.Sprinting ? mMaxSprintSpeed :
                MoveState == PawnMoveState.Crouching ? mMaxCrouchSpeed :
                MoveState == PawnMoveState.Swimming ? mMaxSwimSpeed :
                0f;
        }
    }
    
    /// <summary>
    /// Constant acceleration applied onto the pawn's Velocity, in m/s².
    /// </summary>
    public Vector3 PawnGravity {
        get {
            return
                MoveCondition == PawnMoveCondition.Ground ? mGroundGravity :
                MoveCondition == PawnMoveCondition.Air ? mAirGravity :
                MoveCondition == PawnMoveCondition.Water ? mWaterGravity :
                Vector3.zero;
        }
    }
    #endregion
    #region Movement - Dynamic Variables
    private PawnMoveCondition mMoveCondition = new PawnMoveCondition(PawnMoveCondition.Ground);
    private PawnMoveState mMoveState = new PawnMoveState(PawnMoveState.Walking);

    public PawnMoveCondition MoveCondition {
        get { return mMoveCondition; }
        protected set { mMoveCondition = value; }
    }
    public PawnMoveState MoveState {
        get { return mMoveState; }
        protected set { mMoveState = value; }
    }
    public bool IsGrounded {
        get { return mMoveCondition == PawnMoveCondition.Ground; }
    }
    public bool IsInAir {
        get { return mMoveCondition == PawnMoveCondition.Air; }
    }
    public bool IsInWater {
        get { return mMoveCondition == PawnMoveCondition.Water; }
    }
    #endregion
    // Use this for initialization
    void Start () {
        MovementController = GetComponent<CharacterController>();

        PlanarNormal = PlanarNormal;
        Vector3 DebugVector = new Vector3(5, 1, -2);

        Debug.Log(DebugVector.ToString() + " - " + ToPlanar(DebugVector).ToString() + "; " + FromPlanar(ToPlanar(DebugVector)).ToString());
	}
	
	// Update is called once per frame
	void Update () {
        
        UpdatePawnRotation(Time.deltaTime);
    }

    void FixedUpdate() {
        UpdatePawnVelocity(Time.fixedDeltaTime);
        UpdatePawnPosition(Time.fixedDeltaTime);
        UpdatePawnCondition();
    }

    protected virtual void UpdatePawnRotation(float aDeltaTime) {
        if (PawnController && (BodyUseControllerPitch || BodyUseControllerRoll || BodyUseControllerYaw)) {
            
            Vector3 BaseEuler = transform.rotation.eulerAngles;
            Vector3 ControlEuler = PawnController.ControlEuler;

            Quaternion BodyRotation = new Quaternion();
            BodyRotation.eulerAngles = new Vector3(
                BodyUseControllerPitch ? ControlEuler.x : BaseEuler.x,
                BodyUseControllerYaw   ? ControlEuler.y : BaseEuler.y,
                BodyUseControllerRoll  ? ControlEuler.z : BaseEuler.z
            );

            transform.rotation = BodyRotation;
        }
    }
    protected virtual void UpdatePawnVelocity(float aDeltaTime) {
        Velocity += PawnGravity * aDeltaTime;
        Speed -= Mathf.Min(MoveFriction * aDeltaTime, Speed);
        Speed *= Mathf.Pow(1f - MoveDrag, aDeltaTime);

        if (PawnController) {
            Vector2 MoveInput = PawnController.ConsumeMoveInput();
            Vector2 ForwardAccelerationAdd = MoveInput * MoveAcceleration * aDeltaTime;

            // TODO: Fix this major forward velocity bug.
            Vector3 AA = transform.position + transform.rotation * FromPlanar(new Vector2(-1, -1));
            Vector3 AB = transform.position + transform.rotation * FromPlanar(new Vector2(-1, 1));
            Vector3 BA = transform.position + transform.rotation * FromPlanar(new Vector2(1, -1));
            Vector3 BB = transform.position + transform.rotation * FromPlanar(new Vector2(1, 1));

            Debug.DrawLine(AA, AB, Color.green, 0.25f);
            Debug.DrawLine(AA, BA, Color.green, 0.25f);
            Debug.DrawLine(AB, BB, Color.green, 0.25f);
            Debug.DrawLine(BA, BB, Color.green, 0.25f);
            Debug.DrawLine(transform.position, transform.position + transform.rotation * FromPlanar(ForwardAccelerationAdd) * 5, Color.blue, 0.25f);

            if (Vector2.Dot(ForwardAccelerationAdd, PlanarForwardVelocity) < MaxControlSpeed) {
                PlanarForwardVelocity += ForwardAccelerationAdd;
            }

            if (MoveCondition == PawnMoveCondition.Ground) {
                Velocity += PlanarNormal * (PawnController.InputJump ? JumpAcceleration : 0) * aDeltaTime;
            }
        }
        
    }
    protected virtual void UpdatePawnPosition(float aDeltaTime) {
        
        CollisionFlags MoveCollisions = MovementController.Move(Velocity * aDeltaTime);
        Velocity = MovementController.velocity;

        
        PawnController.OnPawnMoved();
    }
    protected virtual void UpdatePawnCondition() {

        PlanarNormal = Vector3.up;
        MoveCondition = PawnMoveCondition.Air;

        float Radius = MovementController.radius + MovementController.skinWidth;
        Vector3 BottomCenter = transform.position + MovementController.center - new Vector3(0, MovementController.height / 2 - Radius, 0);
        Vector3 TopCenter = transform.position + MovementController.center + new Vector3(0, MovementController.height / 2 - Radius, 0);
        float RayOffset = 0.15f;

        // TODO: Understand why Water check isn't working properly.
        if (Physics.CheckCapsule(TopCenter, BottomCenter, Radius - 0.1f, LayerMask.NameToLayer("Water"), QueryTriggerInteraction.Collide)) {
            if (PawnController) {
                PlanarNormal = PawnController.ControlRotation * Vector3.up;
            }

            MoveCondition = PawnMoveCondition.Water;
        } else {
            RaycastHit[] GroundCheckHits = Physics.SphereCastAll(BottomCenter, Radius, Vector3.down, RayOffset);
            Debug.DrawLine(BottomCenter, BottomCenter + Vector3.down * (RayOffset + Radius), Color.red, 2f);

            foreach (RaycastHit GroundCheck in GroundCheckHits) {
                if (GroundCheck.collider.gameObject != gameObject) {
                    PlanarNormal = GroundCheck.normal;
                    MoveCondition = PawnMoveCondition.Ground;
                    break;
                }
            }
        }
    }

}
