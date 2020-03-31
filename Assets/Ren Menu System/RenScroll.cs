using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class RenScroll : MonoBehaviour
{
    public int buttonsGroup;
    float maxY;
    float minY;
    public float breakSpeed;
    public float scrollSpeed;
    public RectTransform scroll;
    RectTransform scrollMask;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            //set to top of scroll
        }
    }

    private void OnEnable()
    {
        if (Application.isEditor)
        {
            if (scrollMask == null) scrollMask = GetComponent<RectTransform>();
            maxY = scrollMask.rect.yMax - (scroll.rect.height / 2);
            minY = scrollMask.rect.yMin + (scroll.rect.height / 2);
            scroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollMask.sizeDelta.x);
            scroll.localPosition = new Vector2(0, scroll.localPosition.y);
        }
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if(transform.hasChanged || scroll.transform.hasChanged)
            {
                transform.hasChanged = false;
                scroll.transform.hasChanged = false;

                maxY = scrollMask.rect.yMax - (scroll.rect.height / 2);
                minY = scrollMask.rect.yMin + (scroll.rect.height / 2);
                scroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollMask.sizeDelta.x);
                scroll.localPosition = new Vector2(0, scroll.localPosition.y);
            }
        }
    }
}
