using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hinchable : MonoBehaviour
{
	public float maxSice;
	public float lives;
	private float MaxLives;

	public void Start()
	{
		MaxLives = lives;
	}

	public void Hinchar (float d)
	{
		Debug.Log("Hinchando");
		lives -= d;
		float i = Mathf.Lerp(maxSice, 1, lives/MaxLives);

		this.transform.localScale = Vector3.one * i;
	}
}
