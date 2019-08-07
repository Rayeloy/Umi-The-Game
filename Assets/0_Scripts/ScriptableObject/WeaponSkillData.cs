using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponSkillType
{
    attack,
    attack_extend,
    shoot,
    deploy

}

[CreateAssetMenu(fileName = "New weapon skill", menuName = "Weapon Skill")]
public class WeaponSkillData : ScriptableObject
{
    public string skillName;
    public WeaponSkillType weaponSkillType;
    public WeaponType weaponType;
    public bool pressAgainToStopSkill = false;
    public AttackData attack;
    public float cd;
    public Image weaponSkillHUDImage;
    public int levelNeededToUnlock = 0;

    [Header(" -- QTipCannon -- ")]
    [Space]
    [Header(" --- LEAVE IF YOU DON'T KNOW ---")]
    [Header(" [--- Specific parameters ---]")]

    [Range(0, 15)]
    public float maxAttackRange = 6;
    public float extendingSpeed = 10;
    public float retractingSpeed = 10;

    public void ErrorCheck()
    {
        if (skillName != name) Debug.LogWarning("The skill " + name + " has a different skillName (" + skillName + ")");
        if (cd < 0) Debug.LogError("The skill " + skillName + " can't have a negative cd(" + cd + ")");
        else if (cd == 0) Debug.LogWarning("The skill " + skillName + " has cd = 0. Are you sure you want this?");
        if (levelNeededToUnlock < 0) Debug.LogError("The skill " + skillName + " can't have a negative levelNeededToUnlock(" + levelNeededToUnlock + ")");

        if (weaponSkillType == WeaponSkillType.attack_extend)
        {
            if (maxAttackRange <= 0) Debug.LogError("The weaponSkill " + skillName + " is of type " + weaponSkillType + " but has maxAttackRange <= 0 (" + maxAttackRange + ")");
            if (extendingSpeed <= 0) Debug.LogError("The weaponSkill " + skillName + " is of type " + weaponSkillType + " but has extendingSpeed <= 0 (" + extendingSpeed + ")");
            if (retractingSpeed <= 0) Debug.LogError("The weaponSkill " + skillName + " is of type " + weaponSkillType + " but has retractingSpeed <= 0 (" + retractingSpeed + ")");
        }
        attack.ErrorCheck();
    }
}
