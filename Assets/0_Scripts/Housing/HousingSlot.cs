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

    public bool hasAnyWallFurniture
    {
        get
        {
            bool result = false;
            for (int i = 0; i < myWalls.Length; i++)
            {
                if (myWalls[i].myWallFurniture != null) result = true;
            }
            return result;
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
        myWalls[1] = new HousingSlotWall(Direction.Right, rightWall);
        myWalls[2] = new HousingSlotWall(Direction.Up, upWall);
        myWalls[3] = new HousingSlotWall(Direction.Down, downWall);

        myFurnitureObject = null;
    }

    public HousingSlotWall GetWall(Direction _orientation)
    {
        for (int i = 0; i < myWalls.Length; i++)
        {
            if (myWalls[i].orientation == _orientation)
            {
                return myWalls[i];
            }
        }
        return null;
    }

    bool SetWallFurniture(HousingFurnitureData _furnitureMeta, GameObject _gO)
    {
        bool result = false;
        if (!thickness)
        {
            if (_furnitureMeta.thickness)
            {
                for (int i = 0; i < myWalls.Length; i++)
                {
                    if (myWalls[i].orientation != _furnitureMeta.orientation)
                    {
                        if (myWalls[i].myWallFurniture != null)
                        {
                            Debug.LogError("Can't place Wall Furniture here because this furniture has thickness and there is already at least 1 wall furniture in this slot.");
                            return false;
                        }
                    }
                }
                    //TO DO: set to false other walls if has thickness & set this wall 
                    for (int i = 0; i < myWalls.Length; i++)
                    {
                        if (myWalls[i].orientation != _furnitureMeta.orientation)
                        {
                            myWalls[i].valid = false;
                        }
                        else
                        {
                            myWalls[i].myWallFurniture = _furnitureMeta;
                            myWalls[i].gO = myFurnitureObject;
                        }
                    }
                result = true;
            }
            else
            {

            }
        }
        else
        {
            Debug.LogError("Can't place furniture here because this slot has already thickness");
            return false;
        }

        return result;
    }

    public bool SetFurniture(HousingFurnitureData _furnitureMeta, GameObject _gO)
    {
        bool result = false;
        if (_furnitureMeta.furnitureType == FurnitureType.Wall)
        {
            if (slotType == HousingSlotType.Wall || slotType == HousingSlotType.WallAndFloor)
            {
                result = SetWallFurniture(_furnitureMeta, _gO);
            }
            else
            {
                Debug.LogError("Can't place a wall furniture here because the slot is not of wall type.");
                return false;
            }
        }
        else
        {
            //TO DO: 
            Debug.LogError("TO DO");
        }
        return result;
    }
}

public struct HousingGridCoordinates
{
    public int y;
    public int z;
    public int x;

    public HousingGridCoordinates(int _y, int _x, int _z)
    {
        y = _y;
        x = _x;
        z = _z;
    }

    public string printString
    {
        get
        {
            return "("+ y + ","+ z + ","+ x + ")";
        }
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
            return myWallFurniture != null ? myWallFurniture.thickness : false;
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
