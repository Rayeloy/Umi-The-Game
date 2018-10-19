using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	[Header("Referencias")]
	public Animator animator;
	public PlayerMovement playerMovement;

    [Header("Animator Variables")]
    AnimatorStateInfo stateInfo;
    int jumpHash = Animator.StringToHash("Jump");
    int jumpingHash = Animator.StringToHash("Jumping");
    bool jumpingValue;
    int landHash = Animator.StringToHash("Land");
    int jumpStateHash = Animator.StringToHash("Ascender");
    bool landing;
    [Tooltip("Distance to floor at which the landing animation will start")]
    public float startLandHeight = 1;
    //-------------
    bool jump;
    [Header("WEAPONS ATTACH")]
    public Transform rightHand;
    public Transform leftHand;
    //Churros: Pos: -0.0086, 0.0097, 0.0068; Rot: 45.854,-163.913,-5.477; Scale: 1.599267,1.599267,1.599267

    private void Awake()
    {
        
    }

    public void LateUpdate()
    {
		animator.SetFloat("HorizontalSpeed", playerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", playerMovement.currentVel.y);
		animator.SetBool("OnGround", playerMovement.controller.collisions.below);
		if (playerMovement.wallJumpAnim){
			animator.SetTrigger("WallJump");
			playerMovement.wallJumpAnim = false;
		}
        ResetVariables();
        ProcessVariableValues();

    }

    void ResetVariables()
    {

        if (landing && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 && !animator.IsInTransition(0))
        {
            landing = false;
            animator.SetBool(landHash, landing);
        }
    }

    public void ProcessVariableValues()
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (jump || (!playerMovement.controller.collisions.below && playerMovement.controller.collisions.lastBelow))
        {
            if (jump)
            {
                jump = false;
                animator.SetBool(jumpHash, jump);
            }
            jumpingValue = true;
            animator.SetBool(jumpingHash, jumpingValue);
        }
        print("vel.y = "+playerMovement.currentVel.y+"; below = "+ playerMovement.controller.collisions.below+"; distance to floor = "+ playerMovement.controller.collisions.distanceToFloor);
        if ((playerMovement.currentVel.y<0 && !playerMovement.controller.collisions.below && playerMovement.controller.collisions.distanceToFloor <= startLandHeight) 
            || (jumpingValue && playerMovement.controller.collisions.below))
        {
            if (jumpingValue)
            {
                jumpingValue = false;
                animator.SetBool(jumpingHash, jumpingValue);
            }
            print("SET TRIGGER LAND ");
            landing = true;
            animator.SetBool(landHash,landing);
        }
    }

    public void SetJump(bool _jump)
    {
        jump = _jump;
        animator.SetBool(jumpHash, jump);
    }

    public WeaponData SearchWeapon(string name)
    {
        WeaponData[] allWeap = GameController.instance.allWeapons;
        foreach(WeaponData wp in allWeap)
        {
            if(name == wp.weaponName)
            {
                return wp;
            }
        }
        return null;
    }

    public void AttachWeapon(string weaponName)
    {
        WeaponData weapData = SearchWeapon(weaponName);
        Transform wep = Instantiate(weapData.weaponPrefab,rightHand).transform;
        wep.SetParent(rightHand);
        wep.localPosition = weapData.localPosition;
        wep.localRotation = Quaternion.Euler(weapData.localRotation.x, weapData.localRotation.y, weapData.localRotation.z);
        wep.localScale = weapData.localScale; 
    }
}