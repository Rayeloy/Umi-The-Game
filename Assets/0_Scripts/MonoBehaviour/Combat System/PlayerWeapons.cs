using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region ----[ PUBLIC ENUMS ]----
#endregion
public class PlayerWeapons : MonoBehaviour {

    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    [Header("Referencias")]
    public PlayerAnimation_01 myPlayerAnim;
    public PlayerMovement myPlayerMovement;
    public PlayerCombatNew myPlayerCombatNew;
    PlayerHUD myPlayerHUD;
    [HideInInspector]
    public PlayerModel myPlayerModel;
    public WeaponData startingWeaponTeamA;
    public WeaponData startingWeaponTeamB;

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
        myPlayerMovement = GetComponent<PlayerMovement>();
        myPlayerAnim = myPlayerMovement.myPlayerAnimation_01;
        myPlayerCombatNew = myPlayerMovement.myPlayerCombatNew;
        myPlayerHUD = myPlayerMovement.myPlayerHUD;
        myPlayerModel = myPlayerMovement.myPlayerModel;
        weaponsNearby = new List<Weapon>();
    }
    #endregion

    #region Start
    public void KonoStart()
    {
        switch (myPlayerMovement.team)
        {
            case Team.A:
                PickupWeapon(startingWeaponTeamA);
                break;
            case Team.B:
                PickupWeapon(startingWeaponTeamB);
                break;
        }
    }
    #endregion

    #region Update
    public void KonoUpdate()
    {
        if (nearestWeapon!=null && myPlayerMovement.Actions.X.WasPressed)
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
        //myPlayerCombat.FillMyAttacks(currentWeapon.weaponData);
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

    public void AttatchWeapon()
    {
        currentWeapObject.SetParent(myPlayerModel.rightHand);
        currentWeapObject.localPosition = currentWeaponData.handPosition;
        currentWeapObject.localRotation = Quaternion.Euler(currentWeaponData.handRotation.x, currentWeaponData.handRotation.y, currentWeaponData.handRotation.z);
        currentWeapObject.localScale = currentWeaponData.handScale;
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
            AttatchWeapon();
            switch (myPlayerMovement.team)
            {
                case Team.A:
                    ChangeWeaponSkin("Skin2","Green");
                    break;
                case Team.B:
                    ChangeWeaponSkin("Skin2","Pink");
                    break;
            }
        }
    }

    public void AttachWeaponToBack()
    {
        //if (!PhotonNetwork.IsConnected)
        //{
            currentWeapObject.SetParent(myPlayerModel.senaka);
            currentWeapObject.localPosition = currentWeaponData.backPosition;
            currentWeapObject.localRotation = Quaternion.Euler(currentWeaponData.backRotation.x, currentWeaponData.backRotation.y, currentWeaponData.backRotation.z);
            currentWeapObject.localScale = currentWeaponData.backScale;
        //}
    }

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
        for(int i=0; i<weaponsNearby.Count && !found; i++)
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
    public void ChangeWeaponSkin(string skinName="", string recolorName="")
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
