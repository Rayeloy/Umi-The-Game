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
    ZPos=0,
    ZNeg=2,
    XPos=1,
    XNeg=3
}

public class HousingEditModeCameraController : MonoBehaviour
{
    PlayerActions actions;
    [Range(0, 1)]
    public float deadZone = 0.2f;
    JoyStickControls myRightJoyStickControls;

    [HideInInspector]
    public EditCameraDirection currentCameraDir = EditCameraDirection.ZPos;
    EditCameraMode currentCameraMode = EditCameraMode.ZoomedOut;

    GameObject myCameraObject;
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
            return followSelectionPos + Vector3.up * (currentCameraMode == EditCameraMode.FollowSelection? followModeCamHeight: zoomedInModeCamHeight);
        }
    }

    //ROTATION
    [Header(" - Camera Base Rotation - ")]
    public float cameraBaseRotationSpeed = 150;
    public float clampAngleMax = 80f;
    public float clampAngleMin = 80f;
    public float smoothRotMaxTime = 0.5f;
    float smoothCamBaseRotTime = 0;
    float rotY, rotX = 0;
    Vector3 targetCamBaseRot;
    Vector3 currentCamBaseRot;
    float startRotY = 0;
    Quaternion originalCamBaseRot;
    //InsideCameraRotation

    [Header(" - Camera Rotation - ")]
    Vector3 targetCamRot;
    Vector3 currentCamRot;
    public float cameraRotationSpeed = 2;

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
    Vector3 housePos;


    public void KonoAwake(HousingGrid _houseGrid, Vector3 _housePos)
    {
        myCameraObject = GetComponentInChildren<Camera>().gameObject;
        myCameraObject.gameObject.SetActive(false);
        Debug.Log("EditCameraBase: My camera = " + myCameraObject);
        myRightJoyStickControls = new JoyStickControls(GameInfo.instance.myControls.RotateCameraHousingEditMode, deadZone);
        houseGrid = _houseGrid;
        housePos = _housePos;
    }

    public void Activate(Vector3 cameraBasePos, Vector3 _houseFloorCenter, float _zoomedOutCamDist)
    {
        if (myCameraObject == null)
        {
            Debug.LogError("EditCameraBase: My camera is null");
            return;
        }
        myCameraObject.SetActive(true);

        currentCamBaseRot = targetCamBaseRot = Vector3.zero;
        transform.rotation = Quaternion.Euler(currentCamBaseRot.x, currentCamBaseRot.y, currentCamBaseRot.z);
        startRotY = currentCamBaseRot.y;
        centerCamPos = targetCamPos = currentCamPos = cameraBasePos;
        transform.position = currentCamPos;
        currentCamZoom = targetCamZoom = zoomedOutCamZoom = _zoomedOutCamDist;
        myCameraObject.transform.localPosition = new Vector3(0, 0, currentCamZoom);
        myCameraObject.transform.LookAt(_houseFloorCenter);
        originalCamBaseRot = myCameraObject.transform.localRotation;
        Debug.Log("inside camera Rotation = " + myCameraObject.transform.localRotation.eulerAngles.ToString("F4"));

        actions = GameInfo.instance.myControls;
        currentCameraMode = EditCameraMode.ZoomedOut;
        currentCameraDir = EditCameraDirection.ZPos;

        smoothCamBaseRotTime = 0;

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
                myCameraObject.transform.LookAt(followSelectionPos);

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
                float inputX =  actions.RightJoystick.X;
                float inputZ = -actions.RightJoystick.Y;

                Vector3 input = new Vector2(inputX, inputZ);
                if (!actions.isKeyboard && input.magnitude < 0.1f) inputX = inputZ = 0;

                Quaternion localRotation = Quaternion.Euler(0, 0, 0);

                rotY += inputX * cameraBaseRotationSpeed * Time.deltaTime;
                rotX += inputZ * cameraBaseRotationSpeed * Time.deltaTime;
                rotX = Mathf.Clamp(rotX, clampAngleMin, clampAngleMax);
                Debug.Log("RotX = "+rotX+"; RotY = " + rotY);
                targetCamBaseRot = new Vector3(rotX, rotY, 0.0f);
                break;
        }

        //SMOOTH ROTATION/MOVEMENT/ZOOM
        SmoothCamRot();
        SmoothCamPos();
        SmoothCamZoom();

        //APPLY ROTATION/MOVEMENT/ZOOM
        //Debug.Log(" currentCamRot = " + currentCamRot.ToString("F4"));
        transform.rotation = Quaternion.Euler(currentCamBaseRot.x, currentCamBaseRot.y, currentCamBaseRot.z);
        transform.position = currentCamPos;
        myCameraObject.transform.localPosition = new Vector3(0, 0, currentCamZoom);

        myRightJoyStickControls.ResetJoyStick();
    }

    void SmoothCamPos()
    {
        if (currentCamPos == targetCamPos) return;
        currentCamPos = Vector3.Lerp(currentCamPos, targetCamPos, camMoveSpeed * Time.deltaTime);
    }

    float smoothRotVal = 0;
    void SmoothCamRot()
    {
        //if (smoothRotTime >= smoothRotMaxTime) return;
        if (smoothRotVal >= 1) return;
        smoothCamBaseRotTime += Time.deltaTime;
        smoothRotVal = Mathf.Clamp01(smoothCamBaseRotTime / smoothRotMaxTime);
        float y = EasingFunction.EaseOutBack(startRotY, targetCamBaseRot.y, smoothRotVal);
        //Debug.Log(" targetCamRot.y= "+ targetCamRot.y + "; value = " + smoothRotVal + "; y = " + y);
        currentCamBaseRot = new Vector3(currentCamBaseRot.x, y, currentCamBaseRot.z);
    }

    void SmoothCamZoom()
    {
        if (currentCamZoom == targetCamZoom) return;
        currentCamZoom = Mathf.Lerp(currentCamZoom, targetCamZoom, cameraZoomSpeed * Time.deltaTime);
    }

    void RotateCameraRight()
    {
        Debug.Log("Rotate Camera Right");
        smoothCamBaseRotTime = 0;
        smoothRotVal = 0;
        startRotY = currentCamBaseRot.y;
        float newRotY = targetCamBaseRot.y - 90;
        //newRotY += newRotY < 0 ? 360 : 0;
        targetCamBaseRot.y = newRotY;
        currentCameraDir--;
        currentCameraDir += (int)currentCameraDir < 0 ? 4 : 0;
    }

    void RotateCameraLeft()
    {
        Debug.Log("Rotate Camera Left");
        smoothCamBaseRotTime = 0;
        smoothRotVal = 0;
        startRotY = currentCamBaseRot.y;
        float newRotY = targetCamBaseRot.y + 90;
        //newRotY += newRotY > 360 ? -360 : 0;
        targetCamBaseRot.y = newRotY;
        currentCameraDir++;
        currentCameraDir += (int)currentCameraDir > 3 ? -4 : 0;
    }

    void SwitchCameraMode()
    {
        switch (currentCameraMode)
        {
            case EditCameraMode.ZoomedOut:
                currentCameraMode = EditCameraMode.FollowSelection;
                //TO DO: Follow selection
                targetCamZoom = -followSelectionZoom;
                targetCamPos = followSelectionPos;
                Quaternion origRot = myCameraObject.transform.localRotation;
                myCameraObject.transform.LookAt(followSelectionPos);
                break;
            case EditCameraMode.FollowSelection:
                currentCameraMode = EditCameraMode.ZoomedIn;
                targetCamPos = followSelectionPos;
                targetCamZoom = -zoomedInCamZoom;
                myCameraObject.transform.localRotation = Quaternion.identity;
                break;
            case EditCameraMode.ZoomedIn:
                currentCameraMode = EditCameraMode.ZoomedOut;
                targetCamPos = centerCamPos;
                targetCamZoom = -zoomedOutCamZoom;
                currentCamBaseRot = targetCamBaseRot = new Vector3(0, 0, 0);
                myCameraObject.transform.localRotation = originalCamBaseRot;
                break;
        }
    }

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
                currentWidth =  currentWidth / 2 - houseGrid.myHouseMeta.housingSlotSize;
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

    List<MeshRenderer> hiddenWalls;
    void HideWalls()
    {
        Vector3 rayOrigin = myCameraObject.transform.position;
        Vector3 rayDir = myCameraObject.transform.forward;
        float rayLength = 100;
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
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDir, out hit, rayLength, wallLayerMask,QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.tag == "HousingWall")
            maxDistance = hit.distance;
        }
        Debug.DrawRay(rayOrigin, rayDir * maxDistance, Color.red);

        RaycastHit[] hits = new RaycastHit[maxHitsNumber];
        sphereCastRadius = CalculateCurrentRadius();
        int hitsNumber = Physics.SphereCastNonAlloc(rayOrigin, sphereCastRadius, rayDir.normalized, hits, maxDistance, wallLayerMask, QueryTriggerInteraction.Ignore);
        if (hitsNumber > 0)
        {
            List<MeshRenderer> newHiddenWalls = new List<MeshRenderer>();
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
    }
}
