using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponSkillType
{
    attack,
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

    public void ErrorCheck()
    {
        attack.ErrorCheck();
    }
}
