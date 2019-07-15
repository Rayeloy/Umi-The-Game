﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ----[ PUBLIC ENUMS ]----
public enum KnockbackType
{
    none=0,
    outwards = 1,
    inwards = 2,
    autoCenter=3,
    customDir = 4,
    redirect=5
}
public enum EffectType
{
    none=0,
    knockback=1,
    softStun=2,
    stun=3,
    knockdown=4,
    parry=5
}
#endregion
[System.Serializable]
public class AttackEffect
{
    public EffectType effectType;

    [Header("--- Knockback ---")]
    public KnockbackType knockbackType;
    public float knockbackMagnitude = 0;
    [Header("- Custom Dir -")]
    public Vector3 knockbackDir = Vector3.zero;
    [Header("- Redirect -")]
    [Tooltip("The maximum horizontal angle in which we can redirect the enemy with the joystick")]
    [Range(-90,90)]
    public float redirectMaxAngle = 0;
    [Range(-90,90)]
    public float redirectKnockbackYAngle = 0;
    [Header("--- Stun & softStun ---")]
    public float stunTime = 0;
    [Header("--- Knockdown ---")]
    public float knockdownTime = 0;
    [Header("--- Parry ---")]
    public float parryStunTime = 0;

    [Tooltip("0 = 0% of the recovery time when succeeding in a parry, 1 = 100%.")]
    [Range(0, 1)]
    public float parryRecoveryTime = 0.3f;

    

    public void ErrorCheck()
    {
        if (effectType == EffectType.none) Debug.LogError("AttackEffect-> Error: the effectType is set to none!");
        switch (effectType)
        {
            case EffectType.knockback:
                if(knockbackType==KnockbackType.none) Debug.LogError("AttackEffect-> Error: the effect is of type knockback but the knockbackType is set to none!");
                if (knockbackMagnitude == 0) Debug.LogError("AttackEffect-> Error: the effect is of type knockback but the knockbackMagnitude is set to 0!");
                if (knockbackType == KnockbackType.customDir && knockbackDir==Vector3.zero)
                    Debug.LogError("AttackEffect-> Error: the effect is of type knockback and the knockbackType is set to customDir but the customDir is set to Vector3.zero!");
                if (knockbackType == KnockbackType.redirect && redirectMaxAngle==0)
                    Debug.LogError("AttackEffect-> Error: the effect is of type knockback and the knockbackType is set to redirect but the redirectMaxAngle is set to 0!");
                if (knockbackType == KnockbackType.redirect && redirectKnockbackYAngle == 0)
                    Debug.LogError("AttackEffect-> Error: the effect is of type knockback and the knockbackType is set to redirect but the redirectKnockbackYAngle is set to 0!");
                break;
            case EffectType.softStun:
                if(stunTime==0) Debug.LogError("AttackEffect-> Error: the effect is of type softStun but the stunTime is set to 0!");
                break;
            case EffectType.stun:
                if (stunTime == 0) Debug.LogError("AttackEffect-> Error: the effect is of type stun but the stunTime is set to 0!");
                break;
            case EffectType.knockdown:
                if (knockdownTime == 0) Debug.LogError("AttackEffect-> Error: the effect is of type knockdown but the knockdownTime is set to 0!");
                break;
            case EffectType.parry:
                if (parryStunTime == 0) Debug.LogError("AttackEffect-> Error: the effect is of type parry but the parryStunTime is set to 0!");
                if (parryRecoveryTime == 1) Debug.LogError("AttackEffect-> Error: the effect is of type parry but the parryRecoveryTime is set to 1 which means we do the full recovery animation of the parry!");
                break;
        }
    }

}