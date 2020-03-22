using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingFurnitureMenu : MonoBehaviour
{
    public GameObject furnitureMenuParent;


    public void OpenFurnitureMenu()
    {
        furnitureMenuParent.SetActive(true);
    }

    public void CloseFurnitureMenu()
    {
        furnitureMenuParent.SetActive(false);
    }
}
