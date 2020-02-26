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



    public void KonoAwake(HousingHouseData _myHouseMeta, Transform _furnituresParent, GameObject _slotPrefab, GameObject _wallPrefab, Vector3 _housePos, Material[] _highlightedSlotMat)
    {
        myHouseMeta = _myHouseMeta;
        CalculateWorldCenter();

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
        meshR.material = _showWall ? _wallMats[0] : _wallMats[1];
        //meshR.enabled = _showCollisionWalls;
    }

    /// <summary>
    /// Spawns door and sets it to the correct positions and rotation. Returns the player's spawn position and spawn rotation
    /// </summary>
    public bool CreateDoor(out Vector3 playerSpawnPos, out Quaternion playerSpawnRot)
    {
        bool result = false;
        playerSpawnPos = Vector3.zero; playerSpawnRot = Quaternion.identity;

        HouseDoor door = myHouseMeta.door;

        HousingFurniture furniture;
        if (SpawnFurnitureAt(door.doorMeta, new HousingGridCoordinates(0, door.z, door.x), out furniture))
        {
            // PLACE DOOR FURNITURE
            if (!PlaceWallFurniture(furniture))
            {
                Debug.LogError("Can't Place door");
                return false;
            }

            playerSpawnPos = slots[1, door.z, door.x].transform.position;
            playerSpawnRot = Quaternion.Euler(0, door.doorMeta.orientation == Direction.Up ? 180 : door.doorMeta.orientation == Direction.Down ? 0 :
                door.doorMeta.orientation == Direction.Left ? 90 : door.doorMeta.orientation == Direction.Right ? 270 : 0, 0);
            result = true;
        }

        return result;
    }

    #endregion

    public bool SpawnFurniture(HousingFurnitureData _furnitureMeta, out HousingFurniture _furniture)
    {
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
    public bool SpawnFurnitureAt(HousingFurnitureData furnitureMeta, HousingGridCoordinates spawnCoord, out HousingFurniture _furniture)
    {
        bool result = false;
        _furniture = null;
        //We try to spawn it in the middle:
        if (furnitureMeta.widthOrient > width || furnitureMeta.depthOrient > depth || furnitureMeta.height > height) return false;

        _furniture = new HousingFurniture(furnitureMeta);
        bool validLevelFound = false;
        for (int k = spawnCoord.y; k < height && !validLevelFound; k++)
        {
            HousingGridCoordinates currentLevelCoord = new HousingGridCoordinates(k, spawnCoord.z, spawnCoord.x);
            int furnitureMaxY = (currentLevelCoord.y + _furniture.height - 1);
            if (furnitureMaxY >= height) return false;

            if (CanFurnitureFit(_furniture, currentLevelCoord))
            {
                GameObject furnitureObject = Instantiate(_furniture.furnitureMeta.prefab, Vector3.zero, Quaternion.identity, furnituresParent);
                furnitureObject.AddComponent(typeof(HousingFurniture));
                HousingFurniture auxFurniture = furnitureObject.GetComponent<HousingFurniture>();
                auxFurniture.Copy(_furniture);
                _furniture = auxFurniture;
                validLevelFound = true;
            }
            else continue;
        }
        if (!validLevelFound)//We can spawn the object here, but we still need to check if we can place it there
        {
            Debug.LogError("HousingGrid -> SpawnFurnitureAt: Can't spawn furniture at " + spawnCoord.printString);
            return false;
        }

        return result;
    }

    public bool CanFurnitureFit(HousingFurniture _furniture, HousingGridCoordinates coord)
    {
        bool result = false;

        for (int k = 0; k < _furniture.height; k++)
        {
            for (int i = 0; i < _furniture.depth; i++)
            {
                for (int j = 0; j < _furniture.width; j++)
                {
                    HousingGridCoordinates currentCheckCoord = _furniture.GetGridCoord(new HousingGridCoordinates(k, i, j), coord);
                    if (!(GetSlotAt(currentCheckCoord) != null && GetSlotAt(currentCheckCoord).free))
                    {
                        Debug.LogError("Can't place furniture " + _furniture.furnitureMeta.name + " with anchor at " + coord.printString + "; conflict at " + currentCheckCoord.printString);
                        return false;
                    }
                }
            }
        }
        result = true;

        return result;
    }

    public bool  PlaceFurniture(HousingFurniture _furniture)
    {
        bool result = false;
        if (_furniture.furnitureMeta.furnitureType == FurnitureType.Wall) result = PlaceWallFurniture(_furniture);
        else
        {
            Debug.LogError("TO DO");
        }

        return result;
    }

    public bool PlaceWallFurniture(HousingFurniture _furniture)
    {
        bool result = false;

        //Check if valid position

        Transform slotTrans = slots[coord.y, coord.z, coord.x].transform;
        GameObject furnitureObject = Instantiate(_furniture.furnitureMeta.prefab, Vector3.zero, Quaternion.identity, slotTrans);

        Debug.Log("Furniture Data: Coord = " + coord.printString + "; height = " + furnitureMeta.height + "; depth = " + furnitureMeta.depthOrient + "; width = " + furnitureMeta.widthOrient);
        for (int k = 0; k < furnitureMeta.height; k++)
        {
            for (int i = 0; i < furnitureMeta.depthOrient; i++)
            {
                for (int j = 0; j < furnitureMeta.widthOrient; j++)
                {
                    if (!slots[coord.y + k, coord.z + i, coord.x + j].SetFurniture(furnitureMeta, furnitureObject))
                    {
                        Debug.LogError("Can't place wall furniture at (" + (coord.y + k) + "," + (coord.z + i) + "," + (coord.x + j) + ") at orientation " + furnitureMeta.orientation);
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
            Debug.LogError("HousingGrid -> SetWallFurniture -> Error: Can't find the collider of the furniture " + furnitureMeta.name + ".");
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

        if (!result)
        {
            //TO DO: erase furniture

        }

        return result;
    }

    #region --- Selected Slot ---

    public bool HighlightSlotMove(Direction dir, EditCameraDirection editCamDir)
    {
        bool result = false;
        HousingGridCoordinates oldCoord;
        HousingGridCoordinates newCoord = oldCoord = currentSlotCoord;
        Debug.Log(" dir = " + (int)dir + "; (int)editCamDir = " + (int)editCamDir);
        dir += (int)editCamDir;//magic stuff
        dir += (int)dir > 3 ? -4 : (int)dir < 0 ? +4 : 0;
        Debug.Log("dir after = " + (int)dir);

        switch (dir)
        {
            case Direction.Left:
                for (int i = currentSlotCoord.x - 1; i >= 0 && !result; i--)
                {
                    newCoord.x = i;
                    result = HighlightSlot(newCoord);
                    Debug.Log("newCoord = " + newCoord.printString + "; result = " + result);
                }
                //if (newCoord.x -1 >= 0)
                //{
                //    newCoord.x += -1;
                //    result = true;
                //}
                break;
            case Direction.Right:
                for (int i = currentSlotCoord.x + 1; i < width && !result; i++)
                {
                    newCoord.x = i;
                    result = HighlightSlot(newCoord);
                }
                //if (newCoord.x + 1 < myHouseMeta.width)
                //{
                //    newCoord.x += 1;
                //    result = true;
                //}
                break;
            case Direction.Up:
                for (int j = currentSlotCoord.z - 1; j >= 0 && !result; j--)
                {
                    newCoord.z = j;
                    result = HighlightSlot(newCoord);
                }
                //if (newCoord.z - 1 >= 0)
                //{
                //    newCoord.z += -1;
                //    result = true;
                //}
                break;
            case Direction.Down:
                for (int j = currentSlotCoord.z + 1; j < depth && !result; j++)
                {
                    newCoord.z = j;
                    result = HighlightSlot(newCoord);
                }
                //if (newCoord.z + 1 < myHouseMeta.depth)
                //{
                //    newCoord.z += 1;
                //    result = true;
                //}
                break;
        }
        if (result)
        {
            StopHighLightSlot(oldCoord);
        }


        return result;
    }

    public bool HighlightSlot(HousingGridCoordinates coord)
    {
        bool foundValid = false;
        for (int k = coord.y; k < height && !foundValid; k++)
        {
            if (slots[k, coord.z, coord.x] != null &&
                (k - 1 < 0 || (k - 1 >= 0 && (slots[k - 1, coord.z, coord.x] == null || (slots[k - 1, coord.z, coord.x] != null && !slots[k - 1, coord.z, coord.x].free))))
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
                    (k - 1 < 0 || (k - 1 >= 0 && (slots[k - 1, coord.z, coord.x] == null || (slots[k - 1, coord.z, coord.x] != null && !slots[k - 1, coord.z, coord.x].free))))
                    && (k + 1 >= height || (k + 1 < height && (slots[k + 1, coord.z, coord.x] == null || (slots[k + 1, coord.z, coord.x] != null && slots[k + 1, coord.z, coord.x].free)))))
                {
                    coord.y = k;
                    foundValid = true;
                }
            }
        }
        if (!foundValid) return false;

        currentSlotCoord = coord;
        bool result = false;
        MeshRenderer meshR = slots[coord.y, coord.z, coord.x].gameObject.GetComponent<MeshRenderer>();
        if (meshR == null) return false;
        meshR.enabled = true;
        meshR.material = slots[coord.y, coord.z, coord.x].free ? highlightedSlotMat[0] : highlightedSlotMat[1];
        result = true;
        Debug.Log("Highlight slot " + coord.printString);

        return result;
    }

    public void StopHighLightSlot(HousingGridCoordinates coord)
    {
        if (slots[coord.y, coord.z, coord.x] != null)
            slots[coord.y, coord.z, coord.x].gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    void HighlightFurniture()
    {

    }

    #endregion

    #region -- Get & Set --
    HousingSlot GetSlotAt(HousingGridCoordinates coord)
    {
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
