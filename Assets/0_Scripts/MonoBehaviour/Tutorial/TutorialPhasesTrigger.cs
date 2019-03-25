using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPhasesTrigger : MonoBehaviour
{
    public GameController_Tutorial gC;
    public TutorialPhase myTutorialPhase;

    private void OnTriggerEnter(Collider col)
    {
        gC.ProgressLane((int)myTutorialPhase);
    }
}
