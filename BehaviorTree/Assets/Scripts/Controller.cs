using UnityEngine;
using System.Collections;

public class Controller<T> : MonoBehaviour where T : Pawn {


    private Quaternion mControlRotation;
    /// <summary>
    /// The "aiming" of the Controller, as a global rotation.
    /// </summary>
    public Quaternion ControlRotation {
        get { return mControlRotation; }
        protected set { mControlRotation = value; }
    }
    /// <summary>
    /// The ControlRotation as modifiable euler angles.
    /// </summary>
    public Vector3 ControlEuler {
        get { return mControlRotation.eulerAngles; }
        protected set { mControlRotation.eulerAngles = value; }
    }
    
    private Vector2 mRawMoveInput = Vector2.zero;
    /// <summary>
    /// The actual taken input sum.
    /// </summary>
    public Vector2 RawMoveInput {
        get { return mRawMoveInput; }
        protected set { mRawMoveInput = value; }
    }
    /// <summary>
    /// Input adjusted to fit inside a unit circle.
    /// </summary>
    public Vector2 MoveInput {
        get { return RawMoveInput / Mathf.Max(1f, RawMoveInput.magnitude); }
    }

    private bool mInputJump = false;
    /// <summary>
    /// Current jump input state.
    /// </summary>
    public bool InputJump {
        get { return mInputJump; }
        set { mInputJump = value; }
    }

    private bool mIsGrounded = false;
    public bool IsGrounded {
        get { return mIsGrounded; }
        protected set { mIsGrounded = value; }
    }

    [SerializeField]
    private T mControlledPawn;
    public T ControlledPawn {
        get { return mControlledPawn; }
        set { mControlledPawn = value; }
    }
    
	// Use this for initialization
	void Start () {

	}

    /// <summary>
    /// Gets and resets accumulated movement input, clamped into a unit circle.
    /// </summary>
    /// <returns>The movement input, with maximum magnitude of 1.</returns>
    public Vector2 ConsumeMoveInput() {
        Vector2 ReturnVector = MoveInput;
        RawMoveInput = Vector2.zero;

        return ReturnVector;
    }
    public virtual void OnPawnMoved() {
        
    }
}
