/************************************************************************************

Filename    :   OVRPlayerController.cs
Content     :   Player controller interface. 
				This script drives OVR camera as well as controls the locomotion
				of the player, and handles physical contact in the world.	
Created     :   January 8, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]

//-------------------------------------------------------------------------------------
// ***** OVRPlayerController
//
// OVRPlayerController implements a basic first person controller for the Rift. It is 
// attached to the OVRPlayerController prefab, which has an OVRCameraController attached
// to it. 
// 
// The controller will interact properly with a Unity scene, provided that the scene has
// collision assigned to it. 
//
// The OVRPlayerController prefab has an empty GameObject attached to it called 
// ForwardDirection. This game object contains the matrix which motor control bases it
// direction on. This game object should also house the body geometry which will be seen
// by the player.
//
public class MechProtoPC : PlayerController
{
	protected CharacterController 	characterController = null;
	protected OVRCameraController 	CameraController 	= null;

	public float Acceleration 	 = 0.1f;
	public float Damping 		 = 0.15f;
	public float JumpForce 		 = 0.3f;
	public float RotationAmount  = 1.5f;
	public float GravityModifier = 0.379f;
		
	private float   MoveScale 	 = 1.0f;
	private Vector3 MoveThrottle = Vector3.zero;
	private float   FallSpeed 	 = 0.0f;
	
	// Initial direction of controller (passed down into CameraController)
	private Quaternion OrientationOffset = Quaternion.identity;			
	// Rotation amount from inputs (passed down into CameraController)
	private float 	YRotation 	 = 0.0f;
	
	// Transfom used to point player in a given direction; 
	// We should attach objects to this if we want them to rotate 
	// separately from the head (i.e. the body)
	protected Transform DirXform = null;
	
	// We can adjust these to influence speed and rotation of player controller
	private float MoveScaleMultiplier     = 6.0f; 
	private float RotationScaleMultiplier = 1.0f; 
	
	//
	// STATIC VARIABLES
	//
	public static bool  AllowMouseRotation      = true;
	
	private GameObject mechGO = null;
 	
	// * * * * * * * * * * * * *
	
	// Awake
	new public virtual void Awake(){
		base.Awake();
		
		// We use Controller to move player around
		characterController = gameObject.GetComponent<CharacterController>();
		
		if(characterController == null)
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");
					
		// We use OVRCameraController to set rotations to cameras, 
		// and to be influenced by rotation
		OVRCameraController[] CameraControllers;
		CameraControllers = gameObject.GetComponentsInChildren<OVRCameraController>();
		
		if(CameraControllers.Length == 0)
			Debug.LogWarning("OVRPlayerController: No OVRCameraController attached.");
		else if (CameraControllers.Length > 1)
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraController attached.");
		else
			CameraController = CameraControllers[0];	
	
		// Instantiate a Transform from the main game object (will be used to 
		// direct the motion of the PlayerController, as well as used to rotate
		// a visible body attached to the controller)
		DirXform = null;
		Transform[] Xforms = gameObject.GetComponentsInChildren<Transform>();
		
		for(int i = 0; i < Xforms.Length; i++)
		{
			if(Xforms[i].name == "ForwardDirection")
			{
				DirXform = Xforms[i];
				break;
			}
		}
		
		if(DirXform == null)
			Debug.LogWarning("OVRPlayerController: ForwardDirection game object not found. Do not use.");
	}

	// Start
	new public virtual void Start(){
		base.Start();
		
		InitializeInputs();	
		SetCameras();
		
		mechGO = transform.FindChild("Mech").gameObject;
		print("rotation: " + mechGO.transform.rotation);
		
		print("CameraController: " + CameraController.gameObject.transform.rotation);
	}
		
	// Update 
	new public virtual void Update(){
		base.Update();
		
		UpdateMovement();

		Vector3 moveDirection = Vector3.zero;
		
		float motorDamp = (1.0f + (Damping * DeltaTime));
		MoveThrottle.x /= motorDamp;
		MoveThrottle.y = (MoveThrottle.y > 0.0f) ? (MoveThrottle.y / motorDamp) : MoveThrottle.y;
		MoveThrottle.z /= motorDamp;

		moveDirection += MoveThrottle * DeltaTime;
		
		// Gravity
		if (characterController.isGrounded && FallSpeed <= 0)
			FallSpeed = ((Physics.gravity.y * (GravityModifier * 0.002f)));	
		else
			FallSpeed += ((Physics.gravity.y * (GravityModifier * 0.002f)) * DeltaTime);	

		moveDirection.y += FallSpeed * DeltaTime;

		// Offset correction for uneven ground
		float bumpUpOffset = 0.0f;
		
		if (characterController.isGrounded && MoveThrottle.y <= 0.001f)
		{
			bumpUpOffset = Mathf.Max(characterController.stepOffset, 
									 new Vector3(moveDirection.x, 0, moveDirection.z).magnitude); 
			moveDirection -= bumpUpOffset * Vector3.up;
		}			
	 
		Vector3 predictedXZ = Vector3.Scale((characterController.transform.localPosition + moveDirection), 
											 new Vector3(1, 0, 1));	
		
		// Move contoller
		characterController.Move(moveDirection);
		
		Vector3 actualXZ = Vector3.Scale(characterController.transform.localPosition, new Vector3(1, 0, 1));
		
		if (predictedXZ != actualXZ)
			MoveThrottle += (actualXZ - predictedXZ) / DeltaTime; 
		
		// Update rotation using CameraController transform, possibly proving some rules for 
		// sliding the rotation for a more natural movement and body visual
		UpdatePlayerForwardDirTransform();
	}
		
	// UpdateMovement
	//
	// COnsolidate all movement code here
	//
	static float sDeltaRotationOld = 0.0f;
	public virtual void UpdateMovement(){
		
		//print("CameraController: " + CameraController.gameObject.transform.rotation);
		mechGO.transform.rotation = CameraController.gameObject.transform.rotation;
		
		
		// Do not apply input if we are showing a level selection display
		if(OVRMainMenu.sShowLevels == false)
		{
			bool moveForward = false;
			bool moveLeft  	 = false;
			bool moveRight   = false;
			bool moveBack    = false;
				
			MoveScale = 1.0f;
			
			// * * * * * * * * * * *
			// Keyboard input
			
			// Move
			
			// WASD
			if (Input.GetKey(KeyCode.W)) moveForward = true;
			if (Input.GetKey(KeyCode.A)) moveLeft	 = true;
			if (Input.GetKey(KeyCode.S)) moveBack 	 = true; 
			if (Input.GetKey(KeyCode.D)) moveRight 	 = true; 
			// Arrow keys
			if (Input.GetKey(KeyCode.UpArrow))    moveForward = true;
			if (Input.GetKey(KeyCode.LeftArrow))  moveLeft 	  = true;
			if (Input.GetKey(KeyCode.DownArrow))  moveBack 	  = true; 
			if (Input.GetKey(KeyCode.RightArrow)) moveRight   = true; 
			
			if ( (moveForward && moveLeft) || (moveForward && moveRight) ||
				 (moveBack && moveLeft)    || (moveBack && moveRight) )
				MoveScale = 0.70710678f;
			
			// No positional movement if we are in the air
			if (!characterController.isGrounded)	
				MoveScale = 0.0f;
			
			MoveScale *= DeltaTime;
			
			// Compute this for key movement
			float moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;
			
			// Run!
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				moveInfluence *= 2.0f;
			
			if(DirXform != null)
			{
				if (moveForward)
					MoveThrottle += DirXform.TransformDirection(Vector3.forward * moveInfluence);
				if (moveBack)
					MoveThrottle += DirXform.TransformDirection(Vector3.back * moveInfluence);
				if (moveLeft)
					MoveThrottle += DirXform.TransformDirection(Vector3.left * moveInfluence);
				if (moveRight)
					MoveThrottle += DirXform.TransformDirection(Vector3.right * moveInfluence);
			}
			
			// Rotate
			
			// compute for key rotation
			float rotateInfluence = DeltaTime * RotationAmount * RotationScaleMultiplier;
			
			//reduce by half to avoid getting ill
			if (Input.GetKey(KeyCode.Q)) 
				YRotation -= rotateInfluence * 0.5f;  
			if (Input.GetKey(KeyCode.E)) 
				YRotation += rotateInfluence * 0.5f; 
		
			// * * * * * * * * * * *
			// Mouse input
			
			// Move
			
			// Rotate
			float deltaRotation = 0.0f;
			if(AllowMouseRotation == false)
				deltaRotation = Input.GetAxis("Mouse X") * rotateInfluence * 3.25f;
			
			float filteredDeltaRotation = (sDeltaRotationOld * 0.0f) + (deltaRotation * 1.0f);
			YRotation += filteredDeltaRotation;
			sDeltaRotationOld = filteredDeltaRotation;
			
			// * * * * * * * * * * *
			// XBox controller input	
			
			// Compute this for xinput movement
			moveInfluence = Acceleration * 0.1f * MoveScale * MoveScaleMultiplier;
			
			// Run!
			moveInfluence *= 1.0f + OVRGamepadController.GetTriggerLeft();
			
			// Move
			if(DirXform != null)
			{
				if(OVRGamepadController.GetAxisLeftY() > 0.0f)
		    		MoveThrottle += OVRGamepadController.GetAxisLeftY() *
					DirXform.TransformDirection(Vector3.forward * moveInfluence);
				
				if(OVRGamepadController.GetAxisLeftY() < 0.0f)
		    		MoveThrottle += Mathf.Abs(OVRGamepadController.GetAxisLeftY()) *		
					DirXform.TransformDirection(Vector3.back * moveInfluence);
				
				if(OVRGamepadController.GetAxisLeftX() < 0.0f)
		    		MoveThrottle += Mathf.Abs(OVRGamepadController.GetAxisLeftX()) *
					DirXform.TransformDirection(Vector3.left * moveInfluence);
				
				if(OVRGamepadController.GetAxisLeftX() > 0.0f)
					MoveThrottle += OVRGamepadController.GetAxisLeftX() *
					DirXform.TransformDirection(Vector3.right * moveInfluence);
			}
			
			// Rotate
			YRotation += OVRGamepadController.GetAxisRightX() * rotateInfluence;    
		}
		
		// Update cameras direction and rotation
		SetCameras();

	}

	// UpdatePlayerControllerRotation
	// This function will be used to 'slide' PlayerController rotation around based on 
	// CameraController. For now, we are simply copying the CameraController rotation into 
	// PlayerController, so that the PlayerController always faces the direction of the 
	// CameraController. When we add a body, this will change a bit..
	public virtual void UpdatePlayerForwardDirTransform(){
		if ((DirXform != null) && (CameraController != null))
			DirXform.rotation = CameraController.transform.rotation;
	}
	
	///////////////////////////////////////////////////////////
	// PUBLIC FUNCTIONS
	///////////////////////////////////////////////////////////
	
	// Jump
	public bool Jump(){
		if (!characterController.isGrounded)
			return false;

		MoveThrottle += new Vector3(0, JumpForce, 0);

		return true;
	}

	// Stop
	public void Stop(){
		characterController.Move(Vector3.zero);
		MoveThrottle = Vector3.zero;
		FallSpeed = 0.0f;
	}	
	
	// InitializeInputs
	public void InitializeInputs(){
		// Get our start direction
		OrientationOffset = transform.rotation;
		// Make sure to set y rotation to 0 degrees
		YRotation = 0.0f;
	}
	
	// SetCameras
	public void SetCameras(){
		if(CameraController != null)
		{
			// Make sure to set the initial direction of the camera 
			// to match the game player direction
			CameraController.SetOrientationOffset(OrientationOffset);
			CameraController.SetYRotation(YRotation);
		}
	}
	
	// Get/SetMoveScaleMultiplier
	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier){
		moveScaleMultiplier = MoveScaleMultiplier;
	}
	
	public void SetMoveScaleMultiplier(float moveScaleMultiplier){
		MoveScaleMultiplier = moveScaleMultiplier;
	}
	
	// Get/SetRotationScaleMultiplier
	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier){
		rotationScaleMultiplier = RotationScaleMultiplier;
	}
	
	public void SetRotationScaleMultiplier(float rotationScaleMultiplier){
		RotationScaleMultiplier = rotationScaleMultiplier;
	}

	public override string GetControllerName() {
    	return "Default Oculus PC";
   	}  
}