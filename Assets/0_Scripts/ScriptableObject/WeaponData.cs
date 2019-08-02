using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public enum WeaponType
{
    QTip,
    Hammer,
    Boxing_gloves,
    Sword,
    Donut
}

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapon")]
[ExecuteInEditMode]
public class WeaponData : ScriptableObject
{
    [Tooltip("As you want it to show as text on the HUD for picking it up.")]
    public string weaponName;
    public WeaponType weaponType;

    [Tooltip("Prefab with the Weapon script in it. It will be the base of our weapon.")]
    public GameObject weaponPrefab;

    [Tooltip("WeaponSkin metas.")]
    public WeaponSkinData[] weaponSkins;

    [Tooltip("List of all weaponSkill metas of the weapon.")]
    public WeaponSkillData[] allWeaponSkills;


    [Tooltip("Player maximum speed when carrying this weapon. Normal value is 10.")]
    public float playerMaxSpeed = 10;
    [Tooltip("Normal weight is 1.")]
    [Range(0, 3)]
    public float playerWeight = 1;
    public AutocomboData autocombo;
    public AttackData parry;
    //Skill 1
    //Skill 2




    [Header("Weapon in the right hand")]
    [Tooltip("Local position for attaching to the hand.")]
    public Vector3 handPosition;
    [Tooltip("Local rotation for attaching to the hand.")]
    public Vector3 handRotation;
    [Tooltip("Local scale for attaching to the hand.")]
    public Vector3 handScale;

    [Header("Weapon on the back")]
    [Tooltip("Local position for attaching to the hand.")]
    public Vector3 backPosition;
    [Tooltip("Local rotation for attaching to the hand.")]
    public Vector3 backRotation;
    [Tooltip("Local scale for attaching to the hand.")]
    public Vector3 backScale;

    public bool GetSkin(out WeaponSkinData skin, out WeaponSkinRecolor skinRecolor, string skinName = "", string recolorName = "")
    {
        WeaponSkinData weapSkinData = null;
        skinRecolor = new WeaponSkinRecolor();
        skin = null;
        bool nameFound = false;
        if (skinName != "")
        {
            for (int i = 0; i < weaponSkins.Length && !nameFound; i++)
            {
                if (weaponSkins[i].skinName.Contains(skinName))
                {
                    weapSkinData = weaponSkins[i];
                    nameFound = true;
                }
            }
            if (!nameFound)
            {
                Debug.LogError("The weapon skin name " + skinName + " could not be found.");
            }
        }
        else
        {
            weapSkinData = weaponSkins[0];
        }
        if (weapSkinData != null)
        {
            if (recolorName != "")
            {
                nameFound = false;
                for (int i = 0; i < weapSkinData.skinRecolors.Length && !nameFound; i++)
                {
                    if (weapSkinData.skinRecolors[i].skinRecolorName.Contains(recolorName))
                    {
                        skinRecolor = weapSkinData.skinRecolors[i];
                        nameFound = true;
                    }
                }
                if (!nameFound)
                {
                    Debug.LogError("The weapon skin " + weapSkinData.skinName + "'s skin recolor name " + recolorName + " could not be found.");
                }
            }
            else
            {
                skinRecolor = weapSkinData.skinRecolors[0];
            }
        }
        else nameFound = false;

        return nameFound;
    }

    public void ErrorCheck()
    {
        Weapon auxWeap = weaponPrefab.GetComponent<Weapon>();
        if (auxWeap == null) Debug.LogError("Weapon Data -> Error: the weapon prefab has no Weapon script.");
        bool found = false;
        for (int i = 0; i < parry.activePhase.attackHitboxes.Length; i++)
        {
            for (int j = 0; j < parry.activePhase.attackHitboxes[i].effects.Length; j++)
            {
                if (parry.activePhase.attackHitboxes[i].effects[j].effectType == EffectType.parry) found = true;
            }
        }
        if (!found) Debug.LogError("WeaponData -> Error: There is no parry for the weapon " + weaponType + "!");
        if (weaponSkins.Length == 0) Debug.LogError("WeaponData -> Error: There is no weapon skins for the weapon " + weaponType + "!");
        if (playerWeight <= 0) Debug.LogError("WeaponData -> Error: Weight cannot be less or equal than 0 for the weapon " + weaponType + "!");
        autocombo.ErrorCheck();
        parry.ErrorCheck();
        for (int i = 0; i < weaponSkins.Length; i++)
        {
            weaponSkins[i].ErrorCheck();
        }

        //Skills error check
        for (int i = 0; allWeaponSkills != null && i < allWeaponSkills.Length; i++)
        {
            allWeaponSkills[i].ErrorCheck();
        }
    }
}
    

