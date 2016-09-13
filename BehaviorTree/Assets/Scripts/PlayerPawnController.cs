using UnityEngine;
using System.Collections;

public class PlayerPawnController : Controller<Pawn> {
    
	// Update is called once per frame
	void Update () {
        InputVector += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * Time.deltaTime * 10f;
        InputJump = Input.GetButtonDown("Jump");
	}
}
