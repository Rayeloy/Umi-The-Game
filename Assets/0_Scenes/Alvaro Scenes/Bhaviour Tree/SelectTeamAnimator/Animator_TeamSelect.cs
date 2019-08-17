using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animator_TeamSelect : MonoBehaviour
{
    [Header("Referencias")]
    public Animator animator;

    [Header("Animator Variables")]
    AnimatorStateInfo stateInfo;

    public bool anim_ready;

    int idleReadyHash = Animator.StringToHash("IdleReady");
    bool idleReady;

    public void KonoUpdate()
    {
        //if keypressed A variable
        if (!anim_ready)
        {
            idleReady = true;
            animator.SetBool(idleReadyHash, idleReady);
        }
       
        //if keypressed B variable
        if (anim_ready)
        {
            idleReady = false;
            animator.SetBool(idleReadyHash, idleReady);           
        }

    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
