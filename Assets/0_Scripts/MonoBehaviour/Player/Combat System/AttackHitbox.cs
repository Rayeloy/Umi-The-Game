using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
#region ----[ PUBLIC ENUMS ]----
public enum HitboxParentType
{
    player,
    player_animated,
    weaponEdge,
    weaponHandle
}
#endregion
[System.Serializable]
public class AttackHitbox
{
    public HitboxParentType parentType;
    public GameObject hitboxPrefab;
    public AttackEffect[] effects;

    public void ErrorCheck()
    {
        List<EffectType> auxEffects = new List<EffectType>();
        bool errorFound = false;
        for(int i=0;i< effects.Length && !errorFound; i++)
        {
            if (!auxEffects.Contains(effects[i].effectType))
            {
                if((effects[i].effectType==EffectType.softStun || effects[i].effectType == EffectType.stun || effects[i].effectType == EffectType.knockdown) && 
                    (auxEffects.Contains(EffectType.softStun) || auxEffects.Contains(EffectType.stun) || auxEffects.Contains(EffectType.knockdown)))
                {
                    Debug.LogError("AttackHitbox-> Error: there can only be 1 stun/softStun/knockDown effect at the same type!");
                    return;
                }
                else
                {
                    auxEffects.Add(effects[i].effectType);
                }
            }
            else
            {
                errorFound = true;
                Debug.LogError("AttackHitbox-> Error: there can only be 1 effect of the same type!");
                return;
            }
        }
        for(int i=0; i< effects.Length; i++)
        {
            effects[i].ErrorCheck();
        }
    }
    
}
