using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Basic controller script;
//This controller is used as a basis for more advanced controller types ('SidescrollerController', 'CameraWalkerController');
//Custom movement input can be implemented by creating a new script that inherits 'BasicWalkerController' and overrides the 'CalculateMovementDirection' function;
public class BasicWalkerController : MonoBehaviour {

	//References to attached components;
	protected Transform tr;
	protected Mover mover;

	//Names of input axes used for horizontal and vertical input;
	public string horizontalInputAxis = "Horizontal";
	public string verticalInputAxis = "Vertical";

	//Whether or not to use raw input values;
	public bool useRawInput = false;

	//Keycode used for jumping;
	public KeyCode jumpKey = KeyCode.Space;

	//Jump key variables;
	bool jumpKeyWasPressed = false;
	bool jumpKeyWasLetGo = false;
	bool jumpKeyIsPressed = false;

	//Movement speed;
	public float movementSpeed = 7f;

	//'Aircontrol' determines to what degree the player is able to move while in the air;
	[Range(0f, 1f)]
	public float airControl = 0.4f;

	//Jump speed;
	public float jumpSpeed = 10f;

	//Jump duration variables;
	public float jumpDuration = 0.2f;
	float currentJumpStartTime = 0f;

	//'AirFriction' determines how fast the controller loses its momentum while in the air;
	//'GroundFriction' is used instead, if the controller is grounded;
	public float airFriction = 0.5f;
	public float groundFriction = 100f;

	//Current momentum;
	Vector3 momentum = Vector3.zero;

	//Saved velocity from last frame;
	Vector3 savedVelocity = Vector3.zero;

	//Saved horizontal movement velocity from last frame;
	Vector3 savedMovementVelocity = Vector3.zero;

	//Amount of downward gravitation;
	public float gravity = 30f;
	//Amount of downward gravitation when sliding down a slope;
	public float slideGravity = 5f;

	//Acceptable slope angle limit;
	public float slopeLimit = 80f;

	//Whether to calculate and apply momentum relative to the controller's transform;
	public bool useLocalMomentum = false;

	//Enum describing basic controller states; 
	public enum ControllerState
	{
		Grounded,
		Sliding,
		Falling,
		Rising,
		Jumping
	}
	
	ControllerState currentControllerState = ControllerState.Falling;
	 
	//Get references to all necessary components;
	void Awake () {
		mover = GetComponent<Mover>();
		tr = GetComponent<Transform>();

		Setup();
	}

	//This function is called right after Awake(); It can be overridden by inheriting scripts;
	protected virtual void Setup()
	{

	}

	void Update()
	{
		HandleJumpKeyInput();
	}

	//Handle jump booleans for later use in FixedUpdate;
	void HandleJumpKeyInput()
	{
		bool _newJumpKeyPressedState = IsJumpKeyPressed();

		if(jumpKeyIsPressed == false && _newJumpKeyPressedState == true)
			jumpKeyWasPressed = true;

		if(jumpKeyIsPressed == false && _newJumpKeyPressedState == false)
			jumpKeyWasLetGo = true;

		jumpKeyIsPressed = _newJumpKeyPressedState;
	}

	//FixedUpdate;
	void FixedUpdate()
	{
		//Check if mover is grounded;
		mover.CheckForGround();

		//Determine controller state;
		HandleState();

		//Apply friction and gravity to 'momentum';
		HandleMomentum();

		//Check if the player has initiated a jump;
		HandleJumping();

		//Calculate movement velocity;
		Vector3 _velocity = CalculateMovementVelocity();

		//If local momentum is used, transform momentum into world space first;
		Vector3 _worldMomentum = momentum;
		if(useLocalMomentum)
			_worldMomentum = tr.localToWorldMatrix * momentum;

		//Add current momentum to velocity;
		_velocity += _worldMomentum;
		
		//If player is grounded or sliding on a slope, extend mover's sensor range;
		//This enables the player to walk up/down stairs and slopes without losing ground contact;
		mover.SetExtendSensorRange(IsGrounded());

		//Set mover velocity;		
		mover.SetVelocity(_velocity);

		//Store velocity for next frame;
		savedVelocity = _velocity;
		savedMovementVelocity = _velocity - _worldMomentum;

		//Reset jump key booleans;
		jumpKeyWasLetGo = false;
		jumpKeyWasPressed = false;
	}

	//Calculate and return movement direction based on player input;
	//This function can be overridden by inheriting scripts to implement different player controls;
	protected virtual Vector3 CalculateMovementDirection()
	{
		Vector3 _velocity = Vector3.zero;

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

		_velocity += tr.right * _horizontalInput;
		_velocity += tr.forward * _verticalInput;

		//Clamp movement vector to magnitude of 1f;
		if(_velocity.magnitude > 1f)
			_velocity.Normalize();

		return _velocity;
	}

	//Calculate and return movement velocity based on player input, controller state, ground normal [...];
	protected Vector3 CalculateMovementVelocity()
	{
		//Calculate (normalized) movement direction;
		Vector3 _velocity = CalculateMovementDirection();

		//Save movement direction for later;
		Vector3 _velocityDirection = _velocity;

		//Multiply (normalized) velocity with movement speed;
		_velocity *= movementSpeed;

		//If controller is not grounded, multiply movement velocity with 'airControl';
		if(!(currentControllerState == ControllerState.Grounded))
			_velocity = _velocityDirection * movementSpeed * airControl;

		return _velocity;
	}

	//Returns 'true' if the player presses the jump key;
	protected virtual bool IsJumpKeyPressed()
	{
		 return (Input.GetKey(jumpKey));
	}

	//Handle state transitions;
	//Determine current controller state based on current momentum and whether the controller is grounded (or not);
	void HandleState()
	{
		//Check if vertical momentum is pointing upwards;
		bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0f);
		//Check if controller is sliding;
		bool _isSliding = mover.IsGrounded() && IsGroundTooSteep();
		
		switch(currentControllerState)
		{
		case ControllerState.Grounded:

			if(_isRising){
				currentControllerState = ControllerState.Rising;
				OnGroundContactLost();
				break;
			}
			if(!mover.IsGrounded()){
				currentControllerState = ControllerState.Falling;
				OnGroundContactLost();
				break;
			}
			if(_isSliding){
				currentControllerState = ControllerState.Sliding;
				break;
			}
			break;

		case ControllerState.Falling:

			if(_isRising){
				currentControllerState = ControllerState.Rising;
				break;
			}
			if(mover.IsGrounded() && !_isSliding){
				currentControllerState = ControllerState.Grounded;
				OnGroundContactRegained(momentum);
				break;
			}
			if(_isSliding){
				currentControllerState = ControllerState.Sliding;
				OnGroundContactRegained(momentum);
				break;
			}
			break;

		case ControllerState.Sliding:

			if(_isRising){
				currentControllerState = ControllerState.Rising;
				OnGroundContactLost();
				break;
			}
			if(!mover.IsGrounded()){
				currentControllerState = ControllerState.Falling;
				break;
			}
			if(mover.IsGrounded() && !_isSliding){
				OnGroundContactRegained(momentum);
				currentControllerState = ControllerState.Grounded;
				break;
			}
			break;

		case ControllerState.Rising:

			if(!_isRising){
				if(mover.IsGrounded() && !_isSliding){
					currentControllerState = ControllerState.Grounded;
					OnGroundContactRegained(momentum);
					break;
				}
				if(_isSliding){
					currentControllerState = ControllerState.Sliding;
					break;
				}
				if(!mover.IsGrounded()){
					currentControllerState = ControllerState.Falling;
					break;
				}
			}
			break;

		case ControllerState.Jumping:

			//Check for jump timeout;
			if((Time.time - currentJumpStartTime) > jumpDuration)
				currentControllerState = ControllerState.Rising;

			//Check if jump key was let go;
			if(jumpKeyWasLetGo)
				currentControllerState = ControllerState.Rising;

			break;
		}
	}

	//Check if player has initiated a jump;
	void HandleJumping()
	{
		if(currentControllerState == ControllerState.Grounded)
		{
			if(jumpKeyIsPressed == true || jumpKeyWasPressed)
			{
				//Call events;
				OnGroundContactLost();
				OnJumpStart();

				currentControllerState = ControllerState.Jumping;
			}
		}
	}

	//Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
	//Handle sliding down steep slopes;
	void HandleMomentum()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if(useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		Vector3 _verticalMomentum = Vector3.zero;
		Vector3 _horizontalMomentum = Vector3.zero;

		//Split momentum into vertical and horizontal components;
		if(momentum != Vector3.zero)
		{
			_verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
			_horizontalMomentum = momentum - _verticalMomentum;
		}

		//Add gravity to vertical momentum;
		_verticalMomentum -= tr.up * gravity * Time.deltaTime;

		//Remove any downward force if the controller is grounded;
		if(currentControllerState == ControllerState.Grounded)
			_verticalMomentum = Vector3.zero;

		//Apply friction to horizontal momentum based on whether the controller is grounded;
		if(currentControllerState == ControllerState.Grounded)
			_horizontalMomentum = VectorMath.IncrementVectorLengthTowardTargetLength(_horizontalMomentum, groundFriction, Time.deltaTime, 0f);
		else
			_horizontalMomentum = VectorMath.IncrementVectorLengthTowardTargetLength(_horizontalMomentum, airFriction, Time.deltaTime, 0f); 

		//Add horizontal and vertical momentum back together;
		momentum = _horizontalMomentum + _verticalMomentum;

		//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
		if(currentControllerState == ControllerState.Sliding)
		{
			momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());
		}

		//Apply slide gravity along ground normal, if controller is sliding;
		if(currentControllerState == ControllerState.Sliding)
		{
			Vector3 _slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal()).normalized;
			momentum += _slideDirection * slideGravity * Time.deltaTime;
		}

		//If controller is jumping, override vertical velocity with jumpSpeed;
		if(currentControllerState == ControllerState.Jumping)
		{
			momentum = VectorMath.RemoveDotVector(momentum, tr.up);
			momentum += tr.up * jumpSpeed;
		}

		if(useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//Events;

	//This function is called when the player has initiated a jump;
	void OnJumpStart()
	{
		//If local momentum is used, transform momentum into world coordinates first;
		if(useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		//Add jump force to momentum;
		momentum += tr.up * jumpSpeed;

		//Set jump start time;
		currentJumpStartTime = Time.time;

		//Call event;
		if(OnJump != null)
			OnJump(momentum);

		if(useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//This function is called when the player has lost ground contact, i.e. is either falling or rising, or generally in the air;
	void OnGroundContactLost()
	{
		//Calculate current velocity;
		//If velocity would exceed the controller's movement speed, decrease movement velocity appropriately;
		//This prevents unwanted accumulation of velocity;
		float _horizontalMomentumSpeed = VectorMath.RemoveDotVector(GetMomentum(), tr.up).magnitude;
		Vector3 _currentVelocity = GetMomentum() + Vector3.ClampMagnitude(savedMovementVelocity, Mathf.Clamp(movementSpeed - _horizontalMomentumSpeed, 0f, movementSpeed));

		//Calculate length and direction from '_currentVelocity';
		float _length = _currentVelocity.magnitude;
		
		//Calculate velocity direction;
		Vector3 _velocityDirection = Vector3.zero;
		if(_length != 0f)
			_velocityDirection = _currentVelocity/_length;

		//Subtract from '_length', based on 'movementSpeed' and 'airControl', check for overshooting;
		if(_length >= movementSpeed * airControl)
			_length -= movementSpeed * airControl;
		else
			_length = 0f;

		//If local momentum is used, transform momentum into world coordinates first;
		if(useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		momentum = _velocityDirection * _length;

		if(useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//This function is called when the player has landed on a surface after being in the air;
	void OnGroundContactRegained(Vector3 _collisionVelocity)
	{
		//Call 'OnLand' event;
		if(OnLand != null)
			OnLand(_collisionVelocity);
	}

	//Helper functions;

	//Returns 'true' if vertical momentum is above a small threshold;
	private bool IsRisingOrFalling()
	{
		//Calculate current vertical momentum;
		Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), tr.up);

		//Setup threshold to check against;
		//For most applications, a value of '0.001f' is recommended;
		float _limit = 0.001f;

		//Return true if vertical momentum is above '_limit';
		return(_verticalMomentum.magnitude > _limit);
	}

	//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
	private bool IsGroundTooSteep()
	{
		if(!mover.IsGrounded())
			return true;

		return (Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit);
	}

	//Getters;

	//Get last frame's velocity;
	public Vector3 GetVelocity ()
	{
		return savedVelocity;
	}

	//Get last frame's movement velocity (momentum is ignored);
	public Vector3 GetMovementVelocity()
	{
		return savedMovementVelocity;
	}

	//Get current momentum;
	public Vector3 GetMomentum()
	{
		Vector3 _worldMomentum = momentum;
		if(useLocalMomentum)
			_worldMomentum = tr.localToWorldMatrix * momentum;

		return _worldMomentum;
	}

	//Returns 'true' if controller is grounded (or sliding down a slope);
	public bool IsGrounded()
	{
		return(currentControllerState == ControllerState.Grounded || currentControllerState == ControllerState.Sliding);
	}

	//Returns 'true' if controller is sliding;
	public bool IsSliding()
	{
		return(currentControllerState == ControllerState.Sliding);
	}

	//Add momentum to controller;
	public void AddMomentum (Vector3 _momentum)
	{
		if(useLocalMomentum)
			momentum = tr.localToWorldMatrix * momentum;

		momentum += _momentum;	

		if(useLocalMomentum)
			momentum = tr.worldToLocalMatrix * momentum;
	}

	//Events;
	public delegate void VectorEvent(Vector3 v);
	public event VectorEvent OnJump;
	public event VectorEvent OnLand;

}
