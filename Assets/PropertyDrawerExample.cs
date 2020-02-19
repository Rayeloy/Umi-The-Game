using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class PropertyDrawerExample : MonoBehaviour
{
    public Ingredient myIngredient;
    public int levelRows = 4;
    public int levelColumns = 4;
    public FurnitureLevel myLevel = new FurnitureLevel(4,4);

    //public FurnitureLevel myLevel2 = new FurnitureLevel(3);

    private void Update()
    {
        //if (myLevel.spaces != null)
        //    Debug.Log("myLevel.spaces = " + myLevel.spaces);

        if (myLevel.spaces.Length != levelRows || (myLevel.spaces.Length>0 && myLevel.spaces[0].row.Length != levelColumns))
        {
            //Debug.Log("Create new myLevel");
            myLevel = new FurnitureLevel(levelRows,levelColumns);
        }
    }
}
