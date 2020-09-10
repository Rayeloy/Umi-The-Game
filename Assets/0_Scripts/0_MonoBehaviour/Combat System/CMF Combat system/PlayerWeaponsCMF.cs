using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region ----[ PUBLIC ENUMS ]----
#endregion
public class PlayerWeaponsCMF : MonoBehaviour
{
    #region ----[ VARIABLES FOR DESIGNERS ]----
    public bool debugModeOn = false;

    //Referencias
    [Header("Referencias")]
    public PlayerAnimationCMF myPlayerAnim;
    public PlayerMovementCMF myPlayerMovement;
    public PlayerCombatCMF myPlayerCombatNew;
    PlayerHUDCMF myPlayerHUD;
    [HideInInspector] public WeaponSkinData myWeaponSkinData;
    [HideInInspector] public WeaponSkinRecolor myWeaponSkinRecolor;
    WeaponOffsets myWeaponSkinOffsets;

    #endregion

    #region ----[ PROPERTIES ]----
    [HideInInspector]
    public WeaponData currentWeaponData;
    [HideInInspector]
    public Weapon currentWeapon;
    [HideInInspector]
    public WeaponSkin currentWeaponSkin = null;
    private Transform currentWeapObject;
    [HideInInspector]
    public List<Weapon> weaponsNearby;
    Weapon nearestWeapon;

    [HideInInspector]
    public bool hasWeapon
    {
        get
        {
            bool result = false;
            if (currentWeapon != null)
            {
                result = true;
            }
            return result;
        }
        set { }
    }

    [HideInInspector]
    public bool canPickupWeapon
    {
        get
        {
            bool result = false;
            if (nearestWeapon != null)
            {
                result = true;
            }
            return result;
        }
        set { }
    }
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        currentWeaponData = null;
        currentWeapon = null;
        currentWeapObject = null;
        myPlayerHUD = myPlayerMovement.myPlayerHUD;
        //myPlayerModel = myPlayerMovement.myPlayerModel;
        weaponsNearby = new List<Weapon>();
    }
    #endregion

    #region Start
    public void KonoStart()
    {
        currentWeaponData = MasterManager.LocalDatabase.GetWeapon(myWeaponSkinData.weaponType);
        if (myPlayerMovement.gC.gameMode == GameMode.CaptureTheFlag) myWeaponSkinRecolor = myWeaponSkinData.skinRecolors[(int)myPlayerMovement.team];
        PickupWeapon(currentWeaponData);
        //SetTeamWeapon(myPlayerMovement.team);
    }
    #endregion

    #region Update
    public void KonoUpdate()
    {
        if (nearestWeapon != null && myPlayerMovement.actions.X.WasPressed)
        {
            PickupWeapon(nearestWeapon);
        }
        //UpdateNearestWeapon();
    }
    #endregion

    #endregion

    #region ----[ PRIVATE FUNCTIONS ]----
    void UpdateNearestWeapon()
    {
        Weapon oldWeapon = nearestWeapon;
        if (weaponsNearby.Count > 0)
        {
            float shortestDistance = float.MaxValue;
            Weapon auxWeap = null;
            for (int i = 0; i < weaponsNearby.Count; i++)
            {
                if (weaponsNearby[i].weaponData != currentWeapon)
                {
                    float dist = Vector3.Distance(weaponsNearby[i].transform.position, transform.position);
                    if (dist < shortestDistance)
                    {
                        shortestDistance = dist;
                        auxWeap = weaponsNearby[i];
                    }
                }
            }
            nearestWeapon = auxWeap;
        }
        else
        {
            nearestWeapon = null;
        }

        if (oldWeapon != nearestWeapon)
        {
            if (nearestWeapon != null)
            {
                myPlayerHUD.SetPickupWeaponTextMessage(nearestWeapon.weaponData);
            }
            else
            {
                myPlayerHUD.DisablePickupWeaponTextMessage();
            }
        }
        // string list="";
        //for(int i = 0; i < weaponsNearby.Count; i++)
        //{
        //    list += weaponsNearby[i].weaponData.name + ", ";
        //}
        //print("Weapons Nearby: " + list);
    }
    #endregion

    #region ----[ PUBLIC FUNCTIONS ]----
    public void PickupWeapon(Weapon weapon)
    {
        PickupWeapon(weapon.weaponData);
    }

    public void PickupWeapon(WeaponData weaponData)
    {
        if(debugModeOn) Debug.Log("PickupWeapon Start");
        //if (!hasWeapon)
        //{
        //    //print("SET PLAYER LAYER TO GO THROUGH SPAWN WALLS");
        //    LayerMask newLM = LayerMask.GetMask("Stage", "WaterFloor");
        //    myPlayerMovement.controller.collisionMask = newLM;
        //}
        DropWeapon();
        myPlayerMovement.maxMoveSpeed = weaponData.playerMaxSpeed;
        myPlayerMovement.bodyMass = weaponData.playerWeight;
        AttatchWeapon(weaponData);
        myPlayerAnim.weaponType = weaponData.weaponType;
        //myPlayerCombat.FillMyAttacks(currentWeapon.weaponData);
        /*if(!myPlayerMovement.online || (myPlayerMovement.online && base.photonView.IsMine))*/
        myPlayerCombatNew.InitializeCombatSystem(weaponData);
    }

    public void DropWeapon()
    {
        if (hasWeapon)
        {
            myPlayerCombatNew.DropWeapon();
            Destroy(currentWeapObject.gameObject);
            currentWeaponData = null;
            currentWeapObject = null;
            currentWeapon = null;
        }
    }

    public void AttatchWeapon(WeaponData weaponData)
    {
        currentWeaponData = weaponData;
        currentWeapObject = Instantiate(weaponData.weaponPrefab).transform;
        currentWeapon = currentWeapObject.GetComponent<Weapon>();
        if (currentWeapon == null)
        {
            Debug.LogError("This weapons has no Weapon script!");
        }
        else
        {
            currentWeapon.weaponData = currentWeaponData;
            AttatchWeaponToHand();
            ChangeWeaponSkin(myWeaponSkinData.skinName, myWeaponSkinRecolor.skinRecolorName);
        }
    }

    public void AttatchWeaponToHand()
    {
        currentWeapObject.SetParent(myPlayerMovement.myPlayerModel.rightHand);
        switch (currentWeaponData.weaponType)
        {
            case WeaponType.QTip:
            case WeaponType.Hammer:
                //GameObject weapon = Instantiate(myWeaponSkin.skinRecolors[(int)team].skinRecolorPrefab, myPlayerModel.rightHand);
                //WeaponOffsets myWeaponOffsets = myWeaponSkin.GetWeaponOffsets(bodyType);
                currentWeapObject.localPosition = myWeaponSkinOffsets.rHandPositionOffset;
                currentWeapObject.localRotation = Quaternion.Euler(myWeaponSkinOffsets.rHandRotationOffset);
                break;
            case WeaponType.Boxing_gloves:
                break;
        }
    }

    public void AttachWeaponToBack()
    {
        currentWeapObject.SetParent(myPlayerMovement.myPlayerModel.senaka);
        switch (currentWeaponData.weaponType)
        {
            case WeaponType.QTip:
            case WeaponType.Hammer:

                currentWeapObject.localPosition = myWeaponSkinOffsets.backPositionOffset;
                currentWeapObject.localRotation = Quaternion.Euler(myWeaponSkinOffsets.backRotationOffset);
                break;
            case WeaponType.Boxing_gloves:
                break;
        }
    }

    //public void SetTeamWeapon(Team team)
    //{
    //    if(debugModeOn) Debug.Log("SetTeamWeapon Start");
    //    switch (team)
    //    {
    //        case Team.A:
    //            PickupWeapon(startingWeaponTeamA);
    //            break;
    //        case Team.B:
    //            PickupWeapon(startingWeaponTeamB);
    //            break;
    //    }
    //}

    public void AddWeaponNearby(Weapon weapPickup)
    {
        if (!weaponsNearby.Contains(weapPickup))
        {
            weaponsNearby.Add(weapPickup);
        }
        //UpdateNearestWeapon();
    }

    public void RemoveWeaponNearby(Weapon weapPickup)
    {
        if (weaponsNearby.Contains(weapPickup))
        {
            weaponsNearby.Remove(weapPickup);
        }
        //UpdateNearestWeapon();
    }

    public void RemoveWeaponNearby(WeaponData weapData)
    {
        bool found = false;
        for (int i = 0; i < weaponsNearby.Count && !found; i++)
        {
            if (weaponsNearby[i].weaponData == weapData)
            {
                weaponsNearby.RemoveAt(i);
                found = true;
            }
        }
        //UpdateNearestWeapon();
    }

    /// <summary>
    /// Looks for a weapon skin that contains the string skinName and a recolor for that skin that contains the string recolorName
    /// </summary>
    /// <param name="skinName"></param>
    /// <param name="recolorName"></param>
    public void ChangeWeaponSkin(string skinName = "", string recolorName = "")
    {
        currentWeapon.SetSkin(out currentWeaponSkin, skinName, recolorName);
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
