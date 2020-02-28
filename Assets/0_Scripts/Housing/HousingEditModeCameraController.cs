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
    EloyAdvancedAxisControls myRightJoyStickControls;

    [HideInInspector]
    public EditCameraDirection currentCameraDir = EditCameraDirection.ZPos;
    EditCameraMode currentCameraMode = EditCameraMode.ZoomedOut;

    GameObject myCameraObject;
    Transform middleCameraBase;
    HousingGrid houseGrid;

    //POSITION
    Vector3 centerCamPos;
    Vector3 targetCamPos;
    Vector3 currentCamPos;
    [Header(" - Camera Position - ")]
    public float camMoveSpeed = 20;
    float currentCamDist;
    public float followModeCamHeight = 3;
    public float zoomedInModeCamHeight = 1;
    Vector3 followSelectionPos;
    Vector3 followSelectionPosWithOffset
    {
        get
        {
            return followSelectionPos + Vector3.up * (currentCameraMode == EditCameraMode.FollowSelection ? followModeCamHeight : zoomedInModeCamHeight);
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
    Vector3 originalCamRot;
    Vector3 originalForwardVector;

    //ZOOM
    [Header(" - Camera Zoom - ")]
    public float zoomedInCamZoom = 2;
    float zoomedOutCamZoom = 0;
    public float followSelectionZoom = 4;
    float currentCamZoom;
    float targetCamZoom;
    public float cameraZoomSpeed = 10;

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
        myRightJoyStickControls = new EloyAdvancedAxisControls(GameInfo.instance.myControls.RotateCameraHousingEditMode, deadZone);
        houseGrid = _houseGrid;
        housePos = _housePos;
    }

    public void KonoUpdate()
    {
        HideWalls();
    }

    public void KonoLateUpdate()
    {
        followSelectionPos = houseGrid.GetSlotAt(houseGrid.currentSlotCoord).transform.position;
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
                break;
            case EditCameraMode.FollowSelection:
                targetCamPos = followSelectionPosWithOffset;
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
                targetCamPos = followSelectionPosWithOffset;
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
        transform.position = currentCamPos;
        middleCameraBase.transform.localRotation = Quaternion.Euler(currentMiddleCamBaseRot);
        myCameraObject.transform.localPosition = new Vector3(0, 0, -currentCamZoom);
        myCameraObject.transform.localRotation = Quaternion.Euler(currentCamRot);
    
        myRightJoyStickControls.ResetJoyStick();
    }

    public void Activate(Vector3 cameraBasePos, Vector3 _houseFloorCenter, float _zoomedOutCamDist)
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
        centerCamPos = targetCamPos = currentCamPos = cameraBasePos;
        transform.position = currentCamPos;

        //Middle Camera Base Rotation
        currentMiddleCamBaseRot = targetMiddleCamBaseRot = new Vector3(0, 0, 0);
        middleCameraBase.localRotation = Quaternion.Euler(currentMiddleCamBaseRot);

        //Camera zoom
        currentCamZoom = targetCamZoom = zoomedOutCamZoom = _zoomedOutCamDist;
        myCameraObject.transform.localPosition = new Vector3(0, 0, -currentCamZoom);

        //Camera Rotation
        //myCameraObject.transform.LookAt(_houseFloorCenter);
        Vector3 lookDir = _houseFloorCenter - myCameraObject.transform.position;
        myCameraObject.transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        targetCamRot = currentCamRot = originalCamRot = myCameraObject.transform.localRotation.eulerAngles;
        originalForwardVector = transform.forward;
        Debug.LogWarning("originalCamRot = " + originalCamRot.ToString("F4"));


        hiddenWalls = new List<MeshRenderer>();
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

    void SwitchCameraMode()
    {
        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                currentCameraMode = EditCameraMode.FollowSelection;
                targetCamZoom = followSelectionZoom;
                targetCamPos = followSelectionPosWithOffset;
                Vector3 futureCamPos = targetCamPos - originalForwardVector * targetCamZoom;
                targetCamRot = Quaternion.LookRotation(followSelectionPos - futureCamPos, Vector3.up).eulerAngles;
                Debug.LogWarning("targetCamRot = " + targetCamRot.ToString("F4") + "; currentCamRot = " + currentCamRot.ToString("F4"));
                break;
            case EditCameraMode.FollowSelection:
                currentCameraMode = EditCameraMode.ZoomedIn;
                targetCamPos = followSelectionPosWithOffset;
                targetCamZoom = zoomedInCamZoom;
                futureCamPos = targetCamPos - originalForwardVector * targetCamZoom;
                targetCamRot = Quaternion.LookRotation(followSelectionPos - futureCamPos, Vector3.up).eulerAngles;
                Debug.LogWarning("targetCamRot = " + targetCamRot.ToString("F4") + "; currentCamRot = " + currentCamRot.ToString("F4"));
                break;
            case EditCameraMode.ZoomedIn:
                currentCameraMode = EditCameraMode.ZoomedOut;
                //Camera Base Pos
                targetCamPos = centerCamPos;

                //Camera Base Rotation
                float targetCamBaseRotY = currentCameraDir == EditCameraDirection.XPos ? 90: currentCameraDir == EditCameraDirection.ZNeg ? 180 : currentCameraDir == EditCameraDirection.XNeg ? 270 :0;
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
                targetCamRot = originalCamRot;
                //Debug.LogWarning("differentAngle = " + differentAngle.ToString("F4") + "; (middleCameraBase.rotation.eulerAngles.y) = " + (middleCameraBase.rotation.eulerAngles.y).ToString("F4") + "; targetCamBaseRotY = " + targetCamBaseRotY);
                //Debug.LogWarning("targetCamBaseRot = " + targetCamBaseRot.ToString("F4") + "; currentCamBaseRot = " + currentCamBaseRot.ToString("F4")+"; camDir = "+currentCameraDir);
                break;
        }
    }

    #region -- Smoothing --
    void SmoothCamBasePos()
    {
        if (currentCamPos == targetCamPos) return;
        currentCamPos = Vector3.Lerp(currentCamPos, targetCamPos, camMoveSpeed * Time.deltaTime);
    }

    void SmoothCamBaseRot()
    {
        //if (smoothRotTime >= smoothRotMaxTime) return;
        if(currentCameraMode == EditCameraMode.ZoomedIn)
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
        if(currentMiddleCamBaseRot != targetMiddleCamBaseRot)
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
        Debug.LogWarning("Rotate Camera Right: targetCamBaseRot = " + targetCamBaseRot.ToString("F4")+"; currentCamBaseRot = "+currentCamBaseRot.ToString("F4"));
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
        Debug.LogWarning("Rotate Camera Left: targetCamBaseRot = " + targetCamBaseRot.ToString("F4"));
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
                        currentWidth = houseGrid.myHouseMeta.width * houseGrid.myHouseMeta.housingSlotSize;
                        break;
                    case EditCameraDirection.XPos:
                        currentWidth = houseGrid.myHouseMeta.depth * houseGrid.myHouseMeta.housingSlotSize;
                        break;
                    case EditCameraDirection.ZNeg:
                        currentWidth = houseGrid.myHouseMeta.width * houseGrid.myHouseMeta.housingSlotSize;
                        break;
                    case EditCameraDirection.XNeg:
                        currentWidth = houseGrid.myHouseMeta.depth * houseGrid.myHouseMeta.housingSlotSize;
                        break;
                }
                currentWidth = currentWidth / 2 - houseGrid.myHouseMeta.housingSlotSize - 0.01f;
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
        float rayLength = 1000;
        switch (currentCameraMode)
        {
            case EditCameraMode.FollowSelection:
                rayDir = followSelectionPos - myCameraObject.transform.position;
                rayLength = rayDir.magnitude;
                break;
            case EditCameraMode.ZoomedIn:
                rayDir = followSelectionPos - myCameraObject.transform.position;
                rayLength = rayDir.magnitude;
                break;
        }

        //calculate distance to wall
        List<MeshRenderer> newHiddenWalls = new List<MeshRenderer>();
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDir, out hit, rayLength, wallLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.tag == "HousingWall")
                maxDistance = hit.distance;
            Debug.DrawRay(rayOrigin, rayDir * maxDistance, Color.red);
            Vector3 perpVector = Vector3.Cross(Vector3.up, rayDir).normalized;
            Debug.DrawLine(hit.point + (perpVector * sphereCastRadius), hit.point - (perpVector * sphereCastRadius), Color.red);
            Debug.DrawLine(hit.point + (Vector3.up * sphereCastRadius), hit.point - (Vector3.up * sphereCastRadius), Color.red);

            RaycastHit[] hits = new RaycastHit[maxHitsNumber];
            sphereCastRadius = CalculateCurrentRadius();
            int hitsNumber = Physics.SphereCastNonAlloc(rayOrigin, sphereCastRadius, rayDir.normalized, hits, maxDistance, wallLayerMask, QueryTriggerInteraction.Ignore);
            if (hitsNumber > 0)
            {
                for (int i = 0; i < hitsNumber; i++)
                {
                    if (hits[i].collider.tag == "HousingWall")
                    {
                        MeshRenderer meshR = hits[i].collider.GetComponent<MeshRenderer>();
                        if (meshR == null)
                        {
                            Debug.LogError("HousingEditModeCameraController -> HideWalls: Can't find mesh Renderer of " + hits[i].collider.name);
                            continue;
                        }
                        Color oldColor = meshR.material.color;
                        meshR.material.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.01f);
                        newHiddenWalls.Add(meshR);
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
