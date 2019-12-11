using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This script rotates all gameobjects inside the attached trigger collider around a central axis (the forward axis of this gameobject);
//In combination with a tube-shaped collider, this script can be used to let a player walk around on the inside walls of a tunnel;
public class GravityTunnel : MonoBehaviour {

	//List of rigidbodies inside the attached trigger;
	List<Rigidbody> rigidbodies = new List<Rigidbody>();

	void FixedUpdate ()
	{
		foreach (Rigidbody _rigidbody in rigidbodies)
		{
			//Calculate center position based on rigidbody position;
			Vector3 _center = 
				Vector3.Project((_rigidbody.transform.position - transform.position) ,((transform.position + transform.forward) - transform.position)) + transform.position;
			
			RotateRigidbody(_rigidbody.transform, (_center - _rigidbody.transform.position).normalized);
		}
	}

	void OnTriggerEnter(Collider col)
	{
		Rigidbody _rigidbody = col.GetComponent<Rigidbody>();
		if(!_rigidbody)
			return;

		rigidbodies.Add(_rigidbody);
	}

	void OnTriggerExit(Collider col)
	{
		Rigidbody _rigidbody = col.GetComponent<Rigidbody>();
		if(!_rigidbody)
			return;

		rigidbodies.Remove(_rigidbody);

		RotateRigidbody(_rigidbody.transform, Vector3.up);

		//Reset rigidbody rotation;
		Vector3 _eulerAngles = _rigidbody.rotation.eulerAngles;

		_eulerAngles.z = 0f;
		_eulerAngles.x = 0f;

		_rigidbody.MoveRotation(Quaternion.Euler(_eulerAngles));
	}

	void RotateRigidbody(Transform _transform, Vector3 _targetDirection)
	{
		//Get rigidbody component of transform;
		Rigidbody _rigidbody = _transform.GetComponent<Rigidbody>();
		
		_targetDirection.Normalize();

		//Calculate rotation difference;
		Quaternion _rotationDifference = Quaternion.FromToRotation(_transform.up, _targetDirection);

		//Save start and end rotation;
		Quaternion _startRotation = _transform.rotation;
		Quaternion _endRotation = _rotationDifference * _transform.rotation;

		//If the angle between the transform's up vector and target direction exceeds 90 degrees, use a different method to rotate the transform;
		//This ensures that the camera will at least stay pointed at the same general direction when being rotated;
		if(Vector3.Dot(_transform.up, _targetDirection) < -0.001f)
		{
			//Get facing direction of camera;
			Vector3 _facingDirection = _transform.GetComponentInChildren<CameraController>().GetFacingDirection();

			Quaternion _counterRotation = GetCounterRotation(_rotationDifference);
			Quaternion _cameraRotationDifference = Quaternion.AngleAxis(180f, _facingDirection);

			_endRotation = _cameraRotationDifference * _transform.rotation;
			_endRotation = _counterRotation * _endRotation;
		}

		//Rotate rigidbody;
		_rigidbody.MoveRotation(_endRotation);
	}

	//Calculate a counter rotation from a rotation;
	Quaternion GetCounterRotation(Quaternion _rotation)
	{
		Vector3 _axis;
		float _angle;

		_rotation.ToAngleAxis(out _angle, out _axis);
		Quaternion _rotationAdd = Quaternion.AngleAxis( Mathf.Sign(_angle) * 180f, _axis);

		return _rotation * Quaternion.Inverse(_rotationAdd);
	}
}
