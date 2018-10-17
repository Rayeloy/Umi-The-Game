using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
	[Header("Referencias")]
	public Animator animator;
	public PlayerMovement playerMovement;
    [Header("WEAPONS ATTACH")]
    public Transform rightHand;
    public Transform leftHand;
    //Churros: Pos: -0.0086, 0.0097, 0.0068; Rot: 45.854,-163.913,-5.477; Scale: 1.599267,1.599267,1.599267
    public WeaponAttachInfo[] weaponsAttachInfo;

    [System.Serializable]
    public struct WeaponAttachInfo
    {
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;
        public Transform weapon;

        public WeaponAttachInfo(Transform _weapon, Vector3 _pos, Vector3 _rot, Vector3 _scale) {
            weapon = _weapon;
            pos = _pos;
            rot = _rot;
            scale = _scale;
        }
    }

    private void Start()
    {
        animator.SetFloat("mierdiSpeed", 3);

    }
    int frames = 0;
    public void LateUpdate(){
		animator.SetFloat("HorizontalSpeed", playerMovement.currentSpeed);//new Vector2 (playerMovement.currentVel.x, playerMovement.currentVel.z).magnitude);
        animator.SetFloat("VerticalSpeed", playerMovement.currentVel.y);
		animator.SetBool("OnGround", playerMovement.controller.collisions.below);
		if (playerMovement.wallJumpAnim){
			animator.SetTrigger("WallJump");
			playerMovement.wallJumpAnim = false;
		}
        frames++;
        if (frames > 10)
        {
            animator.SetFloat("mierdiSpeed", 1);
        }

    }

    public void AttachWeapon(WeaponAttachInfo weapInf)
    {
        weapInf.weapon.SetParent(rightHand);
        weapInf.weapon.localPosition = weapInf.pos;
        weapInf.weapon.localRotation = Quaternion.Euler(weapInf.rot.x, weapInf.rot.y, weapInf.rot.z);
        weapInf.weapon.localScale = weapInf.scale; 
    }
}