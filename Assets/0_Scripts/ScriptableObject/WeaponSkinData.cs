using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New weapon skin", menuName = "Weapon Skin")]
public class WeaponSkinData : ScriptableObject
{
    public string skinName;
    [Tooltip("Not in use right now.")]
    public float skinPrice;
    //public GameObject[] materialSubMeshes;
    public WeaponSkinRecolor[] skinRecolors;

    public void ErrorCheck()
    {
        if (skinName=="") Debug.LogError("Weapon Skin -> Error: the weapon skin " + name + "has no skinName");
        if (skinName == name) Debug.LogError("Weapon Skin -> Error: the weapon skin " + name + " and it's skinName ("+ skinName + ") are different. You may want to change them to be the same.");
        //if (skinPrefab == null) Debug.LogError("Weapon Skin -> Error: the weapon skin " + skinName + " has no skinPrefab");
        //WeaponSkin weapSkin = skinPrefab.GetComponent<WeaponSkin>();
        //if (weapSkin == null) Debug.LogError("Weapon Skin -> Error: the weapon skin "+ skinName + " skinPrefab's WeaponSkin script could not be found.");
        //if (weapSkin.materialSubMeshes.Length <= 0) Debug.LogError("Weapon Skin -> Error: the weapon skin " + skinName + "has no materialSubMeshes");
        if (skinRecolors.Length<=0) Debug.LogError("Weapon Skin -> Error: the weapon skin " + skinName + "has no skinRecolors");
        List<string> auxSkinRecolorNames = new List<string>();
        for(int i=0; i < skinRecolors.Length; i++)
        {
            //if (weapSkin.materialSubMeshes.Length !=skinRecolors[i].materials.Length)
            //{
            //    Debug.LogError("Weapon Skin -> Error: The weapon skin "+ skinName + " materialSubMeshes and it's skinRecolor "+ skinRecolors[i] .skinRecolorName +
            //        " materials have different array lengths.");
            //}
            if (!auxSkinRecolorNames.Contains(skinRecolors[i].skinRecolorName))
            {
                auxSkinRecolorNames.Add(skinRecolors[i].skinRecolorName);
            }
            else
            {
                Debug.LogError("Weapon Skin -> Error: The weapon skin " + skinName + " has 2 or more skin recolors with the name "+ skinRecolors[i].skinRecolorName);
            }

            skinRecolors[i].ErrorCheck();
        }
    }
}

[System.Serializable]
public struct WeaponSkinRecolor
{
    public string skinRecolorName;
    [Tooltip("Not in use right now.")]
    public float skinRecolorPrice;
    public GameObject skinRecolorPrefab;
    public void ErrorCheck()
    {
        //if (materials.Length <= 0) Debug.LogWarning("Weapon Skin Recolor -> Warning: the skin recolor " + skinRecolorName + "has no materials. Is this the base recolor? (no materials, gray)");
    }
}
