using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectPlayerCanvas : MonoBehaviour
{
    private Camera myCamera;
    public RectTransform teamSelectHUDParent;
    public RectTransform insideUI;
    public Image teamIcon;
    public Image teamNameBackground;
    public Text teamNameText;
    public Image[] lockStateDeactivateImages;
    public Image[] lockStateActivateImages;
    public Image leftArrow;
    public Image rightArrow;
    [Range(0, 1)]
    public float alphaDeactivated = 0.5f;

    public Sprite[] teamIcons;
    public Sprite[] teamBackgrounds;
    public string[] teamTexts;

    public UIAnimation startCharSelectionInsideUI;
    public UIAnimation startCharSelectionLeftArrow;
    public UIAnimation startCharSelectionRightArrow;

    private void Awake()
    {
        myCamera = GetComponent<Canvas>().worldCamera;
    }

    public void StartTeamSelection()
    {
        UnlockTeam();
    }

    public void GoBackToTeamSelection()
    {
        UnlockTeam();
        GameInfo.instance.StartAnimation(startCharSelectionInsideUI, myCamera, true);
        GameInfo.instance.StartAnimation(startCharSelectionLeftArrow, myCamera, true);
        GameInfo.instance.StartAnimation(startCharSelectionRightArrow, myCamera, true);
    }

    void LockTeam()
    {
        for (int i = 0; i < lockStateDeactivateImages.Length; i++)
        {
            lockStateDeactivateImages[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < lockStateActivateImages.Length; i++)
        {
            lockStateActivateImages[i].gameObject.SetActive(true);
        }
    }

    void UnlockTeam()
    {
        for (int i = 0; i < lockStateDeactivateImages.Length; i++)
        {
            lockStateDeactivateImages[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < lockStateActivateImages.Length; i++)
        {
            lockStateActivateImages[i].gameObject.SetActive(false);
        }
    }

    public void StartCharacterSelection()
    {
        LockTeam();
        GameInfo.instance.StartAnimation(startCharSelectionInsideUI, myCamera);
        GameInfo.instance.StartAnimation(startCharSelectionLeftArrow, myCamera);
        GameInfo.instance.StartAnimation(startCharSelectionRightArrow, myCamera);
    }

}
