using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BadDog
{
    public class AutoRotation : MonoBehaviour
    {
        public float yAngleSpeed = 60.0f;

        void Update()
        {
            transform.Rotate(Vector3.up, yAngleSpeed * Time.deltaTime);
        }
    }
}
