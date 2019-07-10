using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Triggers a character to enter the rope mode. Should be used on every rope segment.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class Rope : MonoBehaviour
    {
        /// <summary>
        /// Returns true if the rope here and above is relatively calm.
        /// </summary>
        public bool IsCalm
        {
            get
            {
                if (transform.parent != null)
                {
                    var parent = transform.parent.GetComponent<Rope>();

                    if (parent != null && !parent.IsCalm)
                        return false;
                }

                return Vector3.Dot(Up, Vector3.up) > 0.95f;
            }
        }

        /// <summary>
        /// Vector pointing up in the world.
        /// </summary>
        public Vector3 Up
        {
            get
            {
                switch (_capsule.direction)
                {
                    case 0: return transform.right;
                    case 1: return transform.up;
                    case 2: return transform.forward;
                }

                return Vector3.up;
            }
        }

        /// <summary>
        /// Vector pointing forward in the world.
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                switch (_capsule.direction)
                {
                    case 0: return -transform.up;
                    case 1: return transform.right;
                    case 2: return transform.forward;
                }

                return Vector3.up;
            }
        }

        /// <summary>
        /// Top point on the trigger.
        /// </summary>
        public Vector3 Top
        {
            get { return transform.position + Up * _capsule.bounds.size.y * 0.5f; }
        }

        /// <summary>
        /// Bottom point on the trigger.
        /// </summary>
        public Vector3 Bottom
        {
            get { return transform.position - Up * _capsule.bounds.size.y * 0.5f; }
        }

        /// <summary>
        /// Velocity of the rope.
        /// </summary>
        public Vector3 Velocity
        {
            get { return transform.parent.GetComponent<Rigidbody>().velocity; }
        }

        /// <summary>
        /// Gets the root rope component.
        /// </summary>
        public Rope Root
        {
            get
            {
                var root = findRope(transform);

                while (true)
                {
                    var parent = findRope(root.transform.parent);

                    if (parent != null)
                        root = parent;
                    else
                        break;
                }

                return root;
            }
        }

        /// <summary>
        /// Difference inposition between the current segment and the root segment.
        /// </summary>
        public Vector3 OffsetToParent
        {
            get
            {
                var offset = Vector3.up;
                var rope = findRope(transform);
                var parent = findRope(transform.parent);

                while (parent != null && parent.transform.position.y >= rope.transform.position.y)
                {
                    offset += parent.transform.position - rope.transform.position;
                    rope = parent;
                    parent = findRope(parent.transform.parent);
                }

                return offset;
            }
        }

        private CapsuleCollider _capsule;

        /// <summary>
        /// Applies forces to make the rope calm.
        /// </summary>
        public void Calm()
        {
            calm(transform);
        }

        /// <summary>
        /// Applies a force the the rope.
        /// </summary>
        public void ApplyVelocity(Vector3 velocity)
        {
            var body = transform.parent.GetComponent<Rigidbody>();
            body.velocity += velocity;
        }

        /// <summary>
        /// Remember the capsule collider.
        /// </summary>
        private void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
        }

        /// <summary>
        /// Notifies the segment to a character motor.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null)
                return;

            motor.Grab(this);
        }

        /// <summary>
        /// Notifies a character motor its no longer touching the segment.
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            var motor = other.GetComponent<CharacterMotor>();
            if (motor == null)
                return;

            motor.Release(this);
        }

        /// <summary>
        /// Calms a segment and others preceeding it.
        /// </summary>
        private void calm(Transform t)
        {
            if (t.parent == null)
                return;

            var body = t.parent.GetComponent<Rigidbody>();

            if (body == null)
                return;

            var v = body.velocity;
            v.x = 0;
            v.z = 0;
            body.velocity = Vector3.Lerp(body.velocity, v, Time.deltaTime * 20);

            calm(t.parent);
        }

        private static Rope findRope(Transform transform)
        {
            if (transform == null || transform.parent == null)
                return null;

            var rope = transform.parent.GetComponent<Rope>();

            if (rope != null)
                return rope;

            for (int i = 0; i < transform.parent.childCount; i++)
            {
                rope = transform.parent.GetChild(i).GetComponent<Rope>();

                if (rope != null)
                    return rope;
            }

            return null;
        }
    }
}
