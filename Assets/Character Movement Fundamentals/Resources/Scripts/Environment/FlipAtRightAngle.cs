using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script flips any rigidbody (which also has a 'BasicWalkerController' attached) that touches its trigger around a 90 degree angle;
public class FlipAtRightAngle : MonoBehaviour {

	//Audiosource component which is played when switch is triggered;
	AudioSource audioSource;

	Transform tr;

	void Start()
	{
		//Get component references;
		tr = transform;
		audioSource = GetComponent<AudioSource>();
	}

	void OnTriggerEnter(Collider col)
	{
		if(col.GetComponent<BasicWalkerController>() == null)
			return;

		SwitchDirection(tr.forward, col.GetComponent<BasicWalkerController>());
	}

	void SwitchDirection(Vector3 _newUpDirection, BasicWalkerController _controller)
	{
		float _angleThreshold = 0.001f;

		//Calculate angle;
		float _angleBetweenUpDirections = Vector3.Angle(_newUpDirection, _controller.transform.up);

		//If angle between new direction and current rigidbody rotation is too small, return;
		if(_angleBetweenUpDirections < _angleThreshold)
			return;

		//Play audio cue;
		audioSource.Play();

		Transform _transform = _controller.transform;

		//Rotate gameobject;
		Quaternion _rotationDifference = Quaternion.FromToRotation(_transform.up, _newUpDirection);
		_transform.rotation = _rotationDifference * _transform.rotation;
	}
}