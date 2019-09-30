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
    public bool useKeyboard;
    public InControlManager inControlManager;
    [Tooltip("Only Anyone is working right now")]
    public ButtonControlsMode controlsType;
    public RenButton initialButton;
    [Tooltip("If next button on the right/left/up/down direction is disabled, should we find a new button to go to, or just not move?")]
    public bool automaticButtonFlowOnError;
    [Range(0, 1)]
    public float deadZone = 0.5f;
    [HideInInspector] public RenButton currentButton;

    [HideInInspector] public PlayerActions currentControls;

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
        if(initialButton!=null)
        initialButton.StopHighlightButtonsAndText();
    }
    private void Start()
    {
        currentControls = PlayerActions.CreateDefaultMenuBindings(deadZone);

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
        if((currentControls.LastDeviceClass== InputDeviceClass.Controller && useControllers) || (currentControls.LastDeviceClass == InputDeviceClass.Keyboard && useKeyboard))
        {
            if (currentControls.leftJoystcikAsButtons.right.wasPressed || currentControls.leftJoystcikAsButtons.right.wasPressedLong)
            {
                MoveRight();
            }
            else if (currentControls.leftJoystcikAsButtons.left.wasPressed || currentControls.leftJoystcikAsButtons.left.wasPressedLong)
            {
                MoveLeft();
            }
            else if (currentControls.leftJoystcikAsButtons.up.wasPressed || currentControls.leftJoystcikAsButtons.up.wasPressedLong)
            {
                MoveUp();
            }
            else if (currentControls.leftJoystcikAsButtons.down.wasPressed || currentControls.leftJoystcikAsButtons.down.wasPressedLong)
            {
                MoveDown();
            }
            else if (currentControls.A.WasPressed)
            {
                PressButton();
            }
            else if (currentControls.A.WasReleased)
            {
                ReleaseButton();
            }
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
        if (currentButton.nextRightButton != null)
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
            if (nextButton != null)
            {
                currentButton.StopHighlightButtonsAndText();
                currentButton = nextButton;
                nextButton.HighlightButtonsAndTexts();
            }
        }   
    }

    void MoveLeft()
    {
        if (currentButton.nextLeftButton != null)
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
    }

    void MoveUp()
    {
        Debug.Log("CurrentButton = "+ currentButton + "; currentButton.nextUpButton = " + currentButton.nextUpButton);
        if (currentButton.nextUpButton != null)
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
    }

    void MoveDown()
    {
        if (currentButton.nextDownButton != null)
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
    }

    public void SetSelectedButton(RenButton renButton)
    {
        currentButton = renButton;
        currentButton.HighlightButtonsAndTexts();
    }

    public Image testImage;
    public void TestChangeColorImage()
    {
        Debug.Log("TEST COLOR CHANGE");
        testImage.color = new Color(1, Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
    }
}
