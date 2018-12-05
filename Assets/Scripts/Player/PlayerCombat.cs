

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerHook))]
public class PlayerCombat : MonoBehaviour {
    PlayerMovement myPlayerMovement;
    PlayerWeapons myPlayerWeap;
    PlayerHook myHook;
    PlayerHUD myPlayerHUD;
    public float triggerDeadZone=0.15f;
    //List<string> attacks;
    [HideInInspector]
    public int attackIndex = 0;
    float chargingTime;
    float startupTime;
    float activeTime;
    float recoveryTime;
    public Material[] hitboxMats;
    [HideInInspector]
    public float knockBackSpeed = 30f;
    public Text attackName;
    [HideInInspector]
    public bool conHinchador = false;

    [HideInInspector]
    public List<string> targetsHit;

    [HideInInspector]
    public attackStage attackStg = attackStage.ready;
    public enum attackStage
    {
        ready=0,
        charging=1,
        startup=2,
        active=3,
        recovery=4
    }
    [HideInInspector]
    public List<AttackInfo> myAttacks;//index: 0 = X; 1 = Y; 2 = B
    
    public Transform hitboxes;
    Collider hitbox;
    //public Collider hitbox;
    private void Awake()
    {
        myPlayerMovement = GetComponent<PlayerMovement>();
        myPlayerWeap = GetComponent<PlayerWeapons>();
        myHook = GetComponent<PlayerHook>();
        myPlayerHUD = myPlayerMovement.myPlayerHUD;
        attackStg = attackStage.ready;
        targetsHit = new List<string>();
        myAttacks = new List<AttackInfo>();
    }

    private void Start()
    {
        FillMyAttacks();
        attackIndex = -1;
        //ChangeAttackType(GameController.instance.attackX);
        HideAttackHitBox();
        //ChangeNextAttackType();
    }

    public void KonoUpdate()
    {
        //print("Trigger = " + Input.GetAxis(myPlayerMovement.contName + "LT"));
        if (!myPlayerMovement.noInput && !myPlayerMovement.inWater && attackStg == attackStage.ready && !conHinchador)
        {
            if (myPlayerMovement.Actions.Attack1.WasPressed && !myAttacks[0].cdStarted)//Input.GetButtonDown(myPlayerMovement.contName + "X"))
            {
                ChangeAttackType(0);
                StartAttack();
            }
            if (myPlayerMovement.Actions.Attack2.WasPressed && !myAttacks[1].cdStarted)//Input.GetButtonDown(myPlayerMovement.contName + "Y"))
            {
                ChangeAttackType(1);
                StartAttack();
                //ChangeNextAttackType();
            }
            if (myPlayerMovement.Actions.Attack3.WasPressed && !myAttacks[2].cdStarted)//Input.GetButtonDown(myPlayerMovement.contName + "B"))
            {
                ChangeAttackType(2);
                StartAttack();
                //ChangeNextAttackType();
            }
            if (aiming && myPlayerMovement.Actions.Boost.WasPressed)// HOOK      //Input.GetButtonDown(myPlayerMovement.contName + "RB"))
            {
                myHook.StartHook();
                //ChangeAttackType(GameController.instance.attackHook);
                //StartAttack();

            }
        }

        ProcessAttack();
        ProcessAttacksCD();

        if (myPlayerMovement.Actions.Aim.WasPressed)
        {
            StartAiming();
        }
        if (myPlayerMovement.Actions.Aim.WasReleased)
        {
            StopAiming();
        }
    }

    public void FillMyAttacks()
    {
        AttackInfo att = new AttackInfo(GameController.instance.attackX);
        myAttacks.Add(att);
        att = new AttackInfo(GameController.instance.attackY);
        myAttacks.Add(att);
        att = new AttackInfo(GameController.instance.attackB);
        myAttacks.Add(att);
    }

    public void ChangeAttackType(int index)
    {
        attackIndex = index;
        AttackData attack = myAttacks[attackIndex].attack;
        attackName.text = attack.attackName;
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

    public void HideAttackHitBox()
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

    float attackTime = 0;
    public void StartAttack()
    {
        if (attackStg == attackStage.ready && !myPlayerMovement.noInput)
        {
            targetsHit.Clear();
            attackTime = 0;
            attackStg = chargingTime>0? attackStage.charging : attackStage.startup;
            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[1];
        }
    }

    public void ProcessAttack()
    {
        if (attackStg != attackStage.ready)
        {
            attackTime += Time.deltaTime;
            switch (attackStg)
            {
                case attackStage.ready:
                    break;
                case attackStage.charging:
                    break;
                case attackStage.startup:

                    //animacion startup
                    if (attackTime >= startupTime)
                    {
                        attackTime = 0;
                        attackStg = attackStage.active;
                        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[2];
                    }
                    break;
                case attackStage.active:
                    if (attackTime >= activeTime)
                    {
                        attackTime = 0;
                        attackStg = attackStage.recovery;
                        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[3];
                    }
                    break;
                case attackStage.recovery:
                    if (attackTime >= recoveryTime)
                    {
                        attackTime = 0;
                        attackStg = attackStage.ready;
                        hitbox.GetComponent<MeshRenderer>().material = hitboxMats[0];
                        HideAttackHitBox();

                        myAttacks[attackIndex].StartCD();

                    }
                    break;
            }
        }   
    }

    void ProcessAttacksCD()
    {
        for(int i = 0; i < myAttacks.Count; i++)
        {
            //Debug.LogWarning("Attack "+myAttacks[i].attack.attackName+" in cd? "+myAttacks[i].cdStarted);
           if(myAttacks[i].cdStarted)
            {
                //print("Process CD attack + " + i);
                myAttacks[i].ProcessCD();
            }
        }
    }

    [HideInInspector]
    public bool aiming;
    public void StartAiming()
    {
        if(!aiming)
        {
            aiming = true;
            myPlayerMovement.myCamera.SwitchCamera(CameraController.cameraMode.Shoulder);
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
            myPlayerMovement.myCamera.SwitchCamera(CameraController.cameraMode.Free);
            myPlayerWeap.AttachWeapon();
            myPlayerHUD.StopAim();
        }
    }
}

public class AttackInfo
{
    public AttackData attack;
    public float cdTime;
    public bool cdStarted;
    public AttackInfo(AttackData _attack)
    {
        attack = _attack;
        cdTime = 0;
        cdStarted = false;
    }
    public void StartCD()
    {
        cdTime = 0;
        cdStarted = true;
        //Debug.Log("CD STARTED - ATTACK " + attack.attackName);
    }

    public void ProcessCD()
    {
        //Debug.Log("CD PROCESS - ATTACK " + attack.attackName + "; cdTime = " + cdTime);
        cdTime += Time.deltaTime;
        if (cdTime >= attack.cdTime)
        {
            StopCD();
        }
    }

    public void StopCD()
    {
        cdTime = 0;
        cdStarted = false;
        //Debug.Log("CD FINISHED - ATTACK " + attack.attackName);
    }
}
