using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	[Header("Referencias")]
	public Animator animator;
	public PlayerMovement playerMovement;

	public void LateUpdate(){
		animator.SetFloat("HorizontalSpeed", playerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", playerMovement.currentVel.y);
		animator.SetBool("OnGround", playerMovement.controller.collisions.below);
	}
}