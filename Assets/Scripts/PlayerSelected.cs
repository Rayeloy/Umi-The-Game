using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerSelected : MonoBehaviour
{
	//public int
	public PlayerActions Actions { get; set; }
	//public int actionNum;
	//public PlayerMovement.Team team;

	public Team team = Team.none;

	public SkinnedMeshRenderer Body;
    public Material teamBlueMat;
    public Material teamRedMat;

	//public Renderer cachedRenderer;

	private bool _ready = false;
	/// <value>The Name property gets/sets the value of the string field, _name.</value>
	public bool Ready
	{
		get{return _ready;}
		set
		{
			animator.SetBool("Ready", value);
			_ready = value;
		}
	}

	//public bool Ready = false;
	[Header("Referencias")]
	public Animator animator;

	void Update()
	{
		if (Actions.Jump.WasPressed)
			Ready = !Ready;

		if (Ready){
			if (Actions.Movement.X < -0.5f && team != Team.blue){
				changeTeam(Team.blue);
			}
			else if (Actions.Movement.X > 0.5f && team != Team.red){
				changeTeam(Team.red);
			}
		}

//		else
//		{
//			// Set object material color.
//			//cachedRenderer.material.color = GetColorFromInput();
// 
//			// Rotate target object.
//			transform.Rotate( Vector3.down, 500.0f * Time.deltaTime * Actions.Movement.X, Space.World );
//			transform.Rotate( Vector3.right, 500.0f * Time.deltaTime * Actions.Movement.Y, Space.World );
//		}
	}


//	Color GetColorFromInput()
//	{
//		if (Actions.Jump)
//		{
//			return Color.green;
//		}
//
//		if (Actions.Attack3)
//		{
//			return Color.red;
//		}
//
//		if (Actions.Attack1)
//		{
//			return Color.blue;
//		}
//
//		if (Actions.Attack2)
//		{
//			return Color.yellow;
//		}
//
//		return Color.white;
//	}

	private void changeTeam(Team t)
	{
		team = t;
		switch (t)
        {
            case Team.blue:
                Body.material = teamBlueMat;
                break;
            case Team.red:
                Body.material = teamRedMat;
                break;
        }
	}
}
