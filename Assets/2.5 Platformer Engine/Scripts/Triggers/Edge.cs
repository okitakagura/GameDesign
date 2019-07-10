using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Signifies an edge of an object for a character to latch on to.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class Edge : MonoBehaviour
    {
        /// <summary>
        /// Top plane of the edge.
        /// </summary>
        public float Top
        {
            get { return _box.bounds.max.y; }
        }

        /// <summary>
        /// Position in the middle on top of the edge.
        /// </summary>
        public Vector3 TopPoint
        {
            get
            {
                var point = transform.position;
                point.y = Top;

                return point;
            }
        }

        /// <summary>
        /// Position for a character to put their left hand on.
        /// </summary>
        public Vector3 LeftPoint
        {
            get
            {
                if (!_hasPoints)
                    setupPoints();

                return transform.TransformPoint(_leftPoint);
            }
        }

        /// <summary>
        /// Position for a character to put their right hand on.
        /// </summary>
        public Vector3 RightPoint
        {
            get
            {
                if (!_hasPoints)
                    setupPoints();

                return transform.TransformPoint(_rightPoint);
            }
        }

        private BoxCollider _box;

        private Vector3 _leftPoint;
        private Vector3 _rightPoint;
        private bool _hasPoints;

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null)
                return;

            motor.Grab(this);
        }

        private void OnTriggerStay(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null)
                return;

            motor.Grab(this);
        }

        private void OnTriggerExit(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null)
                return;

            motor.Release(this);
        }

        /// <summary>
        /// Calculates hand positions.
        /// </summary>
        private void setupPoints()
        {
            var hw = _box.size.x * 0.5f;
            var hh = _box.size.y * 0.5f;
            var hd = _box.size.z * 0.5f;

            _leftPoint = new Vector3(hw, hh, hd);
            _rightPoint = new Vector3(hw, hh, -hd);
            _hasPoints = true;
        }
    }
}
