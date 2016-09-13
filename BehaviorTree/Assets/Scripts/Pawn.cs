using UnityEngine;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
public class Pawn : MonoBehaviour {
    private CharacterController mMovementController = null;
    protected CharacterController MovementController {
        get { return mMovementController; }
        private set { mMovementController = value; }
    }
    public virtual Controller<Pawn> PawnController {
        get { return GetComponent<Controller<Pawn>>(); }
    }
    

    [SerializeField]
    private float mSpeed = 0f;
    [SerializeField]
    private Vector3 mDirection = Vector3.forward;
    [SerializeField]
    private Vector2 mPlanarDirection = Vector2.up;
    [SerializeField]
    private Vector3 mPlanarNormal = Vector3.up;
    [SerializeField]
    private Vector3 mConstantAcceleration = Physics.gravity;

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
    /// Projects and rotates a given vector by the Pawn's PlanarNormal, returning back the vector on the 2d-plane defined by the PlanarNormal.
    /// </summary>
    /// <param name="aVector">The vector to project and rotate.</param>
    /// <returns>The vector given on the 2d-plane.</returns>
    public Vector2 ToPlanar(Vector3 aVector) {
        // Project the Direction onto the plane defined by PlanarNormal. Rotate that projection to the plane's coordinate system.
        Vector3 ProjectedRotated = Quaternion.FromToRotation(Vector3.up, PlanarNormal) * Vector3.ProjectOnPlane(aVector, PlanarNormal);

        // Note, Vector3.forward is represented as Vector2.up here.
        return new Vector2(ProjectedRotated.x, ProjectedRotated.z);
    }
    /// <summary>
    /// Rotating a given 2d-vector from the plane defined by the PlanarNormal to the global coordinate system, returning a 3d-vector.
    /// </summary>
    /// <param name="aVector">The vector to rotate.</param>
    /// <returns>The 3d representation of the 2d-vector on the PlanarNormal.</returns>
    public Vector3 FromPlanar(Vector2 aVector) {
        // Rotate the vector to global coordinate system.
        return Quaternion.FromToRotation(PlanarNormal, Vector3.up) * new Vector3(aVector.x, 0, aVector.y);
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
    public Vector2 PlanarForwardVelocity {
        get { return ToPlanar(ForwardVelocity); }
        set {
            Vector3 InputForward = FromPlanar(value);
            ForwardVelocity = InputForward + (ForwardVelocity - FromPlanar(PlanarForwardVelocity));
        }
    }

    /// <summary>
    /// Constant acceleration applied onto the pawn's Velocity, in m/s². Defaults to 'Physics.gravity'.
    /// </summary>
    public Vector3 ConstantAcceleration {
        get { return mConstantAcceleration; }
        set { mConstantAcceleration = value; }
    }

	// Use this for initialization
	void Start () {
        MovementController = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
        Ray GroundCheck = new Ray(transform.position, Vector3.down * 2);
        RaycastHit[] Hits = Physics.RaycastAll(GroundCheck);
        PlanarNormal = Vector3.up;
        foreach(RaycastHit Hit in Hits) {
            if(Hit.collider.gameObject.tag != "Player") {
                PlanarNormal = Hit.normal;
                break;
            }
        }

        Vector2 Input = PawnController.ConsumeInput();

        PlanarForwardVelocity += Input;
        Velocity += PlanarNormal * (PawnController.InputJump ? 10 : 0);
    }

    void FixedUpdate() {

        Velocity += ConstantAcceleration * Time.fixedDeltaTime;

        CollisionFlags MoveCollisions = MovementController.Move(Velocity * Time.fixedDeltaTime);
        Velocity = MovementController.velocity;
        
    }
}
