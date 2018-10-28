using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapons : MonoBehaviour {

    public PlayerAnimation myPlayerAnim;
    [Header("WEAPONS ATTACH")]
    public Transform senaka;//ESPALDA
    public Transform rightHand;
    public Transform leftHand;

    WeaponData currentWeapon;
    Transform currentWeapObject;


    public WeaponData SearchWeapon(string name)
    {
        WeaponData[] allWeap = GameController.instance.allWeapons;
        foreach (WeaponData wp in allWeap)
        {
            if (name == wp.weaponName)
            {
                return wp;
            }
        }
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
