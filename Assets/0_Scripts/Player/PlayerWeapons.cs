using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour {

    [Header("Referencias")]
    public PlayerAnimation myPlayerAnim;
    public PlayerMovement myPlayerMovement;
    [Header("WEAPONS ATTACH")]
    public Transform senaka;//ESPALDA
    public Transform rightHand;
    public Transform leftHand;

    WeaponData currentWeapon;
    Transform currentWeapObject;

    public void KonoAwake()
    {
        myPlayerMovement = GetComponent<PlayerMovement>();
        myPlayerAnim = GetComponent<PlayerAnimation>();
    }


    public WeaponData SearchWeapon(string name)
    {
        List<WeaponData> allWeap = myPlayerMovement.gC.allWeapons;
        for (int i=0; i< allWeap.Count;i++)
        {
            if (name == allWeap[i].weaponName)
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

    public void AttachWeapon(string weaponName)
    {
        currentWeapon = SearchWeapon(weaponName);
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

}
