using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InControl;

public enum ButtonControlsMode
{
    Anyone,
    FirstToInput
}
public class RenController : MonoBehaviour
{
    public static RenController instance;
    public bool useMouse = true;
    public bool useControllers;
    public InControlManager inControlManager;
    public ButtonControlsMode controlsType;
    public RenButton initialButton;
    [Tooltip("If there is no next button on the right/left/up/down direction, should we find a new button to go to, or just not move?")]
    public bool automaticButtonFlowOnError;
    [Range(0, 1)]
    public float deadZone = 0.5f;
    RenButton currentButton;

    PlayerActions currentControls;

    private void Awake()
    {
        instance = this;
        if (inControlManager == null)
        {
            Debug.LogWarning("RenController warning: RenController needs a GameObject with InControlManager script on it to control buttons," +
                " for now the variable is empty, trying to find a InControlManager in the scene...");
            inControlManager = FindObjectOfType<InControlManager>();
            if (inControlManager == null) Debug.LogError("RenController error: no InControlManager could be found!");
        }
        currentButton = initialButton;
    }
    private void Start()
    {
        currentControls = PlayerActions.CreateDefaultMenuBindings();
    }

    private void OnEnable()
    {
        if (inControlManager == null)
        {
            Debug.LogWarning("RenController warning: RenController needs a GameObject with InControlManager script on it to control buttons," +
                " for now the variable is empty, trying to find a InControlManager in the scene...");
            inControlManager = FindObjectOfType<InControlManager>();
            if (inControlManager == null) Debug.LogError("RenController error: no InControlManager could be found!");
        }
    }

    private void Update()
    {

        if(currentControls.LeftJoystick.Right.WasPressed)
        {
            MoveRight();
        }
        else if(currentControls.LeftJoystick.Left.WasPressed)
        {
            MoveLeft();
        }
        else if (currentControls.LeftJoystick.Up.WasPressed)
        {
            MoveUp();
        }
        else if (currentControls.LeftJoystick.Down.WasPressed)
        {
            MoveDown();
        }
        else if (currentControls.A.WasPressed)
        {
            PressButton();
        }else if (currentControls.A.WasReleased)
        {
            ReleaseButton();
        }
    }

    void PressButton()
    {
        currentButton.PressButtonsAndText();
    }

    void ReleaseButton()
    {
        currentButton.ReleaseButtonsAndText();
    }

    void MoveRight()
    {
        List<RenButton> listOfSeenButtons = new List<RenButton>();

        RenButton nextButton = null;
        if (currentButton.nextRightButton.disabled && automaticButtonFlowOnError)
        {
            RenButton auxCurrentButton = currentButton.nextRightButton;
            while (auxCurrentButton.disabled)
            {
                if (!listOfSeenButtons.Contains(auxCurrentButton))
                {
                    listOfSeenButtons.Add(auxCurrentButton);
                    if (auxCurrentButton.nextRightButton != null)
                    {
                        nextButton = auxCurrentButton.nextRightButton;
                    }
                    else
                    {
                        auxCurrentButton = auxCurrentButton.nextRightButton;
                    }
                }
            }
        }
        else
        {
            nextButton = currentButton.nextRightButton;
        }
        if (nextButton!=null)
        {
            currentButton.StopHighlightButtonsAndText();
            currentButton = nextButton;
            nextButton.HighlightButtonsAndTexts();
        }
    }

    void MoveLeft()
    {
        List<RenButton> listOfSeenButtons = new List<RenButton>();

        RenButton nextButton = null;
        if (currentButton.nextLeftButton.disabled && automaticButtonFlowOnError)
        {
            RenButton auxCurrentButton = currentButton.nextLeftButton;
            while (auxCurrentButton.disabled)
            {
                if (!listOfSeenButtons.Contains(auxCurrentButton))
                {
                    listOfSeenButtons.Add(auxCurrentButton);
                    if (auxCurrentButton.nextLeftButton != null)
                    {
                        nextButton = auxCurrentButton.nextLeftButton;
                    }
                    else
                    {
                        auxCurrentButton = auxCurrentButton.nextLeftButton;
                    }
                }
            }
        }
        else
        {
            nextButton = currentButton.nextLeftButton;
        }
        if (nextButton != null)
        {
            currentButton.StopHighlightButtonsAndText();
            currentButton = nextButton;
            nextButton.HighlightButtonsAndTexts();
        }
    }

    void MoveUp()
    {
        List<RenButton> listOfSeenButtons = new List<RenButton>();

        RenButton nextButton = null;
        if (currentButton.nextUpButton.disabled && automaticButtonFlowOnError)
        {
            RenButton auxCurrentButton = currentButton.nextUpButton;
            while (auxCurrentButton.disabled)
            {
                if (!listOfSeenButtons.Contains(auxCurrentButton))
                {
                    listOfSeenButtons.Add(auxCurrentButton);
                    if (auxCurrentButton.nextUpButton != null)
                    {
                        nextButton = auxCurrentButton.nextUpButton;
                    }
                    else
                    {
                        auxCurrentButton = auxCurrentButton.nextUpButton;
                    }
                }
            }
        }
        else
        {
            nextButton = currentButton.nextUpButton;
        }
        if (nextButton != null)
        {
            currentButton.StopHighlightButtonsAndText();
            currentButton = nextButton;
            nextButton.HighlightButtonsAndTexts();
        }
    }

    void MoveDown()
    {
        List<RenButton> listOfSeenButtons = new List<RenButton>();

        RenButton nextButton = null;
        if (currentButton.nextDownButton.disabled && automaticButtonFlowOnError)
        {
            RenButton auxCurrentButton = currentButton.nextDownButton;
            while (auxCurrentButton.disabled)
            {
                if (!listOfSeenButtons.Contains(auxCurrentButton))
                {
                    listOfSeenButtons.Add(auxCurrentButton);
                    if (auxCurrentButton.nextDownButton != null)
                    {
                        nextButton = auxCurrentButton.nextDownButton;
                    }
                    else
                    {
                        auxCurrentButton = auxCurrentButton.nextDownButton;
                    }
                }
            }
        }
        else
        {
            nextButton = currentButton.nextDownButton;
        }
        if (nextButton != null)
        {
            currentButton.StopHighlightButtonsAndText();
            currentButton = nextButton;
            nextButton.HighlightButtonsAndTexts();
        }
    }

    public Image testImage;
    public void TestChangeColorImage()
    {
        Debug.Log("TEST COLOR CHANGE");
        testImage.color = new Color(1, Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
    }
}
