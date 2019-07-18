using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public enum WeaponType
{
    Q_Tip,
    Hammer,
    Boxing_gloves,
    Sword,
    Donut
}

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapon")]
[ExecuteInEditMode]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;
    [Tooltip("As you want it to show as text on the HUD for picking it up.")]
    public string weaponName;
    [Tooltip("Weapon prefab for attaching to the hand.")]
    public GameObject[] weaponSkins;

    [Tooltip("Player maximum speed when carrying this weapon. Normal value is 10.")]
    public float playerMaxSpeed = 10;
    [Tooltip("Normal weight is 1.")]
    [Range(0,3)]
    public float playerWeight = 1;
    public Autocombo autocombo;
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

    public void ErrorCheck()
    {
        bool found = false;
        for(int i=0; i< parry.activePhase.attackHitboxes.Length; i++)
        {
            for(int j=0; j< parry.activePhase.attackHitboxes[i].effects.Length; j++)
            {
                if (parry.activePhase.attackHitboxes[i].effects[j].effectType == EffectType.parry) found = true;
            }
        }
        if(!found) Debug.LogError("WeaponData -> Error: There is no parry for the weapon "+ weaponType + "!");
        if (weaponSkins.Length == 0) Debug.LogError("WeaponData -> Error: There is no weapon skins for the weapon "+ weaponType + "!");
        if(playerWeight<=0) Debug.LogError("WeaponData -> Error: Weight cannot be less or equal than 0 for the weapon " + weaponType + "!");
        autocombo.ErrorCheck();
        parry.ErrorCheck();
    }
    
}
