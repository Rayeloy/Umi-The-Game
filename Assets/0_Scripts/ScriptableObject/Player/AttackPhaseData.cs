using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackPhaseData : ScriptableObject
{
    [HideInInspector]
    public bool isFoldedInEditor;

    public float duration;
    public bool restrictRotation;
    public float rotationSpeed;
    [Tooltip("Do you want this phase to have a hitbox?")]
    public bool hasHitbox;
    [Tooltip("Leave empty if no hitbox needed")]
    public GameObject hitboxPrefab;

    private void OnEnable()
    {
        Debug.Log("AttackPhaseData OnEnable() and I'm " + name);

    }

    //void Init()
    //{
    //    isFoldedInEditor = false;
    //    duration = 0;
    //    restrictRotation = false;
    //    rotationSpeed = 0;
    //    hasHitbox = false;
    //    hitboxPrefab = null;
    //}

}
