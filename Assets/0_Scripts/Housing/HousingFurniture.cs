using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingFurniture : MonoBehaviour
{
    public HousingFurnitureData furnitureMeta;
    public Direction currentOrientation;
    public FurnitureLevel[] currentSpaces;
    HousingGridCoordinates anchor;
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

    public void KonoAwake(HousingFurnitureData _furnitureMeta)
    {
        furnitureMeta = _furnitureMeta;
        currentOrientation = Direction.Up;
        currentSpaces = furnitureMeta.furnitureSpace;
        anchor = _furnitureMeta.anchor;
        switch (_furnitureMeta.orientation)
        {
            default: break;

            case Direction.Right:
                RotateClockWise();
                break;
            case Direction.Down:
                RotateClockWise();
                RotateClockWise();
                break;
            case Direction.Left:
                RotateCounterClockWise();
                break;
        }
    }

    public void Copy(HousingFurniture _furniture)
    {
        furnitureMeta = _furniture.furnitureMeta;
        currentOrientation = _furniture.currentOrientation;
        currentSpaces = _furniture.currentSpaces;
        anchor = _furniture.anchor;
    }

    public void RotateClockWise()
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
        //update new anchor
        HousingGridCoordinates transposedAnchor = new HousingGridCoordinates(0, anchor.z, anchor.x);
        anchor = new HousingGridCoordinates(0, transposedAnchor.z, (depth -1) - transposedAnchor.x);

        currentSpaces = newSpaces;

        //Rotate actual gameObject
        transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y + 90, 0);
    }

    public void RotateCounterClockWise()
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
            //reverse columns
            for (int j = 0; j < depth; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    newSpaces[k].spaces[(width-1) - i].row[j] = transposeMatrix.spaces[i].row[j];
                }
            }
        }
        //update new anchor
        HousingGridCoordinates transposedAnchor = new HousingGridCoordinates(0, anchor.z, anchor.x);
        anchor = new HousingGridCoordinates(0, (width - 1) - transposedAnchor.z, transposedAnchor.x);

        currentSpaces = newSpaces;
        //Rotate actual gameObject
        transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y - 90, 0);
    }

    public HousingGridCoordinates GetGridCoord(HousingGridCoordinates localCoord)
    {
        int xDif = localCoord.x - anchor.x;
        int zDif = localCoord.z - anchor.z;
        HousingGridCoordinates gridCoord = new HousingGridCoordinates(localCoord.y, currentAnchorGridPos.z + zDif, currentAnchorGridPos.x + xDif);

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
