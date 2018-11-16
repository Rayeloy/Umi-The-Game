using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPlayAnimation : MonoBehaviour
{
	[Header("Referencias")]
	public Animator animator;

	public void Play (string name){
		animator.SetTrigger(name);
	}
}
