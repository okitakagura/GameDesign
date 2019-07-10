using UnityEngine;

namespace Platformer
{
    public class MoveTowards : MonoBehaviour
    {
        /// <summary>
        /// Speed of movement in screen widths per second.
        /// </summary>
        [Tooltip("Speed of movement in screen widths per second.")]
        public float Speed = 1.0f;

        /// <summary>
        /// Target of movement.
        /// </summary>
        [Tooltip("Target of movement.")]
        public GameObject Target;

        /// <summary>
        /// Should the Target value be cleared once the object arrives at the destination.
        /// </summary>
        [Tooltip("Should the Target value be cleared once the object arrives at the destination.")]
        public bool StopOnArrival = true;

        private float _targetSpeed;
        private bool _isMoving;

        public void SetTargetSpeed(float value)
        {
            _targetSpeed = value;
        }

        private void Update()
        {
            if (Target == null || Camera.main == null || Speed <= float.Epsilon)
            {
                _isMoving = false;
                return;
            }

            Speed = Mathf.Lerp(Speed, _targetSpeed, Time.deltaTime);

            var vec = Target.transform.position - transform.position;
            var move = Speed * Time.deltaTime;

            if (vec.magnitude <= move)
            {
                transform.position = Target.transform.position;

                if (_isMoving)
                {
                    SendMessage("OnFinishMove", Target, SendMessageOptions.DontRequireReceiver);
                    _isMoving = false;

                    if (StopOnArrival)
                        Target = null;
                }
            }
            else
            {
                transform.position += vec.normalized * move;
                _isMoving = true;
            }
        }
    }
}
