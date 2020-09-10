using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/LocalDatabase")]
public class LocalDatabase : ScriptableObject
{
    public WeaponData[] allWeapons;

    public WeaponData GetWeapon(WeaponType weaponType)
    {
        WeaponData result = null;
        for (int i = 0; i < allWeapons.Length; i++)
        {
            if (allWeapons[i].weaponType == weaponType) result = allWeapons[i];
        }
        return result;
    }
}
