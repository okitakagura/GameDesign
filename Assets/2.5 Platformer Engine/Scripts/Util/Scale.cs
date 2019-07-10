using UnityEngine;

namespace Platformer
{
    public class Scale : MonoBehaviour
    {
        /// <summary>
        /// Target scale value.
        /// </summary>
        [Tooltip("Target scale value.")]
        public Vector3 Target = Vector3.one;

        /// <summary>
        /// Speed of scaling.
        /// </summary>
        [Tooltip("Speed of scaling.")]
        public float Speed = 1.0f;

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Target, Speed * Time.deltaTime);
        }
    }
}
