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
    public const byte Swimming = 3;

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
    private PawnController<Pawn> mPawnController = null;
    public virtual PawnController<Pawn> PawnController {
        get { return mPawnController; }
        set { mPawnController = value; }
    }

    private Quaternion mControlRotation;
    public Quaternion ControlRotation {
        get { return mControlRotation; }
        protected set { mControlRotation = value; }
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

    [SerializeField]
    private float mSpeed = 0f;
    [SerializeField]
    private Vector3 mDirection = Vector3.forward;
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
                //if(Mathf.Abs(Vector3.Dot(Direction, PlanarNormal)) <= 0.999f) {
                //    mPlanarDirection = ToPlanar(Direction).normalized;
                //}
            }
        }
    }
    /// <summary>
    /// The 2d Direction on plane defined by the PlanarNormal. Automatically updated on Direction change.
    /// </summary>
    public Vector3 PlanarDirection {
        get { return ToPlanar(Direction); }
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
    public Vector3 ToPlanar(Vector3 aVector) {
        //// Project the Direction onto the plane defined by PlanarNormal. Rotate that projection to the plane's coordinate system.
        //Vector3 ProjectedRotated = Quaternion.FromToRotation(Quaternion.Inverse(transform.rotation) * PlanarNormal, Vector3.up) * Vector3.ProjectOnPlane(aVector, PlanarNormal);

        //// Note, Vector3.forward is represented as Vector2.up here.
        //return new Vector2(ProjectedRotated.x, ProjectedRotated.z);

        // New method, testing
        //return Quaternion.FromToRotation(Quaternion.Inverse(transform.rotation) * PlanarNormal, Vector3.up) * Vector3.ProjectOnPlane(aVector, PlanarNormal);
        return Quaternion.FromToRotation(Quaternion.Inverse(transform.rotation) * PlanarNormal, Vector3.up) * aVector;
    }
    /// <summary>
    /// Rotates a given 2d-vector from the plane defined by the PlanarNormal to the global coordinate system, returning a 3d-vector.
    /// </summary>
    /// <param name="aVector">The vector to rotate.</param>
    /// <returns>The 3d representation of the 2d-vector on the PlanarNormal.</returns>
    public Vector3 FromPlanar(Vector3 aVector) {
        // Rotate the vector to global coordinate system.
        //return Quaternion.FromToRotation(Vector3.up, Quaternion.Inverse(transform.rotation) * PlanarNormal) * new Vector3(aVector.x, 0, aVector.y);

        // New method, testing
        return Quaternion.FromToRotation(Vector3.up, Quaternion.Inverse(transform.rotation) * PlanarNormal) * aVector;
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
    public Vector3 PlanarVelocity {
        get { return ToPlanar(Velocity); }
        set {
            //Vector3 InputGlobal = FromPlanar(value);
            //Velocity = InputGlobal + (Velocity - FromPlanar(PlanarVelocity));

            // New method, testing
            Velocity = FromPlanar(value);
        }
    }
    /// <summary>
    /// The forward velocity along the plane defined by the PlanarNormal.
    /// </summary>
    public Vector3 PlanarForwardVelocity {
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
    private LayerMask mGroundMask = 1; // Initializes to 'Default' Layer
    [SerializeField]
    private LayerMask mWaterMask = 16; // Initializes to 'Water' Layer

    [SerializeField]
    private float mWalkAcceleration = 80f;
    [SerializeField]
    private float mSprintAcceleration = 70f;
    [SerializeField]
    private float mCrouchAcceleration = 50f;
    [SerializeField]
    private float mSwimAcceleration = 35f;


    [SerializeField]
    private float mGroundAccelerationFactor = 1f;
    [SerializeField]
    private float mAirAccelerationFactor = 0.10f;
    [SerializeField]
    private float mWaterAccelerationFactor = 0.8f;

    [SerializeField]
    private float mJumpAcceleration = 50f;
    [SerializeField]
    private float mJumpTimeMax = 0.15f;

    [SerializeField]
    private float mGroundFriction = 40.0f;
    [SerializeField]
    private float mGroundDrag = 0.5f;
    [SerializeField]
    private float mAirFriction = 0.1f;
    [SerializeField]
    private float mAirDrag = 0.01f;
    [SerializeField]
    private float mWaterFriction = 0.1f;
    [SerializeField]
    private float mWaterDrag = 0.9f;

    [SerializeField]
    private float mMaxWalkSpeed = 4f;
    [SerializeField]
    private float mMaxSprintSpeed = 10f;
    [SerializeField]
    private float mMaxCrouchSpeed = 2f;
    [SerializeField]
    private float mMaxSwimSpeed = 3f;

    [SerializeField]
    private float mMass = 100f;

    [SerializeField]
    private Vector3 mGroundGravity = Physics.gravity * 0.1f;
    [SerializeField]
    private Vector3 mAirGravity = Physics.gravity;
    [SerializeField]
    private Vector3 mWaterGravity = Physics.gravity;

    [SerializeField]
    private bool mRecieveInput = true;

    public LayerMask GroundMask {
        get { return mGroundMask; }
    }
    public LayerMask WaterMask {
        get { return mWaterMask; }
    }
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
    public float JumpTimeMax {
        get { return mJumpTimeMax; }
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
    public float Mass {
        get { return mMass; }
        protected set { mMass = Mathf.Max(0f, value); }
    }
    public bool RecieveInput {
        get { return mRecieveInput; }
        protected set { mRecieveInput = value; }
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

    private bool mJumpState = false;
    private float mLastJumpStart = 0;
    private Vector3 mJumpVector = Vector3.up;

    public PawnMoveCondition MoveCondition {
        get { return mMoveCondition; }
        protected set { mMoveCondition = value; }
    }
    public PawnMoveState MoveState {
        get { return mMoveState; }
        protected set { mMoveState = value; }
    }
    public bool JumpState {
        get { return mJumpState; }
        protected set { mJumpState = value; }
    }
    public float LastJumpStart {
        get { return mLastJumpStart; }
        protected set { mLastJumpStart = value; }
    }
    public Vector3 JumpVector {
        get { return mJumpVector; }
        protected set { mJumpVector = value; }
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
    public bool IsJumping {
        get { return mJumpState && (LastJumpStart + JumpTimeMax) > Time.time; }
    }
   
    #endregion
    
    public virtual void Start () {
        MovementController = GetComponent<CharacterController>();

        PlanarNormal = PlanarNormal;
	}
    public virtual void LateUpdate () {
        UpdatePawnRotation(Time.deltaTime);
        UpdatePawnVelocity(Time.deltaTime);
        UpdatePawnPosition(Time.deltaTime);
        UpdatePawnCondition();
    }
    public virtual void FixedUpdate() {
        //UpdatePawnVelocity(Time.fixedDeltaTime);
        //UpdatePawnPosition(Time.fixedDeltaTime);
        //UpdatePawnCondition();
    }

    protected virtual void UpdatePawnRotation(float aDeltaTime) {
        
    }
    protected virtual void UpdatePawnVelocity(float aDeltaTime) {
        Velocity += PawnGravity * aDeltaTime;
        Speed -= Mathf.Min(MoveFriction * aDeltaTime, Speed);
        Speed *= Mathf.Pow(1f - MoveDrag, aDeltaTime);
        
        if(IsJumping) {
            Velocity += JumpVector * JumpAcceleration * aDeltaTime;
        }
    }
    protected virtual void UpdatePawnPosition(float aDeltaTime) {
        
        MovementController.Move(Velocity * aDeltaTime);
        Velocity = MovementController.velocity;

        Vector3[] Points = new Vector3[8];
        for(int i = 0; i < 8; i++) {
            Points[i] = transform.position + transform.rotation * FromPlanar(new Vector3(0.5f - (i / 2) / 2, 0.5f - (i / 2) % 2, 0.5f - i % 2));
        }
        
        for(int i = 0; i < 8; i++) {
            for(int j = i + 1; j < 8; j++) {
                Debug.DrawLine(Points[i], Points[j], Color.green, Time.deltaTime);
            }
            
        }
    }
    protected virtual void UpdatePawnCondition() {
        // Default to Air and PlanarNormal as up.
        PlanarNormal = Vector3.up;
        MoveCondition = PawnMoveCondition.Air;

        float Radius = MovementController.radius + MovementController.skinWidth;
        Vector3 BottomCenter = transform.position + MovementController.center - new Vector3(0, MovementController.height / 2 - Radius, 0);
        Vector3 TopCenter = transform.position + MovementController.center + new Vector3(0, MovementController.height / 2 - Radius, 0);
        float RayOffset = 0.25f;

        float WaterSkinDepth = 0.3f;

        // Water
        if(Physics.CheckCapsule(TopCenter, BottomCenter, Radius - WaterSkinDepth, WaterMask, QueryTriggerInteraction.Collide)) {

            float SwimYaw = ControlRotation.eulerAngles.y;
            Quaternion SwimRotation = new Quaternion();
            SwimRotation.eulerAngles = new Vector3(0, SwimYaw, 0);

            PlanarNormal = SwimRotation * Vector3.up;
            MoveCondition = PawnMoveCondition.Water;

            if(MoveState != PawnMoveState.Swimming) {
                SwimStart();
            }

        // Ground
        } else {
            if(MoveState == PawnMoveState.Swimming) {
                SwimEnd();
            }
            RaycastHit[] GroundCheckHits = Physics.SphereCastAll(BottomCenter, Radius, Vector3.down, RayOffset, GroundMask, QueryTriggerInteraction.Ignore);
            
            foreach(RaycastHit GroundCheck in GroundCheckHits) {
                if(GroundCheck.collider.gameObject != gameObject) {
                    PlanarNormal = GroundCheck.normal;
                    MoveCondition = PawnMoveCondition.Ground;
                    Debug.DrawLine(GroundCheck.point, GroundCheck.point + PlanarNormal, Color.red, 2f);
                    break;
                }
            }
        }
    }


    public virtual void CancelMoveState() {
        switch(MoveState.Value) {
            case PawnMoveState.Crouching:
                break;
        }

        MoveState = PawnMoveState.Walking;
    }

    #region Event Recievers

    public virtual void MoveInput(Vector3 aInputVector, float aDeltaTime) {
        if (!RecieveInput) {
            return;
        }

        Vector3 CleanInput = IsInWater ? aInputVector.normalized : new Vector3(aInputVector.x, 0, aInputVector.z).normalized;
        Vector3 ForwardAdd = CleanInput * MoveAcceleration * aDeltaTime;
        Vector3 NewPlanarForwardVelocity = PlanarForwardVelocity + ForwardAdd;

        Vector3 DebugCurrentVelocityPosition = transform.position + transform.rotation * FromPlanar(PlanarForwardVelocity);

        Debug.DrawLine(transform.position, DebugCurrentVelocityPosition, Color.blue, 0.1f);
        Debug.DrawLine(DebugCurrentVelocityPosition, transform.position + transform.rotation * FromPlanar(NewPlanarForwardVelocity), Color.magenta, 0.1f);

        if (aInputVector != Vector3.zero) {
            Debug.DrawLine(transform.position, transform.position + new Vector3(0, 0.25f, 0), Color.cyan, 2f);
        }

        // If the new speed is within current limits
        if(NewPlanarForwardVelocity.magnitude <= MaxControlSpeed) {
            PlanarForwardVelocity = NewPlanarForwardVelocity;
        // Else if the old speed was within current limits, add only up to control speed, new vector direction intact though.
        } else if(PlanarForwardVelocity.magnitude <= MaxControlSpeed) {
            PlanarForwardVelocity = NewPlanarForwardVelocity.normalized * MaxControlSpeed;
        // Else when old speed was over MaxControlSpeed
        } else {
            PlanarForwardVelocity += (Vector3.Dot(ForwardAdd, PlanarForwardVelocity) > 0 ? Vector3.zero : Vector3.Project(ForwardAdd, PlanarForwardVelocity)) + Vector3.ProjectOnPlane(ForwardAdd, PlanarForwardVelocity);
        }

        
    }
    public virtual void ViewInput(Quaternion aInputQuat, float aDeltaTime) {
        if (!RecieveInput) {
            return;
        }

        ControlRotation = aInputQuat;

        if(BodyUseControllerPitch || BodyUseControllerRoll || BodyUseControllerYaw) {
            Vector3 BaseEuler = transform.rotation.eulerAngles;
            Vector3 ControlEuler = aInputQuat.eulerAngles;

            Quaternion BodyRotation = new Quaternion();
            BodyRotation.eulerAngles = new Vector3(
                BodyUseControllerPitch ? ControlEuler.x : BaseEuler.x,
                BodyUseControllerYaw ? ControlEuler.y : BaseEuler.y,
                BodyUseControllerRoll ? ControlEuler.z : BaseEuler.z
            );

            transform.rotation = BodyRotation;
        }
    }

    public virtual void JumpStart() {
        if (!RecieveInput) {
            return;
        }

        if (IsGrounded) {
            JumpState = true;
            LastJumpStart = Time.time;
            JumpVector = PlanarNormal;
        }
    }
    public virtual void JumpEnd() {
        JumpState = false;
    }
    public virtual void CrouchStart() {
        if (!RecieveInput) {
            return;
        }

        if (MoveState == PawnMoveState.Walking) {
            MoveState = PawnMoveState.Crouching;
        }
    }
    public virtual void CrouchEnd() {
        if (MoveState == PawnMoveState.Crouching) {
            CancelMoveState();
        }
    }
    public virtual void SprintStart() {
        if (!RecieveInput) {
            return;
        }

        if (MoveState == PawnMoveState.Walking) {
            MoveState = PawnMoveState.Sprinting;
        }
    }
    public virtual void SprintEnd() {
        if (MoveState == PawnMoveState.Sprinting) {
            CancelMoveState();
        }
    }
    public virtual void SwimStart() {
        CancelMoveState();
        MoveState = PawnMoveState.Swimming;
    }
    public virtual void SwimEnd() {
        CancelMoveState();
    }
    #endregion
}
