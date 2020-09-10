using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Transform weaponSkinsParent;
    //public MeshRenderer[] materialSubMeshes;
    public Transform weaponEdge;
    public Transform weaponHandle;

    [HideInInspector]
    public WeaponData weaponData;
    //WeaponSkin currentWeaponSkin;
    Transform currentSkinTransf;
    WeaponSkinData currentSkin;
    WeaponSkinRecolor currentSkinRecolor;


    public void SetSkin(out WeaponSkin weaponSkin, string skinName = "", string skinRecolorName = "")
    {
        bool exito = false;
        weaponSkin = null;
        if (weaponData.GetSkin(out currentSkin, out currentSkinRecolor, skinName, skinRecolorName))
        {
            for (int i=0; i< weaponSkinsParent.childCount;i++)
            {
                Destroy(weaponSkinsParent.GetChild(i).gameObject);
            }
            currentSkinTransf = Instantiate(currentSkinRecolor.skinRecolorPrefab,transform).transform;
            weaponSkin = currentSkinTransf.GetComponent<WeaponSkin>();
            exito = true;
        }

        if (!exito) Debug.LogError("Error: WeaponData: Weapon with name " + skinName + " not found");
    }
}
