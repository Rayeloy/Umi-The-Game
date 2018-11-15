using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New attack", menuName = "Attack")]
public class AttackData : ScriptableObject
{
    public enum KnockbackType
    {
        outwards,
        inwards,
        customDir
    }

    public string attackName;
    [Tooltip("Prefab of the hitbox (object with a collider and the hitbox script) that is already well positioned.")]
    public GameObject hitboxPrefab;
    [Tooltip("Leave at 0 if no charging.")]
    public float chargingTime = 0;
    public float startupTime;
    public float activeTime;
    public float recoveryTime;
    [Tooltip("Time the attacks leaves the target stunned.")]
    public float stunTime;
    [Tooltip("Limited by maxMoveSpeed.")]
    public float knockbackSpeed;
    [Tooltip("Outwards is the basic one.")]
    public KnockbackType knockbackType = KnockbackType.outwards;
    [Tooltip("Leave at 0 if knockbackType is not customDir.")]
    public Vector3 knockbackDirection = Vector3.zero;


}
