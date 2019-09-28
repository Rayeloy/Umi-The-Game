using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum RenButtonNavigationMode
{
    Automatic,
    Manual
}
[ExecuteAlways]
public class RenButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image[] targetImages;
    public Text[] targetTexts;
    public Color normalColor;
    public Color highlightedColor;
    public Color PressedColor;
    public Color DisabledColor;

    [HideInInspector]public bool disabled = false;

    [Header("NAVIGATION")]
    public RenButtonNavigationMode navigationMode;
    public RenButton nextUpButton;
    public RenButton nextDownButton;
    public RenButton nextRightButton;
    public RenButton nextLeftButton;

    //public EventTrigger eventTrigger;
    //public delegate void VoidFunction();
    //public VoidFunction inputVoidFunction;
    public UnityEvent onClick;
    public UnityEvent onMouseEnter;
    public UnityEvent onMouseExit;
    public UnityEvent onButtonHighlight;
    public UnityEvent onButtonPressed;
    public bool isMouseOver = false;

    private void OnGUI()
    {
        //Debug.Log("REN BUTTON ONGUI: SET NORMAL COLOR");
        if (!Application.isPlaying)
        {
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = normalColor;
            }
        }
    }

    private void Awake()
    {
        for (int i = 0; i < targetImages.Length; i++)
        {
            targetImages[i].color = normalColor;
        }
    }

    //MOUSE EVENTS
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {
            isMouseOver = true;
            Debug.Log("Mouse enter");
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = highlightedColor;
            }
            onMouseEnter.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {
            isMouseOver = false;
            Debug.Log("Mouse exit");
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = normalColor;
            }
            onMouseExit.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {
            PressButtonsAndText();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!disabled && RenController.instance.useMouse)
        {
            Debug.Log("Mouse up");
            for (int i = 0; i < targetImages.Length; i++)
            {
                if (isMouseOver)
                {
                    targetImages[i].color = highlightedColor;

                }
                else
                {
                    targetImages[i].color = normalColor;
                }
            }
            onButtonPressed.Invoke();
        }
    }

    //Main Functions
    public void StopHighlightButtonsAndText()
    {
        if (!disabled)
        {
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = normalColor;
            }
        }
    }

    public void HighlightButtonsAndTexts()
    {
        if (!disabled)
        {
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = highlightedColor;
            }
            onButtonHighlight.Invoke();
        }
    }

    public void PressButtonsAndText()
    {
        if (!disabled)
        {
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = PressedColor;
            }
            onClick.Invoke();
        }
    }

    public void ReleaseButtonsAndText()
    {
        if (!disabled)
        {
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = highlightedColor;
            }
            Debug.Log("BUTTON RELEASED");
            onButtonPressed.Invoke();
        }
    }

    public void DisableButtonsAndText()
    {
        if (!disabled)
        {
            disabled = true;
        }
    }

}
