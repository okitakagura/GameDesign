using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Manages weapon attacks.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        /// <summary>
        /// List of attacks for a combo.
        /// </summary>
        [Tooltip("List of attacks for a combo.")]
        public Attack[] Combo;

        /// <summary>
        /// The main attack performed in-air and while running. Also used when there is no combo.
        /// </summary>
        [Tooltip("The main attack performed in-air and while running. Also used when there is no combo.")]
        public Attack Attack = Attack.Default();

        private bool _wasAttackInitiated;
        private float _attackTimer = 0;
        private float _timeScale = 1;
        private CharacterMotor _damagePerformer;

        private GameObject[] _hitList = new GameObject[5];
        private int _hitCount;
        private Attack _attack;
        private Vector3 _forward;

        public void InputDuration(float duration)
        {
            _timeScale = 1.0f / duration;
        }

        /// <summary>
        /// Starts an attack.
        /// </summary>
        public void AttackBy(CharacterMotor performer, Attack attack, Vector3 forward)
        {
            _attack = attack;
            _attackTimer = _attack.Duration;
            _damagePerformer = performer;
            _hitCount = 0;
            _forward = forward;
            SendMessage("OnAttack", _attack, SendMessageOptions.DontRequireReceiver);

            if (attack.Dash > float.Epsilon && performer != null)
                performer.PerformDash(attack.Dash, attack.IsStrongDash);
        }

        /// <summary>
        /// Tells the weapon that it will be used to attack. Usually called during the start of an attack animation.
        /// </summary>
        public void InitiateAttack()
        {
            if (_wasAttackInitiated)
                return;

            _wasAttackInitiated = true;
            SendMessage("OnInitiateAttack", SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Tells the weapon that the attack is over. Usually called at the end of an attack animation.
        /// </summary>
        public void FinishAttack()
        {
            if (!_wasAttackInitiated)
                return;

            _attackTimer = 0;
            _wasAttackInitiated = false;
            SendMessage("OnFinishAttack", SendMessageOptions.DontRequireReceiver);
        }

        /// <summary>
        /// Checks for a hit.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            damageCheck(other.gameObject);
        }

        /// <summary>
        /// Checks for a hit. Ignores if a hit was already detected for the object.
        /// </summary>
        private void OnTriggerStay(Collider other)
        {
            damageCheck(other.gameObject);
        }

        /// <summary>
        /// Checks if it should to damage to the given target and deals if it required.
        /// </summary>
        private void damageCheck(GameObject target)
        {
            if (_attackTimer <= float.Epsilon || (_attackTimer >= _attack.Duration - _attack.HitDelay) || target == null)
                return;

            for (int i = 0; i < _hitCount; i++)
                if (target == _hitList[i])
                    return;

            var motor = target.GetComponent<CharacterMotor>();

            if (motor == null || motor == _damagePerformer)
                return;

            var vec = motor.transform.position - _damagePerformer.transform.position;
            vec.y = 0;

            if (Vector3.Dot(vec, _forward) < 0)
                return;

            if (_hitCount < _hitList.Length)
                _hitList[_hitCount++] = target;

            var normal = transform.position - motor.transform.position;
            if (normal.magnitude > float.Epsilon) normal.Normalize();

            var hit = new Hit(transform.position, normal, _attack.Damage, _damagePerformer.gameObject, _attack.HitPause, _attack.HitPauseDelay);

            motor.SendMessage("OnHit", hit, SendMessageOptions.DontRequireReceiver);
            gameObject.SendMessage("OnHitTarget", hit, SendMessageOptions.DontRequireReceiver);
            
            if (_damagePerformer != null)
                _damagePerformer.SendMessage("OnHitTarget", hit, SendMessageOptions.DontRequireReceiver);

            _attackTimer = 0;
        }

        /// <summary>
        /// Updates the attack timer.
        /// </summary>
        private void Update()
        {
            if (_attackTimer > 0)
                _attackTimer -= Time.deltaTime * _timeScale;
        }
    }
}
