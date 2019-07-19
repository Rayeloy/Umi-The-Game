using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum PlayerInput
{
    Jump,//A
    Autocombo//X
}
[CreateAssetMenu(fileName = "New buffered input", menuName = "BufferedInput")]
public class BufferedInputData : ScriptableObject
{
    [Tooltip("IMPORTANT: ONLY CREATE ONE META PER TYPE OF INPUT")]
    public PlayerInput inputType;
    [Tooltip("Maximum time the input is buffered. If you want it to be more than 3 seconds learn to code and change the max. Also: wtf!?")]
    [Range(0, 3)]
    public float maxTimeBuffered = 0.2f;
}
public class BufferedInput
{
    public BufferedInputData input;
    public float time;//
    public bool buffering;//está a true si está actualmente "buffereado"

    public BufferedInput(BufferedInputData _input)
    {
        input = _input;
        time = 0;
        buffering = false;
    }
    //No se usa. devuelve true si se puede bufferear el input
    bool CanBeBuffered()
    {
        bool result = false;
        switch (input.inputType)
        {
            case PlayerInput.Jump:
                result = true;
                break;
        }
        return result;
    }

    public void StartBuffering()
    {
        time = 0;
        buffering = true;
    }

    public void StopBuffering()
    {
        time = 0;
        buffering = false;
    }

    public void ProcessTime()
    {
        if (buffering)
        {
            time += Time.deltaTime;
            if (time >= input.maxTimeBuffered)
            {
                buffering = false;
            }
        }
    }
}
