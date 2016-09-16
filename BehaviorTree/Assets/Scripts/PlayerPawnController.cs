using UnityEngine;
using System.Collections;

public class PlayerPawnController : Controller<Pawn> {
    
	// Update is called once per frame
	void Update () {
        if(Input.GetButtonDown("Jump")) {
            CallJumpStart();
        }
        if(Input.GetButtonUp("Jump")) {
            CallJumpEnd();
        }
        if(Input.GetButtonDown("Sprint")) {
            CallSprintStart();
        }
        if(Input.GetButtonUp("Sprint")) {
            CallSprintEnd();
        }
        if(Input.GetButtonDown("Crouch")) {
            CallCrouchStart();
        }
        if(Input.GetButtonUp("Crouch")) {
            CallCrouchEnd();
        }
        
        RawMoveInput += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Swim Up"), Input.GetAxis("Vertical"));
        if(RawMoveInput != Vector3.zero) {
            CallMove(ConsumeMoveInput(), Time.deltaTime);
        }

        float ViewInputYaw = Input.GetAxis("Mouse X");
        float ViewInputPitch = Input.GetAxis("Mouse Y");

        if(ViewInputYaw != 0 || ViewInputPitch != 0) {
            ControlRotationYaw += ViewInputYaw;
            //ControlRotationPitch = Mathf.Clamp(ControlRotationPitch + ViewInputPitch, -89, 89);
            ControlRotationPitch = ControlRotationPitch + ViewInputPitch;

            CallView(ControlRotationQuat, Time.deltaTime);
        }

        //ControlRotationEuler = new Vector3(ControlRotationEuler.x + ViewInputPitch, ControlRotationEuler.y + ViewInputYaw, ControlRotationEuler.z);
    }
}
