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
    //public PlayerCombat myPlayerCombat;
    public PlayerCombatNew myPlayerCombatNew;
    PlayerHUD myPlayerHUD;
    [HideInInspector]
    public PlayerModel myPlayerModel;

    //[Header("WEAPONS ATTACH")]
    //public Transform senaka;//ESPALDA
    //public Transform rightHand;
    //public Transform leftHand;
    #endregion

    #region ----[ PROPERTIES ]----
    [HideInInspector]
    public Weapon currentWeapon;
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

    #region ----[ VARIABLES ]----
    #endregion

    #region ----[ MONOBEHAVIOUR FUNCTIONS ]----

    #region Awake
    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovement>();
        myPlayerAnim = myPlayerMovement.myPlayerAnimation_01;
        myPlayerCombatNew = myPlayerMovement.myPlayerCombatNew;
        myPlayerHUD = myPlayerMovement.myPlayerHUD;
        myPlayerModel = myPlayerMovement.myPlayerModel;
        weaponsNearby = new List<Weapon>();
    }
    #endregion

    #region Start
    #endregion

    #region Update
    public void KonoUpdate()
    {

        if (nearestWeapon!=null && myPlayerMovement.Actions.X.WasPressed)
        {
            PickupWeapon(nearestWeapon);
        }

        UpdateNearestWeapon();
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
        if (!hasWeapon)
        {
            //print("SET PLAYER LAYER TO GO THROUGH SPAWN WALLS");
            LayerMask newLM = LayerMask.GetMask("Stage", "WaterFloor");
            myPlayerMovement.controller.collisionMask = newLM;
        }
        DropWeapon();
        myPlayerMovement.maxMoveSpeed = weapon.weaponData.playerMaxSpeed;
        myPlayerMovement.bodyMass = weapon.weaponData.playerWeight;
        AttatchWeapon(weapon);
        //myPlayerCombat.FillMyAttacks(currentWeapon.weaponData);
        myPlayerCombatNew.InitializeCombatSystem(weapon);
    }

    public void DropWeapon()
    {
        if (hasWeapon)
        {
            myPlayerCombatNew.DropWeapon();
            Destroy(currentWeapObject.gameObject);
            currentWeapObject = null;
            currentWeapon = null;
        }
    }

    /*public WeaponData SearchWeapon(WeaponType wepType)
    {
        List<WeaponData> allWeap = myPlayerMovement.gC.allWeapons;
        for (int i = 0; i < allWeap.Count; i++)
        {
            if (wepType == allWeap[i].weaponType)
            {
                return allWeap[i];
            }
        }
        Debug.LogError("Error: Could not find the weapon with the name " + name);
        return null;
    }*/

    public void AttatchWeapon()
    {
        currentWeapObject.SetParent(myPlayerModel.rightHand);
        currentWeapObject.localPosition = currentWeapon.weaponData.handPosition;
        currentWeapObject.localRotation = Quaternion.Euler(currentWeapon.weaponData.handRotation.x, currentWeapon.weaponData.handRotation.y, currentWeapon.weaponData.handRotation.z);
        currentWeapObject.localScale = currentWeapon.weaponData.handScale;
    }

    public void AttatchWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        currentWeapObject = Instantiate(currentWeapon.currentWeaponPrefab, myPlayerModel.rightHand).transform;
        AttatchWeapon();
    }

    public void AttachWeaponToBack()
    {
        //if (!PhotonNetwork.IsConnected)
        //{
            currentWeapObject.SetParent(myPlayerModel.senaka);
            currentWeapObject.localPosition = currentWeapon.weaponData.backPosition;
            currentWeapObject.localRotation = Quaternion.Euler(currentWeapon.weaponData.backRotation.x, currentWeapon.weaponData.backRotation.y, currentWeapon.weaponData.backRotation.z);
            currentWeapObject.localScale = currentWeapon.weaponData.backScale;
        //}
    }

    public void AddWeaponNearby(Weapon weapPickup)
    {
        if (!weaponsNearby.Contains(weapPickup))
        {
            weaponsNearby.Add(weapPickup);
        }
        UpdateNearestWeapon();
    }

    public void RemoveWeaponNearby(Weapon weapPickup)
    {
        if (weaponsNearby.Contains(weapPickup))
        {
            weaponsNearby.Remove(weapPickup);
        }
        UpdateNearestWeapon();
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
        UpdateNearestWeapon();
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
