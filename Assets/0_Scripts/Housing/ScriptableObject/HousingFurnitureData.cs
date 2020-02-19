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

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New housing furniture", menuName = "Housing/Furniture")]
public class HousingFurnitureData : ScriptableObject
{
    public string furnitureName;
    public FurnitureType furnitureType = FurnitureType.None;
    [Tooltip("Only for wall furniture.")]
    public bool thickness = false;
    public GameObject prefab;
    [Header(" --- Furniture Space --- ")]
    public int rows = 3;
    public int columns = 3;
    public bool autoAdjustSpaces = false;
    [Tooltip("The size of this array is the amount of levels you want it to have, or in other words, the maximum height of the furniture." +
        " Every element in this array represents a top view of the furniture at every height level.")]
    public FurnitureLevel[] furnitureSpace;
    FurnitureLevel[] furnitureSpaceOld;


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
    public int minY
    {
        get
        {
            int auxMinY = -1;
            if (validFurnitureSpace)
            {
                auxMinY = furnitureSpace[0].spaces.Length-1;
                for (int k = 0; k < furnitureSpace.Length; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (i < auxMinY) auxMinY = i;
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
                auxMaxY = 0;
                for (int k = 0; k < furnitureSpace.Length; k++)
                {
                    for (int i = 0; i < furnitureSpace[k].spaces.Length; i++)
                    {
                        for (int j = 0; j < furnitureSpace[k].spaces[i].row.Length; j++)
                        {
                            if (furnitureSpace[k].spaces[i].row[j])
                            {
                                if (i > auxMaxY) auxMaxY = i;
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
                auxDepth = maxY - minY + 1;
            }
            return auxDepth;
        }
    }


    private void OnValidate()
    {
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
        Debug.Log("Trying to Auto Adjust Space");
        if(minX > 0)
        {
            int moveAmount = minX;
            AdjustSpace(Direction.Left, moveAmount);
        }

        if(minY > 0)
        {
            int moveAmount = minY;
            AdjustSpace(Direction.Up, moveAmount);
        }
    }

    void AdjustSpace (Direction dir, int moveAmount)
    {
        Debug.Log("Adjusting Space in direction " + dir + " with amount " + moveAmount);
        for (int i = 0; i < furnitureSpace.Length; i++)
        {
            furnitureSpace[i].Adjust(dir, moveAmount);
        }
    }
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
        if(rows != spaces.Length)
        {
            Debug.LogError("FurnitureLevel -> Adjust: Error: rows("+rows+") and spaces.Length("+ spaces.Length + ") do not match");
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
                        Debug.Log("Adjust: Moving ["+i+","+(j+moveAmount)+"] to ["+i+","+j+"] with value = " + spaces[i].row[j + moveAmount]);
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