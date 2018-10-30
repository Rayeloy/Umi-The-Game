using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    PlayerMovement myPlayerMov;
    PlayerCombat myPlayerCombat;
    Hook myHook;

    private void Awake()
    {
        if(tag != "HookBigHB" && tag != "HookSmallHB")
        {
            myPlayerMov = transform.GetComponentsInParent<PlayerMovement>()[0];
            myPlayerCombat = transform.GetComponentsInParent<PlayerCombat>()[0];
            myHook = transform.GetComponentsInParent<Hook>()[0];
        }
    }
    public void KonoAwake(PlayerMovement playerMov, Hook hook)
    {
        myPlayerMov = playerMov;
        myHook = hook;
    }
    private void OnTriggerEnter(Collider col)
    {
        //Debug.LogWarning("I'm " + gameObject.name);
        if (col.gameObject != myPlayerMov.gameObject)
        {

            if (tag == "HookBigHB")
            {
                if (myHook.canHookSomething)
                {
                    if (col.tag == "Flag")
                    {
                        myHook.HookObject(col.transform);
                    }
                    if (col.tag == "Player")
                    {
                        if (myPlayerMov.team != col.GetComponent<PlayerMovement>().team)// IF ENEMY
                        {
                            if (!col.GetComponent<PlayerMovement>().inWater)// OUTSIDE WATER
                            {
                                myHook.HookPlayer(col.GetComponent<PlayerMovement>());
                            }
                        }
                        else
                        {
                            if (col.GetComponent<PlayerMovement>().inWater)//IF ALLY IN WATER
                            {
                                myHook.HookPlayer(col.GetComponent<PlayerMovement>());
                            }
                        }
                    }
                }
            }
            else if (tag == "HookSmallHB")
            {
                if (col.tag == "Stage")
                {
                    myHook.StopHook();
                }
            }
        }
    }
    private void OnTriggerStay(Collider col)
    {
        //Debug.LogWarning("I'm " + gameObject.name);
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if ((tag != "HookBigHB" && tag != "HookSmallHB") && myPlayerCombat.attackStg == PlayerCombat.attackStage.active)
            {
                //print("I'm " + myPlayerMov.gameObject.name + " and I collided with " + col.gameObject);
                if (col.tag == "Player" && myPlayerMov.team != col.GetComponent<PlayerMovement>().team)
                {
                    bool encontrado = false;
                    foreach (string n in myPlayerCombat.targetsHit)
                    {
                        if (n == col.name)
                        {
                            encontrado = true;
                            break;
                        }
                    }
                    if (!encontrado)
                    {
                        //QUE TIPO DE GOLPE
                        print("I'm " + myPlayerMov.gameObject.name + " and I Hit against " + col.gameObject);
                        myPlayerCombat.targetsHit.Add(col.name);
                        //calculate knockback vector
                        Vector3 result = Vector3.zero;
                        print("KNOCKBACK TYPE= " + myPlayerCombat.currentAttack.knockbackType);
                        switch (myPlayerCombat.currentAttack.knockbackType)
                        {
                            case AttackData.KnockbackType.outwards:
                                Vector3 myPos = myPlayerMov.transform.position;
                                Vector3 colPos = col.transform.position;
                                result = new Vector3(colPos.x - myPos.x, 0, colPos.z - myPos.z).normalized;
                                break;
                            case AttackData.KnockbackType.inwards:
                                myPos = myPlayerMov.transform.position;
                                colPos = col.transform.position;
                                result = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                break;
                            case AttackData.KnockbackType.customDir:
                                //calculate real direction based on character's facing direction
                                float facingAngle = -myPlayerMov.facingAngle;
                                Vector3 customDir = myPlayerCombat.currentAttack.knockbackDirection;

                                float theta = facingAngle * Mathf.Deg2Rad;
                                float cs = Mathf.Cos(theta);
                                float sn = Mathf.Sin(theta);
                                float px = customDir.x * cs - customDir.z * sn;
                                float py = customDir.x * sn + customDir.z * cs;
                                result = new Vector3(px, customDir.y, py).normalized;
                                print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                break;
                        }
                        print("KNOCKBACK DIR= " + result);
                        result = result * myPlayerCombat.knockBackSpeed;
                        col.GetComponent<PlayerMovement>().StartRecieveHit(result, myPlayerMov, myPlayerCombat.currentAttack.stunTime);
                    }
                }
            }
        }
    }
}
