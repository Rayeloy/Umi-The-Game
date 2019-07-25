using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ----[ PUBLIC ENUMS ]----
public enum AttackPhaseType
{
    ready = 0,
    charging = 1,
    startup = 2,
    active = 3,
    recovery = 4
}
#endregion
[System.Serializable]
public class AttackPhase
{
    [HideInInspector]
    public bool isFoldedInEditor;
    [HideInInspector]
    public AttackPhaseType attackPhaseType;
    public AttackHitbox[] attackHitboxes;

    public float duration;

    [Tooltip("0 means 0% rotation, 1 -> 100% rotation")]
    [Range(0,1)]
    public float rotationSpeedPercentage;
    [Tooltip("0 means 0% movement, 1 -> 100% movement")]
    [Range(0, 1)]
    public float movementSpeedPercentage;

    //[Tooltip("Do you want this phase to have a hitbox?")]
    //public bool hasHitbox;
    //[Tooltip("Leave empty if no hitbox needed.")]
    //public GameObject hitboxPrefab;
    [Tooltip("Player is invulnerable to attacks during this phase.")]
    public bool invulnerability;

    private void OnEnable()
    {
        Debug.Log("AttackPhaseData OnEnable() and I'm " + this);
    }

    public void ErrorCheck()
    {
        if(attackPhaseType== AttackPhaseType.active && attackHitboxes.Length==0)
        {
            Debug.LogError("AttackPhase-> Error: this is an active phase attackPhase but there is no hitbox!");
        }
        else if (attackPhaseType != AttackPhaseType.active && attackHitboxes.Length>0)
        {
            Debug.LogError("AttackPhase-> Error: only active phases are supposed to have a hitbox!");
        }
        else
        {
            for (int i = 0; i < attackHitboxes.Length; i++)
            {
                attackHitboxes[i].ErrorCheck();
            }
        }
    }

}