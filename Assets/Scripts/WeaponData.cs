using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [Tooltip("Weapon prefab for attaching to the hand.")]
    public GameObject weaponPrefab;

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
