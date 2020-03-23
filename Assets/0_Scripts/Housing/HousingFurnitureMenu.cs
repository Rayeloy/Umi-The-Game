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

    bool openMenuAnimStarted = false;
    bool closeMenuAnimStarted = false;
    public float openCloseAnimMaxTime = 0.5f;
    float openCloseAnimTime = 0;

    public void KonoAwake()
    {
        furnitureMenuParent.SetActive(false);
    }

    public void KonoUpdate()
    {
        ProcessOpenMenuAnim();
        ProcessCloseMenuAnim();
    }
   
    public void OpenFurnitureMenu()
    {
        furnitureMenuParent.SetActive(true);
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

    void StartOpenMenuAnim()
    {
        if (!openMenuAnimStarted)
        {
            openMenuAnimStarted = true;
            openCloseAnimTime = 0;
        }
    }

    void ProcessOpenMenuAnim()
    {
        if (openMenuAnimStarted)
        {
            openCloseAnimTime += Time.deltaTime;
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
            furnitureMenuParent.SetActive(false);
        }
    }

    void StartCloseMenuAnim()
    {
        if (!closeMenuAnimStarted)
        {
            closeMenuAnimStarted = true;
            openCloseAnimTime = 0;
        }
    }

    void ProcessCloseMenuAnim()
    {
        if (closeMenuAnimStarted)
        {
            openCloseAnimTime += Time.deltaTime;
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
