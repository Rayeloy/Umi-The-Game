using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HousingSlotType
{
    Wall,
    WallAndFloor,
    Floot,
    None
}

public class HousingSlot : MonoBehaviour
{
    HousingSlotType slotType = HousingSlotType.None;
    HousingGridCoordinates gridCoordinates;
    Vector3 position;
    float size;
    HousingFurnitureData myFurniture = null;
    HousingFurnitureData[] myWallFurnitures = new HousingFurnitureData[4];
    public HousingSlot(HousingGridCoordinates _gridCoordinates, float _size, HousingSlotType _slotType = HousingSlotType.None)
    {
        gridCoordinates = _gridCoordinates;
        size = _size;
        slotType = _slotType;
        HousingFurnitureData[] myWallFurnitures = new HousingFurnitureData[4];
    }
}

public struct HousingGridCoordinates
{
    int x;
    int z;
    int y;

    public HousingGridCoordinates(int _x, int _z, int _y)
    {
        x = _x;
        y = _y;
        z = _z;
    }
}
