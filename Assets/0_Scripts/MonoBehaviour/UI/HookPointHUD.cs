using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HookPointHUD : MonoBehaviour
{
    Camera myCamera;
    [HideInInspector]
    public HookPoint myHookPoint;
    Transform myHookPointTrans;
    //RectTransform myCanvasRect;
    RectTransform myRect;
    Canvas myCanvas;
    //Vector2 scale;



    public void KonoAwake(HookPoint hookPoint, Camera _myCamera, Canvas _myCanvas)
    {
        //myCanvasRect = transform.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        myHookPoint = hookPoint;
        myHookPointTrans = myHookPoint.transform;
        myRect = GetComponent<RectTransform>();
        myCanvas = _myCanvas;
        myCamera = myCanvas.worldCamera;

        //scale= new Vector2(myCanvasRect.rect.width / Screen.width, myCanvasRect.rect.height / Screen.height);
    }

    public void KonoUpdate()
    {
        //Vector3 vPPos = myCamera.WorldToViewportPoint(myHookPointTrans.position);
        Vector3 screenPoint = myCamera.WorldToScreenPoint(myHookPointTrans.position);
        screenPoint.z = myCanvas.planeDistance;
        //print("HookPointHUD: Update: newPos"+ screenPoint.ToString("F4"));
        Vector2 result;
        result = myCamera.ScreenToWorldPoint(screenPoint);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), screenPoint, myCamera, out result);
        transform.position = result;
    }
}
