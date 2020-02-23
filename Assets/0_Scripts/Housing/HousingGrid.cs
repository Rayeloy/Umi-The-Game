using System.Collections;
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

    HousingSlot[,,] slots;

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
                        wallLeft = (j - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j-1];
                        //check for wall right
                        wallRight = (j + 1) < width && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j+1];
                        //check for wall Up
                        wallUp = (i - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i - 1].row[j];
                        //check for wall Down
                        wallDown = (i + 1) < depth && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i + 1].row[j];

                        bool wall = wallLeft || wallRight || wallUp || wallDown;
                        slotType = wall ? (floor ? HousingSlotType.WallAndFloor : HousingSlotType.Wall) : floor ? HousingSlotType.Floor : HousingSlotType.None;
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

    public void CreateWalls()
    {
        if (myHouseMeta == null) return;

        float wallDist = (myHouseMeta.housingSlotSize / 2) + (wallPrefab.transform.lossyScale.z/2)/*GetComponent<Collider>().bounds.extents.z*/;
        HouseLevel[] houseLevels = myHouseMeta.houseSpace.houseLevels;

        for (int k = 0; k < houseLevels.Length; k++)//for every level
        {
            for (int i = 0; i < houseLevels[k].houseLevelRows.Length; i++)//for every row
            {
                for (int j = 0; j < houseLevels[k].houseLevelRows[i].row.Length; j++)//for every column
                {
                    if (slots[k, i, j] == null) continue;

                    Debug.Log("Create House Walls: ("+k+","+i+","+j+")");
                    //LEFT
                    if (j-1 <0 || (j-1 >= 0 && !houseLevels[k].houseLevelRows[i].row[j-1]))
                    {
                        Transform parentSlot = slots[k,i,j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x - wallDist, parentSlot.transform.position.y, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0,270,0), null);
                        wallObject.name = "LeftWall";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }

                    //RIGHT
                    if (j+1 >= houseLevels[k].houseLevelRows[i].row.Length || (j + 1 < houseLevels[k].houseLevelRows[i].row.Length && !houseLevels[k].houseLevelRows[i].row[j + 1]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x + wallDist, parentSlot.transform.position.y, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), null);
                        wallObject.name = "RightWall";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        wallObject.transform.parent = parentSlot;
                    }

                    //FRONT
                    if (i - 1 < 0 || (i - 1 >= 0 && !houseLevels[k].houseLevelRows[i-1].row[j]))
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
                    if (k-1 < 0 || (k - 1 >= 0 && !houseLevels[k - 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y-wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(90, 0, 0), null);
                        wallObject.name = "FloorTile";
                        wallObject.transform.localScale = new Vector3(myHouseMeta.housingSlotSize, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z);
                        Debug.Log("Wall " + wallObject.name + " localScale =  " + wallObject.transform.localScale + "; slotSize = " + myHouseMeta.housingSlotSize);
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
        HouseDoor door = myHouseMeta.door;
        float slotSize = myHouseMeta.housingSlotSize;
        Transform slotTrans = slots[0, door.x, door.z].transform;
        GameObject doorObject = Instantiate(door.doorMeta.prefab, Vector3.zero, Quaternion.identity, slotTrans);
        Vector3 slotPos = slotTrans.position;
        Collider col = doorObject.GetComponentInChildren<Collider>();
        if(col == null)
        {
            Debug.LogError("HousingGrid -> CreateDoor -> Error: Can't find the collider of the door.");
            playerSpawnPos = Vector3.zero; playerSpawnRot = Quaternion.identity;
            return false;
        }
        float y = slotPos.y - (slotSize / 2) + col.bounds.extents.y;
        float x = slotPos.x + (door.doorMeta.orientation == Direction.Left? -slotSize /2: door.doorMeta.orientation == Direction.Right ?+slotSize/2:0);
        float z = slotPos.z + (door.doorMeta.orientation == Direction.Up ? +slotSize/2: door.doorMeta.orientation == Direction.Down ? -slotSize/2 : 0);
        Vector3 doorPos = new Vector3(x, y, z);
        Quaternion doorRot = Quaternion.Euler(0, door.doorMeta.orientation == Direction.Up ? 0: door.doorMeta.orientation == Direction.Down ?180:
            door.doorMeta.orientation == Direction.Left ?270: door.doorMeta.orientation == Direction.Right ?90 : 0, 0);
        doorObject.transform.position = doorPos;
        doorObject.transform.rotation = doorRot;

        for (int k = 0; k < door.doorMeta.height; k++)
        {
            for (int i = 0; i < door.doorMeta.width; i++)
            {
                slots[k, door.x, door.z].GetWall(door.doorMeta.orientation).gO = doorObject;
                slots[k, door.x, door.z].SetWallObject(door.doorMeta, doorObject);
            }
        }

        playerSpawnPos = slots[1, door.x, door.z].transform.position;
        playerSpawnRot = Quaternion.Euler(0, door.doorMeta.orientation == Direction.Up ? 180 : door.doorMeta.orientation == Direction.Down ? 0 :
            door.doorMeta.orientation == Direction.Left ? 90 : door.doorMeta.orientation == Direction.Right ? 270 : 0, 0);
        return true;
    }
}
