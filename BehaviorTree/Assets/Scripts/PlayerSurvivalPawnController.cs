using UnityEngine;
using System.Collections;

public class PlayerSurvivalPawnController : SurvivalPawnController<SurvivalPawn> {
    
    public virtual void Update () {
        if (Input.GetButtonDown("Jump")) {
            CallJumpStart();
        }
        if (Input.GetButtonUp("Jump")) {
            CallJumpEnd();
        }
        if (Input.GetButtonDown("Sprint")) {
            CallSprintStart();
        }
        if (Input.GetButtonUp("Sprint")) {
            CallSprintEnd();
        }
        if (Input.GetButtonDown("Crouch")) {
            CallCrouchStart();
        }
        if (Input.GetButtonUp("Crouch")) {
            CallCrouchEnd();
        }
        if (Input.GetButtonDown("Fire1")) {
            CallAttackStart();
        }
        if (Input.GetButtonUp("Fire1")) {
            CallAttackEnd();
        }
        if (Input.GetButtonDown("Interact")) {
            CallInteractStart();
        }
        if (Input.GetButtonUp("Interact")) {
            CallInteractEnd();
        }

        RawMoveInput += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Swim Up"), Input.GetAxis("Vertical"));
        
        if (RawMoveInput != Vector3.zero) {
            CallMove(ConsumeMoveInput(), Time.deltaTime);
        }

        float ViewInputYaw = Input.GetAxis("Mouse X");
        float ViewInputPitch = Input.GetAxis("Mouse Y");

        if (ViewInputYaw != 0 || ViewInputPitch != 0) {
            ControlRotationYaw += ViewInputYaw;
            //ControlRotationPitch = Mathf.Clamp(ControlRotationPitch + ViewInputPitch, -89, 89);
            ControlRotationPitch = ControlRotationPitch + ViewInputPitch;

            CallView(ControlRotationQuat, Time.deltaTime);
        }
    }
}
