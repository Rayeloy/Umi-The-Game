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

    public float duration;
    public bool restrictRotation;
    public float rotationSpeed;
    public bool restrictMovement;
    [Range(0, 1)]
    public float movementSpeed;

    [Tooltip("Do you want this phase to have a hitbox?")]
    public bool hasHitbox;
    [Tooltip("Leave empty if no hitbox needed")]
    public GameObject hitboxPrefab;

    private void OnEnable()
    {
        Debug.Log("AttackPhaseData OnEnable() and I'm " + this);

    }

    public void ErrorCheck()
    {

    }

    //void Init()
    //{
    //    isFoldedInEditor = false;
    //    duration = 0;
    //    restrictRotation = false;
    //    rotationSpeed = 0;
    //    hasHitbox = false;
    //    hitboxPrefab = null;
    //}

}