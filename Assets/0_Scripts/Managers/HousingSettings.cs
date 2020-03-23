using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Manager/HousingSettings")]
public class HousingSettings : ScriptableObject
{
    public GameObject gridPrefab;
    public GameObject slotPrefab;
    public float slotSize = 1.3f;
    public Material[] highlightedSlotMats;//0 == good; 1 == bad; 2 == pointer; 3 == placed
    public HousingFurnitureData[] allFurnitureList;
}

