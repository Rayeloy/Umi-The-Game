using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ----[ PUBLIC ENUMS ]----
public enum WeaponSkillState
{
    none,
    ready,
    active,
    cd
}
#endregion

public class WeaponSkill
{
    #region ----[ VARIABLES FOR DESIGNERS ]---- 
    // PUBLIC VARIABLES FOR DESIGNERS HERE
    //Referencias
    public PlayerCombatNew myPlayerCombat;
    public WeaponSkillData myWeaponSkillData;
    #endregion

    #region ----[ PROPERTIES ]----
    // PRIVATE AND [HideInInspector] PUBLIC VARIABLES HERE
    public WeaponSkillState weaponSkillSt = WeaponSkillState.ready;
    //AttackPhaseType attackStg = AttackPhaseType.ready;
    //float attackTime = 0;

    //PHASES
    //bool currentAttackHasRedirect=false;

    //CD
    float currentCDTime = 0;
    #endregion

    #region ----[ CONSTRUCTOR ]----
    public WeaponSkill(PlayerCombatNew _myPlayerCombat, WeaponSkillData _myWeaponSkillData)
    {
        myPlayerCombat = _myPlayerCombat;
        myWeaponSkillData = _myWeaponSkillData;
        weaponSkillSt = WeaponSkillState.ready;
    }
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        weaponSkillSt = WeaponSkillState.ready;
        //AttackStg = AttackPhaseType.ready;

        SpecificAwake();
    }

    protected virtual void SpecificAwake()
    {

    }
    #endregion

    #region Start
    #endregion

    #region Update
    public void KonoUpdate()
    {
        ProcessSkill();
        SpecificProcessSkill();
        ProcessCD();
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----

    #region -- CD --
    void StartCD()
    {
        if (weaponSkillSt == WeaponSkillState.active)
        {
            if (!myPlayerCombat.myPlayerMovement.disableAllDebugs) Debug.LogError("START SKILL CD");
            weaponSkillSt = WeaponSkillState.cd;
            currentCDTime = 0;
        }
    }

    void ProcessCD()
    {
        if (weaponSkillSt == WeaponSkillState.cd)
        {
            currentCDTime += Time.deltaTime;
            //Debug.Log("SKILL "+myWeaponSkillData.skillName+" CD = " + currentCDTime);
            if (currentCDTime >= myWeaponSkillData.cd)
            {
                StopCD();
            }
        }
    }

    void StopCD()
    {
        if (weaponSkillSt == WeaponSkillState.cd)
        {
            if(!myPlayerCombat.myPlayerMovement.disableAllDebugs) Debug.Log("STOP SKILL CD");
            weaponSkillSt = WeaponSkillState.ready;
        }
    }
    #endregion

    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----

    #region -- Skill Flow Functions --
    public void StartSkill()
    {
        Debug.Log("START SKILL?");
        if (weaponSkillSt == WeaponSkillState.ready && myPlayerCombat.canDoCombat)
        {
            Debug.Log("SKILL STARTED ");
            weaponSkillSt = WeaponSkillState.active;
            switch (myWeaponSkillData.weaponSkillType)
            {
                case WeaponSkillType.attack:
                    myPlayerCombat.StartAttack(myWeaponSkillData.attack);
                    break;
                case WeaponSkillType.attack_extend:
                    myPlayerCombat.StartAttack(myWeaponSkillData.attack);
                    break;
            }
        }
    }

    void ProcessSkill()
    {
        if (weaponSkillSt == WeaponSkillState.active)
        {
            switch (myWeaponSkillData.weaponSkillType)
            {
                case WeaponSkillType.attack:
                    if (myPlayerCombat.attackStg == AttackPhaseType.ready)
                    {
                        StopSkill();
                    }
                    //if (myWeaponSkillData.attack.activePhase.phaseCompletionType == PhaseCompletionType.none)
                    //{
                    //    //End active phase?
                    //    EndAttackActivePhase();
                    //}
                    break;
            }
        }
    }

    protected virtual void EndAttackActivePhase()
    {
        if(myPlayerCombat.attackStg == AttackPhaseType.active)
        {
            myPlayerCombat.ChangeAttackPhase(AttackPhaseType.recovery);
        }
    }

    public void StopSkill()
    {
        if (weaponSkillSt == WeaponSkillState.active)
        {
            switch (myWeaponSkillData.weaponSkillType)
            {
                case WeaponSkillType.attack:
                    myPlayerCombat.EndAttack();
                    break;
            }
            StartCD();
            myPlayerCombat.StopWeaponSkill();
        }
    }

    protected virtual void SpecificProcessSkill()
    {

    }
    #endregion

    #endregion
}

#region ----[ STRUCTS & CLASSES ]----
#endregion
