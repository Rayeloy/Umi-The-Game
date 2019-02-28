using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageScript : MonoBehaviour {

    public bool hookable = false;

    public bool wallJumpable = true;

    [Tooltip("Can the player bounce back when dashing agains this wall?")]
    public bool wallDashBounce = true;
}
