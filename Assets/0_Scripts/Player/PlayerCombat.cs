

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#region ----[ PUBLIC ENUMS ]----
public enum AttackStage
{
    ready = 0,
    charging = 1,
    startup = 2,
    active = 3,
    recovery = 4
}
#endregion

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHook))]
public class PlayerCombat : MonoBehaviour {
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    PlayerMovement myPlayerMovement;
    PlayerWeapons myPlayerWeap;
    PlayerHook myHook;
    PlayerHUD myPlayerHUD;
    public Material[] hitboxMats;
    public Transform hitboxes;

    #endregion

    #region ----[ PROPERTIES ]----
    //Esta variable es la que lleva el tiempo actual transcurrido en cada fase, se restea a 0 cada vez que se cambia de fase.
    float attackTime = 0;
    [HideInInspector]
    public int attackIndex = 0;//index: 0 = X ->Basic Attack; 1 = Y -> Strong Attack; 2 = B -> Special Attack
    //tiempos máximos para cada fase. Honestamente guardarlos aparte puede ser una tontería ya que los tenemos ya dentro del AttackData
    float chargingTime;
    float startupTime;
    float activeTime;
    float recoveryTime;

    [HideInInspector]
    public float knockBackSpeed = 30f;
    [HideInInspector]
    public Text attackNameText;
    [HideInInspector]
    public bool conHinchador = false;

    [HideInInspector]
    public List<string> targetsHit;

    [HideInInspector]
    public AttackStage attackStg = AttackStage.ready;
    [HideInInspector]
    public List<AttackInfo> myAttacks;//index: 0 = X ->Basic Attack; 1 = Y -> Strong Attack; 2 = B -> Special Attack

    Collider hitbox;

    [HideInInspector]
    public bool aiming;
    #endregion

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovement>();
        myPlayerWeap = myPlayerMovement.myPlayerWeap;
        myHook = myPlayerMovement.myPlayerHook;
        myPlayerHUD = myPlayerMovement.myPlayerHUD;
        attackStg = AttackStage.ready;
        targetsHit = new List<string>();
        myAttacks = new List<AttackInfo>();
    }
    #endregion

    #region Start
    public void KonoStart()
    {
        //FillMyAttacks();
        attackIndex = -1;
        //ChangeAttackType(gC.attackX);
        HideAttackHitBox();
        //ChangeNextAttackType();
    }
    #endregion

    #region Update
    public void KonoUpdate()
    {
        //print("Trigger = " + Input.GetAxis(myPlayerMovement.contName + "LT"));
        if (!myPlayerMovement.noInput && !myPlayerMovement.inWater && (attackStg == AttackStage.ready || attackStg == AttackStage.recovery) 
            && !conHinchador && myPlayerWeap.hasWeapon)
        {
            //BASIC ATTACK INPUT CHECK
            int index = 0;
            if (myPlayerMovement.Actions.Attack1.WasPressed && (attackStg == AttackStage.ready || CanComboWithDifferentAttack(index)) && !myPlayerWeap.canPickupWeapon)//Input.GetButtonDown(myPlayerMovement.contName + "X"))
            {
                if (CanComboWithDifferentAttack(index))
                {
                    EndAttack();
                }
                ChangeAttackType(index);
                StartAttack();
            }

            //STRONG ATTACK INPUT CHECK
            index = 1;
            if (myPlayerMovement.Actions.Attack2.WasPressed && (attackStg == AttackStage.ready || CanComboWithDifferentAttack(index)))//Input.GetButtonDown(myPlayerMovement.contName + "Y"))
            {
                if (CanComboWithDifferentAttack(index))
                {
                    EndAttack();
                }
                ChangeAttackType(index);
                StartAttack();
                //ChangeNextAttackType();
            }

            //SPECIAL ATTACK INPUT CHECK
            index = 2;
            if (myPlayerMovement.Actions.Attack3.WasPressed && (attackStg == AttackStage.ready || CanComboWithDifferentAttack(index)))//Input.GetButtonDown(myPlayerMovement.contName + "B"))
            {
                if (CanComboWithDifferentAttack(index))
                {
                    EndAttack();
                }
                ChangeAttackType(index);
                StartAttack();
                //ChangeNextAttackType();
            }

            //HOOK INPUT CHECK
            if (aiming && myPlayerMovement.Actions.Boost.WasPressed)//Input.GetButtonDown(myPlayerMovement.contName + "RB"))
            {
                myHook.StartHook();
                //ChangeAttackType(gC.attackHook);
                //StartAttack();
            }
        }

        ProcessAttack();
        //ProcessAttacksCD();

        if (myPlayerMovement.Actions.Aim.WasPressed && myPlayerWeap.hasWeapon)
        {
            StartAiming();
        }
        if (myPlayerMovement.Actions.Aim.WasReleased)
        {
            StopAiming();
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    void ChangeAttackType(int index)
    {
        attackIndex = index;
        AttackData attack = myAttacks[attackIndex].attack;
        
        Debug.Log("myAttacks[" + attackIndex + "].attack = " + myAttacks[attackIndex].attack);

        attackNameText.text = attack.attackName;
        chargingTime = attack.chargingTime;
        startupTime = attack.startupTime;
        activeTime = attack.activeTime;
        recoveryTime = attack.recoveryTime;
        knockBackSpeed = attack.knockbackSpeed;
        //change hitbox
        if (hitboxes.childCount > 0)
        {
            for (int i = 0; i < hitboxes.childCount; i++)
            {
                Destroy(hitboxes.GetChild(i).gameObject);
            }
        }

        GameObject newHitbox = Instantiate(attack.hitboxPrefab, hitboxes, false);
        hitbox = newHitbox.GetComponent<Collider>();
        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
    }

    void HideAttackHitBox()
    {
        if (hitboxes.childCount > 0)
        {
            for (int i = 0; i < hitboxes.childCount; i++)
            {
                Destroy(hitboxes.GetChild(i).gameObject);
            }
        }
    }

    /*void ChangeNextAttackType()
    {
            attackIndex++;
            if (attackIndex >= GameController.instance.allAttacks.Length)
            {
                attackIndex = 0;
            }
            attackName.text = GameController.instance.allAttacks[attackIndex].attackName;
            chargingTime = GameController.instance.allAttacks[attackIndex].chargingTime;
            startupTime = GameController.instance.allAttacks[attackIndex].startupTime;
            activeTime = GameController.instance.allAttacks[attackIndex].activeTime;
            recoveryTime = GameController.instance.allAttacks[attackIndex].recoveryTime;
            knockBackSpeed = GameController.instance.allAttacks[attackIndex].knockbackSpeed;
            //change hitbox
            if (hitboxes.childCount > 0)
            {
                for (int i = 0; i < hitboxes.childCount; i++)
                {
                    Destroy(hitboxes.GetChild(i).gameObject);
                }
            }

            GameObject newHitbox = Instantiate(GameController.instance.allAttacks[attackIndex].hitboxPrefab, hitboxes, false);
            hitbox = newHitbox.GetComponent<Collider>();
            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
    }*/

    void StartAttack()
    {
        if (attackStg == AttackStage.ready && !myPlayerMovement.noInput)
        {
            targetsHit.Clear();
            attackTime = 0;
            attackStg = chargingTime > 0 ? AttackStage.charging : AttackStage.startup;
            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[1];
        }
    }

    void ProcessAttack()
    {
        if (attackStg != AttackStage.ready)
        {
            attackTime += Time.deltaTime;
            switch (attackStg)
            {
                case AttackStage.ready:
                    break;
                case AttackStage.charging:
                    break;
                case AttackStage.startup:

                    //animacion startup
                    if (attackTime >= startupTime)
                    {
                        attackTime = 0;
                        attackStg = AttackStage.active;
                        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[2];
                    }
                    break;
                case AttackStage.active:
                    if (attackTime >= activeTime)
                    {
                        attackTime = 0;
                        attackStg = AttackStage.recovery;
                        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[3];
                    }
                    break;
                case AttackStage.recovery:
                    if (attackTime >= recoveryTime)
                    {
                        EndAttack();
                    }
                    break;
            }
        }
    }

    void EndAttack()
    {
        attackTime = 0;
        attackStg = AttackStage.ready;
        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
        HideAttackHitBox();

        //myAttacks[attackIndex].StartCD();
    }

    bool CanComboWithDifferentAttack(int attackButton)//0 -> basic attack (X); 1 -> strong attack(Y), 2 -> special attack(B).
    {
        bool result = false;
        if (attackButton != attackIndex)
        {
            if (attackStg == AttackStage.recovery)
            {
                float timeLimit = recoveryTime - ((recoveryTime * myAttacks[attackIndex].attack.comboDifferentAttackPercent) / 100);
                if (attackTime >= timeLimit)
                {
                    result = true;
                }
            }
        }
        return result;
    }

    //void ProcessAttacksCD()
    //{
    //    for(int i = 0; i < myAttacks.Count; i++)
    //    {
    //        //Debug.LogWarning("Attack "+myAttacks[i].attack.attackName+" in cd? "+myAttacks[i].cdStarted);
    //       if(myAttacks[i].cdStarted)
    //        {
    //            //print("Process CD attack + " + i);
    //            myAttacks[i].ProcessCD();
    //        }
    //    }
    //}
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    public void FillMyAttacks(WeaponData weaponData)
    {
        AttackInfo att = new AttackInfo(weaponData.basicAttack);
        myAttacks.Add(att);
        //Debug.Log("myAttacks[" + (myAttacks.Count - 1) + "].attack = " + myAttacks[myAttacks.Count - 1].attack.name);
        att = new AttackInfo(weaponData.strongAttack);
        myAttacks.Add(att);
        //Debug.Log("myAttacks[" + (myAttacks.Count - 1) + "].attack = " + myAttacks[myAttacks.Count - 1].attack.name);

        //SPECIAL ATTACK
        //att = new AttackInfo(weaponData.specialAttack);
        //myAttacks.Add(att);
        //Debug.Log("myAttacks[" + (myAttacks.Count - 1) + "].attack = " + myAttacks[myAttacks.Count - 1].attack.name);
    }

    public void EmptyMyAttacks()
    {
        myAttacks.Clear();
    }

    public void StartAiming()
    {
        if (!aiming)
        {
            aiming = true;
            myPlayerMovement.myCamera.SwitchCamera(cameraMode.Shoulder);
            myPlayerWeap.AttachWeaponToBack();
            myPlayerHUD.StartAim();
            //ChangeAttackType(GameController.instance.attackHook);
        }
    }

    public void StopAiming()
    {
        if (aiming)
        {
            aiming = false;
            myPlayerMovement.myCamera.SwitchCamera(cameraMode.Free);
            myPlayerWeap.AttachWeapon();
            myPlayerHUD.StopAim();
        }
    }
    #endregion

    #region ----[ PUN CALLBACKS ]----
    #endregion

    #region ----[ RPC ]----
    #endregion

    #region ----[ NETWORK FUNCTIONS ]----
    #endregion

    #region ----[ IPUNOBSERVABLE ]----
    #endregion
}

#region ----[ STRUCTS ]----
public class AttackInfo
{
    public AttackData attack;
    //public float cdTime;
    //public bool cdStarted;
    public AttackInfo(AttackData _attack)
    {
        attack = _attack;
        //cdTime = 0;
        //cdStarted = false;
    }
    //public void StartCD()
    //{
    //    cdTime = 0;
    //    cdStarted = true;
    //    //Debug.Log("CD STARTED - ATTACK " + attack.attackName);
    //}

    //public void ProcessCD()
    //{
    //    //Debug.Log("CD PROCESS - ATTACK " + attack.attackName + "; cdTime = " + cdTime);
    //    cdTime += Time.deltaTime;
    //    if (cdTime >= attack.cdTime)
    //    {
    //        StopCD();
    //    }
    //}

    //public void StopCD()
    //{
    //    cdTime = 0;
    //    cdStarted = false;
    //    //Debug.Log("CD FINISHED - ATTACK " + attack.attackName);
    //}
}
#endregion



