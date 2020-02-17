using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FurnitureType
{
    None,
    Wall,
    Floor,
    Floor_Small,
    Ceiling
}

public enum FurnitureTag
{

}

[CreateAssetMenu(fileName = "New housing furniture", menuName = "Housing Furniture")]
public class HousingFurnitureData : ScriptableObject
{
    public string furnitureName;
    [HideInInspector]
    public bool thickness = false;
    public FurnitureType furnitureType = FurnitureType.None;
    public GameObject prefab;
    public FurnitureLevel[] furnitureSpace;
}

[System.Serializable]
public class FurnitureLevel
{
    [SerializeField]
    public bool[] row1;
    [SerializeField]
    public bool[] row2;
    [SerializeField]
    public bool[] row3;

    public FurnitureLevel(int columns)
    {
        row1 = new bool[columns];
        row2 = new bool[columns];
        row3 = new bool[columns];
    }
}