using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New weapon", menuName = "Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [Tooltip("Local position for attaching to the hand.")]
    public Vector3 localPosition;
    [Tooltip("Local rotation for attaching to the hand.")]
    public Vector3 localRotation;
    [Tooltip("Local scale for attaching to the hand.")]
    public Vector3 localScale;
    [Tooltip("Weapon prefab for attaching to the hand.")]
    public GameObject weaponPrefab;
}
