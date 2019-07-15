using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KnockbackType
{
    outwards = 0,
    inwards = 1,
    customDir = 2,
    redirect =3
}
public enum EffectType
{
    knockback,
    softStun,
    stun,
    knockdown,
    parry
}
[System.Serializable]
public class AttackEffect
{
    public EffectType effectType;

    [Header("--- Knockback ---")]
    public KnockbackType knockbackType;
    public Vector3 knockbackDir;
    public Vector3 knockbackMagnitude;
    public float knockbackY;
    [Header("--- Stun & softStun ---")]
    public float stunTime;
    [Header("--- Knockdown ---")]
    public float knockdownTime;
    [Header("--- Parry ---")]
    public float parryStunTime;
    [Range(0, 1)]
    public float parryRecoveryTime;

    public void ErrorCheck()
    {

    }

}
