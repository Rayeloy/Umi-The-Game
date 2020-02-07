using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpBoxExample : MonoBehaviour
{
    [HelpBox("This is some help text for Data.", HelpBoxMessageType.Info)]
    public string data;
}
