using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatNew : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    PlayerMovement myPlayerMovement;
    PlayerWeapons myPlayerWeap;
    PlayerHook myHook;
    PlayerHUD myPlayerHUD;
    public Material[] hitboxMats;//0 -> charging; 1-> startup; 2 -> active; 3 -> recovery
    public Transform hitboxesParent;
    [HideInInspector]
    public Transform weaponEdge;
    [HideInInspector]
    public Transform weaponHandle;


    #endregion

    #region ----[ PROPERTIES ]----
    [HideInInspector]
    public WeaponData currentWeapon;
    bool hasWeapon
    {
        get
        {
            return currentWeapon != null && myPlayerWeap.hasWeapon;
        }
    }

    //INVULNERABILTY
    float invulTime = 0;
    float maxInvulTime = 0;
    [HideInInspector]
    public bool invulnerable=false;

    [HideInInspector]
    public AttackPhaseType attackStg = AttackPhaseType.ready;
    float attackTime = 0;
    AttackData currentAttack;

    //Autocombo
    Autocombo autocombo;
    [HideInInspector]
    public bool autocomboStarted;
    int autocomboIndex = -1;
    bool lastAutocomboAttackFinished = false;
    float autocomboTime = 0;
    List<GameObject> currentHitboxes;

    //Parry
    AttackData parry;

    [HideInInspector]
    public bool aiming;

    [HideInInspector]
    public bool isRotationRestricted
    {
        get
        {
            bool result = false;
            if (attackStg != AttackPhaseType.ready && currentAttack.GetAttackPhase(attackStg).restrictRotation)
                result = true;
            return result;
        }
    }
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovement>();
        myPlayerWeap = myPlayerMovement.myPlayerWeap;
        myHook = myPlayerMovement.myPlayerHook;
        myPlayerHUD = myPlayerMovement.myPlayerHUD;

        currentHitboxes = new List<GameObject>();
        attackStg = AttackPhaseType.ready;
        attackTime = 0;
    }
    #endregion

    #region Start
    public void KonoStart()
    {
        //FillMyAttacks();
        //ChangeAttackType(gC.attackX);
        HideAttackHitBox();
        //ChangeNextAttackType();
    }
    #endregion

    #region Update
    public void KonoUpdate()
    {
        //print("Trigger = " + Input.GetAxis(myPlayerMovement.contName + "LT"));

        if (!myPlayerMovement.noInput && !myPlayerMovement.inWater && (attackStg == AttackPhaseType.ready || attackStg == AttackPhaseType.recovery)
             && hasWeapon)
        {
            //Autocombo  INPUT CHECK
            if (myPlayerMovement.Actions.X.WasPressed && (attackStg == AttackPhaseType.ready ) && !myPlayerWeap.canPickupWeapon)//Input.GetButtonDown(myPlayerMovement.contName + "X"))
            {
                StartOrContinueAutocombo();
            }

            //Skill 1
            if (myPlayerMovement.Actions.Y.WasPressed && (attackStg == AttackPhaseType.ready))//Input.GetButtonDown(myPlayerMovement.contName + "Y"))
            {

            }

            //Skill 2
            if (myPlayerMovement.Actions.B.WasPressed && (attackStg == AttackPhaseType.ready))//Input.GetButtonDown(myPlayerMovement.contName + "B"))
            {

            }

            //HOOK INPUT CHECK
            if (aiming && myPlayerMovement.Actions.R2.WasPressed)//Input.GetButtonDown(myPlayerMovement.contName + "RB"))
            {
                myHook.StartHook();
                //ChangeAttackType(gC.attackHook);
                //StartAttack();
            }
        }
        ProcessInvulnerability();
        ProcessAttack();
        //ProcessAttacksCD();

        if (myPlayerMovement.Actions.L2.WasPressed && myPlayerWeap.hasWeapon)
        {
            StartAiming();
        }
        if (myPlayerMovement.Actions.L2.WasReleased)
        {
            StopAiming();
        }
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----

    #region --- Change Attack / Hitbox ---

    void ChangeAttackPhase(AttackPhaseType attackStage)
    {
        Debug.Log("Change attack phase to " + attackStage);
        attackStg = attackStage;

        //change hitbox
        //HideAttackHitBox();
        ChangeHitboxes(attackStg);
        if (currentAttack.GetAttackPhase(attackStg).invulnerability) StartInvulnerabilty(currentAttack.GetAttackPhase(attackStg).duration);

        switch (attackStg)
        {
            case AttackPhaseType.ready:
                myPlayerMovement.ResetPlayerRotationSpeed();
                HideAttackHitBox();
                break;
            case AttackPhaseType.charging:
                if (currentAttack.chargingPhase.restrictRotation)
                {
                    myPlayerMovement.SetPlayerRotationSpeed(currentAttack.chargingPhase.rotationSpeedPercentage);
                    myPlayerMovement.SetPlayerAttackMovementSpeed(currentAttack.chargingPhase.movementSpeed);
                }
                break;
            case AttackPhaseType.startup:
                if (currentAttack.startupPhase.restrictRotation)
                {
                    myPlayerMovement.SetPlayerRotationSpeed(currentAttack.startupPhase.rotationSpeedPercentage);
                    myPlayerMovement.SetPlayerAttackMovementSpeed(currentAttack.startupPhase.movementSpeed);
                }
                break;
            case AttackPhaseType.active:
                if (currentAttack.activePhase.restrictRotation)
                {
                    myPlayerMovement.SetPlayerRotationSpeed(currentAttack.activePhase.rotationSpeedPercentage);
                    myPlayerMovement.SetPlayerAttackMovementSpeed(currentAttack.activePhase.movementSpeed);
                }
                break;
            case AttackPhaseType.recovery:
                if (currentAttack.recoveryPhase.restrictRotation)
                {
                    myPlayerMovement.SetPlayerRotationSpeed(currentAttack.recoveryPhase.rotationSpeedPercentage);
                    myPlayerMovement.SetPlayerAttackMovementSpeed(currentAttack.recoveryPhase.movementSpeed);
                }
                break;
        }
    }

    void ChangeHitboxes(AttackPhaseType att)//hacer después de cambiar de attack stage, no antes!
    {
        //HideAttackHitBox();

        switch (attackStg)
        {
            case AttackPhaseType.charging:
                for (int i = 0; i < currentAttack.activePhase.attackHitboxes.Length; i++)
                {
                    AddHitbox(currentAttack.activePhase.attackHitboxes[i]);
                }
                for (int i = 0; i < currentHitboxes.Count; i++)
                {
                    currentHitboxes[i].GetComponent<MeshRenderer>().material = hitboxMats[0];
                }
                break;
            case AttackPhaseType.startup:
                if (!currentAttack.hasChargingPhase)
                {
                    for(int i=0; i < currentAttack.activePhase.attackHitboxes.Length; i++)
                    {
                        AddHitbox(currentAttack.activePhase.attackHitboxes[i]);
                    }
                }
                for(int i=0; i < currentHitboxes.Count; i++)
                {
                    currentHitboxes[i].GetComponent<MeshRenderer>().material = hitboxMats[1];
                }
                    break;
            case AttackPhaseType.active:
                for (int i = 0; i < currentHitboxes.Count; i++)
                {
                    currentHitboxes[i].GetComponent<MeshRenderer>().material = hitboxMats[2];
                }
                break;
            case AttackPhaseType.recovery:
                for (int i = 0; i < currentHitboxes.Count; i++)
                {
                    currentHitboxes[i].GetComponent<MeshRenderer>().material = hitboxMats[3];
                }
                break;
        }
    }

    void AddHitbox(AttackHitbox attackHitbox)
    {
        GameObject auxHitbox;
        switch (attackHitbox.parentType)
        {
            case HitboxParentType.player:
                auxHitbox = Instantiate(attackHitbox.hitboxPrefab,hitboxesParent);
                currentHitboxes.Add(auxHitbox);
                break;
            case HitboxParentType.weaponEdge:
                auxHitbox = Instantiate(attackHitbox.hitboxPrefab, weaponEdge);
                currentHitboxes.Add(auxHitbox);
                break;
            case HitboxParentType.weaponHandle:
                auxHitbox = Instantiate(attackHitbox.hitboxPrefab, weaponHandle);
                currentHitboxes.Add(auxHitbox);
                break;
            default:
                Debug.LogError("Error: the hitboxParentType is not supported: " + attackHitbox.parentType);
                break;
        }
    }

    void HideAttackHitBox()
    {
        for(int i=0;i<100 && currentHitboxes.Count>0;i++)
        {
            Destroy(currentHitboxes[0]);
            currentHitboxes.RemoveAt(0);
            if (i == 99) Debug.LogError("Error: Trying to hide Hitboxes but we are hiding too many??(99). This was done to avoid an infinite loop and game crash.");
        }
        if (hitboxesParent.childCount > 0)
        {
            for (int i = 0; i < hitboxesParent.childCount; i++)
            {
                Destroy(hitboxesParent.GetChild(i).gameObject);
            }
        }
    }

    #endregion

    #region --- AUTOCOMBO ---

    bool StartAutocombo()
    {
        bool exito = false;
        if (!autocomboStarted && attackStg == AttackPhaseType.ready && !myPlayerMovement.noInput && !aiming)
        {
            exito = true;
            autocomboStarted = true;
            autocomboTime = 0;
            autocomboIndex = -1;
            StartNextAttackAutocombo();
        }
        return exito;
    }

    bool StartNextAttackAutocombo()
    {
        bool exito = false;
        if (autocomboStarted && attackStg == AttackPhaseType.ready && lastAutocomboAttackFinished)
        {
            exito = true;
            autocomboIndex++;
            lastAutocomboAttackFinished = false;
            StartAttack(currentAttack);
        }
        return exito;
    }

    public bool StartOrContinueAutocombo(bool calledFromBuffer=false)
    {
        bool result = false;
        if (!autocomboStarted)
        {
            result = StartAutocombo();
        }
        else
        {
            result = StartNextAttackAutocombo();
        }
        if(!result && calledFromBuffer)
        {
            myPlayerMovement.BufferInput(PlayerInput.Autocombo);
        }
        return result;
    }


    void ProcessAutocombo()
    {
        if (autocomboStarted)
        {
            if (aiming)
            {
                StopAutocombo();
            }
            if (lastAutocomboAttackFinished)
            {
                autocomboTime += Time.deltaTime;
                if (autocomboTime >= autocombo.maxTimeBetweenAttacks)
                {
                    StopAutocombo();
                }
            }
        }
    }

    void StopAutocombo()
    {
        if (autocomboStarted)
        {
            if (attackStg != AttackPhaseType.ready)
            {
                EndAttack();
            }
            autocomboStarted = false;
            lastAutocomboAttackFinished = false;
            autocomboTime = 0;
            autocomboIndex = -1;
        }
    }

    void StartAttack(AttackData newAttack)
    {
        if (attackStg == AttackPhaseType.ready && !myPlayerMovement.noInput && !aiming)
        {
            currentAttack = newAttack;
            //targetsHit.Clear();
            attackTime = 0;
            attackStg = currentAttack.hasChargingPhase ? AttackPhaseType.charging : AttackPhaseType.startup;
            ChangeAttackPhase(attackStg);
        }
    }

    void ProcessAttack()
    {
        if (autocomboStarted && attackStg != AttackPhaseType.ready)
        {
            attackTime += Time.deltaTime;
            switch (attackStg)
            {
                case AttackPhaseType.charging:
                    if (attackTime >= currentAttack.chargingPhase.duration)
                    {
                        float charge = Mathf.Clamp01(attackTime / currentAttack.chargingPhase.duration);
                        attackTime = 0;
                        ChangeAttackPhase(AttackPhaseType.startup);
                    }
                    break;
                case AttackPhaseType.startup:
                    //animacion startup
                    if (attackTime >= currentAttack.startupPhase.duration)
                    {
                        attackTime = 0;
                        ChangeAttackPhase(AttackPhaseType.active);

                    }
                    break;
                case AttackPhaseType.active:
                    if (attackTime >= currentAttack.activePhase.duration)
                    {
                        attackTime = 0;
                        ChangeAttackPhase(AttackPhaseType.recovery);
                        //Do impulse
                        CalculateImpulse(currentAttack);
                    }
                    break;
                case AttackPhaseType.recovery:
                    if (attackTime >= currentAttack.recoveryPhase.duration)
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
        ChangeAttackPhase(AttackPhaseType.ready);
        if (autocomboStarted)
        {
            lastAutocomboAttackFinished = true;
            if (autocomboIndex >= autocombo.attacks.Length)
            {
                StopAutocombo();
            }
        }
        //myAttacks[attackIndex].StartCD();
    }
    
    void CalculateImpulse(AttackData attack)
    {
        Vector3 impulse = attack.impulseDir.normalized * attack.impulseMagnitude;
        myPlayerMovement.DoImpulse(impulse);
    }

    #endregion

    #region --- PARRY ---
    void StartParry()
    {

    }

    public void HitParryEffect()
    {

    }
    #endregion

    #region --- INVULNERABILTY ---
    public void StartInvulnerabilty(float maxTime)
    {
        float missingInvulTime = 0;
        if(invulnerable) missingInvulTime = maxInvulTime - invulTime;

        if (!invulnerable || (invulnerable && maxTime > (missingInvulTime)))
        {
            invulnerable = true;
            invulTime = 0;
            maxInvulTime = maxTime;
        }
    }

    void ProcessInvulnerability()
    {
        invulTime += Time.deltaTime;
        if (invulTime >= maxInvulTime)
        {
            StopInvulnerability();
        }
    }

    void StopInvulnerability()
    {
        invulTime = 0;
        maxInvulTime = 0;
        invulnerable = false;
    }
    #endregion

    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----

    public void InitializeCombatSystem(Weapon weapon)
    {
        weaponEdge = myPlayerWeap.currentWeapon.weaponEdge;
        weaponHandle = myPlayerWeap.currentWeapon.weaponHandle;

        currentWeapon = weapon.weaponData;
        autocombo = currentWeapon.autocombo;
        parry = currentWeapon.parry;

        //Skill 1

        //Skill 2
    }

    public void DropWeapon()
    {
        weaponEdge = null;
        weaponHandle = null;

        currentWeapon = null;
        autocombo = null;
        parry = null;

        //Skill 1

        //Skill 2
    }

    public void StopDoingCombat()
    {
        StopAutocombo();
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
            myPlayerWeap.AttatchWeapon();
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

#endregion
