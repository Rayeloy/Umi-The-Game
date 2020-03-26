using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEndGameTrigger : MonoBehaviour
{
    public GameControllerCMF_Tutorial gC;

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player")
        {
            PlayerMovementCMF player = col.GetComponentInParent<PlayerMovementCMF>();
            if (player != null)
            {
                gC.StartGameOver();
            }
            else
            {
                Debug.LogError("TutorialEndGameTrigger: Error -> player's PlayerMovementCMF script could not be found.");
            }
        }
    }
}
