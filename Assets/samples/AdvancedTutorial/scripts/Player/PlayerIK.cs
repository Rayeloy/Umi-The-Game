using UnityEngine;

namespace Bolt.AdvancedTutorial
{
    public class PlayerIK : Bolt.EntityBehaviour<IPlayerState>
    {
#pragma warning disable 414
        // weight blending
        float weight = 0f;
        float weightto = 0f;
        float weightfrom = 0f;
        float weightacc = 0f;
#pragma warning restore 414
        // if we are aiming or not
        bool _aiming;

        // the animator component
        Animator _animator;

        public override void Attached()
        {
            _animator = GetComponent<Animator>();
        }

        void OnAnimatorIK()
        {
            //TODO: FIX PITCH OFFSET CALCULATION

            //			float pitchOffsetArm = -state.pitch * 0.025f;

            _animator.SetLookAtWeight(1f);
        }
    }
}