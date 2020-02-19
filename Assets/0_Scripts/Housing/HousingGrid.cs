using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingGrid : MonoBehaviour
{
    HousingHouseData myHouseMeta;
    public float slotSize;

    public int width;
    public int depth;
    public int height;
    GameObject slotPrefab;
    Vector3 housePos = Vector3.zero;

    HousingSlot[,,] slots;

    public void KonoAwake(HousingHouseData _myHouseMeta, GameObject _slotPrefab, Vector3 _housePos)
    {
        myHouseMeta = _myHouseMeta;
        width = myHouseMeta.width;
        depth = myHouseMeta.depth;
        height = myHouseMeta.height;

        slots = new HousingSlot[width, depth, height];

        slotPrefab = _slotPrefab;
        housePos = _housePos;
    }

    public void CreateGrid()
    {
        if (myHouseMeta == null) return;

        float slotSize = myHouseMeta.housingSlotSize;

        //fill up the slots
        for (int k = 0; k < slots.GetLength(0); k++)//for every level
        {
            //Instantiate level's empty parent
            Vector3 newLevelParentPos = new Vector3(housePos.x, housePos.y + (slotSize * k), housePos.z);
            Transform newLevelParent = Instantiate(new GameObject(), newLevelParentPos, Quaternion.identity, transform.parent).transform;
            newLevelParent.gameObject.name = "Level " + (k + 1);
            for (int i = 0; i < slots.GetLength(1); i++)//for every row
            {
                //Instantiate row's empty parent
                Vector3 newRowParentPos = new Vector3(newLevelParentPos.x, newLevelParentPos.y, newLevelParentPos.z + (slotSize * i));
                Transform newRowParent = Instantiate(new GameObject(), newRowParentPos, Quaternion.identity, newLevelParent).transform;
                newRowParent.gameObject.name = "Row " + (i + 1);
                for (int j = 0; j < slots.GetLength(2); j++)//for every column
                {
                    if (myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j])
                    {
                        //Instantiate slot
                        Vector3 newSlotPos = new Vector3(newRowParentPos.x + (slotSize * j), newRowParentPos.y, newRowParentPos.z);
                        GameObject newSlotObject = Instantiate(slotPrefab, newSlotPos, Quaternion.identity, newRowParent);
                        HousingSlot newSlot = newSlotObject.GetComponent<HousingSlot>();
                        HousingGridCoordinates coord = new HousingGridCoordinates(k, i, j);

                        #region -- Slot Type --
                        //Decide type of slot
                        HousingSlotType slotType = HousingSlotType.None;
                        bool floor = k == 0;
                        bool ceiling = k == height - 1;
                        bool wallLeft, wallRight, wallUp, wallDown;
                        //check for wall left
                        wallLeft = (i - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i - 1].row[j];
                        //check for wall right
                        wallRight = (i + 1) < width && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i + 1].row[j];
                        //check for wall Up
                        wallUp = (j - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j-1];
                        //check for wall Down
                        wallDown = (j + 1) < depth && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j+1];

                        bool wall = wallLeft || wallRight || wallUp || wallDown;
                        slotType = wall ? (floor ? HousingSlotType.WallAndFloor : HousingSlotType.Wall) : floor? HousingSlotType.Floor: HousingSlotType.None;
                        #endregion

                        //Initialize slot
                        newSlot.KonoAwake(coord, slotSize, slotType, wallLeft, wallRight, wallUp, wallDown);

                        //Add slot to slots array
                        slots[k, i, j] = newSlot;
                    }
                    else
                    {
                        slots[k, i, j] = null;
                    }
                }
            }
        }
    }
}
