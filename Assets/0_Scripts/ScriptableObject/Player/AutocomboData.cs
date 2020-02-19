using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New autocombo", menuName = "Combat/Autocombo")]
public class AutocomboData : ScriptableObject
{
    public string autocomboName;
    public AttackData[] attacks;
    public float maxTimeBetweenAttacks = 0.3f;

    public void ErrorCheck()
    {
        if (attacks.Length < 2) Debug.LogError("Autocombo -> Error: The autocombo "+ autocomboName + " has less than 2 attacks.");
        for(int i = 0; i < attacks.Length; i++)
        {
            if (i+1 < attacks.Length)
            {
                bool softStunHitboxFound = false;
                float maxSoftStunFound = 0;
                for(int j =0; j < attacks[i].activePhase.attackHitboxes.Length; j++)
                {
                    for(int k = 0; k < attacks[i].activePhase.attackHitboxes[j].effects.Length; k++)
                    {
                        if(attacks[i].activePhase.attackHitboxes[j].effects[k].effectType==EffectType.softStun && attacks[i].activePhase.attackHitboxes[j].effects[k].stunTime> maxSoftStunFound)
                        {
                            maxSoftStunFound = attacks[i].activePhase.attackHitboxes[j].effects[k].stunTime;
                            softStunHitboxFound = true;
                        }
                    }
                }
                if (softStunHitboxFound)
                {
                    float maxTimeToNextAttack = attacks[i].activePhase.duration + attacks[i].recoveryPhase.duration + attacks[i + 1].startupPhase.duration + attacks[i].activePhase.duration;
                    if (maxTimeToNextAttack > maxSoftStunFound)
                    {
                        Debug.LogError("Autocombo -> Error: The attack " + attacks[i].attackName + " in the autocombo " + autocomboName +
                            " has a softStun that is too short! it has to be at least "+ maxTimeToNextAttack + " to be a confirmed autocombo!");
                    }
                    else
                    {
                        Debug.LogWarning("Autocombo -> Warning: The attack " + attacks[i].attackName + " in the autocombo " + autocomboName +
                            " has a softStun ("+ maxSoftStunFound + ") that is longer than the maxTimeToNextAttack ("+ maxTimeToNextAttack+") needed. Maybe you want to lower it a bit.");
                    }
                }
                else
                {
                    Debug.LogError("Autocombo -> Error: The attack " + attacks[i].attackName + " in the autocombo " + autocomboName +" does not contain a softStun effect!");
                }
            }
            attacks[i].ErrorCheck();
        }
    }
}
