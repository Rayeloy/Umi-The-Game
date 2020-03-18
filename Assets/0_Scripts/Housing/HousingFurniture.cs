using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingFurniture : MonoBehaviour
{
    [HideInInspector]
    public HousingGrid grid;
    public HousingFurnitureData furnitureMeta;
    public Direction currentOrientation;
    public FurnitureLevel[] currentSpaces;
    public HousingGridCoordinates anchor;
    public HousingGridCoordinates currentAnchorGridPos;

    public bool validCurrentSpaces
    {
        get
        {
            return currentSpaces != null && currentSpaces.Length > 0 && currentSpaces[0].spaces != null && currentSpaces[0].spaces.Length > 0 &&
                currentSpaces[0].spaces[0].row != null && currentSpaces[0].spaces[0].row.Length > 0;
        }
    }

    public int width
    {
        get
        {
            int result = -1;
            if (validCurrentSpaces) result = currentSpaces[0].spaces[0].row.Length;
            return result;
        }
    }
    public int depth
    {
        get
        {
            int result = -1;
            if (validCurrentSpaces) result = currentSpaces[0].spaces.Length;
            return result;
        }
    }
    public int height
    {
        get
        {
            int result = -1;
            if (validCurrentSpaces) result = currentSpaces.Length;
            return result;
        }
    }

    bool hasJustTurnedClockwise = false;
    bool hasJustTurnedCounterClockwise = false;

    public bool turnedClockwise
    {
        get
        {
            if (hasJustTurnedClockwise)
            {
                hasJustTurnedClockwise = false;
                return true;
            }
            return false;
        }
    }
    public bool turnedCounterClockwise
    {
        get
        {
            if (hasJustTurnedCounterClockwise)
            {
                hasJustTurnedCounterClockwise = false;
                return true;
            }
            return false;
        }
    }
    public void ResetRotationBools()
    {
        hasJustTurnedClockwise = hasJustTurnedCounterClockwise = false;
    }

    public List<HousingFurniture> smallFurnitureOn;
    public HousingFurniture furnitureBase;

    public void PrintSpaces()
    {
        for (int k = 0; k < height; k++)
        {
            Debug.Log(" - Level " + k + " -");
            for (int i = 0; i < depth; i++)
            {
                string row = "(";
                for (int j = 0; j < width; j++)
                {
                    if (j != 0) row += ",";
                    row += currentSpaces[k].spaces[i].row[j] ? "1" : "0";
                    if (k == anchor.y && i == anchor.z && j == anchor.x) row += "*";
                }
                row += ")";
                Debug.Log(row);
            }
        }
    }

    public void KonoAwake(HousingFurnitureData _furnitureMeta, HousingGrid _grid)
    {
        furnitureMeta = _furnitureMeta;
        grid = _grid;
        currentOrientation = Direction.Up;
        currentSpaces = furnitureMeta.furnitureSpace;
        anchor = _furnitureMeta.anchor;
        switch (_furnitureMeta.orientation)
        {
            default: break;

            case Direction.Right:
                RotateClockwise();
                break;
            case Direction.Down:
                RotateClockwise();
                RotateClockwise();
                break;
            case Direction.Left:
                RotateCounterClockwise();
                break;
        }
        ResetRotationBools();

        smallFurnitureOn = new List<HousingFurniture>();
    }

    //private void Update()
    //{
    //    if (furnitureMeta.furnitureType == FurnitureType.Floor_Small)
    //    {
    //        for (int i = 0; i < depth; i++)//for every furniture row
    //        {
    //            for (int j = 0; j < width; j++)//for every furniture slot
    //            {
    //                bool val;
    //                HousingGridCoordinates currentCheckCoord = GetGridCoord(new HousingGridCoordinates(0, i, j), out val);
    //                HousingSlot currentSlot = grid.GetSlotAt(currentCheckCoord);
    //                if (val && currentSlot != null && currentSlot.HasThisFurniture(this))
    //                {
    //                    //Check if floor fell
    //                    HousingGridCoordinates underCoord = currentCheckCoord; underCoord.y--;
    //                    if (underCoord.y >= 0 && underCoord.y < grid.height)
    //                    {
    //                        HousingSlot underSlot = grid.GetSlotAt(underCoord);
    //                        if (underSlot != null && !underSlot.hasFurniture)
    //                        {
    //                            grid.DropFurniture(this);
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    }
    //}

    public void Copy(FurnitureRotationData _furnData)
    {
        currentOrientation = _furnData.currentOrientation;
        currentSpaces = _furnData.currentSpaces;
        anchor = _furnData.anchor;
        currentAnchorGridPos = _furnData.currentAnchorGridPos;
        smallFurnitureOn = _furnData.smallFurnitureOn;

        for (int i = 0; i < smallFurnitureOn.Count; i++)
        {
            smallFurnitureOn[i].Copy(_furnData.smallFurnitureOnRotData[i]);
        }
    }

    public void ChangePos(HousingGridCoordinates newAnchorPos)
    {
        Debug.Log("HousingFurniture -> ChangePos STARTED. NewAnchorPos = " + newAnchorPos.printString);
        for (int i = 0; i < smallFurnitureOn.Count; i++)
        {
            HousingFurniture auxFurn = smallFurnitureOn[i];
            Debug.Log("HousingFurniture -> auxFurn.currentAnchorGridPos = "+ auxFurn.currentAnchorGridPos.printString+
                "; currentAnchorGridPos = " + currentAnchorGridPos.printString);

            int yDif = auxFurn.currentAnchorGridPos.y - currentAnchorGridPos.y;
            int zDif = auxFurn.currentAnchorGridPos.z - currentAnchorGridPos.z;
            int xDif = auxFurn.currentAnchorGridPos.x - currentAnchorGridPos.x;
            auxFurn.currentAnchorGridPos = new HousingGridCoordinates(newAnchorPos.y + yDif, newAnchorPos.z + zDif, newAnchorPos.x + xDif);
            Debug.Log("HousingFurniture -> ChangePos: smallFurnitureOn["+ i + "].currentAnchorGridPos = " + auxFurn.currentAnchorGridPos.printString);
        }
        currentAnchorGridPos = newAnchorPos;

    }

    public void RotateClockwise(bool saveRotation = false)
    {
        FurnitureLevel[] newSpaces = new FurnitureLevel[furnitureMeta.height];
        currentOrientation++;
        currentOrientation += (int)currentOrientation > 3 ? -4 : 0;

        for (int k = 0; k < currentSpaces.Length; k++)
        {
            newSpaces[k] = new FurnitureLevel(width, depth);//opposite amount of width and depth
        }

        //Fill new matrix
        for (int k = 0; k < height; k++)
        {
            //Transpose
            FurnitureLevel transposeMatrix = new FurnitureLevel(width, depth);//opposite amount of width and depth
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (GetAtIndex(k, i, j))
                    {
                        transposeMatrix.spaces[j].row[i] = GetAtIndex(k, i, j);
                    }
                }
            }
            //reverse rows
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    newSpaces[k].spaces[i].row[(depth - 1) - j] = transposeMatrix.spaces[i].row[j];
                }
            }
        }

        for (int i = 0; i < smallFurnitureOn.Count; i++)
        {
            HousingFurniture auxFurn = smallFurnitureOn[i];

            //anchors differences
            int yAnchorDif = auxFurn.currentAnchorGridPos.y - currentAnchorGridPos.y;
            int zAnchorDif = auxFurn.currentAnchorGridPos.z - currentAnchorGridPos.z;
            int xAnchorDif = auxFurn.currentAnchorGridPos.x - currentAnchorGridPos.x;
            auxFurn.currentAnchorGridPos = new HousingGridCoordinates(currentAnchorGridPos.y + yAnchorDif, currentAnchorGridPos.z + xAnchorDif,
                currentAnchorGridPos.x + (-zAnchorDif));
            Debug.Log("HousingFurniture -> RotateClockwise: smallFurnitureOn[" + i + "] -> Before:");
            auxFurn.PrintSpaces();
            auxFurn.RotateClockwise(saveRotation);
            auxFurn.ResetRotationBools();
            Debug.Log("HousingFurniture -> RotateClockwise: smallFurnitureOn[" + i + "] -> After:");
            auxFurn.PrintSpaces();
        }

        //update new anchor
        HousingGridCoordinates transposedAnchor = new HousingGridCoordinates(anchor.y, anchor.x, anchor.z);
        anchor = new HousingGridCoordinates(anchor.y, transposedAnchor.z, (depth - 1) - transposedAnchor.x);

        currentSpaces = newSpaces;

        hasJustTurnedClockwise = saveRotation;
    }

    public void RotateCounterClockwise(bool saveRotation = false)
    {
        FurnitureLevel[] newSpaces = new FurnitureLevel[furnitureMeta.height];
        currentOrientation--;
        currentOrientation += (int)currentOrientation < 0 ? +4 : 0;

        for (int k = 0; k < currentSpaces.Length; k++)
        {
            newSpaces[k] = new FurnitureLevel(width, depth);//opposite amount of width and depth
        }

        //Fill new matrix
        for (int k = 0; k < height; k++)
        {
            //Transpose
            FurnitureLevel transposeMatrix = new FurnitureLevel(width, depth);//opposite amount of width and depth
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (GetAtIndex(k, i, j))
                    {
                        transposeMatrix.spaces[j].row[i] = GetAtIndex(k, i, j);
                    }
                }
            }
            //reverse columns
            for (int j = 0; j < depth; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    newSpaces[k].spaces[(width - 1) - i].row[j] = transposeMatrix.spaces[i].row[j];
                }
            }
        }

        for (int i = 0; i < smallFurnitureOn.Count; i++)
        {
            HousingFurniture auxFurn = smallFurnitureOn[i];

            //anchors differences
            int yAnchorDif = auxFurn.currentAnchorGridPos.y - currentAnchorGridPos.y;
            int zAnchorDif = auxFurn.currentAnchorGridPos.z - currentAnchorGridPos.z;
            int xAnchorDif = auxFurn.currentAnchorGridPos.x - currentAnchorGridPos.x;
            auxFurn.currentAnchorGridPos = new HousingGridCoordinates(currentAnchorGridPos.y + yAnchorDif, currentAnchorGridPos.z + (-xAnchorDif),
                currentAnchorGridPos.x + zAnchorDif);
            Debug.Log("HousingFurniture -> RotateCounterClockWise: smallFurnitureOn[" + i + "] -> Before:");
            auxFurn.PrintSpaces();
            auxFurn.RotateCounterClockwise();
            auxFurn.ResetRotationBools();
            Debug.Log("HousingFurniture -> RotateCounterClockWise: smallFurnitureOn[" + i + "] -> After:");
            auxFurn.PrintSpaces();
        }

        //update new anchor
        HousingGridCoordinates transposedAnchor = new HousingGridCoordinates(anchor.y, anchor.x, anchor.z);
        anchor = new HousingGridCoordinates(anchor.y, (width - 1) - transposedAnchor.z, transposedAnchor.x);

        currentSpaces = newSpaces;

        hasJustTurnedCounterClockwise = saveRotation;
    }

    public HousingGridCoordinates GetGridCoord(HousingGridCoordinates localCoord, out bool value)
    {
        int yDif = localCoord.y - anchor.y;
        int zDif = localCoord.z - anchor.z;
        int xDif = localCoord.x - anchor.x;
        HousingGridCoordinates gridCoord = new HousingGridCoordinates(currentAnchorGridPos.y + yDif, currentAnchorGridPos.z + zDif, currentAnchorGridPos.x + xDif);
        value = GetAtIndex(localCoord);
        Debug.Log("HousingFurniture -> GetGridCoord: currentAnchorGridPos = " + currentAnchorGridPos.printString);
        return gridCoord;
    }

    public HousingGridCoordinates GetGridCoord(HousingGridCoordinates localCoord, HousingGridCoordinates anchorGridCoord, out bool value)
    {
        int yDif = localCoord.y - anchor.y;
        int zDif = localCoord.z - anchor.z;
        int xDif = localCoord.x - anchor.x;
        HousingGridCoordinates gridCoord = new HousingGridCoordinates(anchorGridCoord.y + yDif, anchorGridCoord.z + zDif, anchorGridCoord.x + xDif);
        value = GetAtIndex(localCoord);

        return gridCoord;
    }

    public HousingGridCoordinates GetPiledFurnitureGridCoord(HousingGridCoordinates localCoord, HousingGridCoordinates anchorGridCoord, out bool value)
    {
        if (furnitureBase == null) return GetGridCoord(localCoord, anchorGridCoord, out value);

        //anchors differences
        int yAnchorDif = currentAnchorGridPos.y - furnitureBase.currentAnchorGridPos.y;
        int zAnchorDif = currentAnchorGridPos.z - furnitureBase.currentAnchorGridPos.z;
        int xAnchorDif = currentAnchorGridPos.x - furnitureBase.currentAnchorGridPos.x;



        int yDif = localCoord.y - anchor.y;
        int zDif = localCoord.z - anchor.z;
        int xDif = localCoord.x - anchor.x;
        HousingGridCoordinates gridCoord = new HousingGridCoordinates(anchorGridCoord.y + yAnchorDif + yDif,
            anchorGridCoord.z + zAnchorDif + zDif, anchorGridCoord.x + xAnchorDif + xDif);
        value = GetAtIndex(localCoord);

        return gridCoord;
    }

    #region --- Get & Set ---
    bool GetAtIndex(int k, int i, int j)
    {
        return currentSpaces[k].spaces[i].row[j];
    }

    bool GetAtIndex(HousingGridCoordinates coord)
    {
        return currentSpaces[coord.y].spaces[coord.z].row[coord.x];
    }

    bool[] GetAtIndex(int k, int i)
    {
        return currentSpaces[k].spaces[i].row;
    }

    Row[] GetAtIndex(int k)
    {
        return currentSpaces[k].spaces;
    }

    void SetAtIndex(int k, int i, int j, bool val)
    {
        currentSpaces[k].spaces[i].row[j] = val;
    }

    void SetAtIndex(HousingGridCoordinates coord, bool val)
    {
        currentSpaces[coord.y].spaces[coord.z].row[coord.x] = val;
    }
    #endregion
}

public class FurnitureRotationData
    {
    public Direction currentOrientation;
    public FurnitureLevel[] currentSpaces;
    public HousingGridCoordinates anchor;
    public HousingGridCoordinates currentAnchorGridPos;
    public List<HousingFurniture> smallFurnitureOn;
    public FurnitureRotationData[] smallFurnitureOnRotData;

    public FurnitureRotationData(HousingFurniture _furniture)
    {
        currentOrientation = _furniture.currentOrientation;
        currentSpaces = _furniture.currentSpaces;
        anchor = _furniture.anchor;
        currentAnchorGridPos = _furniture.currentAnchorGridPos;
        smallFurnitureOn = _furniture.smallFurnitureOn;
        smallFurnitureOnRotData = new FurnitureRotationData[smallFurnitureOn.Count];

        for (int i = 0; i < smallFurnitureOn.Count; i++)
        {
            smallFurnitureOnRotData[i] = new FurnitureRotationData(smallFurnitureOn[i]);
        }
    }

}
