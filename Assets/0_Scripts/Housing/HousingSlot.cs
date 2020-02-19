using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HousingSlotType
{
    Wall,
    WallAndFloor,
    Floor,
    None
}

public class HousingSlot : MonoBehaviour
{
    GameObject slotObject;
    HousingSlotType slotType = HousingSlotType.None;
    HousingGridCoordinates gridCoordinates;
    Vector3 position;
    float size;

    [HideInInspector]
    public HousingFurnitureData myFurniture = null;
    [HideInInspector]
    public GameObject myFurnitureObject = null;
    [HideInInspector]
    public HousingSlotWall[] myWalls = new HousingSlotWall[4];

    public bool thickness
    {
        get
        {
            bool auxThickness = false;
            for (int i = 0; i < myWalls.Length && !auxThickness; i++)
            {
                auxThickness = myWalls[i].thickness;
            }
            return auxThickness;
        }
    }

    public void KonoAwake(HousingGridCoordinates _gridCoordinates, float _size, HousingSlotType _slotType = HousingSlotType.None,
        bool leftWall = false, bool rightWall = false, bool upWall = false, bool downWall = false)
    {
        slotObject = null;
        slotType = _slotType;
        gridCoordinates = _gridCoordinates;
        size = _size;
        myWalls = new HousingSlotWall[4];
        myWalls[0] = new HousingSlotWall(Direction.Left, leftWall);
        myWalls[0] = new HousingSlotWall(Direction.Right, rightWall);
        myWalls[0] = new HousingSlotWall(Direction.Up, upWall);
        myWalls[0] = new HousingSlotWall(Direction.Down, downWall);

        myFurnitureObject = null;
    }
}

public struct HousingGridCoordinates
{
    int y;
    int x;
    int z;

    public HousingGridCoordinates(int _y, int _x, int _z)
    {
        y = _y;
        x = _x;
        z = _z;
    }
}

public class HousingSlotWall
{
    public Direction orientation;
    public bool valid = false;
    public HousingFurnitureData myWallFurniture;
    public GameObject gO;
    public bool thickness
    {
        get
        {
            return myWallFurniture !=null? myWallFurniture.thickness : false;
        }
    }

    public HousingSlotWall(Direction _orienation, bool _valid = false)
    {
        orientation = _orienation;
        valid = _valid;
        myWallFurniture = null;
        gO = null;
    }
}
