using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region ----[ PUBLIC ENUMS ]----
#endregion
public class PlayerWeapons : MonoBehaviour {

    #region ----[ VARIABLES FOR DESIGNERS ]----
    //Referencias
    [Header("Referencias")]
    public PlayerAnimation myPlayerAnim;
    public PlayerMovement myPlayerMovement;
    public PlayerCombat myPlayerCombat;
    PlayerHUD myPlayerHUD;

    [Header("WEAPONS ATTACH")]
    public Transform senaka;//ESPALDA
    public Transform rightHand;
    public Transform leftHand;
    #endregion

    #region ----[ PROPERTIES ]----
    [HideInInspector]
    public WeaponData currentWeapon;
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
        myPlayerAnim = myPlayerMovement.myPlayerAnimation;
        myPlayerCombat = myPlayerMovement.myPlayerCombat;
        myPlayerHUD = myPlayerMovement.myPlayerHUD;
        weaponsNearby = new List<Weapon>();
    }
    #endregion

    #region Start
    #endregion

    #region Update
    public void KonoUpdate()
    {

        if (nearestWeapon!=null && myPlayerMovement.Actions.Attack1.WasPressed)
        {
            PickupWeapon(nearestWeapon.weaponData);
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
    //no se usa esta ahora mismo
    public void PickupWeapon(WeaponType weapon)
    {
        AttachWeapon(weapon);
        myPlayerCombat.FillMyAttacks(currentWeapon);
    }

    public void PickupWeapon(WeaponData weaponData)
    {
        if (!hasWeapon)
        {
            LayerMask newLM = LayerMask.GetMask("Stage", "WaterFloor");
            myPlayerMovement.controller.collisionMask = newLM;
        }
        DropWeapon();
        myPlayerMovement.maxMoveSpeed = weaponData.playerMaxSpeed;
        AttachWeapon(weaponData);
        myPlayerCombat.FillMyAttacks(currentWeapon);
    }

    public void DropWeapon()
    {
        if (hasWeapon)
        {
            myPlayerCombat.EmptyMyAttacks();
            Destroy(currentWeapObject.gameObject);
            currentWeapObject = null;
            currentWeapon = null;
        }
    }

    public WeaponData SearchWeapon(WeaponType wepType)
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
    }

    public void AttachWeapon()
    {
        currentWeapObject.SetParent(rightHand);
        currentWeapObject.localPosition = currentWeapon.handPosition;
        currentWeapObject.localRotation = Quaternion.Euler(currentWeapon.handRotation.x, currentWeapon.handRotation.y, currentWeapon.handRotation.z);
        currentWeapObject.localScale = currentWeapon.handScale;
    }
    //no se usa ahora mismo
    public void AttachWeapon(WeaponType wepType)
    {
        currentWeapon = SearchWeapon(wepType);
        currentWeapObject = Instantiate(currentWeapon.weaponPrefab, rightHand).transform;
        AttachWeapon();
    }

    public void AttachWeapon(WeaponData weaponData)
    {
        currentWeapon = weaponData;
        currentWeapObject = Instantiate(currentWeapon.weaponPrefab, rightHand).transform;
        AttachWeapon();
    }

    public void AttachWeaponToBack()
    {
        currentWeapObject.SetParent(senaka);
        currentWeapObject.localPosition = currentWeapon.backPosition;
        currentWeapObject.localRotation = Quaternion.Euler(currentWeapon.backRotation.x, currentWeapon.backRotation.y, currentWeapon.backRotation.z);
        currentWeapObject.localScale = currentWeapon.backScale;
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
