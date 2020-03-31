using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EditCameraMode
{
    ZoomedOut,
    FollowSelection,
    ZoomedIn
}

public enum EditCameraDirection
{
    ZPos = 0,
    ZNeg = 2,
    XPos = 1,
    XNeg = 3
}

public class HousingEditModeCameraController : MonoBehaviour
{
    PlayerActions actions;
    [Range(0, 1)]
    public float deadZone = 0.2f;
    EAITwoAxisControls myRightJoyStickControls;

    [HideInInspector]
    public EditCameraDirection currentCameraDir = EditCameraDirection.ZPos;
    EditCameraMode currentCameraMode = EditCameraMode.ZoomedOut;

    GameObject myCameraObject;
    Transform middleCameraBase;
    HousingGrid houseGrid;

    //GRID POINTS
    Vector3 houseCenter = Vector3.zero;
    Vector3 houseMaxCorner = Vector3.zero;
    Vector3 houseMaxMinYCorner = Vector3.zero;
    Vector3 houseMaxMinZCorner = Vector3.zero;
    Vector3 houseMinMaxXCorner = Vector3.zero;

    //POSITION
    Vector3 centerCamBasePos;
    Vector3 targetCamBasePos;
    Vector3 currentCamBasePos;
    [Header(" - Camera Position - ")]
    public float camMoveSpeed = 20;
    float currentCamDist;
    public float followModeCamHeight = 3;
    public float zoomedInModeCamHeight = 1;

    Vector3 followSelectionPosWithOffset
    {
        get
        {
            return houseGrid.GetCameraLookPosition(currentCameraMode) + Vector3.up * (currentCameraMode == EditCameraMode.FollowSelection ? followModeCamHeight : zoomedInModeCamHeight);
        }
    }

    //ROTATION
    [Header(" - Camera Base Rotation - ")]
    public float cameraBaseRotationSpeed = 150;
    public float smoothRotMaxTime = 0.5f;
    float smoothCamBaseRotTime = 0;
    Vector3 targetCamBaseRot;
    Vector3 currentCamBaseRot;
    //Vector3 lastCamBaseRot;
    float startRotY = 0;
    float smoothRotVal = 0;

    //Rotation for the zoomed in camera base
    [Header(" - Middle Camera Base - (For zoom in only) ")]
    public float middleCamBaseRotSpeed = 150;
    public float middleCamBaseRotSmoothSpeed = 2;
    public float clampAngleMax = 80f;
    public float clampAngleMin = -50f;
    Vector3 targetMiddleCamBaseRot;
    Vector3 currentMiddleCamBaseRot;

    //InsideCameraRotation
    [Header(" - Camera Rotation - ")]
    public float cameraRotationSpeed = 2;
    Vector3 targetCamRot;
    Vector3 currentCamRot;
    Vector3 originalFrontCamRot;
    Vector3 originalSideCamRot;
    Vector3 originalForwardVector;

    //ZOOM
    [Header(" - Camera Zoom - ")]
    public float zoomedInCamZoom = 2;
    float zoomedOutCamZoom = 0;
    public float followSelectionZoom = 4;
    float currentCamZoom;
    float targetCamZoom;
    public float cameraZoomSpeed = 10;
    public float camZoomFindStep = 0.2f;

    //SEE THROUGH WALLS
    [Header(" - See Through Walls -")]
    public float sphereCastRadius = 4;
    public float maxDistance = 10;
    public int maxHitsNumber = 50;
    public LayerMask wallLayerMask;
    List<MeshRenderer> hiddenWalls;
    Vector3 housePos;


    public void KonoAwake(HousingGrid _houseGrid, Vector3 _housePos)
    {
        myCameraObject = GetComponentInChildren<Camera>().gameObject;
        myCameraObject.gameObject.SetActive(false);
        Debug.Log("EditCameraBase: My camera = " + myCameraObject);
        middleCameraBase = transform.GetChild(0);
        myRightJoyStickControls = new EAITwoAxisControls(GameInfo.instance.myControls.HousingRotateCamera, deadZone);
        houseGrid = _houseGrid;
        housePos = _housePos;
        CalculateHouseViewPoints();
    }

    public void Activate(Vector3 cameraBasePos, float _zoomedOutCamDist)
    {
        if (myCameraObject == null)
        {
            Debug.LogError("EditCameraBase: My camera is null");
            return;
        }
        myCameraObject.SetActive(true);

        actions = GameInfo.instance.myControls;
        currentCameraMode = EditCameraMode.ZoomedOut;
        currentCameraDir = EditCameraDirection.ZPos;

        //Camera base Rotation
        currentCamBaseRot = targetCamBaseRot = Vector3.zero;
        transform.rotation = Quaternion.Euler(currentCamBaseRot.x, currentCamBaseRot.y, currentCamBaseRot.z);
        startRotY = currentCamBaseRot.y;
        smoothCamBaseRotTime = 0;

        //Camera base position
        centerCamBasePos = targetCamBasePos = currentCamBasePos = cameraBasePos;
        transform.position = currentCamBasePos;

        //Middle Camera Base Rotation
        currentMiddleCamBaseRot = targetMiddleCamBaseRot = new Vector3(0, 0, 0);
        middleCameraBase.localRotation = Quaternion.Euler(currentMiddleCamBaseRot);

        FindCorrectZoomAndRot();

        //Camera zoom
        currentCamZoom = targetCamZoom = zoomedOutCamZoom;
        myCameraObject.transform.localPosition = new Vector3(0, 0, -zoomedOutCamZoom);

        //Camera Rotation
        originalForwardVector = transform.forward;
        myCameraObject.transform.localRotation = Quaternion.Euler(originalFrontCamRot);
        currentCamRot = targetCamRot = originalFrontCamRot;

        hiddenWalls = new List<MeshRenderer>();

        Debug.LogWarning(" houseTopRightCorner = " + houseMaxCorner.ToString("F4") + "; houseBottomRightCorner = " + houseMaxMinYCorner.ToString("F4") +
            "; originalFrontCamRot = " + originalFrontCamRot.ToString("F4"));
    }

    public void DeActivate()
    {
        myCameraObject.gameObject.SetActive(false);
        actions = null;
        //currentCamRot = targetCamRot = Vector3.zero;

        for (int i = 0; i < hiddenWalls.Count; i++)
        {
            Color oldColor = hiddenWalls[i].material.color;
            hiddenWalls[i].material.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
        }
        hiddenWalls.Clear();
    }

    void CalculateHouseViewPoints()
    {
        houseCenter = houseGrid.worldCenter;

        //FRONT VIEW
        Vector3 dirToTopRightCorner = (houseGrid.maxSlotWorldPos - houseCenter).normalized;
        houseMaxCorner = houseGrid.maxSlotWorldPos + (dirToTopRightCorner * MasterManager.HousingSettings.slotSize * 1.5f);

        Vector3 bottomRightSlotPos = new Vector3(houseGrid.maxSlotWorldPos.x, houseGrid.minSlotWorldPos.y, houseGrid.maxSlotWorldPos.z);
        Vector3 dirToBottomRightCorner = (bottomRightSlotPos - houseCenter).normalized;
        houseMaxMinYCorner = bottomRightSlotPos + (dirToBottomRightCorner * MasterManager.HousingSettings.slotSize * 2);

        //SIDEVIEW
        Vector3 topRightSlotPos = new Vector3(houseGrid.maxSlotWorldPos.x, houseGrid.maxSlotWorldPos.y, houseGrid.minSlotWorldPos.z);
        dirToTopRightCorner = (topRightSlotPos - houseCenter).normalized;
        houseMaxMinZCorner = topRightSlotPos + (dirToTopRightCorner * MasterManager.HousingSettings.slotSize * 1.5f);

        bottomRightSlotPos = new Vector3(houseGrid.maxSlotWorldPos.x, houseGrid.minSlotWorldPos.y, houseGrid.minSlotWorldPos.z);
        dirToBottomRightCorner = (bottomRightSlotPos - houseCenter).normalized;
        houseMinMaxXCorner = bottomRightSlotPos + (dirToBottomRightCorner * MasterManager.HousingSettings.slotSize * 2);
    }

    bool findZoomAndRotDone = false;
    void FindCorrectZoomAndRot()
    {
        if (findZoomAndRotDone) return;
        findZoomAndRotDone = true;
        myCameraObject.transform.localRotation = Quaternion.identity;
        transform.localRotation = Quaternion.identity;

        //FRONT
        bool visible = false;
        float frontZoom = 0;
        while (!visible)
        {
            frontZoom += camZoomFindStep;
            for (int cameraAngle = -30; cameraAngle < 70 && !visible; cameraAngle += 1)
            {
                myCameraObject.transform.localRotation = Quaternion.Euler(new Vector3(cameraAngle, 0, 0));
                myCameraObject.transform.localPosition = new Vector3(0, 0, -frontZoom);

                //Check if points visible
                Vector3 cameraTopPoint = myCameraObject.GetComponent<Camera>().WorldToViewportPoint(houseMaxCorner);
                Vector3 cameraBottomPoint = myCameraObject.GetComponent<Camera>().WorldToViewportPoint(houseMaxMinYCorner);
                if (!(cameraTopPoint.x < 0 || cameraTopPoint.x > 1 || cameraTopPoint.y < 0 || cameraTopPoint.y > 1 || cameraTopPoint.z <= 0 ||
                    cameraBottomPoint.x < 0 || cameraBottomPoint.x > 1 || cameraBottomPoint.y < 0 || cameraBottomPoint.y > 1 || cameraBottomPoint.z <= 0))
                {
                    Debug.LogWarning("Front Corners are visible: sideZoom = " + frontZoom + "; cameraAngle = " + cameraAngle);
                    originalFrontCamRot = myCameraObject.transform.localRotation.eulerAngles;
                    visible = true;
                }
            }
        }

        transform.localRotation = Quaternion.Euler(new Vector3(0, -90, 0));
        myCameraObject.transform.localRotation = Quaternion.identity;
        //SIDE 
        visible = false;
        float sideZoom = 0;
        while (!visible)
        {
            sideZoom += camZoomFindStep;
            for (int cameraAngle = -30; cameraAngle < 60 && !visible; cameraAngle += 1)
            {
                myCameraObject.transform.localRotation = Quaternion.Euler(new Vector3(cameraAngle, 0, 0));
                myCameraObject.transform.localPosition = new Vector3(0, 0, -sideZoom);

                //Check if points visible
                Vector3 cameraTopPoint = myCameraObject.GetComponent<Camera>().WorldToViewportPoint(houseMaxMinZCorner);
                Vector3 cameraBottomPoint = myCameraObject.GetComponent<Camera>().WorldToViewportPoint(houseMinMaxXCorner);
                if (!(cameraTopPoint.x < 0 || cameraTopPoint.x > 1 || cameraTopPoint.y < 0 || cameraTopPoint.y > 1 || cameraTopPoint.z <= 0 ||
                    cameraBottomPoint.x < 0 || cameraBottomPoint.x > 1 || cameraBottomPoint.y < 0 || cameraBottomPoint.y > 1 || cameraBottomPoint.z <= 0))
                {
                    Debug.LogWarning("Side Corners are visible: sideZoom = " + sideZoom + "; cameraAngle = " + cameraAngle);
                    originalSideCamRot = myCameraObject.transform.localRotation.eulerAngles;
                    visible = true;
                }
            }
        }

        transform.localRotation = Quaternion.Euler(Vector3.zero);

        //RESULT
        if (frontZoom >= sideZoom)
        {
            zoomedOutCamZoom = currentCamZoom = targetCamZoom = frontZoom;
        }
        else
        {
            zoomedOutCamZoom = currentCamZoom = targetCamZoom = sideZoom;
        }
    }

    public void KonoUpdate()
    {
        HideWalls();
    }

    public void KonoLateUpdate()
    {
        //Debug.LogWarning("Edit Camera is in mode " + currentCameraMode);
        if (actions.ZoomIn.WasPressed) SwitchCameraMode();

        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                //Inputs
                if (myRightJoyStickControls.RightWasPressed)
                {
                    RotateCameraRight();
                }
                else if (myRightJoyStickControls.LeftWasPressed)
                {
                    RotateCameraLeft();
                }
                Debug.DrawLine(houseCenter, houseMaxCorner, Color.yellow);
                Debug.DrawLine(houseCenter, houseMaxMinYCorner, Color.yellow);
                Debug.DrawLine(houseCenter, houseMaxMinZCorner, Color.yellow);
                Debug.DrawLine(houseCenter, houseMinMaxXCorner, Color.yellow);
                break;
            case EditCameraMode.FollowSelection:
                targetCamBasePos = followSelectionPosWithOffset;
                targetCamZoom = houseGrid.lookingAtLargeFurniture ? followSelectionZoom + 2 : followSelectionZoom;
                //myCameraObject.transform.LookAt(followSelectionPos);

                //Inputs
                if (myRightJoyStickControls.RightWasPressed)
                {
                    RotateCameraRight();
                }
                else if (myRightJoyStickControls.LeftWasPressed)
                {
                    RotateCameraLeft();
                }
                break;
            case EditCameraMode.ZoomedIn:
                targetCamBasePos = followSelectionPosWithOffset;
                float inputX = -actions.RightJoystick.X;
                float inputZ = actions.RightJoystick.Y;

                Vector3 input = new Vector2(inputX, inputZ);
                if (actions.Device != null && input.magnitude < 0.1f) inputX = inputZ = 0;

                float currentYRot = middleCameraBase.rotation.eulerAngles.y % 360;
                currentCameraDir = currentYRot >= 45 && currentYRot < 135 ? EditCameraDirection.XPos : currentYRot >= 135 && currentYRot < 225 ? EditCameraDirection.ZNeg :
                    currentYRot >= 225 && currentYRot < 315 ? EditCameraDirection.XNeg : EditCameraDirection.ZPos;
                targetMiddleCamBaseRot.y += inputX * middleCamBaseRotSpeed * Time.deltaTime;
                targetMiddleCamBaseRot.x += inputZ * middleCamBaseRotSpeed * Time.deltaTime;
                targetMiddleCamBaseRot.x = Mathf.Clamp(targetMiddleCamBaseRot.x, clampAngleMin, clampAngleMax);
                break;
        }

        //SMOOTH ROTATION/MOVEMENT/ZOOM
        SmoothCamBaseRot();
        SmoothCamBasePos();
        SmoothMiddleCamBaseRot();
        SmoothCamZoom();
        SmoothCamRot();

        //APPLY ROTATION/MOVEMENT/ZOOM
        //Debug.Log(" currentCamRot = " + currentCamRot.ToString("F4"));
        transform.rotation = Quaternion.Euler(currentCamBaseRot);
        transform.position = currentCamBasePos;
        middleCameraBase.transform.localRotation = Quaternion.Euler(currentMiddleCamBaseRot);
        myCameraObject.transform.localPosition = new Vector3(0, 0, -currentCamZoom);
        myCameraObject.transform.localRotation = Quaternion.Euler(currentCamRot);

        myRightJoyStickControls.ResetJoyStick();
    }

    void SwitchCameraMode()
    {
        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                currentCameraMode = EditCameraMode.FollowSelection;
                targetCamZoom = houseGrid.lookingAtLargeFurniture? followSelectionZoom+2: followSelectionZoom;
                targetCamBasePos = followSelectionPosWithOffset;
                Vector3 futureCamPos = targetCamBasePos - originalForwardVector * targetCamZoom;
                targetCamRot = Quaternion.LookRotation(houseGrid.GetCameraLookPosition() - futureCamPos, Vector3.up).eulerAngles;
                Debug.LogWarning("targetCamRot = " + targetCamRot.ToString("F4") + "; currentCamRot = " + currentCamRot.ToString("F4") + "; futureCamPos = " + futureCamPos.ToString("F4"));
                break;
            case EditCameraMode.FollowSelection:
                currentCameraMode = EditCameraMode.ZoomedIn;
                targetCamBasePos = followSelectionPosWithOffset;
                targetCamZoom = zoomedInCamZoom;
                futureCamPos = targetCamBasePos - originalForwardVector * targetCamZoom;
                targetCamRot = Quaternion.LookRotation(houseGrid.GetCameraLookPosition(EditCameraMode.ZoomedIn) - futureCamPos, Vector3.up).eulerAngles;
                Debug.LogWarning("targetCamRot = " + targetCamRot.ToString("F4") + "; currentCamRot = " + currentCamRot.ToString("F4"));
                break;
            case EditCameraMode.ZoomedIn:
                currentCameraMode = EditCameraMode.ZoomedOut;
                //Camera Base Pos
                targetCamBasePos = centerCamBasePos;

                //Camera Base Rotation
                float targetCamBaseRotY = currentCameraDir == EditCameraDirection.XPos ? 90 : currentCameraDir == EditCameraDirection.ZNeg ? 180 : currentCameraDir == EditCameraDirection.XNeg ? 270 : 0;
                currentCamBaseRot = targetCamBaseRot = new Vector3(0, targetCamBaseRotY, 0);
                //transform.rotation = Quaternion.Euler(currentCamBaseRot);

                //Middle Camera Base Rotation
                Debug.Log("middleCameraBase.rotation = " + middleCameraBase.rotation.eulerAngles.ToString("F4"));
                float differentAngle = (middleCameraBase.rotation.eulerAngles.y) - targetCamBaseRotY;
                currentMiddleCamBaseRot.y = differentAngle;
                targetMiddleCamBaseRot = new Vector3(0, (currentMiddleCamBaseRot.y > 180 ? 360 : 0), 0);

                //Camera Zoom
                targetCamZoom = zoomedOutCamZoom;

                //Camera rotation
                currentCamRot = myCameraObject.transform.localRotation.eulerAngles;
                targetCamRot = ((int)currentCameraDir % 2) == 0 ? originalFrontCamRot : originalSideCamRot;
                //Debug.LogWarning("differentAngle = " + differentAngle.ToString("F4") + "; (middleCameraBase.rotation.eulerAngles.y) = " + (middleCameraBase.rotation.eulerAngles.y).ToString("F4") + "; targetCamBaseRotY = " + targetCamBaseRotY);
                //Debug.LogWarning("targetCamBaseRot = " + targetCamBaseRot.ToString("F4") + "; currentCamBaseRot = " + currentCamBaseRot.ToString("F4")+"; camDir = "+currentCameraDir);
                break;
        }
    }

    #region -- Smoothing --
    void SmoothCamBasePos()
    {
        if (currentCamBasePos == targetCamBasePos) return;
        currentCamBasePos = Vector3.Lerp(currentCamBasePos, targetCamBasePos, camMoveSpeed * Time.deltaTime);
    }

    void SmoothCamBaseRot()
    {
        //if (smoothRotTime >= smoothRotMaxTime) return;
        if (currentCameraMode == EditCameraMode.ZoomedIn)
        {
            //currentCamBaseRot = Vector3.Lerp(currentCamBaseRot, targetCamBaseRot, cameraBaseRotationSpeed * Time.deltaTime);
        }
        else
        {
            if (smoothRotVal >= 1) return;
            smoothCamBaseRotTime += Time.deltaTime;
            smoothRotVal = Mathf.Clamp01(smoothCamBaseRotTime / smoothRotMaxTime);
            float y = EasingFunction.EaseOutBack(startRotY, targetCamBaseRot.y, smoothRotVal);
            //Debug.Log(" targetCamRot.y= "+ targetCamRot.y + "; value = " + smoothRotVal + "; y = " + y);
            currentCamBaseRot = new Vector3(0, y, 0);
        }
    }

    void SmoothMiddleCamBaseRot()
    {
        if (currentMiddleCamBaseRot != targetMiddleCamBaseRot)
            currentMiddleCamBaseRot = Vector3.Lerp(currentMiddleCamBaseRot, targetMiddleCamBaseRot, middleCamBaseRotSmoothSpeed * Time.deltaTime);
    }

    void SmoothCamZoom()
    {
        if (currentCamZoom == targetCamZoom) return;
        currentCamZoom = Mathf.Lerp(currentCamZoom, targetCamZoom, cameraZoomSpeed * Time.deltaTime);
    }

    void SmoothCamRot()
    {
        if (currentCamRot != targetCamRot)
            currentCamRot = Vector3.Lerp(currentCamRot, targetCamRot, cameraRotationSpeed * Time.deltaTime);
    }
    #endregion

    void RotateCameraRight()
    {
        smoothCamBaseRotTime = 0;
        smoothRotVal = 0;
        startRotY = currentCamBaseRot.y;
        float newRotY = targetCamBaseRot.y - 90;
        //newRotY += newRotY < 0 ? 360 : 0;
        targetCamBaseRot.y = newRotY;
        currentCameraDir--;
        currentCameraDir += (int)currentCameraDir < 0 ? 4 : 0;

        if (currentCameraMode == EditCameraMode.ZoomedOut)
        {
            targetCamRot = ((int)currentCameraDir % 2) == 0 ? originalFrontCamRot : originalSideCamRot;
        }
        //Debug.LogWarning("Rotate Camera Right: targetCamBaseRot = " + targetCamBaseRot.ToString("F4") + "; currentCamBaseRot = " + currentCamBaseRot.ToString("F4") + "; targetCamRot = " + targetCamRot);
    }

    void RotateCameraLeft()
    {
        smoothCamBaseRotTime = 0;
        smoothRotVal = 0;
        startRotY = currentCamBaseRot.y;
        float newRotY = targetCamBaseRot.y + 90;
        //newRotY += newRotY > 360 ? -360 : 0;
        targetCamBaseRot.y = newRotY;
        currentCameraDir++;
        currentCameraDir += (int)currentCameraDir > 3 ? -4 : 0;

        if (currentCameraMode == EditCameraMode.ZoomedOut)
        {
            targetCamRot = ((int)currentCameraDir % 2) == 0 ? originalFrontCamRot : originalSideCamRot;
        }
        //Debug.LogWarning("Rotate Camera Left: targetCamBaseRot = " + targetCamBaseRot.ToString("F4") + "; targetCamRot = " + targetCamRot);
    }

    #region -- Hide Walls --

    float CalculateCurrentRadius()
    {
        float currentWidth = 0;//current width of the house from our viewpoint

        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                switch (currentCameraDir)
                {
                    case EditCameraDirection.ZPos:
                        currentWidth = houseGrid.myHouseMeta.width * MasterManager.HousingSettings.slotSize;
                        break;
                    case EditCameraDirection.XPos:
                        currentWidth = houseGrid.myHouseMeta.depth * MasterManager.HousingSettings.slotSize;
                        break;
                    case EditCameraDirection.ZNeg:
                        currentWidth = houseGrid.myHouseMeta.width * MasterManager.HousingSettings.slotSize;
                        break;
                    case EditCameraDirection.XNeg:
                        currentWidth = houseGrid.myHouseMeta.depth * MasterManager.HousingSettings.slotSize;
                        break;
                }
                currentWidth = currentWidth / 2 - MasterManager.HousingSettings.slotSize - 0.01f;
                break;
            case EditCameraMode.FollowSelection:
                currentWidth = 3;
                break;
            case EditCameraMode.ZoomedIn:
                currentWidth = 1;
                break;
        }

        return currentWidth;
    }

    void HideWalls()
    {
        Vector3 rayOrigin = myCameraObject.transform.position;
        Vector3 rayDir = myCameraObject.transform.forward;
        float rayLength = 0;
        switch (currentCameraMode)
        {
            case EditCameraMode.FollowSelection:
                rayDir = houseGrid.GetCameraLookPosition(currentCameraMode) - rayOrigin;
                rayLength = rayDir.magnitude;
                break;
            case EditCameraMode.ZoomedIn:
                rayDir = houseGrid.GetCameraLookPosition(currentCameraMode) - rayOrigin;
                rayLength = rayDir.magnitude;
                break;
            case EditCameraMode.ZoomedOut:
                rayLength = (houseCenter - rayOrigin).magnitude;
                break;
        }
        rayDir.Normalize();
        Debug.DrawRay(rayOrigin, rayDir * rayLength, Color.blue);
        //calculate distance to wall
        List<MeshRenderer> newHiddenWalls = new List<MeshRenderer>();
        RaycastHit[] hits = new RaycastHit[maxHitsNumber];
        int hitsNumber = Physics.RaycastNonAlloc(rayOrigin, rayDir, hits, rayLength, wallLayerMask, QueryTriggerInteraction.Ignore);
        if (hitsNumber > 0)
        {
            float minDist = float.MaxValue;
            float maxDist = 0;
            Vector3 minPoint = Vector3.zero;
            Vector3 maxPoint = Vector3.zero;
            int correctHitsFound = 0;
            float auxSphereRadius = 0;
            for (int i = 0; i < hitsNumber; i++)
            {
                if (hits[i].transform.tag == "HousingWall")
                {
                    correctHitsFound++;
                    float auxDist = (hits[i].point - rayOrigin).magnitude;
                    if (auxDist > maxDist)
                    {
                        maxDist = auxDist;
                        maxPoint = hits[i].point;
                    }
                    if (auxDist < minDist)
                    {
                        minDist = auxDist;
                        minPoint = hits[i].point;
                    }
                }
            }
            //Debug.Log("CorrectHitsFound = " + correctHitsFound);

            if (correctHitsFound >= 1)
            {
                Vector3 middlePoint = VectorMath.MiddlePoint(maxPoint, minPoint);
                maxDistance = (middlePoint - rayOrigin).magnitude;
                auxSphereRadius = (middlePoint - minPoint).magnitude;

                sphereCastRadius = CalculateCurrentRadius();
                sphereCastRadius = auxSphereRadius > sphereCastRadius ? auxSphereRadius : sphereCastRadius;
                Vector3 perpVector = Vector3.Cross(Vector3.up, rayDir).normalized;
                Vector3 point = rayOrigin + (rayDir * maxDistance);

                //Draw sphere?
                Debug.DrawLine(rayOrigin, point, Color.red);
                Debug.DrawLine(point + (perpVector * sphereCastRadius), point - (perpVector * sphereCastRadius), Color.red);
                Debug.DrawLine(point + (Vector3.up * sphereCastRadius), point - (Vector3.up * sphereCastRadius), Color.red);

                hits = new RaycastHit[maxHitsNumber];
                hitsNumber = Physics.SphereCastNonAlloc(rayOrigin, sphereCastRadius, rayDir.normalized, hits, maxDistance, wallLayerMask, QueryTriggerInteraction.Ignore);
                if (hitsNumber > 0)
                {
                    for (int j = 0; j < hitsNumber; j++)
                    {
                        if (hits[j].collider.tag == "HousingWall")
                        {
                            MeshRenderer meshR = hits[j].collider.GetComponent<MeshRenderer>();
                            if (meshR == null)
                            {
                                Debug.LogError("HousingEditModeCameraController -> HideWalls: Can't find mesh Renderer of " + hits[j].collider.name);
                                continue;
                            }
                            Color oldColor = meshR.material.color;
                            meshR.material.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.01f);
                            newHiddenWalls.Add(meshR);
                        }
                    }
                }
            }
        }

        for (int j = 0; j < hiddenWalls.Count; j++)
        {
            if (!newHiddenWalls.Contains(hiddenWalls[j]))
            {
                Color oldColor = hiddenWalls[j].material.color;
                hiddenWalls[j].material.color = new Color(oldColor.r, oldColor.g, oldColor.b, 1f);
            }
        }
        hiddenWalls = newHiddenWalls;
    }

    #endregion

    #region - Auxiliar -

    //Vector3 GetLookingRotAt(Transform myTransform, Vector3 lookingPos, Vector3 target)
    //{
    //    Vector3 oldPos = myTransform.position;

    //    myTransform.position = lookingPos;
    //    Vector3 lookdir = target - lookingPos;
    //    Vector3 result = Quaternion.LookRotation(, Vector3.up)
    //}
    #endregion
}
