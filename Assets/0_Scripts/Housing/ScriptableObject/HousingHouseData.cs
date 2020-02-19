using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Direction
{
    Left,
    Right,
    Up,
    Down
}

[ExecuteInEditMode]
[CreateAssetMenu(fileName = "New housing house", menuName = "Housing/House")]
public class HousingHouseData : ScriptableObject
{
    public string houseName;

    public HouseDoor door;

    [Header(" --- House Space ---")]
    public int width = 10;
    public int depth = 10;
    public int height = 10;

    public HouseSpace houseSpace;
    public bool validHouseSpacing
    {
        get
        {
            return (houseSpace != null && houseSpace.houseLevels.Length > 0 && houseSpace.houseLevels[0] != null && houseSpace.houseLevels[0].houseLevelRows.Length > 0
                && houseSpace.houseLevels[0].houseLevelRows[0] != null && houseSpace.houseLevels[0].houseLevelRows[0].row.Length > 0);
        }
    }
    HouseSpace oldHouseSpace;
    private void OnValidate()
    {
        //CHECK IF houseSpacing CHANGED
        bool houseSpacingChanging = (oldHouseSpace != null && oldHouseSpace.houseLevels.Length != height);

        for (int k = 0; k < houseSpace.houseLevels.Length && !houseSpacingChanging; k++)
        {
            houseSpacingChanging = (houseSpace.houseLevels[k] != null && houseSpace.houseLevels[k].houseLevelRows.Length != depth);
            for (int i = 0; i < houseSpace.houseLevels[k].houseLevelRows.Length && !houseSpacingChanging; i++)
            {
                houseSpacingChanging = (houseSpace.houseLevels[k].houseLevelRows[i] != null && houseSpace.houseLevels[k].houseLevelRows[i].row.Length != width);
            }
        }

        //CREATE NEW SPACE INFO, AND COPY , IF POSSIBLE, PREVIOUS DATA
        if (houseSpacingChanging)
        {
            houseSpace = new HouseSpace(height, depth, width);
            CopyHouseSpace(oldHouseSpace, ref houseSpace);
        }

        //STORE COPY
        oldHouseSpace = new HouseSpace(height, depth, width);
        CopyHouseSpace(houseSpace, ref oldHouseSpace);
    }

    void CopyHouseSpace(HouseSpace origHouseSpace, ref HouseSpace targetHouseSpace)
    {
        if(origHouseSpace==null || targetHouseSpace == null)
        {
            Debug.LogError("HousingHouseData -> CopyHouseSpace: Error: origHouseSpace or targetHouseSpace are null");
            return;
        }

        for (int i = 0; i < origHouseSpace.houseLevels.Length && i < targetHouseSpace.houseLevels.Length; i++)
        {
            CopyHouseLevel(origHouseSpace.houseLevels[i], ref targetHouseSpace.houseLevels[i]);
        }
    }

    void CopyHouseLevel(HouseLevel origHouseLevel, ref HouseLevel targetHouseLevel)
    {
        if(origHouseLevel == null || targetHouseLevel == null)
        {
            Debug.LogError("HousingHouseData -> CopyHouseLevel: Error: origHouseLevel or targetHouseLevel are null");
            return;
        }

        for (int i = 0; i < origHouseLevel.houseLevelRows.Length && i < targetHouseLevel.houseLevelRows.Length; i++)
        {
            Row originRow = origHouseLevel.houseLevelRows[i];
            Row targetRow = targetHouseLevel.houseLevelRows[i];
            for (int j = 0; j < originRow.row.Length && j < targetRow.row.Length; j++)
            {
                //Debug.Log("Copying parameter from old ["+i+","+j+"]("+ originRow.row[j] + ") to new ["+i+","+j+ "](" + targetRow.row[j] + ")");
                targetRow.row[j] = originRow.row[j];
            }
        }
    }
}

[System.Serializable]
public class HouseSpace
{
    public HouseLevel[] houseLevels;
    public int height;
    public int width;
    public int depth;
    public HouseSpace(int _height, int _depth, int _width)
    {
        height = _height;
        depth = _depth;
        width = _width;
        houseLevels = new HouseLevel[height];
        for (int i = 0; i < houseLevels.Length; i++)
        {
            houseLevels[i] = new HouseLevel(depth, width);
        }
    }
}

[System.Serializable]
public class HouseLevel
{
    public int width;
    public int depth;
    public Row[] houseLevelRows;
    public HouseLevel(int _depth, int _width)
    {
        depth = _depth;
        width = _width;
        houseLevelRows = new Row[depth];
        for (int i = 0; i < houseLevelRows.Length; i++)
        {
            houseLevelRows[i] = new Row(width, true);
        }
    }
}

[System.Serializable]
public class HouseDoor
{
    public int x = 0;
    public int y = 0;
    public int height = 2;
    public Direction orientation;

    public HouseDoor(int _x=0, int _y=0, int _height = 2, Direction _orientation = Direction.Up)
    {
        x = _x;
        y = _y;
        height = _height;
        orientation = _orientation;
    }
}
