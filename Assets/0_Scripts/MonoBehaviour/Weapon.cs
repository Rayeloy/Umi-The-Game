using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    public GameObject weaponPrefab;
    Respawn myRespawn;
    [HideInInspector]
    public GameObject currentWeaponPrefab;
    private void Awake()
    {
        myRespawn = transform.parent.parent.GetComponentInChildren<Respawn>();
        //Debug.Log("Respawn "+myRespawn.gameObject.name+" is team "+ myRespawn.team);
        switch (weaponData.name)
        {
            case "Q_Tip":
                //Debug.Log("Q TIP PICKUP");
                switch (myRespawn.team)
                {
                    case Team.A:
                        SetSkin("Blue");
                        break;
                    case Team.B:
                        SetSkin("Red");
                        break;
                }
                break;
            default:
                //Debug.Log("DEFAULT PICKUP");
                SetSkin(0);
                break;

        }
        Vector3 pos = weaponPrefab.transform.position;
        Quaternion rot = weaponPrefab.transform.rotation;
        Vector3 scale = weaponPrefab.transform.localScale;
        weaponPrefab.SetActive(false);
        weaponPrefab = Instantiate(currentWeaponPrefab, pos, rot, transform);
        weaponPrefab.transform.localScale = scale;
    }

    public void SetSkin(int index)
    {
        currentWeaponPrefab = weaponData.weaponSkins[index];
        if (currentWeaponPrefab == null) Debug.LogError("Error: WeaponData: Weapon with index " + index + " not found");
    }
    public void SetSkin(string skinName)
    {
        bool exito = false;
        for (int i = 0; i < weaponData.weaponSkins.Length; i++)
        {
            if (weaponData.weaponSkins[i].name.Contains(skinName))
            {
                currentWeaponPrefab = weaponData.weaponSkins[i];
                exito = true;
                //Debug.Log(name + " current skin set to " + weaponData.weaponSkins[i].name);
            }
        }
        if (!exito) Debug.LogError("Error: WeaponData: Weapon with name " + skinName + " not found");
    }

}
