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
    float pixelW;
    float pixelH;



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
        pixelW = myCamera.pixelWidth;
        pixelH = myCamera.pixelHeight;
        Vector3 screenPos = myCamera.WorldToScreenPoint(myHookPointTrans.position);
        Debug.Log("World pos = " + myHookPointTrans.position.ToString("F4") + "; screenPos = " + screenPos.ToString("F4"));
        //transform.position = screenPos;
        //float outOfScreenX = ((UICamera.rect.x+UICamera.rect.width)-1) * UICamera.pixelWidth;
        //outOfScreenX = Mathf.Clamp(outOfScreenX, 0, float.MaxValue);
        float offsetX = -((pixelW / 2) + (myCamera.rect.x * 2 * pixelW));
        float offsetY = -((pixelH / 2) + (myCamera.rect.y * 2 * pixelH));
        Debug.Log("UICamera.rect.x = " + myCamera.rect.x.ToString("F4") + "; UICamera.pixelWidth = " + myCamera.pixelWidth.ToString("F4"));
        myRect.localPosition = new Vector3(screenPos.x + offsetX, screenPos.y + offsetY, 0);

        //Vector3 vPPos = myCamera.WorldToViewportPoint(myHookPointTrans.position);
        //Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(myCamera, myHookPointTrans.position); //myCamera.WorldToScreenPoint(myHookPointTrans.position);
        //screenPoint.z = myCanvas.planeDistance;
        //print("HookPointHUD: Update: newPos"+ screenPoint.ToString("F4"));
        //Vector2 result;
        //result = myCamera.ScreenToWorldPoint(screenPoint);
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), screenPoint, myCamera, out result);
        //transform.position = result;
        //Vector3 screenPos = myCamera.WorldToViewportPoint(myHookPointTrans.position);
        //myRect.anchorMin = screenPos;
        //myRect.anchorMax = screenPos;
    }
}
