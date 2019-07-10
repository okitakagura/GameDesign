using UnityEngine;
using UnityEngine.UI;

namespace Platformer
{
    /// <summary>
    /// Manages the display of a character's health.
    /// </summary>
    [ExecuteInEditMode]
    public class HealthBar : MonoBehaviour
    {
        /// <summary>
        /// Object whose health is displayed on the health bar.
        /// </summary>
        [Tooltip("Object whose health is displayed on the health bar.")]
        public GameObject Target;

        /// <summary>
        /// Current value of the health bar.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Current value of the health bar.")]
        public float Value = 1.0f;

        /// <summary>
        /// Determines if the health bar is hidden when the target has no health.
        /// </summary>
        [Tooltip("Determines if the health bar is hidden when the target has no health.")]
        public bool HideWhenDead = true;

        /// <summary>
        /// Determines if the health bar is hidden when there is no target.
        /// </summary>
        [Tooltip("Determines if the health bar is hidden when there is no target.")]
        public bool HideWhenNone = false;

        /// <summary>
        /// Link to the object that draws the background of the health bar.
        /// </summary>
        [Tooltip("Link to the object that draws the background of the health bar.")]
        public RectTransform BackgroundRect;

        /// <summary>
        /// Link to the object that draws the health bar.
        /// </summary>
        [Tooltip("Link to the object that draws the health bar.")]
        public RectTransform FillRect;

        /// <summary>
        /// Link to the object that will be used to display a character's name.
        /// </summary>
        [Tooltip("Link to the object that will be used to display a character's name.")]
        public Text Name;

        private void LateUpdate()
        {
            if (Target != null)
            {
                var health = Target.GetComponent<CharacterHealth>();

                if (health != null)
                    Value = health.Health / health.MaxHealth;

                if (Name != null)
                {
                    var name = Target.GetComponent<CharacterName>();

                    if (name == null)
                        Name.text = Target.name;
                    else
                        Name.text = name.Name;
                }
            }

            if (FillRect != null)
                FillRect.anchorMax = new Vector2(Value, 1);

            if (Application.isPlaying)
            {
                var isVisible = (!HideWhenDead || Value > float.Epsilon) && (!HideWhenNone || Target != null);

                if (FillRect != null) FillRect.gameObject.SetActive(isVisible);
                if (BackgroundRect != null) BackgroundRect.gameObject.SetActive(isVisible);
                if (Name != null) Name.gameObject.SetActive(isVisible);
            }
        }
    }
}