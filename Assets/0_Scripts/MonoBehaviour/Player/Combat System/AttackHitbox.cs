using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
#region ----[ PUBLIC ENUMS ]----
public enum HitboxParentType
{
    player,
    player_animated,
    player_localParent,//el prefab viene con un parent propio, para cosas como deformar el hijo de ciertas maneras con la proporción
    player_followTransform,
    weaponEdge,
    weaponHandle,
}
#endregion
[System.Serializable]
public class AttackHitbox
{
    [HideInInspector] public string name;
    public HitboxParentType parentType;
    public GameObject hitboxPrefab;
    public AttackEffect[] effects;

    public AttackEffect GetEffect(EffectType effectType)
    {
        AttackEffect effect = null;
        for(int i=0; i < effects.Length; i++)
        {
            if(effects[i].effectType == effectType)
            {
                effect = effects[i];
            }
        }
        return effect;
    }

    public void ErrorCheck(string attackName, string phaseName)
    {
        name = hitboxPrefab.name;
        if (parentType == HitboxParentType.player_followTransform)
        {
            if (hitboxPrefab.GetComponent<FollowTransform>() == null) Debug.LogError("Attack "+ attackName + ", phase "+ phaseName + ", hitbox "+hitboxPrefab+" is of parent type " 
                + HitboxParentType.player_followTransform.ToString() + " but there is not FollowTransform" +" script in the prefab.");
        }
        List<EffectType> auxEffects = new List<EffectType>();
        bool errorFound = false;
        for(int i=0;i< effects.Length && !errorFound; i++)
        {
            if (!auxEffects.Contains(effects[i].effectType))
            {
                if((effects[i].effectType==EffectType.softStun) && (auxEffects.Contains(EffectType.softStun)))
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
            effects[i].ErrorCheck(attackName, phaseName, name);
        }
    }
}
