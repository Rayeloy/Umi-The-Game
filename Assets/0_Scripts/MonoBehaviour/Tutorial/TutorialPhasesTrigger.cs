using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPhasesTrigger : MonoBehaviour
{
    public GameController_Tutorial gC;
    public TutorialPhase myTutorialPhase;
    public int laneNumber;

    private void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Player")
        {
            Debug.LogWarning("Name:"+name+"; TUTORIAL PHASE TRIGGER ACTIVATED: myTutorialPhase: " + myTutorialPhase + "; laneNumber: " + laneNumber+"; collision with : "+col.name);
            switch (myTutorialPhase)
            {
                case TutorialPhase.StartPhase:
                    print("GC = " + gC + "; GC.TutorialLanes["+ laneNumber + "] = " + gC.tutorialLanes[laneNumber]+ "; gC.tutorialLanes["+ laneNumber + "].phase = "+ gC.tutorialLanes[laneNumber].phase);
                    if (gC.tutorialLanes[laneNumber].phase == TutorialPhase.StartPhase)//Solo si la linea está en la startphase
                    {
                        gC.ProgressLane(laneNumber);
                        gameObject.SetActive(false);
                    }
                    break;
                case TutorialPhase.CannonPhase:
                    PlayerMovement player = col.GetComponent<PlayerBody>().myPlayerMov;
                    if (player != null)
                    {

                        gC.PlayerEnterRing(player);
                    }
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "Player")
        {
            switch (myTutorialPhase)
            {
                case TutorialPhase.CannonPhase:
                    if (!gC.startRingTeamBattle)
                    {
                        PlayerMovement player = col.GetComponent<PlayerMovement>();
                        if (player != null)
                        {
                            gC.playerExitRing(player);
                        }
                    }
                    break;
                case TutorialPhase.RingBattlePhase:
                    if (gC.startRingTeamBattle)
                    {
                        PlayerMovement player = col.GetComponent<PlayerMovement>();
                        if (player != null)
                        {
                            gC.playerExitRing(player);
                        }
                    }
                    break;
            }
        }
    }
}
