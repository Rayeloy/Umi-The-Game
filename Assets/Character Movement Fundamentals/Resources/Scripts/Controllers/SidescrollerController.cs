using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This controller script is based on 'BasicWalkerController' and limits player movement to a 2D plane;
//It can be used to build 2D platformers or other games using 2D controls;
public class SidescrollerController : BasicWalkerController {

	//Calculate movement direction based on player input;
	protected override Vector3 CalculateMovementDirection()
	{
		float _horizontalInput;

		//Get input;
		if(useRawInput){
			_horizontalInput = Input.GetAxisRaw(horizontalInputAxis);
		} else {
			_horizontalInput = Input.GetAxis(horizontalInputAxis);
		}

		Vector3 _velocity = Vector3.zero;

		//Add horizontal movement;
		_velocity += tr.right * _horizontalInput;

		return _velocity;
	}
}
