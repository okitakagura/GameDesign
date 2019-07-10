using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Updates animation and physics when time is not slowed down.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CharacterMotor))]
    public class CharacterTime : MonoBehaviour
    {
        /// <summary>
        /// Delta time to use on the character when slowed down.
        /// </summary>
        public float DeltaTime { get { return _dt; } }

        private float _lastTime;
        private float _dt;

        private Animator _animator;
        private Rigidbody _body;
        private CharacterMotor _motor;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _body = GetComponent<Rigidbody>();
            _motor = GetComponent<CharacterMotor>();
        }

        private void Update()
        {
            var time = Time.realtimeSinceStartup;
            var delta = time - _lastTime;
            _lastTime = time;

            if (Time.timeScale < 1 - float.Epsilon)
            {
                _dt = delta * (1 - Time.timeScale);

                _motor.PerformPixedUpdate(_dt);
                _animator.Update(_dt);
                transform.position += _body.velocity * _dt;
            }
            else
                _dt = Time.deltaTime;
        }
    }
}
