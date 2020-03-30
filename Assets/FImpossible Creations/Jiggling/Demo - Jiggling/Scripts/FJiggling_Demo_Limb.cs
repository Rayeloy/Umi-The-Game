using UnityEngine;

namespace FIMSpace.Jiggling
{
    public class FJiggling_Demo_Limb : FJiggling_Simple
    {
        public bool NoRotationKeyframes = false;
        private Quaternion initialKeyRotation;

        protected override void Init()
        {
            if (initialized) return;

            base.Init();
            initialKeyRotation = TransformToAnimate.localRotation;
        }

        protected override void Update() { }

        protected virtual void LateUpdate()
        {
            if (NoRotationKeyframes) transform.localRotation = initialKeyRotation;

            // Every beginning of late update rotations are the same as in animation played by Animator component
            initRotation = transform.localRotation;

            base.Update();
            //if (animationFinished) enabled = true;
        }

        private void OnMouseDown()
        {
            StartJiggle();
        }
    }
}