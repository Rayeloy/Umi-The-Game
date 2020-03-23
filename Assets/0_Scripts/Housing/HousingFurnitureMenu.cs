using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FurnitureMenuState
{
    family,
    furnitureType,
    furnitures
}
public class HousingFurnitureMenu : MonoBehaviour
{
    public GameObject furnitureMenuParent;
    public RectTransform scrollRect;
    public Vector2 furnitureIconSize;
    public float padding = 0.5f;
    public GameObject furnitureIconRenButtonPrefab;
    List<List<RenButton>> furnitureIcons;
    FurnitureMenuState furnitureMenuState = FurnitureMenuState.family;

    //OPEN / CLOSE ANIMATION 
    [Header(" - Open / Close Animation - ")]
    public float openCloseAnimMaxTime = 0.4f;
    public Transform menuPos;
    public Transform hiddenMenuPos;
    bool openMenuAnimStarted = false;
    bool closeMenuAnimStarted = false;
    float currentAnimVal = 0;
    float openCloseAnimTime = 0;
    float animStartingX = 0;

    public void KonoAwake()
    {
        furnitureMenuParent.SetActive(true);
        furnitureMenuParent.transform.position = hiddenMenuPos.position;
    }

    public void KonoUpdate()
    {
        ProcessOpenMenuAnim();
        ProcessCloseMenuAnim();
    }
   
    public void OpenFurnitureMenu()
    {
        furnitureIcons = new List<List<RenButton>>();
        furnitureMenuState = FurnitureMenuState.family;
        InstantiateRenButtons(FurnitureTag.chair);
        StartOpenMenuAnim();
    }

    public void CloseFurnitureMenu()
    {
        StartCloseMenuAnim();
    }

    #region --- Open / Close Animation ---
    //OPEN MENU ANIM
    void StartOpenMenuAnim()
    {
        if (!openMenuAnimStarted)
        {
            openMenuAnimStarted = true;
            closeMenuAnimStarted = false;
            openCloseAnimTime = 0;
            currentAnimVal = 0;
            animStartingX = furnitureMenuParent.transform.position.x;
        }
    }

    void ProcessOpenMenuAnim()
    {
        if (openMenuAnimStarted)
        {
            if (currentAnimVal >= 1) return;
            openCloseAnimTime += Time.deltaTime;
            currentAnimVal = Mathf.Clamp01(openCloseAnimTime / openCloseAnimMaxTime);
            float x = EasingFunction.EaseOutBack(animStartingX, menuPos.position.x, currentAnimVal);
            Vector3 currentMenuParentPos = new Vector3(x, menuPos.position.y, 0);
            furnitureMenuParent.transform.position = currentMenuParentPos;

            if(openCloseAnimTime >= openCloseAnimMaxTime)
            {
                StopOpenMenuAnim();
            }
        }
    }

    void StopOpenMenuAnim()
    {
        if (openMenuAnimStarted)
        {
            openMenuAnimStarted = false;
        }
    }

    //CLOSE MENU ANIM
    void StartCloseMenuAnim()
    {
        if (!closeMenuAnimStarted)
        {
            closeMenuAnimStarted = true;
            openMenuAnimStarted = false;
            openCloseAnimTime = 0;
            currentAnimVal = 0;
            animStartingX = furnitureMenuParent.transform.position.x;
        }
    }

    void ProcessCloseMenuAnim()
    {
        if (closeMenuAnimStarted)
        {
            if (currentAnimVal >= 1) return;
            openCloseAnimTime += Time.deltaTime;
            currentAnimVal = Mathf.Clamp01(openCloseAnimTime / openCloseAnimMaxTime);
            float x = EasingFunction.EaseInBack(animStartingX, hiddenMenuPos.position.x, currentAnimVal);
            Vector3 currentMenuParentPos = new Vector3(x, menuPos.position.y, 0);
            furnitureMenuParent.transform.position = currentMenuParentPos;

            if (openCloseAnimTime >= openCloseAnimMaxTime)
            {
                StopCloseMenuAnim();
            }
        }
    }

    void StopCloseMenuAnim()
    {
        if (closeMenuAnimStarted)
        {
            closeMenuAnimStarted = false;
        }
    }
    #endregion

    void InstantiateRenButtons(FurnitureTag tag)
    {
        int row = 0;
        Vector2 currentPos;
        Vector2 initialPos = currentPos = new Vector2(scrollRect.rect.xMin + (padding / 2) + (furnitureIconSize.x / 2), scrollRect.rect.yMax - (padding / 2) - (furnitureIconSize.y / 2));
        furnitureIcons.Add(new List<RenButton>());
        for (int i = 0; i < MasterManager.HousingSettings.allFurnitureList.Length; i++)
        {
            if (MasterManager.HousingSettings.allFurnitureList[i].HasTag(tag))
            {
                //instantiate
                GameObject auxButton = Instantiate(furnitureIconRenButtonPrefab, currentPos, Quaternion.identity, scrollRect.transform);
                furnitureIcons[row].Add(auxButton.GetComponent<RenButton>());

                currentPos.y = initialPos.y + (row * (padding + furnitureIconSize.y));
                currentPos.x += padding + furnitureIconSize.x;
                if (currentPos.x + (furnitureIconSize.x / 2) + (padding / 2) > scrollRect.rect.xMax)
                { //next row
                    currentPos.x = initialPos.x;
                    furnitureIcons.Add(new List<RenButton>());
                    row++;
                }
            }
        }
    }
}
