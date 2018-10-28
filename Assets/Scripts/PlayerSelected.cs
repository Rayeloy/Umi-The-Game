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

	public Team team;

	public SkinnedMeshRenderer Body;
    public Material teamBlueMat;
    public Material teamRedMat;

	//public Renderer cachedRenderer;

	public bool Ready = false;

	void Update()
	{
		Debug.Log(Actions.Movement.X < -0.5f && team == Team.red);
		if (Actions.Jump.WasPressed)
		{
			Ready = !Ready;
		}
		
		if (Actions.Movement.X < -0.5f && team != Team.blue){
			changeTeam(Team.blue);
		}
		else if (Actions.Movement.X > 0.5f && team != Team.red){
			changeTeam(Team.red);
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
