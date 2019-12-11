using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This controller script is based on 'BasicWalkerController' and uses a 'CameraController' script to calculate the player's movement direction;
//The 'CameraController' script must be located on a child object of this gameobject;
//This script can be used for a range of different games (third-person platformers, first-person shooters,[...]);
public class CameraWalkerController : BasicWalkerController {

	//Reference to camera controls;
	CameraController cameraControls;

	protected override void Setup()
	{
		//Search for camera controller reference in this gameobjects' children;
		cameraControls = GetComponentInChildren<CameraController>();
	}

	//Calculate movement direction based on camera controller orientation;
	protected override Vector3 CalculateMovementDirection()
	{
		float _horizontalInput;
		float _verticalInput;

		//Get input;
		if(useRawInput){
			_horizontalInput = Input.GetAxisRaw(horizontalInputAxis);
			_verticalInput = Input.GetAxisRaw(verticalInputAxis);
		} else {
			_horizontalInput = Input.GetAxis(horizontalInputAxis);
			_verticalInput = Input.GetAxis(verticalInputAxis);
		}

		Vector3 _direction = Vector3.zero;

		//Use camera axes to construct movement direction;
		_direction += cameraControls.GetFacingDirection() * _verticalInput;
		_direction += cameraControls.GetStrafeDirection() * _horizontalInput;

		//Clamp movement vector to magnitude of '1f';
		if(_direction.magnitude > 1f)
			_direction.Normalize();

		return _direction;
	}

	//Returns reference to camera controller;
	public CameraController GetCameraController()
	{
		return cameraControls;
	}
}
