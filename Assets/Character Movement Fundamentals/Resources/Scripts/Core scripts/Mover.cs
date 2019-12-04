using UnityEngine;
using System.Collections;

//This script handles all physics, collision detection and ground detection;
//It expects a movement velocity (via 'SetVelocity') every 'FixedUpdate' frame from an external script (like a controller script) to work;
//It also provides several getter methods for important information (whether the mover is grounded, the current surface normal [...]);
[ExecuteInEditMode]
public class Mover : MonoBehaviour {

	//Collider variables;
	[Range(0f, 1f)] public float stepHeightRatio = 0.25f;
	[SerializeField] public float colliderHeight = 2f;
	[SerializeField] public float colliderThickness = 1f;
	[SerializeField] public Vector3 colliderOffset = Vector3.zero;

	//References to attached collider(s);
	BoxCollider boxCollider;
	SphereCollider sphereCollider;
	CapsuleCollider capsuleCollider;

	//Sensor variables;
	public Sensor.CastType sensorType = Sensor.CastType.Raycast;
	private float sensorRadiusModifier = 0.8f;
	public LayerMask sensorLayermask  = ~0;
	public bool isInDebugMode = false;
	public int sensorArrayRows = 1;
	public int sensorArrayRayCount = 6;
	public bool sensorArrayRowsAreOffset = false;

	//Ground detection variables;
	bool isGrounded = false;

	//Sensor range variables;
	bool IsUsingExtendedSensorRange  = true;
	float baseSensorRange = 0f;

	//Current upwards (or downwards) velocity necessary to keep the correct distance to the ground;
	Vector3 currentGroundAdjustmentVelocity = Vector3.zero;

	//References to attached components;
	Collider col;
	Rigidbody rig;
	Transform tr;
	Sensor sensor;

	void Awake()
	{
		Setup();

		//Initialize sensor;
		sensor = new Sensor(this.tr, col);
		RecalculateColliderDimensions();
		RecalibrateSensor();
	}

	void Reset () {
		Setup();
	}

	//Setup references to components;
	void Setup()
	{
		tr = GetComponent<Transform>();
		col = GetComponent<Collider>();

		//If no collider is attached to this gameobject, add a collider;
		if(col == null)
		{
			tr.gameObject.AddComponent<CapsuleCollider>();
			col = GetComponent<Collider>();
		}

		rig = GetComponent<Rigidbody>();

		//If no rigidbody is attached to this gameobject, add a rigidbody;
		if(rig == null)
		{
			tr.gameObject.AddComponent<Rigidbody>();
			rig = GetComponent<Rigidbody>();
		}

		boxCollider = GetComponent<BoxCollider>();
		sphereCollider = GetComponent<SphereCollider>();
		capsuleCollider = GetComponent<CapsuleCollider>();

		//Freeze rigidbody rotation and disable rigidbody gravity;
		rig.freezeRotation = true;
		rig.useGravity = false;
	}

	//Recalculate collider height/width/thickness;
	public void RecalculateColliderDimensions()
	{
		//Check if a collider is attached to this gameobject;
		if(col == null)
		{
			//Try to get a reference to the attached collider by calling Setup();
			Setup();

			//Check again;
			if(col == null)
			{
				Debug.LogWarning("There is no collider attached to " + this.gameObject.name + "!");
				return;
			}				
		}

		//Set collider dimensions based on collider variables;
		if(boxCollider)
		{
			Vector3 _size = Vector3.zero;
			_size.x = colliderThickness;
			_size.z = colliderThickness;

			boxCollider.center = colliderOffset * colliderHeight;

			_size.y = colliderHeight * (1f - stepHeightRatio);
			boxCollider.size = _size;

			boxCollider.center = boxCollider.center + new Vector3(0f, stepHeightRatio * colliderHeight/2f, 0f);
		}
		else if(sphereCollider)
		{
			sphereCollider.radius = colliderHeight/2f;
			sphereCollider.center = colliderOffset * colliderHeight;

			sphereCollider.center = sphereCollider.center + new Vector3(0f, stepHeightRatio * sphereCollider.radius, 0f);
			sphereCollider.radius *= (1f - stepHeightRatio);
		}
		else if(capsuleCollider)
		{
			capsuleCollider.height = colliderHeight;
			capsuleCollider.center = colliderOffset * colliderHeight;
			capsuleCollider.radius = colliderThickness/2f;

			capsuleCollider.center = capsuleCollider.center + new Vector3(0f, stepHeightRatio * capsuleCollider.height/2f, 0f);
			capsuleCollider.height *= (1f - stepHeightRatio);

			if(capsuleCollider.height/2f < capsuleCollider.radius)
				capsuleCollider.radius = capsuleCollider.height/2f;
		}

		//Recalibrate sensor variables to fit new collider dimensions;
		RecalibrateSensor();
	}

	//Recalibrate sensor variables;
	void RecalibrateSensor()
	{
		//Set sensor ray origin and direction;
		sensor.SetCastOrigin(GetColliderCenter());
		sensor.SetCastDirection(Sensor.CastDirection.Down);

		//Make sure that the selected layermask does not include the 'Ignore Raycast' layer;
		sensorLayermask ^= (1 << LayerMask.NameToLayer("Ignore Raycast"));

		//Set sensor cast type and layermask;
		sensor.castType = sensorType;
		sensor.layermask = sensorLayermask;

		//Calculate sensor radius/width;
		float _radius = colliderThickness/2f * sensorRadiusModifier;

		//Multiply all sensor lengths with '_safetyDistanceFactor' to compensate for floating point errors;
		float _safetyDistanceFactor = 0.001f;

		//Fit collider height to sensor radius;
		if(boxCollider)
			_radius = Mathf.Clamp(_radius, _safetyDistanceFactor, (boxCollider.size.y/2f) * (1f - _safetyDistanceFactor));
		else if(sphereCollider)
			_radius = Mathf.Clamp(_radius, _safetyDistanceFactor, sphereCollider.radius * (1f - _safetyDistanceFactor));
		else if(capsuleCollider)
			_radius = Mathf.Clamp(_radius, _safetyDistanceFactor, (capsuleCollider.height/2f) * (1f - _safetyDistanceFactor));

		//Set sensor variables;

		//Set sensor radius;
		sensor.sphereCastRadius = _radius;

		//Calculate and set sensor length;
		float _length = 0f;
		_length += (colliderHeight * (1f - stepHeightRatio)) * 0.5f;
		_length += colliderHeight * stepHeightRatio;
		baseSensorRange = _length * (1f + _safetyDistanceFactor);
		sensor.castLength = _length;

		//Set sensor array variables;
		sensor.ArrayRows = sensorArrayRows;
		sensor.arrayRayCount = sensorArrayRayCount;
		sensor.offsetArrayRows = sensorArrayRowsAreOffset;
		sensor.isInDebugMode = isInDebugMode;

		//Set sensor spherecast variables;
		sensor.calculateRealDistance = true;
		sensor.calculateRealSurfaceNormal = true;

		//Recalibrate sensor to the new values;
		sensor.RecalibrateRaycastArrayPositions();
	}

	//Returns the collider's center in world coordinates;
	Vector3 GetColliderCenter()
	{
		if(col == null)
			Setup();

		return col.bounds.center;
	}

	//Check if mover is grounded;
	//Store all relevant collision information for later;
	//Calculate necessary adjustment velocity to keep the correct distance to the ground;
	void Check()
	{
		//Reset ground adjustment velocity;
		currentGroundAdjustmentVelocity = Vector3.zero;

		//Set sensor length;
		if(IsUsingExtendedSensorRange)
			sensor.castLength = baseSensorRange + colliderHeight * stepHeightRatio;
		else
			sensor.castLength = baseSensorRange;
		
		sensor.Cast();

		//If sensor has not detected anything, set flags and return;
		if(!sensor.HasDetectedHit())
		{
			isGrounded = false;
			return;
		}

		//Set flags for ground detection;
		isGrounded = true;

		//Get distance that sensor ray reached;
		float _distance = sensor.GetDistance();

		//Calculate how much mover needs to be moved up or down;
		float _upperLimit = (colliderHeight * (1f - stepHeightRatio)) * 0.5f;
		float _middle = _upperLimit + colliderHeight * stepHeightRatio;
		float _distanceToGo = _middle - _distance;

		//Set new ground adjustment velocity for the next frame;
		currentGroundAdjustmentVelocity = tr.up * (_distanceToGo/Time.fixedDeltaTime);
	}

	//Check if mover is grounded;
	public void CheckForGround()
	{
		Check();
	}

	//Set mover velocity;
	public void SetVelocity(Vector3 _velocity)
	{
		rig.velocity = _velocity + currentGroundAdjustmentVelocity;	
	}	

	//Returns 'true' if mover is touching ground and the angle between hte 'up' vector and ground normal is not too steep (e.g., angle < slope_limit);
	public bool IsGrounded()
	{
		return isGrounded;
	}

	//Setters;

	//Set whether sensor range should be extended;
	public void SetExtendSensorRange(bool _isExtended)
	{
		IsUsingExtendedSensorRange = _isExtended;
	}

	//Set height of collider;
	public void SetColliderHeight(float _newColliderHeight)
	{
		if(colliderHeight == _newColliderHeight)
			return;

		colliderHeight = _newColliderHeight;
		RecalculateColliderDimensions();
	}

	//Set acceptable step height;
	public void SetStepHeightRatio(float _newStepHeightRatio)
	{
		_newStepHeightRatio = Mathf.Clamp(_newStepHeightRatio, 0f, 1f);
		stepHeightRatio = _newStepHeightRatio;
		RecalculateColliderDimensions();
	}

	//Getters;

	public Vector3 GetGroundNormal()
	{
		return sensor.GetNormal();
	}

	public Vector3 GetGroundPoint()
	{
		return sensor.GetPosition();
	}

	public Collider GetGroundCollider()
	{
		return sensor.GetCollider();
	}
	
}
