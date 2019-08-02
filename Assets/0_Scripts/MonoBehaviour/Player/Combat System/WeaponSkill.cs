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
    }
    #endregion

    #region Start
    #endregion

    #region Update
    public void KonoUpdate()
    {
        ProcessSkill();
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
            Debug.LogError("START SKILL CD");
            weaponSkillSt = WeaponSkillState.cd;
            currentCDTime = 0;
        }
    }

    void ProcessCD()
    {
        if (weaponSkillSt == WeaponSkillState.cd)
        {
            currentCDTime += Time.deltaTime;
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
            Debug.LogError("STOP SKILL CD");
            weaponSkillSt = WeaponSkillState.ready;
        }
    }
    #endregion

    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----

    #region -- Skill Flow Functions --
    public void StartSkill()
    {
        if (weaponSkillSt == WeaponSkillState.ready && myPlayerCombat.canDoCombat)
        {
            weaponSkillSt = WeaponSkillState.active;
            switch (myWeaponSkillData.weaponSkillType)
            {
                case WeaponSkillType.attack:
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
                    if (myWeaponSkillData.attack.activePhase.phaseCompletionType == PhaseCompletionType.none)
                    {
                        //End active phase?
                        EndAttackActivePhase();
                    }
                    break;
            }
        }
    }

    protected virtual void EndAttackActivePhase()
    {
        myPlayerCombat.ChangeAttackPhase(AttackPhaseType.recovery);
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
    #endregion

    #endregion
}

#region ----[ STRUCTS & CLASSES ]----
#endregion
