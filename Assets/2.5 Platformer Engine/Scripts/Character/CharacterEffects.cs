using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// Listens for events comign from the CharacterMotor and instantiates effects.
    /// </summary>
    public class CharacterEffects : MonoBehaviour
    {
        /// <summary>
        /// Effect prefab to instantiate when the character lands with big velocity.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the character lands with big velocity.")]
        public GameObject BigLand;

        /// <summary>
        /// Effect prefab to instantiate when the character lands with small velocity.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the character lands with small velocity.")]
        public GameObject SmallLand;

        /// <summary>
        /// Effect prefab to instantiate on each character step.
        /// </summary>
        [Tooltip("Effect prefab to instantiate on each character step.")]
        public GameObject Step;

        /// <summary>
        /// Effect prefab to instantiate and maintain while the character is sliding.
        /// </summary>
        [Tooltip("Effect prefab to instantiate and maintain while the character is sliding.")]
        public GameObject Slide;

        /// <summary>
        /// Effect prefab to instantiate when the character is hit.
        /// </summary>
        [Tooltip("Effect prefab to instantiate when the character is hit.")]
        public GameObject Hit;

        /// <summary>
        /// Velocity threshold in Y axis to switch from small to big landing effect.
        /// </summary>
        [Tooltip("Velocity threshold in Y axis to switch from small to big landing effect.")]
        public float BigLandThreshold = 10;

        private GameObject _slide;
        private int _slideCounter;

        /// <summary>
        /// Instantiates the hit effect when the character is hit.
        /// </summary>
        public void OnHit(Hit hit)
        {
            instantiate(Hit, hit.Position);
        }

        /// <summary>
        /// Instantiates the small or big landing effect depending on the velocity.
        /// </summary>
        public void OnLand(float velocity)
        {
            if (velocity >= BigLandThreshold)
                instantiate(BigLand, transform.position);
            else
                instantiate(SmallLand, transform.position);
        }

        /// <summary>
        /// Instantiates or maintains the previously instantiated slide effect.
        /// </summary>
        public void OnSlide(Vector3 position)
        {
            if (_slide == null && Slide != null)
            {
                _slide = GameObject.Instantiate(Slide);
                _slide.transform.parent = null;
                _slide.SetActive(true);
            }

            if (_slide != null)
                _slide.transform.position = position;

            _slideCounter = 2;
        }

        /// <summary>
        /// Instantiates the step effect.
        /// </summary>
        public void OnStep(Vector3 position)
        {
            instantiate(Step, position);
        }

        /// <summary>
        /// Checks if there were no OnSlide calls recently and destroys the slide effect if that is the case.
        /// </summary>
        private void Update()
        {
            if (_slideCounter <= 0 && _slide != null)
            {
                var audio = _slide.GetComponent<AudioSource>();

                if (audio != null)
                    audio.Stop();

                GameObject.Destroy(_slide, 4f);
                _slide = null;
            }

            _slideCounter--;
        }

        /// <summary>
        /// Helper function to instantiate effect prefabs.
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
