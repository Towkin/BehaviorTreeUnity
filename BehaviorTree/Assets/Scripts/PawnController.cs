using UnityEngine;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class PawnController<T> : MonoBehaviour where T : Pawn {


    private Quaternion mControlRotation;
    /// <summary>
    /// The "aiming" of the Controller, as a global rotation.
    /// </summary>
    public Quaternion ControlRotationQuat {
        get { return mControlRotation; }
        protected set { mControlRotation = value; }
    }
    /// <summary>
    /// The ControlRotation as modifiable euler angles.
    /// </summary>
    public Vector3 ControlRotationEuler {
        get { return mControlRotation.eulerAngles; }
        protected set { mControlRotation.eulerAngles = value; }
    }
    public float ControlRotationPitch {
        get { return mControlRotation.eulerAngles.x; }
        protected set { mControlRotation.eulerAngles = new Vector3(value, ControlRotationYaw, ControlRotationRoll); }
    }
    public float ControlRotationYaw {
        get { return mControlRotation.eulerAngles.y; }
        protected set { mControlRotation.eulerAngles = new Vector3(ControlRotationPitch, value, ControlRotationRoll); }
    }
    public float ControlRotationRoll {
        get { return mControlRotation.eulerAngles.z; }
        protected set { mControlRotation.eulerAngles = new Vector3(ControlRotationPitch, ControlRotationYaw, value); }
    }

    private Vector3 mRawMoveInput = Vector3.zero;
    /// <summary>
    /// The actual taken input sum.
    /// </summary>
    public Vector3 RawMoveInput {
        get { return mRawMoveInput; }
        protected set { mRawMoveInput = value; }
    }
    /// <summary>
    /// Input adjusted to fit inside a unit circle.
    /// </summary>
    public Vector3 MoveInput {
        get { return RawMoveInput / Mathf.Max(1f, RawMoveInput.magnitude); }
    }
    //private bool mInputJump = false;
    /// <summary>
    /// Current jump input state.
    /// </summary>
    //public bool InputJump {
    //    get { return mInputJump; }
    //    set { mInputJump = value; }
    //}
    
    protected delegate void InputButtonEvent();
    protected delegate void InputFloatEvent(float aInput, float aDeltaTime);
    protected delegate void InputVectorEvent(Vector3 aInput, float aDeltaTime);
    protected delegate void InputQuatEvent(Quaternion aInput, float aDeltaTime);

    protected event InputVectorEvent eOnMove;           protected void CallMove(Vector3 aInput, float aDeltaTime)       { if(eOnMove != null) eOnMove(aInput, aDeltaTime); }
    protected event InputQuatEvent eOnView;             protected void CallView(Quaternion aInput, float aDeltaTime)    { if(eOnView != null) eOnView(aInput, aDeltaTime); }

    protected event InputButtonEvent eOnJumpStart;      protected void CallJumpStart()      { if(eOnJumpStart != null)      eOnJumpStart(); }
    protected event InputButtonEvent eOnJumpEnd;        protected void CallJumpEnd()        { if(eOnJumpEnd != null)        eOnJumpEnd(); }
    protected event InputButtonEvent eOnSprintStart;    protected void CallSprintStart()    { if(eOnSprintStart != null)    eOnSprintStart(); }
    protected event InputButtonEvent eOnSprintEnd;      protected void CallSprintEnd()      { if(eOnSprintEnd != null)      eOnSprintEnd(); }
    protected event InputButtonEvent eOnCrouchStart;    protected void CallCrouchStart()    { if(eOnCrouchStart != null)    eOnCrouchStart(); }
    protected event InputButtonEvent eOnCrouchEnd;      protected void CallCrouchEnd()      { if(eOnCrouchEnd != null)      eOnCrouchEnd(); }

    [SerializeField]
    private List<T> mInitialControlledPawns = new List<T>();
    private List<T> mControlledPawns = new List<T>();
    /// <summary>
    /// A read only collection of the controlled pawns. Use AddPawn and RemovePawn to modify the list.
    /// </summary>
    public ReadOnlyCollection<T> ControlledPawns {
        get { return mControlledPawns.AsReadOnly(); }
    }

    /// <summary>
    /// Adds a pawn to the list of controlled pawns. Adds event connections to the pawn's functions.
    /// </summary>
    /// <param name="New Pawn"></param>
    public void AddPawn(T aNewPawn) {
        if(!mControlledPawns.Contains(aNewPawn)) {
            mControlledPawns.Add(aNewPawn);

            // Events
            AddEvents(aNewPawn);
        }
    }
    
    /// <summary>
    /// Removes a pawn from the list of controlled pawns, if the list contains it. Removes the event connections.
    /// </summary>
    /// <param name="aRemovePawn"></param>
    public void RemovePawn(T aRemovePawn) {
        if(mControlledPawns.Contains(aRemovePawn)) {
            mControlledPawns.Remove(aRemovePawn);

            // Events
            RemoveEvents(aRemovePawn);
        }
    }
    protected virtual void AddEvents(T aPawn) {
        eOnMove += aPawn.MoveInput;
        eOnView += aPawn.ViewInput;

        eOnJumpStart += aPawn.JumpStart;
        eOnJumpEnd += aPawn.JumpEnd;
        eOnSprintStart += aPawn.SprintStart;
        eOnSprintEnd += aPawn.SprintEnd;
        eOnCrouchStart += aPawn.CrouchStart;
        eOnCrouchEnd += aPawn.CrouchEnd;
    }
    protected virtual void RemoveEvents(T aPawn) {
        eOnMove -= aPawn.MoveInput;
        eOnView -= aPawn.ViewInput;

        eOnJumpStart -= aPawn.JumpStart;
        eOnJumpEnd -= aPawn.JumpEnd;
        eOnSprintStart -= aPawn.SprintStart;
        eOnSprintEnd -= aPawn.SprintEnd;
        eOnCrouchStart -= aPawn.CrouchStart;
        eOnCrouchEnd -= aPawn.CrouchEnd;
    }

    void OnEnable() {
        foreach(T InitialPawn in mInitialControlledPawns) {
            AddPawn(InitialPawn);
        }

        // Ugly but simple way to automatically add the Pawn Component of the current game object, if no other is specified.
        if(ControlledPawns.Count == 0 && GetComponent<T>()) {
            AddPawn(GetComponent<T>());
        }
    }
    void OnDisable() {
        while (ControlledPawns.Count > 0) {
            RemovePawn(ControlledPawns[0]);
        }
    }
    
    /// <summary>
    /// Gets and resets accumulated movement input, clamped into a unit sphere.
    /// </summary>
    /// <returns>The movement input, with maximum magnitude of 1.</returns>
    protected Vector3 ConsumeMoveInput() {
        Vector3 ReturnVector = MoveInput;
        RawMoveInput = Vector3.zero;

        return ReturnVector;
    }
}
 