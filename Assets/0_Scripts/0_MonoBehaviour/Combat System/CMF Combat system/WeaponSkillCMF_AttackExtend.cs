using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WeaponSkillCMF_AttackExtend : WeaponSkillCMF
{
    public AttackExtendStage attackExtendStg = AttackExtendStage.notStarted;
    float currentDist = 0;
    public Transform referencePoint;
    Vector3 initialPos;
    float initialProportionZ;
    public Transform hitboxParent;//used for extension 

    #region ----[ CONSTRUCTOR ]----
    public WeaponSkillCMF_AttackExtend(PlayerCombatCMF _myPlayerCombat, WeaponSkillData _myWeaponSkillData) : base(_myPlayerCombat, _myWeaponSkillData)
    {
    }
    #endregion
    protected override void SpecificAwake()
    {
        if (!myPlayerCombat.myPlayerMovement.disableAllDebugs) Debug.Log("SPECIFIC AWAKE -- ATTACK EXTEND");
        if (myWeaponSkillData.weaponSkillType != WeaponSkillType.attack_extend) Debug.LogError("The weaponSkillType of the weaponSkill " + myWeaponSkillData.skillName + " should be of type "
            + WeaponSkillType.attack_extend.ToString() + ", instead, it is " + myWeaponSkillData.weaponSkillType);
        //referencePoint = myPlayerCombat.currentHitboxes[0].GetComponent<Hitbox>().referencePos1;
    }

    protected override void SpecificProcessSkill()
    {
        if (weaponSkillSt == WeaponSkillState.active)
        {
            if (myPlayerCombat.attackStg == AttackPhaseType.active)
            {
                StartExtensionAttack();
                ProcessExtensionAttack();
            }
        }
    }

    void StartExtensionAttack()
    {
        if (!myPlayerCombat.myPlayerMovement.disableAllDebugs) Debug.Log("Start Extension Attack?");
        if (attackExtendStg == AttackExtendStage.notStarted)
        {
            attackExtendStg = AttackExtendStage.extending;
            hitboxParent = myPlayerCombat.currentHitboxes[0].transform.parent;
            referencePoint = myPlayerCombat.currentHitboxes[0].GetComponentInChildren<HitboxCMF>().referencePos1;
            initialPos = referencePoint.position;
            initialProportionZ = hitboxParent.localScale.z;
            currentDist = 0;
        }
    }

    void ProcessExtensionAttack()
    {
        if (attackExtendStg == AttackExtendStage.extending || attackExtendStg == AttackExtendStage.retracting)
        {
            if (!myPlayerCombat.myPlayerMovement.disableAllDebugs) Debug.Log("referencePoint.position = " + referencePoint.position);
            if (!myPlayerCombat.myPlayerMovement.disableAllDebugs) Debug.Log("initialPos = " + initialPos);
            currentDist = (referencePoint.position - initialPos).magnitude;
            switch (attackExtendStg)
            {
                case AttackExtendStage.extending:
                    Vector3 newLocalScale = hitboxParent.localScale;
                    newLocalScale.z += myWeaponSkillData.extendingSpeed * Time.deltaTime;
                    hitboxParent.localScale = newLocalScale;
                    if (currentDist >= myWeaponSkillData.maxAttackRange)
                    {
                        StartRetracting();
                    }
                    break;
                case AttackExtendStage.retracting:
                    newLocalScale = hitboxParent.localScale;
                    newLocalScale.z -= myWeaponSkillData.retractingSpeed * Time.deltaTime;
                    hitboxParent.localScale = newLocalScale;
                    if (hitboxParent.localScale.z <= initialProportionZ)
                    {
                        FinishExtensionAttack();
                    }
                    break;
            }
        }
    }

    public void StartRetracting()
    {
        if (attackExtendStg == AttackExtendStage.extending)
        {
            attackExtendStg = AttackExtendStage.retracting;
        }
    }

    void FinishExtensionAttack()
    {
        if (attackExtendStg == AttackExtendStage.retracting)
        {
            StopExtensionAttack();
            EndAttackActivePhase();
            StopSkill();
        }
    }

    public void StopExtensionAttack()
    {
        attackExtendStg = AttackExtendStage.finished;
    }
}
