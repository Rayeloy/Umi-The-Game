﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingGrid : MonoBehaviour
{
    HousingHouseData myHouseMeta;

    public int width;
    public int depth;
    public int height;
    GameObject slotPrefab;
    GameObject wallPrefab;
    Vector3 housePos = Vector3.zero;
    public Vector3 center
    {
        get
        {
            Vector3 result = Vector3.zero;
            if (myHouseMeta.validHouseSpacing)
            {
                Vector3 min = housePos;
                min.x += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.minX);
                min.z += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.minZ);
                min.y += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.minY);
                Vector3 max = housePos;
                max.x += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.maxX);
                max.z += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.maxZ);
                max.y += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.maxY);

                result = max - min;
            }

            return result;
        }
    }

    HousingSlot[,,] slots;// (level,row,column)

    public void KonoAwake(HousingHouseData _myHouseMeta, GameObject _slotPrefab, GameObject _wallPrefab, Vector3 _housePos)
    {
        myHouseMeta = _myHouseMeta;

        width = myHouseMeta.width;
        depth = myHouseMeta.depth;
        height = myHouseMeta.height;

        slots = new HousingSlot[height, depth, width];

        slotPrefab = _slotPrefab;
        wallPrefab = _wallPrefab;
        housePos = _housePos;
    }

    public void CreateGrid(bool showSlotMeshes = false)
    {
        if (myHouseMeta == null) return;

        float slotSize = myHouseMeta.housingSlotSize;

        //fill up the slots
        for (int k = 0; k < slots.GetLength(0); k++)//for every level
        {
            //Instantiate level's empty parent
            Vector3 newLevelParentPos = new Vector3(housePos.x, housePos.y + (slotSize * k), housePos.z);
            Transform newLevelParent = new GameObject("Level " + (k + 1)).transform;
            newLevelParent.position = newLevelParentPos; newLevelParent.rotation = Quaternion.identity; newLevelParent.parent = transform;
            for (int i = 0; i < slots.GetLength(1); i++)//for every row
            {
                //Instantiate row's empty parent
                Vector3 newRowParentPos = new Vector3(newLevelParentPos.x, newLevelParentPos.y, newLevelParentPos.z - (slotSize * i));
                Transform newRowParent = new GameObject("Row " + (i + 1)).transform;
                newRowParent.position = newRowParentPos; newRowParent.rotation = Quaternion.identity; newRowParent.parent = newLevelParent;
                for (int j = 0; j < slots.GetLength(2); j++)//for every column
                {
                    if (myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j])
                    {
                        //Instantiate slot
                        Vector3 newSlotPos = new Vector3(newRowParentPos.x + (slotSize * j), newRowParentPos.y, newRowParentPos.z);
                        GameObject newSlotObject = Instantiate(slotPrefab, newSlotPos, Quaternion.identity, newRowParent);
                        newSlotObject.name = "Slot " + j;
                        newSlotObject.transform.localScale = new Vector3(slotSize, slotSize, slotSize);
                        HousingSlot newSlot = newSlotObject.GetComponent<HousingSlot>();
                        newSlotObject.GetComponent<MeshRenderer>().enabled = showSlotMeshes;
                        HousingGridCoordinates coord = new HousingGridCoordinates(k, i, j);

                        #region -- Slot Type --
                        //Decide type of slot
                        HousingSlotType slotType = HousingSlotType.None;
                        bool floor = k == 0;
                        bool ceiling = k == height - 1;
                        bool wallLeft, wallRight, wallUp, wallDown;
                        //check for wall left
                        wallLeft = j==0 || ((j - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j - 1]);
                        //check for wall right
                        wallRight = j == width-1 || ((j + 1) < width && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j + 1]);
                        //check for wall Up
                        wallUp = i == 0 || ((i - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i - 1].row[j]);
                        //check for wall Down
                        wallDown = i == depth-1 || ((i + 1) < depth && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i + 1].row[j]);

                        bool wall = wallLeft || wallRight || wallUp || wallDown;
                        slotType = wall ? (floor ? HousingSlotType.WallAndFloor : HousingSlotType.Wall) : floor ? HousingSlotType.Floor : HousingSlotType.None;
                        Debug.Log("wall = " + wall + "; floor = " + floor);
                        #endregion
                        Debug.Log("Housing: Initializing slot "+ coord.printString + " with slotType = " +slotType);
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

    public void CreateWalls()
    {
        if (myHouseMeta == null) return;

        float wallDist = (myHouseMeta.housingSlotSize / 2) + (wallPrefab.transform.lossyScale.z / 2)/*GetComponent<Collider>().bounds.extents.z*/;
        HouseLevel[] houseLevels = myHouseMeta.houseSpace.houseLevels;

        for (int k = 0; k < houseLevels.Length; k++)//for every level
        {
            for (int i = 0; i < houseLevels[k].houseLevelRows.Length; i++)//for every row
            {
                for (int j = 0; j < houseLevels[k].houseLevelRows[i].row.Length; j++)//for every column
                {
                    if (slots[k, i, j] == null) continue;

                    //Debug.Log("Create House Walls: ("+k+","+i+","+j+")");
                    //LEFT
                    if (j - 1 < 0 || (j - 1 >= 0 && !houseLevels[k].houseLevelRows[i].row[j - 1]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x - wallDist, parentSlot.transform.position.y, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 270, 0), null);
                        wallObject.name = "LeftWall";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }

                    //RIGHT
                    if (j + 1 >= houseLevels[k].houseLevelRows[i].row.Length || (j + 1 < houseLevels[k].houseLevelRows[i].row.Length && !houseLevels[k].houseLevelRows[i].row[j + 1]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x + wallDist, parentSlot.transform.position.y, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), null);
                        wallObject.name = "RightWall";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }

                    //FRONT
                    if (i - 1 < 0 || (i - 1 >= 0 && !houseLevels[k].houseLevelRows[i - 1].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y, parentSlot.transform.position.z + wallDist);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 0, 0), null);
                        wallObject.name = "FrontWall";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }

                    //BACK
                    if (i + 1 >= houseLevels[k].houseLevelRows.Length || (i + 1 < houseLevels[k].houseLevelRows.Length && !houseLevels[k].houseLevelRows[i + 1].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y, parentSlot.transform.position.z - wallDist);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 180, 0), null);
                        wallObject.name = "BackWall";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }

                    //Floor
                    if (k - 1 < 0 || (k - 1 >= 0 && !houseLevels[k - 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y - wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(90, 0, 0), null);
                        wallObject.name = "FloorTile";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        //Debug.Log("Wall " + wallObject.name + " localScale =  " + wallObject.transform.localScale + "; slotSize = " + myHouseMeta.housingSlotSize);
                        wallObject.transform.parent = parentSlot;
                    }

                    //Ceiling
                    if (k + 1 >= houseLevels.Length || (k + 1 < houseLevels.Length && !houseLevels[k + 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y + wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(-90, 0, 0), null);
                        wallObject.name = "CeilingTile";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the spawn position for the house.
    /// </summary>
    public bool CreateDoor(out Vector3 playerSpawnPos, out Quaternion playerSpawnRot)
    {
        // SET DOOR FURNITURE
        HouseDoor door = myHouseMeta.door;

        if(!SetWallFurniture(new HousingGridCoordinates(0, door.x, door.z), door.doorMeta))
        {
            playerSpawnPos = Vector3.zero; playerSpawnRot = Quaternion.identity;
            Debug.LogError("Can't create door");
            return false;
        }

        playerSpawnPos = slots[1, door.x, door.z].transform.position;
        playerSpawnRot = Quaternion.Euler(0, door.doorMeta.orientation == Direction.Up ? 180 : door.doorMeta.orientation == Direction.Down ? 0 :
            door.doorMeta.orientation == Direction.Left ? 90 : door.doorMeta.orientation == Direction.Right ? 270 : 0, 0);
        return true;
    }

    public void SetFurniture(HousingGridCoordinates coord, HousingFurnitureData furnitureMeta)
    {

    }

    public bool SetWallFurniture(HousingGridCoordinates coord, HousingFurnitureData furnitureMeta)
    {
        bool result = false;

        Transform slotTrans = slots[coord.y, coord.z, coord.x].transform;
        GameObject furnitureObject = Instantiate(furnitureMeta.prefab, Vector3.zero, Quaternion.identity, slotTrans);

        Debug.Log("Furniture Data: Coord = "+ coord.printString +"; height = "+ furnitureMeta.height + "; depth = " + furnitureMeta.depthOrient + "; width = " + furnitureMeta.widthOrient);
        for (int k = 0; k < furnitureMeta.height; k++)
        {
            for (int i = 0; i < furnitureMeta.depthOrient; i++)
            {
                for (int j = 0; j < furnitureMeta.widthOrient; j++)
                {
                    if (!slots[coord.y + k, coord.z + i, coord.x + j].SetFurniture(furnitureMeta, furnitureObject))
                    {
                        Debug.LogError("Can't place wall furniture on ("+ (coord.y + k) + ","+ (coord.z + i) + ","+ (coord.x + j) + ") at orientation "+ furnitureMeta.orientation);
                        return false;
                    }
                }
            }
        }

        float slotSize = myHouseMeta.housingSlotSize;
        Vector3 slotPos = slotTrans.position;
        Collider col = furnitureObject.GetComponentInChildren<Collider>();
        if (col == null)
        {
            Debug.LogError("HousingGrid -> SetWallFurniture -> Error: Can't find the collider of the furniture "+furnitureMeta.name+".");
            //playerSpawnPos = Vector3.zero; playerSpawnRot = Quaternion.identity;
            return false;
        }
        float y = slotPos.y - (slotSize / 2) + col.bounds.extents.y;
        float x = slotPos.x + (furnitureMeta.orientation == Direction.Left ? -slotSize / 2 : furnitureMeta.orientation == Direction.Right ? +slotSize / 2 : 0);
        float z = slotPos.z + (furnitureMeta.orientation == Direction.Up ? +slotSize / 2 : furnitureMeta.orientation == Direction.Down ? -slotSize / 2 : 0);
        Vector3 pos = new Vector3(x, y, z);
        Quaternion rot = Quaternion.Euler(0, furnitureMeta.orientation == Direction.Up ? 0 : furnitureMeta.orientation == Direction.Down ? 180 :
            furnitureMeta.orientation == Direction.Left ? 270 : furnitureMeta.orientation == Direction.Right ? 90 : 0, 0);
        furnitureObject.transform.position = pos;
        furnitureObject.transform.rotation = rot;
        result = true;

        return result;
    }
}
