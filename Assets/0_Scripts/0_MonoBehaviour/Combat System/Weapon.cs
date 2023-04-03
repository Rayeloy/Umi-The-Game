using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform weaponSkinsParent;
    //public MeshRenderer[] materialSubMeshes;
    public Transform weaponEdge;
    public Transform weaponHandle;
    public Transform leftHandPos;
    public WeaponPart[] secondaryParts;

    [HideInInspector]
    public WeaponData weaponData;
    //WeaponSkin currentWeaponSkin;
    Transform currentSkinTransf;
    WeaponSkinData currentWeaponSkinData;
    WeaponSkinRecolor currentWeaponSkinRecolor;


    public void SetSkin(out WeaponSkin weaponSkin, string skinName = "", string skinRecolorName = "")
    {
        bool exito = false;
        weaponSkin = null;
        WeaponSkinData newWeaponSkinData = null;
        WeaponSkinRecolor newWeaponSkinRecolor = new WeaponSkinRecolor();
        if (weaponData.GetSkin(out newWeaponSkinData, out newWeaponSkinRecolor, skinName, skinRecolorName))
        {
            Debug.Log("Current Weapon SKin = " + newWeaponSkinData.name);
            currentWeaponSkinData = newWeaponSkinData;
            currentWeaponSkinRecolor = newWeaponSkinRecolor;
            for (int i=0; i< weaponSkinsParent.childCount;i++)
            {
                Destroy(weaponSkinsParent.GetChild(i).gameObject);
            }
            currentSkinTransf = Instantiate(currentWeaponSkinRecolor.skinRecolorPrefab,transform).transform;
            weaponSkin = currentSkinTransf.GetComponent<WeaponSkin>();
            secondaryParts = weaponSkin.secondaryParts;
            exito = true;
        }

        if (!exito) Debug.LogError("Error: WeaponData: Weapon with name " + skinName + " not found");
    }

    public WeaponSkinData GetCurrentWeaponSkinData()
    {
        return currentWeaponSkinData;
    }

    public WeaponSkinRecolor GetCurrentWeaponSkinRecolor()
    {
        return currentWeaponSkinRecolor;
    }
}
