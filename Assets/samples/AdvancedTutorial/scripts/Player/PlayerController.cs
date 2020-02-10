using UnityEngine;
using System.Collections;
using System.Linq;

namespace Bolt.AdvancedTutorial
{
	public class PlayerController : Bolt.EntityEventListener<IPlayerState>
	{
		const float MOUSE_SENSEITIVITY = 2f;

		bool forward;
		bool backward;
		bool left;
		bool right;
		bool jump;
		bool aiming;
		bool fire;

		int weapon;

		float yaw;
		float pitch;

		PlayerMotor _motor;


		[SerializeField]
		AudioSource _weaponSfxSource;




		void Awake()
		{
			_motor = GetComponent<PlayerMotor>();
		}

		void Update()
		{
			PollKeys(true);

			if (entity.IsOwner && entity.HasControl && Input.GetKey(KeyCode.L))
			{
				for (int i = 0; i < 100; ++i)
				{
					BoltNetwork.Instantiate(BoltPrefabs.SceneCube, new Vector3(Random.value * 512, Random.value * 512, Random.value * 512), Quaternion.identity);
				}
			}
		}

		void PollKeys(bool mouse)
		{
			forward = Input.GetKey(KeyCode.W);
			backward = Input.GetKey(KeyCode.S);
			left = Input.GetKey(KeyCode.A);
			right = Input.GetKey(KeyCode.D);
			jump = Input.GetKey(KeyCode.Space);
			aiming = Input.GetMouseButton(1);
			fire = Input.GetMouseButton(0);

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				weapon = 0;
			}
			else if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				weapon = 1;
			}

			if (mouse)
			{
				yaw += (Input.GetAxisRaw("Mouse X") * MOUSE_SENSEITIVITY);
				yaw %= 360f;

				pitch += (-Input.GetAxisRaw("Mouse Y") * MOUSE_SENSEITIVITY);
				pitch = Mathf.Clamp(pitch, -85f, +85f);
			}
		}

		public override void Attached()
		{


			state.SetAnimator(GetComponentInChildren<Animator>());

			// setting layerweights 
			state.Animator.SetLayerWeight(0, 1);
			state.Animator.SetLayerWeight(1, 1);
		}


		public void ApplyDamage(byte damage)
		{
		}

		public override void SimulateOwner()
		{
		}

		public override void SimulateController()
		{
			PollKeys(false);

			IPlayerCommandInput input = PlayerCommand.Create();

			input.forward = forward;
			input.backward = backward;
			input.left = left;
			input.right = right;
			input.jump = jump;

			input.aiming = aiming;
			input.fire = fire;

			input.yaw = yaw;
			input.pitch = pitch;

			input.weapon = weapon;
			input.Token = new TestToken();

			entity.QueueInput(input);
		}

		public override void ExecuteCommand(Bolt.Command c, bool resetState)
		{

			PlayerCommand cmd = (PlayerCommand)c;

			if (resetState)
			{
				_motor.SetState(cmd.Result.position, cmd.Result.velocity, cmd.Result.isGrounded, cmd.Result.jumpFrames);
			}
			else
			{
				// move and save the resulting state
				var result = _motor.Move(cmd.Input.forward, cmd.Input.backward, cmd.Input.left, cmd.Input.right, cmd.Input.jump, cmd.Input.yaw);

				cmd.Result.position = result.position;
				cmd.Result.velocity = result.velocity;
				cmd.Result.jumpFrames = result.jumpFrames;
				cmd.Result.isGrounded = result.isGrounded;

				if (cmd.IsFirstExecution)
				{
					// animation

					// deal with weapons
					if (cmd.Input.aiming && cmd.Input.fire)
					{
						FireWeapon(cmd);
					}
				}

				if (entity.IsOwner)
				{
					cmd.Result.Token = new TestToken();
				}
			}
		}

		void FireWeapon(PlayerCommand cmd)
		{
		}
	}
}
