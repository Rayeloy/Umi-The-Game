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
    [Header(" --- References ---")]
    public RenController myRenCont;
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
        furnitureIconSize = furnitureIconRenButtonPrefab.GetComponent<RectTransform>().rect.size;
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
        InstantiateFurnitureButtons(FurnitureTag.chair);
        GameInfo.instance.SetRenController(myRenCont);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        StartOpenMenuAnim();
    }

    public void CloseFurnitureMenu()
    {
        GameInfo.instance.QuitRenController();
        StartCloseMenuAnim();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #region --- Open / Close Animation ---
    //OPEN MENU ANIM
    void StartOpenMenuAnim()
    {
        Debug.Log("openMenuAnimStarted = " + openMenuAnimStarted);
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

            if (openCloseAnimTime >= openCloseAnimMaxTime)
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

    void InstantiateFurnitureButtons(FurnitureTag tag)
    {
        Vector2 currentPos;
        Vector2 initialPos = currentPos = new Vector2(scrollRect.rect.xMin + (padding / 2) + (furnitureIconSize.x / 2), scrollRect.rect.yMax - (padding / 2) - (furnitureIconSize.y / 2));
        Debug.Log("InstantiateRenButtons: scrollRect.rect.xMin = " + scrollRect.rect.xMin + "; scrollRect.rect.yMax = " + scrollRect.rect.yMax +
            "; scrollRect.rect.min = " + scrollRect.rect.min);
        Debug.Log("InstantiateRenButtons: furnitureIconSize = " + furnitureIconSize);

        furnitureIcons.Add(new List<RenButton>());
        int row = 0;
        for (int i = 0; i < MasterManager.HousingSettings.allFurnitureList.Length; i++)
        {
            if (MasterManager.HousingSettings.allFurnitureList[i].HasTag(tag))
            {
                Debug.Log("InstantiateRenButtons: currentPos = " + currentPos);
                //instantiate
                GameObject auxButton = myRenCont.InstantiateButton(furnitureIconRenButtonPrefab, Vector3.zero, Quaternion.identity, scrollRect.transform,1);
                RectTransform rectTrans = auxButton.GetComponent<RectTransform>();
                rectTrans.localPosition = currentPos;
                furnitureIcons[row].Add(auxButton.GetComponent<RenButton>());

                currentPos.y = initialPos.y - (row * (padding + furnitureIconSize.y));
                currentPos.x += padding + furnitureIconSize.x;
                if (currentPos.x + (furnitureIconSize.x / 2) + (padding / 2) > scrollRect.rect.xMax)
                { //next row
                    currentPos.x = initialPos.x;
                    furnitureIcons.Add(new List<RenButton>());
                    row++;
                }
            }
        }
        myRenCont.initialButton = furnitureIcons[0][0];
    }

    void ConnectFurnitureButtons()
    {
        for (int i = 0; i < furnitureIcons.Count; i++)
        {
            for (int j = 0; j < furnitureIcons[i].Count; j++)
            {
                if (j < furnitureIcons[i].Count - 1) furnitureIcons[i][j].GetComponent<RenButton>().nextRightButton = furnitureIcons[i][j + 1];
                if (j > 0) furnitureIcons[i][j].GetComponent<RenButton>().nextLeftButton = furnitureIcons[i][j - 1];
                if(i<furnitureIcons.Count-1) furnitureIcons[i][j].GetComponent<RenButton>().nextDownButton = furnitureIcons[i+1][j];


            }
        }
    }

    void ClearFurnitureButtons()
    {
        while (furnitureIcons.Count > 0)
        {
            while (furnitureIcons[0].Count > 0)
            {
                Destroy(furnitureIcons[0][0]);
                furnitureIcons[0].RemoveAt(0);
            }
            furnitureIcons.RemoveAt(0);
        }
    }
}
