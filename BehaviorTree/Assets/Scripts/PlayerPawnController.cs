using UnityEngine;
using System.Collections;

public class PlayerPawnController : Controller<Pawn> {
    
	// Update is called once per frame
	void Update () {
        RawMoveInput += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        InputJump = Input.GetButton("Jump");
        
        float ViewInputYaw = Input.GetAxis("Mouse X");
        float ViewInputPitch = Input.GetAxis("Mouse Y");

        ControlEuler = new Vector3(ControlEuler.x + ViewInputPitch, ControlEuler.y + ViewInputYaw, ControlEuler.z);
    }
}
