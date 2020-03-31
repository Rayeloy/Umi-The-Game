using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[ExecuteAlways]
public class RenScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public bool disabled = false;
    public int buttonsGroup;
    public float breakAcc;
    public float scrollAcc;
    float currentScrollSpeed;

    float currentY;
    float maxY;
    float minY;
    RectTransform scrollMask;
    public RectTransform scroll;
    bool isMouseOver = false;
    bool mouseScrolling = false;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            Debug.Log("AWAKE");
            //set to top of scroll
            if (scrollMask == null) scrollMask = GetComponent<RectTransform>();
            maxY = scrollMask.rect.yMax - (scroll.rect.height / 2);
            minY = scrollMask.rect.yMin + (scroll.rect.height / 2);
            scroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollMask.sizeDelta.x);
            scroll.localPosition = new Vector2(0, scroll.localPosition.y);
            currentY = maxY;
        }
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("ONENABLE");
            if (scrollMask == null) scrollMask = GetComponent<RectTransform>();
            maxY = scrollMask.rect.yMax - (scroll.rect.height / 2);
            minY = scrollMask.rect.yMin + (scroll.rect.height / 2);
            scroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollMask.sizeDelta.x);
            scroll.localPosition = new Vector2(0, scroll.localPosition.y);
        }
        else
        {
            RenController.OnRenInputPressed +=StopMouseScrolling;
        }
    }

    private void OnDisable()
    {
        if (!Application.isPlaying)
        {

        }
        else
        {
            RenController.OnRenInputPressed -= StopMouseScrolling;
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            if(transform.hasChanged || scroll.transform.hasChanged)
            {
                Debug.Log("UPDATE");
                transform.hasChanged = false;
                scroll.transform.hasChanged = false;

                maxY = scrollMask.rect.yMax - (scroll.rect.height / 2);
                minY = scrollMask.rect.yMin + (scroll.rect.height / 2);
                scroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollMask.sizeDelta.x);
                scroll.localPosition = new Vector2(0, scroll.localPosition.y);
            }
        }
        else
        {
            float currentScrollAcc = 0;
            if (isMouseOver)
            {
                if (RenController.instance.currentControls.EAIMouseWheel.PosWasPressed)
                {
                    Debug.LogWarning("SCROLL UP");
                    currentScrollAcc = RenController.instance.currentControls.EAIMouseWheel.PosWasPressedGreatly ? -scrollAcc * 6 : -scrollAcc;
                    StartMouseScrolling(currentScrollAcc);
                }
                else if (RenController.instance.currentControls.EAIMouseWheel.NegWasPressed)
                {
                    Debug.LogWarning("SCROLL DOWN");
                    currentScrollAcc = RenController.instance.currentControls.EAIMouseWheel.NegWasPressedGreatly ? scrollAcc * 6 : scrollAcc;
                    StartMouseScrolling(currentScrollAcc);
                }
            }

            currentY += currentScrollSpeed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, maxY, minY);
            scroll.localPosition = new Vector2(scroll.localPosition.x, currentY);
            //Debug.Log("currentY = "+ currentY.ToString("F6") + "; currentScrollSpeed  = "+ currentScrollSpeed.ToString("F6") + "; scroll.localPosition.y  = " + scroll.localPosition.y);

            float oldScrollSpeed = currentScrollSpeed;
            //BREAK 
            if (currentScrollAcc == 0) currentScrollSpeed += currentScrollSpeed > 0 ? -breakAcc * Time.deltaTime : currentScrollSpeed < 0 ? breakAcc * Time.deltaTime : 0;
            currentScrollSpeed = oldScrollSpeed > 0 ? Mathf.Clamp(currentScrollSpeed, 0, float.MaxValue) : oldScrollSpeed < 0 ? Mathf.Clamp(currentScrollSpeed, float.MinValue, 0) : 0;
        }
    }

    void StartMouseScrolling(float scrollAcceleration)
    {
        mouseScrolling = true;
        if (Mathf.Sign(scrollAcceleration) == Mathf.Sign(currentScrollSpeed)) currentScrollSpeed += scrollAcceleration;
        else currentScrollSpeed = scrollAcceleration;
    }

    void StopMouseScrolling()
    {
        if (mouseScrolling)
        {
            mouseScrolling = false;
            currentScrollSpeed = 0;
        }
    }

    //MOUSE EVENTS
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {
            isMouseOver = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {
            isMouseOver = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {

        }
    }
}
