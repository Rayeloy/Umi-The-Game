using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guitar : MonoBehaviour
{
    string[] sequences = { "ABCD", "CDCD", "DCFG" };

    string currentSequence = "";
    public int maxSequenceLength = 4;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StoreNote("C");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            StoreNote("D");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StoreNote("E");
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            StoreNote("F");
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            StoreNote("G");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            StoreNote("A");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            StoreNote("B");
        }
    }

    void StoreNote(string note)
    {
        if (currentSequence.Length >= maxSequenceLength)
        {
            currentSequence = currentSequence.Substring(1, currentSequence.Length - 1);
        }
        currentSequence += note;

        CheckForCorrectSequence();
    }

    void CheckForCorrectSequence()
    {
        for (int i = 0; i < sequences.Length && currentSequence.Length > 0; i++)
        {
            if (currentSequence.Contains(sequences[i]))
            {
                Debug.Log("Sequence correct! You performed " + sequences[i] + " correctly");
                //maybe erase the notes that have been already checked and have been a correct sequence
                //something like 
                currentSequence = "";
            }
        }
    }
}
