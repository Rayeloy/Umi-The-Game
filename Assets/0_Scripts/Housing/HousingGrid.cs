using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingGrid : MonoBehaviour
{
    [HideInInspector]
    public HousingHouseData myHouseMeta;
    HousingSlot[,,] slots;// (level,row,column)
    public Transform furnituresParent;
    [HideInInspector]
    public HousingGridCoordinates currentSlotCoord;
    public HousingFurniture currentFurniture = null;
    public HousingFurniture highlightedFurniture = null;
    public HousingEditModeCameraController cameraCont= null;


    public int width;
    public int depth;
    public int height;
    GameObject slotPrefab;
    GameObject wallPrefab;
    Vector3 housePos = Vector3.zero;
    Material[] highlightedSlotMat;
    public Vector3 worldCenter;

    Vector3 CalculateWorldCenter()
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
            max.z -= (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.maxZ);
            max.y += (myHouseMeta.housingSlotSize * myHouseMeta.houseSpace.maxY);
            result = VectorMath.MiddlePoint(min, max);
            Debug.Log("HousingGrid: center = " + result.ToString("F4") + "; min = " + min.ToString("F4") + "; max = " + max.ToString("F4"));
        }
        return result;
    }

    HousingGridCoordinates center;
    List<CloseCoordinates> closeCoords = new List<CloseCoordinates>();

    public void KonoAwake(HousingHouseData _myHouseMeta, Transform _furnituresParent, GameObject _slotPrefab, GameObject _wallPrefab,
        Vector3 _housePos, Material[] _highlightedSlotMat, HousingEditModeCameraController _camCont)
    {
        myHouseMeta = _myHouseMeta;
        worldCenter = CalculateWorldCenter();

        cameraCont = _camCont;
        furnituresParent = _furnituresParent;

        width = myHouseMeta.width;
        depth = myHouseMeta.depth;
        height = myHouseMeta.height;

        slots = new HousingSlot[height, depth, width];

        slotPrefab = _slotPrefab;
        wallPrefab = _wallPrefab;
        housePos = _housePos;

        highlightedSlotMat = _highlightedSlotMat;
        currentSlotCoord = new HousingGridCoordinates();
        currentFurniture = null;

        center = new HousingGridCoordinates(0, depth / 2, width / 2);
        //In case cant spawn at center
        closeCoords = new List<CloseCoordinates>();
        for (int i = 0; i < depth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int dist = Mathf.Abs(center.z - i) + Mathf.Abs(center.x - j);
                if (dist > 0)
                {
                    CloseCoordinates auxCloseCoord = new CloseCoordinates(new HousingGridCoordinates(0, i, j), dist);
                    closeCoords.Add(auxCloseCoord);
                }
            }
        }
    }

    #region --- Create & Setup house ---

    public void CreateGrid(bool showSlotMeshes = false)
    {
        if (myHouseMeta == null) return;

        float slotSize = myHouseMeta.housingSlotSize;

        //fill up the slots
        for (int k = 0; k < slots.GetLength(0); k++)//for every level
        {
            //Instantiate level's empty parent
            Vector3 newLevelParentPos = new Vector3(housePos.x, housePos.y + (slotSize * k), housePos.z);
            Transform newLevelParent = new GameObject("Level " + (k)).transform;
            newLevelParent.position = newLevelParentPos; newLevelParent.rotation = Quaternion.identity; newLevelParent.parent = transform;
            for (int i = 0; i < slots.GetLength(1); i++)//for every row
            {
                //Instantiate row's empty parent
                Vector3 newRowParentPos = new Vector3(newLevelParentPos.x, newLevelParentPos.y, newLevelParentPos.z - (slotSize * i));
                Transform newRowParent = new GameObject("Row " + (i)).transform;
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
                        wallLeft = j == 0 || ((j - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j - 1]);
                        //check for wall right
                        wallRight = j == width - 1 || ((j + 1) < width && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i].row[j + 1]);
                        //check for wall Up
                        wallUp = i == 0 || ((i - 1) > 0 && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i - 1].row[j]);
                        //check for wall Down
                        wallDown = i == depth - 1 || ((i + 1) < depth && !myHouseMeta.houseSpace.houseLevels[k].houseLevelRows[i + 1].row[j]);

                        bool wall = wallLeft || wallRight || wallUp || wallDown;
                        slotType = wall ? (floor ? HousingSlotType.WallAndFloor : HousingSlotType.Wall) : floor ? HousingSlotType.Floor : HousingSlotType.None;
                        //Debug.Log("wall = " + wall + "; floor = " + floor);
                        #endregion
                        //Debug.Log("Housing: Initializing slot "+ coord.printString + " with slotType = " +slotType);
                        //Initialize slot
                        newSlot.KonoAwake(coord, slotType, wallUp, wallRight, wallDown, wallLeft);

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

    public void CreateWalls(bool _showCollisionWalls, Material[] wallMats)
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
                        SetUpWall(wallObject, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //RIGHT
                    if (j + 1 >= houseLevels[k].houseLevelRows[i].row.Length || (j + 1 < houseLevels[k].houseLevelRows[i].row.Length && !houseLevels[k].houseLevelRows[i].row[j + 1]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x + wallDist, parentSlot.transform.position.y, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), null);
                        wallObject.name = "RightWall";
                        SetUpWall(wallObject, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //FRONT
                    if (i - 1 < 0 || (i - 1 >= 0 && !houseLevels[k].houseLevelRows[i - 1].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y, parentSlot.transform.position.z + wallDist);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 0, 0), null);
                        wallObject.name = "FrontWall";
                        SetUpWall(wallObject, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //BACK
                    if (i + 1 >= houseLevels[k].houseLevelRows.Length || (i + 1 < houseLevels[k].houseLevelRows.Length && !houseLevels[k].houseLevelRows[i + 1].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y, parentSlot.transform.position.z - wallDist);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 180, 0), null);
                        wallObject.name = "BackWall";
                        SetUpWall(wallObject, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //Floor
                    if (k - 1 < 0 || (k - 1 >= 0 && !houseLevels[k - 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y - wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(90, 0, 0), null);
                        wallObject.name = "FloorTile";
                        if(k==0)
                        wallObject.tag = "HousingFloor";
                        SetUpWall(wallObject, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //Ceiling
                    if (k + 1 >= houseLevels.Length || (k + 1 < houseLevels.Length && !houseLevels[k + 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y + wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(-90, 0, 0), null);
                        wallObject.name = "CeilingTile";
                        SetUpWall(wallObject, myHouseMeta.housingSlotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }
                }
            }
        }
    }

    void SetUpWall(GameObject wallObject, float wallSize, float wallThickness, Transform _parent, bool _showWall, Material[] _wallMats)
    {
        wallObject.transform.localScale = new Vector3(wallSize, wallSize, wallThickness);
        wallObject.transform.parent = _parent;
        MeshRenderer meshR = wallObject.GetComponent<MeshRenderer>();
        if (meshR != null && !_showWall) meshR.material = _wallMats[1];
        //meshR.enabled = _showCollisionWalls;
    }

    /// <summary>
    /// Spawns door and sets it to the correct positions and rotation. Returns the player's spawn position and spawn rotation
    /// </summary>
    public bool CreateDoor(out Vector3 playerSpawnPos, out Quaternion playerSpawnRot)
    {
        Debug.LogWarning("START CREATE DOOR");
        bool result = false;
        playerSpawnPos = Vector3.zero; playerSpawnRot = Quaternion.identity;

        HouseDoor door = myHouseMeta.door;
        HousingGridCoordinates doorCoord = new HousingGridCoordinates(0, door.z, door.x);
        HousingFurniture furniture;
        if (!SpawnFurnitureAt(door.doorMeta, doorCoord, out furniture))
        {
            Debug.LogError("Can't Spawn door at " + doorCoord.printString);
            return false;
        }
        // PLACE DOOR FURNITURE
        if (!PlaceFurniture(furniture))
        {
            Debug.LogError("Can't Place door");
            return false;
        }

        playerSpawnPos = slots[1, door.z, door.x].transform.position;
        playerSpawnRot = Quaternion.Euler(0, door.doorMeta.orientation == Direction.Up ? 180 : door.doorMeta.orientation == Direction.Down ? 0 :
            door.doorMeta.orientation == Direction.Left ? 90 : door.doorMeta.orientation == Direction.Right ? 270 : 0, 0);
        result = true;

        return result;
    }

    #endregion

    public bool SpawnFurniture(HousingFurnitureData _furnitureMeta, out HousingFurniture _furniture)
    {
        Debug.LogWarning("START SPAWN FURNITURE ");
        bool result = false;
        //In case we can't spawn at center, we get a list of close coordinates with distance levels, to check through it
        _furniture = null;
        result = SpawnFurnitureAt(_furnitureMeta, center, out _furniture);
        if (!result)//center is no good, we search for a good place to spawn, as near to center as possible
        {
            List<CloseCoordinates> closeCoordsCopy = closeCoords;
            int safetyCounter = 0;
            int currentDist = 1;
            while (closeCoordsCopy.Count > 0 && safetyCounter < 1000)
            {
                bool found = false;
                for (int i = 0; i < closeCoordsCopy.Count && !found; i++)
                {
                    if (closeCoordsCopy[i].distance == currentDist && SpawnFurnitureAt(_furnitureMeta, closeCoords[i].coord, out _furniture))
                    {
                        return true;
                    }
                    else
                    {
                        closeCoordsCopy.RemoveAt(i);
                        found = true;
                    }
                }
                safetyCounter++;
            }
        }

        return result;
    }

    /// <summary>
    /// Tries to spawn furniture at the given coordinates, if it can't it tries with a higher level until it reaches the ceiling
    /// </summary>
    /// <param name="furnitureMeta"></param>
    /// <param name="spawnCoord"></param>
    /// <returns></returns>
    public bool SpawnFurnitureAt(HousingFurnitureData _furnitureMeta, HousingGridCoordinates spawnCoord, out HousingFurniture _furniture)
    {
        Debug.LogWarning("START SPAWN FURNITURE AT");
        bool result = false;
        _furniture = null;
        //We try to spawn it in the middle:
        if (_furnitureMeta.widthOrient > width || _furnitureMeta.depthOrient > depth || _furnitureMeta.height > height) return false;

        GameObject furnitureObject = Instantiate(_furnitureMeta.prefab, Vector3.zero, Quaternion.identity, furnituresParent);
        furnitureObject.AddComponent(typeof(HousingFurniture));
        HousingFurniture auxFurniture = furnitureObject.GetComponent<HousingFurniture>();
        auxFurniture.KonoAwake(_furnitureMeta);
        _furniture = auxFurniture;
        _furniture.currentAnchorGridPos = spawnCoord;

        bool validLevelFound = false;
        for (int k = spawnCoord.y; k < height && !validLevelFound; k++)
        {
            HousingGridCoordinates currentLevelCoord = new HousingGridCoordinates(k, spawnCoord.z, spawnCoord.x);
            int furnitureMaxY = (currentLevelCoord.y + _furniture.height - 1);
            if (furnitureMaxY >= height) return false;

            if (CanFurnitureFit(_furniture))
            {
                //Move furniture's gameobject correctly
                Vector3 worldPos = GetSlotAt(_furniture.currentAnchorGridPos).transform.position;
                if(_furniture.furnitureMeta.furnitureType == FurnitureType.Wall)
                {
                    worldPos += _furniture.currentOrientation == Direction.Up ? Vector3.forward * myHouseMeta.housingSlotSize/2 :
                        _furniture.currentOrientation == Direction.Down ? -Vector3.forward * myHouseMeta.housingSlotSize/2 :
                        _furniture.currentOrientation == Direction.Right ? Vector3.right * myHouseMeta.housingSlotSize/2 :
                        _furniture.currentOrientation == Direction.Left ? -Vector3.right * myHouseMeta.housingSlotSize/2 : Vector3.zero;
                }
                _furniture.transform.position = worldPos;
                validLevelFound = true;
            }
            else continue;
        }
        if (!validLevelFound)
        {
            Destroy(furnitureObject);
            Debug.LogError("HousingGrid -> SpawnFurnitureAt: Can't spawn furniture at " + spawnCoord.printString);
            return false;
        }
        else
        {
            result = true;
        }

        return result;
    }

    bool CanFurnitureFit(HousingFurniture _furniture)
    {
        Debug.LogWarning("START CAN FURNITURE FIT");
        bool result = false;

        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j));
                    if (!(GetSlotAt(currentCheckCoord) != null && GetSlotAt(currentCheckCoord).free))
                    {
                        Debug.LogError("Can't place furniture " + _furniture.furnitureMeta.name + " with anchor at " + _furniture.currentAnchorGridPos.printString +
                            "; conflict at " + currentCheckCoord.printString+"; k = "+k+"; i = "+i+"; j = "+j);
                        return false;
                    }
                }
            }
        }
        result = true;

        return result;
    }

    public bool PlaceFurniture(HousingFurniture _furniture)
    {
        Debug.LogWarning("START PLACE FURNITURE");
        bool result = false;
        if (_furniture.furnitureMeta.furnitureType == FurnitureType.Wall) result = PlaceWallFurniture(_furniture);
        else
        {
            Debug.LogError("TO DO");
        }

        return result;
    }

    bool PlaceWallFurniture(HousingFurniture _furniture)
    {
        Debug.LogWarning("START PLACE WALL FURNITURE");
        bool result = false;

        //Check if valid position
        result = CheckIfCanBePlaced(_furniture);

        if (!result)
        {
            Debug.LogError("HousingGrid -> PlaceWallFurniture : Can't place wall furniture here because 1 or more slots are not valid.");
            return false;
        }

        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j));
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    Debug.Log("Going to set furniture at " + currentCheckCoord.printString + "; furniture's k = "+k+"; i = "+i+"; j = " +j);
                    if (!currentSlot.SetFurniture(_furniture))
                    {
                        Debug.LogError("can't set furniture, This should NOT be happening because we already checked before");
                        return false;
                    }
                }
            }
        }
        //DeSelect furniture

        result = true;
        #region old setFurniture
        //Transform slotTrans = slots[coord.y, coord.z, coord.x].transform;
        //GameObject furnitureObject = Instantiate(_furniture.furnitureMeta.prefab, Vector3.zero, Quaternion.identity, slotTrans);

        //Debug.Log("Furniture Data: Coord = " + coord.printString + "; height = " + furnitureMeta.height + "; depth = " + furnitureMeta.depthOrient + "; width = " + furnitureMeta.widthOrient);
        //for (int k = 0; k < furnitureMeta.height; k++)
        //{
        //    for (int i = 0; i < furnitureMeta.depthOrient; i++)
        //    {
        //        for (int j = 0; j < furnitureMeta.widthOrient; j++)
        //        {
        //            if (!slots[coord.y + k, coord.z + i, coord.x + j].SetFurniture(furnitureMeta, furnitureObject))
        //            {
        //                Debug.LogError("Can't place wall furniture at (" + (coord.y + k) + "," + (coord.z + i) + "," + (coord.x + j) + ") at orientation " + furnitureMeta.orientation);
        //                return false;
        //            }
        //        }
        //    }
        //}

        //float slotSize = myHouseMeta.housingSlotSize;
        //Vector3 slotPos = slotTrans.position;
        //Collider col = furnitureObject.GetComponentInChildren<Collider>();
        //if (col == null)
        //{
        //    Debug.LogError("HousingGrid -> SetWallFurniture -> Error: Can't find the collider of the furniture " + furnitureMeta.name + ".");
        //    //playerSpawnPos = Vector3.zero; playerSpawnRot = Quaternion.identity;
        //    return false;
        //}
        //float y = slotPos.y - (slotSize / 2) + col.bounds.extents.y;
        //float x = slotPos.x + (furnitureMeta.orientation == Direction.Left ? -slotSize / 2 : furnitureMeta.orientation == Direction.Right ? +slotSize / 2 : 0);
        //float z = slotPos.z + (furnitureMeta.orientation == Direction.Up ? +slotSize / 2 : furnitureMeta.orientation == Direction.Down ? -slotSize / 2 : 0);
        //Vector3 pos = new Vector3(x, y, z);
        //Quaternion rot = Quaternion.Euler(0, furnitureMeta.orientation == Direction.Up ? 0 : furnitureMeta.orientation == Direction.Down ? 180 :
        //    furnitureMeta.orientation == Direction.Left ? 270 : furnitureMeta.orientation == Direction.Right ? 90 : 0, 0);
        //furnitureObject.transform.position = pos;
        //furnitureObject.transform.rotation = rot;
        //result = true;

        //if (!result)
        //{
        //    //TO DO: erase furniture

        //}
        #endregion

        return result;
    }

    bool CheckIfCanBePlaced(HousingFurniture _furniture)
    {
        Debug.LogWarning("START CHECK IF CAN BE PLACED");
        bool result = false;

        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    bool validSlot = false;
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j));
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    if (currentSlot != null && currentSlot.free)
                    {
                        HousingSlot underSlot = null;
                        if (k-1 >= 0) underSlot = GetSlotAt(new HousingGridCoordinates(k - 1, i, j));

                        switch (_furniture.furnitureMeta.furnitureType)
                        {
                            case FurnitureType.Floor:
                                if (k - 1 >= 0 && underSlot == null)
                                    validSlot = true;
                                break;
                            case FurnitureType.Floor_Small:
                                if (k - 1 >= 0 && (underSlot == null || (underSlot != null && underSlot.canPlaceFurnitureOn)))
                                    validSlot = true;
                                break;
                            case FurnitureType.Wall:
                                if (currentSlot.CanPlaceWallFurniture(_furniture))
                                    validSlot = true;
                                break;
                        }
                    }
                    if (!validSlot)
                    {
                        Debug.LogError("Can't place furniture " + _furniture.furnitureMeta.name + " with anchor at " + _furniture.currentAnchorGridPos.printString + "; conflict at " + currentCheckCoord.printString);
                        result = false;
                    }
                    ////Highlight color depending on validSlot
                    //int value = validSlot ? 0 : 1;
                    //HighlightSlot(currentCheckCoord, value);
                }
            }
        }
        result = true;

        return result;
    }

    bool HighlightSlot(HousingGridCoordinates coord, int state)//0 == good; 1 == bad; 2 == nothing
    {
        HousingSlot slot = GetSlotAt(coord);
        MeshRenderer meshR = slot.GetComponent<MeshRenderer>();
        if (meshR == null) return false;
        meshR.enabled = true;
        meshR.material = state == 0 ? highlightedSlotMat[0] : state == 1 ? highlightedSlotMat[1] : highlightedSlotMat[2];
        Debug.Log("Highlight slot " + coord.printString);
        return true;
    }

    public bool StopHighlightSlot(HousingGridCoordinates coord)
    {
        HousingSlot slot = GetSlotAt(coord);
        MeshRenderer meshR = slot.GetComponent<MeshRenderer>();
        if (meshR == null) return false;
        meshR.enabled = false;
        Debug.Log("Stop Highlight slot " + coord.printString);
        return true;
    }

    bool HighlightFurniture(HousingFurniture _furniture)
    {
        bool result = false;
        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    bool validSlot = false;
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j));
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    if (currentSlot != null && currentSlot.free)
                    {
                        HousingSlot underSlot = null;
                        if (k - 1 >= 0) underSlot = GetSlotAt(new HousingGridCoordinates(k - 1, i, j));

                        switch (_furniture.furnitureMeta.furnitureType)
                        {
                            case FurnitureType.Floor:
                                if (k - 1 >= 0 && underSlot == null)
                                    validSlot = true;
                                break;
                            case FurnitureType.Floor_Small:
                                if (k - 1 >= 0 && (underSlot == null || (underSlot != null && underSlot.canPlaceFurnitureOn)))
                                    validSlot = true;
                                break;
                            case FurnitureType.Wall:
                                if (currentSlot.CanPlaceWallFurniture(_furniture))
                                    validSlot = true;
                                break;
                        }
                    }
                    //Highlight color depending on validSlot
                    int value = validSlot ? 0 : 1;
                    if(!HighlightSlot(currentCheckCoord, value))
                    {
                        Debug.LogError("Can't hightlight the slot "+ currentCheckCoord.printString);
                        return false;
                    }
                }
            }
        }
        result = true;

        if(result) highlightedFurniture = _furniture;

        return result;
    }

    bool StopHighlightFurniture(HousingFurniture _furniture)
    {
        bool result = false;

        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j));
                    if (!StopHighlightSlot(currentCheckCoord))
                    {
                        Debug.LogError("Can't Highlight Furniture at slot " + currentCheckCoord.printString);
                        return false;
                    }
                }
            }
        }
        result = true;

        if (result)
        {
            highlightedFurniture = null;
        }
        return result;
    }

    #region --- Selected Slot ---

    public bool SelectSlotAt(HousingGridCoordinates coord)
    {
        bool result = false;
        if (currentFurniture == null)
        {
            #region -- Check if we can select that slot --
            bool foundValid = false;
            for (int k = coord.y; k < height && !foundValid; k++)
            {
                if (slots[k, coord.z, coord.x] != null &&
                    (k - 1 < 0 || (k - 1 >= 0 && (slots[k - 1, coord.z, coord.x] == null || (slots[k - 1, coord.z, coord.x] != null && !slots[k - 1, coord.z, coord.x].free && !slots[k, coord.z, coord.x].free))))
                    && (k + 1 >= height || (k + 1 < height && (slots[k + 1, coord.z, coord.x] == null || (slots[k + 1, coord.z, coord.x] != null && slots[k + 1, coord.z, coord.x].free)))))
                {
                    coord.y = k;
                    foundValid = true;
                }
            }
            if (!foundValid)
            {
                for (int k = 0; k < coord.y && !foundValid; k++)
                {
                    if (slots[k, coord.z, coord.x] != null &&
                        (k - 1 < 0 || (k - 1 >= 0 && (slots[k - 1, coord.z, coord.x] == null || (slots[k - 1, coord.z, coord.x] != null && !slots[k - 1, coord.z, coord.x].free && !slots[k, coord.z, coord.x].free))))
                        && (k + 1 >= height || (k + 1 < height && (slots[k + 1, coord.z, coord.x] == null || (slots[k + 1, coord.z, coord.x] != null && slots[k + 1, coord.z, coord.x].free)))))
                    {
                        coord.y = k;
                        foundValid = true;
                    }
                }
            }
            if (!foundValid) return false;
            #endregion

            //STOP HIGHLIGHT FURNITURE
            if (highlightedFurniture!= null && !GetSlotAt(coord).HasThisFurniture(highlightedFurniture))
            {
                StopHighlightFurniture(highlightedFurniture);
            }

            #region -- Highlight furniture --
            //HIGHLIGHT FURNITURE
            if (GetSlotAt(coord).hasFurniture)
            {
                if (!HighlightFurniture(GetSlotAt(coord).myFurniture))
                {
                    Debug.Log("Can't highlight furniture");
                    return false;
                }
            }
            else if (GetSlotAt(coord).hasAnyWallFurniture)
            {
                HousingSlot slot = GetSlotAt(coord);
                if (slot.myWallFurnitures[(int)cameraCont.currentCameraDir] != null && slot.myWallFurnitures[(int)cameraCont.currentCameraDir].furnitureMeta != null)
                {
                    if (!HighlightFurniture(slot.myWallFurnitures[(int)cameraCont.currentCameraDir]))
                    {
                        Debug.Log("Can't highlight wall furniture");
                        return false;
                    }
                }
                else
                {
                    bool wallFurnitureFound = false;
                    for (int i = 0; i < slot.myWallFurnitures.Length; i++)
                    {
                        if(slot.myWallFurnitures[i]!=null && slot.myWallFurnitures[i].furnitureMeta != null)
                        {
                            wallFurnitureFound = true;
                            if (!HighlightFurniture(slot.myWallFurnitures[i]))
                            {
                                Debug.Log("Can't highlight wall furniture");
                                return false;
                            }
                        }
                    }
                    if (!wallFurnitureFound) Debug.LogError("Can't find the wall furniture that SHOULD exist. THIS SHOULD NOT BE HAPPENING");
                }
            }
            #endregion

            HighlightSlot(coord, 2);
            currentSlotCoord = coord;
            Debug.Log("Highlight slot " + coord.printString);
            result = true;
        }
        else// select a slot while having a furniture picked
        {
            Debug.LogError("TO DO");
        }

        return result;
    }

    public bool MoveSelectSlot(Direction dir)
    {
        bool result = false;
        HousingGridCoordinates oldCoord;
        HousingGridCoordinates newCoord = oldCoord = currentSlotCoord;
        Debug.Log(" dir = " + (int)dir + "; (int)editCamDir = " + (int)cameraCont.currentCameraDir);
        dir += (int)cameraCont.currentCameraDir;//magic stuff
        dir += (int)dir > 3 ? -4 : (int)dir < 0 ? +4 : 0;
        Debug.Log("dir after = " + (int)dir);

        switch (dir)
        {
            case Direction.Left:
                for (int i = currentSlotCoord.x - 1; i >= 0 && !result; i--)
                {
                    newCoord.x = i;
                    result = SelectSlotAt(newCoord);
                    Debug.Log("newCoord = " + newCoord.printString + "; result = " + result);
                }
                break;
            case Direction.Right:
                for (int i = currentSlotCoord.x + 1; i < width && !result; i++)
                {
                    newCoord.x = i;
                    result = SelectSlotAt(newCoord);
                }
                break;
            case Direction.Up:
                for (int j = currentSlotCoord.z - 1; j >= 0 && !result; j--)
                {
                    newCoord.z = j;
                    result = SelectSlotAt(newCoord);
                }
                break;
            case Direction.Down:
                for (int j = currentSlotCoord.z + 1; j < depth && !result; j++)
                {
                    newCoord.z = j;
                    result = SelectSlotAt(newCoord);
                }
                break;
        }
        if (result)
        {
            StopHighlightSlot(oldCoord);
        }

        return result;
    }

    //public bool HighlightSlot(HousingGridCoordinates coord)
    //{
    //    bool foundValid = false;
    //    for (int k = coord.y; k < height && !foundValid; k++)
    //    {
    //        if (slots[k, coord.z, coord.x] != null &&
    //            (k - 1 < 0 || (k - 1 >= 0 && (slots[k - 1, coord.z, coord.x] == null || (slots[k - 1, coord.z, coord.x] != null && !slots[k - 1, coord.z, coord.x].free))))
    //            && (k + 1 >= height || (k + 1 < height && (slots[k + 1, coord.z, coord.x] == null || (slots[k + 1, coord.z, coord.x] != null && slots[k + 1, coord.z, coord.x].free)))))
    //        {
    //            coord.y = k;
    //            foundValid = true;
    //        }
    //    }
    //    if (!foundValid)
    //    {
    //        for (int k = 0; k < coord.y && !foundValid; k++)
    //        {
    //            if (slots[k, coord.z, coord.x] != null &&
    //                (k - 1 < 0 || (k - 1 >= 0 && (slots[k - 1, coord.z, coord.x] == null || (slots[k - 1, coord.z, coord.x] != null && !slots[k - 1, coord.z, coord.x].free))))
    //                && (k + 1 >= height || (k + 1 < height && (slots[k + 1, coord.z, coord.x] == null || (slots[k + 1, coord.z, coord.x] != null && slots[k + 1, coord.z, coord.x].free)))))
    //            {
    //                coord.y = k;
    //                foundValid = true;
    //            }
    //        }
    //    }
    //    if (!foundValid) return false;

    //    currentSlotCoord = coord;
    //    bool result = false;
    //    MeshRenderer meshR = slots[coord.y, coord.z, coord.x].gameObject.GetComponent<MeshRenderer>();
    //    if (meshR == null) return false;
    //    meshR.enabled = true;
    //    meshR.material = slots[coord.y, coord.z, coord.x].free ? highlightedSlotMat[0] : highlightedSlotMat[1];
    //    result = true;
    //    Debug.Log("Highlight slot " + coord.printString);

    //    return result;
    //}

    //public void StopHighlightSlot(HousingGridCoordinates coord)
    //{
    //    if (slots[coord.y, coord.z, coord.x] != null)
    //        slots[coord.y, coord.z, coord.x].gameObject.GetComponent<MeshRenderer>().enabled = false;
    //}

    #endregion

    #region -- Get & Set --
    public HousingSlot GetSlotAt(HousingGridCoordinates coord)
    {
        if (coord.y < 0 || coord.y >= slots.GetLength(0) || coord.z < 0 || coord.z >= slots.GetLength(1) ||
            coord.x < 0 || coord.x >= slots.GetLength(2)) Debug.LogError("Can't get slot at " + coord.printString);
        return slots[coord.y, coord.z, coord.x];
    }

    #endregion

    struct CloseCoordinates
    {
        public int distance;
        public HousingGridCoordinates coord;

        public CloseCoordinates(HousingGridCoordinates _coord, int _dist)
        {
            coord = _coord;
            distance = _dist;
        }
    }
}
