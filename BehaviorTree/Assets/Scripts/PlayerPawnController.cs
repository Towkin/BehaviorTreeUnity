using UnityEngine;
using System.Collections;

public class PlayerPawnController : Controller<Pawn> {
    
	// Update is called once per frame
	void Update () {
        RawMoveInput += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        InputJump = Input.GetButton("Jump");
        
        float ViewInputYaw = Input.GetAxis("Mouse X");
        float ViewInputPitch = Input.GetAxis("Mouse Y");

        ControlRotationYaw += ViewInputYaw;
        ControlRotationPitch = Mathf.Clamp(ControlRotationPitch + ViewInputPitch, -89, 89);
        
        //ControlRotationEuler = new Vector3(ControlRotationEuler.x + ViewInputPitch, ControlRotationEuler.y + ViewInputYaw, ControlRotationEuler.z);
    }
}
