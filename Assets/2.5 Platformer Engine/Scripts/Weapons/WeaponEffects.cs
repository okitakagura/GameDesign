using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Listens for events from the Weapon component and instantiates effects.
    /// </summary>
    public class WeaponEffects : MonoBehaviour
    {
        /// <summary>
        /// Effect prefab to instantiate when the weapon hits something.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the weapon hits something.")]
        public GameObject Hit;

        /// <summary>
        /// Effect prefab to instantiate when the weapon enters the attack mode.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the weapon enters the attack mode.")]
        public GameObject Attack;

        /// <summary>
        /// Trail to activate and deactivate during the weapon attack mode.
        /// </summary>
        [Tooltip("Trail to activate and deactivate during the weapon attack mode.")]
        public Trail Trail;

        /// <summary>
        /// Enables pauses after successful attacks.
        /// </summary>
        [Tooltip("Enables pauses after successful attacks.")]
        public bool PerformPauses;

        /// <summary>
        /// Instantiates the hit effect.
        /// </summary>
        public void OnHitTarget(Hit hit)
        {
            instantiate(Hit, hit.Position);

            if (PerformPauses)
                HitPauseManager.Pause(hit.HitPauseDuration, hit.HitPauseDelay);
        }

        /// <summary>
        /// Instantiates the attack effect.
        /// </summary>
        public void OnAttack(Attack attack)
        {
            instantiate(Attack, transform.position);

            if (Trail != null)
                Trail.SetMaterialOverride(attack.TrailMaterialOverride);
        }

        /// <summary>
        /// Displays the trail.
        /// </summary>
        public void OnInitiateAttack()
        {
            if (Trail != null)
                Trail.Show();
        }

        /// <summary>
        /// Hides the trail.
        /// </summary>
        public void OnFinishAttack()
        {
            if (Trail != null)
                Trail.Hide();
        }

        /// <summary>
        /// Helper function to instantiate prefabs.
        /// </summary>
        private void instantiate(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return;

            var obj = GameObject.Instantiate(prefab);
            obj.transform.parent = null;
            obj.transform.position = position;
            obj.SetActive(true);

            GameObject.Destroy(obj, 3);
        }
    }
}
