﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum KnockbackType
{
    outwards=0,
    inwards=1,
    customDir=2
}
[CreateAssetMenu(fileName = "New attack", menuName = "Attack")]
public class AttackData : ScriptableObject
{
    public string attackName;
    //[Tooltip("Prefab of the hitbox (object with a collider and the hitbox script) that is already well positioned.")]
    //public GameObject hitboxPrefab;

    [Tooltip("If false, the attack starts on the startup phase.")]
    public bool hasChargingPhase = false;
    public AttackPhase chargingPhase;
    public AttackPhase startupPhase;
    public AttackPhase activePhase;
    public AttackPhase recoveryPhase;

    [Tooltip("Percentage of time of the last phase (or recovery phase) that we can skip by using an attack different from this one. 0% means we can't skip it at all," +
        " 100% means we can fully skip it.")]
    [Range(0, 100)]
    public float comboDifferentAttackPercent = 50;
    //[Tooltip("Waiting time untill you can use this attack again, starts counting when you finish all the attack phases.")]
    //public float cdTime;
    [Tooltip("Time the attacks leaves the target stunned.")]
    public float stunTime;
    [Tooltip("Limited by maxMoveSpeed.")]
    public float knockbackSpeed;
    [Tooltip("Outwards is the basic one.")]
    public KnockbackType knockbackType = KnockbackType.outwards;
    [Tooltip("Leave at 0 if knockbackType is not customDir.")]
    public Vector3 knockbackDirection = Vector3.zero;

    private void OnEnable()
    {
        if (chargingPhase == null)
        {
            chargingPhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        }
        if (startupPhase == null)
        {
            startupPhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        }
        if (activePhase == null)
        {
            activePhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        }
        if (recoveryPhase == null)
        {
            recoveryPhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        }
    }
}
[System.Serializable]
public class AttackPhase : ScriptableObject
{
    public float duration;
    public bool restrictRotation;
    public float rotationSpeed;
    [Tooltip("Leave empty if no hitbox needed")]
    public GameObject hitboxPrefab;
}
