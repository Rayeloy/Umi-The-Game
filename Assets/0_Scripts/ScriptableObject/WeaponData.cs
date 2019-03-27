using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapon")]
public class WeaponData : ScriptableObject
{
    public WeaponType weaponType;
    [Tooltip("As you want it to show as text on the HUD for picking it up.")]
    public string weaponName;
    [Tooltip("Weapon prefab for attaching to the hand.")]
    public GameObject[] weaponSkins;

    [Tooltip("Player maximum speed when carrying this weapon. Normal value is 10.")]
    public float playerMaxSpeed = 10;
    public AttackData basicAttack;
    public AttackData strongAttack;
    public AttackData specialAttack;
    //public SpecialAttack

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
}

public enum WeaponType
{
    Q_Tip,
    Hammer,
    Sword,
    Boxing_gloves,
    Donut
}
