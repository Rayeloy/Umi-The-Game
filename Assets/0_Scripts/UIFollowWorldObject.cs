using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowWorldObject : MonoBehaviour
{
    public Transform followObj;
    public Camera UICamera;
    RectTransform myRT;

    private void Awake()
    {
        myRT = GetComponent<RectTransform>();
    }

    private void Update()
    {
        Vector3 screenPos = UICamera.WorldToScreenPoint(followObj.position);
        Debug.Log("World pos = " + followObj.position.ToString("F4") + "; screenPos = " + screenPos.ToString("F4"));
        //transform.position = screenPos;
        //float outOfScreenX = ((UICamera.rect.x+UICamera.rect.width)-1) * UICamera.pixelWidth;
        //outOfScreenX = Mathf.Clamp(outOfScreenX, 0, float.MaxValue);
        float offsetX = -((UICamera.pixelWidth / 2)+(UICamera.rect.x*2 * UICamera.pixelWidth));
        float offsetY = -((UICamera.pixelHeight / 2) + (UICamera.rect.y * 2 * UICamera.pixelHeight));
        Debug.Log("UICamera.rect.x = " + UICamera.rect.x.ToString("F4") + "; UICamera.pixelWidth = " + UICamera.pixelWidth.ToString("F4"));
        myRT.localPosition = new Vector3(screenPos.x+offsetX, screenPos.y+offsetY, 0);
    }
}
