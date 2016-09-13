using UnityEngine;
using System.Collections;

public class Controller<T> : MonoBehaviour where T : Pawn {

    [SerializeField]
    private Vector2 mInputVector = Vector2.zero;
    public Vector2 InputVector {
        get { return mInputVector; }
        set { mInputVector = value; }
    }
    [SerializeField]
    private bool mInputJump = false;
    public bool InputJump {
        get { return mInputJump; }
        set { mInputJump = value; }
    }

    [SerializeField]
    private T mControlledPawn;
    public T ControlledPawn {
        get { return mControlledPawn; }
        set { mControlledPawn = value; }
    }
    
	// Use this for initialization
	void Start () {
        ControlledPawn = GetComponent<T>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate() {
        
    }

    public Vector2 ConsumeInput() {
        Vector2 ReturnVector = InputVector;
        InputVector = Vector2.zero;

        return ReturnVector;
    }
}
