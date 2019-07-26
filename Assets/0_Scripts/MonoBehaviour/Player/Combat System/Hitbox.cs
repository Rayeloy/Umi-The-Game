using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [HideInInspector]
    public PlayerMovement myPlayerMov;
    [HideInInspector]
    public PlayerCombatNew myPlayerCombatNew;
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
                Vector3 resultKnockback = Vector3.zero;
                EffectType stunLikeEffect = EffectType.none;
                float maxStunTime = 0;
                bool encontrado = false;
                switch (col.tag)
                {
                    #region --- PLAYER ---
                    case "Player":
                        PlayerMovement otherPlayer = col.GetComponent<PlayerBody>().myPlayerMov;
                        if (myPlayerMov.team != otherPlayer.team && !myPlayerCombatNew.parryStarted)
                        {
                            encontrado = false;
                            for (int i = 0; i < targetsHit.Count && !encontrado; i++)
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
                                for (int i = 0; i < myAttackHitbox.effects.Length; i++)
                                {
                                    if (stunLikeEffect != EffectType.parry)
                                    {
                                        #region EFFECT TYPE SWITCH
                                        switch (myAttackHitbox.effects[i].effectType)
                                        {
                                            case EffectType.knockback:
                                                //calculate knockback vector
                                                if (!myPlayerMov.disableAllDebugs) Debug.Log("KNOCKBACK TYPE= " + myAttackHitbox.effects[i].knockbackType);
                                                switch (myAttackHitbox.effects[i].knockbackType)
                                                {
                                                    case KnockbackType.outwards:
                                                        Vector3 myPos = myPlayerMov.transform.position;
                                                        Vector3 colPos = col.transform.position;
                                                        resultKnockback = new Vector3(colPos.x - myPos.x, 0, colPos.z - myPos.z).normalized;
                                                        resultKnockback = Quaternion.Euler(0, myAttackHitbox.effects[i].knockbackYAngle, 0) * resultKnockback;
                                                        resultKnockback = CalculateYAngle(col.transform.position, resultKnockback.normalized, myAttackHitbox.effects[i].knockbackYAngle);
                                                        resultKnockback = resultKnockback.normalized * myAttackHitbox.effects[i].knockbackMagnitude;
                                                        break;
                                                    case KnockbackType.inwards:
                                                        myPos = myPlayerMov.transform.position;
                                                        colPos = col.transform.position;
                                                        resultKnockback = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                                        resultKnockback = Quaternion.Euler(0, myAttackHitbox.effects[i].knockbackYAngle, 0) * resultKnockback;
                                                        resultKnockback = CalculateYAngle(col.transform.position, resultKnockback, myAttackHitbox.effects[i].knockbackYAngle);
                                                        resultKnockback = resultKnockback.normalized * myAttackHitbox.effects[i].knockbackMagnitude;
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
                                                        resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
                                                        //print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                                        break;
                                                    case KnockbackType.autoCenter:
                                                        float a = Mathf.Abs(myPlayerMov.breakAcc);
                                                        float iT = myPlayerMov.MissingImpulseTime();
                                                        float impulseDist = (a * Mathf.Pow(iT, 2)) / 2;
                                                        if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("iT = " + iT + "; impulseDist = " + impulseDist);

                                                        Vector3 hitDir = myPlayerCombatNew.currentAttack.impulseMagnitude != 0 ? myPlayerCombatNew.currentImpulse.normalized : myPlayerMov.rotateObj.forward;
                                                        float meNoMaeDist = myAttackHitbox.effects[i].knockbackMagnitude + impulseDist;
                                                        Vector3 meNoMaePos = myPlayerMov.rotateObj.position + (hitDir * meNoMaeDist);//me no mae (目の前) means in front of your eyes
                                                        resultKnockback = (meNoMaePos - otherPlayer.transform.position);
                                                        resultKnockback.y = 0;
                                                        Debug.DrawLine(otherPlayer.transform.position, (meNoMaePos), Color.red, 4);
                                                        if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("hitDir = " + hitDir + "; meNoMaeDist = " + meNoMaeDist + "; meNoMaePos = " + meNoMaePos);

                                                        float d = resultKnockback.magnitude;
                                                        float vi = Mathf.Sqrt((2 * a * d));
                                                        if (!myPlayerMov.disableAllDebugs) Debug.LogWarning("distance = " + d.ToString("F4") + "; Initial velocity = " + vi.ToString("F4") +
                                                            "; myPlayerMov.breakAcc = " + a);
                                                        resultKnockback = resultKnockback.normalized * vi;
                                                        break;
                                                    case KnockbackType.redirect:
                                                        float inputRedirectAngle = SignedRelativeAngle(myPlayerMov.rotateObj.forward, myPlayerMov.currentInputDir, Vector3.up);
                                                        float finalRedirectAngle = Mathf.Clamp(inputRedirectAngle, -myAttackHitbox.effects[i].redirectMaxAngle, myAttackHitbox.effects[i].redirectMaxAngle);
                                                        resultKnockback = Quaternion.Euler(0, finalRedirectAngle, 0) * myPlayerMov.rotateObj.forward;
                                                        //CALCULATE Y ANGLE
                                                        resultKnockback = CalculateYAngle(col.transform.position, resultKnockback.normalized, myAttackHitbox.effects[i].knockbackYAngle);
                                                        //resultKnockback *= myAttackHitbox.effects[i].knockbackMagnitude;
                                                        resultKnockback = resultKnockback.normalized * myAttackHitbox.effects[i].knockbackMagnitude;
                                                        if (!myPlayerMov.disableAllDebugs)
                                                        {
                                                            //Debug.LogError("myPlayerMov.currentInputDir = " + myPlayerMov.currentInputDir+ "; resultKnockback = " + resultKnockback);
                                                            Debug.DrawRay(myPlayerMov.transform.position, resultKnockback.normalized * 1, Color.red);
                                                        }
                                                        break;
                                                }
                                                if (!myPlayerMov.disableAllDebugs) print("KNOCKBACK RESULT= " + resultKnockback);
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
                                                //Reduce Recovery Time Player 1
                                                //StartRecieveParry Player2
                                                if (stunLikeEffect != EffectType.knockdown && stunLikeEffect != EffectType.stun)
                                                {
                                                    stunLikeEffect = EffectType.softStun;
                                                    maxStunTime = myAttackHitbox.effects[i].parryStunTime;
                                                }
                                                //Knockback outwards
                                                Vector3 parryMyPos = myPlayerMov.transform.position;
                                                Vector3 parryColPos = col.transform.position;
                                                resultKnockback = new Vector3(parryColPos.x - parryMyPos.x, 0, parryColPos.z - parryMyPos.z).normalized;
                                                resultKnockback = Quaternion.Euler(0, myAttackHitbox.effects[i].knockbackYAngle, 0) * resultKnockback;
                                                resultKnockback = CalculateYAngle(col.transform.position, resultKnockback, myAttackHitbox.effects[i].knockbackYAngle);
                                                resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
                                                break;
                                        }
                                        #endregion
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
                            for (int i = 0; i < myAttackHitbox.effects.Length; i++)
                            {
                                #region EFFECT TYPE SWITCH
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
                                                resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
                                                break;
                                            case KnockbackType.inwards:
                                                myPos = myPlayerMov.transform.position;
                                                colPos = col.transform.position;
                                                resultKnockback = new Vector3(myPos.x - colPos.x, 0, myPos.z - colPos.z).normalized;
                                                resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
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
                                                resultKnockback = resultKnockback * myAttackHitbox.effects[i].knockbackMagnitude;
                                                //print("Facing Angle(localRot.y)= " + facingAngle + "; customDir = " + customDir);
                                                break;
                                            case KnockbackType.autoCenter:
                                                //Vector3 meNoMaePos = myPlayerMov.transform.position + (myPlayerMov.rotateObj.forward*myAttackHitbox.effects[i].knockbackMagnitude);
                                                //resultKnockback = meNoMaePos - otherPlayer.transform.position;
                                                //float a = Mathf.Abs(myPlayerMov.breakAcc);
                                                //float vi =;
                                                //resultKnockback
                                                break;
                                            case KnockbackType.redirect:
                                                break;
                                        }
                                        //print("KNOCKBACK DIR= " + result);
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
                                #endregion
                            }
                            Dummy dummy = col.GetComponent<Dummy>();
                            dummy.StartRecieveHit(resultKnockback);
                        }
                        break;
                    #endregion

                    #region --- HITBOX ---
                    case "Hitbox":
                        Hitbox otherHitbox = col.GetComponent<Hitbox>();
                        PlayerMovement enemy = otherHitbox.myPlayerMov;
                        if (otherHitbox != null && enemy != null)
                        {
                            if (myPlayerMov.team != enemy.team)
                            {
                                encontrado = false;
                                for (int i = 0; i < targetsHit.Count && !encontrado; i++)
                                {
                                    if (targetsHit[i] == enemy.playerNumber)
                                    {
                                        encontrado = true;
                                    }
                                }
                                if (!encontrado)
                                {
                                    targetsHit.Add(enemy.playerNumber);

                                    //QUE TIPO DE EFFECT
                                    int priorityDiff = (int)Mathf.Sign(myPlayerCombatNew.currentAttack.attackPriority - otherHitbox.myPlayerCombatNew.currentAttack.attackPriority);
                                    switch (priorityDiff)
                                    {
                                        case -1://TENEMOS MENOS PRIORIDAD
                                            break;
                                        case 1://TENEMOS MÁS PRIORIDAD
                                            for (int i = 0; i < myAttackHitbox.effects.Length; i++)
                                            {
                                                if (myAttackHitbox.effects[i].effectType == EffectType.parry && !enemy.myPlayerCombatNew.parryStarted)
                                                {
                                                    if (!myPlayerMov.disableAllDebugs) Debug.Log("PARRY!!!");
                                                    //Reduce Recovery Time Player 1
                                                    myPlayerCombatNew.HitParry();

                                                    //StartRecieveParry Player2
                                                    if (stunLikeEffect != EffectType.none && !myPlayerMov.disableAllDebugs) Debug.LogError("Error: There is another effect apart from the parry effect in the hitbox " + gameObject);
                                                    stunLikeEffect = EffectType.softStun;
                                                    maxStunTime = myAttackHitbox.effects[i].parryStunTime;
                                                    //Knockback outwards
                                                    Vector3 parryMyPos = myPlayerMov.transform.position;
                                                    Vector3 parryColPos = col.transform.position;
                                                    resultKnockback = new Vector3(parryColPos.x - parryMyPos.x, 0, parryColPos.z - parryMyPos.z).normalized;
                                                    resultKnockback = Quaternion.Euler(0, myAttackHitbox.effects[i].knockbackYAngle, 0) * resultKnockback;
                                                    resultKnockback = CalculateYAngle(col.transform.position, resultKnockback, 25f);
                                                    resultKnockback = resultKnockback * 10;
                                                    enemy.StartRecieveHit(myPlayerMov, resultKnockback, stunLikeEffect, maxStunTime);
                                                }
                                            }
                                            break;
                                        case 0://TENEMOS IGUAL PRIORIDAD
                                            break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("Couldn't find the hitbox or playerMovement scripts");
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
                    case "Hitbox":
                        otherPlayer = col.GetComponent<Hitbox>().myPlayerMov;
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

    Vector3 CalculateYAngle(Vector3 enemyPos, Vector3 originDir, float verticalAngle)
    {
        //Vector3 perpVector = new Vector3(-originDir.z, 0, originDir.x);
        //Calculate point along originDir

        Vector3 originDirPoint = enemyPos + originDir.normalized * 1;
        //tan C = c/b; height = c; verticalAngle = C; b = 1;
        float height = Mathf.Tan(verticalAngle * Mathf.Deg2Rad) * 1;
        Vector3 finalPoint = originDirPoint + Vector3.up * height;
        //Debug.DrawLine(enemyPos, originDirPoint, Color.white,4);
        //Debug.DrawLine(originDirPoint, finalPoint, Color.yellow,4);
        //Debug.DrawLine(finalPoint, enemyPos, Color.red,4);
        return (finalPoint - enemyPos);
    }
}
