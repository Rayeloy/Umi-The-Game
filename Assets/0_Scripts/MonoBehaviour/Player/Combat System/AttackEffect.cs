using System.Collections;
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
    redirect=5,
    outwardsFromHitbox = 6
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
    [HideInInspector] public string name;

    public EffectType effectType;
    [Header("[--- KNOCKBACK ---]")]
    public KnockbackType knockbackType;
    [Tooltip("If using the knockbackType Autocenter, the magnitude indicates how far away in front of the player will the hit send the enemy (in Unity units).")]
    public float knockbackMagnitude = 1;
    [Header("- Custom Dir -")]
    public Vector3 knockbackDir = Vector3.zero;
    [Header("- Redirect -")]
    [Tooltip("The maximum horizontal angle in which we can redirect the enemy with the joystick")]
    [Range(-90,90)]
    public float redirectMaxAngle = 45;
    [Header("- Inwards/Outwards/Redirect -")]
    [Range(-90,90)]
    public float knockbackYAngle = 0;
    [Header("[--- STUN & SOFTSTUN ---]")]
    public float stunTime = 0;
    //[Header("--- Knockdown ---")]
    [HideInInspector]
    public const float knockdownTime = 3;
    [Header("[--- PARRY ---]")]
    public float parryStunTime = 0;

    [Tooltip("0 = 0% of the recovery time when succeeding in a parry, 1 = 100%.")]
    [Range(0, 1)]
    public float parryRecoveryTime = 0.3f;

    

    public void ErrorCheck(string AttackName, string PhaseName, string HitboxName)
    {
        name = effectType.ToString();
        if (effectType == EffectType.none) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effectType is set to none!");
        switch (effectType)
        {
            case EffectType.knockback:
                if(knockbackType==KnockbackType.none) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type knockback but the knockbackType is set to none!");
                if (knockbackMagnitude == 0) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type knockback but the knockbackMagnitude is set to 0!");
                if (knockbackType == KnockbackType.customDir && knockbackDir==Vector3.zero)
                    Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type knockback and the knockbackType is set to customDir but the customDir is set to Vector3.zero!");
                if (knockbackType == KnockbackType.redirect && redirectMaxAngle==0)
                    Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type knockback and the knockbackType is set to redirect but the redirectMaxAngle is set to 0!");
                break;
            case EffectType.softStun:
                if(stunTime==0) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type softStun but the stunTime is set to 0!");
                break;
            case EffectType.stun:
                if (stunTime == 0) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type stun but the stunTime is set to 0!");
                break;
            case EffectType.knockdown:
                if (knockdownTime == 0) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ",the effect is of type knockdown but the knockdownTime is set to 0!");
                break;
            case EffectType.parry:
                if (parryStunTime == 0) Debug.LogError("AttackEffect-> Error: In the attack " + AttackName + ", phase " + PhaseName + ", hitbox " + HitboxName + ", the effect is of type parry but the parryStunTime is set to 0!");
                if (parryRecoveryTime == 1) Debug.LogError("AttackEffect-> Error: In the attack "+ AttackName + ", phase "+ PhaseName + ", hitbox "+ HitboxName + ", the effect is of type parry but the parryRecoveryTime is set to 1 which means we do the full recovery animation of the parry!");
                break;
        }
    }

}
