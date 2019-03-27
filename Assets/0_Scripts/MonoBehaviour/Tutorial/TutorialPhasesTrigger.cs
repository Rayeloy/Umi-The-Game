using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPhasesTrigger : MonoBehaviour
{
    public GameController_Tutorial gC;
    public TutorialPhase myTutorialPhase;

    private void OnTriggerEnter(Collider col)
    {
        switch (myTutorialPhase)
        {
            case TutorialPhase.StartPhase:
                if(gC.tutorialLanes[(int)myTutorialPhase].phase==TutorialPhase.StartPhase)
                gC.ProgressLane((int)myTutorialPhase);
                break;
            case TutorialPhase.CannonPhase:
                PlayerMovement player = col.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    gC.PlayerEnterRing(player);
                }
                break;
            case TutorialPhase.RingBattlePhase:
                if (gC.startRingTeamBattle)
                {

                }
                break;
            default:
                gC.ProgressLane((int)myTutorialPhase);
                break;

        }

    }

    private void OnTriggerExit(Collider col)
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
