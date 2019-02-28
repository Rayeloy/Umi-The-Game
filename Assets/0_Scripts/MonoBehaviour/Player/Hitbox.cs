using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    PlayerMovement myPlayerMov;
    PlayerCombat myPlayerCombat;

    private void Awake()
    {
            myPlayerMov = transform.GetComponentsInParent<PlayerMovement>()[0];
            myPlayerCombat = transform.GetComponentsInParent<PlayerCombat>()[0];
    }
    
    private void OnTriggerStay(Collider col)
    {
        //Debug.LogWarning("I'm " + gameObject.name);
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if (myPlayerCombat.attackStg == AttackStage.active)//(tag != "HookBigHB" && tag != "HookSmallHB") && 
            {
                //print("I'm " + myPlayerMov.gameObject.name + " and I collided with " + col.gameObject);
                bool encontrado = false;
                switch (col.tag)
                {
                    case "Player":
                        PlayerMovement otherPlayer = col.GetComponent<PlayerBody>().myPlayerMov;
                        if (myPlayerMov.team != otherPlayer.team)
                        {
                            encontrado = false;
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
                                //print("I'm " + myPlayerMov.gameObject.name + " and I Hit against " + col.gameObject);
                                myPlayerCombat.targetsHit.Add(col.name);
                                //calculate knockback vector
                                Vector3 result = Vector3.zero;
                                //print("KNOCKBACK TYPE= " + myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackType);
                                switch (myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackType)
                                {
                                    case KnockbackType.outwards:
                                        Vector3 myPos = myPlayerMov.transform.position;
                                        Vector3 colPos = col.transform.position;
                                        result = new Vector3(colPos.x - myPos.x, 0, colPos.z - myPos.z).normalized;
                                        break;
                                    case KnockbackType.inwards:
                                        myPos = myPlayerMov.transform.position;
                                        colPos = col.transform.position;
                                        result = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                        break;
                                    case KnockbackType.customDir:
                                        //calculate real direction based on character's facing direction
                                        float facingAngle = -myPlayerMov.facingAngle;
                                        Vector3 customDir = myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackDirection;

                                        float theta = facingAngle * Mathf.Deg2Rad;
                                        float cs = Mathf.Cos(theta);
                                        float sn = Mathf.Sin(theta);
                                        float px = customDir.x * cs - customDir.z * sn;
                                        float py = customDir.x * sn + customDir.z * cs;
                                        result = new Vector3(px, customDir.y, py).normalized;
                                        //print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                        break;
                                }
                                //print("KNOCKBACK DIR= " + result);
                                result = result * myPlayerCombat.knockBackSpeed;
                                otherPlayer.StartRecieveHit(result, myPlayerMov, myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.stunTime);
                            }
                        }
                        break;
                    case "Dummy":
                        encontrado = false;
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
                            //print("I'm " + myPlayerMov.gameObject.name + " and I Hit against " + col.gameObject);
                            myPlayerCombat.targetsHit.Add(col.name);
                            //calculate knockback vector
                            Vector3 result = Vector3.zero;
                            //print("KNOCKBACK TYPE= " + myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackType);
                            switch (myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackType)
                            {
                                case KnockbackType.outwards:
                                    Vector3 myPos = myPlayerMov.transform.position;
                                    Vector3 colPos = col.transform.position;
                                    result = new Vector3(colPos.x - myPos.x, 0, colPos.z - myPos.z).normalized;
                                    break;
                                case KnockbackType.inwards:
                                    myPos = myPlayerMov.transform.position;
                                    colPos = col.transform.position;
                                    result = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                    break;
                                case KnockbackType.customDir:
                                    //calculate real direction based on character's facing direction
                                    float facingAngle = -myPlayerMov.facingAngle;
                                    Vector3 customDir = myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackDirection;

                                    float theta = facingAngle * Mathf.Deg2Rad;
                                    float cs = Mathf.Cos(theta);
                                    float sn = Mathf.Sin(theta);
                                    float px = customDir.x * cs - customDir.z * sn;
                                    float py = customDir.x * sn + customDir.z * cs;
                                    result = new Vector3(px, customDir.y, py).normalized;
                                    //print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                    break;
                            }
                            //print("KNOCKBACK DIR= " + result);
                            result = result * myPlayerCombat.knockBackSpeed;
                            Dummy dummy = col.GetComponent<Dummy>();
                            dummy.StartRecieveHit(result);
                        }
                        break;
                }
            }
        }
    }
}
