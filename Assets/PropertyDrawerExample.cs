using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class PropertyDrawerExample : MonoBehaviour
{
    public Ingredient myIngredient;
    public FurnitureLevel myLevel1 = new FurnitureLevel(3);

    //public FurnitureLevel myLevel2 = new FurnitureLevel(3);

    //private void OnEnable()
    //{
    //    if(myLevel.row1.Length != 3)
    //    {
    //        myLevel = new FurnitureLevel(3);
    //    }
    //}
}
