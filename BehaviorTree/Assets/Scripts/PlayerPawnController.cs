﻿using UnityEngine;
using System.Collections;

public class PlayerPawnController : PawnController<Pawn> {
	
	// Update is called once per frame
	public override void Update () {

		if(Input.GetButtonDown("Fire1")) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		if(Input.GetKeyDown(KeyCode.Escape)) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}

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
		
		float ViewInputYaw = Input.GetAxis("Mouse X");
		float ViewInputPitch = Input.GetAxis("Mouse Y");

		if(ViewInputYaw != 0 || ViewInputPitch != 0) {
			ControlRotationYaw += ViewInputYaw;
			//ControlRotationPitch = Mathf.Clamp(ControlRotationPitch + ViewInputPitch, -89, 89);
			ControlRotationPitch = ControlRotationPitch + ViewInputPitch;
		}

		base.Update();
	}
}
