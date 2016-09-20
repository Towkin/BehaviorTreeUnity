using UnityEngine;
using System.Collections;

public class PawnHead : MonoBehaviour {

    [SerializeField]
    private Pawn mBody;
    public Pawn Body {
        get { return mBody; }
    }

    private Vector3 mOffset;

    void Start () {
        mOffset = transform.localPosition;
    }
    
	// Update is called once per frame
	void Update () {
        if (Body) {
            transform.rotation = Body.ControlRotation;
            transform.localPosition = mOffset + Vector3.up * Mathf.Sin(Time.time * 8) * 0.01f * Body.ForwardVelocity.magnitude;
        }
	}
}
