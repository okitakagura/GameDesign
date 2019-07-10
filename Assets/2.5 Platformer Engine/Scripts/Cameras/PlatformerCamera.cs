using UnityEngine;

namespace Platformer
{
    [RequireComponent(typeof(Camera))]
    public class PlatformerCamera : MonoBehaviour
    {
        /// <summary>
        /// Object for the camera to follow.
        /// </summary>
        [Tooltip("Object for the camera to follow.")]
        public GameObject Target;

        /// <summary>
        /// Offset from the target for the camera to look at.
        /// </summary>
        [Tooltip("Offset from the target for the camera to look at.")]
        public Vector3 TargetOffset = new Vector3(1, 0, 0);

        /// <summary>
        /// Speed for shifting between different screen offsets.
        /// </summary>
        [Tooltip("Speed for shifting between different screen offsets.")]
        public float ShiftSpeed = 0.5f;

        /// <summary>
        /// Distance from the target.
        /// </summary>
        [Tooltip("Distance from the target.")]
        public float Distance = 10;

        /// <summary>
        /// Distance multiplier.
        /// </summary>
        [Tooltip("Distance multiplier.")]
        public float Zoom = 1.0f;

        /// <summary>
        /// Speed of turning when the target changes its orientation.
        /// </summary>
        [Tooltip("Speed of turning when the target changes its orientation.")]
        public float TurnSpeed = 2.0f;

        /// <summary>
        /// Camera borders on screen.
        /// </summary>
        [Tooltip("Camera borders on screen.")]
        public CameraBorders Borders = new CameraBorders(0.3f, 0.3f, 0.4f, 0.4f);

        private Camera _camera;

        private float _zoomTarget = 1.0f;
        private float _zoomSpeed = 1.0f;
        private Quaternion _orientation = Quaternion.identity;
        private Vector2 _offset;
        private Vector3 _pivot;

        private Vector3 _snappedTargetPosition;
        private float _targetSide;
        private float _side;
        private bool _isUsingSide;
        private float _sideTimeout;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        /// <summary>
        /// Initial camera setup.
        /// </summary>
        private void OnEnable()
        {
            var angle = 90f;

            if (Target != null)
            {
                var motor = Target.GetComponent<CharacterMotor>();

                if (motor != null)
                    angle = motor.Angle;

                _pivot = Target.transform.position;
            }

            _orientation = Quaternion.AngleAxis(angle - 90, Vector3.up);

            var offset = transform.position - _pivot;
            offset = Quaternion.Inverse(_orientation) * offset;
            _offset = new Vector2(offset.x, offset.y);
        }

        /// <summary>
        /// Sets up the target zoom value.
        /// </summary>
        public void ZoomTo(float target, float speed)
        {
            _zoomTarget = target;
            _zoomSpeed = speed;
        }

        /// <summary>
        /// Does the camera follow logic.
        /// </summary>
        private void LateUpdate()
        {
            // Updated in many places just in case.
            HitPauseManager.Update();

            if (Target == null)
                return;

            var angle = 90f;
            var motor = Target.GetComponent<CharacterMotor>();

            if (motor != null)
                angle = motor.NextAngle;

            _orientation = Quaternion.Lerp(_orientation, Quaternion.AngleAxis(angle - 90, Vector3.up), Time.deltaTime * TurnSpeed);

            Zoom = Mathf.Lerp(Zoom, _zoomTarget, Time.deltaTime * _zoomSpeed);

            var viewportPosition = _camera.WorldToViewportPoint(Target.transform.position);

            if (viewportPosition.x <= Borders.Left)
            {
                _snappedTargetPosition = Target.transform.position;
                _isUsingSide = true;
                _side = Mathf.Clamp01(_side);
                _targetSide = 1;
                _sideTimeout = 0;
                
            }
            else if (viewportPosition.x > 1.0f - Borders.Right)
            {
                _snappedTargetPosition = Target.transform.position;
                _isUsingSide = true;
                _side = -Mathf.Clamp01(-_side);
                _targetSide = -1;
                _sideTimeout = 0;
            }

            if (viewportPosition.y < Borders.Bottom)
            {
                var point = _camera.ViewportToWorldPoint(new Vector3(viewportPosition.x, Borders.Bottom, viewportPosition.z));
                _offset.y -= Vector3.Dot(transform.up, point - Target.transform.position);
            }
            else if (viewportPosition.y > 1.0f - Borders.Top)
            {
                var point = _camera.ViewportToWorldPoint(new Vector3(viewportPosition.x, 1.0f - Borders.Top, viewportPosition.z));
                _offset.y -= Vector3.Dot(transform.up, point - Target.transform.position);
            }

            if (!_isUsingSide)
                _side = Mathf.Lerp(_side, 0, Time.deltaTime * 4);
            else
                _side = Mathf.Lerp(_side, _targetSide, Time.deltaTime * ShiftSpeed);

            if (_isUsingSide)
            {
                const float margin = 0.1f;
                float shift = Vector3.Dot(Target.transform.position - _snappedTargetPosition, transform.right);
                
                if (_targetSide < 0)
                {
                    if (shift > margin)
                    {
                        _snappedTargetPosition = Target.transform.position;
                        _sideTimeout = 0;
                    }
                    else if (shift < -margin)
                    {
                        _snappedTargetPosition = Target.transform.position;
                        _sideTimeout = 0;
                        _targetSide = 1;
                    }
                    else
                        _sideTimeout += Time.deltaTime;
                }
                else
                {
                    if (shift < -margin)
                    {
                        _snappedTargetPosition = Target.transform.position;
                        _sideTimeout = 0;
                    }
                    else if (shift > margin)
                    {
                        _snappedTargetPosition = Target.transform.position;
                        _sideTimeout = 0;
                        _targetSide = -1;
                    }
                    else
                        _sideTimeout += Time.deltaTime;
                }

                if (_sideTimeout > 0.5f)
                    _isUsingSide = false;
            }
            else
                _sideTimeout = 0;

            const float padding = 0.05f;

            if (_targetSide < 0)
            {
                var point = _camera.ViewportToWorldPoint(new Vector3(Borders.Left + padding, viewportPosition.y, viewportPosition.z));
                _offset.x -= Vector3.Dot(transform.right, point - Target.transform.position) * Mathf.Clamp01(-_side);
            }
            else
            {
                var point = _camera.ViewportToWorldPoint(new Vector3(1.0f - Borders.Right - padding, viewportPosition.y, viewportPosition.z));
                _offset.x -= Vector3.Dot(transform.right, point - Target.transform.position) * Mathf.Clamp01(_side);
            }

            var offset = new Vector3(_offset.x, _offset.y, -Zoom * Distance);
            offset = _orientation * offset;

            var targetOffset = new Vector3(_offset.x, _offset.y, 0);
            targetOffset = _orientation * targetOffset + TargetOffset;

            transform.position = _pivot + offset;
            transform.LookAt(_pivot + targetOffset);

            _pivot = Target.transform.position;
            offset = transform.position - _pivot;
            offset = Quaternion.Inverse(_orientation) * offset;
            _offset = new Vector2(offset.x, offset.y);
        }
    }
}
