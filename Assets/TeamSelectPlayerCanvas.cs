using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectPlayerCanvas : MonoBehaviour
{
    public RectTransform teamSelectHUDParent;
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

}
