using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InControl;

public class PlayerSelected : MonoBehaviour
{
	//public int
	public PlayerActions Actions { get; set; }
	//public int actionNum;

	public Renderer cachedRenderer;

	public bool Ready = false;

	void Start()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	void OnDisable()
	{
		if (Actions != null)
		{
			Actions.Destroy();
		}
	}

	void Update()
	{
		if (Actions.Jump.WasPressed){
			Ready = !Ready;
		}

		if (Actions == null)
		{
			// If no controller exists for this cube, just make it translucent.
			cachedRenderer.material.color = new Color( 1.0f, 1.0f, 1.0f, 0.2f );
		}
		else if (Ready){
			cachedRenderer.material.color = Color.green;
		}
		else
		{
			// Set object material color.
			cachedRenderer.material.color = GetColorFromInput();

			// Rotate target object.
			transform.Rotate( Vector3.down, 500.0f * Time.deltaTime * Actions.Movement.X, Space.World );
			transform.Rotate( Vector3.right, 500.0f * Time.deltaTime * Actions.Movement.Y, Space.World );
		}
	}


	Color GetColorFromInput()
	{
		if (Actions.Jump)
		{
			return Color.green;
		}

		if (Actions.Attack3)
		{
			return Color.red;
		}

		if (Actions.Attack1)
		{
			return Color.blue;
		}

		if (Actions.Attack2)
		{
			return Color.yellow;
		}

		return Color.white;
	}
}
