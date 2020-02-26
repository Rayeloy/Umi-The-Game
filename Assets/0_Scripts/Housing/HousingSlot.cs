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
    [HideInInspector]
    public bool baseFurniture = false;

    GameObject slotObject;
    HousingSlotType slotType = HousingSlotType.None;
    HousingGridCoordinates gridCoordinates;

    public HousingFurniture myFurniture;
    [HideInInspector]
    public HousingFurniture[] myWallFurnitures = new HousingFurniture[4];


    public bool hasFurniture
    {
        get
        {
            return myFurniture != null;
        }
    }
    public bool thickness
    {
        get
        {
            bool auxThickness = false;
            for (int i = 0; i < myWallFurnitures.Length && !auxThickness; i++)
            {
                if(myWallFurnitures[i]!=null && myWallFurnitures[i].furnitureMeta!=null)
                auxThickness = myWallFurnitures[i].furnitureMeta.thickness;
            }
            return auxThickness;
        }
    }
    public bool hasAnyWallFurniture
    {
        get
        {
            bool result = false;
            for (int i = 0; i < myWallFurnitures.Length; i++)
            {
                if (myWallFurnitures[i] != null && myWallFurnitures[i].furnitureMeta != null) result = true;
            }
            return result;
        }
    }
    public bool free
    {
        get
        {
            return !baseFurniture && !thickness && !hasFurniture;
        }
    }
    bool[] hasWalls;


    public void KonoAwake(HousingGridCoordinates _gridCoordinates, HousingSlotType _slotType = HousingSlotType.None,
        bool upWall = false, bool rightWall = false, bool downWall = false,bool leftWall = false, bool _baseFurniture = false)
    {
        slotObject = null;
        slotType = _slotType;
        gridCoordinates = _gridCoordinates;
        myWallFurnitures = new HousingFurniture[4];
        hasWalls = new bool[4];
        hasWalls[0] = upWall;
        hasWalls[1] = rightWall;
        hasWalls[2] = downWall;
        hasWalls[3] = leftWall;

        myFurniture = null;

        baseFurniture = _baseFurniture;
    }

    public HousingFurniture GetWall(Direction _orientation)
    {
        return myWallFurnitures[(int)_orientation];
    }

    bool SetWallFurniture(HousingFurniture _wallFurniture)
    {
        bool result = false;

        if (thickness)
        {
            Debug.LogError("THIS MESSAGE SHOULD NOT BE APPEARING. Can't place furniture here because the slot has thickness. ");
            return false;
        }

        if(!hasWalls[(int)_wallFurniture.currentOrientation])
        {
            Debug.LogError("Can't place furniture here because the slot has no "+ _wallFurniture.currentOrientation + " wall.");
            return false;
        }

        if (_wallFurniture.furnitureMeta.thickness)
        {
            for (int i = 0; i < myWallFurnitures.Length; i++)
            {
                if(i != (int)_wallFurniture.currentOrientation)
                {
                    //Delete furniture and send back to inventory
                    Debug.LogError("TO DO: send back to inventory");
                    Destroy(myWallFurnitures[i].gameObject);
                    myWallFurnitures[i] = null;
                }
            }
        }

        //SET WALL FURNITURE
        myWallFurnitures[(int)_wallFurniture.currentOrientation] = _wallFurniture;
        result = true;

        return result;
    }

    public bool SetFurniture(HousingFurniture _furniture)
    {
        if (!free)
        {
            Debug.LogError("HousingSlot -> SetFurniture: Can't place a furniture because the slot is not free");
            return false;
        }
        bool result = false;
        if (_furniture.furnitureMeta.furnitureType == FurnitureType.Wall)
        {
            if (slotType == HousingSlotType.Wall || slotType == HousingSlotType.WallAndFloor)
            {
                result = SetWallFurniture(_furniture);
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
            Debug.LogError("TO DO?");
            myFurniture = _furniture;
        }
        return result;
    }
}

[System.Serializable]
public struct HousingGridCoordinates
{
    public int y;
    public int z;
    public int x;

    public HousingGridCoordinates(int _y, int _z, int _x)
    {
        y = _y;
        x = _x;
        z = _z;
    }

    public static HousingGridCoordinates Sum(HousingGridCoordinates coordA, HousingGridCoordinates coordB)
    {
        return new HousingGridCoordinates(coordA.y + coordB.y, coordA.z + coordB.z, coordA.x + coordB.x);
    }

    public string printString
    {
        get
        {
            return "(" + y + "," + z + "," + x + ")";
        }
    }
}

//public class HousingSlotWall
//{
//    public Direction orientation;
//    public bool valid = false;
//    public HousingFurniture myWallFurniture;
//    public bool thickness
//    {
//        get
//        {
//            return myWallFurniture != null ? myWallFurniture.thickness : false;
//        }
//    }

//    public HousingSlotWall(Direction _orienation, bool _valid = false)
//    {
//        orientation = _orienation;
//        valid = _valid;
//        myWallFurniture = null;
//    }
//}
