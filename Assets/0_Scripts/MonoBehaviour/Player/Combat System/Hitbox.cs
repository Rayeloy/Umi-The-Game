using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    PlayerMovement myPlayerMov;
    PlayerCombatNew myPlayerCombatNew;
    public AttackHitbox myAttackHitbox;

    List<int> targetsHit;
    List<string> dummiesHit;

    private void Awake()
    {
        myPlayerMov = transform.GetComponentsInParent<PlayerMovement>()[0];
        myPlayerCombatNew = transform.GetComponentsInParent<PlayerCombatNew>()[0];
        targetsHit = new List<int>();
        dummiesHit = new List<string>();
    }
    
    private void OnTriggerStay(Collider col)
    {
        //Debug.LogWarning("I'm " + gameObject.name);
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if (myPlayerCombatNew.attackStg == AttackPhaseType.active)//(tag != "HookBigHB" && tag != "HookSmallHB") && 
            {
                //print("I'm " + myPlayerMov.gameObject.name + " and I collided with " + col.gameObject);
                bool encontrado = false;
                switch (col.tag)
                {
                    #region --- PLAYER ---
                    case "Player":
                        PlayerMovement otherPlayer = col.GetComponent<PlayerBody>().myPlayerMov;
                        if (myPlayerMov.team != otherPlayer.team)
                        {
                            encontrado = false;
                            for(int i=0; i < targetsHit.Count && !encontrado; i++)
                            {
                                if (targetsHit[i] == otherPlayer.playerNumber)
                                {
                                    encontrado = true;
                                }
                            }
                            if (!encontrado)
                            {
                                targetsHit.Add(otherPlayer.playerNumber);
                                //QUE TIPO DE EFFECT
                                Vector3 resultKnockback = Vector3.zero;
                                EffectType stunLikeEffect = EffectType.none;
                                float maxStunTime = 0;
                                for (int i=0; i < myAttackHitbox.effects.Length; i++)
                                {
                                    switch (myAttackHitbox.effects[i].effectType)
                                    {
                                        case EffectType.knockback:
                                            //calculate knockback vector
                                            //print("KNOCKBACK TYPE= " + myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackType);
                                            switch (myAttackHitbox.effects[i].knockbackType)
                                            {
                                                case KnockbackType.outwards:
                                                    Vector3 myPos = myPlayerMov.transform.position;
                                                    Vector3 colPos = col.transform.position;
                                                    resultKnockback = new Vector3(colPos.x - myPos.x, 0, colPos.z - myPos.z).normalized;
                                                    resultKnockback = Quaternion.Euler(0, myAttackHitbox.effects[i].knockbackYAngle, 0) * resultKnockback;
                                                    break;
                                                case KnockbackType.inwards:
                                                    myPos = myPlayerMov.transform.position;
                                                    colPos = col.transform.position;
                                                    resultKnockback = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                                    resultKnockback = Quaternion.Euler(0, myAttackHitbox.effects[i].knockbackYAngle, 0) * resultKnockback;
                                                    break;
                                                case KnockbackType.customDir:
                                                    //calculate real direction based on character's facing direction
                                                    float facingAngle = -myPlayerMov.facingAngle;
                                                    Vector3 customDir = myAttackHitbox.effects[i].knockbackDir;

                                                    float theta = facingAngle * Mathf.Deg2Rad;
                                                    float cs = Mathf.Cos(theta);
                                                    float sn = Mathf.Sin(theta);
                                                    float px = customDir.x * cs - customDir.z * sn;
                                                    float py = customDir.x * sn + customDir.z * cs;
                                                    resultKnockback = new Vector3(px, customDir.y, py).normalized;
                                                    //print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                                    break;
                                                case KnockbackType.autoCenter:
                                                    Vector3 meNoMaePos = myPlayerMov.rotateObj.position + myPlayerMov.rotateObj.forward * myAttackHitbox.effects[i].knockbackMagnitude;//me no mae (目の前) means in front of your eyes
                                                    resultKnockback = meNoMaePos - otherPlayer.transform.position;
                                                    break;
                                                case KnockbackType.redirect:
                                                    float inputRedirectAngle = SignedRelativeAngle(myPlayerMov.rotateObj.forward, myPlayerMov.currentInputDir, Vector3.up);
                                                    float finalRedirectAngle = Mathf.Clamp(inputRedirectAngle,-myAttackHitbox.effects[i].redirectMaxAngle, myAttackHitbox.effects[i].redirectMaxAngle);
                                                    resultKnockback = Quaternion.Euler(0, finalRedirectAngle, 0) * myPlayerMov.rotateObj.forward;
                                                    resultKnockback *= myAttackHitbox.effects[i].knockbackMagnitude;
                                                    break;
                                            }
                                            //print("KNOCKBACK DIR= " + result);
                                            resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
                                            break;
                                        case EffectType.knockdown:
                                            stunLikeEffect = EffectType.knockdown;
                                            maxStunTime = AttackEffect.knockdownTime;
                                            break;
                                        case EffectType.softStun:
                                            if (stunLikeEffect != EffectType.knockdown && stunLikeEffect != EffectType.stun)
                                            {
                                                stunLikeEffect = EffectType.softStun;
                                                maxStunTime = myAttackHitbox.effects[i].stunTime;
                                            }
                                            break;
                                        case EffectType.stun:
                                            if (stunLikeEffect != EffectType.knockdown)
                                            {
                                                stunLikeEffect = EffectType.stun;
                                                maxStunTime = myAttackHitbox.effects[i].stunTime;
                                            }
                                            break;
                                        case EffectType.parry:
                                            break;
                                    }
                                }
                                otherPlayer.StartRecieveHit(myPlayerMov, resultKnockback, stunLikeEffect, maxStunTime);
                                //print("I'm " + myPlayerMov.gameObject.name + " and I Hit against " + col.gameObject);
                            }
                        }
                        break;
                    #endregion

                    #region --- DUMMY ---
                    case "Dummy":
                        encontrado = false;
                        foreach (string n in dummiesHit)
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
                            dummiesHit.Add(col.name);
                            //calculate knockback vector
                            //QUE TIPO DE EFFECT
                            Vector3 resultKnockback = Vector3.zero;
                            EffectType stunLikeEffect = EffectType.none;
                            float maxStunTime = 0;
                            for (int i = 0; i < myAttackHitbox.effects.Length; i++)
                            {
                                switch (myAttackHitbox.effects[i].effectType)
                                {
                                    case EffectType.knockback:
                                        //calculate knockback vector
                                        //print("KNOCKBACK TYPE= " + myPlayerCombat.myAttacks[myPlayerCombat.attackIndex].attack.knockbackType);
                                        switch (myAttackHitbox.effects[i].knockbackType)
                                        {
                                            case KnockbackType.outwards:
                                                Vector3 myPos = myPlayerMov.transform.position;
                                                Vector3 colPos = col.transform.position;
                                                resultKnockback = new Vector3(colPos.x - myPos.x, 0, colPos.z - myPos.z).normalized;
                                                break;
                                            case KnockbackType.inwards:
                                                myPos = myPlayerMov.transform.position;
                                                colPos = col.transform.position;
                                                resultKnockback = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                                break;
                                            case KnockbackType.customDir:
                                                //calculate real direction based on character's facing direction
                                                float facingAngle = -myPlayerMov.facingAngle;
                                                Vector3 customDir = myAttackHitbox.effects[i].knockbackDir;

                                                float theta = facingAngle * Mathf.Deg2Rad;
                                                float cs = Mathf.Cos(theta);
                                                float sn = Mathf.Sin(theta);
                                                float px = customDir.x * cs - customDir.z * sn;
                                                float py = customDir.x * sn + customDir.z * cs;
                                                resultKnockback = new Vector3(px, customDir.y, py).normalized;
                                                //print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                                break;
                                            case KnockbackType.autoCenter:
                                                break;
                                            case KnockbackType.redirect:
                                                break;
                                        }
                                        //print("KNOCKBACK DIR= " + result);
                                        resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
                                        break;
                                    case EffectType.knockdown:
                                        stunLikeEffect = EffectType.knockdown;
                                        maxStunTime = AttackEffect.knockdownTime;
                                        break;
                                    case EffectType.softStun:
                                        if (stunLikeEffect != EffectType.knockdown && stunLikeEffect != EffectType.stun)
                                        {
                                            stunLikeEffect = EffectType.softStun;
                                            maxStunTime = myAttackHitbox.effects[i].stunTime;
                                        }
                                        break;
                                    case EffectType.stun:
                                        if (stunLikeEffect != EffectType.knockdown)
                                        {
                                            stunLikeEffect = EffectType.stun;
                                            maxStunTime = myAttackHitbox.effects[i].stunTime;
                                        }
                                        break;
                                    case EffectType.parry:
                                        break;
                                }
                            }
                            Dummy dummy = col.GetComponent<Dummy>();
                            dummy.StartRecieveHit(resultKnockback);
                        }
                        break;
                        #endregion
                }
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.gameObject != myPlayerMov.gameObject)
        {
            if (myPlayerCombatNew.attackStg == AttackPhaseType.active)//(tag != "HookBigHB" && tag != "HookSmallHB") && 
            {
                //print("I'm " + myPlayerMov.gameObject.name + " and I collided with " + col.gameObject);
                switch (col.tag)
                {
                    case "Player":
                        PlayerMovement otherPlayer = col.GetComponent<PlayerBody>().myPlayerMov;
                        if (myPlayerMov.team != otherPlayer.team)
                        {
                            bool encontrado = false;
                            for (int i = 0; i < targetsHit.Count && !encontrado; i++)
                            {
                                if (targetsHit[i] == otherPlayer.playerNumber)
                                {
                                    encontrado = true;
                                    targetsHit.RemoveAt(i);
                                }
                            }
                        }
                        break;
                    case "Dummy":
                        Dummy dummy = col.GetComponent<Dummy>();
                            bool encontrado1 = false;
                            for (int i = 0; i < targetsHit.Count && !encontrado1; i++)
                            {
                                if (dummiesHit[i] == dummy.name)
                                {
                                    encontrado1 = true;
                                dummiesHit.RemoveAt(i);
                                }
                            }
                        break;
                }
            }
        }
    }

    //AUXILIAR 

    /// <summary>
    /// //Funcion que calcula el angulo de un vector respecto a otro que se toma como referencia de "foward"
    /// </summary>
    /// <param name="referenceForward"></param>
    /// <param name="newDirection"></param>
    /// <returns></returns>
    float SignedRelativeAngle(Vector3 referenceForward, Vector3 newDirection, Vector3 referenceUp)
    {
        // the vector perpendicular to referenceForward (90 degrees clockwise)
        // (used to determine if angle is positive or negative)
        Vector3 referenceRight = Vector3.Cross(referenceUp, referenceForward);
        // Get the angle in degrees between 0 and 180
        float angle = Vector3.Angle(newDirection, referenceForward);
        // Determine if the degree value should be negative.  Here, a positive value
        // from the dot product means that our vector is on the right of the reference vector   
        // whereas a negative value means we're on the left.
        float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
        return (sign * angle);//final angle
    }
}
