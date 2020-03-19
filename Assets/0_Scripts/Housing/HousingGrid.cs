using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingGrid : MonoBehaviour
{
    #region  --- Variables --- 

    [HideInInspector]
    public HousingHouseData myHouseMeta;
    HousingSlot[,,] slots;// (level,row,column)
    public Transform furnituresParent;
    [HideInInspector]
    public HousingGridCoordinates currentSlotCoord;
    public HousingFurniture currentFurniture = null;
    public HousingFurniture highlightedFurniture = null;
    public HousingEditModeCameraController cameraCont = null;
    EditCameraDirection oldCamDir;

    public bool stickToWall;
    public int width;
    public int depth;
    public int height;
    GameObject slotPrefab;
    GameObject wallPrefab;
    Vector3 housePos = Vector3.zero;
    Material[] highlightedSlotMat;
    public Vector3 worldCenter;
    public Vector3 minSlotWorldPos;
    public Vector3 maxSlotWorldPos;

    Vector3 CalculateWorldPositions()
    {
        Vector3 result = Vector3.zero;
        if (myHouseMeta.validHouseSpacing)
        {
            Vector3 min = housePos;
            min.x += (MasterManager.HousingSettings.slotSize * myHouseMeta.houseSpace.minX);
            min.z += (MasterManager.HousingSettings.slotSize * myHouseMeta.houseSpace.minZ);
            min.y += (MasterManager.HousingSettings.slotSize * myHouseMeta.houseSpace.minY);
            Vector3 max = housePos;
            max.x += (MasterManager.HousingSettings.slotSize * myHouseMeta.houseSpace.maxX);
            max.z -= (MasterManager.HousingSettings.slotSize * myHouseMeta.houseSpace.maxZ);
            max.y += (MasterManager.HousingSettings.slotSize * myHouseMeta.houseSpace.maxY);
            result = VectorMath.MiddlePoint(min, max);
            minSlotWorldPos = min;
            maxSlotWorldPos = max;
            Debug.Log("HousingGrid -> HousingGrid: center = " + result.ToString("F4") + "; min = " + min.ToString("F4") + "; max = " + max.ToString("F4") + "; housePos = " + housePos.ToString("F4"));
        }
        return result;
    }

    HousingGridCoordinates center;
    List<CloseCoordinates> closeCoords = new List<CloseCoordinates>();

    bool IsCoordValid(HousingGridCoordinates coord)
    {
        return coord.y >= 0 && coord.y < height && coord.x >= 0 && coord.x < width && coord.z >= 0 && coord.z < depth;
    }

    #endregion

    #region --- MonoBehaviour Functions ---
    public void KonoAwake(HousingHouseData _myHouseMeta, Transform _furnituresParent, GameObject _wallPrefab,
        Vector3 _housePos, Material[] _highlightedSlotMat, HousingEditModeCameraController _camCont)
    {
        transform.name = "HousingGrid";
        myHouseMeta = _myHouseMeta;
        housePos = _housePos;//DON'T CHANGE ORDER OF THIS
        worldCenter = CalculateWorldPositions();//DON'T CHANGE ORDER OF THIS

        cameraCont = _camCont;
        oldCamDir = _camCont.currentCameraDir;
        furnituresParent = _furnituresParent;

        width = myHouseMeta.width;
        depth = myHouseMeta.depth;
        height = myHouseMeta.height;

        slots = new HousingSlot[height, depth, width];

        slotPrefab = MasterManager.HousingSettings.slotPrefab;
        wallPrefab = _wallPrefab;
        Debug.Log("housePos = " + housePos.ToString("F4"));

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

    public void KonoUpdate()
    {
        if (oldCamDir != cameraCont.currentCameraDir)
        {
            oldCamDir = cameraCont.currentCameraDir;
            ReSelectSlot();
        }
    }

    #endregion

    #region --- Create & Setup house ---

    public void CreateGrid(bool showSlotMeshes = false)
    {
        if (myHouseMeta == null) return;

        float slotSize = MasterManager.HousingSettings.slotSize;

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
                        bool floor = k - 1 < 0 || (k - 1 >= 0 && !myHouseMeta.houseSpace.houseLevels[k - 1].houseLevelRows[i].row[j]);
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

        float wallDist = (MasterManager.HousingSettings.slotSize / 2) + (wallPrefab.transform.lossyScale.z / 2)/*GetComponent<Collider>().bounds.extents.z*/;
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
                        SetUpWall(wallObject, MasterManager.HousingSettings.slotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //RIGHT
                    if (j + 1 >= houseLevels[k].houseLevelRows[i].row.Length || (j + 1 < houseLevels[k].houseLevelRows[i].row.Length && !houseLevels[k].houseLevelRows[i].row[j + 1]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x + wallDist, parentSlot.transform.position.y, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 90, 0), null);
                        wallObject.name = "RightWall";
                        SetUpWall(wallObject, MasterManager.HousingSettings.slotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //FRONT
                    if (i - 1 < 0 || (i - 1 >= 0 && !houseLevels[k].houseLevelRows[i - 1].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y, parentSlot.transform.position.z + wallDist);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 0, 0), null);
                        wallObject.name = "FrontWall";
                        SetUpWall(wallObject, MasterManager.HousingSettings.slotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //BACK
                    if (i + 1 >= houseLevels[k].houseLevelRows.Length || (i + 1 < houseLevels[k].houseLevelRows.Length && !houseLevels[k].houseLevelRows[i + 1].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y, parentSlot.transform.position.z - wallDist);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(0, 180, 0), null);
                        wallObject.name = "BackWall";
                        SetUpWall(wallObject, MasterManager.HousingSettings.slotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //Floor
                    if (k - 1 < 0 || (k - 1 >= 0 && !houseLevels[k - 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y - wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(90, 0, 0), null);
                        wallObject.name = "FloorTile";
                        if (k == 0)
                            wallObject.tag = "HousingFloor";
                        SetUpWall(wallObject, MasterManager.HousingSettings.slotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
                    }

                    //Ceiling
                    if (k + 1 >= houseLevels.Length || (k + 1 < houseLevels.Length && !houseLevels[k + 1].houseLevelRows[i].row[j]))
                    {
                        Transform parentSlot = slots[k, i, j].transform;
                        Vector3 wallPos = new Vector3(parentSlot.transform.position.x, parentSlot.transform.position.y + wallDist, parentSlot.transform.position.z);
                        GameObject wallObject = Instantiate(wallPrefab, wallPos, Quaternion.Euler(-90, 0, 0), null);
                        wallObject.name = "CeilingTile";
                        SetUpWall(wallObject, MasterManager.HousingSettings.slotSize, wallPrefab.transform.localScale.z, parentSlot, _showCollisionWalls, wallMats);
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
            Debug.LogError("HousingGrid -> Can't Spawn door at " + doorCoord.printString);
            return false;
        }
        // PLACE DOOR FURNITURE
        if (!PlaceFurniture(true))
        {
            Debug.LogError("HousingGrid -> Can't Place door");
            return false;
        }

        playerSpawnPos = slots[1, door.z, door.x].transform.position;
        playerSpawnRot = Quaternion.Euler(0, door.doorMeta.orientation == Direction.Up ? 180 : door.doorMeta.orientation == Direction.Down ? 0 :
            door.doorMeta.orientation == Direction.Left ? 90 : door.doorMeta.orientation == Direction.Right ? 270 : 0, 0);

        result = true;

        return result;
    }

    #endregion

    #region --- Spawn Furniture ---

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
        Debug.LogWarning("START SPAWN FURNITURE AT: spawnCoord = " + spawnCoord.printString);
        bool result = false;
        _furniture = null;
        //We try to spawn it in the middle:
        if (_furnitureMeta.widthOrient > width || _furnitureMeta.depthOrient > depth || _furnitureMeta.height > height) return false;

        GameObject furnitureObject = Instantiate(_furnitureMeta.prefab, Vector3.zero, Quaternion.identity, furnituresParent);
        furnitureObject.AddComponent(typeof(HousingFurniture));
        HousingFurniture auxFurniture = furnitureObject.GetComponent<HousingFurniture>();
        auxFurniture.KonoAwake(_furnitureMeta, this);
        _furniture = auxFurniture;

        bool validLevelFound = false;
        int furnitureMaxY = 0;
        for (int k = spawnCoord.y; k < height && !validLevelFound && furnitureMaxY < height; k++)
        {
            HousingGridCoordinates currentLevelCoord = new HousingGridCoordinates(k, spawnCoord.z, spawnCoord.x);
            furnitureMaxY = (currentLevelCoord.y + _furniture.height - 1);
            if (furnitureMaxY >= height) continue;

            _furniture.ChangePos(currentLevelCoord);

            if (CanFurnitureFit(_furniture))
            {
                //Move furniture's gameobject correctly
                PositionFurniture(_furniture);
                validLevelFound = true;
            }
            else Debug.Log("HousingGrid -> SpawnFurnitureAt: Can't fit furniture at " + _furniture.currentAnchorGridPos.printString);
        }
        Debug.Log("HousingGrid -> SpawnFurnitureAt: TEST TEST TEST");
        if (!validLevelFound)
        {
            Debug.LogError("HousingGrid -> SpawnFurnitureAt: Can't spawn furniture at " + spawnCoord.printString);
            Destroy(furnitureObject);
            return false;
        }
        else
        {
            Debug.Log("HousingGrid -> SpawnFurnitureAt: VALID LEVEL FOUND!");
            //Select furniture
            currentFurniture = _furniture;
            result = SelectSlotAt(_furniture.currentAnchorGridPos);
            if (!result)
            {
                currentFurniture = null;
                Debug.LogError("HousingGrid -> SpawnFurnitureAt: Can't select the slot " + _furniture.currentAnchorGridPos.printString + " with the furniture " + _furniture.name);
                return false;
            }
        }

        return result;
    }

    void PositionFurniture(HousingFurniture _furniture)
    {
        Debug.LogWarning("START POSITION FURNITURE: _furniture = " + _furniture.name);
        Vector3 worldPos = GetSlotAt(_furniture.currentAnchorGridPos).transform.position;
        if (_furniture.furnitureMeta.furnitureType == FurnitureType.Wall)
        {
            worldPos += _furniture.currentOrientation == Direction.Up ? Vector3.forward * MasterManager.HousingSettings.slotSize / 2 :
                _furniture.currentOrientation == Direction.Down ? -Vector3.forward * MasterManager.HousingSettings.slotSize / 2 :
                _furniture.currentOrientation == Direction.Right ? Vector3.right * MasterManager.HousingSettings.slotSize / 2 :
                _furniture.currentOrientation == Direction.Left ? -Vector3.right * MasterManager.HousingSettings.slotSize / 2 : Vector3.zero;
        }
        _furniture.transform.position = worldPos;

        for (int i = 0; i < _furniture.smallFurnitureOn.Count; i++)
        {
            HousingFurniture auxFurn = _furniture.smallFurnitureOn[i];
            worldPos = GetSlotAt(auxFurn.currentAnchorGridPos).transform.position;
            if (auxFurn.furnitureMeta.furnitureType == FurnitureType.Wall)
            {
                worldPos += auxFurn.currentOrientation == Direction.Up ? Vector3.forward * MasterManager.HousingSettings.slotSize / 2 :
                    auxFurn.currentOrientation == Direction.Down ? -Vector3.forward * MasterManager.HousingSettings.slotSize / 2 :
                    auxFurn.currentOrientation == Direction.Right ? Vector3.right * MasterManager.HousingSettings.slotSize / 2 :
                    auxFurn.currentOrientation == Direction.Left ? -Vector3.right * MasterManager.HousingSettings.slotSize / 2 : Vector3.zero;
            }
            Debug.Log("HousingGrid -> PositionFurniture: Moving smallFurnitureOn[" + i + "]");
            auxFurn.transform.position = worldPos;
        }
    }

    bool CanFurnitureFit(HousingFurniture _furniture)
    {
        Debug.LogWarning("START CAN FURNITURE FIT");
        bool result = false;
        bool baseFound = false;//if at least 1 slot that is on level 0 and on a "floor" is found, this becomes true.
        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> CanFurnitureFit: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    bool auxBaseFound;
                    bool validSlot = CanFurnitureSlotFit(_furniture, currentCheckCoord, k == 0, out auxBaseFound);
                    if (!baseFound && auxBaseFound) baseFound = true;

                    if (!validSlot)
                    {
                        Debug.LogError("HousingGrid -> CanFurnitureFit: Can't place furniture " + _furniture.furnitureMeta.name + " with anchor at " + _furniture.currentAnchorGridPos.printString +
    "; conflict at " + currentCheckCoord.printString + "; k = " + k + "; i = " + i + "; j = " + j);
                        return false;
                    }
                    else
                    {
                        Debug.Log("HousingGrid -> CanFurnitureFit: found good selectable slot at " + currentCheckCoord.printString);
                    }
                }
            }
        }

        if (_furniture.furnitureMeta.furnitureType == FurnitureType.Floor || _furniture.furnitureMeta.furnitureType == FurnitureType.Floor_Small)
        {
            if (!baseFound)
            {
                Debug.LogError("HousingGrid -> CanFurnitureFit: not even 1 base found");
                return false;
            }
        }

        #region --- furniture on ---
        for (int m = 0; m < _furniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = _furniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> CanFurnitureFit: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        bool validSlot = CanFurnitureSlotFit(auxFurn, currentCheckCoord, k == 0, out baseFound);

                        if (!validSlot)
                        {
                            Debug.LogError("HousingGrid -> CanFurnitureFit: Can't place furniture " + auxFurn.furnitureMeta.name + " with anchor at " + auxFurn.currentAnchorGridPos.printString +
        "; conflict at " + currentCheckCoord.printString + "; k = " + k + "; i = " + i + "; j = " + j);
                            return false;
                        }
                        else
                        {
                            Debug.Log("HousingGrid -> CanFurnitureFit: found good selectable slot at " + currentCheckCoord.printString);
                        }
                    }
                }
            }
        }
        #endregion

        result = true;

        return result;
    }

    bool CanFurnitureSlotFit(HousingFurniture _furniture, HousingGridCoordinates coord, bool furnitureBase, out bool baseFound)
    {
        bool validSlot = false;
        baseFound = false;
        if (!IsCoordValid(coord))
        {
            Debug.LogError("HousingGrid -> CanFurnitureFit: " + coord.printString + " is not a valid grid coordinate");
            return false;
        }
        HousingSlot slot = GetSlotAt(coord);
        switch (_furniture.furnitureMeta.furnitureType)
        {
            case FurnitureType.Floor:
                if (slot != null && !slot.hasFurniture)
                {
                    validSlot = true;
                    if (furnitureBase && !baseFound && (OnFloor(coord) || OnTopOfPile(coord)))
                    {
                        baseFound = true;
                    }
                }
                break;
            case FurnitureType.Floor_Small:
                if (slot != null && !slot.hasFurniture)
                {
                    validSlot = true;
                    if (furnitureBase && !baseFound && (OnFloor(coord) || OnTopOfPile(coord)))
                    {
                        baseFound = true;
                    }
                }
                break;
            case FurnitureType.Wall:
                if (slot != null && ((furnitureBase && (OnFloor(coord) || slot.hasAnyWall)) || (!furnitureBase)))
                {
                    validSlot = true;
                }
                break;
        }

        return validSlot;
    }

    #endregion

    #region --- Pick & Place ---
    public bool PickOrPlace()
    {
        if (currentFurniture != null)
        {
            return PlaceFurniture();
        }
        else if (highlightedFurniture != null)
        {
            return PickFurniture();
        }
        return false;
    }

    #region -- Place Furniture --

    public bool PlaceFurniture(bool _defaultFurniture = false)
    {
        Debug.LogWarning("START PLACE FURNITURE");
        bool result = false;
        if (currentFurniture == null)
        {
            Debug.LogError("HousingGrid -> Can't place furniture because currentFurniture == null.");
            return false;
        }

        if (currentFurniture.furnitureMeta.furnitureType == FurnitureType.Wall) result = PlaceWallFurniture();
        else
        {
            //Check if valid position
            result = CheckIfFurnitureCanBePlaced(currentFurniture);
            if (!result)
            {
                Debug.LogError("HousingGrid -> PlaceFurniture : Can't place furniture here because 1 or more slots are not valid.");
                return false;
            }

            for (int k = 0; k < currentFurniture.height; k++)//for every furniture level
            {
                for (int i = 0; i < currentFurniture.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < currentFurniture.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = currentFurniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> PlaceFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                        Debug.Log("HousingGrid -> PlaceFurniture: Going to set furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
                        if (!currentSlot.SetFurniture(currentFurniture))
                        {
                            Debug.LogError("HousingGrid -> PlaceFurniture: can't set furniture, This should NOT be happening because we already checked before");
                            return false;
                        }
                    }
                }
            }

            for (int m = 0; m < currentFurniture.smallFurnitureOn.Count; m++)
            {
                HousingFurniture auxFurn = currentFurniture.smallFurnitureOn[m];
                for (int k = 0; k < auxFurn.height; k++)//for every furniture level
                {
                    for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                    {
                        for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                        {
                            bool val;
                            HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                            if (!val)
                            {
                                Debug.LogError("HousingGrid -> PlaceFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                                continue;
                            }
                            HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                            Debug.Log("HousingGrid -> PlaceFurniture: Going to set furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
                            if (!currentSlot.SetFurniture(auxFurn))
                            {
                                Debug.LogError("HousingGrid -> PlaceFurniture: can't set furniture, This should NOT be happening because we already checked before");
                                return false;
                            }
                        }
                    }
                }
            }

            result = true;
            //Check for furnitureBase
            SetFurnitureBase(currentFurniture);
        }
        if (result)
        {
            StopHighlightPickedFurniture(currentSlotCoord);
            currentFurniture.defaultFurniture = _defaultFurniture;
            currentFurniture = null;
            SelectSlotAt(currentSlotCoord);
        }

        return result;
    }

    public bool PlaceFurniture(HousingFurniture _furniture)
    {
        Debug.LogWarning("START PLACE FURNITURE ");
        bool result = false;
        if (_furniture == null)
        {
            Debug.LogError("HousingGrid -> Can't place furniture because _furniture == null.");
            return false;
        }

        if (currentFurniture.furnitureMeta.furnitureType == FurnitureType.Wall) Debug.LogError("TO DO");//result = PlaceWallFurniture();

        //Check if valid position
        result = CheckIfFurnitureCanBePlaced(_furniture);
        if (!result)
        {
            Debug.LogError("HousingGrid -> PlaceFurniture : Can't place furniture here because 1 or more slots are not valid.");
            return false;
        }

        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> PlaceFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    Debug.Log("HousingGrid -> PlaceFurniture: Going to set furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
                    if (!currentSlot.SetFurniture(_furniture))
                    {
                        Debug.LogError("HousingGrid -> PlaceFurniture: can't set furniture, This should NOT be happening because we already checked before");
                        return false;
                    }
                }
            }
        }
        result = true;

        if (result)
        {
            //Check for furnitureBase
            SetFurnitureBase(currentFurniture);
        }

        return result;
    }

    bool PlaceWallFurniture()
    {
        Debug.LogWarning("START PLACE WALL FURNITURE");
        bool result = false;
        if (currentFurniture == null)
        {
            Debug.LogError("HousingGrid -> PlaceWallFurniture: Can't place furniture because currentFurniture == null. This message should not happen because this function " +
                "is already encapsulated by PlaceFurniture, which already checks this.");
            return false;
        }

        //Check if valid position
        result = CheckIfFurnitureCanBePlaced(currentFurniture);

        if (!result)
        {
            Debug.LogError("HousingGrid -> PlaceWallFurniture : Can't place wall furniture here because 1 or more slots are not valid.");
            return false;
        }

        for (int k = 0; k < currentFurniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < currentFurniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < currentFurniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = currentFurniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> PlaceWallFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    Debug.Log("HousingGrid -> PlaceWallFurniture: Going to set furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
                    if (!currentSlot.SetFurniture(currentFurniture))
                    {
                        Debug.LogError("HousingGrid -> PlaceWallFurniture: can't set furniture, This should NOT be happening because we already checked before");
                        return false;
                    }
                }
            }
        }

        result = true;

        return result;
    }

    bool SetFurnitureBase(HousingFurniture _furniture)
    {
        bool baseSet = false;
        for (int i = 0; i < _furniture.depth && !baseSet; i++)//for every furniture row
        {
            for (int j = 0; j < _furniture.width && !baseSet; j++)//for every furniture slot
            {
                bool val;
                HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(0, i, j), out val);
                if (!val)
                {
                    Debug.LogWarning("HousingGrid -> PlaceFurniture: this local coord does not exist " + new HousingGridCoordinates(0, i, j).printString);
                }
                currentCheckCoord.y--;
                if (IsCoordValid(currentCheckCoord))
                {
                    HousingSlot underSlot = GetSlotAt(currentCheckCoord);
                    if (underSlot != null && underSlot.hasFurniture)
                    {
                        _furniture.furnitureBase = underSlot.myFurniture.furnitureBase != null ? underSlot.myFurniture.furnitureBase : underSlot.myFurniture;
                        _furniture.furnitureBase.smallFurnitureOn.Add(_furniture);
                        baseSet = true;
                    }

                }
            }
        }

        return baseSet;
    }

    bool CheckIfFurnitureCanBePlaced(HousingFurniture _furniture)
    {
        Debug.LogWarning("START CHECK IF FURNITURE CAN BE PLACED");
        bool result;

        HousingFurniture furnitureUnder = null;
        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> CheckIfFurnitureCanBePlaced: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    bool validSlot = CheckIfFurnitureSlotCanBePlaced(_furniture, currentCheckCoord, k == 0);
                    if (!validSlot)
                    {
                        Debug.LogError("HousingGrid -> CheckIfFurnitureCanBePlaced: Can't place furniture " + _furniture.furnitureMeta.name + " with anchor at " + _furniture.currentAnchorGridPos.printString +
                            "; conflict at " + currentCheckCoord.printString);
                        return false;
                    }
                    if (k == 0 && _furniture.furnitureMeta.furnitureType == FurnitureType.Floor_Small)
                    {
                        currentCheckCoord.y--;
                        if (IsCoordValid(currentCheckCoord))
                        {
                            HousingSlot underSlot = GetSlotAt(currentCheckCoord);
                            if (underSlot != null && underSlot.hasFurniture)
                            {
                                if (furnitureUnder == null)
                                    furnitureUnder = underSlot.myFurniture;
                                else if (furnitureUnder != underSlot.myFurniture)
                                {
                                    Debug.LogError("HousingGrid -> CheckIfFurnitureCanBePlaced: Can't place a furniture on more than 1 furniture at the same time!");
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int m = 0; m < _furniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = _furniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> CheckIfFurnitureCanBePlaced: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        bool validSlot = CheckIfFurnitureSlotCanBePlaced(auxFurn, currentCheckCoord, false);
                        if (!validSlot)
                        {
                            Debug.LogError("HousingGrid -> CheckIfFurnitureCanBePlaced: Can't place furniture " + auxFurn.furnitureMeta.name + " with anchor at " + auxFurn.currentAnchorGridPos.printString +
                                "; conflict at " + currentCheckCoord.printString);
                            return false;
                        }
                    }
                }
            }
        }
        result = true;

        return result;
    }

    bool CheckIfFurnitureSlotCanBePlaced(HousingFurniture _furniture, HousingGridCoordinates coord, bool furnitureBase)
    {
        Debug.LogWarning("START CHECK IF FURNITURE SLOT CAN BE PLACED: coord = " + coord.printString);
        bool result = false;
        if (IsCoordValid(coord))
        {
            HousingSlot currentSlot = GetSlotAt(coord);
            if (currentSlot != null)
            {
                HousingSlot underSlot = null;
                if (coord.y - 1 >= 0) underSlot = GetSlotAt(new HousingGridCoordinates(coord.y - 1, coord.z, coord.x));

                switch (_furniture.furnitureMeta.furnitureType)
                {
                    case FurnitureType.Floor:
                        if (currentSlot.free && ((furnitureBase && OnFloor(coord)) || !furnitureBase))
                            result = true;
                        break;
                    case FurnitureType.Floor_Small:
                        if (currentSlot.free && ((furnitureBase && (OnFloor(coord) || OnTopOfPile(coord))) || !furnitureBase))
                            result = true;
                        break;
                    case FurnitureType.Wall:
                        if (currentSlot.CanPlaceWallFurniture(_furniture))
                            result = true;
                        break;
                }
            }
            if (!result)
            {
                Debug.LogError("HousingGrid -> CheckIfFurnitureSlotCanBePlaced: Can't place furniture Slot of " + _furniture.furnitureMeta.name + "  at " + coord.printString);
                return false;
            }
        }
        return result;
    }

    #endregion

    #region -- Pick Furniture --

    public bool PickFurniture()
    {
        Debug.LogWarning("START PICK FURNITURE");
        bool result = false;
        if (currentFurniture != null || highlightedFurniture == null)
        {
            Debug.LogError("HousingGrid -> PickFurniture: Can't pick furniture because currentFurniture != null or highlightedFurniture == null.");
            return false;
        }
        if (highlightedFurniture.defaultFurniture)
        {
            Debug.LogError("HousingGrid -> PickFurniture: Can't pick furniture because it is a default furniture");
            return false;
        }
        //if (highlightedFurniture.smallFurnitureOn.Count > 0)
        //{
        //    Debug.LogError("PickFurniture: Can't pick furniture because there is a small furniture on it.");
        //    return false;
        //}
        Debug.Log("highlightedFurniture = " + highlightedFurniture.name);
        for (int k = 0; k < highlightedFurniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < highlightedFurniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < highlightedFurniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = highlightedFurniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> PickFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    if (!currentSlot.UnSetFurniture(highlightedFurniture))
                    {
                        Debug.LogError("HousingGrid -> PickFurniture: can't UnSet furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
                        return false;
                    }
                }
            }
        }

        for (int m = 0; m < highlightedFurniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = highlightedFurniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> PickFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                        if (!currentSlot.UnSetFurniture(auxFurn))
                        {
                            Debug.LogError("HousingGrid -> PickFurniture: can't UnSet furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
                            return false;
                        }
                    }
                }
            }
        }
        currentFurniture = highlightedFurniture;
        currentSlotCoord = currentFurniture.currentAnchorGridPos;
        if (!HighlightPickedFurniture())
        {
            Debug.LogError("HousingGrid -> PickFurniture: Can't highlight furniture");
            return false;
        }

        if (!HighlightSlot(currentSlotCoord, 2))
        {
            Debug.LogError("HousingGrid -> PickFurniture: Can't highlight anchor's slot");
            return false;
        }
        if (currentFurniture.furnitureBase != null)
        {
            currentFurniture.furnitureBase.smallFurnitureOn.Remove(currentFurniture);
            currentFurniture.furnitureBase = null;
        }

        result = true;

        return result;
    }

    //public bool DropFurniture(HousingFurniture _furniture)
    //{
    //    if (_furniture.furnitureMeta.furnitureType != FurnitureType.Floor_Small)
    //    {
    //        Debug.LogError("DropFurniture: This furniture(" + _furniture.furnitureMeta.furnitureName + ") is not of type " + FurnitureType.Floor_Small);
    //        return false;
    //    }

    //    //UnSet furniture
    //    for (int k = 0; k < _furniture.height; k++)//for every furniture level
    //    {
    //        for (int i = 0; i < _furniture.depth; i++)//for every furniture row
    //        {
    //            for (int j = 0; j < _furniture.width; j++)//for every furniture slot
    //            {
    //                bool val;
    //                HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
    //                if (!val)
    //                {
    //                    Debug.LogError("DropFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
    //                    continue;
    //                }
    //                HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
    //                if (!currentSlot.UnSetFurniture(_furniture))
    //                {
    //                    Debug.LogError("DropFurniture: can't UnSet furniture at " + currentCheckCoord.printString + "; furniture's k = " + k + "; i = " + i + "; j = " + j);
    //                    return false;
    //                }
    //            }
    //        }
    //    }

    //    bool foundValid = false;
    //    HousingGridCoordinates coord = _furniture.currentAnchorGridPos;
    //    for (int k = coord.y; k >= 0 && !foundValid; k--)//Check downwards
    //    {
    //        HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
    //        _furniture.currentAnchorGridPos = checkCoord;
    //        if (CanFurnitureFit(_furniture))
    //        {
    //            //stickToWall = GetSlotAt(checkCoord).hasAnyWall;
    //            coord.y = k;
    //            foundValid = true;
    //            Debug.Log("DropFurniture: Found good Selectable slot at " + coord.printString);
    //        }
    //    }

    //    if (!foundValid)
    //    {
    //        Debug.LogError("DropFurniture: couldn't find valid slots to select with furniture");
    //        return false;
    //    }
    //    //Move furniture's gameobject correctly
    //    PositionFurniture(_furniture);
    //    PlaceFurniture(_furniture);


    //    return true;
    //}

    #endregion

    #endregion

    #region --- Highlight Slots ---

    bool HighlightSlotFull(HousingGridCoordinates coord)//0 == good; 1 == bad; 2 == pointer; 3 == placed
    {
        Debug.LogWarning("START HIGHLIGHT SLOT FULL");
        HousingSlot slot = GetSlotAt(coord);
        if (slot == null)
        {
            Debug.LogError("HousingGrid -> HighlightSlot: Invalid slot to highlight");
            return false;
        }

        if (currentFurniture == null)
        {
            if (slot.hasAnyFurniture)
            {
                //HIGHLIGHT FURNITURE
                if (GetSlotAt(coord).hasFurniture)
                {
                    if (!HighlightPlacedFurniture(slot.myFurniture))
                    {
                        Debug.Log("HousingGrid -> Can't highlight furniture");
                        return false;
                    }
                }
                else if (slot.hasAnyWallFurniture)
                {
                    if (slot.myWallFurnitures[(int)cameraCont.currentCameraDir] != null && slot.myWallFurnitures[(int)cameraCont.currentCameraDir].furnitureMeta != null)
                    {
                        if (!HighlightPlacedFurniture(slot.myWallFurnitures[(int)cameraCont.currentCameraDir]))
                        {
                            Debug.Log("HousingGrid -> Can't highlight wall furniture");
                            return false;
                        }
                    }
                    else
                    {
                        bool wallFurnitureFound = false;
                        for (int i = 0; i < slot.myWallFurnitures.Length && !wallFurnitureFound; i++)
                        {
                            if (slot.myWallFurnitures[i] != null && slot.myWallFurnitures[i].furnitureMeta != null)
                            {
                                wallFurnitureFound = true;
                                if (!HighlightPlacedFurniture(slot.myWallFurnitures[i]))
                                {
                                    Debug.Log("HousingGrid -> Can't highlight wall furniture");
                                    return false;
                                }
                            }
                        }
                        if (!wallFurnitureFound) Debug.LogError("HousingGrid -> Can't find the wall furniture that SHOULD exist. THIS SHOULD NOT BE HAPPENING");
                    }
                }
            }
            if (!HighlightSlot(coord, 2))
            {
                Debug.LogError("HousingGrid -> Couldn't highlight slot " + coord.printString);
                return false;
            }
        }
        else
        {
            Debug.LogError("HousingGrid -> HighlightSlotFull: TO DO");
        }

        return true;
    }

    bool HighlightSlot(HousingGridCoordinates coord, int state)//0 == good; 1 == bad; 2 == pointer; 3 == placed
    {
        if (state == 2) Debug.LogWarning("START HIGHLIGHT SLOT for POINTER");
        HousingSlot slot = GetSlotAt(coord);
        if (slot == null)
        {
            Debug.LogError("HousingGrid -> HighlightSlot: Invalid slot to highlight");
            return false;
        }
        if (state == 2 && currentFurniture != null)
        {
            slot.anchorObject.SetActive(true);
        }
        else
        {
            MeshRenderer meshR = slot.GetComponent<MeshRenderer>();
            if (meshR == null) return false;
            meshR.enabled = true;
            meshR.material = highlightedSlotMat[state];
        }
        return true;
    }

    public bool StopHighlightSlot(HousingGridCoordinates coord)
    {
        Debug.Log("START STOP HIGHLIGHT SLOT: at " + coord.printString);
        HousingSlot slot = GetSlotAt(coord);
        MeshRenderer meshR = slot.GetComponent<MeshRenderer>();
        if (meshR == null) return false;
        meshR.enabled = false;
        if (slot.anchorObject.activeInHierarchy) slot.anchorObject.SetActive(false);

        return true;
    }

    bool HighlightPlacedFurniture(HousingFurniture _furniture)
    {
        Debug.LogWarning("START HIGHLIGHT PLACED FURNITURE");

        bool result = false;
        for (int k = 0; k < _furniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < _furniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < _furniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HighlightPlacedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                    Debug.Log("HighlightPlacedFurniture: highlighting furniture slot " + currentCheckCoord.printString);
                    if (currentSlot.HasThisFurniture(_furniture))
                    {
                        if (!HighlightSlot(currentCheckCoord, 3))
                        {
                            Debug.LogError("HighlightPlacedFurniture: Can't highlight the slot " + currentCheckCoord.printString);
                            return false;
                        }
                    }
                }
            }
        }

        for (int m = 0; m < _furniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = _furniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HighlightPlacedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        HousingSlot currentSlot = GetSlotAt(currentCheckCoord);
                        Debug.Log("HighlightPlacedFurniture: highlighting furniture slot " + currentCheckCoord.printString);
                        if (currentSlot.HasThisFurniture(auxFurn))
                        {
                            if (!HighlightSlot(currentCheckCoord, 3))
                            {
                                Debug.LogError("HighlightPlacedFurniture: Can't highlight the slot " + currentCheckCoord.printString);
                                return false;
                            }
                        }
                    }
                }
            }
        }

        result = true;

        if (result) highlightedFurniture = _furniture;

        return result;
    }

    public bool StopHighlightPlacedFurniture()
    {
        Debug.LogWarning("START STOP PLACED HIGHLIGHT FURNITURE");
        bool result = false;
        if (highlightedFurniture == null)
        {
            Debug.LogWarning("HousingGrid -> StopHighlightPlacedFurniture: can't stop hightlighting this furniture.");
            return false;
        }
        for (int k = 0; k < highlightedFurniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < highlightedFurniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < highlightedFurniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = highlightedFurniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> StopHighlightPlacedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    HousingSlot slot = GetSlotAt(currentCheckCoord);
                    if (slot != null && slot.HasThisFurniture(highlightedFurniture))
                    {
                        if (!StopHighlightSlot(currentCheckCoord))
                        {
                            Debug.LogWarning("HousingGrid -> StopHighlightPlacedFurniture: Can't Highlight Furniture at slot " + currentCheckCoord.printString);
                            return false;
                        }
                    }
                }
            }
        }
        for (int m = 0; m < highlightedFurniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = highlightedFurniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> StopHighlightPlacedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        HousingSlot slot = GetSlotAt(currentCheckCoord);
                        if (slot != null && slot.HasThisFurniture(auxFurn))
                        {
                            if (!StopHighlightSlot(currentCheckCoord))
                            {
                                Debug.LogWarning("HousingGrid -> StopHighlightPlacedFurniture: Can't Highlight Furniture at slot " + currentCheckCoord.printString);
                                return false;
                            }
                        }
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

    bool HighlightPickedFurniture()
    {
        Debug.LogWarning("START HIGHLIGHT PICKED FURNITURE");
        bool result = false;
        if (currentFurniture == null)
        {
            Debug.LogError("HousingGrid -> HighlightPickedFurniture: there is not picked furniture");
            return false;
        }
        Debug.Log("currentFurniture = " + currentFurniture.name);

        HousingFurniture furnitureUnder = null;
        for (int k = 0; k < currentFurniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < currentFurniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < currentFurniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = currentFurniture.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> HighlightPickedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    bool validSlot = CheckIfFurnitureSlotCanBePlaced(currentFurniture, currentCheckCoord, k == 0);

                    #region - Check if only 1 furniture under -
                    if (validSlot && k == 0)
                    {
                        HousingGridCoordinates underCoord = currentCheckCoord;
                        underCoord.y--;
                        if (IsCoordValid(underCoord))
                        {
                            HousingSlot underSlot = GetSlotAt(underCoord);
                            if (underSlot != null && underSlot.hasFurniture)
                            {
                                if (furnitureUnder == null)
                                    furnitureUnder = underSlot.myFurniture;
                                else if (furnitureUnder != underSlot.myFurniture)
                                {
                                    Debug.LogWarning("HousingGrid -> HighlightPickedFurniture: Can't place a furniture on more than furniture at the same time!");
                                    validSlot = false;
                                }
                            }
                        }
                    }
                    #endregion

                    if (validSlot) Debug.Log("HousingGrid -> HighlightPickedFurniture:  Found a good placeable slot at " + currentCheckCoord.printString);
                    int value = validSlot ? 0 : 1;
                    if (!HighlightSlot(currentCheckCoord, value))
                    {
                        Debug.LogError("HousingGrid -> HighlightPickedFurniture: Can't highlight the slot " + currentCheckCoord.printString);
                        return false;
                    }
                }
            }
        }

        for (int m = 0; m < currentFurniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = currentFurniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetGridCoord(new HousingGridCoordinates(k, i, j), out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> HighlightPickedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        bool validSlot = CheckIfFurnitureSlotCanBePlaced(auxFurn, currentCheckCoord, false);

                        #region - Check if only 1 furniture under -
                        //if (validSlot && k == 0)
                        //{
                        //    HousingGridCoordinates underCoord = currentCheckCoord;
                        //    underCoord.y--;
                        //    if (IsCoordValid(underCoord))
                        //    {
                        //        HousingSlot underSlot = GetSlotAt(underCoord);
                        //        if (underSlot != null && underSlot.hasFurniture)
                        //        {
                        //            if (furnitureUnder == null)
                        //                furnitureUnder = underSlot.myFurniture;
                        //            else if (furnitureUnder != underSlot.myFurniture)
                        //            {
                        //                Debug.LogWarning("HighlightPickedFurniture: Can't place a furniture on more than furniture at the same time!");
                        //                validSlot = false;
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion

                        if (validSlot) Debug.Log("HousingGrid -> HighlightPickedFurniture:  Found a good placeable slot at " + currentCheckCoord.printString);
                        int value = validSlot ? 0 : 1;
                        if (!HighlightSlot(currentCheckCoord, value))
                        {
                            Debug.LogError("HousingGrid -> HighlightPickedFurniture: Can't highlight the slot " + currentCheckCoord.printString);
                            return false;
                        }
                    }
                }
            }
        }
        result = true;

        if (result) highlightedFurniture = currentFurniture;

        return result;
    }

    public bool StopHighlightPickedFurniture(HousingGridCoordinates oldAnchorCoord)
    {
        Debug.LogWarning("START STOP HIGHLIGHT PICKED FURNITURE");
        bool result = false;
        if (currentFurniture == null)
        {
            Debug.LogWarning("HousingGrid -> StopHighlightPickedFurniture: can't stop hightlighting this furniture.");
            return false;
        }
        if (highlightedFurniture == null)
        {
            Debug.LogWarning("HousingGrid -> StopHighlightPickedFurniture: can't stop hightlighting this furniture.");
            return false;
        }

        bool turnedClockwise = false, turnedCounterClockwise = false;
        if (highlightedFurniture.turnedClockwise)
        {
            highlightedFurniture.RotateCounterClockwise();
            turnedCounterClockwise = true;
        }
        else if (highlightedFurniture.turnedCounterClockwise)
        {
            highlightedFurniture.RotateClockwise();
            turnedClockwise = true;
        }

        for (int k = 0; k < highlightedFurniture.height; k++)//for every furniture level
        {
            for (int i = 0; i < highlightedFurniture.depth; i++)//for every furniture row
            {
                for (int j = 0; j < highlightedFurniture.width; j++)//for every furniture slot
                {
                    bool val;
                    HousingGridCoordinates currentCheckCoord = highlightedFurniture.GetGridCoord(new HousingGridCoordinates(k, i, j), oldAnchorCoord, out val);
                    if (!val)
                    {
                        Debug.LogError("HousingGrid -> StopHighlightPickedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                        continue;
                    }
                    HousingSlot slot = GetSlotAt(currentCheckCoord);
                    Debug.Log("HousingGrid -> StopHighlightPickedFurniture: k = " + k + "; i = " + i + "; j = " + j + "; oldAnchorCoord = " + oldAnchorCoord.printString);
                    if (slot != null)
                    {
                        if (!StopHighlightSlot(currentCheckCoord))
                        {
                            Debug.LogWarning("HousingGrid -> StopHighlightPickedFurniture: Can't Stop Highlight Furniture at slot " + currentCheckCoord.printString);
                            return false;
                        }
                    }
                    else
                    {
                        Debug.LogError("HousingGrid -> StopHighLightPickedFurniture: This should not be happening. This means we had the furniture in a non existing slot in the last movement!");
                    }
                }
            }
        }

        for (int m = 0; m < highlightedFurniture.smallFurnitureOn.Count; m++)
        {
            HousingFurniture auxFurn = highlightedFurniture.smallFurnitureOn[m];
            for (int k = 0; k < auxFurn.height; k++)//for every furniture level
            {
                for (int i = 0; i < auxFurn.depth; i++)//for every furniture row
                {
                    for (int j = 0; j < auxFurn.width; j++)//for every furniture slot
                    {
                        bool val;
                        HousingGridCoordinates currentCheckCoord = auxFurn.GetPiledFurnitureGridCoord(new HousingGridCoordinates(k, i, j), oldAnchorCoord, out val);
                        if (!val)
                        {
                            Debug.LogError("HousingGrid -> StopHighlightPickedFurniture: this local coord does not exist " + new HousingGridCoordinates(k, i, j).printString);
                            continue;
                        }
                        HousingSlot slot = GetSlotAt(currentCheckCoord);
                        Debug.Log("HousingGrid -> StopHighlightPickedFurniture: k = " + k + "; i = " + i + "; j = " + j + "; oldAnchorCoord = " + oldAnchorCoord.printString);
                        if (slot != null)
                        {
                            if (!StopHighlightSlot(currentCheckCoord))
                            {
                                Debug.LogWarning("HousingGrid -> StopHighlightPickedFurniture: Can't Stop Highlight Furniture at slot " + currentCheckCoord.printString);
                                return false;
                            }
                        }
                        else
                        {
                            Debug.LogError("HousingGrid -> StopHighLightPickedFurniture: This should not be happening. This means we had the furniture in a non existing slot in the last movement!");
                        }
                    }
                }
            }
        }
        result = true;

        if (result)
        {
            if (turnedCounterClockwise) highlightedFurniture.RotateClockwise();
            else if (turnedClockwise) highlightedFurniture.RotateCounterClockwise();
            highlightedFurniture = null;
        }
        return result;
    }

    #endregion

    #region --- Move & Select Slot ---

    bool OnFloor(HousingGridCoordinates coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError("HousingGrid -> OnFloor -> Invalid Coords: " + coord.printString);
            return false;
        }
        HousingSlot slot = GetSlotAt(coord);
        HousingSlot downSlot = null;
        if (coord.y - 1 >= 0) downSlot = GetSlotAt(new HousingGridCoordinates(coord.y - 1, coord.z, coord.x));
        return slot != null && (coord.y - 1 < 0 || downSlot == null);
    }

    bool InPile(HousingGridCoordinates coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError("HousingGrid -> InPile -> Invalid Coords: " + coord.printString);
            return false;
        }
        HousingSlot slot = GetSlotAt(coord);
        HousingSlot downSlot = null;
        if (coord.y - 1 >= 0) downSlot = GetSlotAt(new HousingGridCoordinates(coord.y - 1, coord.z, coord.x));
        HousingSlot upSlot = null;
        if (coord.y + 1 < height) upSlot = GetSlotAt(new HousingGridCoordinates(coord.y + 1, coord.z, coord.x));

        return slot != null && (slot.hasFurniture || slot.free) && ((downSlot != null && downSlot.hasFurniture) || (upSlot != null && upSlot.hasFurniture));
    }

    bool TopOfPile(HousingGridCoordinates coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError("HousingGrid -> TopOfPile -> Invalid Coords: " + coord.printString);
            return false;
        }
        HousingSlot slot = GetSlotAt(coord);
        HousingSlot upSlot = null;
        if (coord.y + 1 < height) upSlot = GetSlotAt(new HousingGridCoordinates(coord.y + 1, coord.z, coord.x));

        return slot != null && slot.hasFurniture && (coord.y + 1 >= height || upSlot == null || (upSlot != null && !upSlot.hasFurniture));
    }

    bool OnTopOfPile(HousingGridCoordinates coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError("HousingGrid -> TopOfPile -> Invalid Coords: " + coord.printString);
            return false;
        }
        HousingSlot slot = GetSlotAt(coord);

        HousingGridCoordinates downCoord = coord;
        downCoord.y--;


        return downCoord.y >= 0 && slot != null && slot.free && TopOfPile(downCoord);
    }

    bool CanSelectSlot(HousingGridCoordinates coord)
    {
        if (coord.y < 0 || coord.y >= height)
        {
            Debug.LogError("HousingGrid -> CanSelectSlot -> Invalid Coords: " + coord.printString);
            return false;
        }

        return (OnFloor(coord) && !InPile(coord)) || TopOfPile(coord);
    }

    bool CanSelectWallSlot(HousingGridCoordinates coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError("HousingGrid -> CanSelectWallSlot -> Invalid Coords: " + coord.printString);
            return false;
        }
        HousingSlot slot = GetSlotAt(coord);

        return slot != null && (slot.hasAnyWall) && !slot.hasFurniture;
    }

    public bool SelectSlotAt(HousingGridCoordinates coord)
    {
        Debug.LogWarning("START SELECT SLOT AT");
        bool result = false;
        bool foundValid = false;
        #region --- No Furniture Picked ---
        if (currentFurniture == null)
        {
            #region Slot Search and Check in same column
            if (stickToWall && CanSelectWallSlot(coord))
            {
                foundValid = true;
            }
            else
            {
                for (int k = coord.y; k < height && !foundValid; k++)//Check upwards
                {
                    HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
                    if (CanSelectSlot(checkCoord))
                    {
                        stickToWall = GetSlotAt(checkCoord).hasAnyWall;
                        coord.y = k;
                        foundValid = true;
                        Debug.Log("HousingGrid -> Found good Selectable slot at " + coord.printString);
                    }
                }
                if (!foundValid)
                {
                    for (int k = coord.y - 1; k >= 0 && !foundValid; k--)//Check downwards
                    {
                        HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
                        if (CanSelectSlot(checkCoord))
                        {
                            stickToWall = GetSlotAt(checkCoord).hasAnyWall;
                            coord.y = k;
                            foundValid = true;
                            Debug.Log("HousingGrid -> Found good Selectable slot at " + coord.printString);
                        }
                    }
                }
            }

            #endregion

            if (!foundValid)
            {
                Debug.LogError("HousingGrid -> SelectSlotAt: couldn't find a valid slot to select");
                return false;
            }

            //STOP HIGHLIGHT FURNITURE
            StopHighlightSlot(currentSlotCoord);
            StopHighlightPlacedFurniture();

            result = HighlightSlotFull(coord);
            if (result) currentSlotCoord = coord;
            else
            {
                Debug.LogError("HousingGrid -> SelectSlotAt: Can't highlight slot full");
                return false;
            }
        }
        #endregion
        #region --- With Furniture Picked ---
        else// select a slot while having a furniture picked. Always move anchor and then check furniture around it
        {
            Debug.Log("currentFurniture = " + currentFurniture.name);
            HousingGridCoordinates oldAnchorPos = currentFurniture.currentAnchorGridPos;
            for (int k = coord.y; k < height && !foundValid; k++)//Check upwards
            {
                HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
                currentFurniture.ChangePos(checkCoord);
                if (CanFurnitureFit(currentFurniture))
                {
                    //stickToWall = GetSlotAt(checkCoord).hasAnyWall;
                    coord.y = k;
                    foundValid = true;
                    Debug.Log("HousingGrid -> Found good Selectable slot at anchor " + coord.printString);
                }
            }
            if (!foundValid)
            {
                for (int k = coord.y - 1; k >= 0 && !foundValid; k--)//Check downwards
                {
                    HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
                    currentFurniture.ChangePos(checkCoord);
                    if (CanFurnitureFit(currentFurniture))
                    {
                        //stickToWall = GetSlotAt(checkCoord).hasAnyWall;
                        coord.y = k;
                        foundValid = true;
                        Debug.Log("HousingGrid -> Found good Selectable slot at " + coord.printString);
                    }
                }
            }
            if (!foundValid)
            {
                currentFurniture.ChangePos(oldAnchorPos);
                Debug.LogError("HousingGrid -> SelectSlotAt: couldn't find valid slots to select with furniture");
                return false;
            }

            //STOP HIGHLIGHT FURNITURE
            StopHighlightSlot(currentSlotCoord);//I think it's not necessary
            StopHighlightPickedFurniture(currentSlotCoord);

            if (HighlightPickedFurniture()) result = HighlightSlot(coord, 2);
            else
            {
                Debug.LogError("HousingGrid -> SelectSlotAt: Can't highlight picked furniture");
                return false;
            }

            if (result)
            {
                currentSlotCoord = coord;
                //Move furniture's gameobject correctly
                PositionFurniture(currentFurniture);
                //currentFurniture.transform.position = GetSlotAt(currentSlotCoord).transform.position;
            }
            else
            {
                Debug.LogError("HousingGrid -> SelectSlotAt: Can't highlight anchor's slot");
                return false;
            }
        }
        #endregion

        return result;
    }

    public bool SelectWallSlotAt(HousingGridCoordinates coord, bool goingDown = false)
    {
        bool result = false;
        bool foundValid = false;
        #region --- No Furniture Picked ---
        if (currentFurniture == null)
        {
            #region Slot Search and Check in same column
            if (!goingDown)
            {
                Debug.Log("HousingGrid -> SelectWallSlotAt: going up");
                for (int k = coord.y; k < height && !foundValid; k++)
                {
                    if (CanSelectWallSlot(new HousingGridCoordinates(k, coord.z, coord.x)))
                    {
                        stickToWall = true;
                        coord.y = k;
                        foundValid = true;
                    }
                    else if (CanSelectSlot(new HousingGridCoordinates(k, coord.z, coord.x)))
                    {
                        stickToWall = false;
                        coord.y = k;
                        foundValid = true;
                    }
                }
            }
            else
            {
                Debug.Log("HousingGrid -> SelectWallSlotAt: going down");
                for (int k = coord.y; k >= 0 && !foundValid; k--)
                {
                    if (CanSelectWallSlot(new HousingGridCoordinates(k, coord.z, coord.x)) || CanSelectSlot(new HousingGridCoordinates(k, coord.z, coord.x)))
                    {
                        coord.y = k;
                        foundValid = true;
                    }
                }
            }
            #endregion

            if (!foundValid)
            {
                Debug.LogError("HousingGrid -> SelectWallSlotAt: Couldn't find valid slot going " + (goingDown ? "down" : "up"));
                return false;
            }

            //STOP HIGHLIGHT FURNITURE
            StopHighlightSlot(currentSlotCoord);
            StopHighlightPlacedFurniture();

            result = HighlightSlotFull(coord);
            if (result) currentSlotCoord = coord;
            else
            {
                Debug.LogError("HousingGrid ->SelectSlotAt: Can't highlight slot full");
                return false;
            }
        }
        #endregion
        #region --- With Furniture Picked ---
        else// select a slot while having a furniture picked
        {
            HousingGridCoordinates oldAnchorPos = currentFurniture.currentAnchorGridPos;
            #region Slot Search and Check in same column
            if (!goingDown)
            {
                Debug.Log("HousingGrid -> SelectWallSlotAt: going up");
                for (int k = coord.y; k < height && !foundValid; k++)
                {
                    HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
                    currentFurniture.ChangePos(checkCoord);
                    if (CanFurnitureFit(currentFurniture))
                    {
                        stickToWall = true;
                        coord.y = k;
                        foundValid = true;
                    }
                    //else if (CanSelectSlot(new HousingGridCoordinates(k, coord.z, coord.x)))
                    //{
                    //    stickToWall = false;
                    //    coord.y = k;
                    //    foundValid = true;
                    //}
                }
            }
            else
            {
                Debug.Log("HousingGrid -> SelectWallSlotAt: going down");
                for (int k = coord.y; k >= 0 && !foundValid; k--)
                {
                    HousingGridCoordinates checkCoord = new HousingGridCoordinates(k, coord.z, coord.x);
                    currentFurniture.ChangePos(checkCoord);
                    if (CanFurnitureFit(currentFurniture))
                    {
                        coord.y = k;
                        foundValid = true;
                    }
                }
            }
            #endregion

            if (!foundValid)
            {
                currentFurniture.ChangePos(oldAnchorPos);
                Debug.LogError("HousingGrid -> SelectSlotAt: couldn't find valid slots to select with furniture");
                return false;
            }

            //STOP HIGHLIGHT FURNITURE
            StopHighlightSlot(currentSlotCoord);//I think it's not necessary
            StopHighlightPickedFurniture(currentSlotCoord);

            if (HighlightPickedFurniture()) result = HighlightSlot(coord, 2);
            else
            {
                Debug.LogError("HousingGrid -> SelectSlotAt: Can't highlight picked furniture");
                return false;
            }

            if (result)
            {
                currentSlotCoord = coord;
                //Move furniture's gameobject correctly
                PositionFurniture(currentFurniture);
                //currentFurniture.transform.position = GetSlotAt(currentSlotCoord).transform.position;
            }
            else
            {
                Debug.LogError("HousingGrid -> SelectSlotAt: Can't highlight anchor's slot");
                return false;
            }
        }
        #endregion
        return result;
    }

    public bool ReSelectSlot()
    {
        if (currentFurniture != null) return false;
        StopHighlightPlacedFurniture();
        return HighlightSlotFull(currentSlotCoord); ;
    }

    public bool MoveSelectSlot(Direction inputDir)
    {
        Debug.LogWarning("START MOVE SELECT SLOT: inputDir = " + inputDir);
        bool result = false;
        bool oldStickToWall = stickToWall;
        HousingGridCoordinates oldCoord;
        HousingGridCoordinates newCoord = oldCoord = currentSlotCoord;

        if (inputDir == Direction.Down && stickToWall)
        {
            result = MoveSelectDown();
        }

        Direction realDir = inputDir;
        realDir += (int)cameraCont.currentCameraDir;//magic stuff
        realDir += (int)realDir > 3 ? -4 : (int)realDir < 0 ? +4 : 0;
        if (!result)
        {
            switch (realDir)
            {
                case Direction.Left:
                    newCoord.x = currentSlotCoord.x - 1;
                    if (newCoord.x >= 0)
                        result = SelectSlotAt(newCoord);
                    //Debug.Log("newCoord = " + newCoord.printString + "; result = " + result);
                    break;
                case Direction.Right:
                    newCoord.x = currentSlotCoord.x + 1;
                    if (newCoord.x < width)
                        result = SelectSlotAt(newCoord);
                    break;
                case Direction.Up:
                    newCoord.z = currentSlotCoord.z - 1;
                    if (newCoord.z >= 0)
                        result = SelectSlotAt(newCoord);
                    break;
                case Direction.Down:
                    newCoord.z = currentSlotCoord.z + 1;
                    if (newCoord.z < depth)
                        result = SelectSlotAt(newCoord);
                    break;
            }
        }

        if (!result)
        {
            HousingSlot slot = GetSlotAt(oldCoord);
            int frontWall = (int)realDir + 1; frontWall += (int)frontWall > 3 ? -4 : 0;
            int backWall = (int)realDir - 1; backWall += (int)backWall < 0 ? +4 : 0;
            if (((inputDir == Direction.Left || inputDir == Direction.Right) && (slot.hasWalls[frontWall] || slot.hasWalls[backWall])) || inputDir == Direction.Down)
            {
                //do nothing
            }
            else
            {
                result = MoveSelectUp();
            }
        }

        return result;
    }

    public bool MoveSelectSlotSkipWall(Direction dir)
    {
        bool result = false;
        HousingGridCoordinates oldCoord;
        HousingGridCoordinates newCoord = oldCoord = currentSlotCoord;
        dir += (int)cameraCont.currentCameraDir;//magic stuff
        dir += (int)dir > 3 ? -4 : (int)dir < 0 ? +4 : 0;

        switch (dir)
        {
            case Direction.Left:
                for (int i = currentSlotCoord.x - 1; i >= 0 && !result; i--)
                {
                    newCoord.x = i;
                    result = SelectSlotAt(newCoord);
                    //Debug.Log("newCoord = " + newCoord.printString + "; result = " + result);
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

    public bool MoveSelectUp()
    {
        bool result = false;
        if (currentSlotCoord.y + 1 >= height) return false;

        HousingGridCoordinates oldCoord = currentSlotCoord;
        HousingGridCoordinates coord = new HousingGridCoordinates(oldCoord.y + 1, oldCoord.z, oldCoord.x);
        result = SelectWallSlotAt(coord, false);
        if (!result)
        {
            Debug.LogError("HousingGrid ->Can't MoveSelectUp because we can't select the slot " + coord.printString);
            return false;
        }

        return result;
    }

    public bool MoveSelectDown()
    {
        bool result = false;
        if (currentSlotCoord.y - 1 < 0) return false;

        HousingGridCoordinates oldCoord = currentSlotCoord;
        HousingGridCoordinates coord = new HousingGridCoordinates(oldCoord.y - 1, oldCoord.z, oldCoord.x);
        result = SelectWallSlotAt(coord, true);
        if (!result)
        {
            Debug.LogError("HousingGrid ->Can't MoveSelectDown because we can't select the slot " + coord.printString);
            return false;
        }

        return result;
    }

    #endregion

    #region --- Rotate Furniture ---

    public bool RotateFurniture(bool clockwise)
    {
        Debug.LogWarning("START ROTATE FURNITURE " + (clockwise ? "clockwise" : "counterClockwise"));
        bool result = false;
        if (currentFurniture != null)
        {
            Debug.Log("BEFORE");
            currentFurniture.PrintSpaces();

            FurnitureRotationData furnRotData = new FurnitureRotationData(currentFurniture);

            if (clockwise) currentFurniture.RotateClockwise(true);
            else currentFurniture.RotateCounterClockwise(true);
            Debug.Log("AFTER");
            currentFurniture.PrintSpaces();

            if (!SelectSlotAt(currentSlotCoord))
            {
                Debug.LogError("HousingGrid -> RotateFurniture: can't Select furniture at " + currentSlotCoord.printString);
                currentFurniture.Copy(furnRotData);
                currentFurniture.ResetRotationBools();//not necessary I think
                currentFurniture.PrintSpaces();
                return false;
            }

            //Rotate actual gameObject
            currentFurniture.transform.localRotation = Quaternion.Euler(0, currentFurniture.transform.localRotation.eulerAngles.y + (90 * (clockwise ? 1 : -1)), 0);
            for (int i = 0; i < currentFurniture.smallFurnitureOn.Count; i++)
            {
                HousingFurniture auxFurn = currentFurniture.smallFurnitureOn[i];
                auxFurn.transform.localRotation = Quaternion.Euler(0, auxFurn.transform.localRotation.eulerAngles.y + (90 * (clockwise ? 1 : -1)), 0);
            }
            result = true;
        }
        return result;
    }

    #endregion

    #region -- Get & Set --

    public HousingSlot GetSlotAt(HousingGridCoordinates coord)
    {
        if (coord.y < 0 || coord.y >= slots.GetLength(0) || coord.z < 0 || coord.z >= slots.GetLength(1) ||
            coord.x < 0 || coord.x >= slots.GetLength(2)) Debug.LogError("HousingGrid ->Can't get slot at " + coord.printString);
        return slots[coord.y, coord.z, coord.x];
    }

    public bool lookingAtLargeFurniture
    {
        get
        {
            HousingFurniture lookAtFurn = currentFurniture != null ? currentFurniture : highlightedFurniture != null ? highlightedFurniture : null;
            if (lookAtFurn != null)
            {
                Vector3 min = GetSlotAt(lookAtFurn.min).transform.position;
                Vector3 max = GetSlotAt(lookAtFurn.max).transform.position;
                float diameter = (max - min).magnitude;
                return diameter > 4;
            }
            else return false;
        }
    }

    public Vector3 GetCameraLookPosition(EditCameraMode camMode = EditCameraMode.FollowSelection)
    {
        HousingFurniture lookAtFurn = currentFurniture != null ? currentFurniture : highlightedFurniture != null ? highlightedFurniture : null;
        if (lookAtFurn == null) return GetSlotAt(currentSlotCoord).transform.position;
        else 
        {
            Vector3 min = camMode == EditCameraMode.FollowSelection? GetSlotAt(lookAtFurn.minFull).transform.position :
                GetSlotAt(lookAtFurn.min).transform.position;
            Vector3 max = camMode == EditCameraMode.FollowSelection ? GetSlotAt(lookAtFurn.maxFull).transform.position :
                GetSlotAt(lookAtFurn.max).transform.position;
            return VectorMath.MiddlePoint(min, max);
        }
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
