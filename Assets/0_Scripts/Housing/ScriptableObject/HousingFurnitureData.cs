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
    chair,
    illumination,
    wardrove,
    decoration,
    table,
    Stinray,
    Eridon,
    Sochi,
    Vanilla,
    Oktirome
}

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New housing furniture", menuName = "Housing/Furniture")]
public class HousingFurnitureData : ScriptableObject
{
    public string furnitureName;
    public FurnitureTag[] tags;
    public Sprite icon;
    public FurnitureType furnitureType = FurnitureType.None;
    public GameObject prefab;
    [Tooltip("The default rotation of the furniture. Normal value = Up. Left means looking to the positive X, " +
        "Right means looking to negative X, Up means looking to negative Z, Down means looking to positive Z.")]
    public Direction orientation = Direction.Up;

    [Header(" --- Furniture Space --- ")]
    public bool autoAnchor = true;
    [Tooltip("This is used as the center of rotation when rotating the furniture.")]
    public HousingGridCoordinates anchor;
    public int rows = 3;
    public int columns = 3;
    public bool autoAdjustSpaces = false;
    [Tooltip("The size of this array is the amount of levels you want it to have, or in other words, the maximum height of the furniture." +
        " Every element in this array represents a top view of the furniture at every height level.")]
    public FurnitureLevel[] furnitureSpace;
    FurnitureLevel[] furnitureSpaceOld;

    [Header(" -- WALL FURNITURE -- ")]
    [Tooltip("Only for wall furniture.")]
    public bool thickness = false;

    public bool validFurnitureSpace
    {
        get
        {
            return (furnitureSpace.Length != 0 && furnitureSpace.Length > 0 && furnitureSpace[0] != null && furnitureSpace[0].spaces.Length > 0 &&
                    furnitureSpace[0].spaces[0] != null && furnitureSpace[0].spaces[0].row.Length > 0);
        }
    }

    public int minX
    {
        get
        {
            int auxMinX = -1;
            if (validFurnitureSpace)
            {
                auxMinX = furnitureSpace[0].spaces[0].row.Length - 1;
                for (int k = 0; k < furnitureSpace.Length; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (j < auxMinX) auxMinX = j;
                            }
                        }
                    }
                }
            }
            return auxMinX;
        }
    }
    public int maxX
    {
        get
        {
            int auxMaxX = -1;
            if (validFurnitureSpace)
            {
                auxMaxX = 0;
                for (int k = 0; k < furnitureSpace.Length; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (j > auxMaxX) auxMaxX = j;
                            }
                        }
                    }
                }
            }
            return auxMaxX;
        }
    }
    public int minZ
    {
        get
        {
            int auxMinZ = -1;
            if (validFurnitureSpace)
            {
                auxMinZ = furnitureSpace[0].spaces.Length - 1;
                for (int k = 0; k < furnitureSpace.Length; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (i < auxMinZ) auxMinZ = i;
                            }
                        }
                    }
                }
            }
            return auxMinZ;
        }
    }
    public int maxZ
    {
        get
        {
            int auxMaxZ = -1;
            if (validFurnitureSpace)
            {
                auxMaxZ = 0;
                for (int k = 0; k < furnitureSpace.Length; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (i > auxMaxZ) auxMaxZ = i;
                            }
                        }
                    }
                }
            }
            return auxMaxZ;
        }
    }
    public int minY
    {
        get
        {
            int auxMinY = -1;
            if (validFurnitureSpace)
            {
                bool found = false;
                auxMinY = furnitureSpace.Length - 1;
                for (int k = 0; k < furnitureSpace.Length && !found; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length && !found; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length && !found; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (k < auxMinY) auxMinY = k;
                                found = true;
                            }
                        }
                    }
                }
            }
            return auxMinY;
        }
    }
    public int maxY
    {
        get
        {
            int auxMaxY = -1;
            if (validFurnitureSpace)
            {
                bool found = false;
                auxMaxY = 0;
                for (int k = furnitureSpace.Length - 1; k >= 0 && !found; k--)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length && !found; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length && !found; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (k > auxMaxY) auxMaxY = k;
                                found = true;
                            }
                        }
                    }
                }
            }
            return auxMaxY;
        }
    }
    public int width
    {
        get
        {
            int auxWidth = -1;
            if (validFurnitureSpace)
            {
                auxWidth = maxX - minX + 1;
            }
            return auxWidth;
        }
    }
    public int depth
    {
        get
        {
            int auxDepth = -1;
            if (validFurnitureSpace)
            {
                auxDepth = maxZ - minZ + 1;
            }
            return auxDepth;
        }
    }
    public int height
    {
        get
        {
            int auxHeight = -1;
            if (validFurnitureSpace)
            {
                auxHeight = (maxY - minY) + 1;
            }
            return auxHeight;
        }
    }
    public int widthOrient
    {
        get
        {
            int auxWidth = -1;
            if (orientation == Direction.Up || orientation == Direction.Down)
            {
                auxWidth = width;
            }
            else
            {
                auxWidth = depth;
            }
            return auxWidth;
        }
    }
    public int depthOrient
    {
        get
        {
            int auxDepth = -1;
            if (orientation == Direction.Up || orientation == Direction.Down)
            {
                auxDepth = depth;
            }
            else
            {
                auxDepth = width;
            }
            return auxDepth;
        }
    }


    private void OnValidate()
    {
        AutoAnchor();

        if (autoAdjustSpaces)
        {
            autoAdjustSpaces = false;
            AutoAdjustSpace();
        }


        //CHECK IF ANY SPACE PARAMETER CHANGED
        bool canvasSpaceChanged = (furnitureSpaceOld != null && furnitureSpace.Length != furnitureSpaceOld.Length);

        for (int i = 0; i < furnitureSpace.Length && !canvasSpaceChanged; i++)
        {
            if (furnitureSpace[i].spaces.Length != rows) canvasSpaceChanged = true;
            for (int j = 0; j < furnitureSpace[i].spaces.Length && !canvasSpaceChanged; j++)
            {
                if (furnitureSpace[i].spaces[j].row.Length != columns) canvasSpaceChanged = true;
            }
        }

        //CREATE NEW SPACE INFO, AND COPY , IF POSSIBLE, PREVIOUS DATA
        if (canvasSpaceChanged)
        {
            for (int i = 0; i < furnitureSpace.Length; i++)
            {
                furnitureSpace[i] = new FurnitureLevel(rows, columns);

                CopyFurnitureSpace(furnitureSpaceOld, ref furnitureSpace);
            }
        }


        //STORE COPY
        furnitureSpaceOld = new FurnitureLevel[furnitureSpace.Length];
        for (int i = 0; i < furnitureSpaceOld.Length; i++)
        {
            furnitureSpaceOld[i] = new FurnitureLevel(rows, columns);
        }
        CopyFurnitureSpace(furnitureSpace, ref furnitureSpaceOld);
    }

    void CopyFurnitureSpace(FurnitureLevel[] originFurnitureSpace, ref FurnitureLevel[] targetFurnitureSpace)
    {
        if (originFurnitureSpace != null && targetFurnitureSpace != null)
        {
            for (int i = 0; i < targetFurnitureSpace.Length && i < originFurnitureSpace.Length; i++)
            {
                CopyFurnitureLevel(originFurnitureSpace[i], ref targetFurnitureSpace[i]);
            }
        }
        else
        {
            Debug.LogError("HousingFurnitureData -> CopyFurnitureSpace: Couldn't copy the furniture space due to either originFurnitureSpace or targetFurnitureSpace being null.");
        }
    }

    void CopyFurnitureLevel(FurnitureLevel origFurnLev, ref FurnitureLevel targetFurnLev)
    {
        //targetFurnLev.rows = origFurnLev.rows;
        //targetFurnLev.columns = origFurnLev.columns;
        for (int i = 0; i < origFurnLev.spaces.Length && i < targetFurnLev.spaces.Length; i++)
        {
            Row originRow = origFurnLev.spaces[i];
            Row targetRow = targetFurnLev.spaces[i];
            for (int j = 0; j < originRow.row.Length && j < targetRow.row.Length; j++)
            {
                //Debug.Log("Copying parameter from old ["+i+","+j+"]("+ originRow.row[j] + ") to new ["+i+","+j+ "](" + targetRow.row[j] + ")");
                targetRow.row[j] = originRow.row[j];
            }
        }
    }

    void AutoAdjustSpace()
    {
        if (validFurnitureSpace)
        {
            Debug.Log("Trying to Auto Adjust Space");
            if (minX > 0)
            {
                int moveAmount = minX;
                AdjustSpace(Direction.Left, moveAmount);
            }

            if (minZ > 0)
            {
                int moveAmount = minZ;
                AdjustSpace(Direction.Up, moveAmount);
            }


            //Create New copy "old"
            furnitureSpaceOld = new FurnitureLevel[furnitureSpace.Length];
            for (int i = 0; i < furnitureSpaceOld.Length; i++)
            {
                furnitureSpaceOld[i] = new FurnitureLevel(rows, columns);
            }
            CopyFurnitureSpace(furnitureSpace, ref furnitureSpaceOld);

            //Check for empty rows and columns

            //REMOVE EMPTY ROWS
            for (int i = GetAtIndex(0).Length - 1; i >= 0 && i > maxZ; i--)
            {
                //Debug.Log("Looking for empty Columns: i = " + i);
                bool emptyRow = true;
                for (int k = 0; k < furnitureSpace.Length && emptyRow; k++)
                {
                    //Debug.Log("Looking for empty Columns: k = " + k);

                    for (int j = 0; j < GetAtIndex(0, i).Length && emptyRow; j++)
                    {
                        //Debug.Log("Looking for empty Columns: j = " + j);

                        if (GetAtIndex(k, i, j))
                        {
                            emptyRow = false;
                        }
                    }
                }
                if (emptyRow)
                {
                    //Debug.Log("remove a column");
                    rows--;
                }
            }
            if (columns <= 0 || rows <= 0) return;

            //REMOVE EMPTY ROWS
            for (int j = GetAtIndex(0, 0).Length - 1; j >= 0 && j > maxX; j--)
            {
                bool emptyColumn = true;
                for (int k = 0; k < furnitureSpace.Length && emptyColumn; k++)
                {
                    for (int i = 0; i < GetAtIndex(0, 0).Length && emptyColumn; i++)
                    {
                        if (GetAtIndex(k, i, j))
                        {
                            emptyColumn = false;
                        }
                    }
                }
                if (emptyColumn)
                {
                    //Debug.Log("remove a row");
                    columns--;
                }
            }
        }
    }

    void AdjustSpace(Direction dir, int moveAmount)
    {
        //Debug.LogWarning("Adjusting Space in direction " + dir + " with amount " + moveAmount);
        for (int i = 0; i < furnitureSpace.Length; i++)
        {
            furnitureSpace[i].Adjust(dir, moveAmount);
        }
    }

    void AutoAnchor()
    {
        if (!autoAnchor) return;
        autoAnchor = false;
        HousingGridCoordinates center = new HousingGridCoordinates(0, minZ + depth / 2, minX + width / 2);
        //HousingGridCoordinates newAnchor = new HousingGridCoordinates();
        //int distToCenter = int.MaxValue;
        //    for (int i = 0; i < depth; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //        //if(GetAtIndex(k,i,j))
        //        int auxDist = Mathf.Abs(i - minX) + Mathf.Abs(j - depth);
        //        if (auxDist < distToCenter)
        //        {
        //            distToCenter = auxDist;
        //            newAnchor = new HousingGridCoordinates()
        //        }
        //    }
        anchor = center;
    }

    public bool HasTag(FurnitureTag tag)
    {
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i] == tag) return true;
        }
        return false;
    }

    //bool CheckValidAnchor()
    //{
    //    return GetAtIndex(anchor);
    //}

    #region --- Get ---
    bool GetAtIndex(int k, int i, int j)
    {
        return furnitureSpace[k].spaces[i].row[j];
    }

    bool GetAtIndex(HousingGridCoordinates coord)
    {
        return furnitureSpace[coord.y].spaces[coord.z].row[coord.x];
    }

    bool[] GetAtIndex(int k, int i)
    {
        return furnitureSpace[k].spaces[i].row;
    }

    Row[] GetAtIndex(int k)
    {
        return furnitureSpace[k].spaces;
    }
    #endregion
}

[System.Serializable]
public class FurnitureLevel
{
    public Row[] spaces;
    public int rows;
    public int columns;

    public FurnitureLevel(int _rows, int _columns)
    {
        rows = _rows;
        columns = _columns;
        spaces = new Row[rows];
        for (int i = 0; i < spaces.Length; i++)
        {
            spaces[i] = new Row(_columns);
        }
        //row2 = new bool[columns];
        //row3 = new bool[columns];
    }

    public void Adjust(Direction dir, int moveAmount)
    {
        if (rows != spaces.Length)
        {
            Debug.LogError("FurnitureLevel -> Adjust: Error: rows(" + rows + ") and spaces.Length(" + spaces.Length + ") do not match");
            return;
        }
        if (columns != spaces[0].row.Length)
        {
            Debug.LogError("FurnitureLevel -> Adjust: Error: columns(" + columns + ") and spaces[0].row.Length(" + spaces[0].row.Length + ") do not match");
            return;
        }

        //Create New spaces to write new positions
        Row[] newSpaces = new Row[rows];
        for (int i = 0; i < newSpaces.Length; i++)
        {
            newSpaces[i] = new Row(columns);
        }
        switch (dir)
        {
            case Direction.Left:
                for (int i = 0; i < spaces.Length; i++)
                {
                    for (int j = 0; j < spaces[i].row.Length && (j + moveAmount) < spaces[i].row.Length; j++)
                    {
                        newSpaces[i].row[j] = spaces[i].row[j + moveAmount];
                        //Debug.Log("Adjust: Moving [" + i + "," + (j + moveAmount) + "] to [" + i + "," + j + "] with value = " + spaces[i].row[j + moveAmount]);
                    }
                }
                break;
            case Direction.Right:
                for (int i = 0; i < spaces.Length; i++)
                {
                    //Row newRow = new Row(columns);
                    for (int j = spaces[i].row.Length - 1; j > 0 && (j - moveAmount) > 0; j--)
                    {
                        newSpaces[i].row[j] = spaces[i].row[j - moveAmount];
                    }
                    //spaces[i] = newRow;
                }
                break;
            case Direction.Up:
                for (int i = 0; i < spaces.Length && (i + moveAmount) < spaces.Length; i++)
                {
                    //Row newRow = new Row(columns);
                    for (int j = 0; j < spaces[i].row.Length; j++)
                    {
                        newSpaces[i].row[j] = spaces[i + moveAmount].row[j];
                    }
                    //spaces[i] = newRow;
                }
                break;
            case Direction.Down:
                for (int i = spaces.Length; i > 0 && (i - moveAmount) > 0; i++)
                {
                    //Row newRow = new Row(columns);
                    for (int j = 0; j < spaces[i].row.Length; j++)
                    {
                        newSpaces[i].row[j] = spaces[i - moveAmount].row[j];
                    }
                    //spaces[i] = newRow;
                }
                break;
        }

        //Copy new positions into the original Spaces
        for (int i = 0; i < spaces.Length; i++)
        {
            for (int j = 0; j < spaces[i].row.Length; j++)
            {
                spaces[i].row[j] = newSpaces[i].row[j];
            }
        }
    }

    //public void Resize(int new)
    //}
}

[System.Serializable]
public class Row
{
    public bool[] row;
    public Row(int _columns, bool defaultValue = false)
    {
        row = new bool[_columns];
        if (defaultValue)
        {
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = defaultValue;
            }
        }
    }
}