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
    public Material teamNeutralMat;
    public Material teamBlueMat;
    public Material teamRedMat;

    //public Renderer cachedRenderer;

    private bool _ready = false;
    /// <value>The Name property gets/sets the value of the string field, _name.</value>
    public bool Ready
    {
        get { return _ready; }
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

        if (!Ready)
        {
            if (Actions.Movement.X < -0.5f && joystickNeutral)
            {
                joystickNeutral = false;
                switch (team)
                {
                    case Team.none:
                        changeTeam(Team.blue);
                        break;
                    case Team.red:
                        changeTeam(Team.none);
                        break;
                }
            }
            else if (Actions.Movement.X > 0.5f && joystickNeutral)
            {
                joystickNeutral = false;
                switch (team)
                {
                    case Team.none:
                        changeTeam(Team.red);
                        break;
                    case Team.blue:
                        changeTeam(Team.none);
                        break;
                }
            }else if (Actions.Movement.X >= -0.5f && Actions.Movement.X <= 0.5f)
            {
                joystickNeutral = true;
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
    bool joystickNeutral = true;

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
            case Team.none:
                Body.material = teamNeutralMat;
                break;
        }
    }
}
