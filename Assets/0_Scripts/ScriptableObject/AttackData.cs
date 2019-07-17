using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New attack", menuName = "Attack")]
public class AttackData : ScriptableObject
{
    public string attackName;
    [Tooltip("For RPC sending in online mode.")]
    public int attackID;
    [Tooltip("0 -> this is the base priority, the priority of the basic attacks (autocombo).")]
    public int attackPriority;
    //[Tooltip("Prefab of the hitbox (object with a collider and the hitbox script) that is already well positioned.")]
    //public GameObject hitboxPrefab;

    [Tooltip("If false, the attack starts on the startup phase.")]
    public bool hasChargingPhase = false;
    public AttackPhase chargingPhase;
    public AttackPhase startupPhase;
    public AttackPhase activePhase;
    public AttackPhase recoveryPhase;


    //[Tooltip("Waiting time untill you can use this attack again, starts counting when you finish all the attack phases.")]
    //public float cdTime;
    //[Tooltip("Time the attacks leaves the target stunned.")]
    //public float stunTime;
    [HideInInspector]
    public Vector3 impulseDir;
    public float impulseMagnitude;

    //NOT IN USE
    [Header("DEPRECATED")]
    [Tooltip("Deprecated: Percentage of time of the last phase (or recovery phase) that we can skip by using an attack different from this one. 0% means we can't skip it at all," +
    " 100% means we can fully skip it.")]
    [Range(0, 100)]
    public float comboDifferentAttackPercent = 50;

    private void OnDisable()
    {
        if (chargingPhase.attackPhaseType != AttackPhaseType.charging) chargingPhase.attackPhaseType = AttackPhaseType.charging;
        if (startupPhase.attackPhaseType != AttackPhaseType.startup) startupPhase.attackPhaseType = AttackPhaseType.startup;
        if (activePhase.attackPhaseType != AttackPhaseType.active) activePhase.attackPhaseType = AttackPhaseType.active;
        if (recoveryPhase.attackPhaseType != AttackPhaseType.recovery) recoveryPhase.attackPhaseType = AttackPhaseType.recovery;
    }
    private void OnEnable()
    {
        if (chargingPhase.attackPhaseType != AttackPhaseType.charging) chargingPhase.attackPhaseType = AttackPhaseType.charging;
        if (startupPhase.attackPhaseType != AttackPhaseType.startup) startupPhase.attackPhaseType = AttackPhaseType.startup;
        if (activePhase.attackPhaseType != AttackPhaseType.active) activePhase.attackPhaseType = AttackPhaseType.active;
        if (recoveryPhase.attackPhaseType != AttackPhaseType.recovery) recoveryPhase.attackPhaseType = AttackPhaseType.recovery;

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

    public AttackPhase GetAttackPhase(AttackPhaseType attackStage)
    {
        switch (attackStage)
        {
            case AttackPhaseType.charging:
                return chargingPhase;
            case AttackPhaseType.startup:
                return startupPhase;
            case AttackPhaseType.active:
                return activePhase;
            case AttackPhaseType.recovery:
                return recoveryPhase;
        }
        return null;
    }

    public void ErrorCheck()
    {
        if (hasChargingPhase)
        {
            if (chargingPhase ==null)
            {
                Debug.LogError("AttackData-> Error: Attack " + attackName + " has hasChargingPhase set to true but there is no chargingPhase");
            }
            if(chargingPhase.attackPhaseType != AttackPhaseType.charging) Debug.LogError("AttackData-> Error: Attack " + attackName + "'s chargingPhase has a different type.");
        }
        if (startupPhase.attackPhaseType != AttackPhaseType.startup) Debug.LogError("AttackData-> Error: Attack " + attackName + "'s startupPhase has a different type.");
        if (activePhase.attackPhaseType != AttackPhaseType.active) Debug.LogError("AttackData-> Error: Attack " + activePhase + "'s chargingPhase has a different type.");
        if (recoveryPhase.attackPhaseType != AttackPhaseType.recovery) Debug.LogError("AttackData-> Error: Attack " + recoveryPhase + "'s recoveryPhase has a different type.");

        if (hasChargingPhase) chargingPhase.ErrorCheck();
        startupPhase.ErrorCheck();
        activePhase.ErrorCheck();
        recoveryPhase.ErrorCheck();
    }
}
