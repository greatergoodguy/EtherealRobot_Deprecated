using UnityEngine;
using System.Collections;

public class KeyboardInput : Input_BB {
	public override bool GetButton_Accel() {
    	return Input.GetKey(KeyCode.Space);
   	}  
	
	
	public override bool GetButton_Jump() {
    	return true;
   	}  
	
	
	public override bool GetButton_Debug() {
    	return true;
   	}  
	
	public override bool GetButton_Brake() {
    	return Input.GetKey(KeyCode.LeftControl);
   	}  
	
	public override bool GetButtonDown_Pause() {
    	return Input.GetKey(KeyCode.P);
   	}  
	
	public override float GetAxis_MouseX() {
		return Input.GetAxis("Mouse X");
	}
}