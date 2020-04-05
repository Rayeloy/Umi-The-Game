using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;
using UnityEngine.EventSystems;
using UnityEngine.Events;


public enum TransitionMode
{
    None,
    ColorTint,
    SpriteSwap
    //Animation
}
public enum RenButtonNavigationMode
{
    Automatic,
    Manual
}
[ExecuteAlways]
public class RenButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    RenController myRenCont;

    public bool disabled = false;

    public TransitionMode transition = TransitionMode.ColorTint;
    public Image[] targetImages;
    [Header("TRANSITION: COLOR TINT")]
    public Text[] targetTexts;
    public Color normalColor;
    public Color highlightedColor;
    public Color pressedColor;
    public Color disabledColor;
    [Header("TRANSITION: SPRITE SWAP")]
    public Sprite normalSprite;
    public Sprite highlightedSprite;
    public Sprite pressedSprite;
    public Sprite disabledSprite;

    [Header("NAVIGATION")]
    public RenButtonNavigationMode navigationMode = RenButtonNavigationMode.Manual;
    public int buttonGroup = 0;
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
    public UnityEvent onButtonStopHighlight;
    public UnityEvent onButtonPressed;
    bool isMouseOver = false;

    bool lastEnabled;

    public bool isPartOfScroll
    {
        get
        {
            return GetComponentInParent<RenScroll>() != null;
        }
    }

    private void OnGUI()
    {

        if (!Application.isPlaying)
        {
            for (int i = 0; i < targetImages.Length; i++)
            {
                targetImages[i].color = normalColor;
            }
            //if (targetTexts.Length==0)
            //{
            //    Debug.Log("REN BUTTON ONGUI: create 1 space for targetTexts");
            //    targetTexts = new Text[1];
            //}
            //if (targetTexts[0] == null)
            //{
            //    Debug.Log("REN BUTTON ONGUI: find child text ");
            //    targetTexts[0] = gameObject.GetComponentInChildren<Text>();
            //}
        }
    }

    private void Awake()
    {
        Canvas myCanvas = GetComponentInParent<Canvas>();
        if(myCanvas == null)
        {
            Debug.LogError("RenButton "+name+" : this button has no parent canvas!");
        }
        myRenCont = myCanvas.GetComponentInChildren<RenController>();
        if (myRenCont == null)
        {
            Debug.LogError("RenButton " + name + " : this button has no RenController in its canvas!");
        }

        for (int i = 0; i < targetImages.Length; i++)
        {
            targetImages[i].color = normalColor;
        }
        if (Application.isPlaying)
        {
            myRenCont.AddButton(buttonGroup, this);
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            SetNextButtons();
            lastEnabled = this.enabled;
        }
    }

    private void Update()
    {
        if(this.enabled != lastEnabled)
        {
            if (this.enabled)
            {
                EnableButtonsAndText();
            }
            else
            {
                DisableButtonsAndText();
            }
        }
    }

    void SetNextButtons()
    {
        if(navigationMode == RenButtonNavigationMode.Automatic)
        {
            //Debug.Log(" --- Setting Button Navegation for " + name);
            myRenCont.SetButtonNavigation(this);
        }
    }


    private void OnDrawGizmos()
    {
        ShowNavigationGizmos();
    }

    void ShowNavigationGizmos()
    {
        Vector3 origin = transform.position;

        if (nextUpButton != null)
        {
            Vector3 auxOrigin = origin;
            auxOrigin.y += GetComponent<RectTransform>().rect.height / 2;
            Vector3 auxEnd = nextUpButton.transform.position;
            auxEnd.y -= nextUpButton.GetComponent<RectTransform>().rect.height / 2;
            GizmoTools.DrawCurveArrow(auxOrigin, auxEnd, Color.yellow, Color.blue, 4, 3, 20, 40);
        }
        if (nextLeftButton != null)
        {
            Vector3 auxOrigin = origin;
            auxOrigin.x -= GetComponent<RectTransform>().rect.width / 2;
            Vector3 auxEnd = nextLeftButton.transform.position;
            auxEnd.x += nextLeftButton.GetComponent<RectTransform>().rect.width / 2;
            GizmoTools.DrawCurveArrow(auxOrigin, auxEnd, Color.yellow, Color.blue, 4, 3, 20, 40);
        }
        if (nextRightButton != null)
        {
            Vector3 auxOrigin = origin;
            auxOrigin.x += GetComponent<RectTransform>().rect.width / 2;
            Vector3 auxEnd = nextRightButton.transform.position;
            auxEnd.x -= nextRightButton.GetComponent<RectTransform>().rect.width / 2;
            GizmoTools.DrawCurveArrow(auxOrigin, auxEnd, Color.yellow, Color.blue, 4, 3, 20, 40);
        }
        if (nextDownButton != null)
        {
            Vector3 auxOrigin = origin;
            auxOrigin.y -= GetComponent<RectTransform>().rect.height / 2;
            Vector3 auxEnd = nextDownButton.transform.position;
            auxEnd.y += nextDownButton.GetComponent<RectTransform>().rect.height / 2;
            GizmoTools.DrawCurveArrow(auxOrigin, auxEnd, Color.yellow, Color.blue, 4, 3, 20, 40);
        }
    }

    //MOUSE EVENTS
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {
            if (myRenCont.currentButton != null)
                myRenCont.currentButton.StopHighlightButtonsAndText();
            myRenCont.currentButton = this;

            isMouseOver = true;
            //Debug.Log("Mouse enter");
            //for (int i = 0; i < targetImages.Length; i++)
            //{
            //    targetImages[i].color = highlightedColor;
            //}
            HighlightButtonsAndTexts();

            onMouseEnter.Invoke();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {
            if (myRenCont.currentButton == this)
            {
                myRenCont.currentButton.StopHighlightButtonsAndText();
                myRenCont.currentButton = null;
            }
            isMouseOver = false;
            //Debug.Log("Mouse exit");
            //for (int i = 0; i < targetImages.Length; i++)
            //{
            //    targetImages[i].color = normalColor;
            //}
            onMouseExit.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {
            PressButtonsAndText();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!disabled && myRenCont.useMouse)
        {
            Debug.Log("Mouse up");
            ReleaseButtonsAndText();
            onButtonPressed.Invoke();
        }
    }

    //Main Functions
    public void StopHighlightButtonsAndText()
    {
        if (!disabled)
        {
            //Debug.Log("BUTTON STOP HIGHLIGHTED");
            switch (transition)
            {
                case TransitionMode.ColorTint:
                    for (int i = 0; i < targetImages.Length; i++)
                    {
                        targetImages[i].color = normalColor;
                    }
                    break;
                case TransitionMode.SpriteSwap:
                    targetImages[0].sprite = normalSprite;
                    break;
            }
            onButtonStopHighlight.Invoke();
        }
    }

    public void HighlightButtonsAndTexts()
    {
        if (!disabled)
        {
            //Debug.Log("BUTTON HIGHLIGHTED: transition mode = "+transition);
            switch (transition)
            {
                case TransitionMode.ColorTint:
                    for (int i = 0; i < targetImages.Length; i++)
                    {
                        targetImages[i].color = highlightedColor;
                    }
                    break;
                case TransitionMode.SpriteSwap:
                    targetImages[0].sprite = highlightedSprite;
                    break;
            }

            onButtonHighlight.Invoke();
        }
    }

    public void PressButtonsAndText()
    {
        if (!disabled)
        {
            switch (transition)
            {
                case TransitionMode.ColorTint:
                    for (int i = 0; i < targetImages.Length; i++)
                    {
                        targetImages[i].color = pressedColor;
                    }
                    break;
                case TransitionMode.SpriteSwap:
                    targetImages[0].sprite = pressedSprite;
                    break;
            }
            onClick.Invoke();
        }
    }

    public void ReleaseButtonsAndText()
    {
        if (!disabled)
        {
            //Debug.Log("BUTTON RELEASED");
            switch (transition)
            {
                case TransitionMode.ColorTint:
                    for (int i = 0; i < targetImages.Length; i++)
                    {
                        targetImages[i].color = highlightedColor;
                    }
                    break;
                case TransitionMode.SpriteSwap:
                    targetImages[0].sprite = highlightedSprite;
                    break;
            }
            onButtonPressed.Invoke();
        }
    }

    public void DisableButtonsAndText()
    {
        if (!disabled)
        {
            disabled = true;
            switch (transition)
            {
                case TransitionMode.ColorTint:
                    for (int i = 0; i < targetImages.Length; i++)
                    {
                        targetImages[i].color = disabledColor;
                    }
                    break;
                case TransitionMode.SpriteSwap:
                    targetImages[0].sprite = disabledSprite;
                    break;
            }
        }
    }

    public void EnableButtonsAndText()
    {
        if (disabled)
        {
            disabled = false;
            switch (transition)
            {
                case TransitionMode.ColorTint:
                    for (int i = 0; i < targetImages.Length; i++)
                    {
                        targetImages[i].color = normalColor;
                    }
                    break;
                case TransitionMode.SpriteSwap:
                    targetImages[0].sprite = normalSprite;
                    break;
            }
        }
    }
}
