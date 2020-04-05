using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum RenScrollState
{
    None,
    AutoScroll,
    MouseWheel
}

[ExecuteAlways]
public class RenScroll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    RenController myRenCont;

    RenScrollState scrollSt = RenScrollState.None;
    public bool disabled = false;
    public int buttonsGroupNum;
    public float breakAcc;
    public float scrollAcc;
    float currentScrollSpeed;

    float currentY;
    float maxY;
    float minY;
    RectTransform scrollMask;
    public RectTransform scroll;
    bool isMouseOver = false;

    [Header("Auto Scroll")]
    public float autoScrollMaxTime = 0.3f;
    float autoScrollTime = 0;
    float autoScrollTargetY = 0;
    float autoScrollStartY = 0;
    float autoScrollVal = 0;
    public float margin = 10;


    private void Awake()
    {
        if (Application.isPlaying)
        {
            Debug.Log("AWAKE");
            Canvas myCanvas = GetComponentInParent<Canvas>();
            if (myCanvas == null)
            {
                Debug.LogError("RenButton " + name + " : this button has no parent canvas!");
            }
            myRenCont = myCanvas.GetComponentInChildren<RenController>();
            if (myRenCont == null)
            {
                Debug.LogError("RenButton " + name + " : this button has no RenController in its canvas!");
            }

            //set to top of scroll
            if (scrollMask == null) scrollMask = GetComponent<RectTransform>();
            maxY = scrollMask.rect.yMax - (scroll.rect.height / 2);
            minY = scrollMask.rect.yMin + (scroll.rect.height / 2);
            scroll.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, scrollMask.sizeDelta.x);
            currentY = maxY;
            scroll.localPosition = new Vector2(0, currentY);
            Debug.Log("maxY = "+maxY+"; minY = "+minY+"; scroll.localPosition = " + scroll.localPosition);
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
            if (transform.hasChanged || scroll.transform.hasChanged)
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
                if (myRenCont.currentControls.EAIMouseWheel.PosWasPressed)
                {
                    Debug.LogWarning("SCROLL UP");
                    currentScrollAcc = myRenCont.currentControls.EAIMouseWheel.PosWasPressedGreatly ? -scrollAcc * 6 : -scrollAcc;
                    StartMouseScrolling(currentScrollAcc);
                }
                else if (myRenCont.currentControls.EAIMouseWheel.NegWasPressed)
                {
                    Debug.LogWarning("SCROLL DOWN");
                    currentScrollAcc = myRenCont.currentControls.EAIMouseWheel.NegWasPressedGreatly ? scrollAcc * 6 : scrollAcc;
                    StartMouseScrolling(currentScrollAcc);
                }
            }

            if(scrollSt != RenScrollState.MouseWheel)
            {
                float dist;
                if (CheckIfButtonOutOfMask(out dist))
                {
                    StartAutoScrolling(scroll.localPosition.y - (dist - (scrollMask.rect.height/2 * Mathf.Sign(dist))));
                }
            }

            switch (scrollSt)
            {
                case RenScrollState.MouseWheel:
                    //Scroll acceleration
                    currentY += currentScrollSpeed * Time.deltaTime;
                    currentY = Mathf.Clamp(currentY, maxY, minY);
                    scroll.localPosition = new Vector2(scroll.localPosition.x, currentY);

                    float oldScrollSpeed = currentScrollSpeed;
                    //BREAK 
                    if (currentScrollAcc == 0) currentScrollSpeed += currentScrollSpeed > 0 ? -breakAcc * Time.deltaTime : currentScrollSpeed < 0 ? breakAcc * Time.deltaTime : 0;
                    currentScrollSpeed = oldScrollSpeed > 0 ? Mathf.Clamp(currentScrollSpeed, 0, float.MaxValue) : oldScrollSpeed < 0 ? Mathf.Clamp(currentScrollSpeed, float.MinValue, 0) : 0;
                    break;
                case RenScrollState.AutoScroll:
                    ProcessAutoScrolling();
                    break;
            }
        }
    }

    void StartMouseScrolling(float scrollAcceleration)
    {
        scrollSt = RenScrollState.MouseWheel;
        if (Mathf.Sign(scrollAcceleration) == Mathf.Sign(currentScrollSpeed)) currentScrollSpeed += scrollAcceleration;
        else currentScrollSpeed = scrollAcceleration;
    }

    void StopMouseScrolling()
    {
        if (scrollSt == RenScrollState.MouseWheel)
        {
            scrollSt = RenScrollState.None;
            currentScrollSpeed = 0;
        }
    }

    void StartAutoScrolling(float targetY)
    {
        scrollSt = RenScrollState.AutoScroll;
        autoScrollTargetY = Mathf.Clamp(targetY, maxY, minY);
        autoScrollStartY = scroll.localPosition.y;
        autoScrollVal = 0;
        autoScrollTime = 0;
    }

    void ProcessAutoScrolling()
    {
        if(scrollSt == RenScrollState.AutoScroll)
        {
            if (autoScrollVal >= 1) return;
            autoScrollTime += Time.deltaTime;
            autoScrollVal = Mathf.Clamp01(autoScrollTime / autoScrollMaxTime);
            float y = EasingFunction.EaseOutQuart(autoScrollStartY, autoScrollTargetY, autoScrollVal);
            scroll.localPosition = new Vector2(0, y);
        }
    }

    /// <summary>
    /// Returns distance to button selected if it is out of the mask.
    /// </summary>
    /// <returns></returns>
    bool CheckIfButtonOutOfMask(out float distance)
    {
        distance = -1;
        if (myRenCont.currentButton == null) return false;

        ButtonGroup buttonGroup = myRenCont.GetGroup(buttonsGroupNum);
        if (buttonGroup != null)
        {
            RenButton button = myRenCont.currentButton;
            if (myRenCont.currentButton.buttonGroup != buttonsGroupNum)
            {
                return false;
            }
            if (button.transform.parent != scroll)
            {
                return false;
            }
            distance = button.GetComponent<RectTransform>().localPosition.y + scroll.localPosition.y;
            distance += ((button.GetComponent<RectTransform>().rect.height / 2) + margin) * Mathf.Sign(distance) ;
            //distance += scrollMask.rect.height / 2 * -Mathf.Sign(distance);
            Debug.Log("CheckIfButtonOutOfMask: distance = " + distance);
            if (Mathf.Abs(distance) > scrollMask.rect.height / 2)
            {
                Debug.Log("Button is out of mask!");
                return true;
            }
        }
        return false;
    }

    //MOUSE EVENTS
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {
            isMouseOver = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {
            isMouseOver = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {

        }
    }
}
