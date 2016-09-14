using UnityEngine;
using System.Collections;

public class PawnHead : MonoBehaviour {

    [SerializeField]
    private Pawn mBody;
    public Pawn Body {
        get { return mBody; }
    }
    
	// Update is called once per frame
	void Update () {
        transform.rotation = Body.PawnController.ControlRotation;
	}
}
