using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Maintains character health.
    /// </summary>
    public class CharacterHealth : MonoBehaviour
    {
        /// <summary>
        /// Current health of the character.
        /// </summary>
        [Tooltip("Current health of the character.")]
        [Range(0, 1000)]
        public float Health = 100f;

        /// <summary>
        /// Max health to regenerate to.
        /// </summary>
        [Tooltip("Max health to regenerate to.")]
        [Range(0, 1000)]
        public float MaxHealth = 100f;

        /// <summary>
        /// Amount of health regenerated per second.
        /// </summary>
        [Tooltip("Amount of health regenerated per second.")]
        public float Regeneration = 0f;

        private void LateUpdate()
        {
            Health = Mathf.Clamp(Health + Regeneration * Time.deltaTime, 0, MaxHealth);
        }

        /// <summary>
        /// Reduce health on bullet hit.
        /// </summary>
        public void OnHit(Hit hit)
        {
            Deal(hit.Damage);
        }

        /// <summary>
        /// Deals a specific amount of damage.
        /// </summary>
        public void Deal(float amount)
        {
            if (Health <= 0)
                return;

            Health -= amount;
            Health = Mathf.Clamp(Health, 0, MaxHealth);

            if (Health <= float.Epsilon)
            {
              
                SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            }
        
        }
    }
}