using UnityEngine;

namespace Platformer
{
    public class MoveTowardsUI : MonoBehaviour
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

        /// <summary>
        /// Should the object move in Z coordinate too.
        /// </summary>
        [Tooltip("Should the object move in Z coordinate too.")]
        public bool MoveInZ = true;

        /// <summary>
        /// Z coordinate to move to.
        /// </summary>
        [Tooltip("Z coordinate to move to.")]
        public float Z = 0;

        private float _targetSpeed;

        private bool _isMoving;
        private Vector3 _previousScreenPosition;

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

            if (!_isMoving)
                _previousScreenPosition = Camera.main.WorldToScreenPoint(transform.position);

            Vector3 targetScreenPosition;
            var rect = Target.GetComponent<RectTransform>();

            if (rect == null)
                targetScreenPosition = Camera.main.WorldToScreenPoint(Target.transform.position);
            else
                targetScreenPosition = RectTransformUtility.WorldToScreenPoint(null, rect.position);

            var vec = targetScreenPosition - _previousScreenPosition;

            if (MoveInZ)
                vec.z = Z - _previousScreenPosition.z;
            else
                vec.z = 0;

            var move = Screen.width * Speed * Time.deltaTime;

            Vector3 finalScreenPosition;

            if (vec.magnitude <= move)
            {
                transform.position = Camera.main.ScreenToWorldPoint(targetScreenPosition);

                if (_isMoving)
                {
                    SendMessage("OnFinishMove", Target, SendMessageOptions.DontRequireReceiver);
                    _isMoving = false;

                    if (StopOnArrival)
                        Target = null;
                }

                return;
            }
            else
            {
                finalScreenPosition = _previousScreenPosition + vec.normalized * move;
                _isMoving = true;
            }

            _previousScreenPosition = finalScreenPosition;
            transform.position = Camera.main.ScreenToWorldPoint(finalScreenPosition);
        }
    }
}
