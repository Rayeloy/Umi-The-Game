

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombat : MonoBehaviour {
    PlayerMovement myPlayerMovement;
    public float triggerDeadZone=0.15f;
    [HideInInspector]
    public bool LTPulsado = false;
    [HideInInspector]
    public bool RTPulsado = false;
    //List<string> attacks;
    [HideInInspector]
    public int attackIndex = 0;
    [HideInInspector]
    public AttackData currentAttack;
    float chargingTime;
    float startupTime;
    float activeTime;
    float recoveryTime;
    public Material[] hitboxMats;
    [HideInInspector]
    public float knockBackSpeed = 30f;
    public Text attackName;

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

    public GameObject churro;
    public Transform hitboxes;
    Collider hitbox;
    //public Collider hitbox;
    private void Awake()
    {
        myPlayerMovement = GetComponent<PlayerMovement>();
        attackStg = attackStage.ready;
        targetsHit = new List<string>();
    }

    private void Start()
    {
        attackIndex = -1;
        ChangeAttackType(GameController.instance.attackX);
        HideAttackHitBox();
        //ChangeNextAttackType();
    }

    public void KonoUpdate()
    {
        //print("Trigger = " + Input.GetAxis(myPlayerMovement.contName + "LT"));
        if (!myPlayerMovement.noInput && !myPlayerMovement.inWater && attackStg == attackStage.ready)
        {
            if (Input.GetButtonDown(myPlayerMovement.contName + "X"))
            {
                ChangeAttackType(GameController.instance.attackX);
                StartAttack();
            }
            if (Input.GetButtonDown(myPlayerMovement.contName + "Y"))
            {
                ChangeAttackType(GameController.instance.attackY);
                StartAttack();
                //ChangeNextAttackType();
            }
            if (Input.GetButtonDown(myPlayerMovement.contName + "B"))
            {
                ChangeAttackType(GameController.instance.attackB);
                StartAttack();
                //ChangeNextAttackType();
            }
            if (LTPulsado && !RTPulsado && Input.GetButtonDown(myPlayerMovement.contName + "RB"))
            {
                RTPulsado = true;
                ChangeAttackType(GameController.instance.attackHook);
                StartAttack();
            }
        }

        ProcessAttack();

        if (Input.GetButtonUp(myPlayerMovement.contName + "RB"))
        {
            RTPulsado = false;
        }

        if (Input.GetButtonDown(myPlayerMovement.contName + "LB") && !LTPulsado)
        {
            print("startAiming");
            LTPulsado = true;
            StartAiming();
        }
        if (Input.GetButtonUp(myPlayerMovement.contName + "LB") && LTPulsado)
        {
            print("stopAiming");
            LTPulsado = false;
            StopAiming();
        }
    }

    public void ChangeAttackType(AttackData attack)
    {
        currentAttack = attack;
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
        if (attackStg == attackStage.ready)
        {
            targetsHit.Clear();
            attackTime = 0;
            attackStg = chargingTime>0? attackStage.charging : attackStage.startup;
            hitbox.GetComponent<MeshRenderer>().material = hitboxMats[1];
        }
    }

    public void ProcessAttack()
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
                if(attackTime>= startupTime)
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
                }
                break;
        }
    }
    
    void StartAiming()
    {
        myPlayerMovement.myCamera.SwitchCamera(CameraControler.cameraMode.Shoulder);
        //ChangeAttackType(GameController.instance.attackHook);
    }

    void StopAiming()
    {
        myPlayerMovement.myCamera.SwitchCamera(CameraControler.cameraMode.Free);
    }
}
