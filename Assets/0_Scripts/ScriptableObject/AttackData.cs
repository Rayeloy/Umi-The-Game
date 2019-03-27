using System.Collections;
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
        //Debug.Log("AttackData OnEnable() and I'm "+name);
        //if (chargingPhase == null)
        //{
        //    Debug.Log("chargingPhase created");
        //    chargingPhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        //    //chargingPhase = new AttackPhase();
        //}
        //if (startupPhase == null)
        //{
        //    Debug.Log("startupPhase created");
        //    startupPhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        //    //startupPhase = new AttackPhase();
        //}
        //if (activePhase == null)
        //{
        //    Debug.Log("activePhase created");
        //    activePhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        //    //activePhase = new AttackPhase();
        //}
        //if (recoveryPhase == null)
        //{
        //    Debug.Log("recoveryPhase created");
        //    recoveryPhase = CreateInstance(typeof(AttackPhase)) as AttackPhase;
        //    //recoveryPhase = new AttackPhase();
        //}
    }

    public AttackPhase GetAttackPhase(AttackStage attackStage)
    {
        switch (attackStage)
        {
            case AttackStage.charging:
                return chargingPhase;
            case AttackStage.startup:
                return startupPhase;
            case AttackStage.active:
                return activePhase;
            case AttackStage.recovery:
                return recoveryPhase;
        }
        return null;
    }
}
