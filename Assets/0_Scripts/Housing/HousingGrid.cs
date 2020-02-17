using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingGrid : MonoBehaviour
{
    public float slotSize; 
    
    public int width;
    public int depth;
    public int height;

    HousingSlot[,,] slots;

    public HousingGrid(int _width, int _depth, int _height)
    {
        width = _width;
        depth = _depth;
        height = _height;

        slots = new HousingSlot[width, depth, height];

        //fill up the slots
        for (int i = 0; i < slots.GetLength(0); i++)
        {
            for (int j = 0; j < slots.GetLength(1); j++)
            {
                for (int k = 0; k < slots.GetLength(2); k++)
                {

                }
            }
        }
    }
}
