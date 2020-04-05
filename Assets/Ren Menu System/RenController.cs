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
    //public static RenController instance;
    [HideInInspector] public bool disabled = false;
    public bool useMouse = false;
    public bool useControllers = true;
    public bool useKeyboard = true;
    public InControlManager inControlManager;
    [Tooltip("Only Anyone is working right now :( ")]
    public ButtonControlsMode controlsType;


    [Range(0, 1)]
    public float deadZone = 0.5f;
    public float turboFrequency = 0.4f;
    [Header(" -- BUTTON NAVIGATION --")]
    public RenButton initialButton;
    [Tooltip("If next button on the right/left/up/down direction is disabled, should we find a new button to go to, or just not move? Basically, jump through disabled buttons or not.")]
    public bool automaticButtonFlowOnError;
    public bool verticalJump = true;
    public bool horizontalJump = true;
    [Tooltip("Minimum horizontal &/or vertical distance to consider valid a button to set it as nextButton in the automatic navigation mode of buttons." +
        " Don't touch this if you don't know what you are doing you fool!")]
    public float minDist = 25;
    public float maxPerpDist = 200;

    [HideInInspector] public RenButton currentButton;
    [HideInInspector] public PlayerActions currentControls;
    public List<ButtonGroup> buttonGroups;

    public delegate void RenInputPress();
    public static event RenInputPress OnRenInputPressed;

    bool validInput
    {
        get
        {
            return ((currentControls.LastDeviceClass == InputDeviceClass.Controller && useControllers) || (currentControls.LastDeviceClass == InputDeviceClass.Keyboard && useKeyboard));
        }
    }

    private void Awake()
    {
        //instance = this;
        if (inControlManager == null)
        {
            //Debug.LogWarning("RenController warning: RenController needs a GameObject with InControlManager script on it to control buttons," +
            //    " for now the variable is empty, trying to find a InControlManager in the scene...");
            inControlManager = FindObjectOfType<InControlManager>();
            if (inControlManager == null) Debug.LogError("RenController error: no InControlManager could be found! Ignore this message if loading from map level / not from main menu.");
        }
        currentButton = initialButton;
        buttonGroups = new List<ButtonGroup>();
        CreateGroup(0);
    }

    private void Start()
    {
        currentControls = PlayerActions.CreateDefaultMenuBindings(deadZone, turboFrequency);
        if (inControlManager == null)
        {
            Debug.LogWarning("RenController warning: RenController needs a GameObject with InControlManager script on it to control buttons," +
                " for now the variable is empty, trying to find a InControlManager in the scene...");
            inControlManager = FindObjectOfType<InControlManager>();
            if (inControlManager == null) Debug.LogError("RenController error: no InControlManager could be found!");
        }
        if (initialButton == null)
        {
            ButtonGroup standardGroup = GetGroup(0);
            if (standardGroup != null && standardGroup.buttons.Count > 0)
            {
                initialButton = standardGroup.buttons[0];
            }
        }
        if (initialButton != null)
        {
            currentButton = initialButton;
            initialButton.HighlightButtonsAndTexts();
        }
    }

    private void OnEnable()
    {
        if (inControlManager == null)
        {
            Debug.LogWarning("RenController warning: RenController needs a GameObject with InControlManager script on it to control buttons," +
                " for now the variable is empty, trying to find a InControlManager in the scene...");
            inControlManager = FindObjectOfType<InControlManager>();
            if (inControlManager == null) Debug.LogError("RenController error: no InControlManager could be found! Ignore this message if loading from map level / not from main menu.");
        }
    }

    private void Update()
    {

        if (!disabled)
        {
            if ((currentControls.leftJoystcikAsButtons.RightWasPressed) && validInput)
            {
                if (OnRenInputPressed != null) OnRenInputPressed();
                MoveRight();
            }
            else if ((currentControls.leftJoystcikAsButtons.LeftWasPressed) && validInput)
            {
                if (OnRenInputPressed != null) OnRenInputPressed();
                MoveLeft();
            }
            else if ((currentControls.leftJoystcikAsButtons.UpWasPressed) && validInput)
            {
                if (OnRenInputPressed != null) OnRenInputPressed();
                MoveUp();
            }
            else if ((currentControls.leftJoystcikAsButtons.DownWasPressed) && validInput)
            {
                if (OnRenInputPressed != null) OnRenInputPressed();
                MoveDown();
            }
            else if (currentControls.A.WasPressed && validInput)
            {
                if (OnRenInputPressed != null) OnRenInputPressed();
                PressButton();
            }
            else if (currentControls.A.WasReleased && validInput)
            {
                if (OnRenInputPressed != null) OnRenInputPressed();
                ReleaseButton();
            }
            currentControls.leftJoystcikAsButtons.ResetJoyStick();
        }
    }

    #region --- Button Navigation ---

    public GameObject InstantiateButton(GameObject buttonPrefab, Vector3 pos, Quaternion rot, Transform parent, int buttonGroup)
    {
        GameObject auxButtonObject = Instantiate(buttonPrefab, pos, rot, parent);
        RenButton renButton = auxButtonObject.GetComponent<RenButton>();
        if (renButton == null)
        {
            Debug.LogError("This button has no RenButton script!");
            return null;
        }
        AddButton(buttonGroup, renButton);
        renButton.buttonGroup = buttonGroup;
        return auxButtonObject;
    }

    public void DestroyButton(RenButton renButton)
    {
        ButtonGroup buttonGroup = GetGroup(renButton.buttonGroup);
        for (int i = 0; i < buttonGroup.buttons.Count; i++)
        {
            if (buttonGroup.buttons[i] == renButton)
            {
                Destroy(buttonGroup.buttons[i]);
                buttonGroup.buttons.RemoveAt(i);

            }
        }
    }

    public void AddButton(int group, RenButton renButton)
    {
        bool found = false;
        ButtonGroup buttonGroup = GetGroup(group);
        if (buttonGroup == null)//create new group
        {
            buttonGroup = CreateGroup(group);
        }
        buttonGroup.buttons.Add(renButton);
    }

    ButtonGroup CreateGroup(int group)
    {
        for (int i = 0; i < buttonGroups.Count; i++)
        {
            if (buttonGroups[i].groupNumber == group) return null;
        }
        ButtonGroup newGroup = new ButtonGroup(group);
        buttonGroups.Add(newGroup);
        return newGroup;
    }

    public ButtonGroup GetGroup(int groupNumber)
    {
        for (int i = 0; i < buttonGroups.Count; i++)
        {
            if (buttonGroups[i].groupNumber == groupNumber) return buttonGroups[i];
        }
        return null;
    }

    public void SetButtonNavigation(RenButton renButton)
    {
        ButtonGroup buttonGroup = GetGroup(renButton.buttonGroup);
        if (buttonGroup == null)
        {
            Debug.LogWarning("The button " + renButton.name + " is trying to set up its Button Navigation (because it is on automatic), but the buttonGroup " + renButton.buttonGroup + " doesn't exist.");
            return;
        }
        RenButton right, left, up, down;
        right = left = up = down = null;
        float rightDist, leftDist, upDist, downDist;
        rightDist = leftDist = upDist = downDist = float.MaxValue;

        #region -- Initial Connections --
        for (int i = 0; i < buttonGroup.buttons.Count; i++)
        {
            if (buttonGroup.buttons[i].disabled || buttonGroup.buttons[i] == renButton) continue;

            Vector3 distVector = buttonGroup.buttons[i].transform.position - renButton.transform.position;
            //Debug.Log("SetButtonNavigation: objective = " + buttonGroup.buttons[i].name + "; distVector = " + distVector);
            //Right
            if (distVector.x > 0)
            {
                if (Mathf.Abs(distVector.x) > minDist && distVector.magnitude < rightDist)
                {
                    right = buttonGroup.buttons[i];
                    rightDist = distVector.magnitude;
                }
            }
            //Left
            if (distVector.x < 0)
            {
                if (Mathf.Abs(distVector.x) > minDist && distVector.magnitude < leftDist)
                {
                    left = buttonGroup.buttons[i];
                    leftDist = distVector.magnitude;
                }
            }
            //Up
            if (distVector.y > 0)
            {
                if (Mathf.Abs(distVector.y) > minDist && distVector.magnitude < upDist)
                {
                    up = buttonGroup.buttons[i];
                    upDist = distVector.magnitude;
                }
            }
            //Down
            if (distVector.y < 0)
            {
                if (Mathf.Abs(distVector.y) > minDist && distVector.magnitude < downDist)
                {
                    down = buttonGroup.buttons[i];
                    downDist = distVector.magnitude;
                }
            }
        }
        #endregion

        #region --Jump Connections --
        RenButton jumpLeft, jumpRight, jumpUp, jumpDown;
        jumpLeft = jumpRight = jumpDown = jumpUp = null;
        if ((verticalJump && (up == null || down == null)) || (horizontalJump && (right == null || left == null)))
        {
            // Debug.Log("Look for button Jump");
            rightDist = leftDist = upDist = downDist = float.MinValue;
            float minY, maxY, minX, maxX;
            minY = down == null ? -maxPerpDist : down.transform.position.y - renButton.transform.position.y; minY = Mathf.Clamp(minY, float.MinValue, -maxPerpDist);
            maxY = up == null ? maxPerpDist : up.transform.position.y - renButton.transform.position.y; maxY = Mathf.Clamp(maxY, maxPerpDist, float.MaxValue);
            minX = left == null ? -maxPerpDist : left.transform.position.x - renButton.transform.position.x; minX = Mathf.Clamp(minX, float.MinValue, -maxPerpDist);
            maxX = right == null ? maxPerpDist : right.transform.position.x - renButton.transform.position.x; maxX = Mathf.Clamp(maxX, maxPerpDist, float.MaxValue);

            for (int i = 0; i < buttonGroup.buttons.Count; i++)
            {
                if (buttonGroup.buttons[i].disabled || buttonGroup.buttons[i] == renButton) continue;

                Vector3 targetPos = buttonGroup.buttons[i].transform.position;
                Vector3 distVector = targetPos - renButton.transform.position;
                //Debug.Log("SetButtonNavigation: objective = " + buttonGroup.buttons[i].name + "; distVector = " + distVector);
                //Right
                if (horizontalJump && right == null && distVector.x < 0)
                {
                    if (distVector.y < maxY && distVector.y > minY && Mathf.Abs(distVector.x) > rightDist)
                    {
                        jumpRight = buttonGroup.buttons[i];
                        rightDist = Mathf.Abs(distVector.x);
                    }
                }
                //Left
                if (horizontalJump && left == null && distVector.x > 0)
                {
                    if (distVector.y < maxY && distVector.y > minY && Mathf.Abs(distVector.x) > leftDist)
                    {
                        jumpLeft = buttonGroup.buttons[i];
                        leftDist = Mathf.Abs(distVector.x);
                    }
                }
                //Up
                if (verticalJump && up == null && distVector.y < 0)
                {
                    if (distVector.x < maxX && distVector.x > minX && Mathf.Abs(distVector.y) > upDist)
                    {
                        jumpUp = buttonGroup.buttons[i];
                        upDist = Mathf.Abs(distVector.y);
                    }
                }
                //Down
                if (verticalJump && down == null && distVector.y > 0)
                {
                    if (distVector.x < maxX && distVector.x > minX && Mathf.Abs(distVector.y) > downDist)
                    {
                        //Debug.Log("Down Jump Button found");
                        jumpDown = buttonGroup.buttons[i];
                        downDist = Mathf.Abs(distVector.y);
                    }
                }
            }
        }
        #endregion

        string nullGameObject = "null gameObject";
        //Debug.Log("SetNextButtons: right = " + (right == null ? null : (right.gameObject == null ? nullGameObject : right.gameObject.name)));
        //Debug.Log("SetNextButtons: left = " + (left == null ? null : (left.gameObject == null ? nullGameObject : left.gameObject.name)));
        //Debug.Log("SetNextButtons: up = " + (up == null ? null : (up.gameObject == null ? nullGameObject : up.gameObject.name)));
        //Debug.Log("SetNextButtons: down = " + (down == null ? null : (down.gameObject == null ? nullGameObject : down.gameObject.name)));
        //+ "; left = " + left == null ? null : left.gameObject +"; up = " + up == null ? null : up.gameObject + "; down = " + down == null ? null : down.gameObject);
        renButton.nextRightButton = right != null ? right : jumpRight;
        renButton.nextLeftButton = left != null ? left : jumpLeft;
        renButton.nextUpButton = up != null ? up : jumpUp;
        renButton.nextDownButton = down != null ? down : jumpDown;
    }

    #endregion

    #region --- Button Interaction ---
    void PressButton()
    {
        if (currentButton == null) return;
        currentButton.PressButtonsAndText();
    }

    void ReleaseButton()
    {
        if (currentButton == null) return;
        currentButton.ReleaseButtonsAndText();
    }

    void MoveRight()
    {
        if (currentButton == null)
        {
            currentButton = initialButton;
            currentButton.HighlightButtonsAndTexts();
        }
        else if (currentButton.nextRightButton != null)
        {
            List<RenButton> listOfSeenButtons = new List<RenButton>();

            RenButton nextButton = null;
            if (currentButton.nextRightButton.disabled && automaticButtonFlowOnError)
            {
                Debug.LogWarning("next right Button is disabled and automaticButtonFlowOnError is on");
                RenButton auxCurrentButton = currentButton.nextRightButton;
                int i = 0;
                while (auxCurrentButton.disabled || i > 5)
                {
                    if (!listOfSeenButtons.Contains(auxCurrentButton))
                    {
                        listOfSeenButtons.Add(auxCurrentButton);
                        if (auxCurrentButton.nextRightButton != null)
                        {
                            auxCurrentButton = auxCurrentButton.nextRightButton;
                            if (!auxCurrentButton.disabled) nextButton = auxCurrentButton;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    i++;
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
        if (currentButton == null)
        {
            currentButton = initialButton;
            currentButton.HighlightButtonsAndTexts();
        }
        else if (currentButton.nextLeftButton != null)
        {
            List<RenButton> listOfSeenButtons = new List<RenButton>();

            RenButton nextButton = null;
            if (currentButton.nextLeftButton.disabled && automaticButtonFlowOnError)
            {
                Debug.LogWarning("next left Button is disabled and automaticButtonFlowOnError is on");
                RenButton auxCurrentButton = currentButton.nextLeftButton;
                int i = 0;
                while (auxCurrentButton.disabled || i > 5)
                {
                    if (!listOfSeenButtons.Contains(auxCurrentButton))
                    {
                        listOfSeenButtons.Add(auxCurrentButton);
                        if (auxCurrentButton.nextLeftButton != null)
                        {
                            auxCurrentButton = auxCurrentButton.nextLeftButton;
                            if (!auxCurrentButton.disabled) nextButton = auxCurrentButton;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    i++;
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
        if (currentButton == null)
        {
            currentButton = initialButton;
            currentButton.HighlightButtonsAndTexts();
        }
        else if (currentButton.nextUpButton != null)
        {
            //Debug.Log("Move up");
            List<RenButton> listOfSeenButtons = new List<RenButton>();

            RenButton nextButton = null;
            if (currentButton.nextUpButton.disabled && automaticButtonFlowOnError)
            {
                Debug.LogWarning("next up Button is disabled and automaticButtonFlowOnError is on");
                RenButton auxCurrentButton = currentButton.nextUpButton;
                int i = 0;
                while (auxCurrentButton.disabled || i > 5)
                {
                    if (!listOfSeenButtons.Contains(auxCurrentButton))
                    {
                        listOfSeenButtons.Add(auxCurrentButton);
                        if (auxCurrentButton.nextUpButton != null)
                        {
                            auxCurrentButton = auxCurrentButton.nextUpButton;
                            if (!auxCurrentButton.disabled) nextButton = auxCurrentButton;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                    i++;
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
        if (currentButton == null)
        {
            currentButton = initialButton;
            currentButton.HighlightButtonsAndTexts();
        }
        else if (currentButton.nextDownButton != null)
        {
            List<RenButton> listOfSeenButtons = new List<RenButton>();

            RenButton nextButton = null;
            if (currentButton.nextDownButton.disabled && automaticButtonFlowOnError)
            {
                Debug.LogWarning("next down Button is disabled and automaticButtonFlowOnError is on");
                RenButton auxCurrentButton = currentButton.nextDownButton;
                int i = 0;
                while (auxCurrentButton.disabled || i >= 5)
                {
                    Debug.Log("AuxCurrentButton = " + auxCurrentButton.gameObject.name + "; auxCurrentButton.nextDownButton = " + auxCurrentButton.nextDownButton.gameObject.name);
                    if (!listOfSeenButtons.Contains(auxCurrentButton))
                    {
                        listOfSeenButtons.Add(auxCurrentButton);
                        if (auxCurrentButton.nextDownButton != null)
                        {
                            auxCurrentButton = auxCurrentButton.nextDownButton;
                            if (!auxCurrentButton.disabled) nextButton = auxCurrentButton;
                        }
                        else
                        {
                            Debug.Log("BREAK: nextDownButton == null");
                            break;
                        }
                    }
                    else
                    {
                        Debug.Log("BREAK: listOfSeenButtons contains this button already");
                        break;
                    }
                    i++;

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
    #endregion

    public void SetSelectedButton(RenButton renButton)
    {
        currentButton.StopHighlightButtonsAndText();
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

public class ButtonGroup
{
    public int groupNumber;
    public List<RenButton> buttons;

    public ButtonGroup(int _groupNumber)
    {
        groupNumber = _groupNumber;
        buttons = new List<RenButton>();
    }
}
