using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum HitboxParentType
{
    player,
    player_animated,
    weaponEdge,
    weaponHandle
}
public class AttackHitbox
{
    public HitboxParentType parentType;
    public GameObject hitboxPrefab;
    List<AttackEffect> effects;

    public void ErrorCheck()
    {
        List<AttackEffect> auxEffects = new List<AttackEffect>();
        bool errorFound = false;
        for(int i=0;i< effects.Count && !errorFound; i++)
        {
            if (!auxEffects.Contains(effects[i]))
            {
                auxEffects.Add(effects[i]);
            }
            else
            {
                errorFound = true;
                Debug.LogError("AttackHitbox-> Error: there can only be 1 effect of the same type!");
            }
        }
        for(int i=0; i< effects.Count; i++)
        {
            effects[i].ErrorCheck();
        }
    }
    
}
