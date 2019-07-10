using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Platformer
{
    public enum CharacterDirection
    {
        Right,
        Left,
        Inside,
        Outside,
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterMotor : MonoBehaviour
    {
        #region Types

        struct TurnDesc
        {
            public Turn Turn;
            public Vector3 Position;
            public float PreviousAngle;
        }

        struct RopeBit
        {
            //绳子
            public Rope Object;
            public Vector3 LastLocalPosition;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Possible action to perform.
        /// </summary>
        public OnAction Action
        {
            //get 用来访问私有成员变量
            get
            {
                foreach (var action in _actions)
                    if (action.IsValid(gameObject))
                        return action;

                return null;
            }
        }

        /// <summary>
        /// Is current currently dashing.
        /// </summary>
        public bool IsDashing
        {
            get { return _isDashing; }
        }

        /// <summary>
        /// Potential angle if the character continues to walk inside a turn trigger. Equals Angle in any other situation.
        /// </summary>
        public float NextAngle
        {
            get { return _nextAngle; }
        }

        /// <summary>
        /// Angle to orientate Vector3.forard around the Y axis to point it towards the direction of walking.
        /// </summary>
        public float WalkAngle
        {
            //get { return Direction == CharacterDirection.Right ? Angle : Angle - 180; }
            get
            {
                if (Direction == CharacterDirection.Right)
                {
                    return Angle;
                }
                if (Direction == CharacterDirection.Inside)
                {
                    return Angle + 90;
                }
                if (Direction == CharacterDirection.Outside)
                {
                    return Angle - 90;
                }
                else return Angle - 180;
            }
        }

        /// <summary>
        /// Is the character blocked by an obstacle.
        /// </summary>
        public bool IsBlocked
        {
            get { return _obstacle != null; }
        }

        /// <summary>
        /// Obstacle that blocks the character.
        /// </summary>
        public GameObject Obstacle
        {
            get { return _obstacle; }
        }

        /// <summary>
        /// Is the character touching ground.
        /// </summary>
        public bool IsGrounded
        {
            get { return _isGrounded; }
        }

        /// <summary>
        /// Is the character pushing an object. 
        /// </summary>
        public bool IsPushing
        {
            get { return _isPushing; }
        }

        /// <summary>
        /// Is the character facing the walk direction.
        /// </summary>
        public bool IsFacingWalkDirection
        {
            get { return directionStrength > 0.5f; }
        }

        /// <summary>
        /// Current direction of walking.
        /// </summary>
        public Vector3 Forward
        {
            get { return Quaternion.AngleAxis(WalkAngle, Vector3.up) * Vector3.forward; }
        }

        public Vector3 InOut
        {
            get { return Quaternion.AngleAxis(WalkAngle, Vector3.up) * Vector3.right; }
        }

        /// <summary>
        /// Direction for walking right relative to the camera.
        /// </summary>
        public Vector3 CameraRight
        {
            get { return Quaternion.AngleAxis(Angle, Vector3.up) * Vector3.forward; }
        }

        /// <summary>
        /// Direction for walking left relative to the camera.
        /// </summary>
        public Vector3 CameraLeft
        {
            get { return Quaternion.AngleAxis(Angle, Vector3.up) * -Vector3.forward; }
        }

        /// <summary>
        /// Is invincible from falling.
        /// </summary>
        public bool IsInvincible
        {
            get { return _invincibilityTimer > float.Epsilon; }
        }

        /// <summary>
        /// Is currently in fall mode.
        /// </summary>
        public bool IsFalling
        {
            get { return _isFalling; }
        }

        /// <summary>
        /// Returns true if the character has been recently hit.
        /// </summary>
        public bool IsGettingHit
        {
            get { return _isGettingHit; }
        }

        /// <summary>
        /// Is the character currently attacking (swinging a weapon).
        /// </summary>
        public bool IsAttacking
        {
            get { return _isDoingCombo; }
        }

        /// <summary>
        /// Is the character currently hanging on an edge.
        /// </summary>
        public bool IsHangingOnEdge
        {
            get
            {
                if (_edge == null)
                    return false;

                if (_edge.Top < transform.position.y + 0.5f && !_isEdgeClimbing)
                    return false;

                if (!_wasHangingOnEdge && _body.velocity.y > 0)
                    return false;

                _wasHangingOnEdge = Vector3.Dot(transform.forward, _edge.transform.right) > 0;
                return _wasHangingOnEdge;
            }
        }

        /// <summary>
        /// Is the character currently hanging on a rope.
        /// </summary>
        public bool IsHangingOnRope
        {
            get { return _handGrabbedRope.Object != null; }
        }

        /// <summary>
        /// Is the character currently sliding on a wall.
        /// </summary>
        public bool IsOnWall
        {
            get
            {
                if (_isGrounded || _wall == null || _edge != null || IsHangingOnRope)
                    return false;

                return Vector3.Dot(transform.forward, _wallNormal) < 0;
            }
        }

        /// <summary>
        /// Is the character currently on a sloped surface.
        /// </summary>
        public bool IsOnSlope
        {
            get { return _isOnSlope; }
        }

        /// <summary>
        /// Is the surface character is on not steep enough.
        /// </summary>
        public bool IsOnWalkableSurface
        {
            get { return _isOnWalkableSurface; }
        }

        /// <summary>
        /// Is the character currently climbing up or down a rope.
        /// </summary>
        public bool IsRopeClimbing
        {
            get { return IsHangingOnRope && Mathf.Abs(_ropeClimb) > 0.5f; }
        }

        #endregion

        #region Public fields

        /// <summary>
        /// Controls wheter the character is in a state of death. Dead characters have no collisions and ignore any input.
        /// </summary>
        [Tooltip("Controls wheter the character is in a state of death.")]
        public bool IsAlive = true;

        /// <summary>
        /// Can the character be made to enter a fallen state.
        /// </summary>
        [Tooltip("Can the character be made to enter a fallen state.")]
        public bool CanBeMadeToFall = true;

        /// <summary>
        /// Does the character stop their attack when hit by an enemy.
        /// </summary>
        [Tooltip("Does the character stop their attack when hit by an enemy.")]
        public bool HittingInterruptsAttacks = true;

        /// <summary>
        /// Weapon for the character to use in attacks.
        /// </summary>
        [Tooltip("Weapon for the character to use in attacks.")]
        public Weapon Weapon;

        /// <summary>
        /// Shield for the character to show or hide during climbing.
        /// </summary>
        [Tooltip("Shield for the character to show or hide during climbing.")]
        public GameObject Shield;

        /// <summary>
        /// Current orientation of the character. Angle is of the direction right relatively to the camera.
        /// </summary>
        [Tooltip("Current orientation of the character. Angle is of the direction right relatively to the camera.")]
        public float Angle = 90;

        /// <summary>
        /// Direction the character is facing relative to the camera.
        /// </summary>
        [Tooltip("Direction the character is facing relative to the camera.")]
        public CharacterDirection Direction=CharacterDirection.Inside;

        /// <summary>
        /// Trail object to show when speed up.
        /// </summary>
        [Tooltip("Trail object to show when speed up.")]
        public Trail SpeedTrail;

        /// <summary>
        /// Settings for walking.
        /// </summary>
        [Tooltip("Settings for walking.")]
        public WalkSettings Walk = WalkSettings.Default();

        /// <summary>
        /// Settings for dashing.
        /// </summary>
        [Tooltip("Settings for dashing.")]
        public DashSettings Dash = DashSettings.Default();

        /// <summary>
        /// Settings for slopes.
        /// </summary>
        [Tooltip("Settings for slopes.")]
        public SlopeSettings Slope = SlopeSettings.Default();

        /// <summary>
        /// Settings for jumping, gravity, and air movement.
        /// </summary>
        [Tooltip("Settings for jumping, gravity, and air movement.")]
        public AirSettings Air = AirSettings.Default();

        /// <summary>
        /// Settings for wall sliding and jumping.
        /// </summary>
        [Tooltip("Settings for wall sliding and jumping.")]
        public WallSettings Wall = WallSettings.Default();

        /// <summary>
        /// Settings for rope and edge climbing.
        /// </summary>
        [Tooltip("Settings for rope and edge climbing.")]
        public ClimbingSettings Climbing = ClimbingSettings.Default();

        /// <summary>
        /// IK settings for the character.
        /// </summary>
        [Tooltip("IK settings for the character.")]
        public IKSettings IK = IKSettings.Default();

        #endregion

        #region Fields

        private bool _hasRegistered;

        private Animator _animator;
        private Rigidbody _body;
        private CapsuleCollider _capsule;

        private float _speedUp = 1.0f;
        private float _speedTimer = 0;

        public static int monster_num = 0;
        private bool _isDashing = false;
        private bool _doesDashCausesToFall = false;
        private float _dashDelta = 0;
        private float _coveredDashDistance = 0;
        private float _dashDistance;
        private List<GameObject> _dashHits = new List<GameObject>();

        private float _nextAngle;
        private float _previousAngle;

        private float _movementInput = 0;
        private float _climbInput = 0;
        private bool _jumpInput = false;
        private bool _previousJumpInput = false;

        private float _noMovementTimer = 0;
        private float _groundTimer = 0;
        private float _jumpTimer = 0;
        private float _hangTimer = 0;
        private float _freeRopeTimer = 1;
        private Rope _freeTimedRope;
        private float _wallJumpTimer = 0;
        private float _postDashGravityTimer = 0;

        private float _directionTimer = 1;
        private CharacterDirection _previousDirection = CharacterDirection.Inside;

        private float _movement = 0;
        private float _lastMovementIntensity = 0;
        private float _ropeClimb = 0;
        private bool _wantsToStandUp = false;
        private bool _wasJustHit = false;
        private bool _isGettingHit = false;
        private bool _isEdgeJumping = false;
        private bool _isGrounded = true;
        private bool _isPushing;
        private bool _isEdgeClimbing;
        private GameObject _obstacle;
        private GameObject _wall;
        private Vector3 _wallNormal;

        private bool _canDoSecondJump = true;
        private bool _isSecondJumping = false;
        private float _secondJumpTimer = 0;

        private int _comboIndex;
        private bool _isDoingCombo;
        private bool _isDoingNonComboAttack;
        private bool _wantsToAttack;
        private bool _hasStartedCurrentAttack;
        private bool _hasReachedComboAnimation;
        private bool _wantsToBreakCombo;

        private bool _wantsToJump = false;
        private bool _isJumping = false;
        private float _jumpLegTimer = 0;
        private float _currentFoot = 0;

        private float _footDelay = 0;
        private float _slideDelay = 0;
        private float _landDelay = 0;

        private float _invincibilityTimer = 0;

        private bool _isFalling;
        private bool _isStandingUp;
        private bool _isTemporaryFall;
        private bool _hasBeganFalling;

        private bool _isOnSlope = false;
        private bool _isSliding = false;
        private bool _isWallSliding = false;
        private bool _isOnWalkableSurface = false;

        private RaycastHit[] _raycastHits = new RaycastHit[32];
        private RaycastHit[] _secondaryRaycastHits = new RaycastHit[32];
        private Collider[] _colliders = new Collider[32];

        private Edge _edge;
        private bool _wasHangingOnEdge;
        private Vector3 _lastEdgeRight;
        private Vector3 _lastLocalPointOnEdge;
        private Vector3 _lastEdgeRelevantPosition;

        private RopeBit _handGrabbedRope;
        private Rope _nearestHipRope;
        private Vector3 _relativeHeadPosition;
        private Vector3 _relativeLeftHandPosition;
        private Vector3 _relativeRightHandPosition;
        private Vector3 _relativeHipPosition;
        private float _ropeHipOffset;
        private float _relativeHeadHeight;
        private float _relativeHipHeight;
        private Rope _legGrabbedRope;
        private float _ropeAngle = 0;
        private List<RopeBit> _collidedRopeBits = new List<RopeBit>();

        private List<OnAction> _actions = new List<OnAction>();

        private List<TurnDesc> _turns = new List<TurnDesc>();
        private Vector3 _originalPosition;

        private IK _leftArmIK = new IK();
        private IK _rightArmIK = new IK();
        private IK _headIK = new IK();

        private float _armIKIntensity = 0;
        private float _headIKIntensity = 0;
        private float _hipIKIntensity = 0;

        #endregion

        #region Public methods

        /// <summary>
        /// Makes the character perform a dash with the given distance.
        /// </summary>
        public void PerformDash(float distance, bool causesToFall)
        {
            if (distance > float.Epsilon)
            {
                _isDashing = true;
                _doesDashCausesToFall = causesToFall;
                _dashDelta = 0;
                _coveredDashDistance = 0;
                _dashDistance = distance;
                _dashHits.Clear();
            }
        }

        /// <summary>
        /// Scales character speed up for the specified amount of time.
        /// </summary>
        public void SpeedUp(float scale, float duration)
        {
            _speedUp = scale;
            _speedTimer = duration;
        }

        /// <summary>
        /// Adds the action to the possible action list.
        /// </summary>
        public void EnterAction(OnAction action)
        {
            //if (!_actions.Contains(action))
            //_actions.Add(action);
        }

        /// <summary>
        /// Removes the action from the possible action list.
        /// </summary>
        public void LeaveAction(OnAction action)
        {
            //if (_actions.Contains(action))
            //_actions.Remove(action);
        }

        /// <summary>
        /// Makes the character enter edge hanging mode if possible.
        /// </summary>
        public void Grab(Edge edge)
        {
            //if (IsHangingOnRope || IsHangingOnEdge || _isEdgeClimbing)
            //    return;

            //if (edge != null && _jumpTimer < 0.1f)
            //{
            //    if (_edge != edge)
            //        updateEdgePoints(edge);

            //    _edge = edge;
            //}
        }

        /// <summary>
        /// Notifies the character that they are no longer colliding with the edge's trigger.
        /// </summary>
        public void Release(Edge edge)
        {
            //if (_edge == edge)
            //{
            //    _edge = null;
            //    _isEdgeJumping = false;
            //    _edge = null;
            //    _isEdgeClimbing = false;
            //    _wasHangingOnEdge = false;
            //}
        }

        /// <summary>
        /// Notifies the character they are touching a rope segment.
        /// </summary>
        public void Grab(Rope rope)
        {
            //foreach (var old in _collidedRopeBits)
            //    if (old.Object == rope)
            //        return;

            //RopeBit bit;
            //bit.Object = rope;
            //bit.LastLocalPosition = rope.transform.InverseTransformPoint(handPosition);
            //_collidedRopeBits.Add(bit);
        }

        /// <summary>
        /// Notifies the character they are no longer touching a rope segment.
        /// </summary>
        public void Release(Rope rope)
        {
            //for (int i = 0; i < _collidedRopeBits.Count; i++)
            //if (_collidedRopeBits[i].Object == rope)
            //{
            //    _collidedRopeBits.RemoveAt(i);
            //    i = 0;
            //}
        }

        /// <summary>
        /// Returns true if the given turn is the last taken.
        /// </summary>
        public bool IsLastTurn(Turn turn)
        {
            if (_turns.Count > 0 && _turns[_turns.Count - 1].Turn == turn)
                return true;

            return false;
        }

        /// <summary>
        /// Sets a potential angle so the camera will turn when the characters enters a turn trigger.
        /// </summary>
        public void InputNextAngle(float angle)
        {
            _nextAngle = angle;
        }

        /// <summary>
        /// Sets NextAngle to be equal to current Angle.
        /// </summary>
        public void CancelNextAngle()
        {
            _nextAngle = Angle;
        }

        /// <summary>
        /// Input a turn trigger.
        /// </summary>
        public void InputTurn(Turn turn, float angle)
        {
            //if (_turns.Count > 0)
            //{
            //    var last = _turns[_turns.Count - 1];

            //    if (turn == last.Turn && Mathf.Abs(Mathf.DeltaAngle(angle, last.PreviousAngle)) < 0.01)
            //    {
            //        Angle = last.PreviousAngle;
            //        _previousAngle = Angle;
            //        _nextAngle = Angle;
            //        _turns.RemoveAt(_turns.Count - 1);

            //        return;
            //    }
            //}

            //if (Mathf.Abs(Mathf.DeltaAngle(angle, Angle)) < 0.01)
            //    return;

            //var desc = new TurnDesc();
            //desc.Turn = turn;
            //desc.PreviousAngle = Angle;
            //desc.Position = turn.transform.position;
            //_turns.Add(desc);

            //Angle = angle;
            //_previousAngle = angle;
            //_nextAngle = angle;
        }

        /// <summary>
        /// Order the character to climb up or down during the frame.
        /// </summary>
        public void InputClimb(float direction)
        {
            _climbInput = direction;
        }

        /// <summary>
        /// Order the character to try jumping.
        /// </summary>
        public void InputJump()
        {
            _jumpInput = true;
        }

        /// <summary>
        /// Order the character to try attacking.
        /// </summary>
        public void InputAttack()
        {
            _wantsToAttack = true;
        }

        /// <summary>
        /// Order the character to move.
        /// </summary>
        public void InputMovement(float value)
        {
            _movementInput = value;
        }

        /// <summary>
        /// Makes the character enter fallen mode when not invincible.
        /// </summary>
        public void KnockOut(bool isTemporary = false)
        {
            if (IsInvincible)
                return;

            if (!IsAlive)
                isTemporary = false;

            if (!isTemporary)
                _isStandingUp = false;

            if (_isFalling)
            {
                _isTemporaryFall = isTemporary;
                return;
            }

            _isFalling = true;
            _isTemporaryFall = isTemporary;
            _hasBeganFalling = false;
        }

        /// <summary>
        /// Makes the character stand up from the fallen mode.
        /// </summary>
        public void StandUp()
        {
            if (IsAlive)
                _wantsToStandUp = true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Sets IsAlive to false upon the character death.
        /// </summary>
        public void Die()
        {
            IsAlive = false;
            if (this.GetComponent<NavMeshAgent>()) 
            {
                this.GetComponent<NavMeshAgent>().isStopped = true;
            }
            Debug.Log("stop");
            Debug.Log(this.name);
            if(this.name == "Hero")
            {
                monster_num = 0;
            }
            if (this.name.IndexOf("Mage", System.StringComparison.Ordinal) >= 0)
                monster_num++;
            //Debug.Log(monster_num);
            KnockOut();
        }

        /// <summary>
        /// Starts the hit animation.
        /// </summary>
        public void OnHit(Hit hit)
        {
            if (!_isGettingHit)
                _wasJustHit = true;
        }

        #endregion

        #region Behaviour

        private void OnEnable()
        {
            if (IsAlive)
            {
                Characters.Register(this);
                _hasRegistered = true;
            }
        }

        private void OnDisable()
        {
            Characters.Unregister(this);
            _hasRegistered = false;
        }

        // Execute even if the script is not activated
        private void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
            _animator = GetComponent<Animator>();
            _body = GetComponent<Rigidbody>();

            _originalPosition = transform.position;
            _nextAngle = Angle;
            _previousAngle = Angle;

            if (Direction == CharacterDirection.Right)
                transform.eulerAngles = new Vector3(0, Angle, 0);
            if (Direction == CharacterDirection.Inside)
                transform.eulerAngles = new Vector3(0, Angle + 90, 0);
            if (Direction == CharacterDirection.Outside)
                transform.eulerAngles = new Vector3(0, Angle - 90, 0);
            else
                transform.eulerAngles = new Vector3(0, Angle - 180, 0);
        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            PerformPixedUpdate(dt);
        }

        internal void PerformPixedUpdate(float dt)
        {
            applyMovement(dt);
            applyGravity(dt);
            applyTurn(dt);
            applyEdge(dt);
            //applyPath();
        }

        private void LateUpdate()
        {
            if (IsAlive && !_hasRegistered)
            {
                _hasRegistered = true;
                Characters.Register(this);
            }
            else if (!IsAlive && _hasRegistered)
            {
                _hasRegistered = false;
                Characters.Unregister(this);
            }

            // Updated in many places just in case.
            HitPauseManager.Update();

            var dt = Time.deltaTime;
            var time = GetComponent<CharacterTime>();

            if (time != null && time.isActiveAndEnabled)
                dt = time.DeltaTime;

            if (Angle != _previousAngle)
            {
                _previousAngle = Angle;
                _nextAngle = Angle;
            }

            if (!IsAlive && !_isFalling)
                KnockOut();

            if ((_isFalling && !_isTemporaryFall) || !IsAlive)
                gameObject.layer = 9;
            else
                gameObject.layer = 8;

            updateGround();
            updateWallAndObstacle();
            updatePush();
            updateFall();
            updateInvincibility(dt);
            updateGettingHit();
            updateWeapons();
            updateEdge();
            updateRope(dt);
            updateSlide();
            updateAnimator(dt);
            updateIK(dt);

            applyJump();

            if (_speedTimer >= 0)
                _speedTimer -= dt;

            if (_speedTimer <= float.Epsilon)
                _speedUp = 1.0f;

            if (SpeedTrail != null)
            {
                if (_speedUp > 1.01f || _isDashing)
                    SpeedTrail.Show();
                else
                    SpeedTrail.Hide();
            }

            if (_isSliding)
            {
                if (Direction == CharacterDirection.Right)
                    _movementInput = 1;
                if (Direction == CharacterDirection.Inside)
                    _movementInput = 2;
                if (Direction == CharacterDirection.Outside)
                    _movementInput = -2;
                else
                    _movementInput = -1;
            }

            if (_isDoingCombo && Mathf.Abs(_movementInput) > 0.5f)
                _wantsToBreakCombo = true;

            _movement = _movementInput;
            _wantsToJump = _jumpInput && !_previousJumpInput;
            _previousJumpInput = _jumpInput;

            if (IsHangingOnRope)
            {
                var obj = _legGrabbedRope;

                if (obj == null)
                    obj = _handGrabbedRope.Object;

                var v = obj.Velocity;
                v.y = 0;

                var offset = obj.OffsetToParent;
                offset.y = 0;

                if (v.magnitude > 1.2f || offset.magnitude > 1.0f)
                    _ropeClimb = Mathf.Lerp(_ropeClimb, 0, dt);
                else
                    _ropeClimb = Mathf.Lerp(_ropeClimb, _climbInput, dt * 5);

                if (Mathf.Abs(_climbInput) > 0.5f)
                    _handGrabbedRope.Object.Calm();
            }
            else
                _ropeClimb = 0;

            if (_secondJumpTimer >= 0)
            {
                _secondJumpTimer -= dt;

                if (_secondJumpTimer <= 0)
                    _isSecondJumping = false;
            }

            if (_postDashGravityTimer >= 0)
                _postDashGravityTimer -= dt;

            if (_wallJumpTimer >= 0)
                _wallJumpTimer -= dt;

            if (_directionTimer < 1)
                _directionTimer += dt;

            if (_previousDirection != Direction)
            {
                _previousDirection = Direction;
                _directionTimer = 0;
            }

            if (IsHangingOnRope)
                _freeRopeTimer = Climbing.RopeIgnoreTime;
            else if (_freeRopeTimer >= 0)
                _freeRopeTimer -= dt;

            if (!IsHangingOnEdge && !IsHangingOnRope)
                _hangTimer = 0;
            else if (_hangTimer < 1)
                _hangTimer += dt;

            if (Mathf.Abs(_movementInput) > float.Epsilon)
                _noMovementTimer = 0;
            else if (_noMovementTimer < 1)
                _noMovementTimer += dt;

            if (!_isGrounded)
                _groundTimer = 0;
            else if (_groundTimer < 1)
                _groundTimer += dt;

            if (_jumpTimer >= 0)
                _jumpTimer -= dt;

            if (_jumpTimer < 0.1f)
                _isJumping = false;

            if (_footDelay >= 0) _footDelay -= dt;
            if (_slideDelay >= 0) _slideDelay -= dt;
            if (_landDelay >= 0) _landDelay -= dt;

            _climbInput = 0;
            _movementInput = 0;
            _jumpInput = false;
        }

        #endregion

        #region Fixed update methods

        private void applyPath()
        {
        //    var a = _originalPosition;

        //    if (_turns.Count > 0)
        //        a = _turns[_turns.Count - 1].Position;

        //    if (Direction == CharacterDirection.Inside || Direction == CharacterDirection.Outside)
        //    {
        //       var b = a + Quaternion.AngleAxis(Angle, Vector3.up) * Vector3.right;
        //        var ground = Util.FindClosestToLine(a, b, transform.position);
        //        transform.position = new Vector3(ground.x, transform.position.y, ground.z);
        //    }
        //    if (Direction == CharacterDirection.Left || Direction == CharacterDirection.Right)
        //    {
        //        var b = a + Quaternion.AngleAxis(Angle, Vector3.up) * Vector3.forward;
        //        var ground = Util.FindClosestToLine(a, b, transform.position);
        //        transform.position = new Vector3(ground.x, transform.position.y, ground.z);
        //    }

        }

        private void applyJump()
        {
            if (_isGrounded || _body.velocity.y < 0)
                _isEdgeJumping = false;

            if (!_wantsToJump)
                return;

            if (_isFalling)
                return;

            if ((!_isGrounded && !_canDoSecondJump) && !IsHangingOnEdge && !IsHangingOnRope && !IsOnWall)
                return;

            if (_jumpTimer > float.Epsilon)
                return;

            _isDashing = false;
            _postDashGravityTimer = 0;

            if (_isDoingCombo && _wantsToJump)
                _wantsToBreakCombo = true;

            _jumpTimer = 0.25f;

            var v = _body.velocity;
            v.y = _isGrounded ? Air.GroundJumpStrength : Air.AirJumpStrength;

            if (IsOnWall)
            {
                v += _wallNormal * Wall.JumpStrength;
                _wallJumpTimer = Wall.JumpTime;

                if (Vector3.Dot(_wallNormal, transform.forward) < 0)
                {
                    if (Direction == CharacterDirection.Right)
                        Direction = CharacterDirection.Left;
                    if (Direction == CharacterDirection.Inside)
                        Direction = CharacterDirection.Outside;
                    if (Direction == CharacterDirection.Outside)
                        Direction = CharacterDirection.Inside;
                    if (Direction == CharacterDirection.Left)
                        Direction = CharacterDirection.Right;
                }
            }
            else if (IsHangingOnEdge)
            {
                if (!_isEdgeClimbing)
                    _isEdgeJumping = true;

                _isEdgeClimbing = false;
            }
            else if (IsHangingOnRope)
            {
                var r = _handGrabbedRope.Object.Velocity;

                if (r.magnitude > Climbing.RopeJumpStrength)
                    v += r.normalized * Climbing.RopeJumpStrength;
                else
                    v += r;

                _freeTimedRope = _handGrabbedRope.Object.Root;
                _handGrabbedRope = new RopeBit();
            }
            else if (!_isGrounded && _canDoSecondJump)
            {
                _isSecondJumping = true;
                _secondJumpTimer = 0.4f;
                _canDoSecondJump = false;
            }

            _body.velocity = v;

            if (_edge != null)
                Release(_edge);

            _wantsToJump = false;
            _isJumping = true;
        }

        private void applyMovement(float dt)
        {
            _lastMovementIntensity = 0;

            if (_isDashing)
            {
                var t = _dashDelta;
                t = t * t * (3.0f - 2.0f * t);

                var dist = t * _dashDistance;
                var offset = dist - _coveredDashDistance;

                if (dt > float.Epsilon)
                {
                    if (Direction == CharacterDirection.Left || Direction == CharacterDirection.Right)

                    {
                        _body.velocity = Forward * offset / dt;
                        //Debug.Log("left-right");
                    }
                    if (Direction == CharacterDirection.Inside || Direction == CharacterDirection.Outside)

                    {
                        _body.velocity = InOut * offset / dt;
                        //Debug.Log("In-Out");
                    }
                }

                _coveredDashDistance = dist;
                _dashDelta += dt * Dash.Speed / _dashDistance;

                if (_doesDashCausesToFall)
                {
                    var radius = transform.TransformVector(Vector3.forward * _capsule.radius).magnitude;
                    for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, radius + 0.5f, _colliders); i++)
                        if (_colliders[i].gameObject != gameObject && _colliders[i].gameObject.layer == gameObject.layer)
                            if (!_dashHits.Contains(_colliders[i].gameObject))
                            {
                                var hitBody = _colliders[i].GetComponent<Rigidbody>();

                                if (hitBody != null)
                                    hitBody.AddForce(Dash.Push.x * Forward +
                                                     Dash.Push.y * Vector3.up);

                                var hitMotor = _colliders[i].GetComponent<CharacterMotor>();

                                if (hitMotor != null)
                                    hitMotor.KnockOut(true);

                                _dashHits.Add(_colliders[i].gameObject);
                            }
                }

                _postDashGravityTimer = Dash.NoGravityTime;

                if (_dashDelta >= 1)
                    _isDashing = false;

                return;
            }

            if (IsHangingOnEdge)
            {
                if (_hangTimer > 0.5f && !_isEdgeJumping)
                {
                    var right = Quaternion.AngleAxis(Angle, Vector3.up) * -Vector3.right;
                    var dot = Vector3.Dot(right, _edge.transform.forward);
                    var dir = dot * _movement;

                    if (Mathf.Abs(_movement) > 0.5f)
                    {
                        if (dir > 0.5f)
                            _isEdgeClimbing = true;
                    }
                }

                return;
            }

            if (IsHangingOnRope)
            {
                _body.velocity = Vector3.zero;

                var obj = _legGrabbedRope;

                if (obj == null)
                    obj = _nearestHipRope;

                if (obj == null)
                    obj = _handGrabbedRope.Object;

                if (obj != null)
                    obj.ApplyVelocity(ropeForward * _movement * dt * Climbing.SwingSpeed);
            }
            else
            {
                var v = _body.velocity;
                if (Direction == CharacterDirection.Inside || Direction == CharacterDirection.Outside)
                {
                    v.z = v.x;
                    v.x = 0;
                    //Debug.Log("In-Out");
                }

                //Debug.Log("v"+v);
                var o = Quaternion.AngleAxis(Angle, Vector3.up);
                var rv = Quaternion.Inverse(o) * v;
                //Debug.Log("rv" + rv);
                var speed = (_isGrounded ? Walk.Speed : Air.Speed) * _speedUp;
                var acc = _isGrounded ? Walk.Acceleration : Air.Acceleration;
                var decc = _isGrounded ? Walk.Decceleration : Air.Decceleration;
                var canMove = !_isGrounded || _isOnWalkableSurface;

                if (_isSliding)
                {
                    if ((Direction == CharacterDirection.Right|| Direction == CharacterDirection.Inside) && _movement > 0.5f)
                        canMove = true;
                    else if ((Direction == CharacterDirection.Left || Direction == CharacterDirection.Outside)&& _movement < -0.5f)
                        canMove = true;
                }

                if (!_isGrounded && _wallJumpTimer > float.Epsilon)
                {
                }
                //else if (_isFalling)
                //    rv.z = Mathf.Lerp(rv.z, 0, dt * decc);
                else if (Mathf.Abs(_movement) > float.Epsilon && canMove)
                {
                    if (Direction == CharacterDirection.Left || Direction == CharacterDirection.Right)
                    {

                        v.x = 0;
                        rv.z = Mathf.Lerp(rv.z, _movement * speed * directionStrength, dt * acc);
                    }
                    else
                    {
                        v.z = 0;
                        rv.x = Mathf.Lerp(rv.x, _movement * speed * directionStrength, dt * acc);
                    }
                }

                //else if (_isOnSlope)
                //{
                //    if (!canMove || Mathf.Abs(_movement) < 0.2f)
                //    {
                //        if (rv.z > 0.3f)
                //            Direction = CharacterDirection.Right;
                //        else if (rv.z < -0.3f)
                //            Direction = CharacterDirection.Left;
                //    }

                //    if (canMove)
                //        rv.z = Mathf.Lerp(rv.z, _movement * speed * directionStrength, dt * acc);
                //    else
                //        rv.z = 0;
                //}
                else if (_noMovementTimer > 0.1f && _isGrounded)
                {
                    if (Direction == CharacterDirection.Left || Direction == CharacterDirection.Right)
                    {
                        rv.z = Mathf.Lerp(rv.z, 0, dt * 30);
                    }
                    if (Direction == CharacterDirection.Inside || Direction == CharacterDirection.Outside)
                    {
                        rv.x = Mathf.Lerp(rv.x, 0, dt * 30);
                    }

                }
                else
                {
                    if (Direction == CharacterDirection.Left || Direction == CharacterDirection.Right)
                    {
                        rv.z = Mathf.Lerp(rv.z, 0, dt * decc);
                    }
                    if (Direction == CharacterDirection.Inside || Direction == CharacterDirection.Outside)
                    {
                        rv.x = Mathf.Lerp(rv.x, 0, dt * decc);
                    }

                }
                if (Direction == CharacterDirection.Left || Direction == CharacterDirection.Right)
                {
                    _lastMovementIntensity = directionStrength * Mathf.Abs(rv.z) / speed;
                    rv.x = 0;
                }
                if (Direction == CharacterDirection.Inside || Direction == CharacterDirection.Outside)
                {
                    _lastMovementIntensity = directionStrength * Mathf.Abs(rv.x) / speed;
                    rv.z = 0;
                }
               
                //rv.z = 0;
                v = o * rv;
                //Debug.Log("speed"+v);
                _body.velocity = v;
            }
        }

        private void applyGravity(float dt)
        {
            if (!IsAlive)
            {
                _body.velocity -= new Vector3(0, Air.Gravity, 0) * dt;
                return;
            }

            var strength = Air.Gravity;

            if (IsOnWall && _body.velocity.y < 0)
                strength = Wall.Gravity;

            var force = new Vector3(0, strength, 0) * dt;

            if (IsHangingOnEdge)
            {
                if (_isEdgeClimbing)
                    _body.velocity = Vector3.zero;
                else
                {
                    _isEdgeJumping = false;

                    var velocity = _body.velocity - force;
                    var miny = _edge.Top + Climbing.EdgeHeight - height;

                    if (transform.position.y < miny + 0.1f)
                        velocity = Vector3.Lerp(velocity, Vector3.zero, Time.deltaTime * 30);

                    if (transform.position.y < miny)
                        transform.position = new Vector3(transform.position.x, miny, transform.position.z);

                    _body.velocity = velocity;
                }
            }
            else if (IsHangingOnRope)
            {
            }
            else if (!_isGrounded && !_isDashing && _postDashGravityTimer > 0)
            {
            }
            else if (_noMovementTimer < 0.1f || !_isGrounded || IsOnSlope || _groundTimer < 0.2f)
            {
                if (_isGrounded && _jumpTimer < 0.1f)
                {
                    if (_isDashing)
                        _body.velocity -= force * 10;
                    else
                        _body.velocity -= force * 2;
                }
                else
                    _body.velocity -= force;
            }
        }

        private void applyTurn(float dt)
        {
            if (_isFalling)
                return;

            var current = transform.eulerAngles;
            var delta = Mathf.DeltaAngle(current.y, WalkAngle);

            if (Mathf.Abs(delta) > 160)
                if ((Direction == CharacterDirection.Left && delta < 0)|| (Direction == CharacterDirection.Outside && delta < 0) ||
                    (Direction == CharacterDirection.Inside && delta < 0)|(Direction == CharacterDirection.Right && delta > 0))
                    delta = -delta;

            transform.eulerAngles = new Vector3(0,
                                                Mathf.Lerp(current.y, current.y + delta, dt * Walk.TurnSpeed * _speedUp),
                                                0);
        }

        private void applyEdge(float dt)
        {
            if (_isEdgeClimbing)
            {
                var current = _animator.GetCurrentAnimatorStateInfo(0);

                if (current.IsName("Climb"))
                {
                    if (_edge != null)
                        _lastEdgeRight = _edge.transform.right;

                    transform.position += (height * Vector3.up * 1.6f + _lastEdgeRight) * dt / current.length;
                }
            }
            else if (IsHangingOnEdge)
            {
                _isEdgeJumping = false;

                float clamp = transform.position.y;
                var topLerp = Vector3.Lerp(transform.position, _edge.TopPoint + Vector3.up * (Climbing.EdgeHeight - height), dt * 10);

                if (topLerp.y < clamp)
                    topLerp.y = clamp;

                transform.position = topLerp;
            }
        }

        #endregion

        #region Update methods

        private void updateEdge()
        {
            if (!IsHangingOnEdge)
                return;

            transform.position += _edge.transform.TransformPoint(_lastLocalPointOnEdge) - _lastEdgeRelevantPosition;
            updateEdgePoints(_edge);
        }

        private void updateRope(float dt)
        {
            var newHandGrabbedRope = idealHandGrabbedRope;
            var newNearestHipRope = idealNearestHipRope;
            var newLegGrabbedRope = idealLegGrabbedRope;

            var canChangeGrabs = newHandGrabbedRope != null && (IsRopeClimbing || _handGrabbedRope.Object == null);

            if (_relativeHeadHeight < 0 && newHandGrabbedRope != null)
                canChangeGrabs = true;

            if (canChangeGrabs)
                if (_freeRopeTimer <= float.Epsilon || newHandGrabbedRope.Root != _freeTimedRope || (_handGrabbedRope.Object != null && newHandGrabbedRope.Root == _handGrabbedRope.Object.Root))
                {
                    if (_handGrabbedRope.Object != null)
                    {
                        var point = _handGrabbedRope.Object.transform.TransformPoint(_handGrabbedRope.LastLocalPosition);

                        _handGrabbedRope.Object = newHandGrabbedRope;
                        _handGrabbedRope.LastLocalPosition = newHandGrabbedRope.transform.InverseTransformPoint(point);
                    }
                    else
                    {
                        _handGrabbedRope.Object = newHandGrabbedRope;
                        _handGrabbedRope.LastLocalPosition = newHandGrabbedRope.transform.InverseTransformPoint(handPosition);
                    }

                    {
                        var closest = Util.FindClosestToLine(newHandGrabbedRope.transform.position, newHandGrabbedRope.transform.position + newHandGrabbedRope.Up, headPosition);
                        _relativeHeadHeight = Vector3.Dot(newHandGrabbedRope.Up, closest - newHandGrabbedRope.transform.position);
                    }
                }

            if (IsHangingOnRope)
            {
                if (canChangeGrabs || _legGrabbedRope == null)
                    _legGrabbedRope = newLegGrabbedRope;

                if (canChangeGrabs)
                {
                    _nearestHipRope = newNearestHipRope;

                    if (_nearestHipRope != null)
                    {
                        var closest = Util.FindClosestToLine(_nearestHipRope.transform.position - _nearestHipRope.Up * 10, _nearestHipRope.transform.position + _nearestHipRope.Up * 10, hipPosition);
                        _relativeHipHeight = Vector3.Dot(_nearestHipRope.Up, closest - _nearestHipRope.transform.position);
                    }
                }

                _wallJumpTimer = 0;
                _ropeAngle = Mathf.Lerp(_ropeAngle, currentRopeAngle, dt * 10);

                var climbing = _animator.deltaPosition.y;

                if (Mathf.Abs(climbing) > float.Epsilon)
                {
                    var shift = ropeUp * climbing * Climbing.RopeClimbSpeed;
                    _handGrabbedRope.LastLocalPosition += _handGrabbedRope.Object.transform.InverseTransformVector(shift);
                }

                var rope = _handGrabbedRope.Object;
                var local = _handGrabbedRope.LastLocalPosition;
                var idealPosition = rope.transform.TransformPoint(local);

                var rd = Vector3.Dot(transform.position, transform.right);
                idealPosition += transform.right * (rd - Vector3.Dot(idealPosition, transform.right));

                var rf = Vector3.Dot(rope.transform.position, transform.forward);
                idealPosition += transform.forward * (rf - Vector3.Dot(idealPosition, transform.forward));

                var offset = IsRopeClimbing ? 0.15f : 0.3f;

                if (Direction == CharacterDirection.Left)
                    idealPosition += ropeForward * offset;
                else
                    idealPosition -= ropeForward * offset;

                idealPosition += transform.position - handPosition;

                transform.position = Vector3.Lerp(transform.position, idealPosition, dt * 30 * Mathf.Clamp01(_hangTimer * 2 + 0.1f));

                if (IsRopeClimbing)
                {
                    var hasGrabbed = false;

                    foreach (var bit in _collidedRopeBits)
                        if (bit.Object == _handGrabbedRope.Object)
                        {
                            hasGrabbed = true;
                            break;
                        }

                    if (!hasGrabbed)
                    {
                        _freeTimedRope = _handGrabbedRope.Object.Root;
                        _handGrabbedRope = new RopeBit();
                    }
                }
            }
            else
            {
                _legGrabbedRope = null;
                _ropeAngle = 0;
            }

            for (int i = 0; i < _collidedRopeBits.Count; i++)
                if (_handGrabbedRope.Object != _collidedRopeBits[i].Object)
                {
                    RopeBit bit;
                    bit.Object = _collidedRopeBits[i].Object;
                    bit.LastLocalPosition = bit.Object.transform.InverseTransformPoint(handPosition);

                    _collidedRopeBits[i] = bit;
                }
        }

        private void updateGettingHit()
        {
            if (!HittingInterruptsAttacks)
                if (_wantsToAttack || _isDoingCombo)
                {
                    _wasJustHit = false;
                    _isGettingHit = false;
                    return;
                }

            if (!_wasJustHit && !_isGettingHit)
                return;

            var current = _animator.GetCurrentAnimatorStateInfo(1);
            var next = _animator.GetNextAnimatorStateInfo(1);

            if (current.IsName("Get Hit") || next.IsName("Get Hit"))
            {
                _isGettingHit = true;
                _wasJustHit = false;

                if (current.IsName("Get Hit") && current.normalizedTime < 0.7f)
                    _wantsToAttack = false;
            }
            else if (!_wasJustHit)
                _isGettingHit = false;
            else
                _wantsToAttack = false;
        }

        private void updateWeapons()
        {
            if (_isFalling || !IsAlive)
            {
                _wantsToAttack = false;
                return;
            }

            var hasFreeHands = !IsHangingOnRope && !IsHangingOnEdge;

            if (!hasFreeHands)
                _wantsToAttack = false;

            if (Weapon != null)
                Weapon.gameObject.SetActive(hasFreeHands);

            if (Shield != null)
                Shield.gameObject.SetActive(hasFreeHands);

            if (Weapon == null)
                return;

            if (!_isDoingCombo)
                _comboIndex = 0;

            if (!_wantsToAttack && !_isDoingCombo && !_isDoingNonComboAttack)
            {
                _hasStartedCurrentAttack = false;
                Weapon.FinishAttack();

                return;
            }

            var couldDoCombo = Mathf.Abs(_movementInput) < 0.5f && _isGrounded && Weapon.Combo != null && Weapon.Combo.Length > 0;

            if (_isDoingCombo)
            {
                _isDoingNonComboAttack = false;

                var attack = Weapon.Combo[_comboIndex];

                var current = _animator.GetCurrentAnimatorStateInfo(0);
                var next = _animator.GetNextAnimatorStateInfo(0);
                var isCurrent = current.IsName(attack.AnimationState) || (attack.AlternateAnimationState != null && current.IsName(attack.AlternateAnimationState));
                var isNext = next.IsName(attack.AnimationState) || (attack.AlternateAnimationState != null && next.shortNameHash != 0 && next.IsName(attack.AlternateAnimationState));

                if (isCurrent)
                    Weapon.InputDuration(current.length);
                else if (isNext)
                    Weapon.InputDuration(next.length);

                if (isCurrent || isNext)
                {
                    _hasReachedComboAnimation = true;

                    if (!_wantsToBreakCombo)
                    {
                        if (!_hasStartedCurrentAttack && isCurrent && current.normalizedTime > attack.Start)
                        {
                            Weapon.AttackBy(this, attack, Forward);
                            _hasStartedCurrentAttack = true;
                        }

                        if (_wantsToAttack)
                        {
                            if (_hasStartedCurrentAttack && _comboIndex + 1 < Weapon.Combo.Length)
                            {
                                _hasStartedCurrentAttack = false;
                                _comboIndex++;
                                _animator.SetTrigger("ContinueCombo");
                            }

                            _wantsToAttack = false;
                        }
                    }
                }
                else if (_hasReachedComboAnimation)
                {
                    var isPrevious = false;

                    if (_comboIndex > 0)
                    {
                        var name = Weapon.Combo[_comboIndex - 1].AnimationState;
                        isPrevious = current.IsName(name) || next.IsName(name);

                        if (!isPrevious)
                        {
                            var alternate = Weapon.Combo[_comboIndex - 1].AlternateAnimationState;
                            isPrevious = current.IsName(alternate) ||
                                         (attack.AlternateAnimationState != null && next.shortNameHash != 0 && next.IsName(alternate));
                         }
                    }

                    if (!isPrevious)
                    {
                        _comboIndex = 0;
                        _isDoingCombo = false;
                        _wantsToAttack = false;
                        _hasStartedCurrentAttack = false;
                        _wantsToBreakCombo = false;
                        Weapon.FinishAttack();
                    }
                }
            }
            else if (_isDoingNonComboAttack)
            {
                var attack = Weapon.Attack;

                var current = _animator.GetCurrentAnimatorStateInfo(1);
                var next = _animator.GetNextAnimatorStateInfo(1);
                var isCurrent = current.IsName(attack.AnimationState);
                var isNext = next.IsName(attack.AnimationState);

                if (isCurrent)
                    Weapon.InputDuration(current.length);
                else if (isNext)
                    Weapon.InputDuration(next.length);

                if (isCurrent || isNext)
                {
                    if (!_hasStartedCurrentAttack && isCurrent && current.normalizedTime > attack.Start)
                    {
                        Weapon.AttackBy(this, attack, Forward);
                        _hasStartedCurrentAttack = true;
                    }

                    if (_wantsToAttack && !_hasStartedCurrentAttack)
                        _wantsToAttack = false;
                }
                else
                {
                    _isDoingNonComboAttack = false;
                    _hasStartedCurrentAttack = false;

                    if (couldDoCombo && _wantsToAttack)
                    {
                        _animator.SetTrigger("LateComboStart");
                        _isDoingCombo = true;
                        _hasReachedComboAnimation = false;
                        _comboIndex = 1;
                        _wantsToAttack = false;
                    }
                    else
                        Weapon.FinishAttack();
                }
            }
            else if (_wantsToAttack)
            {
                if (couldDoCombo)
                {
                    _animator.SetTrigger("StartCombo");
                    _isDoingCombo = true;
                    _hasReachedComboAnimation = false;
                    _comboIndex = 0;
                }
                else
                {
                    _animator.SetTrigger("Attack");
                    _isDoingNonComboAttack = true;
                }

                _wantsToAttack = false;
                _hasStartedCurrentAttack = false;
                _wantsToBreakCombo = false;

                Weapon.InitiateAttack();
            }
        }

        private void updateInvincibility(float dt)
        {
            if (_invincibilityTimer > 0)
                _invincibilityTimer -= dt;
        }

        //改变位置
        private void updateGround()
        {
            var wasGrounded = _isGrounded;

            _isOnSlope = false;
            _isOnWalkableSurface = false;
            _isSliding = false;
            _isGrounded = false;

            for (int i = 0; i < Physics.RaycastNonAlloc(transform.position + Vector3.up * 0.1f, Vector3.down, _raycastHits, Air.GroundThreshold + 0.1f); i++)
            {
                var hit = _raycastHits[i];

                if (!hit.collider.isTrigger && !Physics.GetIgnoreLayerCollision(gameObject.layer, hit.collider.gameObject.layer))
                    if (hit.collider.gameObject != gameObject)
                    {
                        var isOnPerson = hit.collider.gameObject.layer == 8;

                        if (isOnPerson)
                        {
                            if (Direction == CharacterDirection.Right)
                                InputMovement(1);
                            if (Direction == CharacterDirection.Inside)
                                InputMovement(2);
                            if (Direction == CharacterDirection.Outside)
                                InputMovement(-2);
                            if (Direction == CharacterDirection.Left)
                                InputMovement(-1);
                        }
                        else
                        {
                            var up = Vector3.Dot(Vector3.up, hit.normal);
                            var forward = Vector3.Dot(transform.forward, hit.normal);
                            var slope = Mathf.Acos(up) * Mathf.Rad2Deg;

                            if (up > 0.99f) slope = 0;

                            if (slope < Slope.MaxWalkAngle)
                                _isOnWalkableSurface = true;

                            if (slope > Slope.MinSlopeAngle && slope < Slope.MaxSlopeAngle)
                            {
                                if (forward > 0)
                                {
                                    for (int k = 0; k < Physics.RaycastNonAlloc(transform.position + Vector3.up * 0.1f + transform.forward * 0.1f, Vector3.down, _secondaryRaycastHits, Air.GroundThreshold + 0.1f); k++)
                                    {
                                        var secondary = _secondaryRaycastHits[k];

                                        if (!secondary.collider.isTrigger && !Physics.GetIgnoreLayerCollision(gameObject.layer, secondary.collider.gameObject.layer))
                                            if (secondary.collider.gameObject != gameObject)
                                                if (secondary.collider.gameObject.layer != 8)
                                                {
                                                    var secondaryUp = Vector3.Dot(Vector3.up, secondary.normal);
                                                    var secondaryForward = Vector3.Dot(transform.forward, secondary.normal);
                                                    var secondarySlope = Mathf.Acos(secondaryUp) * Mathf.Rad2Deg;

                                                    if (secondaryUp > 0.99f) secondarySlope = 0;

                                                    if (secondarySlope > Slope.MinSlopeAngle && secondarySlope < Slope.MaxSlopeAngle)
                                                    {
                                                        _isOnSlope = true;

                                                        if (forward > 0)
                                                            _isSliding = true;
                                                    }
                                                }
                                    }

                                }
                            }

                            if (!wasGrounded)
                                if (_landDelay <= float.Epsilon)
                                {
                                    SendMessage("OnLand", -_body.velocity.y, SendMessageOptions.DontRequireReceiver);
                                    _landDelay = 0.2f;
                                }

                            _isGrounded = true;
                        }
                    }
            }

            if (_isGrounded)
                _canDoSecondJump = true;
        }

        private void updateSlide()
        {
            if (_isSliding || _isWallSliding)
                SendMessage("OnSlide", transform.position + (_isWallSliding ? (Vector3.up * 0.5f) : Vector3.zero), SendMessageOptions.DontRequireReceiver);
        }

        private void updateWallAndObstacle()
        {
            _isWallSliding = false;

            if (_isGrounded)
            {
                Vector3 normal;

                var low = findObstacle(WalkAngle, 0.3f, Walk.ObstacleThreshold, out normal);

                if (low == null)
                    _obstacle = findObstacle(WalkAngle, 0.9f, Walk.ObstacleThreshold, out normal);
                else
                    _obstacle = low;

                _wall = null;
            }
            else
            {
                _wall = findObstacle(WalkAngle, 0.5f, Wall.Threshold, out _wallNormal);

                var isBack = false;

                if (_wall == null)
                    if (Mathf.Abs(_movementInput) < 0.5f)
                    {
                        _wall = findObstacle(WalkAngle - 180f, 0.5f, Wall.Threshold, out _wallNormal);
                        isBack = _wall != null;
                    }

                if (_wall != null && _wallJumpTimer > 0 && _wallJumpTimer < Wall.JumpTime - 0.1f)
                {
                    _wallJumpTimer = 0;

                    if (isBack && _edge == null)
                    {
                        if (Direction == CharacterDirection.Right)
                            Direction = CharacterDirection.Left;

                        if (Direction == CharacterDirection.Inside)
                            Direction = CharacterDirection.Outside;

                        if (Direction == CharacterDirection.Outside)
                            Direction = CharacterDirection.Inside;

                        if (Direction == CharacterDirection.Left)
                            Direction = CharacterDirection.Right;
                    }

                    _obstacle = null;

                    if (!IsHangingOnEdge)
                        if (_wall != null && _body.velocity.y < -1)
                            _isWallSliding = true;
                }
            }
        }

        private void updatePush()
        {
            _isPushing = false;

            var direction = Quaternion.AngleAxis(WalkAngle, Vector3.up) * Vector3.forward;

            for (int i = 0; i < Physics.RaycastNonAlloc(transform.position + Vector3.up * _capsule.center.y * height * 0.5f, direction, _raycastHits, Walk.PushThreshold + _capsule.radius); i++)
            {
                var hit = _raycastHits[i];

                if (!hit.collider.isTrigger && !Physics.GetIgnoreLayerCollision(gameObject.layer, hit.collider.gameObject.layer))
                    if (hit.collider.gameObject != gameObject)
                    {
                        var obj = hit.collider.gameObject;
                        if (obj.CompareTag("Pushable"))
                        {
                            _isPushing = true;
                        }
                    }
            }
        }

        private void updateFall()
        {
            if (!_isFalling)
            {
                _isStandingUp = false;
                _isTemporaryFall = false;
                _hasBeganFalling = false;
                return;
            }

            if (_isTemporaryFall)
                StandUp();

            var current = _animator.GetCurrentAnimatorStateInfo(0);
            var next = _animator.GetNextAnimatorStateInfo(0);

            if (current.IsName("Knock Out") || next.IsName("Knock Out"))
                _hasBeganFalling = true;
            else if (_hasBeganFalling)
            {
                if (!IsInvincible)
                    _invincibilityTimer = 1;

                if (_wantsToStandUp && IsGrounded)
                {
                    _isStandingUp = true;
                    _wantsToStandUp = false;

                    if (current.IsName("Standing") || next.IsName("Standing"))
                        _isFalling = false;
                }
                else if (current.IsName("Standing") || next.IsName("Standing"))
                    _hasBeganFalling = false;
            }
        }

        private void updateAnimator(float dt)
        {
            var state = _animator.GetCurrentAnimatorStateInfo(0);

            float runCycle = Mathf.Repeat(state.normalizedTime, 1);
            float foot = (runCycle < 0.2f || runCycle >= 0.7f ? 1 : -1) * _movementInput;

            if (Direction == CharacterDirection.Left)
                foot = -foot;

            if (_isGrounded)
            {
                if (_jumpLegTimer > 0)
                    _jumpLegTimer -= dt;
                else
                    _animator.SetFloat("JumpLeg", foot);

                if (foot != _currentFoot)
                {
                    var velocity = _body.velocity;
                    velocity.y = 0;

                    if (velocity.magnitude > 1 && !_isSliding && _isOnWalkableSurface)
                        if (_footDelay <= float.Epsilon)
                        {
                            if (foot < 0)
                                SendMessage("OnStep", _animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, SendMessageOptions.DontRequireReceiver);
                            else
                                SendMessage("OnStep", _animator.GetBoneTransform(HumanBodyBones.RightFoot).position, SendMessageOptions.DontRequireReceiver);

                            _footDelay = 0.2f;
                        }

                    _currentFoot = foot;
                }
            }
            else
                _jumpLegTimer = 0.5f;

            _animator.SetFloat("Walk", Mathf.Abs(_movement) * directionStrength, 0.05f, dt);
            _animator.SetBool("IsJumping", _isJumping);
            _animator.SetBool("IsGrounded", _isGrounded);
            _animator.SetBool("IsPushing", _isPushing);
            _animator.SetBool("IsHangingOnEdge", IsHangingOnEdge && Mathf.Abs(_edge.Top + Climbing.EdgeHeight - height - transform.position.y) < height * 0.5f);
            _animator.SetBool("IsHangingOnRope", IsHangingOnRope);
            _animator.SetBool("IsEdgeJumping", _isEdgeJumping);
            _animator.SetBool("KnockOut", _isFalling && !_hasBeganFalling);
            _animator.SetBool("KeepFalling", !IsAlive || (_isFalling && !_isStandingUp));
            _animator.SetBool("WasJustHit", _wasJustHit);
            _animator.SetBool("IsSliding", _isSliding);
            _animator.SetBool("IsEdgeClimbing", _isEdgeClimbing);
            _animator.SetBool("IsOnWall", IsOnWall);
            _animator.SetBool("IsRopeClimbing", IsHangingOnRope && Mathf.Abs(_ropeClimb) > 0.1f);
            _animator.SetFloat("Climb", _ropeClimb);
            _animator.SetFloat("Turn", Mathf.DeltaAngle(transform.eulerAngles.y, WalkAngle) / 180f);
            _animator.SetBool("ComboBreak", _wantsToBreakCombo);
            _animator.SetBool("SecondJump", _isSecondJumping);

            _animator.speed = _speedUp;

            if (IsHangingOnRope)
            {
                var swing = Mathf.Clamp(_ropeAngle / 30f, -1f, 1f);
                _animator.SetFloat("Swing", swing);
            }
        }

        private void updateIK(float dt)
        {
            var hips = _animator.GetBoneTransform(HumanBodyBones.Hips);

            float targetArmIKIntensity = 0;
            float targetHipIKIntensity = 0;
            float targetHeadIKIntensity = 0;
            float targetRopeHipOffset = _ropeHipOffset;
            Vector3 targetRelativeHipPosition = Vector3.zero;
            Vector3 targetRelativeHeadPosition = Vector3.zero;

            if (IsHangingOnRope)
            {
                targetHipIKIntensity = 1;

                var swing = Mathf.Abs(Mathf.Clamp(_ropeAngle / 30f, -1f, 1f));
                var climb = Mathf.Pow(Mathf.Abs(_ropeClimb), 4);

                targetArmIKIntensity = 1 - climb * (1 - swing);
                targetHeadIKIntensity = 1 - climb * (1 - swing);
            }
            else
                _relativeHeadPosition = Vector3.zero;

            if (IsHangingOnEdge)
            {
                if (!_isEdgeClimbing)
                    targetArmIKIntensity = 1.0f;

                if (IK.LeftHand != null)
                {
                    _leftArmIK.Target = IK.LeftHand;
                    _leftArmIK.Bones = IK.LeftArmChain.Bones;
                    _leftArmIK.UpdateMove(_edge.LeftPoint, 0, _armIKIntensity, IK.LeftArmChain.Iterations);
                }

                if (IK.RightHand != null)
                {
                    _rightArmIK.Target = IK.RightHand;
                    _rightArmIK.Bones = IK.RightArmChain.Bones;
                    _rightArmIK.UpdateMove(_edge.RightPoint, 0, _armIKIntensity, IK.RightArmChain.Iterations);
                }
            }

            var handPosition = Vector3.zero;

            if (IsHangingOnRope)
                handPosition = _handGrabbedRope.Object.transform.position + _handGrabbedRope.Object.Up * _relativeHeadHeight;

            if (IsHangingOnRope)
            {
                targetRopeHipOffset = IsRopeClimbing ? 0.05f : 0.25f;

                if (_nearestHipRope != null)
                {
                    Vector3 hipPosition;

                    var rope = _nearestHipRope;

                    if (rope != null)
                    {
                        hipPosition = rope.transform.position + rope.Up * _relativeHipHeight;

                        if (Direction == CharacterDirection.Right)
                            hipPosition -= rope.Forward * _ropeHipOffset;
                        else
                            hipPosition += rope.Forward * _ropeHipOffset;
                    }
                    else
                    {
                        hipPosition = this.handPosition - ropeUp * (handHeight - hipHeight);

                        if (Direction == CharacterDirection.Right)
                            hipPosition -= ropeForward * _ropeHipOffset * Mathf.Clamp01(currentRopeAngle / 30f);
                        else
                            hipPosition += ropeForward * _ropeHipOffset * Mathf.Clamp01(-currentRopeAngle / 30f);
                    }

                    targetRelativeHipPosition = hipPosition - hips.position;
                }
                else
                {
                    var hipPosition = handPosition + (this.hipPosition - this.handPosition) - _handGrabbedRope.Object.Up * 0.2f;

                    if (Direction == CharacterDirection.Right)
                        hipPosition = hipPosition - _handGrabbedRope.Object.Forward * _ropeHipOffset;
                    else
                        hipPosition = hipPosition + _handGrabbedRope.Object.Forward * _ropeHipOffset;

                    targetRelativeHipPosition = hipPosition - hips.position;
                }
            }

            if (_hipIKIntensity > 0.01f)
            {
                var newPosition = hips.position + _relativeHipPosition;
                var distance = Vector3.Distance(hips.position, newPosition);

                hips.position = Vector3.Lerp(hips.position, newPosition, Mathf.Clamp01(_hipIKIntensity * (distance * 4 + Time.deltaTime * 4)));
            }

            if (IsHangingOnRope)
            {
                if (_nearestHipRope != null)
                {
                    if (IK.Head != null)
                    {
                        var currentOffset = Vector3.Dot(IK.Head.position - handPosition, _handGrabbedRope.Object.Forward);
                        currentOffset = Mathf.Clamp(currentOffset, 0.2f, 0.3f);

                        var offset = Mathf.Clamp(0.4f,
                                                 currentOffset - 0.2f,
                                                 currentOffset + 0.2f);

                        Vector3 headPosition;

                        if (Direction == CharacterDirection.Right)
                            headPosition = handPosition - _handGrabbedRope.Object.Forward * offset;
                        else
                            headPosition = handPosition + _handGrabbedRope.Object.Forward * offset;

                        var goodDist = Vector3.Distance(IK.Head.position, hips.position);
                        headPosition = hips.position + Vector3.Normalize(headPosition - hips.position) * goodDist;
                        headPosition += (headHeight - handHeight) * _nearestHipRope.Up;

                        targetRelativeHeadPosition = headPosition - IK.Head.position;
                    }
                }
                else
                    targetRelativeHeadPosition = Vector3.zero;
            }

            if (_headIKIntensity > 0.01f)
                if (IK.Head != null)
                {
                    _headIK.Target = IK.Head;
                    _headIK.Bones = IK.UpperBodyChain.Bones;
                    _headIK.UpdateMove(_relativeHeadPosition + IK.Head.position, 0, _headIKIntensity, IK.UpperBodyChain.Iterations);
                }

            if (IsHangingOnRope)
            {
                Vector3 end;

                if (Direction == CharacterDirection.Right)
                    end = handPosition + _handGrabbedRope.Object.Forward * 0.15f;
                else
                    end = handPosition - _handGrabbedRope.Object.Forward * 0.15f;

                var up = _handGrabbedRope.Object.Up;
                var right = Vector3.Cross(_handGrabbedRope.Object.Up, _handGrabbedRope.Object.Forward);

                if (IK.LeftHand != null)
                    _relativeLeftHandPosition = end - up * 0.1f + right * 0.03f - IK.LeftHand.position;
                else
                    _relativeLeftHandPosition = Vector3.zero;

                if (IK.RightHand != null)
                    _relativeRightHandPosition = end + up * 0.1f - right * 0.03f - IK.RightHand.position;
                else
                    _relativeRightHandPosition = Vector3.zero;
            }

            var targetLeftHandPosition = IK.LeftHand != null ? (_relativeLeftHandPosition + IK.LeftHand.position) : Vector3.zero;
            var targetRightHandPosition = IK.RightHand != null ? (_relativeRightHandPosition + IK.RightHand.position) : Vector3.zero;

            if (_armIKIntensity > 0.01f)
            {
                if (IK.LeftHand != null)
                {
                    _leftArmIK.Target = IK.LeftHand;
                    _leftArmIK.Bones = IK.LeftArmChain.Bones;
                    _leftArmIK.UpdateMove(targetLeftHandPosition, 0, _armIKIntensity, IK.LeftArmChain.Iterations);
                }

                if (IK.RightHand != null)
                {
                    _rightArmIK.Target = IK.RightHand;
                    _rightArmIK.Bones = IK.RightArmChain.Bones;
                    _rightArmIK.UpdateMove(targetRightHandPosition, 0, _armIKIntensity, IK.RightArmChain.Iterations);
                }
            }

            if (IsHangingOnRope && !IsRopeClimbing)
            {
                var forward = _handGrabbedRope.Object.Forward;
                
                if (Direction == CharacterDirection.Right)
                    forward = -forward;

                var offset = 0f;

                const float handToHeadDistance = 0.6f;

                if (IK.LeftHand != null)
                {
                    offset = Vector3.Dot(forward, IK.LeftHand.position - targetLeftHandPosition) * _armIKIntensity;

                    if (IK.Head != null)
                    {
                        var dot = Vector3.Dot(forward, IK.Head.position - targetLeftHandPosition);

                        if (dot < handToHeadDistance)
                            targetRelativeHeadPosition += forward * (handToHeadDistance - dot);
                    }
                }
                else if (IK.RightHand != null)
                {
                    offset = Vector3.Dot(forward, IK.RightHand.position - targetRightHandPosition) * _armIKIntensity;

                    if (IK.Head != null)
                    {
                        var dot = Vector3.Dot(forward, IK.Head.position - targetRightHandPosition);

                        if (dot < handToHeadDistance)
                            targetRelativeHeadPosition += forward * (handToHeadDistance - dot);
                    }
                }

                targetRopeHipOffset -= offset;
            }

            _armIKIntensity = Mathf.Lerp(_armIKIntensity, targetArmIKIntensity, dt * 10);
            _headIKIntensity = Mathf.Lerp(_headIKIntensity, targetHeadIKIntensity, dt * 10);
            _hipIKIntensity = Mathf.Lerp(_hipIKIntensity, targetHipIKIntensity, dt * 10);
            _ropeHipOffset = targetRopeHipOffset;

            _relativeHipPosition = Vector3.Lerp(_relativeHipPosition, targetRelativeHipPosition, dt * 4);
            _relativeHeadPosition = Vector3.Lerp(_relativeHeadPosition, targetRelativeHeadPosition, dt * 4);
        }

        #endregion

        #region Private properties

        private Vector3 ropeUp
        {
            get
            {
                var back = Quaternion.AngleAxis(Angle, Vector3.up) * -Vector3.forward;
                return Quaternion.AngleAxis(currentRopeAngle, back) * Vector3.up;
            }
        }

        private Vector3 ropeForward
        {
            get
            {
                var forward = Quaternion.AngleAxis(Angle, Vector3.up) * Vector3.forward;
                return Quaternion.AngleAxis(currentRopeAngle, forward) * forward;
            }
        }

        private float headHeight { get { return height * 1.0f; } }
        private float handHeight { get { return height * 0.8f; } }
        private float hipHeight { get { return height * 0.4f; } }
        private float legHeight { get { return height * 0.2f; } }

        private float height
        {
            get { return _capsule.bounds.size.y; }
        }

        private Vector3 headPosition
        {
            get { return transform.position + Vector3.up * headHeight; }
        }

        private Vector3 handPosition
        {
            get { return transform.position + Vector3.up * handHeight; }
        }

        private Vector3 hipPosition
        {
            get { return transform.position + Vector3.up * hipHeight; }
        }

        private Vector3 legPosition
        {
            get { return transform.position + Vector3.up * legHeight; }
        }

        private Rope idealHandGrabbedRope
        {
            get
            {
                var head = handPosition;

                foreach (var bit in _collidedRopeBits)
                    if (Vector3.Dot(bit.Object.Up, head - bit.Object.Bottom) > 0 &&
                        Vector3.Dot(bit.Object.Up, bit.Object.Top - head) > 0)
                        return bit.Object;

                return null;
            }
        }

        private Rope idealNearestHipRope
        {
            get
            {
                var hips = hipPosition;

                foreach (var bit in _collidedRopeBits)
                    if (Vector3.Dot(bit.Object.Up, hips - bit.Object.Bottom) > 0 &&
                        Vector3.Dot(bit.Object.Up, bit.Object.Top - hips) > 0)
                        return bit.Object;

                return null;
            }
        }

        private Rope idealLegGrabbedRope
        {
            get
            {
                var legs = legPosition;

                foreach (var bit in _collidedRopeBits)
                    if (Vector3.Dot(bit.Object.Up, legs - bit.Object.Bottom) > 0 &&
                        Vector3.Dot(bit.Object.Up, bit.Object.Top - legs) > 0)
                        return bit.Object;

                return null;
            }
        }

        private float currentRopeAngle
        {
            get
            {
                if (!IsHangingOnRope)
                    return 0f;

                var offset = -_handGrabbedRope.Object.OffsetToParent;
                offset = -transform.InverseTransformVector(offset).normalized;

                return Mathf.Atan2(-offset.z, offset.y) * Mathf.Rad2Deg;
            }
        }

        private float directionStrength
        {
            get
            {
                var outOf180 = Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, WalkAngle)) / 180f;
                var outOfLast90 = Mathf.Clamp01(outOf180 - 0.5f) * 2;

                return Mathf.Clamp01(1.0f - outOfLast90);
            }
        }

        #endregion

        #region Private methods

        private void updateEdgePoints(Edge edge)
        {
            _lastEdgeRelevantPosition = transform.position;
            _lastLocalPointOnEdge = edge.transform.InverseTransformPoint(transform.position);
        }

        private GameObject findObstacle(float directionAngle, float height, float threshold, out Vector3 normal)
        {
            var direction = Quaternion.AngleAxis(directionAngle, Vector3.up) * Vector3.forward;

            for (int i = 0; i < Physics.RaycastNonAlloc(transform.position + Vector3.up * _capsule.center.y * height, direction, _raycastHits, threshold + _capsule.radius); i++)
            {
                var hit = _raycastHits[i];

                if (!hit.collider.isTrigger && !Physics.GetIgnoreLayerCollision(gameObject.layer, hit.collider.gameObject.layer) && hit.collider.gameObject.layer != 8)
                    if (hit.collider.gameObject != gameObject)
                    {
                        var up = Vector3.Dot(Vector3.up, hit.normal);
                        var slope = Mathf.Acos(up) * Mathf.Rad2Deg;

                        if (slope > Wall.MinAngle)
                        {
                            normal = -direction;
                            return hit.collider.gameObject;
                        }
                    }
            }

            normal = Vector3.zero;
            return null;
        }

        #endregion
    }
}